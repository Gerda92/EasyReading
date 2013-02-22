using eBdb.EpubReader;
using HtmlAgilityPack;
using MvcApplication3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace MvcApplication3.Lib
{
    public class BookFormatter
    {
        public static string GetHtmlBody(string path)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(System.Web.Hosting.HostingEnvironment.MapPath(path));
            return doc.DocumentNode.SelectSingleNode("//body").InnerHtml;
        }
        public static string GetHtmlNavs(string path)
        {
            return "";
        }
        public static Book CreateHtmlFile(string filePath)
        {
            Book book = new Book();
            Epub EpubFile = new Epub(filePath);

            book.Title = EpubFile.Title[0];
            book.Author = EpubFile.Creator[0];
            book.Language = EpubFile.Language[0];
            //Group = group;

            string bookHtml = EpubFile.GetContentAsHtml();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(bookHtml);

            // deletes the content that stupid epubreader appends
            /*
            var trash = doc.DocumentNode.SelectNodes("//body/*[position()<3]");
            foreach (HtmlNode node in trash)
            {
                node.Remove();
            }
            */
            // selects all p and div that may contain text (or may not...)
            // var nodes = doc.DocumentNode.SelectNodes("//p | //div");
            var nodes = doc.DocumentNode.SelectNodes("//body//*[text()[normalize-space()]]");
            int counter = 0;
            foreach (HtmlNode ptag in nodes)
            {
                string p = ptag.InnerHtml;
                string newp = "";

                Regex tagMatch = new Regex("(<[^<>]*>)");
                var splits = tagMatch.Split(p);
                foreach (string text in tagMatch.Split(p))
                {
                    if (tagMatch.IsMatch(text))
                    {
                        newp += text; continue;
                    }
                    
                    // long-suffering regex matching sentences (with <br/>!)
                    // Regex sentenceMatch = new Regex("[^ \f\n\r\t\v]([^.!?<>])+([.!?]\"|[.!?]|[<]br[/]*[>])");
                    Regex sentenceMatch = new Regex("([^ \f\n\r\t\v][^.!?]*[.!?]+[\" ]*)");
                    foreach (string chunk in sentenceMatch.Split(text))
                    {
                        if (chunk.Length > 0)
                            newp += "<span class=\"sentence\" id=" + EpubFile.Language[0] + counter + ">" + chunk + " </span>";
                        counter++;
                    }

                }

                if (newp.Length > 0)
                {
                    ptag.InnerHtml = newp;
                }
                else
                {
                    //ptag.Remove();
                }
            }

            // adding Navs

            HtmlNode toc = doc.DocumentNode.SelectSingleNode("//body//ul");
            foreach (HtmlNode li in toc.ChildNodes)
            {
                string source = li.ChildNodes[0].Attributes["href"].Value;
                source = source.Substring(1, source.Length - 1);
                HtmlNode nearestSentence = doc.DocumentNode.SelectSingleNode(
                    "//body//*[@id='" + source + "']//following::*[@class='sentence']");
                string newId = "part-" + nearestSentence.Id;
                li.ChildNodes[0].Attributes["href"].Value = "#" + newId;
                doc.DocumentNode.SelectSingleNode(
                    "//body//*[@id='" + source + "']").Id = newId;

                if (li.ChildNodes.Count > 1)
                {
                    foreach (HtmlNode child_li in li.ChildNodes[1].ChildNodes)
                    {
                        source = child_li.ChildNodes[0].Attributes["href"].Value;
                        source = source.Substring(1, source.Length - 1);
                        nearestSentence = doc.DocumentNode.SelectSingleNode(
                            "//body//*[@id='" + source + "']//following::*[@class='sentence']");
                        newId = "part-" + nearestSentence.Id;
                        child_li.ChildNodes[0].Attributes["href"].Value = "#" + newId;
                        doc.DocumentNode.SelectSingleNode(
                            "//body//*[@id='" + source + "']").Id = newId;

                    }
                }
            }
            

            string newPath = "~/App_Data/HtmlBooks/book_" + DateTime.Now.Ticks + ".html";
            doc.Save(System.Web.Hosting.HostingEnvironment.MapPath(newPath));

            book.Path = newPath;

            return book;
        }
    }

}