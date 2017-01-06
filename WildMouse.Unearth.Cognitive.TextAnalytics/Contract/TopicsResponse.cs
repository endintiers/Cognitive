using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildMouse.Unearth.Cognitive.TextAnalytics.Contract
{
    public class TopicsResponse
    {

        public class Topic
        {
            public string id { get; set; }
            public double score { get; set; }
            public string keyPhrase { get; set; }
        }

        public class TopicAssignment
        {
            public string documentId { get; set; }
            public string topicId { get; set; }
            public double distance { get; set; }
        }

        public class Error
        {
            public string id { get; set; }
            public string message { get; set; }
        }

        public class OperationProcessingResult
        {
            public List<Topic> topics { get; set; }
            public List<TopicAssignment> topicAssignments { get; set; }
            public List<Error> errors { get; set; }
            public string discriminator { get; set; }
        }

        public string status { get; set; }
        public string createdDateTime { get; set; }
        public string operationType { get; set; }
        public string message { get; set; }
        public OperationProcessingResult operationProcessingResult { get; set; }
    }
}
