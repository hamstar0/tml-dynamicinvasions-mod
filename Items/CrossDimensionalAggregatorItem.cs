using DynamicInvasions.NetProtocol;
using HamstarHelpers.DebugHelpers;
using HamstarHelpers.ItemHelpers;
using HamstarHelpers.NPCHelpers;
using HamstarHelpers.PlayerHelpers;
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
			if( !mymod.Config.Data.Enabled ) { return false; }

			var modworld = mymod.GetModWorld<MyModWorld>();
			var item_info = this.item.GetGlobalItem<AggregatorItemInfo>();

			if( !item_info.IsInitialized ) {
				return false;
			}

			if( !modworld.Logic.CanStartInvasion( mymod ) ) {
				Main.NewText( "Signal disrupted by mass of surface activity.", Main.errorColor );
				return false;
			}

			Item fuel_item = PlayerItemHelpers.FindFirstOfItemFor( player, new HashSet<int> { ItemID.DD2ElderCrystal } );
			int fuel_cost = this.GetFuelCost();
			bool has_fuel = fuel_item != null && !fuel_item.IsAir && fuel_cost <= fuel_item.stack;
			if( !has_fuel ) {
				Main.NewText( "Not enough Eternia Crystals.", Main.errorColor );
				return false;
			}

			return has_fuel;
		}


		public override bool UseItem( Player player ) {
			if( player.itemAnimation > 0 && player.itemTime == 0 ) {
				player.itemTime = this.item.useTime;

				Item fuel_item;

				if( this.CanStartInvasion( player, out fuel_item ) ) {
					this.ActivateInvasion( fuel_item );
				}
			}

			return true;
		}

		
		public override void AddRecipes() {
			var mymod = (DynamicInvasionsMod)this.mod;
			var myrecipe = new InterdimensionaAggregatorItemRecipe( mymod, mymod.Config.Data.BannersPerAggregator );
			myrecipe.AddRecipe();
		}


		public override void ModifyTooltips( List<TooltipLine> tooltips ) {
			var item_info = this.item.GetGlobalItem<AggregatorItemInfo>();

			if( !item_info.IsInitialized ) {
				var no_tip = new TooltipLine( this.mod, "AggregatorNoLabel", "No invasion to summon; try crafting instead." );
				no_tip.overrideColor = Color.Red;

				tooltips.Add( no_tip );
				return;
			}

			int fuel_cost = this.GetFuelCost();
			var use_tip = new TooltipLine( this.mod, "AggregatorUses", "Eternia Crystals needed: "+ fuel_cost );
			tooltips.Add( use_tip );

			tooltips.Add( new TooltipLine( this.mod, "AggregatorListLabel", "Creates an invasion of the following:" ) );
			
			string[] names = item_info.GetNpcNames();
			for( int i=0; i<names.Length; i++ ) {
				var npc_tip = new TooltipLine( this.mod, "AggregatorListItem_"+i, names[i] );
				npc_tip.overrideColor = Color.Green;

				tooltips.Add( npc_tip );
			}
		}


		////////////////

		public int GetFuelCost() {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( mymod.IsCheatMode()) {
				return 0;
			}

			int uses = this.item.GetGlobalItem<AggregatorItemInfo>().Uses;

			return (int)((float)(uses + 1) * mymod.Config.Data.AggregatorFuelCostMultiplier);
		}

		////////////////

		private bool CanStartInvasion( Player player, out Item fuel_item ) {
			fuel_item = PlayerItemHelpers.FindFirstOfItemFor( player, new HashSet<int> { ItemID.DD2ElderCrystal } );
			if( fuel_item == null || fuel_item.IsAir ) {
				return false;
			}

			var mymod = (DynamicInvasionsMod)this.mod;
			var modworld = mymod.GetModWorld<MyModWorld>();
			var item_info = this.item.GetGlobalItem<AggregatorItemInfo>();
			int fuel_cost = this.GetFuelCost();

			if( !item_info.IsInitialized ) {
				return false;
			}

			// Not enough fuel?
			if( fuel_cost > fuel_item.stack ) {
				return false;
			}
			
			return modworld.Logic.CanStartInvasion( mymod );
		}

		private void ActivateInvasion( Item fuel_item ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			var modworld = mymod.GetModWorld<MyModWorld>();
			var item_info = this.item.GetGlobalItem<AggregatorItemInfo>();
			int fuel_cost = this.GetFuelCost();

			if( mymod.IsDebugInfoMode() ) {
				Main.NewText( "Activating invasion..." );
			}

			ItemHelpers.ReduceStack( fuel_item, fuel_cost );
			item_info.Use();

			if( Main.netMode == 0 ) {
				modworld.Logic.StartInvasion( mymod, item_info.MusicType, item_info.BannerItemTypesToNpcTypes );
			} else if( Main.netMode == 1 ) {
				ClientPacketHandlers.SendInvasionRequestFromClient( mymod, item_info.MusicType, item_info.BannerItemTypesToNpcTypes );
			}
		}
	}



	class InterdimensionaAggregatorItemRecipe : ModRecipe {
		public int BannerCount { get; private set; }

		private IList<int> BannerItemTypes = new List<int>();
		private int MusicBoxItemType = -1;


		public InterdimensionaAggregatorItemRecipe( DynamicInvasionsMod mymod, int banner_count ) : base( mymod ) {
			this.BannerCount = banner_count;

			this.AddTile( TileID.TinkerersWorkbench );

			if( !mymod.IsCheatMode() ) {
				this.AddRecipeGroup( "HamstarHelpers:MagicMirrors", 1 );
				this.AddIngredient( ItemID.DarkShard, 1 );  //ItemID.Obsidian
				this.AddIngredient( ItemID.LightShard, 1 ); //ItemID.Cloud
			}

			this.AddRecipeGroup( "HamstarHelpers:RecordedMusicBoxes", 1 );
			this.AddRecipeGroup( "HamstarHelpers:NpcBanners", banner_count );
			this.SetResult( mymod.ItemType<CrossDimensionalAggregatorItem>() );
		}

		public override int ConsumeItem( int item_type, int num_required ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			var music_item_types = ItemMusicBoxHelpers.GetMusicBoxes();
			var banner_item_types = NPCBannerHelpers.GetBannerItemTypes();
			Item[] inv = Main.LocalPlayer.inventory;

			if( banner_item_types.Contains(item_type) ) {
				SortedSet<int> banner_items = ItemFinderHelpers.FindIndexOfEachItemInCollection( inv, banner_item_types );

				this.BannerItemTypes = new List<int>();
				
				foreach( int i in banner_items ) {
					Item banner_item = inv[i];

					for( int j=0; j<banner_item.stack; j++ ) {
						this.BannerItemTypes.Add( banner_item.type );
						if( this.BannerItemTypes.Count >= num_required ) { break; }
					}
					if( this.BannerItemTypes.Count >= num_required ) { break; }
				}
			} else if( music_item_types.Contains( item_type ) ) {
				int idx = ItemFinderHelpers.FindIndexOfFirstOfItemInCollection( inv, music_item_types );
				if( idx >= 0 ) {
					this.MusicBoxItemType = inv[idx].type;
				}
			}

			if( mymod.IsDebugInfoMode() ) {
				Item item = new Item();
				item.SetDefaults( item_type );
				DebugHelpers.Log( "consumed "+num_required+" of "+item_type+" ("+item.Name+")" );
			}

			return num_required;
		}

		public override void OnCraft( Item item ) {
			if( this.MusicBoxItemType == -1 ) { throw new Exception( "No music box given for custom invasion summon item." ); }
			if( this.BannerItemTypes.Count == 0 ) { throw new Exception( "No banners given for custom invasion summon item." ); }

			var item_info = item.GetGlobalItem<AggregatorItemInfo>();
			
			item_info.Initialize( this.MusicBoxItemType, this.BannerItemTypes );
		}

		public override bool RecipeAvailable() {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return false; }
			
			return mymod.Config.Data.CraftableAggregators;
		}
	}
}
