using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        // Existing properties
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int PendingUsers { get; set; }
        public Dictionary<string, int>? UserStatusCounts { get; set; }
        public List<ApplicationUser>? PendingUsersList { get; set; }

        public int TotalBooks { get; set; }
        public int AvailableCopies { get; set; }
        public List<KeyValuePair<string, double>>? BookCategoryPercentages { get; set; }

        public int CurrentlyBorrowed { get; set; }
        public int OverdueBooks { get; set; }

        public List<KeyValuePair<string, int>>? BorrowingTrend { get; set; }
        public List<Borrow>? RecentBorrows { get; set; }

        // New property for Recent Activities
        public List<RecentActivity>? RecentActivities { get; set; }
    }

    public class RecentActivity
    {
        public string UserName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - Timestamp;

                if (timeSpan.TotalMinutes < 1)
                    return "Just now";
                else if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
                else if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
                else if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
                else
                    return Timestamp.ToString("MMM dd, yyyy");
            }
        }
    }
}