using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildMouse.Unearth.Cognitive.TextAnalytics.Contract
{
    public class DetectLanguageResponse
    {
        public class DetectedLanguage
        {
            public string name { get; set; }
            public string iso6391Name { get; set; }
            public double score { get; set; }
        }

        public class Document
        {
            public string id { get; set; }
            public List<DetectedLanguage> detectedLanguages { get; set; }
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
