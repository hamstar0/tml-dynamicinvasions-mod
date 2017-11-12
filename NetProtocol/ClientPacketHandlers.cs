using HamstarHelpers.DebugHelpers;
using HamstarHelpers.Utilities.Config;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions.NetProtocol {
	public static class ClientPacketHandlers {
		public static void HandlePacket( DynamicInvasionsMod mymod, BinaryReader reader ) {
			NetProtocolTypes protocol = (NetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case NetProtocolTypes.ModSettings:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "ClientPacketHandlers.ModSettings" ); }
				ClientPacketHandlers.ReceiveModSettingsOnClient( mymod, reader );
				break;
			case NetProtocolTypes.Invasion:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "ClientPacketHandlers.Invasion" ); }
				ClientPacketHandlers.ReceiveInvasionOnClient( mymod, reader );
				break;
			case NetProtocolTypes.InvasionStatus:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "ClientPacketHandlers.InvasionStatus" ); }
				ClientPacketHandlers.ReceiveInvasionStatusOnClient( mymod, reader );
				break;
			default:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "ClientPacketHandlers ...? " + protocol ); }
				break;
			}
		}



		////////////////
		// Client Senders
		////////////////

		public static void SendModSettingsRequestFromClient( DynamicInvasionsMod mymod ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.RequestModSettings );

			packet.Send();
		}
		
		public static void SendInvasionRequestFromClient( DynamicInvasionsMod mymod, int music_type, IReadOnlyList<KeyValuePair<int, ISet<int>>> spawn_info ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();
			string spawn_info_enc = JsonConfig<IReadOnlyList<KeyValuePair<int, ISet<int>>>>.Serialize( spawn_info );

			packet.Write( (byte)NetProtocolTypes.RequestInvasion );
			packet.Write( (int)music_type );
			packet.Write( (string)spawn_info_enc );
			
			packet.Send();
		}

		public static void SendInvasionStatusRequestFromClient( DynamicInvasionsMod mymod ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.RequestInvasionStatus );

			packet.Send();
		}



		////////////////
		// Client Receivers
		////////////////

		private static void ReceiveModSettingsOnClient( DynamicInvasionsMod mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }
			
			mymod.Config.DeserializeMe( reader.ReadString() );
		}

		private static void ReceiveInvasionOnClient( DynamicInvasionsMod mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			int music_type = reader.ReadInt32();
			string spawn_info_enc = reader.ReadString();
			var spawn_info = JsonConfig<List<KeyValuePair<int, ISet<int>>>>.Deserialize( spawn_info_enc );

			var modworld = mymod.GetModWorld<MyModWorld>();
			modworld.Logic.StartInvasion( mymod, music_type, spawn_info.AsReadOnly() );
		}

		private static void ReceiveInvasionStatusOnClient( DynamicInvasionsMod mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }
			
			var modworld = mymod.GetModWorld<MyModWorld>();

			modworld.Logic.MyNetReceive( reader );
		}
	}
}
