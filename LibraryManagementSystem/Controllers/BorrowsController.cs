using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;


namespace LibraryManagementSystem.Controllers
{
    [Authorize(Policy = "ActiveUserOnly")]
    public class BorrowsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int MaxActiveBorrows = 3;
        public BorrowsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var activeBorrowsCount = await _context.Borrows
                .Where(b => b.UserId == user.Id && b.ReturnedAt == null)
                .CountAsync();
            if (activeBorrowsCount >= MaxActiveBorrows)
            {
                TempData["Error"] = "You have reached the maximum number of active borrowings (3).";
                return RedirectToAction("Index", "Books");
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null || book.AvailableCopies <= 0)
            {
                TempData["Error"] = "Book is not available for borrowing.";
                return RedirectToAction("Index", "Books");
            }

            var borrow = new Borrow
            {
                BookId = bookId,
                UserId = user.Id,
                BorrowedAt = DateTime.Now,
                DueAt = DateTime.Now.AddMinutes(1),
                ReturnedAt = null,
                Status = "Borrowed"
            };

            _context.Borrows.Add(borrow);
            book.AvailableCopies--;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"You have borrowed '{book.Title}'. Due date is {borrow.DueAt:d}.";
            return RedirectToAction("MyBorrows", "Borrows");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var borrow = await _context.Borrows
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id && b.ReturnedAt == null);

            if (borrow == null)
                return NotFound();

            borrow.ReturnedAt = DateTime.Now;
            borrow.Status = "Returned";
            borrow.Book.AvailableCopies++;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"You have returned '{borrow.Book.Title}'.";
            return RedirectToAction("MyBorrows");
        }

        public async Task<IActionResult> MyBorrows()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var borrows = await _context.Borrows
                .Include(b => b.Book)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.BorrowedAt)
                .ToListAsync();

            var activeCount = borrows.Count(b => b.ReturnedAt == null);
            var overdueCount = borrows.Count(b => b.ReturnedAt == null && b.DueAt < DateTime.Now);
            var totalCount = borrows.Count();

            ViewData["ActiveCount"] = activeCount;
            ViewData["MaxActiveBorrows"] = MaxActiveBorrows;
            ViewData["OverdueCount"] = overdueCount;
            ViewData["TotalCount"] = totalCount;

            return View(borrows);
        }

    }
}