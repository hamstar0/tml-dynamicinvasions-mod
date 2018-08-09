using DynamicInvasions.Items;
using DynamicInvasions.NetProtocol;
using HamstarHelpers.Helpers.PlayerHelpers;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions {
	class DynamicInvasionsPlayer : ModPlayer {
		public bool HasEnteredWorld = false;


		////////////////

		public override bool CloneNewInstances { get { return false; } }

		public override void Initialize() {
			this.HasEnteredWorld = false;
		}

		public override void clientClone( ModPlayer client_clone ) {
			var clone = (DynamicInvasionsPlayer)client_clone;
			clone.HasEnteredWorld = this.HasEnteredWorld;
		}


		////////////////

		public override void OnEnterWorld( Player player ) {
			if( player.whoAmI != Main.myPlayer ) { return; }
			if( this.player.whoAmI != Main.myPlayer ) { return; }

			var mymod = (DynamicInvasionsMod)this.mod;

			if( Main.netMode != 2 ) {   // Not server
				if( !mymod.ConfigJson.LoadFile() ) {
					mymod.ConfigJson.SaveFile();
				}
			}

			if( Main.netMode == 1 ) {   // Client
				ClientPacketHandlers.SendModSettingsRequestFromClient( mymod );
				ClientPacketHandlers.SendInvasionStatusRequestFromClient( mymod );
			}

			this.HasEnteredWorld = true;
		}


		////////////////

		public override void PreUpdate() {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return; }

			if( this.player.whoAmI == Main.myPlayer ) {
				var modworld = this.mod.GetModWorld<DynamicInvasionsWorld>();
				modworld.Logic.Update( mymod );
			}
		}

		////////////////

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
