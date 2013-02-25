using MvcApplication3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication3.Controllers
{
    public class AlignmentController : Controller
    {

        private BookDbContext db = new BookDbContext();

        [HttpGet]
        public ActionResult TwoBooks(int id1, int id2)
        {
            var book1 = db.Books.Find(id1);
            var book2 = db.Books.Find(id2);
            return View(new Object[] {book1, book2});
        }

        //
        // POST: /Chapter/Create

        [HttpPost]
        public JsonResult CreateChapterBinding(int id1, int? id2 = null)
        {
            var ch1 = db.Chapters.Single(c => c.Id == id1);
            var ch2 = id2 == null ? null : db.Chapters.Single(c => c.Id == id2);

            ChapterBinding cb = new ChapterBinding()
            {
                ChapterOne = ch1,
                ChapterTwo = ch2
            };

            db.ChapterBindings.Add(cb);
            db.SaveChanges();

            return Json(cb);
        }

        [HttpPost]
        public JsonResult CreateBookmarkBinding(int book1, int book2, string id1, string id2 = null)
        {

            BookmarkBinding mark = new BookmarkBinding()
            {
                BookId1 = book1,
                BookId2 = book2,
                BookmarkId1 = id1,
                BookmarkId2 = id2
            };

            db.BookmarkBindings.Add(mark);
            db.SaveChanges();

            return Json(mark);
        }

    }
}
