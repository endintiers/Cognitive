using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WildMouse.Unearth.Cognitive.TextAnalytics.Contract;

namespace WildMouse.Unearth.Cognitive.TextAnalytics
{
    public class TopicsClient
    {
        private string _accountKey;
        private string _baseUrl;
        private string _uri;
        private int _timeoutMinutes;
        private int _pollPeriodSec;

        public TopicsClient(string accountKey, int timeoutMinutes = 20, int pollPeriodSec = 60,
            string baseUrl = @"https://westus.api.cognitive.microsoft.com/",
            string uri = @"text/analytics/v2.0/topics")
        {
            _accountKey = accountKey;
            _baseUrl = baseUrl;
            _uri = uri;
            _timeoutMinutes = timeoutMinutes;
            _pollPeriodSec = pollPeriodSec;
        }

        /// <summary>
        /// Detect Topics (at least 100 documents)
        /// Documents should have a common theme. Random text will fail with "Internal error while executing BES operation."
        /// Runs batch, will take a while. Status shown in Debug Output every 60 seconds
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TopicsResponse> DetectTopicsAsync(TopicsRequest request)
        {
            TopicsResponse response = new TopicsResponse();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _accountKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonRequest = JsonConvert.SerializeObject(request);

                byte[] byteData = Encoding.UTF8.GetBytes(jsonRequest);

                // Start topic detection and get the URL we need to poll for results.
                var initialResult = await CallTopicEndpoint(client, _uri, byteData);
                if (initialResult.OperationLocation == string.Empty)
                {
                    // Manufacture an Error in the response contract
                    response.status = "FailedToStart";
                    var additional = string.Empty;
                    if (request.documents.Count < 100)
                    {
                        additional = ", Perhaps because No. of Documents is less than 100";
                    }
                    response.message = string.Format("HTTP Status Code: {0}{1}", initialResult.StatusCode, additional);
                }
                else
                {
                    var callbackUrl = initialResult.OperationLocation;
                    Trace.TraceInformation("Topic detection started. Polling for results at: " + callbackUrl);

                    // Poll the service until the job has completed.
                    var sw = new Stopwatch();
                    sw.Start();
                    while (sw.Elapsed.TotalMinutes < _timeoutMinutes)
                    {
                        var result = await GetTopicResult(client, callbackUrl);
                        if (result.IndexOf("\"status\":\"Succeeded\"", StringComparison.InvariantCulture) > 0)
                        {
                            Trace.TraceInformation("Topic detection succeeded. Result:\n" + result);
                            response = JsonConvert.DeserializeObject<TopicsResponse>(result);
                            break;
                        }
                        if (result.IndexOf("\"status\":\"Failed\"", StringComparison.InvariantCulture) > 0)
                        {
                            Trace.TraceInformation("Topic detection failed. Result:\n" + result);
                            response = JsonConvert.DeserializeObject<TopicsResponse>(result);
                            break;
                        }
                        else
                        {
                            Trace.TraceInformation(result);
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(_pollPeriodSec));
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// Call the Detect Topics endpoint to attempt to start a batch operation on the documents
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="byteData"></param>
        /// <returns>A result containing HTTP Status and polling URL</returns>
        private async Task<TopicsInitialResult> CallTopicEndpoint(HttpClient client, string uri, byte[] byteData)
        {
            var result = new TopicsInitialResult() { StatusCode = HttpStatusCode.PreconditionFailed, OperationLocation = string.Empty };
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(uri, content);
                result.StatusCode = response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    // Return URL containing OperationID to poll from.
                    result.OperationLocation = response.Headers.GetValues("Operation-Location").First();
                }
            }
            return result;
        }

        private async Task<string> GetTopicResult(HttpClient client, string uri)
        {
            var response = await client.GetAsync(uri);
            return await response.Content.ReadAsStringAsync();
        }

    }
}
