using PoolObserver.Common.Constants;
using System;
using System.ComponentModel;
using Telegram.Bot;

namespace PoolObserver.Bot.Managers
{
    public sealed class BotManager
    {
        private BackgroundWorker backgroundWorker;
        private EventManager eventManager;
        private string token;

        public BotManager(string token)
        {
            this.backgroundWorker = new BackgroundWorker();
            this.token = token;
        }

        public void Start()
        {
            LoggingManager.LogEvent(LogTemplates.BotInit, LogType.Event);
            try
            {
                this.backgroundWorker.DoWork += this.Observe;

                if (this.backgroundWorker.IsBusy != true)
                {
                    this.backgroundWorker.RunWorkerAsync();
                }
                LoggingManager.LogEvent(LogTemplates.BotInitSuccess, LogType.Success);
            }
            catch (Exception ex)
            {
                LoggingManager.LogEvent(string.Format(LogTemplates.BotInitFail, ex.Message), LogType.Error);
            }
        }

        private async void Observe(object sender, DoWorkEventArgs e)
        {
            try
            {
                var telegramBotClient = new TelegramBotClient(this.token);
                this.eventManager = new EventManager(telegramBotClient);
                await telegramBotClient.SetWebhookAsync(string.Empty);
                int offset = 0;

                while (true)
                {
                    var updates = await telegramBotClient.GetUpdatesAsync(offset);

                    foreach (var update in updates)
                    {
                        this.eventManager.ResolveUpdates(update);
                        offset = update.Id + 1;
                    }
                }
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                LoggingManager.LogEvent(string.Format(LogTemplates.ReceiveUpdateFail, ex.Message), LogType.Error);
            }
        }
    }
}
