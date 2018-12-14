using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;
using System.Linq;

namespace Microsoft.Bot.ChatBotApp
{
    /// <summary>
    /// QnA Maker Service
    /// </summary>
    public class QnAMService
    {
        string qnaKBId = ConfigurationManager.AppSettings["QnAKnowledgebaseId"];
        string endpointHostName = ConfigurationManager.AppSettings["QnAEndpointHostName"];
        string qnaAuthKey = ConfigurationManager.AppSettings["QnAAuthKey"];
        string QnAGetCnt = ConfigurationManager.AppSettings["QnAGetCnt"];

        /// <summary>
        /// QnA Makerに問い合わせし、回答を取得するFunction
        /// </summary>
        public async Task<QnAModel> GetQnA(string message, string UserId, string FilterStirng)
        {
            // AppInsightの定義
            TelemetryClient telemetry = new TelemetryClient();
            telemetry.Context.User.Id = UserId;

            var methodname = "/knowledgebases/" + qnaKBId + "/generateAnswer/";
            var uri = endpointHostName + methodname;

            string option = "";
            if(!string.IsNullOrEmpty(UserId))
            {
                option = ",'userId': '" + UserId + "'";
            }
            if (!string.IsNullOrEmpty(FilterStirng))
            {
                option = ",'strictFilters': [{'name': 'category','value': '" + FilterStirng + "'}]";
            }

            var question = @"{'question': '" + message + @"','top': " + QnAGetCnt + option + " }";

            //private QnAModel qnaRes;
            var qnaRes = new QnAModel();
            try
            {
                var resjson = await Post(uri, question);

                qnaRes = JsonConvert.DeserializeObject<QnAModel>(resjson);

                // App Insightにログをセットするためのコード
                string TraceMessage = "QnAMaker Response";
                var properties = new Dictionary<string, string> { };
                properties.Add("InputText", message);
                for (int x = 0; x < qnaRes.answers.Length; ++x)
                {
                    string questions = "";
                    foreach (var i in qnaRes.answers[x].questions)
                    {
                        if (questions == "")
                        {
                            questions = i;
                        }
                        else
                        {
                            questions = questions + ";" + i;
                        }
                    }
                    properties.Add("questions_" + x, questions);

                    properties.Add("answer_" + x, qnaRes.answers[x].answer);
                    properties.Add("score_" + x, Convert.ToString(qnaRes.answers[x].score));
                    properties.Add("id_" + x, Convert.ToString(qnaRes.answers[x].id));
                    properties.Add("source_" + x, qnaRes.answers[x].source);

                    // metadataに入っている「rep」キーのテキストを代表質問として扱う
                    foreach (var i in qnaRes.answers[x].metadata)
                    {
                        properties.Add(i.name + x, i.value);
                    }
                }
                //telemetry.TrackEvent("QnA Maker Result", properties);
                telemetry.TrackTrace(TraceMessage, SeverityLevel.Information, properties);
            }
            catch(Exception ex)
            {
                telemetry.TrackException(ex);
            }
            return qnaRes;
        }

        public async Task<string> Post(string uri, string body)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                request.Headers.Add("Authorization", "EndpointKey " + qnaAuthKey);

                var response = await client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}