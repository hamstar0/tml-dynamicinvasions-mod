using System;
using Terraria;


namespace DynamicInvasions.Invasion {
	partial class AutomaticInvasions {
		public AutomaticInvasions() {
			HamstarHelpers.Services.Hooks.WorldHooks.WorldTimeHooks.AddDayHook( "dynamic_invasion", () => {
				if( Main.rand.Next( DynamicInvasionsMod.Config.AutoInvasionAverageDays ) == 0 ) {
					this.GenerateInvasion();
				}
			} );
		}


		public void GenerateInvasion() {
			//TODO
		}
	}
}
