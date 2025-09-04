using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel();
            var users = await _userManager.Users.ToListAsync();

            // User statistics
            model.TotalUsers = users.Count;
            model.ActiveUsers = users.Count(u => u.Status == "Active");
            model.PendingUsers = users.Count(u => u.Status == "PendingVerification");
            model.UserStatusCounts = users.GroupBy(u => u.Status ?? "Unknown")
                                          .ToDictionary(g => g.Key, g => g.Count());

            model.PendingUsersList = users.Where(u => u.Status == "PendingVerification")
                                          .Take(3)
                                          .ToList();

            // Book statistics
            model.TotalBooks = await _context.Books.CountAsync();
            model.AvailableCopies = await _context.Books.SumAsync(b => b.AvailableCopies);

            var categoryCounts = await _context.Books
                .GroupBy(b => b.Category ?? "Unknown")
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            int totalBooksInCategories = categoryCounts.Sum(c => c.Count);
            model.BookCategoryPercentages = totalBooksInCategories > 0
               ? categoryCounts.Select(c => new KeyValuePair<string, double>(c.Category, (double)c.Count * 100.0 / totalBooksInCategories)).ToList()
               : new List<KeyValuePair<string, double>>();

            // Borrow statistics
            model.CurrentlyBorrowed = await _context.Borrows.CountAsync(b => b.ReturnedAt == null);
            model.OverdueBooks = await _context.Borrows.CountAsync(b => b.ReturnedAt == null && b.DueAt < DateTime.Now);

            // Borrowing trend
            var sixMonthsAgo = DateTime.Today.AddMonths(-5);
            var startOfPeriod = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            var borrowGroups = await _context.Borrows
                .Where(b => b.BorrowedAt >= startOfPeriod)
                .GroupBy(b => new { b.BorrowedAt.Year, b.BorrowedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var allMonths = new List<KeyValuePair<string, int>>();
            for (var i = 0; i < 6; i++)
            {
                var month = startOfPeriod.AddMonths(i);
                var count = borrowGroups.FirstOrDefault(g => g.Year == month.Year && g.Month == month.Month)?.Count ?? 0;
                allMonths.Add(new KeyValuePair<string, int>($"{month.Year}-{month.Month:D2}", count));
            }
            model.BorrowingTrend = allMonths;

            // Recent Activities
            model.RecentActivities = await GetRecentActivities();

            return View(model);
        }

        private async Task<List<RecentActivity>> GetRecentActivities()
        {
            var activities = new List<RecentActivity>();
            var sevenDaysAgo = DateTime.Now.AddDays(-7);

            // Recent borrows
            var recentBorrows = await _context.Borrows
                .Where(b => b.BorrowedAt >= sevenDaysAgo)
                .Include(b => b.User)
                .Include(b => b.Book)
                .OrderByDescending(b => b.BorrowedAt)
                .Take(10)
                .ToListAsync();

            foreach (var borrow in recentBorrows)
            {
                activities.Add(new RecentActivity
                {
                    UserName = borrow.User?.FullName ?? "Unknown User",
                    Description = $"Borrowed {borrow.Book?.Title ?? "Unknown Book"}",
                    ActivityType = "Borrowed",
                    Timestamp = borrow.BorrowedAt
                });
            }

            // Recent returns
            var recentReturns = await _context.Borrows
                .Where(b => b.ReturnedAt != null && b.ReturnedAt >= sevenDaysAgo)
                .Include(b => b.User)
                .Include(b => b.Book)
                .OrderByDescending(b => b.ReturnedAt)
                .Take(10)
                .ToListAsync();

            foreach (var returnedBorrow in recentReturns)
            {
                activities.Add(new RecentActivity
                {
                    UserName = returnedBorrow.User?.FullName ?? "Unknown User",
                    Description = $"Returned \"{returnedBorrow.Book?.Title ?? "Unknown Book"}\"",
                    ActivityType = "Returned",
                    Timestamp = returnedBorrow.ReturnedAt ?? DateTime.Now
                });
            }

            // Recent registrations (users created in the last 7 days)
            var recentUsers = await _userManager.Users
                .Where(u => u.DateCreated >= sevenDaysAgo)
                .OrderByDescending(u => u.DateCreated)
                .Take(10)
                .ToListAsync();

            foreach (var user in recentUsers)
            {
                activities.Add(new RecentActivity
                {
                    UserName = user.FullName ?? "Unknown User",
                    Description = "Registered",
                    ActivityType = "Registered",
                    Timestamp = user.DateCreated
                });
            }

            // Recent overdue books
            var overdueBooks = await _context.Borrows
                .Where(b => b.ReturnedAt == null && b.DueAt < DateTime.Now && b.DueAt >= sevenDaysAgo)
                .Include(b => b.User)
                .Include(b => b.Book)
                .OrderByDescending(b => b.DueAt)
                .Take(5)
                .ToListAsync();

            foreach (var overdue in overdueBooks)
            {
                activities.Add(new RecentActivity
                {
                    UserName = overdue.User?.FullName ?? "Unknown User",
                    Description = $"\"{overdue.Book?.Title ?? "Unknown Book"}\" is overdue",
                    ActivityType = "Overdue",
                    Timestamp = overdue.DueAt
                });
            }

            // Sort all activities by timestamp (most recent first) and take top 8
            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(4)
                .ToList();
        }
    }
}