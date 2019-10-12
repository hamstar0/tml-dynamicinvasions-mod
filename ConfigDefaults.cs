using HamstarHelpers.Classes.UI.ModConfig;
using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;


namespace DynamicInvasions {
	public class DynamicInvasionsConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ServerSide;


		////

		[DefaultValue( true )]
		public bool Enabled = true;

		public bool DebugModeInfo = false;
		public bool DebugModeReset = false;
		public bool DebugModeCheat = false;

		[DefaultValue( true )]
		public bool CraftableAggregators = true;
		[DefaultValue( 1 )]
		public int MirrorsPerAggregator = 1;
		[DefaultValue( 5 )]
		public int BannersPerAggregator = 5;
		[DefaultValue( 1.5f )]
		[CustomModConfigItem( typeof( FloatInputElement ) )]
		public float AggregatorFuelCostMultiplier = 1.5f;

		[DefaultValue( 30 )]
		public int InvasionArrivalTimeInSeconds = 30;

		[DefaultValue( 80 )]
		public int InvasionMinSize = 80;
		[DefaultValue( 40 )]
		public int InvasionAddedSizePerStrongPlayer = 40;
		[DefaultValue( 72 )]
		public int InvasionSpawnRate = 72;
		[DefaultValue( 16 )]
		public int InvasionSpawnMax = 16;
		[DefaultValue( 2000f )]
		public float InvasionSpawnRatePerType = 2000f;

		[DefaultValue( 0.25f )]
		[CustomModConfigItem( typeof( FloatInputElement ) )]
		public float InvaderLootDropPercentChance = 0.25f;

		[DefaultValue( true )]
		public bool CanCraftTrophiesIntoBanners = true;

		[DefaultValue( true )]
		public bool MidBossesAllowed = true;
		[DefaultValue( 0.5f )]
		[CustomModConfigItem( typeof( FloatInputElement ) )]
		public float MidBossHpMultiplier = 0.5f;

		[DefaultValue( true )]
		public bool AutoInvasions = true;
		[DefaultValue( 9 )]
		public int AutoInvasionAverageDays = 9;

		[DefaultValue( true )]
		public bool CanAbortInvasions = true;
		[DefaultValue( 3 )]
		public int InvasionAbortFuelCost = 3;
	}
}
