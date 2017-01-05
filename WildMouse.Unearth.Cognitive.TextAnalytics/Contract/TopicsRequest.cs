using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildMouse.Unearth.Cognitive.TextAnalytics.Contract
{
    public class TopicsRequest
    {
        public class Document
        {
            public string id { get; set; }
            public string text { get; set; }
        }

        public List<string> stopWords { get; set; }
        public List<string> topicsToExclude { get; set; }
        public List<Document> documents { get; set; }
    }
}
