namespace SWD302_Project_HostelManagement.ViewModels;

public class HostelSearchPageViewModel
{
    public HostelSearchFilterViewModel Filters { get; set; } = new();

    public List<HostelSearchResultViewModel> Results { get; set; } = new();

    public string? Message { get; set; }
}
