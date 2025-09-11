namespace LibraryManagementSystem.Models
{
    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int ActiveMembers { get; set; }
        public int BooksBorrowedThisMonth { get; set; }
        public double SatisfactionRating { get; set; }
        public List<Book> FeaturedBooks { get; set; } = new();
    }
}
