using System.ComponentModel.DataAnnotations;

namespace RBACapi.Models.Dtos
{
    public class FacilitySelectionDto
    {
        public string SITE_CODE { get; set; } = string.Empty;
        public string DOMAIN_CODE { get; set; } = string.Empty;
        public string FACT_CODE { get; set; } = string.Empty;
    }

    public class UsersAuthorizeCreateRequestDto
    {
        [Required(ErrorMessage = "Application Code is required.")]
        public string APP_CODE { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role Code is required.")]
        public string ROLE_CODE { get; set; } = string.Empty;
        [Required(ErrorMessage = "User ID is required.")]
        public string USERID { get; set; } = string.Empty;
        [Required(ErrorMessage = "At least one facility is required.")]
        [MinLength(1, ErrorMessage = "At least one facility must be selected.")]
        public List<FacilitySelectionDto> Facilities { get; set; } = new List<FacilitySelectionDto>();

        public string? FNAME { get; set; }
        public string? LNAME { get; set; }
        public string? ORG { get; set; }
        public bool? ACTIVE { get; set; } = true;
    }

    public class UsersAuthorizeCreateResponseDto
    {
        public string AUTH_CODE { get; set; } = string.Empty;
        public string? APP_CODE { get; set; }
        public string? ROLE_CODE { get; set; }
        public string? USERID { get; set; }
        public string? SITE_CODE { get; set; }
        public string? DOMAIN_CODE { get; set; }
        public string? FACT_CODE { get; set; }
        public bool? ACTIVE { get; set; }
    }

    public class UsersAuthorizeUpdateRequestDto
    {
        // APP_CODE และ USERID แก้ไขไม่ได้
        public string APP_CODE { get; set; } = string.Empty;
        public string USERID { get; set; } = string.Empty;
        public string? ROLE_CODE { get; set; }
        public List<FacilitySelectionDto>? Facilities { get; set; }
        public string? FNAME { get; set; }
        public string? LNAME { get; set; }
        public string? ORG { get; set; }
        public bool? ACTIVE { get; set; }
    }
}