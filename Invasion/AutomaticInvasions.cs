using HamstarHelpers.WorldHelpers;
using System;
using Terraria;
using Terraria.ModLoader;

namespace DynamicInvasions.Invasion {
	public partial class AutomaticInvasions {
		public AutomaticInvasions() {
			WorldHelpers.AddDayHook( "dynamic_invasion", new Action( delegate {
				var mymod = (DynamicInvasionsMod)ModLoader.GetMod("DynamicInvasions");

				if( Main.rand.Next( mymod.Config.Data.AutoInvasionAverageDays ) == 0 ) {
					this.GenerateInvasion();
				}
			} ) );
		}


		public void GenerateInvasion() {
			//TODO
		}
	}
}
