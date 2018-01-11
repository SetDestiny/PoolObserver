using Newtonsoft.Json;
using PoolObserver.Common.Constants;
using PoolObserver.Common.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PoolObserver.Bot.Managers
{
    public class PoolManager
    {
        private TelegramBotClient telegramBotClient;
        private MessageManager messageManager;
        private HttpClient client;
        private string miner;


        public PoolManager(TelegramBotClient telegramBotClient, string miner)
        {
            this.miner = miner;
            this.telegramBotClient = telegramBotClient;
            this.messageManager = new MessageManager(this.telegramBotClient);
            client = new HttpClient();
            client.BaseAddress = new Uri(Links.FlyPool);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<PoolStat> GetPoolStat()
        {
            PoolStat poolStat = null;

            var path = string.Format("miner/{0}/currentStats", miner);
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(path))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        poolStat = JsonConvert.DeserializeObject<PoolStat>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return poolStat;
        }

        public async Task<Workers> GetWorkersStat()
        {
            Workers workers = null;

            var path = string.Format("miner/{0}/workers", miner);
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(path))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        workers = JsonConvert.DeserializeObject<Workers>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return workers;
        }

        public MinerStatus GetMinerStatus(PoolStat poolStat)
        {
            if (poolStat.Status != "OK")
            {
                return MinerStatus.InternalServerError;
            }
            else
            {
                if (poolStat.Data.CurrentHashrate < poolStat.Data.AverageHashrate)
                {
                    return MinerStatus.PowerDecreasing;
                }
                if (poolStat.Data.CurrentHashrate == 0 || poolStat.Data.ActiveWorkers == 0)
                {
                    return MinerStatus.Idling;
                }
                if (!poolStat.Data.CurrentHashrate.HasValue || !poolStat.Data.ActiveWorkers.HasValue)
                {
                    return MinerStatus.Idling;
                }
                return MinerStatus.Working;
            }
        }
    }
}
