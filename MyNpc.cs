using HamstarHelpers.Helpers.WorldHelpers;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions {
	class DynamicInvasionsNpc : GlobalNPC {
		public override void EditSpawnRate( Player player, ref int spawnRate, ref int maxSpawns ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return; }
			var modworld = this.mod.GetModWorld<DynamicInvasionsWorld>();

			if( modworld.Logic.HasInvasionFinishedArriving() && WorldHelpers.IsAboveWorldSurface( player.position ) ) {
				spawnRate = mymod.ConfigJson.Data.InvasionSpawnRate;
				maxSpawns = mymod.ConfigJson.Data.InvasionSpawnMax;
			}
		}

		public override void EditSpawnPool( IDictionary<int, float> pool, NPCSpawnInfo spawnInfo ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return; }
			var modworld = this.mod.GetModWorld<DynamicInvasionsWorld>();

			if( modworld.Logic.HasInvasionFinishedArriving() && WorldHelpers.IsAboveWorldSurface( spawnInfo.player.position ) ) {
				modworld.Logic.EditSpawnPool( pool, spawnInfo );
			}
		}


		public override bool PreNPCLoot( NPC npc ) {
			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.ConfigJson.Data.Enabled ) { return base.PreNPCLoot( npc ); }
			var modworld = this.mod.GetModWorld<DynamicInvasionsWorld>();

			bool hasInvasionArrived = modworld.Logic.HasInvasionFinishedArriving();
			bool isAboveSurface = WorldHelpers.IsAboveWorldSurface( npc.position );

			if( hasInvasionArrived && isAboveSurface ) {
				float chancePercent = mymod.ConfigJson.Data.InvaderLootDropPercentChance;
				return Main.rand.NextFloat() < chancePercent;
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
