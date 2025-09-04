using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class BookSearchViewModel
    {
        public string? SearchString { get; set; }

        public string? Category { get; set; }

        public bool AvailableOnly { get; set; } = false;

        public IEnumerable<Book>? Books { get; set; } = new List<Book>();

        public int ActiveBorrowCount {  get; set; }

        public List<string>? Categories { get; set; }



    }
}