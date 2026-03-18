using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Data;
using SWD302_Project_HostelManagement.Models;
using SWD302_Project_HostelManagement.Proxies;
using SWD302_Project_HostelManagement.VNPay;

namespace SWD302_Project_HostelManagement.Controllers;

[Route("Payment")]
[Authorize(Roles = "Tenant")]
public class PaymentController : Controller
{
    private readonly AppDbContext _context;
    private readonly PaymentProxy _paymentProxy;
    private readonly EmailProxy _emailProxy;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        AppDbContext context,
        PaymentProxy paymentProxy,
        EmailProxy emailProxy,
        ILogger<PaymentController> logger)
    {
        _context = context;
        _paymentProxy = paymentProxy;
        _emailProxy = emailProxy;
        _logger = logger;
    }

    // ================================================================
    // COMET: initiateDepositPayment(in tenantId, in bookingId, in amount) : String
    // ================================================================
    // M3: Booking Eligibility Request
    // M4: Booking Payment Eligibility Information  
    // M5: Transaction Initiation Request
    // M6→M8: PaymentProxy → Payment Gateway
    // Return: Payment URL to redirect browser to VNPay
    // ================================================================
    [HttpPost("InitiatePayment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InitiateDepositPayment(int bookingId, decimal amount)
    {
        try
        {
            // Extract tenantId from Claims (ClaimTypes.NameIdentifier)
            var tenantIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(tenantIdStr))
            {
                TempData["Error"] = "You must be logged in to make a payment.";
                return RedirectToAction("Login", "Auth");
            }

            var tenantId = int.Parse(tenantIdStr);

            _logger.LogInformation(
                "M3-M4: Booking Eligibility Request - TenantId={TenantId}, BookingId={BookingId}, Amount={Amount}",
                tenantId, bookingId, amount);

            // M3: Find BookingRequest by bookingId
            var booking = await _context.BookingRequests
                .FirstOrDefaultAsync(b => b.BookingId == bookingId
                                       && b.TenantId == tenantId);

            if (booking == null)
            {
                _logger.LogWarning("Booking not found: BookingId={BookingId}, TenantId={TenantId}",
                    bookingId, tenantId);
                throw new InvalidOperationException("Booking not found.");
            }

            // M4: Check booking.getStatus() != "PendingPayment"
            if (booking.Status != "PendingPayment")
            {
                _logger.LogWarning(
                    "Booking not eligible for payment: BookingId={BookingId}, Status={Status}",
                    bookingId, booking.Status);
                throw new InvalidOperationException(
                    $"This booking is not eligible for payment. Current status: {booking.Status}");
            }

            _logger.LogInformation(
                "M5: Transaction Initiation Request - BookingId={BookingId}, Amount={Amount}",
                bookingId, amount);

            // M6→M8: PaymentProxy.initiateTransaction(bookingId, amount)
            // PaymentProxy forwards to external Payment Gateway (VNPay)
            string paymentUrl;
            try
            {
                paymentUrl = _paymentProxy.InitiateTransaction(bookingId, amount);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid payment parameters: bookingId={BookingId}, amount={Amount}",
                    bookingId, amount);
                throw new InvalidOperationException("Invalid payment parameters.", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Payment Gateway error for booking {BookingId}", bookingId);
                throw new InvalidOperationException("Cannot connect to Payment Gateway.", ex);
            }

            if (string.IsNullOrEmpty(paymentUrl))
            {
                _logger.LogError("Payment URL is null or empty for booking {BookingId}", bookingId);
                throw new InvalidOperationException("Cannot create payment URL from Payment Gateway.");
            }

            _logger.LogInformation(
                "M8: Payment URL generated - Redirecting to Payment Gateway for BookingId={BookingId}",
                bookingId);

            // Return Payment URL for TenantInteraction to redirect
            return Redirect(paymentUrl);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Business logic error in initiateDepositPayment");
            TempData["Error"] = ex.Message;
            return RedirectToAction("BookingRequestIndex", "Tenant");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in initiateDepositPayment for bookingId={BookingId}", 
                bookingId);
            TempData["Error"] = "An error occurred while initiating payment.";
            return RedirectToAction("BookingRequestIndex", "Tenant");
        }
    }

    // ================================================================
    // COMET: processPaymentResult(in bookingId, in isSuccess, in amountPaid, in gatewayRef)
    // ================================================================
    // Called by VNPay as callback (GET request with query parameters)
    // M10: Tenant Email Request
    // M11: Tenant Email (response)
    // M9 + M12-M17: Success flow (update status, create records, send email)
    // M7A.4 + M7A.5-M7A.8: Failure flow (create notification, send email)
    // ================================================================

    /// <summary>
    /// VNPay callback endpoint (matches ReturnUrl in appsettings.json)
    /// Routes to processPaymentResult for actual processing
    /// </summary>
    [HttpGet("VNPayReturn")]
    [AllowAnonymous]
    public async Task<IActionResult> VNPayReturn()
    {
        _logger.LogInformation("VNPayReturn callback received with query: {Query}", Request.QueryString);
        return await ProcessPaymentResult();
    }

    [HttpGet("ProcessPaymentResult")]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessPaymentResult()
    {
        try
        {
            var queryParams = Request.Query;

            // ================================================================
            // Validate VNPay signature to ensure callback is authentic
            // ================================================================
            var lib = new VnPayLibrary();
            foreach (var (key, value) in queryParams)
                lib.AddResponseData(key, value.ToString());

            var vnpSecureHash = queryParams["vnp_SecureHash"].ToString();
            var isValidSignature = lib.ValidateSignature(
                vnpSecureHash,
                VNPayConfig.HashSecret);

            if (!isValidSignature)
            {
                _logger.LogWarning("Invalid VNPay signature");
                TempData["Error"] = "Invalid payment response signature.";
                return RedirectToAction("Index", "Home");
            }

            // ================================================================
            // Parse VNPay response and extract parameters
            // ================================================================
            var transactionStatus = queryParams["vnp_TransactionStatus"].ToString();
            var txnRef = queryParams["vnp_TxnRef"].ToString();
            var amountStr = queryParams["vnp_Amount"].ToString();
            var gatewayRef = queryParams["vnp_TransactionNo"].ToString();

            // Extract bookingId from TxnRef format: BOOKING_[bookingId]_[timestamp]
            int bookingId = int.Parse(txnRef.Split('_')[1]);
            decimal amountPaid = decimal.Parse(amountStr) / 100;  // VNPay sends amount * 100
            bool isSuccess = transactionStatus == "00";  // "00" = VNPay success code

            _logger.LogInformation(
                "Payment result received: BookingId={BookingId}, IsSuccess={IsSuccess}, Amount={Amount}",
                bookingId, isSuccess, amountPaid);

            // ================================================================
            // COMET: processPaymentResult(in bookingId, in isSuccess, in amountPaid, in gatewayRef)
            // M10→M11: Tenant Email Request/Response
            // M9 + M12-M17: Success flow
            // M7A.4 + M7A.5-M7A.8: Failure flow
            // ================================================================

            // M10: Tenant Email Request - Find booking and tenant
            var booking = await _context.BookingRequests
                .Include(b => b.Tenant)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                _logger.LogError("Booking not found: {BookingId}", bookingId);
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("Index", "Home");
            }

            // M11: Tenant Email (response) - Get email from booking's tenant
            int tenantId = booking.TenantId;
            var tenant = booking.Tenant;

            if (tenant == null)
            {
                _logger.LogError("Tenant not found: {TenantId}", tenantId);
                TempData["Error"] = "Tenant not found.";
                return RedirectToAction("Index", "Home");
            }

            string email = tenant.Email;

            if (isSuccess)
            {
                // ========== SUCCESS FLOW (M9, M12-M17) ==========
                _logger.LogInformation("M9-M17: Processing successful payment for BookingId={BookingId}", bookingId);

                // M9: Update Status = "DepositPaid"
                booking.Status = "DepositPaid";
                booking.UpdatedDate = DateTime.UtcNow;

                // M12: Create Transaction Record
                var transaction = new PaymentTransaction
                {
                    BookingId = bookingId,
                    TenantId = tenantId,
                    Amount = amountPaid,
                    Status = "Success",
                    PaymentMethod = "VNPay",
                    GatewayRef = gatewayRef,
                    PaidAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PaymentTransactions.Add(transaction);

                // M13: Create Notification Record
                var notification = new Notification
                {
                    BookingId = bookingId,
                    RecipientEmail = email,
                    Subject = "Payment Successful - Booking Confirmed",
                    MessageContent = $"Your payment of {amountPaid:N0} VND for booking #{bookingId} " +
                                     "has been successfully processed. Your booking is now confirmed.",
                    Type = "PaymentSuccess",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);

                // Save changes to database
                await _context.SaveChangesAsync();

                // M14→M17: Send Email via EmailProxy
                bool emailSent = _emailProxy.SendEmail(email, notification);

                if (emailSent)
                {
                    notification.Status = "Sent";
                    notification.SentAt = DateTime.UtcNow;
                    _context.Notifications.Update(notification);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("M14-M17: Success email sent to {Email}", email);
                }
                else
                {
                    _logger.LogWarning("Success email failed to send to {Email}", email);
                }

                TempData["Success"] = "Payment successful! Your booking is now confirmed.";
            }
            else
            {
                // ========== FAILURE FLOW (M7A.4, M7A.5-M7A.8) ==========
                _logger.LogInformation("M7A.4-M7A.8: Processing failed payment for BookingId={BookingId}", bookingId);

                // M7A.4: Create Notification Record [Failure]
                var notification = new Notification
                {
                    BookingId = bookingId,
                    RecipientEmail = email,
                    Subject = "Payment Failed - Please try again",
                    MessageContent = $"Your payment for booking #{bookingId} was not successful. " +
                                     "Please try to pay again or contact our support team.",
                    Type = "PaymentFailed",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);

                // Save notification to database
                await _context.SaveChangesAsync();

                // M7A.5→M7A.8: Send Failure Email via EmailProxy
                if (!string.IsNullOrWhiteSpace(email))
                {
                    bool emailSent = _emailProxy.SendEmail(email, notification);

                    if (emailSent)
                    {
                        notification.Status = "Sent";
                        notification.SentAt = DateTime.UtcNow;
                        _context.Notifications.Update(notification);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("M7A.5-M7A.8: Failure email sent to {Email}", email);
                    }
                    else
                    {
                        _logger.LogWarning("Failure email failed to send to {Email}", email);
                    }
                }

                TempData["Error"] = "Payment failed. Please try again or contact our support.";
            }

            return RedirectToAction("BookingRequestIndex", "Tenant");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in processPaymentResult");
            TempData["Error"] = "An error occurred while processing the payment result.";
            return RedirectToAction("Index", "Home");
        }
    }
}
