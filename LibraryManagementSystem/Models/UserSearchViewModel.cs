using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibraryManagementSystem.Models
{
    public class UserSearchViewModel
    {
        public string? SearchString { get; set; }
        public string? Status {  get; set; }
        public IEnumerable<ApplicationUser>? Users { get; set; } = new List<ApplicationUser>();
        public List<SelectListItem> StatusList { get; set; } = new List<SelectListItem>();
    }
}
