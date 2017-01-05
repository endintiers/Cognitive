using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildMouse.Unearth.Cognitive.TextAnalytics.Contract
{
    public class DetectLanguageRequest
    {
        public class Document
        {
            public string id { get; set; }
            public string text { get; set; }
        }
        public List<Document> documents { get; set; }
    }
}
