namespace LibraryManagementSystem.Models
{
    public class DashboardStats
    {
        public int TotalBooks { get; set; }
        public int ActiveMembers { get; set; }
        public int BooksBorrowedThisMonth { get; set; }
        public double SatisfactionRating { get; set; }
    }
}
