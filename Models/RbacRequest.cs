using System.ComponentModel.DataAnnotations;

namespace RBACapi.Models
{
    public class RbacRequest
    {
        [Required]
        public string ROLE_CODE { get; set; } = string.Empty;

        [Required]
        public string APP_CODE { get; set; } = string.Empty;

        [Required]
        public List<string> FUNC_CODES { get; set; } = new();

        public string? CREATED_BY { get; set; }
    }

    public class RbacUpdateRequest : RbacRequest
    {
        [Required]
        public string UPDATED_BY { get; set; } = string.Empty;
    }
}