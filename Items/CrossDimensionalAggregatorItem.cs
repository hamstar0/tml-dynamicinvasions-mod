using DynamicInvasions.NetProtocol;
using HamstarHelpers.Helpers.DebugHelpers;
using HamstarHelpers.Helpers.ItemHelpers;
using HamstarHelpers.Helpers.NPCHelpers;
using HamstarHelpers.Helpers.PlayerHelpers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace DynamicInvasions.Items {
	//InterdimensionalTranslocator
	//EphemeralEffectuatorItem
	class CrossDimensionalAggregatorItem : ModItem {
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


		public override bool CanUseItem( Player player ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return false; }

			var modworld = mymod.GetModWorld<DynamicInvasionsWorld>();
			var itemInfo = this.item.GetGlobalItem<AggregatorItemInfo>();

			if( !itemInfo.IsInitialized ) {
				return false;
			}

			if( !modworld.Logic.CanStartInvasion() ) {
				Main.NewText( "Signal disrupted by mass of surface activity.", Main.errorColor );
				return false;
			}

			Item fuelItem = PlayerItemFinderHelpers.FindFirstOfItemFor( player, new HashSet<int> { ItemID.DD2ElderCrystal } );
			int fuelCost = this.GetFuelCost();
			bool hasFuel = fuelItem != null && !fuelItem.IsAir && fuelCost <= fuelItem.stack;
			if( !hasFuel ) {
				Main.NewText( "Not enough Eternia Crystals.", Main.errorColor );
				return false;
			}

			return hasFuel;
		}


		public override bool UseItem( Player player ) {
			if( player.itemAnimation > 0 && player.itemTime == 0 ) {
				player.itemTime = this.item.useTime;

				Item fuelItem;

				if( this.CanStartInvasion( player, out fuelItem ) ) {
					this.ActivateInvasion( fuelItem );
				}
			}

			return true;
		}

		
		public override void AddRecipes() {
			var mymod = (DynamicInvasionsMod)this.mod;
			var myrecipe = new CrossDimensionaAggregatorItemRecipe( mymod, mymod.ConfigJson.Data.BannersPerAggregator );
			myrecipe.AddRecipe();

			var revertRecipe = new ModRecipe( mymod );
			revertRecipe.AddIngredient( mymod.ItemType<CrossDimensionalAggregatorItem>(), 1 );
			revertRecipe.SetResult( ItemID.MagicMirror, 1 );
		}


		public override void ModifyTooltips( List<TooltipLine> tooltips ) {
			var itemInfo = this.item.GetGlobalItem<AggregatorItemInfo>();

			if( !itemInfo.IsInitialized ) {
				var noTip = new TooltipLine( this.mod, "AggregatorNoLabel", "No invasion to summon; try crafting instead." );
				noTip.overrideColor = Color.Red;

				tooltips.Add( noTip );
				return;
			}

			int fuelCost = this.GetFuelCost();
			var useTip = new TooltipLine( this.mod, "AggregatorUses", "Eternia Crystals needed: "+ fuelCost );
			tooltips.Add( useTip );

			tooltips.Add( new TooltipLine( this.mod, "AggregatorListLabel", "Creates an invasion of the following:" ) );
			
			string[] names = itemInfo.GetNpcNames();
			for( int i=0; i<names.Length; i++ ) {
				var npcTip = new TooltipLine( this.mod, "AggregatorListItem_"+i, names[i] );
				npcTip.overrideColor = Color.Green;

				tooltips.Add( npcTip );
			}
		}


		////////////////

		public int GetFuelCost() {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( mymod.Config.DebugModeCheat ) {
				return 0;
			}

			int uses = this.item.GetGlobalItem<AggregatorItemInfo>().Uses;

			return (int)((float)(uses + 1) * mymod.ConfigJson.Data.AggregatorFuelCostMultiplier);
		}

		////////////////

		private bool CanStartInvasion( Player player, out Item fuelItem ) {
			fuelItem = PlayerItemFinderHelpers.FindFirstOfItemFor( player, new HashSet<int> { ItemID.DD2ElderCrystal } );
			if( fuelItem == null || fuelItem.IsAir ) {
				return false;
			}

			var mymod = (DynamicInvasionsMod)this.mod;
			var myworld = mymod.GetModWorld<DynamicInvasionsWorld>();
			var itemInfo = this.item.GetGlobalItem<AggregatorItemInfo>();
			int fuelCost = this.GetFuelCost();

			if( !itemInfo.IsInitialized ) {
				return false;
			}

			// Not enough fuel?
			if( fuelCost > fuelItem.stack ) {
				return false;
			}
			
			return myworld.Logic.CanStartInvasion();
		}

		private void ActivateInvasion( Item fuelItem ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			var myworld = mymod.GetModWorld<DynamicInvasionsWorld>();
			var itemInfo = this.item.GetGlobalItem<AggregatorItemInfo>();
			int fuelCost = this.GetFuelCost();

			if( mymod.Config.DebugModeInfo ) {
				Main.NewText( "Activating invasion..." );
			}

			ItemHelpers.ReduceStack( fuelItem, fuelCost );
			itemInfo.Use();

			if( Main.netMode == 0 ) {
				myworld.Logic.StartInvasion( mymod, itemInfo.MusicType, itemInfo.BannerItemTypesToNpcTypes );
			} else if( Main.netMode == 1 ) {
				ClientPacketHandlers.SendInvasionRequestFromClient( itemInfo.MusicType, itemInfo.BannerItemTypesToNpcTypes );
			}
		}
	}
}
