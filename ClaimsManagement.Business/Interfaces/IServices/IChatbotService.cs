namespace ClaimsManagement.Business.Interfaces.IServices
{
    public interface IChatbotService
    {
        Task<string> ProcessMessageAsync(string message, int userRole, int userId);
    }
}