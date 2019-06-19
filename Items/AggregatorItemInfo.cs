using HamstarHelpers.Components.Config;
using HamstarHelpers.Helpers.ItemHelpers;
using HamstarHelpers.Helpers.NPCHelpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace DynamicInvasions.Items {
	class AggregatorItemInfo : GlobalItem {
		public static List<KeyValuePair<int, ISet<int>>> GetNpcSetsOfBanners( IList<int> bannerItemTypes ) {
			var list = new List<KeyValuePair<int, ISet<int>>>( bannerItemTypes.Count );

			foreach( int bannerItemType in bannerItemTypes ) {
				var npcs = NPCBannerHelpers.GetNpcTypesOfBannerItemType( bannerItemType );
				var kv = new KeyValuePair<int, ISet<int>>( bannerItemType, npcs );
				list.Add( kv );
			}

			return list;
		}




		////////////////

		public bool IsInitialized { get; private set; }
		public int MusicType { get; private set; }
		public IReadOnlyList<KeyValuePair<int, ISet<int>>> BannerItemTypesToNpcTypes { get; private set; }
		public int Uses { get; private set; }


		////////////////

		public override bool InstancePerEntity => true;



		////////////////

		public AggregatorItemInfo() : base() {
			this.IsInitialized = false;
			this.MusicType = 0;
			this.BannerItemTypesToNpcTypes = null;
			this.Uses = 0;
		}

		public override GlobalItem Clone( Item item, Item itemClone ) {
			var clone = (AggregatorItemInfo)base.Clone( item, itemClone );
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
			if( item.type != this.mod.ItemType<CrossDimensionalAggregatorItem>() ) { return; }

			bool isInit = tags.GetBool( "is_init" );
			int music = tags.GetInt( "music_type" );
			int uses = tags.GetInt( "uses" );
			string spawnNpcEnc = tags.GetString( "spawn_npcs" );

			IReadOnlyList<KeyValuePair<int, ISet<int>>> list = null;
			if( spawnNpcEnc != "" || isInit ) {
				var rawList = JsonConfig<List<KeyValuePair<int, ISet<int>>>>.Deserialize( spawnNpcEnc );
				list = rawList.AsReadOnly();
			}
			
			this.IsInitialized = isInit;
			this.MusicType = music;
			this.BannerItemTypesToNpcTypes = list;
			this.Uses = uses;
		}


		public override TagCompound Save( Item item ) {
			if( item.type != this.mod.ItemType<CrossDimensionalAggregatorItem>() ) { return new TagCompound(); }

			string spawnNpcEnc = "";
			if( this.BannerItemTypesToNpcTypes != null ) {
				spawnNpcEnc = JsonConfig<IReadOnlyList<KeyValuePair<int, ISet<int>>>>.Serialize( this.BannerItemTypesToNpcTypes );
			}

			return new TagCompound {
				{ "is_init", (bool)this.IsInitialized },
				{ "music_type", (int)this.MusicType },
				{ "uses", (int)this.Uses },
				{ "spawn_npcs", (string)spawnNpcEnc }
			};
		}


		public override void NetReceive( Item item, BinaryReader reader ) {
			if( item.type != this.mod.ItemType<CrossDimensionalAggregatorItem>() ) { return; }

			bool isInit = reader.ReadBoolean();
			int music = reader.ReadInt32();
			int uses = reader.ReadInt32();
			string spawnNpcEnc = reader.ReadString();

			IReadOnlyList<KeyValuePair<int, ISet<int>>> list = null;
			if( spawnNpcEnc != "" || isInit ) {
				var rawList = JsonConfig<List<KeyValuePair<int, ISet<int>>>>.Deserialize( spawnNpcEnc );
				list = rawList.AsReadOnly();
			}

			this.IsInitialized = isInit;
			this.MusicType = music;
			this.BannerItemTypesToNpcTypes = list;
			this.Uses = uses;
		}

		public override void NetSend( Item item, BinaryWriter writer ) {
			if( item.type != this.mod.ItemType<CrossDimensionalAggregatorItem>() ) { return; }

			string spawnNpcEnc = "";
			if( this.BannerItemTypesToNpcTypes != null ) {
				spawnNpcEnc = JsonConfig<IReadOnlyList<KeyValuePair<int, ISet<int>>>>.Serialize( this.BannerItemTypesToNpcTypes );
			}

			writer.Write( (bool)this.IsInitialized );
			writer.Write( (int)this.MusicType );
			writer.Write( (int)this.Uses );
			writer.Write( (string)spawnNpcEnc );
		}


		////////////////

		public void Initialize( int musicBoxItemType, IList<int> bannerItemTypes ) {
			var list = AggregatorItemInfo.GetNpcSetsOfBanners( bannerItemTypes );

			this.IsInitialized = true;
			this.MusicType = MusicBoxHelpers.GetMusicTypeOfVanillaMusicBox( musicBoxItemType );
			this.BannerItemTypesToNpcTypes = new List<KeyValuePair<int, ISet<int>>>( list ).AsReadOnly();
		}


		public string[] GetNpcNames() {
			if( this.BannerItemTypesToNpcTypes == null ) { return new string[] { }; }

			string[] names = new string[ this.BannerItemTypesToNpcTypes.Count ];

			int i = 0;

			foreach( var kv in this.BannerItemTypesToNpcTypes ) {
				int npcType = kv.Value.First();
				if( npcType != 0 ) {
					NPC npc = new NPC();
					npc.SetDefaults( npcType );
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
