using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LibraryManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public static readonly Dictionary<string, string[]> Transitions = new()
        {
            ["PendingVerification"] = new[] { "Active", "Rejected", "Banned" },
            ["Active"] = new[] { "Suspended", "Banned", "Inactive", "PendingVerification" },
            ["Suspended"] = new[] { "Active", "Inactive", "Banned" },
            ["Banned"] = Array.Empty<string>(),
            ["Rejected"] = new[] { "PendingVerification", "Banned" },
            ["Inactive"] = new[] { "Active", "PendingVerification" }
        };

        public static string[] GetAllowedTransitions(string currentStatus)
        {
            return Transitions.ContainsKey(currentStatus)
                ? Transitions[currentStatus]
                : Array.Empty<string>();
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(UserSearchViewModel model)
        {
            // Default status load - show pending verification first for admin workflow
            if (string.IsNullOrWhiteSpace(model.Status))
            {
                model.Status = "PendingVerification";
            }

            // Get all users for accurate status counts (not filtered)
            var allUsers = await _userManager.Users.ToListAsync();
            ViewBag.AllUsers = allUsers;

            // Apply search and status filters
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.SearchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.FullName.Contains(model.SearchString) ||
                    u.Email.Contains(model.SearchString) ||
                    u.StudentIdNumber.Contains(model.SearchString));
            }

            if (!string.IsNullOrWhiteSpace(model.Status))
            {
                usersQuery = usersQuery.Where(u => u.Status == model.Status);
            }

            // Order by status priority and then by creation date
            usersQuery = usersQuery.OrderBy(u => u.Status == "PendingVerification" ? 0 :
                                           u.Status == "Active" ? 1 :
                                           u.Status == "Suspended" ? 2 :
                                           u.Status == "Rejected" ? 3 :
                                           u.Status == "Inactive" ? 4 : 5)
                                    .ThenByDescending(u => u.Id); // Assuming newer users have higher IDs

            model.Users = await usersQuery.ToListAsync();

            // Add some metadata for better UX
            ViewBag.TotalUsers = allUsers.Count;
            ViewBag.FilteredCount = model.Users?.Count() ?? 0;

            return View(model);
        }

        // GET: Admin/Users/Details/id
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                TempData["Error"] = "User ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // Get allowed status transitions for this user
            var allowedStatuses = GetAllowedTransitions(user.Status);

            // Pass allowedStatuses via ViewBag or ViewData
            ViewBag.AllowedStatuses = allowedStatuses;
            ViewBag.CurrentStatus = user.Status;

            return View(user);
        }

        // POST: Admin/Users/Verify
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(string id, string newStatus, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newStatus))
            {
                TempData["Error"] = "Invalid request. User ID or new status missing.";
                return ReturnToAppropriateView(returnUrl, null);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return ReturnToAppropriateView(returnUrl, null);
            }

            var allowedStatuses = GetAllowedTransitions(user.Status);

            if (!allowedStatuses.Contains(newStatus))
            {
                TempData["Error"] = $"Invalid status transition from {user.Status} to {newStatus}. This action is not allowed.";
                return ReturnToAppropriateView(returnUrl, user.Status);
            }

            var oldStatus = user.Status;
            user.Status = newStatus;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                TempData["Error"] = $"Failed to update user status: {errors}";
                return ReturnToAppropriateView(returnUrl, oldStatus);
            }

            // Log the status change (you might want to add audit logging here)
            TempData["Success"] = $"User '{user.FullName}' status successfully changed from {oldStatus} to {newStatus}.";

            // Smart redirect based on the action
            return ReturnToAppropriateView(returnUrl, newStatus, id);
        }

        private IActionResult ReturnToAppropriateView(string returnUrl, string status, string userId = null)
        {
            // If we have a return URL, use it
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            // If we have a user ID, return to details
            if (!string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Details), new { id = userId });
            }

            // Otherwise return to index with status filter
            if (!string.IsNullOrEmpty(status))
            {
                return RedirectToAction(nameof(Index), new { status = status });
            }

            // Default fallback
            return RedirectToAction(nameof(Index));
        }

        // Helper method to get user statistics for dashboard or other views
        [HttpGet]
        public async Task<JsonResult> GetUserStatistics()
        {
            var allUsers = await _userManager.Users.ToListAsync();

            var statistics = new
            {
                TotalUsers = allUsers.Count,
                PendingVerification = allUsers.Count(u => u.Status == "PendingVerification"),
                Active = allUsers.Count(u => u.Status == "Active"),
                Suspended = allUsers.Count(u => u.Status == "Suspended"),
                Banned = allUsers.Count(u => u.Status == "Banned"),
                Rejected = allUsers.Count(u => u.Status == "Rejected"),
                Inactive = allUsers.Count(u => u.Status == "Inactive"),
                EmailVerified = allUsers.Count(u => u.EmailConfirmed),
                EmailUnverified = allUsers.Count(u => !u.EmailConfirmed)
            };

            return Json(statistics);
        }
    }
}