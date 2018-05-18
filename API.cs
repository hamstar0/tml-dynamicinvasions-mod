using System.Collections.Generic;


namespace DynamicInvasions {
	public static class DynamicInvasionsAPI {
		public static DynamicInvasionsConfigData GetModSettings() {
			return DynamicInvasionsMod.Instance.Config.Data;
		}


		public static void StartInvasion( int music_type, IReadOnlyList<KeyValuePair<int, ISet<int>>> spawn_info ) {
			var wrld = DynamicInvasionsMod.Instance.GetModWorld<DynamicInvasionsWorld>();
			wrld.Logic.StartInvasion( DynamicInvasionsMod.Instance, music_type, spawn_info );
		}
	}
}
