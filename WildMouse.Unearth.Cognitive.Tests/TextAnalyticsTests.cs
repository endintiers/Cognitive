using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using WildMouse.Unearth.Cognitive.TextAnalytics;
using WildMouse.Unearth.Cognitive.TextAnalytics.Contract;

namespace WildMouse.Unearth.Tests
{
    /// <summary>
    /// Tests for WildMouse.Unearth.Cognitive.TextAnalytics
    /// </summary>
    [TestClass]
    public class TextAnalyticsTests
    {
        private const string _textAnalyticsAPIKey = "{your text analytics API Key here}";

        [TestMethod]
        public void TestKeyPhrasesText()
        {
            // Warning - avoid use of synchronous versions of methods in ASP.NET
            var client = new TextAnalyticsClient(_textAnalyticsAPIKey);
            var keyPhrases = client.GetKeyPhrasesForText("Cada uno sabe (a) donde le aprieta el zapato.", "es");
            Assert.AreEqual("zapato", keyPhrases[0]);
        }

        [TestMethod]
        public async Task TestKeyPhrasesTextAsync()
        {
            var client = new TextAnalyticsClient(_textAnalyticsAPIKey);
            var keyPhrases = await client.GetKeyPhrasesForTextAsync("How now, brown cow?");
            Assert.AreEqual("brown cow", keyPhrases[0]);
        }

        [TestMethod]
        public async Task TestKeyPhrasesAsyncContract()
        {
            var client = new TextAnalyticsClient(_textAnalyticsAPIKey);

            var request = new KeyPhrasesRequest();
            request.documents = new List<KeyPhrasesRequest.Document>();
            request.documents.Add(new KeyPhrasesRequest.Document()
            { id = "1", language = "en", text = "How now, brown cow" });
            request.documents.Add(new KeyPhrasesRequest.Document()
            { id = "2", language = "en", text = "Trust, but verify" });

            // Call for multiple documents
            var response = await client.GetKeyPhrasesAsync(request);

            Assert.AreEqual(0, response.errors.Count);
            Assert.AreEqual(2, response.documents.Count);
        }

        [TestMethod]
        public void TestSentimentText()
        {
            var client = new TextAnalyticsClient(_textAnalyticsAPIKey);
            // Call for a single document
            var score = client.GetSentimentForText("Son tout bon.", "fr");
            Assert.IsTrue(score > 0.5); // Positive
            score = client.GetSentimentForText("Its far too hot here in Melbourne.");
            Assert.IsTrue(score < 0.5); // Negative
        }

        [TestMethod]
        public async Task TestSentimentTextAsync()
        {
            var client = new TextAnalyticsClient(_textAnalyticsAPIKey);
            // Call for a single document
            var score = await client.GetSentimentForTextAsync("What a wonderful day this is.");
            Assert.IsTrue(score > 0.5); // Positive
            score = await client.GetSentimentForTextAsync("This is the worst lunch I have ever had.", "en");
            Assert.IsTrue(score < 0.5); // Negative
        }

        [TestMethod]
        public async Task TestSentimentAsyncContract()
        {
            var client = new TextAnalyticsClient(_textAnalyticsAPIKey);

            var request = new SentimentRequest();
            request.documents = new List<SentimentRequest.Document>();
            request.documents.Add(new SentimentRequest.Document()
            { id = "1", language = "en", text = "What a wonderful day this is." });
            request.documents.Add(new SentimentRequest.Document()
            { id = "2", language = "en", text = "This is the worst lunch I have ever had." });

            // Call for multiple documents
            var response = await client.GetSentimentAsync(request);

            Assert.AreEqual(0, response.errors.Count);
            Assert.AreEqual(2, response.documents.Count);
        }

        [TestMethod]
        public void TestDetectLanguagesText()
        {
            var client = new TextAnalyticsClient(_textAnalyticsAPIKey);
            // Call for a single document
            var language = client.DetectLanguageForText("苦あれば楽あり");
            Assert.AreEqual("ja", language);
            language = client.DetectLanguageForText("Alah bisa karena biasa.");
            Assert.AreEqual("id", language);
        }

        [TestMethod]
        public async Task TestDetectLanguagesTextAsync()
        {
            var client = new TextAnalyticsClient(_textAnalyticsAPIKey);
            // Call for a single document
            var language = await client.DetectLanguageForTextAsync("The Economist is undoubtedly the smartest weekly newsmagazine in the English language. I always look forward to its quirky year-end double issue.");
            Assert.AreEqual("en", language);
            language = await client.DetectLanguageForTextAsync("Anfangen ist leicht, Beharren eine Kunst.");
            Assert.AreEqual("de", language);
        }

        [TestMethod]
        public async Task TestDetectLanguagesAsyncContract()
        {
            var client = new TextAnalyticsClient(_textAnalyticsAPIKey);

            var request = new DetectLanguageRequest();
            request.documents = new List<DetectLanguageRequest.Document>();
            request.documents.Add(new DetectLanguageRequest.Document()
            { id = "1", text = "失败是成功之母" });
            request.documents.Add(new DetectLanguageRequest.Document()
            { id = "2", text = "No dejes para mañana lo que puedas hacer hoy." });

            // Call for multiple documents
            var response = await client.DetectLanguagesAsync(request);

            Assert.AreEqual(0, response.errors.Count);
            Assert.AreEqual(2, response.documents.Count);
            Assert.AreEqual("zh_chs", response.documents[0].detectedLanguages[0].iso6391Name);
            Assert.AreEqual("es", response.documents[1].detectedLanguages[0].iso6391Name);
        }

        [TestMethod]
        public async Task TestDetectTopics()
        {
            var client = new TopicsClient(_textAnalyticsAPIKey);

            var request = new TopicsRequest();
            request.stopWords = new List<string>();
            request.topicsToExclude = new List<string>();
            request.documents = new List<TopicsRequest.Document>();

            var oneHundredStrings = GetOneHundredStrings();
            Assert.AreEqual(100, oneHundredStrings.Length);
            for (int i = 0; i < oneHundredStrings.Length; i++)
            {
                request.documents.Add(new TopicsRequest.Document()
                { id = (i + 1).ToString(), text = oneHundredStrings[i] });
            }

            // Detect Topics (at least 100 documents)
            // Documents should have a common theme. Random text will fail with "Internal error while executing BES operation."
            // Runs batch, will take 5-10 minutes. Status shown in Debug Output every 60 seconds
            var response = await client.DetectTopicsAsync(request);

            Assert.AreEqual("Succeeded", response.status);
            Assert.AreEqual(0, response.operationProcessingResult.errors.Count);
        }

        [TestMethod]
        public async Task TestDetectTopicsInsufficientStrings()
        {
            var client = new TopicsClient(_textAnalyticsAPIKey);

            var request = new TopicsRequest();
            request.stopWords = new List<string>();
            request.topicsToExclude = new List<string>();
            request.documents = new List<TopicsRequest.Document>();
            request.documents.Add(new TopicsRequest.Document()
            { id = "1", text = "Well, this wont work at all" });
            request.documents.Add(new TopicsRequest.Document()
            { id = "2", text = "But how doesn't it work?" });

            var response = await client.DetectTopicsAsync(request);

            Assert.AreEqual("FailedToStart", response.status);
        }

        private string[] GetOneHundredStrings()
        {
            var oneHundredStrings = new List<string>();
            foreach (string line in File.ReadLines(@".\Documents\Glossary.csv"))
            {
                oneHundredStrings.Add(line.Replace('|', ' '));
                if (oneHundredStrings.Count > 99)
                    break;
            }
            return oneHundredStrings.ToArray();
        }
    }
}
