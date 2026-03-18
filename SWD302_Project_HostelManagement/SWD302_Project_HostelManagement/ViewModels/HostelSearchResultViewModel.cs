namespace SWD302_Project_HostelManagement.ViewModels;

public class HostelSearchResultViewModel
{
    public int HostelId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int AvailableRoomCount { get; set; }
}
