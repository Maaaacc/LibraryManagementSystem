 using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        public string? Author { get; set; }

        [Required(ErrorMessage = "ISBN is required.")]
        public string? ISBN { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? CoverImagePath { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
    }
}
