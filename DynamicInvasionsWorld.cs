using DynamicInvasions.Invasion;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace DynamicInvasions {
	class DynamicInvasionsWorld : ModWorld {
		public InvasionLogic Logic { get; private set; }


		public override void Initialize() {
			var mymod = (DynamicInvasionsMod)this.mod;
			this.Logic = new InvasionLogic( mymod );
		}


		public override void Load( TagCompound tags ) {
			var mymod = (DynamicInvasionsMod)this.mod;

			if( mymod.Config.DebugModeReset ) {
				Main.invasionDelay = 0;
			} else {
				this.Logic.LoadMe( tags );
			}
		}

		public override TagCompound Save() {
			return this.Logic.SaveMe();
		}

		//public override void NetSend( BinaryWriter writer ) {
			//this.Logic.GetNetSender()( writer );
		//}

		//public override void NetReceive( BinaryReader reader ) {
			//this.Logic.GetNetReceiver()( reader );
		//}


		public override void PreUpdate() {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return; }

			if( Main.netMode == 2 ) {	// Server
				this.Logic.Update( mymod );
			}
		}
	}
}
