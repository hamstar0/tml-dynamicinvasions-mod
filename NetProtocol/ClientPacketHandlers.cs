using HamstarHelpers.Helpers.Debug;
using Newtonsoft.Json;
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
			case NetProtocolTypes.Invasion:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ClientPacketHandlers.Invasion" ); }
				ClientPacketHandlers.ReceiveInvasionOnClient( reader );
				break;
			case NetProtocolTypes.InvasionStatus:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ClientPacketHandlers.InvasionStatus" ); }
				ClientPacketHandlers.ReceiveInvasionStatusOnClient( reader );
				break;
			case NetProtocolTypes.EndInvasion:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ClientPacketHandlers.EndInvasion" ); }
				ClientPacketHandlers.ReceiveEndInvasionOnClient( reader );
				break;
			default:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ClientPacketHandlers ...? " + protocol ); }
				break;
			}
		}



		////////////////
		// Client Senders
		////////////////
		
		public static void SendInvasionRequestFromClient( int musicType, IReadOnlyList<KeyValuePair<int, ISet<int>>> spawnInfo ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			ModPacket packet = mymod.GetPacket();
			string spawnInfoEnc = JsonConvert.SerializeObject( spawnInfo );

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

		public static void SendEndInvasionRequestFromClient() {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.RequestEndInvasion );

			packet.Send();
		}



		////////////////
		// Client Receivers
		////////////////

		private static void ReceiveInvasionOnClient( BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			int musicType = reader.ReadInt32();
			string spawnInfoEnc = reader.ReadString();
			var spawnInfo = JsonConvert.DeserializeObject<List<KeyValuePair<int, ISet<int>>>>( spawnInfoEnc );

			var modworld = ModContent.GetInstance<DynamicInvasionsWorld>();
			modworld.Logic.StartInvasion( musicType, spawnInfo.AsReadOnly() );
		}

		private static void ReceiveInvasionStatusOnClient( BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var modworld = ModContent.GetInstance<DynamicInvasionsWorld>();

			modworld.Logic.MyNetReceive( reader );
		}
		
		private static void ReceiveEndInvasionOnClient( BinaryReader reader ) {
			// Clients only
			if( Main.netMode != 1 ) { return; }

			var modworld = ModContent.GetInstance<DynamicInvasionsWorld>();

			modworld.Logic.EndInvasion();
		}
	}
}
