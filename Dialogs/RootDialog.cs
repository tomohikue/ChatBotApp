using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;
using System.Configuration;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Bot.ChatBotApp { 
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private string[] questions = new string[0];
        private string[] answers = new string[0];
        private int QnAConfidenceThreshold = Convert.ToInt16(ConfigurationManager.AppSettings["QnAConfidenceThreshold"]);

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.GetinputText);
        }
        private async Task GetinputText(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            await context.PostAsync($"FAQチャットボットです。皆様の業務のお手伝いができるように、勉強を始めました。");
            PromptDialog.Text(context, MessageReceivedAsync, $"ご質問を1文でどうぞ！");
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;

            await context.PostAsync($"ちょっと調べてみますね！");

            // QnA Service からデータを取得する
            QnAMService QnAMService = new QnAMService();
            QnAModel qnaRes = await QnAMService.GetQnA(message,"tomohiku","");

            if (qnaRes.answers[0].id != -1) {
                for (int x = 0; x < qnaRes.answers.Length; ++x)
                {
                    if (qnaRes.answers[x].score > QnAConfidenceThreshold) {

                        Array.Resize(ref questions, x + 1);

                        // metadataに入っている「rep」キーのテキストを代表質問として扱う
                        foreach (var i in qnaRes.answers[x].metadata)
                        {
                            if (i.name == "rep")
                            {
                                questions[x] = i.value;
                            }
                        }
                        // metadataに該当のキーが無かったら、最初の質問を代表質問として扱う
                        if (string.IsNullOrEmpty(questions[x]))
                        {
                            questions[x] = qnaRes.answers[x].questions[0];
                        }
                        Array.Resize(ref answers, x + 1);
                        answers[x] = qnaRes.answers[x].answer;
                    }
                }

                if (questions.Length >= 2)
                {
                    // await context.PostAsync($"■代表質問■\n" + questions[0] + "\n■回答■\n" + answers[0]);
                    PromptDialog.Choice(context, this.ShowAnswerAsync, questions, $"この中にご質問はありますか？");
                }
                else if (questions.Length == 1)
                {
                    await context.PostAsync($"■代表質問■\n" + questions[0] + "\n■回答■\n" + answers[0] );
                    PromptDialog.Choice(context, this.SaveyAsync, ShowAnswerAsyncText, $"お役に立ちましたでしょうか？");
                }
            }
            else
            {
                PromptDialog.Text(context, MessageReceivedAsync, $"申し訳ありません。適した答えが見つかりませんでした。再度ご質問をどうぞ！");
            }
        }

        List<string> ShowAnswerAsyncText = new List<string>() {
                "はい"
                ,"いいえ"
            };

        private async Task ShowAnswerAsync(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;

            for (int x = 0; x < questions.Length; ++x)
            {
                if (message == questions[x])
                {
                    await context.PostAsync($"■代表質問■\n" + questions[x] + "\n■回答■\n" + answers[x]);
                }
            }

            PromptDialog.Choice(context, this.SaveyAsync, ShowAnswerAsyncText, $"お役に立ちましたでしょうか？");
        }
        private async Task SaveyAsync(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;

            await context.PostAsync($"フィードバック頂き、ありがとうございました。");
        }
    }
}