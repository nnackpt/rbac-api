using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RBACapi.Models
{
    [Table("CM_APPS_ROLES")]
    public class CM_APPS_ROLES
    {
        [Key]
        public string ROLE_CODE { get; set; } = string.Empty;

        [ForeignKey("CM_APPLICATIONS")]
        public string? APP_CODE { get; set; }
        public string? NAME { get; set; }
        public string? DESC { get; set; }
        public string? HOME_URL { get; set; }
        public bool? ACTIVE { get; set; }
        public string? CREATED_BY { get; set; }
        public DateTimeOffset? CREATED_DATETIME { get; set; }
        public string? UPDATED_BY { get; set; }
        public DateTimeOffset? UPDATED_DATETIME { get; set; }

        // Navigation property
        public CM_APPLICATIONS? CM_APPLICATIONS { get; set; }
    }
}