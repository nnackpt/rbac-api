namespace RBACapi.Models
{
    public class CM_USERS_AUTHORIZE
    {
        public string AUTH_CODE { get; set; } = string.Empty;

        public string? ROLE_CODE { get; set; }
        public string? APP_CODE { get; set; }
        public string? SITE_CODE { get; set; }
        public string? DOMAIN_CODE { get; set; }
        public string? FACT_CODE { get; set; }
        public string? USERID { get; set; }
        public string? FNAME { get; set; }
        public string? LNAME { get; set; }
        public string? ORG { get; set; }
        public bool? ACTIVE { get; set; }
        public string? CREATED_BY { get; set; }
        public DateTimeOffset? CREATED_DATETIME { get; set; }
        public string? UPDATED_BY { get; set; }
        public DateTimeOffset? UPDATED_DATETIME { get; set; }
    }
}