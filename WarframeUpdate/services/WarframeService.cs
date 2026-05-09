using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
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
            var fissuresTask = FetchAsync<List<FissureModel>>("pc/fissures", TimeSpan.FromSeconds(30));
            var voidTraderTask = FetchAsync<VoidTraderModel>("pc/voidTrader", TimeSpan.FromMinutes(30));
            var dailyDealsTask = FetchAsync<List<DailyDealModel>>("pc/dailyDeals", TimeSpan.FromMinutes(10));
            var invasionsTask = FetchAsync<List<InvasionModel>>("pc/invasions", TimeSpan.FromMinutes(5));
            var cetusCycleTask = FetchAsync<CetusCycleModel>("pc/cetusCycle", TimeSpan.FromMinutes(2));
            var vallisCycleTask = FetchAsync<VallisCycleModel>("pc/vallisCycle", TimeSpan.FromMinutes(2));

            await Task.WhenAll(sortieTask, nightwaveTask, fissuresTask, voidTraderTask, dailyDealsTask, invasionsTask, cetusCycleTask, vallisCycleTask);

            var (sortie, sortieStale) = await sortieTask;
            var (nightwave, nightwaveStale) = await nightwaveTask;
            var (fissuresRaw, fissuresStale) = await fissuresTask;
            var (voidTrader, traderStale) = await voidTraderTask;
            var (dailyDeals, dealsStale) = await dailyDealsTask;
            var (invasions, invasionsStale) = await invasionsTask;
            var (cetusCycle, cetusStale) = await cetusCycleTask;
            var (vallisCycle, vallisStale) = await vallisCycleTask;

            var fissures = (fissuresRaw ?? new List<FissureModel>())
                .Where(f => f.Expiry > DateTime.UtcNow && !f.IsStorm)
                .OrderBy(f => f.Expiry)
                .ToList();

            bool apiOffline = sortieStale || nightwaveStale || fissuresStale || traderStale
                           || dealsStale || invasionsStale || cetusStale || vallisStale;

            System.Diagnostics.Debug.WriteLine($"[WarframeService] Dashboard loaded - ApiOffline: {apiOffline}, Fissures: {fissures.Count}");

            return new DashboardViewModel
            {
                Sortie = sortie,
                Nightwave = nightwave,
                Fissures = fissures,
                VoidTrader = voidTrader,
                DailyDeals = dailyDeals ?? new List<DailyDealModel>(),
                Invasions = invasions ?? new List<InvasionModel>(),
                CetusCycle = cetusCycle,
                VallisCycle = vallisCycle,
                ApiOffline = apiOffline,
            };
        }


        private static async Task<(T Data, bool WasStale)> FetchAsync<T>(string endpoint, TimeSpan cacheDuration) where T : class
        {
            T stale = null;
            lock (_cache)
            {
                if (_cache.TryGetValue(endpoint, out var cached))
                {
                    if (cached.Expiry > DateTime.UtcNow)
                    {
                        System.Diagnostics.Debug.WriteLine($"[WarframeService] Returning cached data for {endpoint}");
                        return ((T)cached.Data, false);
                    }
                    stale = (T)cached.Data;
                }
            }

            try
            {
                var url = $"{endpoint}?language=en";
                System.Diagnostics.Debug.WriteLine($"[WarframeService] Fetching {url}");

                var json = await _client.GetStringAsync(url);

                if (endpoint == "pc/invasions")
                    System.Diagnostics.Debug.WriteLine($"[WarframeService] Raw invasions JSON: {json.Substring(0, Math.Min(500, json.Length))}...");

                var data = JsonConvert.DeserializeObject<T>(json);

                lock (_cache)
                {
                    _cache[endpoint] = (data, DateTime.UtcNow.Add(cacheDuration));
                }

                System.Diagnostics.Debug.WriteLine($"[WarframeService] Successfully fetched {endpoint}");
                return (data, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WarframeService] Error fetching {endpoint}: {ex.Message}");
                if (stale != null)
                    System.Diagnostics.Debug.WriteLine($"[WarframeService] Falling back to stale cache for {endpoint}");
                return (stale, stale != null);
            }
        }

    }
}
