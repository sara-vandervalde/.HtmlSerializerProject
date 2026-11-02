using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HtmlSerializerProject
{
    public class HtmlHelper
    {
        private readonly static HtmlHelper _instance = new HtmlHelper();
        public static HtmlHelper Instance => _instance;

        public string[] HtmlTags { get; set; }
        public string[] HtmlVoidTags { get; set; }

        private HtmlHelper()
        {
            var seedDir = Path.Combine(AppContext.BaseDirectory, "Seed");

            var tagsJson = File.ReadAllText(Path.Combine(seedDir, "HtmlTags.json"));
            HtmlTags = JsonSerializer.Deserialize<string[]>(tagsJson) ?? Array.Empty<string>();

            var voidTagsJson = File.ReadAllText(Path.Combine(seedDir, "HtmlVoidTags.json"));
            HtmlVoidTags = JsonSerializer.Deserialize<string[]>(voidTagsJson) ?? Array.Empty<string>();
        }
    }
}
