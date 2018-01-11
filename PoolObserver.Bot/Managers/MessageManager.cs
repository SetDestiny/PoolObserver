using PoolObserver.Common.Constants;
using PoolObserver.Common.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PoolObserver.Bot.Managers
{
    class MessageManager
    {
        private TelegramBotClient telegramBotClient;

        public MessageManager(TelegramBotClient telegramBotClient)
        {
            this.telegramBotClient = telegramBotClient;
        }

        public async void Reply(Message message)
        {
            if (message.Text.Contains("/get_profile"))
            {
                await telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Please provide photo and i'll find this person...");
            }
        }

        HttpWebRequest GetNewRequest(string targetUrl)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(targetUrl);
            request.AllowAutoRedirect = false;
            return request;
        }

        public void NotifyMinerObserver(Message message, MinerStatus status = MinerStatus.Working, PoolStat poolStat = null)
        {
            switch (status)
            {
                case MinerStatus.Idling:
                    {
                        var text = string.Format("No worker detected! Please check your environment...\n-current hashrate: {0:0.00}\n-active workers: {1}", poolStat.Data.CurrentHashrate, poolStat.Data.ActiveWorkers);
                        telegramBotClient.SendTextMessageAsync(message.Chat.Id, text);
                        break;
                    }
                case MinerStatus.PowerDecreasing:
                    {
                        var text = string.Format("Power decreasing, please check your environment...\n-current hashrate: {0:0.00}\n-active workers: {1}", poolStat.Data.CurrentHashrate, poolStat.Data.ActiveWorkers);
                        telegramBotClient.SendTextMessageAsync(message.Chat.Id, text);
                        break;
                    }
                case MinerStatus.InternalServerError:
                    {
                        telegramBotClient.SendTextMessageAsync(message.Chat.Id, "No reply from flypool! Please check your environment and pool...");
                        break;
                    }
                case MinerStatus.WrongToken:
                    {
                        telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Please, provide valid token...");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public async void SendTextMessage(Message message, string text)
        {
            if (message != null)
            {
                await telegramBotClient.SendTextMessageAsync(message.Chat.Id, text);
            }
        }
    }
}
