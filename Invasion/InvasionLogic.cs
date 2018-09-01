using HamstarHelpers.Helpers.DebugHelpers;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace DynamicInvasions.Invasion {
	partial class InvasionLogic {
		public static Texture2D ProgressBarTexture { get; private set; }

		internal static void ModLoad( DynamicInvasionsMod mymod ) {
			if( InvasionLogic.ProgressBarTexture == null && !Main.dedServ ) {	// Not server
				InvasionLogic.ProgressBarTexture = mymod.GetTexture( "InvasionIcon" );
			}
		}


		////////////////

		private InvasionData Data;
		//private AutomaticInvasions Auto = null;


		internal InvasionLogic( DynamicInvasionsMod mymod ) {
			this.Data = new InvasionData( mymod );

			//if( Main.netMode == 0 || Main.netMode == 2 ) {
			//	this.Auto = new AutomaticInvasions();
			//}
		}


		////////////////

		internal void LoadMe( TagCompound tags ) { this.Data.LoadMe( tags ); }
		internal TagCompound SaveMe() { return this.Data.SaveMe(); }
		internal void MyNetSend( BinaryWriter writer ) { this.Data.MyNetSend( writer ); }
		internal void MyNetReceive( BinaryReader reader ) { this.Data.MyNetReceive( reader ); }

		////////////////

		public bool CanStartInvasion( DynamicInvasionsMod mymod ) {
			if( !this.Data.IsInvading && Main.invasionDelay > 0 ) {
				Main.invasionDelay = 0;	// Failsafe?
			}
			
			int old_max_life = Main.LocalPlayer.statLifeMax;

			Main.LocalPlayer.statLifeMax = 200;
			bool can_start = Main.CanStartInvasion();
			Main.LocalPlayer.statLifeMax = old_max_life;
			
			return can_start;
		}

		public bool HasInvasionFinishedArriving() {
			return this.Data.IsInvading && this.Data.InvasionEnrouteDuration == 0;
		}


		public void StartInvasion( DynamicInvasionsMod mymod, int music_type, IReadOnlyList<KeyValuePair<int, ISet<int>>> spawn_info ) {
			Main.invasionDelay = 2; // Lightweight invasion
			var spawn_npcs = spawn_info.SelectMany( id => id.Value ).ToList();
			int size = 0;

			if( mymod.Config.DebugModeInfo ) {
				string str = string.Join( ",", spawn_npcs.ToArray() );
				LogHelpers.Log( "starting invasion music: " + music_type + ", npcs: " + str );
			}
			
			int invadable_player_count = 0;
			for( int i = 0; i < 255; ++i ) {
				if( Main.player[i].active && Main.player[i].statLifeMax >= 200 ) {
					++invadable_player_count;
				}
			}
			if( invadable_player_count == 0 ) {
				invadable_player_count = 1;
			}

			if( mymod.Config.DebugModeCheat ) {
				size = 30;
			} else {
				int base_amt = mymod.ConfigJson.Data.InvasionMinSize;
				int per_player_amt = mymod.ConfigJson.Data.InvasionAddedSizePerStrongPlayer;
				size = base_amt + (per_player_amt * invadable_player_count);
			}

			this.Data.Initialize( true, size, size, 60 * mymod.ConfigJson.Data.InvasionArrivalTimeInSeconds, 0, 0, music_type, spawn_npcs );
		}

		
		public void EndInvasion() {
			this.Data.EndInvasion();
			this.InvasionWarning( "The dimensional breach has closed." );
		}
		
		////////////////

		public void InvaderKilled( NPC npc ) {
			if( !this.Data.SpawnNpcTypeList.Contains( npc.type ) ) { return; }

			this.Data.InvasionSize--;

			if( this.Data.InvasionSize <= 0 ) {
				this.EndInvasion();
			}
		}


		////////////////

		public void Update( DynamicInvasionsMod mymod ) {
			if( mymod.Config.DebugModeInfo ) {
				DebugHelpers.Print( "DynamicInvasionInfo", "IsInvading: "+this.Data.IsInvading+
					", : enroute: "+this.Data.InvasionEnrouteDuration+
					", size: "+this.Data.InvasionSize+
					", max: "+this.Data.InvasionSizeStart,
					20
				);
			}

			if( this.Data.IsInvading ) {
				Main.invasionDelay = 2; // Lightweight invasion

				if( this.Data.InvasionEnrouteDuration > 0 ) {
					this.Data.InvasionEnrouteDuration--;
					if( this.Data.InvasionEnrouteWarningDuration == 0 ) {
						this.Data.InvasionEnrouteWarningDuration = 12 * 60;
						this.InvasionWarning( "A dimensional breach is growing..." );
					} else {
						this.Data.InvasionEnrouteWarningDuration--;
					}
				}

				if( this.Data.InvasionEnrouteDuration == 1 ) {
					this.Data.InvasionEnrouteDuration = 0;
					this.InvasionWarning( "A dimensional breach has arrived!" );
				}
			} else {
				Main.invasionDelay = 0;
			}
		}


		public void UpdateMusic( ref int music ) {
			if( this.HasInvasionFinishedArriving() ) {
				music = this.Data.MusicType;
			}
		}

		////////////////

		public void EditSpawnPool( IDictionary<int, float> pool, NPCSpawnInfo spawn_info ) {
			foreach( int npc_type in this.Data.SpawnNpcTypeList ) {
				if( !pool.ContainsKey(npc_type) ) { pool[npc_type] = 1000f; }
				else { pool[npc_type] += 1000f; }
			}
		}
	}
}
