using DynamicInvasions.Invasion;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace DynamicInvasions {
	class MyModWorld : ModWorld {
		public InvasionLogic Logic { get; private set; }


		public override void Initialize() {
			this.Logic = new InvasionLogic();
		}


		public override void Load( TagCompound tags ) {
			var mymod = (DynamicInvasions)this.mod;

			if( mymod.IsForcedResetMode() ) {
				Main.invasionDelay = 0;
			} else {
				this.Logic.LoadMe( tags );
			}
		}

		public override TagCompound Save() {
			return this.Logic.SaveMe();
		}

		public override void NetSend( BinaryWriter writer ) {
			//this.Logic.NetSend( writer );
		}

		public override void NetReceive( BinaryReader reader ) {
			//this.Logic.NetReceive( reader );
		}


		public override void PreUpdate() {
			var mymod = (DynamicInvasions)this.mod;
			if( !mymod.Config.Data.Enabled ) { return; }

			if( Main.netMode == 2 ) {
				this.Logic.Update( mymod );
			}
		}
	}
}
