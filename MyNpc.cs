using HamstarHelpers.Helpers.WorldHelpers;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions {
	class DynamicInvasionsNpc : GlobalNPC {
		public override void EditSpawnRate( Player player, ref int spawn_rate, ref int max_spawns ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return; }
			var modworld = this.mod.GetModWorld<DynamicInvasionsWorld>();

			if( modworld.Logic.HasInvasionFinishedArriving() && WorldHelpers.IsAboveWorldSurface( player.position ) ) {
				spawn_rate = mymod.ConfigJson.Data.InvasionSpawnRate;
				max_spawns = mymod.ConfigJson.Data.InvasionSpawnMax;
			}
		}

		public override void EditSpawnPool( IDictionary<int, float> pool, NPCSpawnInfo spawn_info ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return; }
			var modworld = this.mod.GetModWorld<DynamicInvasionsWorld>();

			if( modworld.Logic.HasInvasionFinishedArriving() && WorldHelpers.IsAboveWorldSurface( spawn_info.player.position ) ) {
				modworld.Logic.EditSpawnPool( pool, spawn_info );
			}
		}


		public override bool PreNPCLoot( NPC npc ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return base.PreNPCLoot( npc ); }
			var modworld = this.mod.GetModWorld<DynamicInvasionsWorld>();

			bool has_invasion_arrived = modworld.Logic.HasInvasionFinishedArriving();
			bool is_above_surface = WorldHelpers.IsAboveWorldSurface( npc.position );

			if( has_invasion_arrived && is_above_surface ) {
				float chance_percent = mymod.ConfigJson.Data.InvaderLootDropPercentChance;
				return Main.rand.NextFloat() < chance_percent;
			}
			return base.PreNPCLoot( npc );
		}


		public override bool CheckDead( NPC npc ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return base.CheckDead(npc); }
			var modworld = this.mod.GetModWorld<DynamicInvasionsWorld>();

			if( modworld.Logic.HasInvasionFinishedArriving() && WorldHelpers.IsAboveWorldSurface(npc.position) ) {
				if( npc.life <= 0 ) {
					modworld.Logic.InvaderKilled( npc );
				}
			}
			
			return base.CheckDead( npc );
		}
	}
}
