using HamstarHelpers.DebugHelpers;
using HamstarHelpers.Utilities.Config;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions.NetProtocol {
	static class ServerPacketHandlers {
		public static void RoutePacket( DynamicInvasionsMod mymod, BinaryReader reader, int player_who ) {
			NetProtocolTypes protocol = (NetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case NetProtocolTypes.RequestModSettings:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "ServerPacketHandlers.RequestModSettings" ); }
				ServerPacketHandlers.ReceiveModSettingsRequestOnServer( mymod, reader, player_who );
				break;
			case NetProtocolTypes.RequestInvasion:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "ServerPacketHandlers.RequestInvasion" ); }
				ServerPacketHandlers.ReceiveInvasionRequestOnServer( mymod, reader, player_who );
				break;
			case NetProtocolTypes.RequestInvasionStatus:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "ServerPacketHandlers.RequestInvasionStatus" ); }
				ServerPacketHandlers.ReceiveInvasionStatusRequestOnServer( mymod, reader, player_who );
				break;
			default:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "ServerPacketHandlers ...? " + protocol ); }
				break;
			}
		}


		
		////////////////
		// Server Senders
		////////////////

		public static void SendModSettingsFromServer( DynamicInvasionsMod mymod, Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.ModSettings );
			packet.Write( (string)mymod.Config.SerializeMe() );

			packet.Send( (int)player.whoAmI );
		}

		public static void SendInvasionFromServer( DynamicInvasionsMod mymod, Player player, int music_type, string spawn_info_enc ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)NetProtocolTypes.Invasion );
			packet.Write( (int)music_type );
			packet.Write( (string)spawn_info_enc );

			packet.Send( (int)player.whoAmI );
		}

		public static void SendInvasionStatusFromServer( DynamicInvasionsMod mymod, Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ModPacket packet = mymod.GetPacket();
			var modworld = mymod.GetModWorld<MyWorld>();

			packet.Write( (byte)NetProtocolTypes.InvasionStatus );
			modworld.Logic.MyNetSend( packet );

			packet.Send( (int)player.whoAmI );
		}


		
		////////////////
		// Server Receivers
		////////////////

		private static void ReceiveModSettingsRequestOnServer( DynamicInvasionsMod mymod, BinaryReader reader, int player_who ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ServerPacketHandlers.SendModSettingsFromServer( mymod, Main.player[player_who] );
		}

		private static void ReceiveInvasionRequestOnServer( DynamicInvasionsMod mymod, BinaryReader reader, int player_who ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			int music_type = reader.ReadInt32();
			string spawn_info_enc = reader.ReadString();
			var spawn_info = JsonConfig<List<KeyValuePair<int, ISet<int>>>>.Deserialize( spawn_info_enc );

			var modworld = mymod.GetModWorld<MyWorld>();
			modworld.Logic.StartInvasion( mymod, music_type, spawn_info.AsReadOnly() );

			for( int i = 0; i < Main.player.Length; i++ ) {
				Player player = Main.player[i];
				if( player == null || !player.active ) { continue; }

				ServerPacketHandlers.SendInvasionFromServer( mymod, player, music_type, spawn_info_enc );
			}
		}

		private static void ReceiveInvasionStatusRequestOnServer( DynamicInvasionsMod mymod, BinaryReader reader, int player_who ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ServerPacketHandlers.SendInvasionStatusFromServer( mymod, Main.player[player_who] );
		}
	}
}
