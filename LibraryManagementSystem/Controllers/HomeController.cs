using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Get total books count
            var totalBooks = await _context.Books.CountAsync();

            // Get active members count
            var activeMembers = await _context.Users.CountAsync();

            // Get books borrowed this month
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var booksBorrowedThisMonth = await _context.Borrows
                .Where(b => b.BorrowedAt >= startOfMonth && b.BorrowedAt < startOfMonth.AddMonths(1))
                .CountAsync();

            // Calculate satisfaction rating (simplified)
            var totalBorrows = await _context.Borrows.CountAsync();
            var overdueBorrows = await _context.Borrows
                .Where(b => b.ReturnedAt == null && b.DueAt < DateTime.Today)
                .CountAsync();

            double satisfactionRating = 5.0;
            if (totalBorrows > 0)
            {
                var overdueRate = (double)overdueBorrows / totalBorrows;
                satisfactionRating = Math.Max(5.0 - (overdueRate * 2.0), 1.0);
            }

            // Get featured books (most popular available books)
            var featuredBooks = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .OrderByDescending(b => b.TotalCopies - b.AvailableCopies)
                .Take(3)
                .ToListAsync();

            // Pass data to view
            ViewData["TotalBooks"] = totalBooks;
            ViewData["ActiveMembers"] = activeMembers;
            ViewData["BooksBorrowedThisMonth"] = booksBorrowedThisMonth;
            ViewData["SatisfactionRating"] = satisfactionRating.ToString("F1");
            ViewData["FeaturedBooks"] = featuredBooks;

            return View();
        }
    }
}