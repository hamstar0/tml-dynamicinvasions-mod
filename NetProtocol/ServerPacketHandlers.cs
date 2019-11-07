using HamstarHelpers.Helpers.Debug;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions.NetProtocol {
	static class ServerPacketHandlers {
		public static void RoutePacket( BinaryReader reader, int playerWho ) {
			NetProtocolTypes protocol = (NetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case NetProtocolTypes.RequestInvasion:
				if( DynamicInvasionsMod.Config.DebugModeInfo ) { LogHelpers.Log( "ServerPacketHandlers.RequestInvasion" ); }
				ServerPacketHandlers.ReceiveInvasionRequestOnServer( reader, playerWho );
				break;
			case NetProtocolTypes.RequestInvasionStatus:
				if( DynamicInvasionsMod.Config.DebugModeInfo ) { LogHelpers.Log( "ServerPacketHandlers.RequestInvasionStatus" ); }
				ServerPacketHandlers.ReceiveInvasionStatusRequestOnServer( reader, playerWho );
				break;
			case NetProtocolTypes.RequestEndInvasion:
				if( DynamicInvasionsMod.Config.DebugModeInfo ) { LogHelpers.Log( "ServerPacketHandlers.RequestEndInvasion" ); }
				ServerPacketHandlers.ReceiveEndInvasionRequestOnServer( reader, playerWho );
				break;
			default:
				if( DynamicInvasionsMod.Config.DebugModeInfo ) { LogHelpers.Log( "ServerPacketHandlers ...? " + protocol ); }
				break;
			}
		}


		
		////////////////
		// Server Senders
		////////////////

		public static void SendInvasionFromServer( Player player, int musicType, string spawnInfoEnc ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.Invasion );
			packet.Write( (int)musicType );
			packet.Write( (string)spawnInfoEnc );

			packet.Send( (int)player.whoAmI );
		}

		public static void SendInvasionStatusFromServer( Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			ModPacket packet = mymod.GetPacket();
			var modworld = ModContent.GetInstance<DynamicInvasionsWorld>();

			packet.Write( (byte)NetProtocolTypes.InvasionStatus );
			modworld.Logic.MyNetSend( packet );

			packet.Send( (int)player.whoAmI );
		}
		
		public static void SendEndInvasionFromServer( Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			ModPacket packet = mymod.GetPacket();
			var modworld = ModContent.GetInstance<DynamicInvasionsWorld>();

			packet.Write( (byte)NetProtocolTypes.EndInvasion );

			packet.Send( (int)player.whoAmI );
		}



		////////////////
		// Server Receivers
		////////////////

		private static void ReceiveInvasionRequestOnServer( BinaryReader reader, int playerWho ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			int musicType = reader.ReadInt32();
			string spawnInfoEnc = reader.ReadString();
			var spawnInfo = JsonConvert.DeserializeObject<List<KeyValuePair<int, ISet<int>>>>( spawnInfoEnc );

			var myworld = ModContent.GetInstance<DynamicInvasionsWorld>();
			myworld.Logic.StartInvasion( musicType, spawnInfo.AsReadOnly() );

			for( int i = 0; i < Main.player.Length; i++ ) {
				Player player = Main.player[i];
				if( player == null || !player.active ) { continue; }

				ServerPacketHandlers.SendInvasionFromServer( player, musicType, spawnInfoEnc );
			}
		}

		private static void ReceiveInvasionStatusRequestOnServer( BinaryReader reader, int playerWho ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ServerPacketHandlers.SendInvasionStatusFromServer( Main.player[playerWho] );
		}
		
		private static void ReceiveEndInvasionRequestOnServer( BinaryReader reader, int playerWho ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var myworld = ModContent.GetInstance<DynamicInvasionsWorld>();

			myworld.Logic.EndInvasion();

			for( int i = 0; i < Main.player.Length; i++ ) {
				Player player = Main.player[i];
				if( player == null || !player.active ) { continue; }

				ServerPacketHandlers.SendEndInvasionFromServer( player );
			}
		}
	}
}
