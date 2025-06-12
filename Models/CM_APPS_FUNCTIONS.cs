using System;

namespace RBACapi.Models
{
    public class CM_APPS_FUNCTIONS
    {
        public string FUNC_CODE { get; set; } = null!;
        public string APP_CODE { get; set; } = null!;
        public string? NAME { get; set; }
        public string? DESC { get; set; }
        public string? FUNC_URL { get; set; }
        public bool? ACTIVE { get; set; }
        public string? CREATED_BY { get; set; }
        public DateTime? CREATED_DATETIME { get; set; }
        public string? UPDATED_BY { get; set; }
        public DateTime? UPDATED_DATETIME { get; set; }

        // Navigation property
        public CM_APPLICATIONS? CM_APPLICATIONS { get; set; }
    }
}