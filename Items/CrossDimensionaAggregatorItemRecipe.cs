using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.DotNET;
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

		public override int ConsumeItem( int ingredientItemOrRecipeGroupItemType, int numRequired ) {
			var bannerItemTypes = NPCBannerHelpers.GetBannerItemTypes();
			int consumed = 0;

			if( bannerItemTypes.Contains(ingredientItemOrRecipeGroupItemType) ) {
				this.MarkConsumeableBannerItems( numRequired, out consumed );
			} else {
				if( this.MarkConsumeMusicBoxItem(ingredientItemOrRecipeGroupItemType) != null ) {
					consumed++;
				}
			}

			if( DynamicInvasionsMod.Config.DebugModeInfo ) {
				if( consumed == 0 ) {
					Item item = new Item();
					item.SetDefaults( ingredientItemOrRecipeGroupItemType, true );
					LogHelpers.Log( "No item consumed of ingredient/recipe group [" + ingredientItemOrRecipeGroupItemType + "] (" + item.Name + ")" );
				} else {
					LogHelpers.Log( "Consumed " + consumed + " of " + numRequired + " of item group type [" + ingredientItemOrRecipeGroupItemType + "]" );
				}
			}

			return numRequired;
		}

		////

		private void MarkConsumeableBannerItems( int numRequired, out int consumed ) {
			var bannerItemTypes = NPCBannerHelpers.GetBannerItemTypes();

			int myConsumed = 0;
			IEnumerable<int> bannerIndexes = Main.LocalPlayer.inventory
				.SafeSelect( (item, idx) => (item, idx) )
				.SafeWhere( (itemAndIdx) => {
					if( myConsumed >= numRequired ) {
						return false;
					}

					if( itemAndIdx.item == null || itemAndIdx.item.IsAir ) {
						return false;
					}

					if( !bannerItemTypes.Contains(itemAndIdx.item.type) ) {
						return false;
					}

					myConsumed += itemAndIdx.item.stack;
					return true;
				} )
				.SafeSelect( (itemAndIdx) => itemAndIdx.idx );

			void registerBanners( out int consumedAgain ) {
				consumedAgain = 0;

				foreach( int invIdx in bannerIndexes ) {
					Item item = Main.LocalPlayer.inventory[invIdx];

					for( int i = 0; i < item.stack; i++ ) {
						this.BannerItemTypes.Add( item.type );

						consumedAgain++;
						if( consumedAgain >= numRequired ) {
							return;
						}
					}
				}
			}

			consumed = 0;
			registerBanners( out consumed );
		}

		private Item MarkConsumeMusicBoxItem( int consumedItemGroupType ) {
			var musicItemTypes = MusicBoxHelpers.GetVanillaMusicBoxItemIds();
			if( !musicItemTypes.Contains( consumedItemGroupType ) ) {
				return null;
			}

			Item[] inv = Main.LocalPlayer.inventory;
			//var musicItemTypes = MusicBoxHelpers.GetVanillaMusicBoxItemIds();

			Item musicBoxItem = inv.FirstOrDefault( ( item ) => {
				return !( item?.IsAir ?? true )
					&& musicItemTypes.Contains( item.type );
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
				throw new Exception( "Need " + this.BannerCount + " banners (of " + this.BannerItemTypes.Count + ") for custom invasion summon item." );
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
