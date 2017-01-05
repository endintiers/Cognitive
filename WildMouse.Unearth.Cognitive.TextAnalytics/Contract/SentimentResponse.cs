using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildMouse.Unearth.Cognitive.TextAnalytics.Contract
{
    public class SentimentResponse
    {
        public class Document
        {
            public double score { get; set; }
            public string id { get; set; }
        }
        public class Error
        {
            public string id { get; set; }
            public string message { get; set; }
        }
        public List<Document> documents { get; set; }
        public List<Error> errors { get; set; }
    }
}
