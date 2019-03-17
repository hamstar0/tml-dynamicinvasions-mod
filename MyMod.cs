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
using HamstarHelpers.Helpers.TmlHelpers;
using HamstarHelpers.Components.Errors;
using HamstarHelpers.Helpers.TmlHelpers.ModHelpers;
using HamstarHelpers.Services.Promises;


namespace DynamicInvasions {
	partial class DynamicInvasionsMod : Mod {
		public static DynamicInvasionsMod Instance { get; private set; }



		public JsonConfig<DynamicInvasionsConfigData> ConfigJson { get; private set; }
		public DynamicInvasionsConfigData Config => this.ConfigJson.Data;



		////////////////

		public DynamicInvasionsMod() {
			DynamicInvasionsMod.Instance = this;

			this.ConfigJson = new JsonConfig<DynamicInvasionsConfigData>( DynamicInvasionsConfigData.ConfigFileName,
				ConfigurationDataBase.RelativePath, new DynamicInvasionsConfigData() );
		}

		////////////////

		public override void Load() {
			string depErr = TmlHelpers.ReportBadDependencyMods( this );
			if( depErr != null ) { throw new HamstarException( depErr ); }

			this.LoadConfig();

			InvasionLogic.ModLoad();

			Promises.AddPostWorldUnloadEachPromise( () => {
				try {
					var myworld = this.GetModWorld<DynamicInvasionsWorld>();
					myworld.Uninitialize();
				} catch { }
			} );
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
				ErrorLogger.Log( "Dynamic Invasions updated to " + this.Version.ToString() );
				this.ConfigJson.SaveFile();
			}
		}

		public override void Unload() {
			DynamicInvasionsMod.Instance = null;
		}


		////////////////

		public override void HandlePacket( BinaryReader reader, int playerWho ) {
			if( Main.netMode == 1 ) {   // Client
				ClientPacketHandlers.HandlePacket( reader );
			} else if( Main.netMode == 2 ) {    // Server
				ServerPacketHandlers.RoutePacket( reader, playerWho );
			}
		}

		////////////////

		public override object Call( params object[] args ) {
			return ModBoilerplateHelpers.HandleModCall( typeof(DynamicInvasionsAPI), args );
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
