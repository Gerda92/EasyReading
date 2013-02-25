using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MvcApplication3.Models
{
    public class BookDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<BookGroup> BookGroups { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<ChapterBinding> ChapterBindings { get; set; }
        public DbSet<BookmarkBinding> BookmarkBindings { get; set; }
    }
}