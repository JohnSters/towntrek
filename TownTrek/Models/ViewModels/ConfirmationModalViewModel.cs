namespace TownTrek.Models.ViewModels
{
    public class ConfirmationModalViewModel
    {
        public string Title { get; set; } = "Confirm Action";
        public string Message { get; set; } = "Are you sure you want to proceed?";
        public string? Details { get; set; }
        public string ConfirmText { get; set; } = "Confirm";
        public string CancelText { get; set; } = "Cancel";
        public string IconType { get; set; } = "info"; // success, warning, danger, info
        public string ConfirmButtonType { get; set; } = "primary"; // primary, success, warning, danger
        public string FormAction { get; set; } = "";
        public string FormMethod { get; set; } = "post";
        public Dictionary<string, string> HiddenFields { get; set; } = new Dictionary<string, string>();
    }
} 