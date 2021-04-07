using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XCentium.Models;
using System.Net.Http;
using System.Net;
using System.Xml;
using System.Text.RegularExpressions;
using System.Text;

namespace XCentium.Controllers
{
    public class HomeController : Controller
    {

        public HomeController()
        {

        }

        public IActionResult Index(FormModel model)
        {
            string url = model.UserInputUrl;

            if (CheckURLValid(url))
            {

                string host = GetURLHost(url);
                var response = CallUrl(url).Result;
                var imageList = GetImagePaths(host, response);
                (int totalCount, List<WordCount> words) = GetWordCounts(response);

                model.Images = imageList;
                model.TotalWordCount = totalCount;
                model.Words = words.OrderByDescending(x => x.Count).Take(10).ToList();
            }
            else
            {
                model.ErrorText = "The url is not valid";
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        private string GetURLHost(string url)
        {
            Uri myUri = new Uri(url);
            return myUri.Host;
        }

        private bool CheckURLValid(string strURL)
        {
            if (strURL == null) return false;
            return Uri.IsWellFormedUriString(strURL, UriKind.Absolute);
        }

        private (int, List<WordCount>) GetWordCounts(string html)
        {
            int totalCount = 0;
            var wordCounts = new List<WordCount>();
            if (html == null)
            {
                return (totalCount, wordCounts);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (var item in doc.DocumentNode.DescendantsAndSelf())
            {
                if (item.NodeType == HtmlNodeType.Text)
                {
                    if (item.InnerText.Trim() != "")
                    {
                        var words = item.InnerText.Split(' ');
                        foreach(var word in words)
                        {
                            StringBuilder cleanWord = new StringBuilder();
                            foreach(var letter in word.ToLower())
                            {
                                if (char.IsLetter(letter))
                                {
                                    cleanWord.Append(letter);
                                }
                            }
                            if(!string.IsNullOrWhiteSpace(cleanWord.ToString())) {
                                totalCount++;
                                var possibleWord = wordCounts.SingleOrDefault(x => x.Word == cleanWord.ToString());
                                if (possibleWord != null)
                                {
                                    possibleWord.Count++;
                                }
                                else
                                {
                                    wordCounts.Add(new WordCount()
                                    {
                                        Word = word,
                                        Count = 1
                                    });
                                }
                            }
                        }
                    }
                }
            }
            
            return (totalCount, wordCounts);
        }

        private List<string> GetImagePaths(string host, string html)
        {
            var imageList = new List<string>();
            if (string.IsNullOrEmpty(html))
            {
                return imageList;
            }

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            HtmlNodeCollection imgs = htmlDoc.DocumentNode.SelectNodes("//img[@src]");
            if (imgs == null)
            {
                return imageList;
            }

            foreach (HtmlNode img in imgs)
            {
                if(img.Attributes["src"] != null)
                {
                    HtmlAttribute src = img.Attributes["src"];
                    var imageUrl = src.Value;

                    //check if relative or absolute image path
                    if (CheckURLValid(imageUrl) || imageUrl.StartsWith("//"))
                    {
                        imageList.Add(imageUrl);
                    }
                    else
                    {
                        imageList.Add(host + imageUrl);
                    }
                }
            }

            return imageList;
        }
    }
}
