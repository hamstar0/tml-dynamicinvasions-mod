using System;


namespace DynamicInvasions {
	public class DynamicInvasionsConfigData {
		public readonly static string ConfigFileName = "Dynamic Invasions Config.json";


		////////////////

		public string VersionSinceUpdate = "";

		public bool Enabled = true;

		public bool DebugModeInfo = false;
		public bool DebugModeReset = false;
		public bool DebugModeCheat = false;

		public bool CraftableAggregators = true;
		public int MirrorsPerAggregator = 1;
		public int BannersPerAggregator = 5;
		public float AggregatorFuelCostMultiplier = 1.5f;

		public int InvasionArrivalTimeInSeconds = 30;

		public int InvasionMinSize = 80;
		public int InvasionAddedSizePerStrongPlayer = 40;
		public int InvasionSpawnRate = 72;
		public int InvasionSpawnMax = 16;

		public float InvaderLootDropPercentChance = 0.25f;

		public bool CanCraftTrophiesIntoBanners = true;

		public bool MidBossesAllowed = true;
		public float MidBossHpMultiplier = 0.5f;

		public bool AutoInvasions = true;
		public int AutoInvasionAverageDays = 9;



		////////////////

		public bool UpdateToLatestVersion() {
			var newConfig = new DynamicInvasionsConfigData();
			var versSince = this.VersionSinceUpdate != "" ?
				new Version( this.VersionSinceUpdate ) :
				new Version();

			if( versSince >= DynamicInvasionsMod.Instance.Version ) {
				return false;
			}
			
			this.VersionSinceUpdate = DynamicInvasionsMod.Instance.Version.ToString();

			return true;
		}
	}
}
