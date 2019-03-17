using HamstarHelpers.Components.Config;
using HamstarHelpers.Helpers.DebugHelpers;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions.NetProtocol {
	static class ServerPacketHandlers {
		public static void RoutePacket( BinaryReader reader, int playerWho ) {
			var mymod = DynamicInvasionsMod.Instance;
			NetProtocolTypes protocol = (NetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case NetProtocolTypes.RequestModSettings:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ServerPacketHandlers.RequestModSettings" ); }
				ServerPacketHandlers.ReceiveModSettingsRequestOnServer( reader, playerWho );
				break;
			case NetProtocolTypes.RequestInvasion:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ServerPacketHandlers.RequestInvasion" ); }
				ServerPacketHandlers.ReceiveInvasionRequestOnServer( reader, playerWho );
				break;
			case NetProtocolTypes.RequestInvasionStatus:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ServerPacketHandlers.RequestInvasionStatus" ); }
				ServerPacketHandlers.ReceiveInvasionStatusRequestOnServer( reader, playerWho );
				break;
			default:
				if( mymod.Config.DebugModeInfo ) { LogHelpers.Log( "ServerPacketHandlers ...? " + protocol ); }
				break;
			}
		}


		
		////////////////
		// Server Senders
		////////////////

		public static void SendModSettingsFromServer( Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.ModSettings );
			packet.Write( (string)mymod.ConfigJson.SerializeMe() );

			packet.Send( (int)player.whoAmI );
		}

		public static void SendInvasionFromServer( Player player, int musicType, string spawnInfoCnc ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.Invasion );
			packet.Write( (int)musicType );
			packet.Write( (string)spawnInfoCnc );

			packet.Send( (int)player.whoAmI );
		}

		public static void SendInvasionStatusFromServer( Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			ModPacket packet = mymod.GetPacket();
			var modworld = mymod.GetModWorld<DynamicInvasionsWorld>();

			packet.Write( (byte)NetProtocolTypes.InvasionStatus );
			modworld.Logic.MyNetSend( packet );

			packet.Send( (int)player.whoAmI );
		}


		
		////////////////
		// Server Receivers
		////////////////

		private static void ReceiveModSettingsRequestOnServer( BinaryReader reader, int playerWho ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;

			ServerPacketHandlers.SendModSettingsFromServer( Main.player[playerWho] );
		}

		private static void ReceiveInvasionRequestOnServer( BinaryReader reader, int playerWho ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;
			int musicType = reader.ReadInt32();
			string spawnInfoEnc = reader.ReadString();
			var spawnInfo = JsonConfig<List<KeyValuePair<int, ISet<int>>>>.Deserialize( spawnInfoEnc );

			var modworld = mymod.GetModWorld<DynamicInvasionsWorld>();
			modworld.Logic.StartInvasion( mymod, musicType, spawnInfo.AsReadOnly() );

			for( int i = 0; i < Main.player.Length; i++ ) {
				Player player = Main.player[i];
				if( player == null || !player.active ) { continue; }

				ServerPacketHandlers.SendInvasionFromServer( player, musicType, spawnInfoEnc );
			}
		}

		private static void ReceiveInvasionStatusRequestOnServer( BinaryReader reader, int playerWho ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			var mymod = DynamicInvasionsMod.Instance;

			ServerPacketHandlers.SendInvasionStatusFromServer( Main.player[playerWho] );
		}
	}
}
