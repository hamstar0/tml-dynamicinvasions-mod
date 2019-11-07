using HamstarHelpers.Helpers.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Localization;


namespace DynamicInvasions.Invasion {
	partial class InvasionLogic {
		public bool RunProgressBarAnimation() {
			if( this.Data.InvasionEnrouteDuration > 0 ) {
				this.Data.InvasionProgressIntroAnimation = 160;
			} else {
				if( !Main.gamePaused && this.Data.InvasionProgressIntroAnimation > 0 ) {
					--this.Data.InvasionProgressIntroAnimation;
				}

				if( this.Data.InvasionProgressIntroAnimation > 0 ) {
					this.Data.ProgressMeterIntroZoom = Math.Min( this.Data.ProgressMeterIntroZoom + 0.05f, 1f );
				} else if( !this.Data.IsInvading ) {
					this.Data.ProgressMeterIntroZoom = Math.Max( this.Data.ProgressMeterIntroZoom - 0.05f, 0f );
				}
			}

			return this.Data.ProgressMeterIntroZoom > 0f;
		}


		public void DrawProgressBar( SpriteBatch sb ) {
			try {
				float alpha = this.Data.ProgressMeterIntroZoom;
				int progress = this.Data.InvasionSizeStart - this.Data.InvasionSize;
				int progressMax = this.Data.InvasionSizeStart;
				float progressPercent = MathHelper.Clamp( (float)progress / (float)progressMax, 0.0f, 1f );

				float scale = (float)( 0.5 + (double)alpha * 0.5 );
				Texture2D tex = InvasionLogic.ProgressBarTexture;

				int width = (int)( 200.0 * (double)scale );
				int height = (int)( 45.0 * (double)scale );
				Vector2 position = new Vector2( (float)( Main.screenWidth - 120 ), (float)( Main.screenHeight - 40 ) );
				Rectangle rect = new Rectangle( (int)position.X - width / 2, (int)position.Y - height / 2, width, height );
				Texture2D colorBarTex = Main.colorBarTexture;
				Texture2D colorBlipTex = Main.colorBlipTexture;

				Utils.DrawInvBG( sb, rect, new Color( 63, 65, 151, 255 ) * 0.785f );

				string percentStr = Language.GetTextValue( "Game.WaveCleared", progressMax != 0 ? (object)( ( progressPercent * 100f ).ToString( "N0" ) + "%" ) : (object)progress.ToString() );

				if( progressMax != 0 ) {
					Vector2 percentStrDim = Main.fontMouseText.MeasureString( percentStr );
					float rescale = scale;

					if( (double)percentStrDim.Y > 22.0 ) {
						rescale *= 22f / percentStrDim.Y;
					}
					float scaleOffset = 169f * scale;
					float y = 8f * scale;
					Vector2 pos = position + Vector2.UnitY * y + Vector2.UnitX * 1f;
					Vector2 pos2 = pos + Vector2.UnitX * ( progressPercent - 0.5f ) * scaleOffset;

					sb.Draw( colorBarTex, position, new Rectangle?(), Color.White * alpha, 0.0f, new Vector2( (float)( colorBarTex.Width / 2 ), 0.0f ), scale, SpriteEffects.None, 0.0f );
					Utils.DrawBorderString( sb, percentStr, pos + new Vector2( 0.0f, -4f ), Color.White * alpha, rescale, 0.5f, 1f, -1 );
					sb.Draw( Main.magicPixel, pos2, new Rectangle?( new Rectangle( 0, 0, 1, 1 ) ), new Color( (int)byte.MaxValue, 241, 51 ) * alpha, 0.0f, new Vector2( 1f, 0.5f ), new Vector2( scaleOffset * progressPercent, y ), SpriteEffects.None, 0.0f );
					sb.Draw( Main.magicPixel, pos2, new Rectangle?( new Rectangle( 0, 0, 1, 1 ) ), new Color( (int)byte.MaxValue, 165, 0, (int)sbyte.MaxValue ) * alpha, 0.0f, new Vector2( 1f, 0.5f ), new Vector2( 2f, y ), SpriteEffects.None, 0.0f );
					sb.Draw( Main.magicPixel, pos2, new Rectangle?( new Rectangle( 0, 0, 1, 1 ) ), Color.Black * alpha, 0.0f, new Vector2( 0.0f, 0.5f ), new Vector2( scaleOffset * ( 1f - progressPercent ), y ), SpriteEffects.None, 0.0f );
				}

				Vector2 strDim = Main.fontMouseText.MeasureString( this.Data.Label );
				float strXOffset = 120f;
				if( strDim.X > 200.0f ) {
					strXOffset += strDim.X - 200f;
				}
				Rectangle rectangle = Utils.CenteredRectangle( new Vector2( (float)Main.screenWidth - strXOffset, (float)( Main.screenHeight - 80 ) ), ( strDim + new Vector2( (float)( tex.Width + 12 ), 6f ) ) * scale );

				Utils.DrawInvBG( sb, rectangle, this.Data.LabelColor );
				sb.Draw( tex, rectangle.Left() + Vector2.UnitX * scale * 8f, new Rectangle?(), Color.White * alpha, 0.0f, new Vector2( 0.0f, (float)( tex.Height / 2 ) ), scale * 0.8f, SpriteEffects.None, 0.0f );
				Utils.DrawBorderString( sb, this.Data.Label, rectangle.Right() + Vector2.UnitX * scale * -22f, Color.White * alpha, scale * 0.9f, 1f, 0.4f, -1 );
			} catch( Exception e ) {
				LogHelpers.WarnAndPrintOnce( e.Message );
				LogHelpers.WarnOnce( e.ToString() );
			}
		}


		public void InvasionWarning( string msg ) {
			Color color = new Color( 175, 75, (int)byte.MaxValue );

			if( Main.netMode == 0 ) {   // Single
				Main.NewText( msg, color.R, color.G, color.B, false );
			} else if( Main.netMode == 2 ) {   // Server
				NetMessage.BroadcastChatMessage( NetworkText.FromLiteral( msg ), color, -1 );
			}
		}
	}
}
