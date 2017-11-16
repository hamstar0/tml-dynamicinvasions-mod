using HamstarHelpers.DebugHelpers;
using HamstarHelpers.Utilities.Config;
using System;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.UI;
using Terraria;
using System.IO;
using DynamicInvasions.NetProtocol;
using DynamicInvasions.Invasion;


namespace DynamicInvasions {
    class DynamicInvasionsMod : Mod {
		public static DynamicInvasionsMod Instance { get; private set; }

		public static string GithubUserName { get { return "hamstar0"; } }
		public static string GithubProjectName { get { return "tml-dynamicinvasions-mod"; } }

		public static string ConfigRelativeFilePath {
			get { return ConfigurationDataBase.RelativePath + Path.DirectorySeparatorChar + DynamicInvasionsConfigData.ConfigFileName; }
		}
		public static void ReloadConfigFromFile() {
			if( Main.netMode != 0 ) {
				throw new Exception( "Cannot reload configs outside of single player." );
			}
			if( DynamicInvasionsMod.Instance != null ) {
				DynamicInvasionsMod.Instance.Config.LoadFile();
			}
		}


		////////////////

		public JsonConfig<DynamicInvasionsConfigData> Config { get; private set; }


		////////////////

		public DynamicInvasionsMod() {
			this.Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};

			this.Config = new JsonConfig<DynamicInvasionsConfigData>( DynamicInvasionsConfigData.ConfigFileName,
				ConfigurationDataBase.RelativePath, new DynamicInvasionsConfigData() );
		}

		////////////////

		public override void Load() {
			DynamicInvasionsMod.Instance = this;

			var hamhelpmod = ModLoader.GetMod( "HamstarHelpers" );
			var min_ver = new Version( 1, 2, 0 );
			if( hamhelpmod.Version < min_ver ) {
				throw new Exception( "Hamstar Helpers must be version " + min_ver.ToString() + " or greater." );
			}

			this.LoadConfig();
			
			InvasionLogic.ModLoad( this );
		}

		private void LoadConfig() {
			try {
				if( !this.Config.LoadFile() ) {
					this.Config.SaveFile();
				}
			} catch( Exception e ) {
				DebugHelpers.Log( e.Message );
				this.Config.SaveFile();
			}

			if( this.Config.Data.UpdateToLatestVersion() ) {
				ErrorLogger.Log( "Dynamic Invasions updated to " + DynamicInvasionsConfigData.ConfigVersion.ToString() );
				this.Config.SaveFile();
			}
		}

		public override void Unload() {
			DynamicInvasionsMod.Instance = null;
		}


		////////////////

		public override void HandlePacket( BinaryReader reader, int player_who ) {
			if( Main.netMode == 1 ) {   // Client
				ClientPacketHandlers.HandlePacket( this, reader );
			} else if( Main.netMode == 2 ) {    // Server
				ServerPacketHandlers.RoutePacket( this, reader, player_who );
			}
		}


		////////////////
		
		public override void UpdateMusic( ref int music ) {
			if( !this.Config.Data.Enabled ) { return; }

			if( Main.myPlayer != -1 && !Main.gameMenu && Main.LocalPlayer.active ) {
				var modworld = this.GetModWorld<MyWorld>();
				modworld.Logic.UpdateMusic( ref music );
			}
		}


		public override void ModifyInterfaceLayers( List<GameInterfaceLayer> layers ) {
			if( !this.Config.Data.Enabled ) { return; }
			int idx = layers.FindIndex( layer => layer.Name.Equals( "Vanilla: Invasion Progress Bars" ) );

			if( idx != -1 ) {
				var func = new GameInterfaceDrawMethod( delegate() {
						if( Main.netMode != 2 ) {  // Not server
						var modworld = this.GetModWorld<MyWorld>();

						if( modworld.Logic.RunProgressBarAnimation() ) {
							modworld.Logic.DrawProgressBar( Main.spriteBatch );
						}
					}
					return true;
				} );
				var layer = new LegacyGameInterfaceLayer( "DynamicInvasions: Progress", func, InterfaceScaleType.UI );

				layers.Insert( idx, layer );
			}
		}

		
		////////////////

		public bool IsDebugInfoMode() {
			return (this.Config.Data.DEBUGFLAGS & 1) > 0;
		}
		public bool IsForcedResetMode() {
			return (this.Config.Data.DEBUGFLAGS & 2) > 0;
		}
		public bool IsCheatMode() {
			return (this.Config.Data.DEBUGFLAGS & 4) > 0;
		}
	}
}
