using eBdb.EpubReader;
using HtmlAgilityPack;
using EasyReading.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace EasyReading.Lib
{
    public class BookFormatter
    {

        #region create and prepare book

        public static Book ExtractBook(string filePath)
        {
            Book book = new Book();

            book.SourcePath = filePath;

            Epub EpubFile = new Epub(System.Web.Hosting.HostingEnvironment.MapPath(filePath));

            book.Title = EpubFile.Title[0];
            book.Author = EpubFile.Creator[0];
            book.Language = EpubFile.Language[0];

            string bookHtml = EpubFile.GetContentAsHtml();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(bookHtml);

            string newPath = "~/App_Data/HtmlBooks/book_" + DateTime.Now.Ticks + ".html";

            book.Path = newPath;

            doc.Save(System.Web.Hosting.HostingEnvironment.MapPath(newPath));

            return book;
        }

        public static void PrepareForAlignment(Book book)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(System.Web.Hosting.HostingEnvironment.MapPath(book.Path));

            StripTrash(doc);

            BreakIntoSentences(doc.DocumentNode, book.Id + "-" + book.Language + "-");

            List<Chapter> toc = extractTOC(doc.DocumentNode, doc.DocumentNode.SelectSingleNode("//ul"), book);
            book.Chapters = toc;

            doc.Save(System.Web.Hosting.HostingEnvironment.MapPath(book.Path));

        }

        #endregion create and prepare book

        #region get book

        public static string GetHtmlBody(string path)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(System.Web.Hosting.HostingEnvironment.MapPath(path));
            return doc.DocumentNode.SelectSingleNode("//body").InnerHtml;
        }

        #endregion get book

        #region private methods

        private static List<Chapter> extractTOC(HtmlNode root, HtmlNode toc, Book book, Chapter parent = null)
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
                string newId = "ch-" + book.Id + "-" + nearestSentence.Id;

                ch.ChapterId = newId;
                ch.Order = getOrder(nearestSentence.Id);

                //refresh hrefs in contents and ids of chapter anchors

                nodes[i].FirstChild.Attributes["href"].Value = "#" + newId;

                nodes[i].FirstChild.SetAttributeValue("class", "chapter");

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

        private static HtmlNode getNextSentence(HtmlNode root, string id)
        {
            var child = root.SelectSingleNode("//*[@id='" + id + "']//*[@class='sentence']");
            if (child != null)
                return child;
            return root.SelectSingleNode(
                            "//*[@id='" + id + "']//following::*[@class='sentence']");
        }

        private static HtmlNodeCollection getNodesBetween(HtmlNode root, string id1, string id2)
        {
            return root.SelectNodes("//*[@id='" + id1 +
                "']/following-sibling::*[following-sibling::*[@id='" + id2 + "']]");
        }

        private static HtmlNode getById(HtmlNode root, string id)
        {
            return root.SelectSingleNode("//*[@id='" + id + "']");
        }

        private static int getOrder(string id)
        {
            string num = Regex.Match(id, "[0-9]+$").Value;
            int parsed = -1;
            Int32.TryParse(num, out parsed);
            return parsed;
        }

        private static void StripTrash(HtmlDocument doc)
        {
            // deletes the content that stupid epubreader (okay, good reader) appends
            // RemoveNodeCollection(doc.DocumentNode.SelectNodes("//body/*[position()<3]"));
            // gutengerg's header and footer
            var pgheader = doc.DocumentNode.SelectNodes("//body//*[contains(@class, 'pgheader')]");
            if (pgheader != null)
                RemoveNodeCollection(pgheader);

        }

        private static void RemoveNodeCollection(HtmlNodeCollection a)
        {
            foreach (HtmlNode node in a)
            {
                node.Remove();
            }
        }


        private static void BreakIntoSentences(HtmlNode root, string sentenceIdPrefix)
        {
            // selects all p and div that may contain text (or may not...)
            var nodes = root.SelectNodes("//body//*[text()[normalize-space()]]");

            int counter = 0;
            foreach (HtmlNode ptag in nodes)
            {
                string p = ptag.InnerHtml;
                string newp = "";

                // split by tags (mainly for <br/>)
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
                            newp += "<span class=\"sentence\" id=" + sentenceIdPrefix +
                                "-" + counter + ">" + chunk + " </span>";
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
        }

        #endregion private methods

    }



}