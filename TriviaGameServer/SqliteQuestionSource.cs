using System;
using System.Collections.Generic;
using System.Text;
using TriviaGameProtocol;
using Microsoft.Data.Sqlite;

namespace TriviaGameServer
{
    public class SqliteQuestionSource : QuestionSource
    {
        public Question GetQuestion(string category)
        {

            List<string> message = setUpConnToDatabase(category);

            string q = message[0];
            string opA = message[2];
            string opB = message[3];
            string opC = message[4];
            string opD = message[5];
            TriviaQuestion Q = new TriviaQuestion();
            Q.question = q;
            Q.optionA = opA;
            Q.optionB = opB;
            Q.optionC = opC;
            Q.optionD = opD;

            Question res;
            res.question = Q;
            res.answer = char.Parse(message[1]);
            return res;
        }

        private List<string> setUpConnToDatabase(string category)
        {
            List<string> message = new List<string>();

            // using Microsoft.Data.Sqlite
            using (var connection = new SqliteConnection("Data Source=../../../../TriviaGame.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                // got rid of questionID
                command.CommandText =
                @"
                SELECT questionDescription, correctAnswer, optionA, optionB, optionC, optionD
                FROM Question
                WHERE category = $catCard  
                ORDER BY RANDOM()
                LIMIT 1
                ";
                command.Parameters.AddWithValue("$catCard", category);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string result;

                        for (int i = 0; i < 6; i++)
                        {
                            result = reader.GetString(i);
                            message.Add(result);
                        }
                    }
                }
            }
            return message;
        }

    }
}
