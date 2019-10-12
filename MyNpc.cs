using HamstarHelpers.Helpers.World;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions {
	class DynamicInvasionsNpc : GlobalNPC {
		public override void EditSpawnRate( Player player, ref int spawnRate, ref int maxSpawns ) {
			if( Main.gameMenu ) { return; }

			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.Config.Enabled ) { return; }
			var myworld = ModContent.GetInstance<DynamicInvasionsWorld>();

			if( myworld.Logic.HasInvasionFinishedArriving() && WorldHelpers.IsAboveWorldSurface( player.position ) ) {
				spawnRate = mymod.Config.InvasionSpawnRate;
				maxSpawns = mymod.Config.InvasionSpawnMax;
			}
		}

		public override void EditSpawnPool( IDictionary<int, float> pool, NPCSpawnInfo spawnInfo ) {
			if( Main.gameMenu ) { return; }

			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.Config.Enabled ) { return; }
			var myworld = ModContent.GetInstance<DynamicInvasionsWorld>();

			if( myworld.Logic.HasInvasionFinishedArriving() && WorldHelpers.IsAboveWorldSurface( spawnInfo.player.position ) ) {
				myworld.Logic.EditSpawnPool( pool, spawnInfo );
			}
		}


		public override bool PreNPCLoot( NPC npc ) {
			if( Main.gameMenu ) { return base.PreNPCLoot( npc ); }

			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.Config.Enabled ) { return base.PreNPCLoot( npc ); }

			var myworld = ModContent.GetInstance<DynamicInvasionsWorld>();

			bool hasInvasionArrived = myworld.Logic.HasInvasionFinishedArriving();
			bool isAboveSurface = WorldHelpers.IsAboveWorldSurface( npc.position );

			if( hasInvasionArrived && isAboveSurface ) {
				float chancePercent = mymod.Config.InvaderLootDropPercentChance;
				return Main.rand.NextFloat() < chancePercent;
			}
			return base.PreNPCLoot( npc );
		}


		public override bool CheckDead( NPC npc ) {
			if( Main.gameMenu ) { return base.CheckDead( npc ); }

			var mymod = (DynamicInvasionsMod)this.mod;
			if( !mymod.Config.Enabled ) { return base.CheckDead(npc); }
			var modworld = ModContent.GetInstance<DynamicInvasionsWorld>();

			if( modworld.Logic.HasInvasionFinishedArriving() && WorldHelpers.IsAboveWorldSurface(npc.position) ) {
				if( npc.life <= 0 ) {
					modworld.Logic.InvaderKilled( npc );
				}
			}
			
			return base.CheckDead( npc );
		}
	}
}
