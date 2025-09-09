﻿namespace LibraryManagementSystem.Models
{
    public class FeaturedBookDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Category { get; set; }
        public int AvailableCopies { get; set; }
        public int TotalCopies { get; set; }
        public string? CoverImagePath { get; set; }
    }

}
