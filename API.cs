using System.Collections.Generic;


namespace DynamicInvasions {
	public static class DynamicInvasionsAPI {
		public static DynamicInvasionsConfigData GetModSettings() {
			return DynamicInvasionsMod.Instance.ConfigJson.Data;
		}


		public static void StartInvasion( int musicType, IReadOnlyList<KeyValuePair<int, ISet<int>>> spawnInfo ) {
			var wrld = DynamicInvasionsMod.Instance.GetModWorld<DynamicInvasionsWorld>();
			wrld.Logic.StartInvasion( DynamicInvasionsMod.Instance, musicType, spawnInfo );
		}
	}
}
