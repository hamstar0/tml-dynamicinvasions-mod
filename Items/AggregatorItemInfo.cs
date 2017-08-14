using HamstarHelpers.ItemHelpers;
using HamstarHelpers.NPCHelpers;
using HamstarHelpers.Utilities.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace DynamicInvasions.Items {
	class AggregatorItemInfo : GlobalItem {
		public override bool InstancePerEntity { get { return true; } }
		

		public override GlobalItem Clone( Item item, Item item_clone ) {
			var clone = (AggregatorItemInfo)base.Clone( item, item_clone );
			clone.IsInitialized = this.IsInitialized;
			clone.MusicType = this.MusicType;
			clone.BannerItemTypesToNpcTypes = this.BannerItemTypesToNpcTypes;
			clone.Uses = this.Uses;
			return clone;
		}

		public override bool NeedsSaving( Item item ) {
			return this.IsInitialized && item.type == this.mod.ItemType<CrossDimensionalAggregatorItem>();
		}

		public override void Load( Item item, TagCompound tags ) {
			bool is_init = tags.GetBool( "is_init" );
			int music = tags.GetInt( "music_type" );
			var list = JsonConfig<List<KeyValuePair<int, ISet<int>>>>.Deserialize( tags.GetString( "spawn_npcs" ) );
			int uses = tags.GetInt( "uses" );

			this.IsInitialized = is_init;
			this.MusicType = music;
			this.BannerItemTypesToNpcTypes = list.AsReadOnly();
			this.Uses = uses;
		}

		public override TagCompound Save( Item item ) {
			string spawn_npc_enc = JsonConfig<IReadOnlyList<KeyValuePair<int, ISet<int>>>>.Serialize( this.BannerItemTypesToNpcTypes );

			return new TagCompound {
				{ "is_init", (bool)this.IsInitialized },
				{ "music_type", (int)this.MusicType },
				{ "spawn_npcs", (string)spawn_npc_enc },
				{ "uses", (int)this.Uses }
			};
		}

		public override void NetSend( Item item, BinaryWriter writer ) {
			string spawn_npc_enc = JsonConfig<IReadOnlyList<KeyValuePair<int, ISet<int>>>>.Serialize( this.BannerItemTypesToNpcTypes );

			writer.Write( (bool)this.IsInitialized );
			writer.Write( (int)this.MusicType );
			writer.Write( (string)spawn_npc_enc );
			writer.Write( (int)this.Uses );
		}

		public override void NetReceive( Item item, BinaryReader reader ) {
			bool is_init = reader.ReadBoolean();
			int music = reader.ReadInt32();
			var list = JsonConfig<List<KeyValuePair<int, ISet<int>>>>.Deserialize( reader.ReadString() );
			int uses = reader.ReadInt32();

			this.IsInitialized = is_init;
			this.MusicType = music;
			this.BannerItemTypesToNpcTypes = list.AsReadOnly();
			this.Uses = uses;
		}



		////////////////

		public bool IsInitialized { get; private set; }
		public int MusicType { get; private set; }
		public IReadOnlyList<KeyValuePair<int, ISet<int>>> BannerItemTypesToNpcTypes { get; private set; }
		public int Uses { get; private set; }


		public AggregatorItemInfo() : base() {
			this.IsInitialized = false;
			this.MusicType = 0;
			this.BannerItemTypesToNpcTypes = null;
			this.Uses = 0;
		}


		public void Initialize( int music_box_item_type, IList<int> banner_item_types ) {
			var list = new List<KeyValuePair<int, ISet<int>>>( banner_item_types.Count );

			foreach( int banner_item_type in banner_item_types ) {
				var npcs = NPCBannerHelpers.GetNpcTypesOfBannerItemType( banner_item_type );
				var kv = new KeyValuePair<int, ISet<int>>( banner_item_type, npcs );
				list.Add( kv );
			}

			this.IsInitialized = true;
			this.MusicType = ItemMusicBoxHelpers.GetMusicTypeOfMusicBox( music_box_item_type );
			this.BannerItemTypesToNpcTypes = new List<KeyValuePair<int, ISet<int>>>( list ).AsReadOnly();
		}


		public string[] GetNpcNames() {
			if( this.BannerItemTypesToNpcTypes == null ) { return new string[] { }; }

			string[] names = new string[ this.BannerItemTypesToNpcTypes.Count ];

			int i = 0;

			foreach( var kv in this.BannerItemTypesToNpcTypes ) {
				int npc_type = kv.Value.First();
				if( npc_type != 0 ) {
					NPC npc = new NPC();
					npc.SetDefaults( npc_type );
					names[i] = npc.TypeName;
				} else {
					names[i] = "-";
				}
				i++;
			}

			return names;
		}


		public void Use() {
			this.Uses++;
		}
	}
}
