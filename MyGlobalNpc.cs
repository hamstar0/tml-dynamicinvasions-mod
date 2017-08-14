using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions {
	class MyGlobalNpc : GlobalNPC {
		public override void EditSpawnRate( Player player, ref int spawn_rate, ref int max_spawns ) {
			var mymod = (DynamicInvasions)this.mod;
			if( !mymod.Config.Data.Enabled ) { return; }
			var modworld = this.mod.GetModWorld<MyModWorld>();
			
			if( modworld.Logic.HasInvasionFinishedArriving() ) {
				spawn_rate = mymod.Config.Data.InvasionSpawnRate;
				max_spawns = mymod.Config.Data.InvasionSpawnMax;
			}
		}

		public override void EditSpawnPool( IDictionary<int, float> pool, NPCSpawnInfo spawn_info ) {
			var mymod = (DynamicInvasions)this.mod;
			if( !mymod.Config.Data.Enabled ) { return; }
			var modworld = this.mod.GetModWorld<MyModWorld>();

			if( modworld.Logic.HasInvasionFinishedArriving() ) {
				modworld.Logic.EditSpawnPool( pool, spawn_info );
			}
		}


		public override bool CheckDead( NPC npc ) {
			var mymod = (DynamicInvasions)this.mod;
			if( !mymod.Config.Data.Enabled ) { return base.CheckDead(npc); }
			var modworld = this.mod.GetModWorld<MyModWorld>();

			if( modworld.Logic.HasInvasionFinishedArriving() ) {
				if( npc.life <= 0 ) {
					modworld.Logic.InvaderKilled( npc );
				}
			}

			return base.CheckDead( npc );
		}
	}
}
