using HamstarHelpers.Classes.UI.ModConfig;
using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;


namespace DynamicInvasions {
	class MyFloatInputElement : FloatInputElement { }





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

		[Range( 0, 99 )]
		[DefaultValue( 1 )]
		public int MirrorsPerAggregator = 1;

		[Range( 0, 30 )]
		[DefaultValue( 5 )]
		public int BannersPerAggregator = 5;

		[Range( 0f, 100f )]
		[DefaultValue( 1.5f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float AggregatorFuelCostMultiplier = 1.5f;


		[Range( 1, 60 * 60 )]
		[DefaultValue( 30 )]
		public int InvasionArrivalTimeInSeconds = 30;


		[Range( 1, 10000 )]
		[DefaultValue( 80 )]
		public int InvasionMinSize = 80;

		[Range( 1, 10000 )]
		[DefaultValue( 40 )]
		public int InvasionAddedSizePerStrongPlayer = 40;

		[Range( 1, 1000 )]
		[DefaultValue( 72 )]
		public int InvasionSpawnRate = 72;  // Decrease amount to increase rate

		[Range( 1, 1000 )]
		[DefaultValue( 16 )]
		public int InvasionSpawnMax = 16;

		[Range( 1f, 10000f )]
		[DefaultValue( 2000f )]
		public float InvasionSpawnRatePerType = 2000f;


		[Range( 0f, 1f )]
		[DefaultValue( 0.25f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float InvaderLootDropPercentChance = 0.25f;


		[DefaultValue( true )]
		public bool CanCraftTrophiesIntoBanners = true;


		[DefaultValue( true )]
		public bool MidBossesAllowed = true;

		[Range( 0.01f, 1000f )]
		[DefaultValue( 0.5f )]
		[CustomModConfigItem( typeof( MyFloatInputElement ) )]
		public float MidBossHpMultiplier = 0.5f;


		[DefaultValue( true )]
		public bool AutoInvasions = true;

		[Range( 1, 1000 )]
		[DefaultValue( 9 )]
		public int AutoInvasionAverageDays = 9;


		[DefaultValue( true )]
		public bool CanAbortInvasions = true;

		[Range( 0, 100 )]
		[DefaultValue( 3 )]
		public int InvasionAbortFuelCost = 3;
	}
}
