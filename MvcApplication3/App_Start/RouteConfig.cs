using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EasyReading
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "BookChapters",
                url: "Book/{book_id}/Chapter/{action}/{id}",
                defaults: new { controller = "Chapter", action = "Index", id = UrlParameter.Optional }  // Parameter defaults
            );

            routes.MapRoute(
                name: "Alignment",
                url: "Alignment/{id1}/{id2}",
                defaults: new { controller = "Alignment", action = "TwoBooks" }  // Parameter defaults
            );

            routes.MapRoute(
                name: "ChapterBinding",
                url: "Alignment/CreateChapter/{id1}/{id2}",
                defaults: new { controller = "Alignment", action = "CreateChapterBinding" }  // Parameter defaults
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}