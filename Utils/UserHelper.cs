using System.Security.Principal;

namespace RBACapi.Utils
{
    public static class UserHelper
    {
        public static string GetCurrentUsername(IIdentity? identity)
        {
            var fullName = identity?.Name;
            if (!string.IsNullOrEmpty(fullName))
            {
                var parts = fullName.Split('\\');
                return parts.Length > 1 ? parts[1] : parts[0];
            }

            return "anonymous";
        }
    }
}
