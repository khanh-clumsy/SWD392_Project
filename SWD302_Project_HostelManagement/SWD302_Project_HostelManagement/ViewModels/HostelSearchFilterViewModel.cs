namespace SWD302_Project_HostelManagement.ViewModels;

public class HostelSearchFilterViewModel
{
    public string? Keywords { get; set; }

    public string? Address { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public string? Status { get; set; }
}
