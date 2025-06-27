using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RBACapi.Models
{
    [Table("CM_APPLICATIONS")]
    public class CM_APPLICATIONS
    {
        [Key]
        public string APP_CODE { get; set; } = null!;

        public string? NAME { get; set; }
        public string? TITLE { get; set; }
        public string? DESC { get; set; }
        public bool? ACTIVE { get; set; }
        public string? BASE_URL { get; set; }
        public string? LOGIN_URL { get; set; }
        public string? CREATED_BY { get; set; }
        public DateTimeOffset? CREATED_DATETIME { get; set; }
        public string? UPDATED_BY { get; set; }
        public DateTimeOffset? UPDATED_DATETIME { get; set; }
    }
}