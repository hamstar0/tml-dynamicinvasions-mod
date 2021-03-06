﻿using DynamicInvasions.Invasion;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace DynamicInvasions {
	class DynamicInvasionsWorld : ModWorld {
		public InvasionLogic Logic { get; private set; }



		////////////////

		public override void Initialize() {
			var mymod = (DynamicInvasionsMod)this.mod;
			this.Logic = new InvasionLogic();
		}

		internal void Uninitialize() {
			this.Logic = null;
		}

		////////////////

		public override void Load( TagCompound tags ) {
			if( DynamicInvasionsMod.Config.DebugModeReset ) {
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

			
		////////////////

		public override void PreUpdate() {
			if( !DynamicInvasionsMod.Config.Enabled ) { return; }

			if( Main.netMode == 2 ) {	// Server
				this.Logic.Update();
			}
		}
	}
}
