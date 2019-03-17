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

			if( !modworld.Logic.CanStartInvasion( mymod ) ) {
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
			var myrecipe = new InterdimensionaAggregatorItemRecipe( mymod, mymod.ConfigJson.Data.BannersPerAggregator );
			myrecipe.AddRecipe();
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
			
			return myworld.Logic.CanStartInvasion( mymod );
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
				ClientPacketHandlers.SendInvasionRequestFromClient( mymod, itemInfo.MusicType, itemInfo.BannerItemTypesToNpcTypes );
			}
		}
	}




	class InterdimensionaAggregatorItemRecipe : ModRecipe {
		public int BannerCount { get; private set; }

		private IList<int> BannerItemTypes = new List<int>();
		private int MusicBoxItemType = -1;


		public InterdimensionaAggregatorItemRecipe( DynamicInvasionsMod mymod, int bannerCount ) : base( mymod ) {
			this.BannerCount = bannerCount;

			this.AddTile( TileID.TinkerersWorkbench );

			if( !mymod.Config.DebugModeCheat && mymod.Config.MirrorsPerAggregator > 0 ) {
				this.AddRecipeGroup( "HamstarHelpers:MagicMirrors", mymod.Config.MirrorsPerAggregator );
				//this.AddIngredient( ItemID.DarkShard, 1 );  //ItemID.Obsidian
				//this.AddIngredient( ItemID.LightShard, 1 ); //ItemID.Cloud
			}

			this.AddRecipeGroup( "HamstarHelpers:RecordedMusicBoxes", 1 );
			this.AddRecipeGroup( "HamstarHelpers:NpcBanners", bannerCount );
			this.SetResult( mymod.ItemType<CrossDimensionalAggregatorItem>() );
		}

		public override int ConsumeItem( int itemType, int numRequired ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			var musicItemTypes = MusicBoxHelpers.GetVanillaMusicBoxItemIds();
			var bannerItemTypes = NPCBannerHelpers.GetBannerItemTypes();
			Item[] inv = Main.LocalPlayer.inventory;
			
			if( bannerItemTypes.Contains(itemType) ) {
				ISet<int> bannerItems = ItemFinderHelpers.FindIndexOfEach( inv, bannerItemTypes );

				this.BannerItemTypes = new List<int>();
				
				foreach( int i in bannerItems ) {
					Item bannerItem = inv[i];

					for( int j=0; j<bannerItem.stack; j++ ) {
						this.BannerItemTypes.Add( bannerItem.type );
						if( this.BannerItemTypes.Count >= numRequired ) { break; }
					}
					if( this.BannerItemTypes.Count >= numRequired ) { break; }
				}
			} else if( musicItemTypes.Contains( itemType ) ) {
				int idx = ItemFinderHelpers.FindIndexOfFirstOfItemInCollection( inv, musicItemTypes );
				if( idx >= 0 ) {
					this.MusicBoxItemType = inv[idx].type;
				}
			}

			if( mymod.Config.DebugModeInfo ) {
				Item item = new Item();
				item.SetDefaults( itemType );
				LogHelpers.Log( "consumed "+numRequired+" of "+itemType+" ("+item.Name+")" );
			}

			return numRequired;
		}

		public override void OnCraft( Item item ) {
			if( this.MusicBoxItemType == -1 ) { throw new Exception( "No music box given for custom invasion summon item." ); }
			if( this.BannerItemTypes.Count == 0 ) { throw new Exception( "No banners given for custom invasion summon item." ); }

			var itemInfo = item.GetGlobalItem<AggregatorItemInfo>();
			
			itemInfo.Initialize( this.MusicBoxItemType, this.BannerItemTypes );
		}

		public override bool RecipeAvailable() {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return false; }
			
			return mymod.ConfigJson.Data.CraftableAggregators;
		}
	}
}
