using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RBACapi.Models
{
    [Table("CM_RBAC")]
    public class CM_RBAC
    {
        [Key]
        public string RBAC_CODE { get; set; } = string.Empty;

        [ForeignKey("CM_APPS_ROLES")]
        public string? ROLE_CODE { get; set; }
        public CM_APPS_ROLES? CM_APPS_ROLES { get; set; }

        [ForeignKey("CM_APPS_FUNCTIONS")]
        public string? FUNC_CODE { get; set; }
        public CM_APPS_FUNCTIONS? CM_APPS_FUNCTIONS { get; set; }

        [ForeignKey("CM_APPLICATIONS")]
        public string? APP_CODE { get; set; }
        public CM_APPLICATIONS? CM_APPLICATIONS { get; set; }

        public bool? ACTIVE { get; set; }
        public string? CREATED_BY { get; set; }
        public DateTimeOffset? CREATED_DATETIME { get; set; }
        public string? UPDATED_BY { get; set; }
        public DateTimeOffset? UPDATED_DATETIME { get; set; }
    }
}