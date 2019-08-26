using HamstarHelpers.Helpers.Debug;
using HamstarHelpers.Helpers.Items;
using HamstarHelpers.Helpers.NPCs;
using System;
using System.Collections.Generic;
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

			if( !mymod.Config.DebugModeCheat && mymod.Config.MirrorsPerAggregator > 0 ) {
				this.AddRecipeGroup( "ModHelpers:MagicMirrors", mymod.Config.MirrorsPerAggregator );
				//this.AddIngredient( ItemID.DarkShard, 1 );  //ItemID.Obsidian
				//this.AddIngredient( ItemID.LightShard, 1 ); //ItemID.Cloud
			}

			this.AddRecipeGroup( "ModHelpers:VanillaRecordedMusicBoxes", 1 );
			this.AddRecipeGroup( "ModHelpers:MobBanners", bannerCount );
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
			if( !mymod.Config.Enabled ) { return false; }
			
			return mymod.Config.CraftableAggregators;
		}
	}
}
