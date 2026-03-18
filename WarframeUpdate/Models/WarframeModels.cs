using System.Collections.Generic;
using Newtonsoft.Json;

namespace WarframeTracker.Models
{
    // ── Sortie ────────────────────────────────────────────
    public class SortieModel
    {
        [JsonProperty("faction")]
        public string Faction { get; set; }

        [JsonProperty("expiry")]
        public string Expiry { get; set; }

        [JsonProperty("variants")]
        public List<SortieMission> Variants { get; set; } = new List<SortieMission>();
    }

    public class SortieMission
    {
        [JsonProperty("missionType")]
        public string MissionType { get; set; }

        [JsonProperty("modifier")]
        public string Modifier { get; set; }

        [JsonProperty("node")]
        public string Node { get; set; }
    }

    // ── Nightwave ─────────────────────────────────────────
    public class NightwaveModel
    {
        [JsonProperty("season")]
        public int Season { get; set; }

        [JsonProperty("expiry")]
        public string Expiry { get; set; }

        [JsonProperty("activeChallenges")]
        public List<NightwaveChallenge> ActiveChallenges { get; set; } = new List<NightwaveChallenge>();
    }

    public class NightwaveChallenge
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("reputation")]
        public int Reputation { get; set; }

        [JsonProperty("isDaily")]
        public bool IsDaily { get; set; }

        [JsonProperty("isElite")]
        public bool IsElite { get; set; }

        [JsonProperty("expiry")]
        public string Expiry { get; set; }
    }

    // ── Void Fissures ──────────────────────────────────────
    public class FissureModel
    {
        [JsonProperty("node")]
        public string Node { get; set; }

        [JsonProperty("missionType")]
        public string MissionType { get; set; }

        [JsonProperty("tier")]
        public string Tier { get; set; }

        [JsonProperty("tierNum")]
        public int TierNum { get; set; }

        [JsonProperty("expiry")]
        public string Expiry { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("isStorm")]
        public bool IsStorm { get; set; }
    }

    // ── Void Trader ────────────────────────────────────────
    public class VoidTraderModel
    {
        [JsonProperty("character")]
        public string Character { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("expiry")]
        public string Expiry { get; set; }

        [JsonProperty("inventory")]
        public List<TraderItem> Inventory { get; set; } = new List<TraderItem>();
    }

    public class TraderItem
    {
        [JsonProperty("item")]
        public string Item { get; set; }

        [JsonProperty("ducats")]
        public int Ducats { get; set; }

        [JsonProperty("credits")]
        public int Credits { get; set; }
    }

    // ── Daily Deals ────────────────────────────────────────
    public class DailyDealModel
    {
        [JsonProperty("item")]
        public string Item { get; set; }

        [JsonProperty("originalPrice")]
        public int OriginalPrice { get; set; }

        [JsonProperty("salePrice")]
        public int SalePrice { get; set; }

        [JsonProperty("discount")]
        public int Discount { get; set; }

        [JsonProperty("sold")]
        public int Sold { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("expiry")]
        public string Expiry { get; set; }
    }

    // ── Invasions ──────────────────────────────────────────
    public class InvasionModel
    {
        [JsonProperty("node")]
        public string Node { get; set; }

        [JsonProperty("attackingFaction")]
        public string AttackingFaction { get; set; }

        [JsonProperty("defendingFaction")]
        public string DefendingFaction { get; set; }

        [JsonProperty("completion")]
        public double Completion { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }

        [JsonProperty("attackerReward")]
        public InvasionReward AttackerReward { get; set; }

        [JsonProperty("defenderReward")]
        public InvasionReward DefenderReward { get; set; }
    }

    public class InvasionReward
    {
        [JsonProperty("asString")]
        public string AsString { get; set; }
    }

    // ── Cycles ─────────────────────────────────────────────
    public class CetusCycleModel
    {
        [JsonProperty("isDay")]
        public bool IsDay { get; set; }

        [JsonProperty("expiry")]
        public string Expiry { get; set; }

        [JsonProperty("timeLeft")]
        public string TimeLeft { get; set; }
    }

    public class VallisCycleModel
    {
        [JsonProperty("isWarm")]
        public bool IsWarm { get; set; }

        [JsonProperty("expiry")]
        public string Expiry { get; set; }

        [JsonProperty("timeLeft")]
        public string TimeLeft { get; set; }
    }

    // ── Dashboard ViewModel ────────────────────────────────
    public class DashboardViewModel
    {
        public SortieModel Sortie { get; set; }
        public NightwaveModel Nightwave { get; set; }
        public List<FissureModel> Fissures { get; set; }
        public VoidTraderModel VoidTrader { get; set; }
        public List<DailyDealModel> DailyDeals { get; set; }
        public List<InvasionModel> Invasions { get; set; }
        public CetusCycleModel CetusCycle { get; set; }
        public VallisCycleModel VallisCycle { get; set; }
    }
}
