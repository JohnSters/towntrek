namespace TownTrek.Models.ViewModels;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int StatusCode { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public bool ShowDetails { get; set; }
}
