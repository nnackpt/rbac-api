namespace RBACapi.Models.Dtos
{
    public class AuthUsersResponseDto
    {
        // public string AuthCode { get; set; } = string.Empty;
        public string ApplicationTitle { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Org { get; set; } = string.Empty;
        public string SiteFacility { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTimeOffset? CreatedDateTime { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTimeOffset? UpdatedDateTime { get; set; }
    }
}