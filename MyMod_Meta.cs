using HamstarHelpers.Helpers.DebugHelpers;
using HamstarHelpers.Components.Config;
using System;
using System.IO;
using Terraria.ModLoader;
using Terraria;


namespace DynamicInvasions {
	partial class DynamicInvasionsMod : Mod {
		public static string GithubUserName { get { return "hamstar0"; } }
		public static string GithubProjectName { get { return "tml-dynamicinvasions-mod"; } }

		public static string ConfigFileRelativePath {
			get { return ConfigurationDataBase.RelativePath + Path.DirectorySeparatorChar + DynamicInvasionsConfigData.ConfigFileName; }
		}
		public static void ReloadConfigFromFile() {
			if( Main.netMode != 0 ) {
				throw new Exception( "Cannot reload configs outside of single player." );
			}
			if( DynamicInvasionsMod.Instance != null ) {
				if( !DynamicInvasionsMod.Instance.ConfigJson.LoadFile() ) {
					DynamicInvasionsMod.Instance.ConfigJson.SaveFile();
				}
			}
		}

		public static void ResetConfigFromDefaults() {
			if( Main.netMode != 0 ) {
				throw new Exception( "Cannot reset to default configs outside of single player." );
			}

			var newConfig = new DynamicInvasionsConfigData();
			//newConfig.SetDefaults();

			DynamicInvasionsMod.Instance.ConfigJson.SetData( newConfig );
			DynamicInvasionsMod.Instance.ConfigJson.SaveFile();
		}
	}
}
