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

		////

		public override int ConsumeItem( int ingredientOrRecipeGroupItemType, int numRequired ) {
			int consumed;
			Item consumedItem = this.MarkConsumeBannerItems( ingredientOrRecipeGroupItemType, numRequired, out consumed );
			
			if( consumedItem == null ) {
				consumedItem = this.MarkConsumeMusicBoxItem( ingredientOrRecipeGroupItemType );

				if( consumedItem != null ) {
					consumed = 1;
				}
			}

			if( DynamicInvasionsMod.Config.DebugModeInfo ) {
				if( consumedItem == null ) {
					Item item = new Item();
					item.SetDefaults( ingredientOrRecipeGroupItemType, true );
					LogHelpers.Log( "No item consumed of ingredient/recipe group ["+ ingredientOrRecipeGroupItemType + "] ("+item.Name+")" );
				} else {
					LogHelpers.Log( "Consumed "+consumed+" of "+numRequired+" of item ["+consumedItem.type+"] ("+consumedItem.Name+")" );
				}
			}

			return numRequired;
		}

		////

		private Item MarkConsumeBannerItems( int consumedItemGroupType, int numRequired, out int consumed ) {
			var bannerItemTypes = NPCBannerHelpers.GetBannerItemTypes();
			if( !bannerItemTypes.Contains(consumedItemGroupType) ) {
				consumed = 0;
				return null;
			}
			
			Item[] inv = Main.LocalPlayer.inventory;
			Item bannerItem = inv.FirstOrDefault( ( item ) => {
				return !(item?.IsAir ?? true)
					&& bannerItemTypes.Contains(item.type);
			} );
			if( bannerItem == null ) {
				consumed = 0;
				return null;
			}

			this.BannerItemTypes = new List<int>();

			int i;
			for( i = 0; i < bannerItem.stack; i++ ) {
				this.BannerItemTypes.Add( bannerItem.type );

				if( this.BannerItemTypes.Count >= numRequired ) {
					break;
				}
			}

			consumed = i;
			return bannerItem;
		}

		private Item MarkConsumeMusicBoxItem( int consumedItemGroupType ) {
			var musicItemTypes = MusicBoxHelpers.GetVanillaMusicBoxItemIds();
			if( !musicItemTypes.Contains(consumedItemGroupType) ) {
				return null;
			}

			Item[] inv = Main.LocalPlayer.inventory;
			//var musicItemTypes = MusicBoxHelpers.GetVanillaMusicBoxItemIds();

			Item musicBoxItem = inv.FirstOrDefault( ( item ) => {
				return !(item?.IsAir ?? true)
					&& musicItemTypes.Contains(item.type);
			} );
			this.MusicBoxItemType = musicBoxItem?.type ?? -1;

			return musicBoxItem;
		}


		////////////////

		public override void OnCraft( Item item ) {
			if( this.MusicBoxItemType == -1 ) {
				throw new Exception( "No music box given for custom invasion summon item." );
			}
			if( this.BannerItemTypes.Count < this.BannerCount ) {
				throw new Exception( "Need "+this.BannerCount+" banners (of "+this.BannerItemTypes.Count+") for custom invasion summon item." );
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
