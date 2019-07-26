using HamstarHelpers.Helpers.Debug;
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



		////////////////

		internal static void ModLoad() {
			var mymod = DynamicInvasionsMod.Instance;

			if( InvasionLogic.ProgressBarTexture == null && !Main.dedServ ) {	// Not server
				InvasionLogic.ProgressBarTexture = mymod.GetTexture( "InvasionIcon" );
			}
		}


		////////////////

		private InvasionData Data;
		//private AutomaticInvasions Auto = null;



		////////////////

		internal InvasionLogic() {
			this.Data = new InvasionData();

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

		public bool IsInvasionHappening() {
			return this.Data.IsInvading;
		}


		public bool CanStartInvasion() {
			if( !this.IsInvasionHappening() && Main.invasionDelay > 0 ) {
				Main.invasionDelay = 0;	// Failsafe?
			}
			
			int oldMaxLife = Main.LocalPlayer.statLifeMax;

			Main.LocalPlayer.statLifeMax = 200;
			bool canStart = Main.CanStartInvasion();
			Main.LocalPlayer.statLifeMax = oldMaxLife;
			
			return canStart;
		}

		public bool HasInvasionFinishedArriving() {
			return this.IsInvasionHappening() && this.Data.InvasionEnrouteDuration == 0;
		}


		public void StartInvasion( int musicType, IReadOnlyList<KeyValuePair<int, ISet<int>>> spawnInfo ) {
			var mymod = DynamicInvasionsMod.Instance;

			Main.invasionDelay = 2; // Lightweight invasion
			var spawnNpcs = spawnInfo.SelectMany( id => id.Value ).ToList();
			int size = 0;

			if( mymod.Config.DebugModeInfo ) {
				string str = string.Join( ",", spawnNpcs.ToArray() );
				LogHelpers.Log( "starting invasion music: " + musicType + ", npcs: " + str );
			}
			
			int invadablePlayerCount = 0;
			for( int i = 0; i < 255; ++i ) {
				if( Main.player[i].active && Main.player[i].statLifeMax >= 200 ) {
					++invadablePlayerCount;
				}
			}
			if( invadablePlayerCount == 0 ) {
				invadablePlayerCount = 1;
			}

			if( mymod.Config.DebugModeCheat ) {
				size = 30;
			} else {
				int baseAmt = mymod.Config.InvasionMinSize;
				int perPlayerAmt = mymod.Config.InvasionAddedSizePerStrongPlayer;
				size = baseAmt + (perPlayerAmt * invadablePlayerCount);
			}

			this.Data.Initialize( true, size, size, 60 * mymod.Config.InvasionArrivalTimeInSeconds, 0, 0, musicType, spawnNpcs );
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

		public void Update() {
			var mymod = DynamicInvasionsMod.Instance;

			if( mymod.Config.DebugModeInfo ) {
				DebugHelpers.Print( "DynamicInvasionInfo", "IsInvading: "+this.Data.IsInvading+
					", : enroute: "+this.Data.InvasionEnrouteDuration+
					", size: "+this.Data.InvasionSize+
					", max: "+this.Data.InvasionSizeStart,
					20
				);
			}

			if( this.IsInvasionHappening() ) {
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

		public void EditSpawnPool( IDictionary<int, float> pool, NPCSpawnInfo spawnInfo ) {
			var mymod = DynamicInvasionsMod.Instance;

			foreach( int npcType in this.Data.SpawnNpcTypeList ) {
				if( !pool.ContainsKey(npcType) ) { pool[npcType] = mymod.Config.InvasionSpawnRatePerType; }
				else { pool[npcType] += mymod.Config.InvasionSpawnRatePerType; }
			}
		}
	}
}
