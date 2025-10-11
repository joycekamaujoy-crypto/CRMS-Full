namespace CRMS_API.Domain.DTOs
{
    public class AuthResponseDto
    {
        public int userId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }
}
