using DynamicInvasions.NetProtocol;
using HamstarHelpers.Helpers.Items;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions.Items {
	partial class CrossDimensionalAggregatorItem : ModItem {
		public override bool CanUseItem( Player player ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.Config.Enabled ) { return false; }

			var modworld = ModContent.GetInstance<DynamicInvasionsWorld>();
			var itemInfo = this.item.GetGlobalItem<AggregatorItemInfo>();

			if( !itemInfo.IsInitialized ) {
				return false;
			}

			if( !modworld.Logic.CanStartInvasion() ) {
				Main.NewText( "Signal disrupted by mass of surface activity.", Main.errorColor );
				return false;
			}

			Item fuelItem = CrossDimensionalAggregatorItem.GetFuelItemFromInventory( player );
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

		////////////////

		public override bool ConsumeItem( Player player ) {
			return false;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void RightClick( Player player ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			var myworld = ModContent.GetInstance<DynamicInvasionsWorld>();

			if( mymod.Config.CanAbortInvasions && myworld.Logic.IsInvasionHappening() ) {
				Item fuelItem = CrossDimensionalAggregatorItem.GetFuelItemFromInventory( player );
				int fuelAmt = fuelItem != null && !fuelItem.IsAir
					? fuelItem.stack
					: 0;

				if( mymod.Config.InvasionAbortFuelCost == 0 || fuelAmt >= mymod.Config.InvasionAbortFuelCost ) {
					if( mymod.Config.InvasionAbortFuelCost > 0 ) {
						ItemHelpers.ReduceStack( fuelItem, mymod.Config.InvasionAbortFuelCost );
					}

					Main.NewText( "Ending invasion..." );

					if( Main.netMode == 0 ) {
						myworld.Logic.EndInvasion();
					} else if( Main.netMode == 1 ) {
						ClientPacketHandlers.SendEndInvasionRequestFromClient();
					}
				} else {
					Main.NewText( "You need "+mymod.Config.InvasionAbortFuelCost+" Eternia Crystals to abort an invasion.", Color.Yellow );
				}
			} else {
				Main.NewText( "No custom invasion in progress.", Color.Yellow );
			}
		}


		////////////////

		private bool CanStartInvasion( Player player, out Item fuelItem ) {
			fuelItem = CrossDimensionalAggregatorItem.GetFuelItemFromInventory( player );
			if( fuelItem == null || fuelItem.IsAir ) {
				return false;
			}

			var myworld = ModContent.GetInstance<DynamicInvasionsWorld>();
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
			var myworld = ModContent.GetInstance<DynamicInvasionsWorld>();
			var itemInfo = this.item.GetGlobalItem<AggregatorItemInfo>();
			int fuelCost = this.GetFuelCost();

			if( mymod.Config.DebugModeInfo ) {
				Main.NewText( "Activating invasion..." );
			}

			ItemHelpers.ReduceStack( fuelItem, fuelCost );
			itemInfo.Use();

			if( Main.netMode == 0 ) {
				myworld.Logic.StartInvasion( itemInfo.MusicType, itemInfo.BannerItemTypesToNpcTypes );
			} else if( Main.netMode == 1 ) {
				ClientPacketHandlers.SendInvasionRequestFromClient( itemInfo.MusicType, itemInfo.BannerItemTypesToNpcTypes );
			}
		}
	}
}
