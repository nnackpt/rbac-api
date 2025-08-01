namespace RBACapi.Models.Dtos
{
    public class FormOptionsDto
    {
        public List<string> Applications { get; set; } = new List<string>();
        public List<string> Roles { get; set; } = new List<string>();
    }
}