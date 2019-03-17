using HamstarHelpers.Components.Config;
using HamstarHelpers.Helpers.DebugHelpers;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions.NetProtocol {
	static class ClientPacketHandlers {
		public static void HandlePacket( BinaryReader reader ) {
			var mymod = DynamicInvasionsMod.Instance;
			NetProtocolTypes protocol = (NetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case NetProtocolTypes.ModSettings:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ClientPacketHandlers.ModSettings" ); }
				ClientPacketHandlers.ReceiveModSettingsOnClient( reader );
				break;
			case NetProtocolTypes.Invasion:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ClientPacketHandlers.Invasion" ); }
				ClientPacketHandlers.ReceiveInvasionOnClient( reader );
				break;
			case NetProtocolTypes.InvasionStatus:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ClientPacketHandlers.InvasionStatus" ); }
				ClientPacketHandlers.ReceiveInvasionStatusOnClient( reader );
				break;
			default:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ClientPacketHandlers ...? " + protocol ); }
				break;
			}
		}



		////////////////
		// Client Senders
		////////////////

		public static void SendModSettingsRequestFromClient() {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.RequestModSettings );

			packet.Send();
		}
		
		public static void SendInvasionRequestFromClient( int musicType, IReadOnlyList<KeyValuePair<int, ISet<int>>> spawnInfo ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			ModPacket packet = mymod.GetPacket();
			string spawnInfoEnc = JsonConfig<IReadOnlyList<KeyValuePair<int, ISet<int>>>>.Serialize( spawnInfo );

			packet.Write( (byte)NetProtocolTypes.RequestInvasion );
			packet.Write( (int)musicType );
			packet.Write( (string)spawnInfoEnc );
			
			packet.Send();
		}

		public static void SendInvasionStatusRequestFromClient() {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.RequestInvasionStatus );

			packet.Send();
		}



		////////////////
		// Client Receivers
		////////////////

		private static void ReceiveModSettingsOnClient( BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			bool success;
			
			mymod.ConfigJson.DeserializeMe( reader.ReadString(), out success );
		}

		private static void ReceiveInvasionOnClient( BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			int musicType = reader.ReadInt32();
			string spawnInfoEnc = reader.ReadString();
			var spawnInfo = JsonConfig<List<KeyValuePair<int, ISet<int>>>>.Deserialize( spawnInfoEnc );

			var modworld = mymod.GetModWorld<DynamicInvasionsWorld>();
			modworld.Logic.StartInvasion( mymod, musicType, spawnInfo.AsReadOnly() );
		}

		private static void ReceiveInvasionStatusOnClient( BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			var modworld = mymod.GetModWorld<DynamicInvasionsWorld>();

			modworld.Logic.MyNetReceive( reader );
		}
	}
}
