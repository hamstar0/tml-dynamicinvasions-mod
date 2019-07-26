using DynamicInvasions.NetProtocol;
using Newtonsoft.Json;
using System.Collections.Generic;
using Terraria;


namespace DynamicInvasions {
	public static class DynamicInvasionsAPI {
		public static void StartInvasion( int musicType, IReadOnlyList<KeyValuePair<int, ISet<int>>> spawnInfo ) {
			var myworld = DynamicInvasionsMod.Instance.GetModWorld<DynamicInvasionsWorld>();

			if( Main.netMode == 0 ) {
				myworld.Logic.StartInvasion( musicType, spawnInfo );
			} else if( Main.netMode == 1 ) {
				ClientPacketHandlers.SendInvasionRequestFromClient( musicType, spawnInfo );
			} else if( Main.netMode == 2 ) {
				string spawnInfoEnc = JsonConvert.SerializeObject( spawnInfo );

				myworld.Logic.StartInvasion( musicType, spawnInfo );

				for( int i = 0; i < Main.player.Length; i++ ) {
					Player player = Main.player[i];
					if( player == null || !player.active ) { continue; }

					ServerPacketHandlers.SendInvasionFromServer( player, musicType, spawnInfoEnc );
				}
			}
		}

		public static void EndInvasion() {
			var myworld = DynamicInvasionsMod.Instance.GetModWorld<DynamicInvasionsWorld>();

			if( Main.netMode == 0 ) {
				myworld.Logic.EndInvasion();
			} else if( Main.netMode == 1 ) {
				ClientPacketHandlers.SendEndInvasionRequestFromClient();
			} else if( Main.netMode == 2 ) {
				myworld.Logic.EndInvasion();

				for( int i = 0; i < Main.player.Length; i++ ) {
					Player player = Main.player[i];
					if( player == null || !player.active ) { continue; }

					ServerPacketHandlers.SendEndInvasionFromServer( player );
				}
			}
		}
	}
}
