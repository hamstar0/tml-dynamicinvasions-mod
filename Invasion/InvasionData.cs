using HamstarHelpers.Utilities.Config;
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



		public InvasionData( DynamicInvasionsMod mymod ) {
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


		public void Initialize( bool is_invading, int size, int start_size, int enroute_time, int warn_time, int intro_time, int music_type, string spawn_npcs_enc ) {
			var spawn_npcs = JsonConfig<IList<int>>.Deserialize( spawn_npcs_enc );
			this.Initialize( is_invading, size, start_size, enroute_time, warn_time, intro_time, music_type, spawn_npcs );
		}

		public void Initialize( bool is_invading, int size, int start_size, int enroute_time, int warn_time, int intro_time, int music_type, IList<int> spawn_npcs ) {
			this.IsInvading = is_invading;
			this.InvasionSize = size;
			this.InvasionSizeStart = start_size;
			this.InvasionEnrouteDuration = enroute_time;
			this.InvasionEnrouteWarningDuration = warn_time;
			this.InvasionProgressIntroAnimation = intro_time;
			this.MusicType = music_type;
			this.SpawnNpcTypeList = spawn_npcs;
		}


		////////////////

		public void LoadMe( TagCompound tags ) {
			if( !tags.ContainsKey( "is_invading" ) ) { return; }

			bool is_invading = tags.GetBool( "is_invading" );
			int size = tags.GetInt( "size" );
			int start_size = tags.GetInt( "start_size" );
			int enroute_time = tags.GetInt( "enroute_time" );
			int warn_time = tags.GetInt( "warn_time" );
			int intro_time = tags.GetInt( "intro_time" );
			int music_type = tags.GetInt( "music_type" );
			string spawn_npcs = tags.GetString( "spawn_npcs" );

			this.Initialize( is_invading, size, start_size, enroute_time, warn_time, intro_time, music_type, spawn_npcs );
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
			bool is_invading = reader.ReadBoolean();
			int size = reader.ReadInt32();
			int start_size = reader.ReadInt32();
			int enroute_time = reader.ReadInt32();
			int warn_time = reader.ReadInt32();
			int intro_time = reader.ReadInt32();
			int music_type = reader.ReadInt32();
			string spawn_npcs = reader.ReadString();

			this.Initialize( is_invading, size, start_size, enroute_time, warn_time, intro_time, music_type, spawn_npcs );
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
