using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadAndWrite.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverPath { get; set; }
        public string Content { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public bool IsFrozen { get; set; }
        public double AvgRating { get; set; }
        public string Genres { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public bool IsFrozen { get; set; }
    }

    public class Review
    {
        public int ReviewId { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public bool IsFrozen { get; set; }
    }

    public class Genre
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; }
    }

    public class ReadingList
    {
        public int ReadingListId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string AuthorName { get; set; }
        public string Section { get; set; }
        public double AvgRating { get; set; }
    }
}