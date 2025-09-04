using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? StudentIdNumber { get; set; }
        public string? StudentIdImagePath { get; set; }
        public string? Status { get; set; } = "PendingVerification";
        public DateTime DateCreated {  get; set; } = DateTime.Now;
    }
}
