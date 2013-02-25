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

            // deletes the content that stupid epubreader (okay, good reader) appends
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
                        if (Regex.Replace(chunk, "[ \f\n\r\t\v]", "").Length > 0)
                        {
                            newp += "<span class=\"sentence\" id=" + EpubFile.Language[0] + counter + ">" + chunk + " </span>";
                            counter++;
                        }
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
            List<Chapter> toc = extractTOC(doc.DocumentNode, doc.DocumentNode.SelectSingleNode("//ul"), book);
            book.Chapters = toc;

            string newPath = "~/App_Data/HtmlBooks/book_" + DateTime.Now.Ticks + ".html";
            doc.Save(System.Web.Hosting.HostingEnvironment.MapPath(newPath));

            book.Path = newPath;

            return book;
        }

        public static List<Chapter> extractTOC(HtmlNode root, HtmlNode toc, Book book, Chapter parent = null)
        {
            HtmlNodeCollection nodes = toc.ChildNodes;
            List<Chapter> chapters = new List<Chapter>();

            for (int i = 0; i < nodes.Count; i++)
            {
                HtmlNode li = nodes[i];

                Chapter ch = new Chapter();
                //ch.InBook = book;
                ch.Parent = parent;

                // source (epub) chapter's attribute
                string source = li.ChildNodes[0].Attributes["href"].Value;
                source = source.Substring(1, source.Length - 1);

                // calculate chapter id
                HtmlNode nearestSentence = getNextSentence(root, source);
                string newId = "ch-" + nearestSentence.Id;

                ch.ChapterId = newId;
                ch.Order = getOrder(nearestSentence.Id);

                //refresh hrefs in contents and ids of chapter anchors
                nodes[i].FirstChild.Attributes["href"].Value = "#" + newId;
                root.SelectSingleNode(
                    "//body//*[@id='" + source + "']").Id = newId;

                if (li.ChildNodes.Count > 1)
                {
                    List<Chapter> child_li = extractTOC(root, li.ChildNodes[1], book, ch);
                    ch.Children = child_li;

                }
                chapters.Add(ch);
            }
            return chapters;
        }

        public static HtmlNode getNextSentence(HtmlNode root, string id)
        {
            var child = root.SelectSingleNode("//*[@id='" + id + "']//*[@class='sentence']");
            if (child != null)
                return child;
            return root.SelectSingleNode(
                            "//*[@id='" + id + "']//following::*[@class='sentence']");
        }

        public static HtmlNodeCollection getNodesBetween(HtmlNode root, string  id1, string id2)
        {
            return root.SelectNodes("//*[@id='" + id1 +
                "']/following-sibling::*[following-sibling::*[@id='" + id2 + "']]");
        }

        public static HtmlNode getById(HtmlNode root, string id)
        {
            return root.SelectSingleNode("//*[@id='" + id + "']");
        }

        public static int getOrder(string id)
        {
            string num = Regex.Match(id, "[0-9]+$").Value;
            int parsed = -1;
            Int32.TryParse(num, out parsed);
            return parsed;
        }
    }


}