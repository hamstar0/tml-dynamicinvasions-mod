using HamstarHelpers.Helpers.Players;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace DynamicInvasions.Items {
	//InterdimensionalTranslocator
	//EphemeralEffectuatorItem
	partial class CrossDimensionalAggregatorItem : ModItem {
		public static Item GetFuelItemFromInventory( Player player ) {
			return PlayerItemFinderHelpers.FindFirstOfPossessedItemFor( player, new HashSet<int> { ItemID.DD2ElderCrystal }, false );
		}



		////////////////

		public override void SetStaticDefaults() {
			this.DisplayName.SetDefault( "Cross-Dimensional Aggregator" );
			this.Tooltip.SetDefault( "Draws an invasion of known-creatures from across realms. Runs until out of juice."+'\n'+
				"Consumes Eternia Crystals for power." );
		}

		public override void SetDefaults() {
			this.item.width = 32;
			this.item.height = 32;
			this.item.maxStack = 1;
			//this.item.value = Item.buyPrice( 0, mymod.Config.Data.BannersPerTranslocator, 0, 0 );
			this.item.value = Item.buyPrice( 0, 5, 0, 0 );
			this.item.rare = 4;
			//this.item.consumable = true;
			this.item.useStyle = 4;
			this.item.useTime = 30;
			this.item.useAnimation = 30;
			this.item.maxStack = 1;
			this.item.UseSound = SoundID.Item46;
		}


		////////////////

		public override void ModifyTooltips( List<TooltipLine> tooltips ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			var itemInfo = this.item.GetGlobalItem<AggregatorItemInfo>();

			if( !itemInfo.IsInitialized ) {
				var noTip = new TooltipLine( this.mod, "AggregatorNoLabel", "No invasion to summon; try crafting instead." );
				noTip.overrideColor = Color.Red;

				tooltips.Add( noTip );
				return;
			}

			if( mymod.Config.CanAbortInvasions ) {
				string abortTipStr = mymod.Config.InvasionAbortFuelCost >= 1
					? "Right-click to abort current invasion (costs "+mymod.Config.InvasionAbortFuelCost+" Eternia Crystals)."
					: "Right-click to abort current invasion.";
				var abortTip = new TooltipLine( this.mod, "AggregatorAbort", abortTipStr );

				tooltips.Add( abortTip );
			}

			int fuelCost = this.GetFuelCost();
			var useTip = new TooltipLine( this.mod, "AggregatorUses", "Eternia Crystals needed: " + fuelCost );
			tooltips.Add( useTip );

			tooltips.Add( new TooltipLine( this.mod, "AggregatorListLabel", "Creates an invasion of the following:" ) );

			string[] names = itemInfo.GetNpcNames();
			for( int i = 0; i < names.Length; i++ ) {
				var npcTip = new TooltipLine( this.mod, "AggregatorListItem_" + i, names[i] );
				npcTip.overrideColor = Color.Green;

				tooltips.Add( npcTip );
			}
		}


		////////////////

		public override void AddRecipes() {
			var mymod = (DynamicInvasionsMod)this.mod;
			var myrecipe = new CrossDimensionaAggregatorItemRecipe( mymod, mymod.Config.BannersPerAggregator );
			myrecipe.AddRecipe();

			var revertRecipe = new ModRecipe( mymod );
			revertRecipe.AddIngredient( mymod.ItemType<CrossDimensionalAggregatorItem>(), 1 );
			revertRecipe.SetResult( ItemID.MagicMirror, 1 );
			revertRecipe.AddRecipe();
		}


		////////////////

		public int GetFuelCost() {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( mymod.Config.DebugModeCheat ) {
				return 0;
			}

			int uses = this.item.GetGlobalItem<AggregatorItemInfo>().Uses;

			return (int)((float)(uses + 1) * mymod.Config.AggregatorFuelCostMultiplier);
		}
	}
}
