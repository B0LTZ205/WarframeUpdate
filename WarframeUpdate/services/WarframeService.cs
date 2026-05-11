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

        // ── Incarnon Genesis Rotation ──────────────────────────
        // Resets every Sunday 00:00 UTC. Cycles through sets in order.
        // Reference: Sunday 2024-01-07 00:00 UTC = Set 1.
        // If the current set looks wrong in-game, adjust the reference date by ±1 week.
        private static readonly DateTime IncarnOnReferenceDate =
            new DateTime(2024, 1, 7, 0, 0, 0, DateTimeKind.Utc);

        private static readonly List<List<IncarnOnWeapon>> IncarnOnSets =
            new List<List<IncarnOnWeapon>>
        {
            new List<IncarnOnWeapon> // Week 1 (A)
            {
                new IncarnOnWeapon { Name = "Braton", Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Lato",   Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Skana",  Type = WeaponType.Melee     },
                new IncarnOnWeapon { Name = "Paris",  Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Kunai",  Type = WeaponType.Secondary },
            },
            new List<IncarnOnWeapon> // Week 2 (B)
            {
                new IncarnOnWeapon { Name = "Boar",     Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Gammacor", Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Angstrum", Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Gorgon",   Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Anku",     Type = WeaponType.Melee     },
            },
            new List<IncarnOnWeapon> // Week 3 (C)
            {
                new IncarnOnWeapon { Name = "Bo",     Type = WeaponType.Melee     },
                new IncarnOnWeapon { Name = "Latron", Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Furis",  Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Furax",  Type = WeaponType.Melee     },
                new IncarnOnWeapon { Name = "Strun",  Type = WeaponType.Primary   },
            },
            new List<IncarnOnWeapon> // Week 4 (D)
            {
                new IncarnOnWeapon { Name = "Lex",            Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Magistar",       Type = WeaponType.Melee     },
                new IncarnOnWeapon { Name = "Boltor",         Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Bronco",         Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Ceramic Dagger", Type = WeaponType.Melee     },
            },
            new List<IncarnOnWeapon> // Week 5 (E)
            {
                new IncarnOnWeapon { Name = "Torid",         Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Dual Toxocyst", Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Dual Ichor",    Type = WeaponType.Melee     },
                new IncarnOnWeapon { Name = "Miter",         Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Atomos",        Type = WeaponType.Secondary },
            },
            new List<IncarnOnWeapon> // Week 6 (F)
            {
                new IncarnOnWeapon { Name = "Ack & Brunt", Type = WeaponType.Melee     },
                new IncarnOnWeapon { Name = "Soma",        Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Vasto",       Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Nami Solo",   Type = WeaponType.Melee     },
                new IncarnOnWeapon { Name = "Burston",     Type = WeaponType.Primary   },
            },
            new List<IncarnOnWeapon> // Week 7 (G)
            {
                new IncarnOnWeapon { Name = "Zylok",  Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Sibear",  Type = WeaponType.Melee     },
                new IncarnOnWeapon { Name = "Dread",   Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Despair", Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Hate",    Type = WeaponType.Melee     },
            },
            new List<IncarnOnWeapon> // Week 8 (H)
            {
                new IncarnOnWeapon { Name = "Dera",    Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Sybaris", Type = WeaponType.Primary   },
                new IncarnOnWeapon { Name = "Cestra",  Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Sicarus", Type = WeaponType.Secondary },
                new IncarnOnWeapon { Name = "Okina",   Type = WeaponType.Melee     },
            },
        };

        public static IncarnOnWeekModel GetCurrentIncarnOn()
        {
            var now = DateTime.UtcNow;
            var weeksSinceRef = (int)Math.Floor((now - IncarnOnReferenceDate).TotalDays / 7.0);
            var setIndex = ((weeksSinceRef % IncarnOnSets.Count) + IncarnOnSets.Count) % IncarnOnSets.Count;

            // Next Sunday 00:00 UTC
            var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilSunday == 0) daysUntilSunday = 7;
            var nextReset = now.Date.AddDays(daysUntilSunday);

            return new IncarnOnWeekModel
            {
                SetNumber  = setIndex + 1,
                TotalSets  = IncarnOnSets.Count,
                Weapons    = IncarnOnSets[setIndex],
                NextReset  = nextReset,
            };
        }

        public async Task<DashboardViewModel> GetDashboardAsync()
        {
            var sortieTask       = FetchAsync<SortieModel>("pc/sortie", TimeSpan.FromMinutes(10));
            var nightwaveTask    = FetchAsync<NightwaveModel>("pc/nightwave", TimeSpan.FromMinutes(10));
            var fissuresTask     = FetchAsync<List<FissureModel>>("pc/fissures", TimeSpan.FromSeconds(30));
            var voidTraderTask   = FetchAsync<VoidTraderModel>("pc/voidTrader", TimeSpan.FromMinutes(30));
            var dailyDealsTask   = FetchAsync<List<DailyDealModel>>("pc/dailyDeals", TimeSpan.FromMinutes(10));
            var invasionsTask    = FetchAsync<List<InvasionModel>>("pc/invasions", TimeSpan.FromMinutes(5));
            var cetusCycleTask   = FetchAsync<CetusCycleModel>("pc/cetusCycle", TimeSpan.FromMinutes(2));
            var vallisCycleTask  = FetchAsync<VallisCycleModel>("pc/vallisCycle", TimeSpan.FromMinutes(2));
            var arbitrationTask  = FetchAsync<ArbitrationModel>("pc/arbitration", TimeSpan.FromMinutes(1));
            var archonHuntTask   = FetchAsync<ArchonHuntModel>("pc/archonHunt", TimeSpan.FromMinutes(30));

            await Task.WhenAll(sortieTask, nightwaveTask, fissuresTask, voidTraderTask,
                               dailyDealsTask, invasionsTask, cetusCycleTask, vallisCycleTask,
                               arbitrationTask, archonHuntTask);

            var (sortie, sortieStale)           = await sortieTask;
            var (nightwave, nightwaveStale)     = await nightwaveTask;
            var (fissuresRaw, fissuresStale)    = await fissuresTask;
            var (voidTrader, traderStale)       = await voidTraderTask;
            var (dailyDeals, dealsStale)        = await dailyDealsTask;
            var (invasions, invasionsStale)     = await invasionsTask;
            var (cetusCycle, cetusStale)        = await cetusCycleTask;
            var (vallisCycle, vallisStale)      = await vallisCycleTask;
            var (arbitration, arbitrationStale) = await arbitrationTask;
            var (archonHunt, archonHuntStale)   = await archonHuntTask;

            var fissures = (fissuresRaw ?? new List<FissureModel>())
                .Where(f => f.Expiry > DateTime.UtcNow && !f.IsStorm)
                .OrderBy(f => f.Expiry)
                .ToList();

            // Log arbitration fields so we can see exactly what the API returned
            if (arbitration != null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[WarframeService] Arbitration fields — Node: '{arbitration.Node}' | " +
                    $"Type: '{arbitration.Type}' | Enemy: '{arbitration.Enemy}' | " +
                    $"Activation: '{arbitration.Activation}' | Expiry: '{arbitration.Expiry}'");
            }

            // Discard only if BOTH node AND type look like transitional placeholder data.
            bool badNode = string.IsNullOrWhiteSpace(arbitration?.Node) ||
                           string.Equals(arbitration?.Node, "SolNode000", StringComparison.OrdinalIgnoreCase);
            bool badType = string.IsNullOrWhiteSpace(arbitration?.Type) ||
                           string.Equals(arbitration?.Type, "Unknown", StringComparison.OrdinalIgnoreCase);

            if (arbitration != null && badNode && badType)
            {
                System.Diagnostics.Debug.WriteLine("[WarframeService] Discarding transitional arbitration data (bad node + bad type)");
                lock (_cache) { _cache.Remove("pc/arbitration"); }
                arbitration = null;
            }

            bool apiOffline = sortieStale || nightwaveStale || fissuresStale || traderStale
                           || dealsStale  || invasionsStale || cetusStale    || vallisStale
                           || archonHuntStale;

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
                IncarnOn      = GetCurrentIncarnOn(),
                Arbitration   = arbitration,
                ArchonHunt    = archonHunt,
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
