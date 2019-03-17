using HamstarHelpers.Components.Config;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;


namespace DynamicInvasions.Invasion {
	class InvasionData {
		public bool IsInvading;
		public int InvasionSize;
		public int InvasionSizeStart;

		public string Label;
		public Color LabelColor;

		public int InvasionEnrouteDuration;
		public int InvasionEnrouteWarningDuration;

		public int InvasionProgressIntroAnimation;
		public float ProgressMeterIntroZoom;

		public int MusicType;

		public IList<int> SpawnNpcTypeList { get; private set; }



		////////////////

		public InvasionData() {
			var list = new List<int>();

			this.IsInvading = false;
			this.Label = "Dimensional Breach";
			this.LabelColor = Color.White;
			this.SpawnNpcTypeList = list;

			this.InvasionSize = 0;
			this.InvasionSizeStart = 0;
			this.InvasionEnrouteDuration = 0;
			this.InvasionEnrouteWarningDuration = 0;
			this.InvasionProgressIntroAnimation = 0;
			this.MusicType = 0;

			this.ProgressMeterIntroZoom = 0;
		}


		public void Initialize( bool isInvading, int size, int startSize, int enrouteTime, int warnTime, int introTime, int musicType, string spawnNpcsEnc ) {
			var spawnNpcs = JsonConfig<IList<int>>.Deserialize( spawnNpcsEnc );
			this.Initialize( isInvading, size, startSize, enrouteTime, warnTime, introTime, musicType, spawnNpcs );
		}

		public void Initialize( bool isInvading, int size, int startSize, int enrouteTime, int warnTime, int introTime, int musicType, IList<int> spawnNpcs ) {
			this.IsInvading = isInvading;
			this.InvasionSize = size;
			this.InvasionSizeStart = startSize;
			this.InvasionEnrouteDuration = enrouteTime;
			this.InvasionEnrouteWarningDuration = warnTime;
			this.InvasionProgressIntroAnimation = introTime;
			this.MusicType = musicType;
			this.SpawnNpcTypeList = spawnNpcs;
		}


		////////////////

		public void LoadMe( TagCompound tags ) {
			if( !tags.ContainsKey( "is_invading" ) ) { return; }

			bool isInvading = tags.GetBool( "is_invading" );
			int size = tags.GetInt( "size" );
			int startSize = tags.GetInt( "start_size" );
			int enrouteTime = tags.GetInt( "enroute_time" );
			int warnTime = tags.GetInt( "warn_time" );
			int introTime = tags.GetInt( "intro_time" );
			int musicType = tags.GetInt( "music_type" );
			string spawnNpcs = tags.GetString( "spawn_npcs" );

			this.Initialize( isInvading, size, startSize, enrouteTime, warnTime, introTime, musicType, spawnNpcs );
		}

		public TagCompound SaveMe() {
			return new TagCompound { { "is_invading", this.IsInvading },
				{ "size", this.InvasionSize },
				{ "start_size", this.InvasionSizeStart },
				{ "enroute_time", this.InvasionEnrouteDuration },
				{ "warn_time", this.InvasionEnrouteWarningDuration },
				{ "intro_time", this.InvasionProgressIntroAnimation },
				{ "music_type", this.MusicType },
				{ "spawn_npcs", JsonConfig<IList<int>>.Serialize( this.SpawnNpcTypeList ) }
			};
		}

		public void MyNetSend( BinaryWriter writer ) {
			writer.Write( (bool)this.IsInvading );
			writer.Write( (int)this.InvasionSize );
			writer.Write( (int)this.InvasionSizeStart );
			writer.Write( (int)this.InvasionEnrouteDuration );
			writer.Write( (int)this.InvasionEnrouteWarningDuration );
			writer.Write( (int)this.InvasionProgressIntroAnimation );
			writer.Write( (int)this.MusicType );
			writer.Write( (string)JsonConfig<IList<int>>.Serialize( this.SpawnNpcTypeList ) );
		}

		public void MyNetReceive( BinaryReader reader ) {
			bool isInvading = reader.ReadBoolean();
			int size = reader.ReadInt32();
			int startSize = reader.ReadInt32();
			int enrouteTime = reader.ReadInt32();
			int warnTime = reader.ReadInt32();
			int introTime = reader.ReadInt32();
			int musicType = reader.ReadInt32();
			string spawnNpcs = reader.ReadString();

			this.Initialize( isInvading, size, startSize, enrouteTime, warnTime, introTime, musicType, spawnNpcs );
		}


		////////////////

		public void EndInvasion() {
			this.IsInvading = false;
			this.InvasionSize = 0;
			this.InvasionSizeStart = 0;
			this.InvasionEnrouteDuration = 0;
			this.MusicType = 0;
		}
	}
}
