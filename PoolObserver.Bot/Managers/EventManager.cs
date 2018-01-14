
using PoolObserver.Common.Constants;
using PoolObserver.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoolObserver.Bot.Managers
{
    class EventManager
    {
        private TelegramBotClient telegramBotClient;
        private MessageManager messageManager;
        private BackgroundWorker backgroundWorker;
        private List<Subscriber> subs;
        private bool expectToken;

        public EventManager(TelegramBotClient telegramBotClient)
        {
            this.telegramBotClient = telegramBotClient;
            this.messageManager = new MessageManager(this.telegramBotClient);
            this.backgroundWorker = new BackgroundWorker();
            this.subs = new List<Subscriber>();
        }

        public void ResolveUpdates(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.MessageUpdate:
                    {
                        this.ResolveMessageUpdate(update);
                        break;
                    }
                default:
                    {
                        messageManager.SendTextMessage(update.Message, "I don't understand you! Try again in another way bro...");
                        break;
                    }
            }
        }

        private void ResolveMessageUpdate(Update update)
        {
            switch (update.Message.Type)
            {
                case MessageType.TextMessage:
                    {
                        if (this.expectToken)
                        {
                            this.ObserveMiner(update);
                            this.expectToken = false;
                            return;
                        }

                        if (update.Message.Text[0] == '/')
                        {
                            LoggingManager.LogEvent("Trying to resolve received command...", LogType.Event);
                            var command = GetCommandType(update.Message.Text);
                            if (command == Command.Subscribe)
                            {
                                this.expectToken = true;
                                messageManager.SendTextMessage(update.Message, "Please, provide your wallet...");
                                return;
                            }

                            ResolveCommand(command, update);
                        }
                        else
                        {
                            messageManager.Reply(update.Message);
                        }
                        break;
                    }
                case MessageType.PhotoMessage:
                    {
                        //messageManager.ReplyProfile(update.Message);
                        break;
                    }
                default:
                    {
                        messageManager.SendTextMessage(update.Message, "I don't understand you! Try again in another way bro...");
                        break;
                    }
            }
        }
        private async void ResolveCommand(Command command, Update update)
        {
            switch (command)
            {
                case Command.ObserveMiner:
                    {
                        //this.ObserveMiner(update);
                        break;
                    }
                case Command.GetCurrentHashrate:
                    {
                        var matches = subs.Where(i => (i.Update as Update).Message.Chat.Id == update.Message.Chat.Id).ToList();

                        if (matches.Count > 0)
                        {
                            foreach (var sub in matches)
                            {
                                Workers workers = await (sub.PoolManager as PoolManager).GetWorkersStat();

                                foreach (var item in workers.Data)
                                {
                                    messageManager.SendTextMessage(update.Message, string.Format("Worker :{0}\n-current hashrate:{1}", item.Worker, item.CurrentHashrate));
                                }
                            }
                        }
                        else
                        {
                            messageManager.SendTextMessage(update.Message, "You don't have any subscription!");
                        }
                        break;
                    }
                case Command.ChatStart:
                    {
                        messageManager.SendTextMessage(update.Message, "Hi bro, i can help you to track your mining activity, please welcome!");
                        break;
                    }
                case Command.GetProfile:
                    {
                        messageManager.SendTextMessage(update.Message, "Hi, now send me photo to perform search!");
                        break;
                    }
                default:
                    {
                        messageManager.SendTextMessage(update.Message, "I don't understand you! Try again in another way bro...");
                        break;
                    }
            }
        }

        private Command GetCommandType(string command)
        {
            foreach (var prop in typeof(Commands).GetFields())
            {
                try
                {
                    if (command.Contains(prop.GetRawConstantValue().ToString()))
                    {
                        return (Command)Enum.Parse(typeof(Command), prop.Name);
                    }
                }
                catch
                {
                    return Command.Unknown;
                }
            }
            return Command.Unknown;
        }

        private void ObserveMiner(Update update)
        {
            try
            {
                string miner = update.Message.Text;
                if (miner.Length != 35 && miner.Length != 40)
                {
                    LoggingManager.LogEvent(string.Format("Failed to subscribe cash ***{0}", miner.Substring((int)(miner.Length / 1.5)).ToLower()), LogType.Error);
                    messageManager.SendTextMessage(update.Message, "It seems like you provide invalid token...");
                    return;
                }

                var matches = subs.Where(i => i.TokenId == miner).ToList();

                if (matches.Count > 0)
                {
                    LoggingManager.LogEvent(string.Format("Already subscribed on cash ***{0}", miner.Substring((int)(miner.Length / 1.5)).ToLower()), LogType.Warning);
                    messageManager.SendTextMessage(update.Message, string.Format("Already subscribed on cash ***{0} with status:{1}", miner.Substring((int)(miner.Length / 1.5)).ToLower(), matches.FirstOrDefault().Observer.Status));
                    return;
                }


                Subscriber sub = new Subscriber();
                sub.TokenId = miner;
                sub.PoolManager = new PoolManager(telegramBotClient, miner);
                sub.Update = update;

                sub.Observer = new Task(async () =>
                {
                    bool decreasing = false;
                    LoggingManager.LogEvent(string.Format("Trying to subscribe cash ***{0}", miner.Substring((int)(miner.Length / 1.5)).ToLower()), LogType.Event);
                    LoggingManager.LogEvent(string.Format("Subscribed on cash *{0}", miner.Substring((int)(miner.Length - 4)).ToLower()), LogType.Success);
                    messageManager.SendTextMessage((sub.Update as Update).Message, string.Format("Subscribed on cash ***{0}", miner.Substring((int)(miner.Length / 1.5)).ToLower()));

                    while (true)
                    {
                        var poolStat = await (sub.PoolManager as PoolManager).GetPoolStat();
                        var poolStatus = (sub.PoolManager as PoolManager).GetMinerStatus(poolStat);
                        var margin = new String(' ', 23);
                        var border = new String('-', 50);
                        var logText = string.Format("{0}\n{1}Process recived data from *{2}" +
                                                    "\n{3}-current hashrate: {4:0.00}" +
                                                    "\n{5}-active workers: {6}",
                            border,
                            margin,
                            miner.Substring((int)(miner.Length - 4)).ToLower(),
                            margin,
                            poolStat.Data.CurrentHashrate,
                            margin,
                            poolStat.Data.ActiveWorkers);

                        if (poolStatus == MinerStatus.PowerDecreasing && !decreasing)
                        {
                            decreasing = true;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(60000);
                            continue;
                        }
                        LoggingManager.LogEvent(logText, LogType.Event);
                        messageManager.NotifyMinerObserver((sub.Update as Update).Message, poolStatus, poolStat);

                        System.Threading.Thread.Sleep(60000);
                    }

                });

                subs.Add(sub);
                sub.Observer.Start();


            }
            catch
            {

                messageManager.NotifyMinerObserver(update.Message, MinerStatus.WrongToken);
            }
        }
    }
}
