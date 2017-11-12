using DynamicInvasions.Items;
using DynamicInvasions.NetProtocol;
using HamstarHelpers.PlayerHelpers;
using System;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions {
	class MyModPlayer : ModPlayer {
		public bool HasEnteredWorld = false;


		public override void Initialize() {
			this.HasEnteredWorld = false;
		}

		public override void clientClone( ModPlayer client_clone ) {
			var clone = (MyModPlayer)client_clone;
			clone.HasEnteredWorld = this.HasEnteredWorld;
		}

		public override void OnEnterWorld( Player player ) {
			if( this.player.whoAmI == Main.myPlayer ) {
				var mymod = (DynamicInvasionsMod)this.mod;

				if( Main.netMode != 2 ) {   // Not server
					if( !mymod.Config.LoadFile() ) {
						mymod.Config.SaveFile();
					}
				}

				if( Main.netMode == 1 ) {   // Client
					ClientPacketHandlers.SendModSettingsRequestFromClient( mymod );
					ClientPacketHandlers.SendInvasionStatusRequestFromClient( mymod );
				}

				this.HasEnteredWorld = true;
			}
		}


		public override void PreUpdate() {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return; }

			if( this.player.whoAmI == Main.myPlayer ) {
				var modworld = this.mod.GetModWorld<MyModWorld>();
				modworld.Logic.Update( mymod );
			}
		}


		public override bool PreItemCheck() {
			if( this.player.itemAnimation > 0 ) {
				Item held_item = this.player.HeldItem;

				if( held_item.type == this.mod.ItemType<CrossDimensionalAggregatorItem>() ) {
					Dust.NewDust( PlayerItemHelpers.TipOfHeldItem( this.player ), held_item.width, held_item.height, 15, 0, 0, 150, Main.DiscoColor, 1.2f );
				}
			}
			
			return base.PreItemCheck();
		}
	}
}
