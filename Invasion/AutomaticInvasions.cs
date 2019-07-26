using System;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions.Invasion {
	partial class AutomaticInvasions {
		public AutomaticInvasions() {
			HamstarHelpers.Services.Hooks.WorldHooks.WorldTimeHooks.AddDayHook( "dynamic_invasion", () => {
				var mymod = (DynamicInvasionsMod)ModLoader.GetMod("DynamicInvasions");

				if( Main.rand.Next( mymod.Config.AutoInvasionAverageDays ) == 0 ) {
					this.GenerateInvasion();
				}
			} );
		}


		public void GenerateInvasion() {
			//TODO
		}
	}
}
