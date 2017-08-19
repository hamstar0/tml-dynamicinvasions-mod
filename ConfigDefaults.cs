using System;


namespace DynamicInvasions {
	public class ConfigurationData {
		public readonly static Version ConfigVersion = new Version( 0, 8, 1 );


		public string VersionSinceUpdate = "";

		public bool Enabled = true;

		public int DEBUGFLAGS = 0;	// 1: Info; 2: Reset; 4: Cheaty mode

		public bool CraftableAggregators = true;
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
			var new_config = new ConfigurationData();
			var vers_since = this.VersionSinceUpdate != "" ?
				new Version( this.VersionSinceUpdate ) :
				new Version();

			if( vers_since >= ConfigurationData.ConfigVersion ) {
				return false;
			}
			
			this.VersionSinceUpdate = ConfigurationData.ConfigVersion.ToString();

			return true;
		}
	}
}
