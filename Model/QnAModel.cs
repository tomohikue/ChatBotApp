using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Bot.ChatBotApp
{
    public class QnAModel
    {
        public Answer[] answers { get; set; }
    }

    public class Answer
    {
        public string[] questions { get; set; }
        public string answer { get; set; }
        public float score { get; set; }
        public int id { get; set; }
        public string source { get; set; }
        public metadatas[] metadata { get; set; }
    }
    public class metadatas
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}