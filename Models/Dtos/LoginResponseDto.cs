namespace RBACapi.Models.Dtos
{
    public class UserAuthorizationDto
    {
        public string AUTH_CODE { get; set; } = string.Empty;
        public string? ROLE_CODE { get; set; }
        public string? APP_CODE { get; set; }
        public string? SITE_CODE { get; set; }
    }

    public class LoginResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public IEnumerable<UserAuthorizationDto> Authorizations { get; set; } = Enumerable.Empty<UserAuthorizationDto>();
    }
}