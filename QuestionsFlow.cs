using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestEchoBot.Bots
{
    public class QuestionsFlow
    {
        public enum Question
        {
            Name,
            Age,
            Date,
            None, 
        }

        public Question LastQuestionAsked { get; set; } = Question.None;
    }
}
