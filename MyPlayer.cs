using DynamicInvasions.Items;
using DynamicInvasions.NetProtocol;
using HamstarHelpers.Helpers.PlayerHelpers;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions {
	class DynamicInvasionsPlayer : ModPlayer {
		public bool HasEnteredWorld = false;
		
		////////////////

		public override bool CloneNewInstances => false;

		////////////////



		public override void Initialize() {
			this.HasEnteredWorld = false;
		}

		public override void clientClone( ModPlayer clientClone ) {
			var clone = (DynamicInvasionsPlayer)clientClone;
			clone.HasEnteredWorld = this.HasEnteredWorld;
		}


		////////////////

		public override void OnEnterWorld( Player player ) {
			if( player.whoAmI != Main.myPlayer ) { return; }
			if( this.player.whoAmI != Main.myPlayer ) { return; }

			var mymod = (DynamicInvasionsMod)this.mod;

			if( Main.netMode == 0 ) {
				if( !mymod.ConfigJson.LoadFile() ) {
					mymod.ConfigJson.SaveFile();
				}
			}

			if( Main.netMode == 1 ) {   // Client
				ClientPacketHandlers.SendModSettingsRequestFromClient();
				ClientPacketHandlers.SendInvasionStatusRequestFromClient();
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
				Item heldItem = this.player.HeldItem;

				if( heldItem.type == this.mod.ItemType<CrossDimensionalAggregatorItem>() ) {
					Dust.NewDust( PlayerItemHelpers.TipOfHeldItem( this.player ), heldItem.width, heldItem.height, 15, 0, 0, 150, Main.DiscoColor, 1.2f );
				}
			}
			
			return base.PreItemCheck();
		}
	}
}
