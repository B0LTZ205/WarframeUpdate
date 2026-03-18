using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using WarframeTracker.Models;

namespace WarframeTracker.Services
{
    public class WarframeService
    {
        private static readonly HttpClient _client;
        private static readonly string BaseUrl = "https://api.warframestat.us/";

        // Simple in-memory cache: key -> (data, expiry)
        private static readonly Dictionary<string, (object Data, DateTime Expiry)> _cache
            = new Dictionary<string, (object, DateTime)>();

        static WarframeService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(BaseUrl);
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.Timeout = TimeSpan.FromSeconds(15);
        }

        public async Task<DashboardViewModel> GetDashboardAsync()
        {
            var tasks = new Task[]
            {
                FetchAsync<SortieModel>("pc/sortie", TimeSpan.FromMinutes(10)),
                FetchAsync<NightwaveModel>("pc/nightwave", TimeSpan.FromMinutes(10)),
                FetchAsync<List<FissureModel>>("pc/fissures", TimeSpan.FromMinutes(5)),
                FetchAsync<VoidTraderModel>("pc/voidTrader", TimeSpan.FromMinutes(30)),
                FetchAsync<List<DailyDealModel>>("pc/dailyDeals", TimeSpan.FromMinutes(10)),
                FetchAsync<List<InvasionModel>>("pc/invasions", TimeSpan.FromMinutes(5)),
                FetchAsync<CetusCycleModel>("pc/cetusCycle", TimeSpan.FromMinutes(2)),
                FetchAsync<VallisCycleModel>("pc/vallisCycle", TimeSpan.FromMinutes(2))
            };

            await Task.WhenAll(tasks);

            return new DashboardViewModel
            {
                Sortie      = ((Task<SortieModel>)tasks[0]).Result,
                Nightwave   = ((Task<NightwaveModel>)tasks[1]).Result,
                Fissures    = ((Task<List<FissureModel>>)tasks[2]).Result,
                VoidTrader  = ((Task<VoidTraderModel>)tasks[3]).Result,
                DailyDeals  = ((Task<List<DailyDealModel>>)tasks[4]).Result,
                Invasions   = ((Task<List<InvasionModel>>)tasks[5]).Result,
                CetusCycle  = ((Task<CetusCycleModel>)tasks[6]).Result,
                VallisCycle = ((Task<VallisCycleModel>)tasks[7]).Result,
            };
        }

        private static async Task<T> FetchAsync<T>(string endpoint, TimeSpan cacheDuration) where T : class
        {
            lock (_cache)
            {
                if (_cache.TryGetValue(endpoint, out var cached) && cached.Expiry > DateTime.UtcNow)
                    return (T)cached.Data;
            }

            try
            {
                var json = await _client.GetStringAsync($"{endpoint}?language=en");
                var data = JsonConvert.DeserializeObject<T>(json);
                lock (_cache)
                {
                    _cache[endpoint] = (data, DateTime.UtcNow.Add(cacheDuration));
                }
                return data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WarframeService] Failed to fetch {endpoint}: {ex.Message}");
                return null;
            }
        }
    }
}
