using HamstarHelpers.Helpers.DebugHelpers;
using HamstarHelpers.Components.Config;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using DynamicInvasions.NetProtocol;
using DynamicInvasions.Invasion;


namespace DynamicInvasions {
	partial class DynamicInvasionsMod : Mod {
		public static DynamicInvasionsMod Instance { get; private set; }

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

			var new_config = new DynamicInvasionsConfigData();
			//new_config.SetDefaults();

			DynamicInvasionsMod.Instance.ConfigJson.SetData( new_config );
			DynamicInvasionsMod.Instance.ConfigJson.SaveFile();
		}


		////////////////

		public JsonConfig<DynamicInvasionsConfigData> ConfigJson { get; private set; }
		public DynamicInvasionsConfigData Config { get { return this.ConfigJson.Data; } }


		////////////////

		public DynamicInvasionsMod() {
			this.Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};

			this.ConfigJson = new JsonConfig<DynamicInvasionsConfigData>( DynamicInvasionsConfigData.ConfigFileName,
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
				if( !this.ConfigJson.LoadFile() ) {
					this.ConfigJson.SaveFile();
				}
			} catch( Exception e ) {
				LogHelpers.Log( e.Message );
				this.ConfigJson.SaveFile();
			}

			if( this.ConfigJson.Data.UpdateToLatestVersion() ) {
				ErrorLogger.Log( "Dynamic Invasions updated to " + DynamicInvasionsConfigData.ConfigVersion.ToString() );
				this.ConfigJson.SaveFile();
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
			if( !this.ConfigJson.Data.Enabled ) { return; }

			if( Main.myPlayer != -1 && !Main.gameMenu && Main.LocalPlayer.active ) {
				var modworld = this.GetModWorld<DynamicInvasionsWorld>();
				modworld.Logic.UpdateMusic( ref music );
			}
		}


		public override void ModifyInterfaceLayers( List<GameInterfaceLayer> layers ) {
			if( !this.ConfigJson.Data.Enabled ) { return; }
			int idx = layers.FindIndex( layer => layer.Name.Equals( "Vanilla: Invasion Progress Bars" ) );

			if( idx != -1 ) {
				var func = new GameInterfaceDrawMethod( delegate() {
						if( Main.netMode != 2 ) {  // Not server
						var modworld = this.GetModWorld<DynamicInvasionsWorld>();

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
	}
}
