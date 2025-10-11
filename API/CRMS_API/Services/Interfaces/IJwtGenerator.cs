namespace CRMS_API.Services.Interfaces
{
    public interface IJwtGenerator
    {
        string GenerateJwtToken(int userId, string email, string role);
    }
}
