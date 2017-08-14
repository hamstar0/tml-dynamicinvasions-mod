using DynamicInvasions.Items;
using HamstarHelpers.DebugHelpers;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions.Invasion {
	class InvasionLogic : InvasionInfo {
		public static Texture2D ProgressBarTexture { get; private set; }

		public static void ModLoad( DynamicInvasions mymod ) {
			if( InvasionLogic.ProgressBarTexture == null && !Main.dedServ ) {	// Client
				InvasionLogic.ProgressBarTexture = mymod.GetTexture( "InvasionIcon" );
			}
		}

		
		////////////////

		public InvasionLogic() : base() { }

		////////////////

		public bool CanStartInvasion( DynamicInvasions mymod, AggregatorItemInfo item_info ) {
			if( !item_info.IsInitialized ) {
				return false;
			}

			if( !this.IsInvading && Main.invasionDelay > 0 ) {
				Main.invasionDelay = 0;	// Failsafe?
			}
			
			int old_max_life = Main.LocalPlayer.statLifeMax;

			Main.LocalPlayer.statLifeMax = 200;
			bool can_start = Main.CanStartInvasion();
			Main.LocalPlayer.statLifeMax = old_max_life;
			
			return can_start;
		}

		public bool HasInvasionFinishedArriving() {
			return this.IsInvading && this.InvasionEnrouteDuration == 0;
		}


		public void StartInvasion( DynamicInvasions mymod, int music_type, IReadOnlyList<KeyValuePair<int, ISet<int>>> spawn_info ) {
			Main.invasionDelay = 2; // Lightweight invasion

			var list = spawn_info.SelectMany( id => id.Value ).ToList();

			if( mymod.IsDebugInfoMode() ) {
				string str = string.Join( ",", list.ToArray() );
				DebugHelpers.Log( "starting invasion music: " + music_type + ", npcs: " + str );
			}
			
			this.IsInvading = true;
			this.MusicType = music_type;
			this._SpawnNpcTypeList = list;
			this.SpawnNpcTypeList = list.AsReadOnly();

			int invadable_player_count = 0;
			for( int i = 0; i < 255; ++i ) {
				if( Main.player[i].active && Main.player[i].statLifeMax >= 200 ) {
					++invadable_player_count;
				}
			}
			if( invadable_player_count == 0 ) {
				invadable_player_count = 1;
			}

			if( mymod.IsCheatMode() ) {
				this.InvasionSize = 30;
			} else {
				int base_amt = mymod.Config.Data.InvasionMinSize;
				int per_player_amt = mymod.Config.Data.InvasionAddedSizePerStrongPlayer;
				this.InvasionSize = base_amt + (per_player_amt * invadable_player_count);
			}
			this.InvasionSizeStart = this.InvasionSize;

			this.InvasionEnrouteDuration = 60 * mymod.Config.Data.InvasionArrivalTimeInSeconds;
			this.InvasionEnrouteWarningDuration = 0;
			this.InvasionProgressIntroAnimation = 0;
			this.ProgressMeterIntroZoom = 0.0f;
		}

		
		public void EndInvasion() {
			this.IsInvading = false;
			this.InvasionSize = 0;
			this.InvasionSizeStart = 0;
			this.InvasionEnrouteDuration = 0;
			this.MusicType = 0;

			this.InvasionWarning( "The dimensional breach has closed." );
		}



		////////////////

		public void InvaderKilled( NPC npc ) {
			if( !this.SpawnNpcTypeList.Contains(npc.type) ) { return; }

			this.InvasionSize--;

			if( this.InvasionSize <= 0 ) {
				this.EndInvasion();
			}
		}

		////////////////

		public void Update( DynamicInvasions mymod ) {
			if( mymod.IsDebugInfoMode() ) {
				DebugHelpers.Display["info"] = "IsInvading: "+this.IsInvading+
					", : enroute: "+this.InvasionEnrouteDuration+
					", size: "+this.InvasionSize+
					", max: "+this.InvasionSizeStart;
			}

			if( this.IsInvading ) {
				Main.invasionDelay = 2; // Lightweight invasion

				if( this.InvasionEnrouteDuration > 0 ) {
					this.InvasionEnrouteDuration--;
					if( this.InvasionEnrouteWarningDuration == 0 ) {
						this.InvasionEnrouteWarningDuration = 12 * 60;
						this.InvasionWarning( "A dimensional breach is growing..." );
					} else {
						this.InvasionEnrouteWarningDuration--;
					}
				}

				if( this.InvasionEnrouteDuration == 1 ) {
					this.InvasionEnrouteDuration = 0;
					this.InvasionWarning( "A dimensional breach has arrived!" );
				}
			} else {
				Main.invasionDelay = 0;
			}
		}

		////////////////

		public void EditSpawnPool( IDictionary<int, float> pool, NPCSpawnInfo spawn_info ) {
			foreach( int npc_type in this.SpawnNpcTypeList ) {
				if( !pool.ContainsKey(npc_type) ) { pool[npc_type] = 1000f; }
				else { pool[npc_type] += 1000f; }
			}
		}
	}
}
