using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using DynamicInvasions.NetProtocol;
using DynamicInvasions.Invasion;
using HamstarHelpers.Services.Hooks.LoadHooks;
using HamstarHelpers.Helpers.TModLoader.Mods;


namespace DynamicInvasions {
	partial class DynamicInvasionsMod : Mod {
		public static DynamicInvasionsMod Instance { get; private set; }



		////////////////

		public DynamicInvasionsConfig Config => ModContent.GetInstance<DynamicInvasionsConfig>();



		////////////////

		public DynamicInvasionsMod() {
			DynamicInvasionsMod.Instance = this;
		}

		////////////////

		public override void Load() {
			InvasionLogic.ModLoad();

			LoadHooks.AddPostWorldUnloadEachHook( () => {
				try {
					var myworld = ModContent.GetInstance<DynamicInvasionsWorld>();
					myworld.Uninitialize();
				} catch { }
			} );
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

		public override void UpdateMusic( ref int music, ref MusicPriority priority ) {
			if( this.Config == null || !this.Config.Enabled ) { return; }

			if( Main.myPlayer != -1 && !Main.gameMenu && Main.LocalPlayer.active ) {
				var modworld = ModContent.GetInstance<DynamicInvasionsWorld>();
				modworld.Logic.UpdateMusic( ref music );
			}
		}


		public override void ModifyInterfaceLayers( List<GameInterfaceLayer> layers ) {
			if( this.Config == null || !this.Config.Enabled ) { return; }
			int idx = layers.FindIndex( layer => layer.Name.Equals( "Vanilla: Invasion Progress Bars" ) );

			if( idx != -1 ) {
				var func = new GameInterfaceDrawMethod( delegate() {
						if( Main.netMode != 2 ) {  // Not server
						var modworld = ModContent.GetInstance<DynamicInvasionsWorld>();

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
