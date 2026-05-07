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
            var sortieTask = FetchAsync<SortieModel>("pc/sortie", TimeSpan.FromMinutes(10));
            var nightwaveTask = FetchAsync<NightwaveModel>("pc/nightwave", TimeSpan.FromMinutes(10));
            var fissuresTask = FetchAsync<List<FissureModel>>("pc/fissures", TimeSpan.FromMinutes(5));
            var voidTraderTask = FetchAsync<VoidTraderModel>("pc/voidTrader", TimeSpan.FromMinutes(30));
            var dailyDealsTask = FetchAsync<List<DailyDealModel>>("pc/dailyDeals", TimeSpan.FromMinutes(10));
            var invasionsTask = FetchAsync<List<InvasionModel>>("pc/invasions", TimeSpan.FromMinutes(5));
            var cetusCycleTask = FetchAsync<CetusCycleModel>("pc/cetusCycle", TimeSpan.FromMinutes(2));
            var vallisCycleTask = FetchAsync<VallisCycleModel>("pc/vallisCycle", TimeSpan.FromMinutes(2));

            await Task.WhenAll(sortieTask, nightwaveTask, fissuresTask, voidTraderTask, dailyDealsTask, invasionsTask, cetusCycleTask, vallisCycleTask);

            var sortie = await sortieTask;
            var nightwave = await nightwaveTask;
            var fissures = await fissuresTask ?? new List<FissureModel>();
            var voidTrader = await voidTraderTask;
            var dailyDeals = await dailyDealsTask ?? new List<DailyDealModel>();
            var invasions = await invasionsTask ?? new List<InvasionModel>();
            var cetusCycle = await cetusCycleTask;
            var vallisCycle = await vallisCycleTask;

            System.Diagnostics.Debug.WriteLine($"[WarframeService] Dashboard loaded - Fissures count: {fissures?.Count ?? 0}");

            return new DashboardViewModel
            {
                Sortie = sortie,
                Nightwave = nightwave,
                Fissures = fissures,
                VoidTrader = voidTrader,
                DailyDeals = dailyDeals,
                Invasions = invasions,
                CetusCycle = cetusCycle,
                VallisCycle = vallisCycle,
            };
        }

        private static async Task<T> FetchAsync<T>(string endpoint, TimeSpan cacheDuration) where T : class
        {
            lock (_cache)
            {
                if (_cache.TryGetValue(endpoint, out var cached) && cached.Expiry > DateTime.UtcNow)
                {
                    System.Diagnostics.Debug.WriteLine($"[WarframeService] Returning cached data for {endpoint}");
                    return (T)cached.Data;
                }
            }

            try
            {
                var url = $"{endpoint}?language=en";
                System.Diagnostics.Debug.WriteLine($"[WarframeService] Fetching {url}");
                
                var json = await _client.GetStringAsync(url);
                
                // Debug log raw JSON for invasions
                if (endpoint == "pc/invasions")
                {
                    System.Diagnostics.Debug.WriteLine($"[WarframeService] Raw invasions JSON: {json.Substring(0, Math.Min(500, json.Length))}...");
                }
                
                var data = JsonConvert.DeserializeObject<T>(json);
                
                lock (_cache)
                {
                    _cache[endpoint] = (data, DateTime.UtcNow.Add(cacheDuration));
                }
                
                System.Diagnostics.Debug.WriteLine($"[WarframeService] Successfully fetched {endpoint}");
                return data;
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"[WarframeService] HTTP Error fetching {endpoint}: {httpEx.Message}");
                return null;
            }
            catch (JsonSerializationException jsonEx)
            {
                System.Diagnostics.Debug.WriteLine($"[WarframeService] JSON Parse Error for {endpoint}: {jsonEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WarframeService] Unexpected error fetching {endpoint}: {ex.Message}");
                return null;
            }
        }
    }
}
