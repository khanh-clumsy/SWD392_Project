using System.ComponentModel.DataAnnotations;

namespace SWD302_Project_HostelManagement.ViewModels
{
	
	// UC09 — Add New Hostel Property
	
	public class HostelCreateViewModel
	{
		[Required(ErrorMessage = "Tên hostel là bắt buộc.")]
		[StringLength(200, ErrorMessage = "Tên không vượt quá 200 ký tự.")]
		[Display(Name = "Tên Hostel")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
		[Display(Name = "Địa chỉ")]
		public string Address { get; set; } = string.Empty;

		[Display(Name = "Mô tả")]
		public string? Description { get; set; }
	}

	
	// UC12 — Change Room Status
	
	public class RoomChangeStatusViewModel
	{
		public int RoomId { get; set; }

		[Display(Name = "Số phòng")]
		public string RoomNumber { get; set; } = string.Empty;

		public string HostelName { get; set; } = string.Empty;
		public int HostelId { get; set; }

		[Display(Name = "Trạng thái hiện tại")]
		public string CurrentStatus { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng chọn trạng thái mới.")]
		[Display(Name = "Trạng thái mới")]
		public string NewStatus { get; set; } = string.Empty;

		// Danh sách status được phép chọn (inject từ Controller)
		public string[] AllowedStatuses { get; set; } = Array.Empty<string>();

		// Thông tin cảnh báo booking conflict
		public bool HasActiveBookings { get; set; }
		public int ActiveBookingCount { get; set; }
	}
}