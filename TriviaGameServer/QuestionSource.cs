using System;
using System.Collections.Generic;
using System.Text;
using TriviaGameProtocol;

namespace TriviaGameServer
{
    public struct Question
    {
        public TriviaQuestion question;
        public char answer;
    }
    public interface QuestionSource
    {
        public Question GetQuestion(string category);
    }
}
