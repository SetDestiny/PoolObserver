using PoolObserver.Bot.Managers;
using System;
using System.Configuration;


namespace PoolObserver
{
    class Program
    {
        static void Main(string[] args)
        {
            var telegramBotToken = ConfigurationManager.AppSettings["TokenId"];
            BotManager botManager = new BotManager(telegramBotToken);
            botManager.Start();
            Console.Read();
        }
    }
}
