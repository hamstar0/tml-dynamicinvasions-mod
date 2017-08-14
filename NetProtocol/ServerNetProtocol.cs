﻿using HamstarHelpers.DebugHelpers;
using HamstarHelpers.Utilities.Config;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace DynamicInvasions.NetProtocol {
	public enum ServerNetProtocolTypes : byte {
		RequestModSettings=0,
		RequestInvasion,
		RequestInvasionStatus
	}


	public static class ServerNetProtocol {
		public static void RoutePacket( DynamicInvasions mymod, BinaryReader reader, int player_who ) {
			ServerNetProtocolTypes protocol = (ServerNetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case ServerNetProtocolTypes.RequestModSettings:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "RouteReceivedServerPackets.RequestModSettings" ); }
				ServerNetProtocol.ReceiveModSettingsRequestOnServer( mymod, reader, player_who );
				break;
			case ServerNetProtocolTypes.RequestInvasion:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "RouteReceivedServerPackets.RequestInvasion" ); }
				ServerNetProtocol.ReceiveInvasionRequestOnServer( mymod, reader, player_who );
				break;
			case ServerNetProtocolTypes.RequestInvasionStatus:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "RouteReceivedServerPackets.RequestInvasion" ); }
				ServerNetProtocol.ReceiveInvasionStatusRequestOnServer( mymod, reader, player_who );
				break;
			default:
				if( mymod.IsDebugInfoMode() ) { DebugHelpers.Log( "RouteReceivedServerPackets ...? " + protocol ); }
				break;
			}
		}


		
		////////////////
		// Server Senders
		////////////////

		public static void SendModSettingsFromServer( DynamicInvasions mymod, Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)ClientNetProtocolTypes.ModSettings );
			packet.Write( (string)mymod.Config.SerializeMe() );

			packet.Send( (int)player.whoAmI );
		}

		public static void SendInvasionFromServer( DynamicInvasions mymod, Player player, int music_type, string spawn_info_enc ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)ClientNetProtocolTypes.Invasion );
			packet.Write( (int)music_type );
			packet.Write( (string)spawn_info_enc );

			packet.Send( (int)player.whoAmI );
		}

		public static void SendInvasionStatusFromServer( DynamicInvasions mymod, Player player ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ModPacket packet = mymod.GetPacket();
			var modworld = mymod.GetModWorld<MyModWorld>();

			packet.Write( (byte)ClientNetProtocolTypes.InvasionStatus );
			modworld.Logic.MyNetSend( packet );

			packet.Send( (int)player.whoAmI );
		}


		
		////////////////
		// Server Receivers
		////////////////

		private static void ReceiveModSettingsRequestOnServer( DynamicInvasions mymod, BinaryReader reader, int player_who ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ServerNetProtocol.SendModSettingsFromServer( mymod, Main.player[player_who] );
		}

		private static void ReceiveInvasionRequestOnServer( DynamicInvasions mymod, BinaryReader reader, int player_who ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			int music_type = reader.ReadInt32();
			string spawn_info_enc = reader.ReadString();
			var spawn_info = JsonConfig<List<KeyValuePair<int, ISet<int>>>>.Deserialize( spawn_info_enc );

			var modworld = mymod.GetModWorld<MyModWorld>();
			modworld.Logic.StartInvasion( mymod, music_type, spawn_info.AsReadOnly() );

			for( int i = 0; i < Main.player.Length; i++ ) {
				Player player = Main.player[i];
				if( player == null || !player.active ) { continue; }

				ServerNetProtocol.SendInvasionFromServer( mymod, player, music_type, spawn_info_enc );
			}
		}

		private static void ReceiveInvasionStatusRequestOnServer( DynamicInvasions mymod, BinaryReader reader, int player_who ) {
			// Server only
			if( Main.netMode != 2 ) { return; }

			ServerNetProtocol.SendInvasionStatusFromServer( mymod, Main.player[player_who] );
		}
	}
}
