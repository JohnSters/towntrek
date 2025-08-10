namespace TownTrek.Services
{
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public object? Data { get; set; }

        public static ServiceResult Success(object? data = null) => new() { IsSuccess = true, Data = data };
        public static ServiceResult Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }
}