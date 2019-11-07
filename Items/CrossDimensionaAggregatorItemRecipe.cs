using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.Items;
using HamstarHelpers.Helpers.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace DynamicInvasions.Items {
	class CrossDimensionaAggregatorItemRecipe : ModRecipe {
		public int BannerCount { get; private set; }

		private IList<int> BannerItemTypes = new List<int>();
		private int MusicBoxItemType = -1;



		////////////////

		public CrossDimensionaAggregatorItemRecipe( DynamicInvasionsMod mymod, int bannerCount ) : base( mymod ) {
			this.BannerCount = bannerCount;

			this.AddTile( TileID.TinkerersWorkbench );

			if( !DynamicInvasionsMod.Config.DebugModeCheat && DynamicInvasionsMod.Config.MirrorsPerAggregator > 0 ) {
				this.AddRecipeGroup( "ModHelpers:MagicMirrors", DynamicInvasionsMod.Config.MirrorsPerAggregator );
				//this.AddIngredient( ItemID.DarkShard, 1 );  //ItemID.Obsidian
				//this.AddIngredient( ItemID.LightShard, 1 ); //ItemID.Cloud
			}

			this.AddRecipeGroup( "ModHelpers:VanillaRecordedMusicBoxes", 1 );
			this.AddRecipeGroup( "ModHelpers:MobBanners", bannerCount );
			this.SetResult( ModContent.ItemType<CrossDimensionalAggregatorItem>() );
		}

		public override int ConsumeItem( int itemType, int numRequired ) {
			int consumed;
			
			if( !this.MarkConsumeBannerItems(itemType, numRequired, out consumed) ) {
				if( this.MarkConsumeMusicBoxItem( itemType ) ) {
					consumed = 1;
				}
			}

			if( DynamicInvasionsMod.Config.DebugModeInfo ) {
				Item item = new Item();
				item.SetDefaults( itemType, true );
				LogHelpers.Log( "Consumed "+consumed+" of "+numRequired+" of "+itemType+" ("+item.Name+")" );
			}

			return numRequired;
		}

		////

		private bool MarkConsumeBannerItems( int consumedItemType, int numRequired, out int consumed ) {
			var bannerItemTypes = NPCBannerHelpers.GetBannerItemTypes();
			if( !bannerItemTypes.Contains(consumedItemType) ) {
				consumed = 0;
				return false;
			}

			Item[] inv = Main.LocalPlayer.inventory;
			int itemIdx = inv.FirstOrDefault(
				( item ) => {
					return (!item?.IsAir ?? false)
						|| item.type == consumedItemType;
				} )?.type ?? -1;
			if( itemIdx == -1 ) {
				LogHelpers.Warn( "Could not find item of type " + consumedItemType );
				consumed = 0;
				return false;
			}

			this.BannerItemTypes = new List<int>();

			Item bannerItem = inv[itemIdx];
			int i;
			for( i = 0; i < bannerItem.stack; i++ ) {
				this.BannerItemTypes.Add( bannerItem.type );

				if( this.BannerItemTypes.Count >= numRequired ) {
					break;
				}
			}

			consumed = i;
			return true;
		}

		private bool MarkConsumeMusicBoxItem( int consumedItemType ) {
			var musicItemTypes = MusicBoxHelpers.GetVanillaMusicBoxItemIds();
			if( musicItemTypes.Contains(consumedItemType) ) {
				return false;
			}

			Item[] inv = Main.LocalPlayer.inventory;
			//var musicItemTypes = MusicBoxHelpers.GetVanillaMusicBoxItemIds();

			this.MusicBoxItemType = inv.FirstOrDefault( ( item ) => {
				return (!item?.IsAir ?? false) && consumedItemType == item.type;
			} )?.type ?? -1;

			return true;
		}


		////////////////

		public override void OnCraft( Item item ) {
			if( this.MusicBoxItemType == -1 ) {
				throw new Exception( "No music box given for custom invasion summon item." );
			}
			if( this.BannerItemTypes.Count < this.BannerCount ) {
				throw new Exception( "Insufficient or no banners given for custom invasion summon item." );
			}

			var itemInfo = item.GetGlobalItem<AggregatorItemInfo>();
			
			itemInfo.Initialize( this.MusicBoxItemType, this.BannerItemTypes );
		}


		public override bool RecipeAvailable() {
			if( !DynamicInvasionsMod.Config.Enabled ) { return false; }
			
			return DynamicInvasionsMod.Config.CraftableAggregators;
		}
	}
}
