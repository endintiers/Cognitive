using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WildMouse.Unearth.Cognitive.TextAnalytics.Contract;

namespace WildMouse.Unearth.Cognitive.TextAnalytics
{
    public class TextAnalyticsClient
    {
        private string _accountKey;
        private string _baseUrl;
        private string _keyPhraseUri;
        private string _sentimentUri;
        private string _languagesUri;

        public TextAnalyticsClient(string accountKey,
            string baseUrl = @"https://westus.api.cognitive.microsoft.com/",
            string keyPhraseUri = @"text/analytics/v2.0/keyPhrases",
            string sentimentUri = @"text/analytics/v2.0/sentiment",
            string languagesUri = @"text/analytics/v2.0/languages")
        {
            _accountKey = accountKey;
            _baseUrl = baseUrl;
            _keyPhraseUri = keyPhraseUri;
            _sentimentUri = sentimentUri;
            _languagesUri = languagesUri;
        }

        public List<string> GetKeyPhrasesForText(string textToExamine, string language = "en")
        {
            return AsyncHelper.RunSync(() => GetKeyPhrasesForTextAsync(textToExamine, language));
        }

        public async Task<List<string>> GetKeyPhrasesForTextAsync(string textToExamine, string language = "en")
        {
            List<string> response;

            var request = new KeyPhrasesRequest();
            request.documents = new List<KeyPhrasesRequest.Document>();
            request.documents.Add(new KeyPhrasesRequest.Document()
            { id = "1", language = language, text = textToExamine });

            var resp = await GetKeyPhrasesAsync(request);

            if (resp.errors.Count > 0)
            {
                throw new ApplicationException("Error calling KeyPhrases: " + resp.errors[0].message);
            }

            if (resp.documents.Count < 1)
            {
                throw new ApplicationException("Error calling KeyPhrases: No document returned.");
            }

            response = resp.documents[0].keyPhrases;
            return response;
        }

        public async Task<KeyPhrasesResponse> GetKeyPhrasesAsync(KeyPhrasesRequest request)
        {
            KeyPhrasesResponse response = null; 

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _accountKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonRequest = JsonConvert.SerializeObject(request);
                byte[] byteData = Encoding.UTF8.GetBytes(jsonRequest);

                var jsResponse = await CallEndpoint(client, _keyPhraseUri, byteData);

                response = JsonConvert.DeserializeObject<KeyPhrasesResponse>(jsResponse);

                if (response == null || response.errors == null)
                {
                    // failed to convert - unexpected jsResponse
                    response = new KeyPhrasesResponse();
                    response.documents = new List<KeyPhrasesResponse.Document>();
                    var err = new KeyPhrasesResponse.Error() { id = "1", message = jsResponse };
                    response.errors = new List<KeyPhrasesResponse.Error>() { err };
                }

                if (response.errors.Count > 0)
                {
                    Trace.TraceWarning("Error calling text analytics key phrases: " + response.errors[0].message);
                }

            }
            return response;
        }

        public double GetSentimentForText(string textToExamine, string language = "en")
        {
            return AsyncHelper.RunSync(() => GetSentimentForTextAsync(textToExamine, language));
        }

        public async Task<double> GetSentimentForTextAsync(string textToExamine, string language = "en")
        {
            var request = new SentimentRequest() {documents = new List<SentimentRequest.Document>()};
            request.documents.Add(new SentimentRequest.Document()
                {language = language, id = "1", text = textToExamine});

            var response = await GetSentimentAsync(request);

            if (response.errors.Count > 0)
            {
                throw new ApplicationException("Error calling sentiment API " + response.errors[0].message);
            }

            if (response.documents.Count < 1)
            {
                throw new ApplicationException("No document returned by sentiment API");
            }

            var result = response.documents[0].score;
            return result;
        }


        public async Task<SentimentResponse> GetSentimentAsync(SentimentRequest request)
        {
            SentimentResponse response = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);

                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _accountKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonRequest = JsonConvert.SerializeObject(request);
                byte[] byteData = Encoding.UTF8.GetBytes(jsonRequest);

                var jsResponse = await CallEndpoint(client, _sentimentUri, byteData);

                response = JsonConvert.DeserializeObject<SentimentResponse>(jsResponse);

                if (response == null || response.errors == null)
                {
                    // failed to convert - unexpected jsResponse
                    response = new SentimentResponse();
                    response.documents = new List<SentimentResponse.Document>();
                    var err = new SentimentResponse.Error() { id = "1", message = jsResponse };
                    response.errors = new List<SentimentResponse.Error>() { err };
                }

                if (response.errors.Count > 0)
                {
                    Trace.TraceWarning("Error calling text analytics sentiment: " + response.errors[0].message);
                }

            }
            return response;
        }

        public string DetectLanguageForText(string textToExamine)
        {
            return AsyncHelper.RunSync(() => DetectLanguageForTextAsync(textToExamine));
        }

        public async Task<string> DetectLanguageForTextAsync(string textToExamine)
        {
            var request = new DetectLanguageRequest() { documents = new List<DetectLanguageRequest.Document>() };
            request.documents.Add(new DetectLanguageRequest.Document()
            { id = "1", text = textToExamine });

            var response = await DetectLanguagesAsync(request);

            if (response.errors.Count > 0)
            {
                throw new ApplicationException("Error calling languages API " + response.errors[0].message);
            }

            if (response.documents.Count < 1)
            {
                throw new ApplicationException("No document returned by languages API");
            }

            if (response.documents[0].detectedLanguages.Count < 1)
            {
                throw new ApplicationException("No languages detected by API");
            }

            var result = response.documents[0].detectedLanguages[0].iso6391Name;
            return result;
        }


        public async Task<DetectLanguageResponse> DetectLanguagesAsync(DetectLanguageRequest request)
        {
            DetectLanguageResponse response = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);

                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _accountKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonRequest = JsonConvert.SerializeObject(request);
                byte[] byteData = Encoding.UTF8.GetBytes(jsonRequest);

                var jsResponse = await CallEndpoint(client, _languagesUri, byteData);

                response = JsonConvert.DeserializeObject<DetectLanguageResponse>(jsResponse);

                if (response == null || response.errors == null)
                {
                    // failed to convert - unexpected jsResponse
                    response = new DetectLanguageResponse();
                    response.documents = new List<DetectLanguageResponse.Document>();
                    var err = new DetectLanguageResponse.Error() { id = "1", message = jsResponse };
                    response.errors = new List<DetectLanguageResponse.Error>() { err };
                }

                if (response.errors.Count > 0)
                {
                    Trace.TraceWarning("Error calling text analytics languages: " + response.errors[0].message);
                }
            }
            return response;
        }

        private async Task<String> CallEndpoint(HttpClient client, string uri, byte[] byteData)
        {
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
