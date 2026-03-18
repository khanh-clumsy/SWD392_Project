using Microsoft.EntityFrameworkCore;
using SWD302_Project_HostelManagement.Models;

namespace SWD302_Project_HostelManagement.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Check if Tenants already exist
            if (await context.Tenants.AnyAsync()) return;

            // Seed Admins
            var admin = new Admin
            {
                Email = "admin@hostel.com",
                PasswordHash = "admin123456",  // Plain text password
                Name = "Super Admin",
                Status = "Active",
                CreatedDate = DateTime.UtcNow,
                AvatarUrl = null
            };
            await context.Admins.AddAsync(admin);
            await context.SaveChangesAsync();

            // Seed HostelOwners
            var owners = new List<HostelOwner>
            {
                new HostelOwner
                {
                    Email = "owner1@hostel.com",
                    PasswordHash = "owner1234567",  // Plain text password
                    Name = "Nguyễn Văn An",
                    PhoneNumber = "0901234567",
                    BusinessLicense = "BL-2024-001",
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow,
                    AvatarUrl = null
                },
                new HostelOwner
                {
                    Email = "owner2@hostel.com",
                    PasswordHash = "owner2234567",  // Plain text password
                    Name = "Trần Thị Bình",
                    PhoneNumber = "0912345678",
                    BusinessLicense = "BL-2024-002",
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow,
                    AvatarUrl = null
                }
            };
            await context.HostelOwners.AddRangeAsync(owners);
            await context.SaveChangesAsync();

            // Seed Tenants
            var tenants = new List<Tenant>
            {
                new Tenant
                {
                    Email = "tenant1@gmail.com",
                    PasswordHash = "tenant1234567",  // Plain text password
                    Name = "Lê Văn Cường",
                    PhoneNumber = "0923456789",
                    IdentityCard = "079200012345",
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow,
                    AvatarUrl = null
                },
                new Tenant
                {
                    Email = "tenant2@gmail.com",
                    PasswordHash = "tenant2234567",  // Plain text password
                    Name = "Phạm Thị Dung",
                    PhoneNumber = "0934567890",
                    IdentityCard = "079200023456",
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow,
                    AvatarUrl = null
                },
                new Tenant
                {
                    Email = "tenant3@gmail.com",
                    PasswordHash = "tenant3234567",  // Plain text password
                    Name = "Hoàng Văn Em",
                    PhoneNumber = "0945678901",
                    IdentityCard = "079200034567",
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow,
                    AvatarUrl = null
                }
            };
            await context.Tenants.AddRangeAsync(tenants);
            await context.SaveChangesAsync();

            // Seed Hostels
            var hostels = new List<Hostel>
            {
                new Hostel
                {
                    OwnerId = owners[0].OwnerId,
                    Name = "Cozy Hostel Downtown",
                    Address = "123 Nguyen Hue, District 1, HCMC",
                    Description = "Budget-friendly hostel in the heart of downtown",
                    Status = "Approved",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                },
                new Hostel
                {
                    OwnerId = owners[1].OwnerId,
                    Name = "Modern City Hostel",
                    Address = "456 Tran Hung Dao, District 5, HCMC",
                    Description = "Modern facilities with shared kitchens",
                    Status = "Approved",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                }
            };
            await context.Hostels.AddRangeAsync(hostels);
            await context.SaveChangesAsync();

            // Seed Rooms
            var rooms = new List<Room>
            {
                new Room
                {
                    HostelId = hostels[0].HostelId,
                    OwnerId = owners[0].OwnerId,
                    RoomNumber = "101",
                    RoomType = "Dorm (4-bed)",
                    Capacity = 4,
                    PricePerMonth = 2000000,
                    Area = 20,
                    Floor = 1,
                    Status = "Available",
                    Description = "Spacious 4-bed dorm with shared bathroom",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Room
                {
                    HostelId = hostels[0].HostelId,
                    OwnerId = owners[0].OwnerId,
                    RoomNumber = "102",
                    RoomType = "Private Single",
                    Capacity = 1,
                    PricePerMonth = 4000000,
                    Area = 12,
                    Floor = 1,
                    Status = "Available",
                    Description = "Comfortable private single room",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Room
                {
                    HostelId = hostels[1].HostelId,
                    OwnerId = owners[1].OwnerId,
                    RoomNumber = "201",
                    RoomType = "Dorm (6-bed)",
                    Capacity = 6,
                    PricePerMonth = 1800000,
                    Area = 25,
                    Floor = 2,
                    Status = "Available",
                    Description = "Large 6-bed dorm with modern facilities",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            await context.Rooms.AddRangeAsync(rooms);
            await context.SaveChangesAsync();
        }
    }
}
