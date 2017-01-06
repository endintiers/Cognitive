using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WildMouse.Unearth.Cognitive.TextAnalytics.Contract
{
    public class TopicsInitialResult
    {
        public HttpStatusCode StatusCode { get; set; }
        public string OperationLocation { get; set; }
    }
}
