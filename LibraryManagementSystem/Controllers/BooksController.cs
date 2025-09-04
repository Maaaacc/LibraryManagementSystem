using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(BookSearchViewModel model)
        {
            var booksQuery = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(model.SearchString))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(model.SearchString) ||
                    b.Author.Contains(model.SearchString) ||
                    b.ISBN.Contains(model.SearchString));
            }
            if (!string.IsNullOrEmpty(model.Category))
            {
                booksQuery = booksQuery.Where(b => b.Category == model.Category);
            }
            if (model.AvailableOnly)
            {
                booksQuery = booksQuery.Where(b => b.AvailableCopies > 0);
            }

            model.Books = await booksQuery.ToListAsync();

            //Get categories for dropdown
            model.Categories = await _context.Books
                .Select(b => b.Category)
                .Distinct()
                .ToListAsync();

            //Check current user's acitve borrows
            var user = await _userManager.GetUserAsync(User);
            if(user != null)
            {
                model.ActiveBorrowCount = await _context.Borrows
                    .Where(b => b.UserId == user.Id && b.ReturnedAt == null)
                    .CountAsync();
            }

            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            //Pass the user's borrow count to view

            var user = await _userManager.GetUserAsync(User);
            int activeBorrowCount = 0;
            if(user != null)
            {
                activeBorrowCount = await _context.Borrows
                    .Where(b => b.UserId == user.Id && b.ReturnedAt == null)
                    .CountAsync();
            }

            ViewBag.ActiveBorrowCount = activeBorrowCount;

            return View(book);
        }


    }
}