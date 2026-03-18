using System.ComponentModel.DataAnnotations;

namespace SWD302_Project_HostelManagement.ViewModels
{
    public class RoomCreateViewModel
    {
        public int HostelId { get; set; }

        [Required]
        public string RoomNumber { get; set; }

        [Required]
        public decimal PricePerMonth { get; set; }
    }
}