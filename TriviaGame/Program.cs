using System;

namespace TriviaGameClient
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new TriviaGame())
                game.Run();
        }
    }
}
