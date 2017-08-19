using HamstarHelpers.DebugHelpers;
using HamstarHelpers.Utilities.Config;
using System;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.UI;
using Terraria;
using System.IO;
using DynamicInvasions.NetProtocol;


namespace DynamicInvasions {
    public class DynamicInvasions : Mod {
		public JsonConfig<ConfigurationData> Config { get; private set; }

		public int DEBUGFLAGS { get; private set; }


		public DynamicInvasions() {
			this.DEBUGFLAGS = 0;

			this.Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};

			string filename = "Dynamic Invasions Config.json";
			this.Config = new JsonConfig<ConfigurationData>( filename, "Mod Configs", new ConfigurationData() );
		}


		public override void Load() {
			var hamhelpmod = ModLoader.GetMod( "HamstarHelpers" );
			var min_ver = new Version( 1, 0, 17 );
			if( hamhelpmod.Version < min_ver ) {
				throw new Exception( "Hamstar's Helpers must be version "+ min_ver.ToString()+" or greater." );
			}

			this.LoadConfig();
			
			Invasion.InvasionLogic.ModLoad( this );
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
				ErrorLogger.Log( "Dynamic Invasions updated to " + ConfigurationData.ConfigVersion.ToString() );
				this.Config.SaveFile();
			}

			this.DEBUGFLAGS = this.Config.Data.DEBUGFLAGS;
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
				var modworld = this.GetModWorld<MyModWorld>();
				modworld.Logic.UpdateMusic( ref music );
			}
		}


		public override void ModifyInterfaceLayers( List<GameInterfaceLayer> layers ) {
			if( !this.Config.Data.Enabled ) { return; }
			int idx = layers.FindIndex( layer => layer.Name.Equals( "Vanilla: Invasion Progress Bars" ) );

			if( idx != -1 ) {
				var func = new GameInterfaceDrawMethod( delegate() {
						if( Main.netMode != 2 ) {  // Not server
						var modworld = this.GetModWorld<MyModWorld>();

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
			return (this.DEBUGFLAGS & 1) > 0;
		}
		public bool IsForcedResetMode() {
			return (this.DEBUGFLAGS & 2) > 0;
		}
		public bool IsCheatMode() {
			return (this.DEBUGFLAGS & 4) > 0;
		}
	}
}
