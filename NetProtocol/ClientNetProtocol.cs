using HamstarHelpers.DebugHelpers;
using HamstarHelpers.Utilities.Config;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions.NetProtocol {
	public enum ClientNetProtocolTypes : byte {
		ModSettings=128,
		Invasion,
		InvasionStatus
	}


	public static class ClientNetProtocol {
		public static void RoutePacket( DynamicInvasions mymod, BinaryReader reader ) {
			ClientNetProtocolTypes protocol = (ClientNetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case ClientNetProtocolTypes.ModSettings:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "RouteReceivedClientPackets.ModSettings" ); }
				ClientNetProtocol.ReceiveModSettingsOnClient( mymod, reader );
				break;
			case ClientNetProtocolTypes.Invasion:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "RouteReceivedClientPackets.Invasion" ); }
				ClientNetProtocol.ReceiveInvasionOnClient( mymod, reader );
				break;
			case ClientNetProtocolTypes.InvasionStatus:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "RouteReceivedClientPackets.InvasionStatus" ); }
				ClientNetProtocol.ReceiveInvasionStatusOnClient( mymod, reader );
				break;
			default:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "RouteReceivedClientPackets ...? "+protocol ); }
				break;
			}
		}



		////////////////
		// Client Senders
		////////////////

		public static void SendModSettingsRequestFromClient( DynamicInvasions mymod ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)ServerNetProtocolTypes.RequestModSettings );

			packet.Send();
		}
		
		public static void SendInvasionRequestFromClient( DynamicInvasions mymod, int music_type, IReadOnlyList<KeyValuePair<int, ISet<int>>> spawn_info ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();
			string spawn_info_enc = JsonConfig<IReadOnlyList<KeyValuePair<int, ISet<int>>>>.Serialize( spawn_info );

			packet.Write( (byte)ServerNetProtocolTypes.RequestInvasion );
			packet.Write( (int)music_type );
			packet.Write( (string)spawn_info_enc );
			
			packet.Send();
		}

		public static void SendInvasionStatusRequestFromClient( DynamicInvasions mymod ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)ServerNetProtocolTypes.RequestInvasionStatus );

			packet.Send();
		}



		////////////////
		// Client Receivers
		////////////////

		private static void ReceiveModSettingsOnClient( DynamicInvasions mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }
			
			mymod.Config.DeserializeMe( reader.ReadString() );
		}

		private static void ReceiveInvasionOnClient( DynamicInvasions mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			int music_type = reader.ReadInt32();
			string spawn_info_enc = reader.ReadString();
			var spawn_info = JsonConfig<List<KeyValuePair<int, ISet<int>>>>.Deserialize( spawn_info_enc );

			var modworld = mymod.GetModWorld<MyModWorld>();
			modworld.Logic.StartInvasion( mymod, music_type, spawn_info.AsReadOnly() );
		}

		private static void ReceiveInvasionStatusOnClient( DynamicInvasions mymod, BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }
			
			var modworld = mymod.GetModWorld<MyModWorld>();

			modworld.Logic.MyNetReceive( reader );
		}
	}
}
