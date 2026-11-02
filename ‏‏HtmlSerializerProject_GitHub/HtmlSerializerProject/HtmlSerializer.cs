using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HtmlSerializerProject
{
    internal class HtmlSerializer
    {
        public async Task<string> Load(string url)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();
            return html;
        }

        private List<string> SplitToParts(string html)
        {
            var cleanHtml = Regex.Replace(html, @"\s+", " ");

            var parts = Regex.Split(cleanHtml, @"(<[^>]*>)")
                             .Select(s => s.Trim())
                             .Where(s => s.Length > 0)
                             .ToList();
            return parts;
        }

        public HtmlElement Serialize(string html)
        {
            var parts = SplitToParts(html);

            var root = new HtmlElement { Name = "root" };
            var current = root;

            foreach (var part in parts)
            {
                // ----- טקסט רגיל -----
                if (!part.StartsWith("<"))
                {
                    current.InnerHtml += part;
                    continue;
                }

                // ----- הערה או DOCTYPE -----
                if (part.StartsWith("<!"))
                    continue;

                var content = part.Substring(1, part.Length - 2).Trim();
                var firstWord = content.Split(' ')[0].ToLower();

                // ----- סוף ה-HTML -----
                if (firstWord == "/html")
                    break;

                // ----- תגית סוגרת -----
                if (firstWord.StartsWith("/"))
                {
                    if (current.Parent != null)
                        current = current.Parent;
                    continue;
                }

                // ----- תגית פותחת -----
                var tagName = firstWord.TrimEnd('/');

                if (!HtmlHelper.Instance.HtmlTags.Contains(tagName))
                {
                    current.InnerHtml += part;
                    continue;
                }

                var element = new HtmlElement
                {
                    Name = tagName,
                    Parent = current
                };
                current.Children.Add(element);

                ParseAttributes(element, content.Substring(firstWord.Length));

                bool isSelfClosing = content.EndsWith("/")
                    || HtmlHelper.Instance.HtmlVoidTags.Contains(tagName);

                if (!isSelfClosing)
                    current = element;
            }

            return root;
        }


        private void ParseAttributes(HtmlElement element, string attributesText)
        {
            var regex = new Regex(@"(?<name>[\w:-]+)\s*=\s*(?:""(?<value>[^""]*)""|'(?<value>[^']*)')");

            foreach (Match match in regex.Matches(attributesText))
            {
                var name = match.Groups["name"].Value.ToLower();
                var value = match.Groups["value"].Value;

                element.Attributes[name] = value;

                if (name == "id")
                    element.Id = value;

                if (name == "class")
                    element.Classes = value.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

    }
}

