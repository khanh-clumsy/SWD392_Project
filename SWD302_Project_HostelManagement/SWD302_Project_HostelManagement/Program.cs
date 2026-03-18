using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace SWD302_Project_HostelManagement
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register custom services
            builder.Services.AddScoped<SWD302_Project_HostelManagement.Proxies.EmailProxy>();
            builder.Services.AddScoped<SWD302_Project_HostelManagement.Proxies.PaymentProxy>();

            // Add DbContext - Choose database provider based on environment
            builder.Services.AddDbContext<SWD302_Project_HostelManagement.Data.AppDbContext>(options =>
            {
                // ===== POSTGRESQL (Production - Supabase) =====
                // Uncomment when ready to deploy to production
                //options.UseNpgsql(
                //    builder.Configuration.GetConnectionString("DefaultConnection"),
                //    npgsqlOptions =>
                //    {
                //        // Specify PostgreSQL version to avoid reading internal Supabase schemas
                //        npgsqlOptions.SetPostgresVersion(15, 0);
                //    }
                //);

                // ===== SQL SERVER (Development - Local) =====
                // Using local SQL Server for migrations and development
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnectionSqlServer")
                );
            });

            // ===== ADD COOKIE AUTHENTICATION =====
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                });

            builder.Services.AddAuthorization();
            builder.Services.AddScoped<SWD302_Project_HostelManagement.Services.CancelCoordinator>();

            builder.Services.AddScoped<SWD302_Project_HostelManagement.Services.BookingCoordinator>();


            var app = builder.Build();

            // ===== INITIALIZE VNPAY CONFIGURATION =====
            SWD302_Project_HostelManagement.VNPay.VNPayConfig.Initialize(builder.Configuration);

            // Seed data chỉ chạy trong môi trường Development
            if (app.Environment.IsDevelopment())
            {
                await SWD302_Project_HostelManagement.Data.DbSeeder.SeedAsync(app.Services);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // ===== ADD AUTHENTICATION & AUTHORIZATION MIDDLEWARE =====
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
