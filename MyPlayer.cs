using DynamicInvasions.Items;
using DynamicInvasions.NetProtocol;
using HamstarHelpers.Helpers.Players;
using HamstarHelpers.Services.Messages.Inbox;
using Microsoft.Xna.Framework;
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
				this.FinishModSettingsSync();
			}

			if( Main.netMode == 1 ) {   // Client
				ClientPacketHandlers.SendInvasionStatusRequestFromClient();
			}

			this.HasEnteredWorld = true;
		}

		////

		internal void FinishModSettingsSync() {
			var mymod = (DynamicInvasionsMod)this.mod;

			string msg = "Want to summon custom invasions? Craft a Cross Dimensional Aggregator item at a Tinkerer's Workship with: ";
			if( mymod.Config.MirrorsPerAggregator > 0 ) {
				msg += mymod.Config.MirrorsPerAggregator + "x Magic/Ice Mirror, ";
			}
			msg += mymod.Config.BannersPerAggregator + "x monster banners (any), ";
			msg += "1x Music Box (recorded)";

			InboxMessages.SetMessage( "DynamicInvasionsRecipe", msg, false );
		}


		////////////////

		public override void PreUpdate() {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.Config.Enabled ) { return; }

			if( this.player.whoAmI == Main.myPlayer ) {
				var modworld = ModContent.GetInstance<DynamicInvasionsWorld>();
				modworld.Logic.Update();
			}
		}

		////////////////

		public override bool PreItemCheck() {
			if( this.player.itemAnimation > 0 ) {
				Item heldItem = this.player.HeldItem;

				if( heldItem.type == ModContent.ItemType<CrossDimensionalAggregatorItem>() ) {
					Vector2 pos = PlayerItemHelpers.TipOfHeldItem( this.player );
					Dust.NewDust( pos, heldItem.width, heldItem.height, 15, 0, 0, 150, Main.DiscoColor, 1.2f );
				}
			}
			
			return base.PreItemCheck();
		}
	}
}
