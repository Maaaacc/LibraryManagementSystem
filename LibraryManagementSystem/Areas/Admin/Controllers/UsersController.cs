using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Centralized allowed transitions
        private static readonly Dictionary<string, string[]> StatusTransitions = new()
        {
            ["PendingVerification"] = new[] { "Active", "Rejected", "Banned" },
            ["Active"] = new[] { "Suspended", "Banned", "Inactive", "PendingVerification" },
            ["Suspended"] = new[] { "Active", "Inactive", "Banned" },
            ["Banned"] = Array.Empty<string>(),
            ["Rejected"] = new[] { "PendingVerification", "Banned" },
            ["Inactive"] = new[] { "Active", "PendingVerification" }
        };

        private string[] GetAllowedTransitions(string currentStatus)
        {
            return StatusTransitions.ContainsKey(currentStatus)
                ? StatusTransitions[currentStatus]
                : Array.Empty<string>();
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(UserSearchViewModel model)
        {
            // Default status load
            if (string.IsNullOrWhiteSpace(model.Status))
            {
                model.Status = "PendingVerification";
            }

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

            model.Users = await usersQuery.ToListAsync();

            return View(model);
        }

        // GET: Admin/Users/Details/id
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/Users/Verify
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(string id, string newStatus)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newStatus))
            {
                TempData["Error"] = "Invalid request. User ID or new status missing.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var allowedStatuses = GetAllowedTransitions(user.Status);
            if (!allowedStatuses.Contains(newStatus))
            {
                TempData["Error"] = $"Invalid status change from {user.Status} to {newStatus}.";
                return RedirectToAction(nameof(Index), new { status = user.Status });
            }

            user.Status = newStatus;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Failed to update user status.";
                return RedirectToAction(nameof(Index), new { status = user.Status });
            }

            TempData["Success"] = $"User status updated to {newStatus}.";
            return RedirectToAction(nameof(Index), new { status = newStatus });
        }
    }
}
