using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ManageBooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;


        public ManageBooksController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

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

            return View(model);
        }

        // GET: Admin/ManageBooks/Details/5

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(new List<string>
            {
                "Fiction",
                "Science",
                "History",
                "Biography",
                "Technology"
            });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile CoverImage)
        {
            if (ModelState.IsValid)
            {
                if (CoverImage != null && CoverImage.Length > 0)
                {
                    var imagePath = await SaveBookCoverAsync(CoverImage, book.Title);
                    book.CoverImagePath = imagePath;
                }

                _context.Add(book);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Book created successfully!" });
            }

            return PartialView("Create", book);
        }

        // GET: Admin/ManageBooks/Edit/5

        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            ViewData["Categories"] = await _context.Books
                .Select(b => b.Category)
                .Distinct()
                .ToListAsync();

            return PartialView("Edit", book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,ISBN,Category,TotalCopies,AvailableCopies,CoverImagePath")] Book book, IFormFile CoverImage)
        {
            if (id != book.Id)
                return Json(new { success = false, message = "Invalid book ID" });

            // ignore CoverImage validation during edit
            ModelState.Remove("CoverImage");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage).ToList();

                return Json(new { success = false, message = "Validation failed", errors });
            }

            try
            {
                if (CoverImage != null && CoverImage.Length > 0)
                {
                    var imagePath = await SaveBookCoverAsync(CoverImage, book.Title);
                    book.CoverImagePath = imagePath;
                }

                _context.Update(book);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Book updated successfully!" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "A concurrency error occurred. Please try again." });
            }
        }




        // GET: Admin/ManageBooks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return PartialView("Delete", book); // return confirmation modal view
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return Json(new { success = false });
            }

            // Delete cover image if exists
            if (!string.IsNullOrEmpty(book.CoverImagePath))
            {
                var fullPath = Path.Combine(_environment.WebRootPath, book.CoverImagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Book permanently deleted successfully!" });
        }


        private async Task<string> SaveBookCoverAsync(IFormFile coverImage, string bookTitle)
        {
            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "bookcovers");
            Directory.CreateDirectory(uploadFolder);

            // Create safe filename
            var safeTitle = string.Join("_", bookTitle.Split(Path.GetInvalidFileNameChars()));
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var extension = Path.GetExtension(coverImage.FileName);

            // Final filename format: title_timestamp.ext
            var fileName = $"{safeTitle}_{timestamp}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await coverImage.CopyToAsync(stream);

            return "/uploads/bookcovers/" + fileName;
        }
    }
}