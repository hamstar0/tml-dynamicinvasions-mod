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
			float alpha = this.Data.ProgressMeterIntroZoom;
			int progress = this.Data.InvasionSizeStart - this.Data.InvasionSize;
			int progress_max = this.Data.InvasionSizeStart;
			float progress_percent = MathHelper.Clamp( (float)progress / (float)progress_max, 0.0f, 1f );

			float scale = (float)(0.5 + (double)alpha * 0.5);
			Texture2D tex = InvasionLogic.ProgressBarTexture;

			int width = (int)(200.0 * (double)scale);
			int height = (int)(45.0 * (double)scale);
			Vector2 position = new Vector2( (float)(Main.screenWidth - 120), (float)(Main.screenHeight - 40) );
			Rectangle rect = new Rectangle( (int)position.X - width / 2, (int)position.Y - height / 2, width, height );
			Texture2D color_bar_tex = Main.colorBarTexture;
			Texture2D color_blip_tex = Main.colorBlipTexture;

			Utils.DrawInvBG( sb, rect, new Color( 63, 65, 151, 255 ) * 0.785f );

			string percent_str = Language.GetTextValue( "Game.WaveCleared", progress_max != 0 ? (object)((progress_percent * 100f).ToString( "N0" ) + "%") : (object)progress.ToString() );

			if( progress_max != 0 ) {
				Vector2 percent_str_dim = Main.fontMouseText.MeasureString( percent_str );
				float rescale = scale;

				if( (double)percent_str_dim.Y > 22.0 ) {
					rescale *= 22f / percent_str_dim.Y;
				}
				float scale_offset = 169f * scale;
				float y = 8f * scale;
				Vector2 pos = position + Vector2.UnitY * y + Vector2.UnitX * 1f;
				Vector2 pos2 = pos + Vector2.UnitX * (progress_percent - 0.5f) * scale_offset;

				sb.Draw( color_bar_tex, position, new Rectangle?(), Color.White * alpha, 0.0f, new Vector2( (float)(color_bar_tex.Width / 2), 0.0f ), scale, SpriteEffects.None, 0.0f );
				Utils.DrawBorderString( sb, percent_str, pos + new Vector2( 0.0f, -4f ), Color.White * alpha, rescale, 0.5f, 1f, -1 );
				sb.Draw( Main.magicPixel, pos2, new Rectangle?( new Rectangle( 0, 0, 1, 1 ) ), new Color( (int)byte.MaxValue, 241, 51 ) * alpha, 0.0f, new Vector2( 1f, 0.5f ), new Vector2( scale_offset * progress_percent, y ), SpriteEffects.None, 0.0f );
				sb.Draw( Main.magicPixel, pos2, new Rectangle?( new Rectangle( 0, 0, 1, 1 ) ), new Color( (int)byte.MaxValue, 165, 0, (int)sbyte.MaxValue ) * alpha, 0.0f, new Vector2( 1f, 0.5f ), new Vector2( 2f, y ), SpriteEffects.None, 0.0f );
				sb.Draw( Main.magicPixel, pos2, new Rectangle?( new Rectangle( 0, 0, 1, 1 ) ), Color.Black * alpha, 0.0f, new Vector2( 0.0f, 0.5f ), new Vector2( scale_offset * (1f - progress_percent), y ), SpriteEffects.None, 0.0f );
			}

			Vector2 str_dim = Main.fontMouseText.MeasureString( this.Data.Label );
			float str_x_offset = 120f;
			if( str_dim.X > 200.0f ) {
				str_x_offset += str_dim.X - 200f;
			}
			Rectangle rectangle = Utils.CenteredRectangle( new Vector2( (float)Main.screenWidth - str_x_offset, (float)(Main.screenHeight - 80) ), (str_dim + new Vector2( (float)(tex.Width + 12), 6f )) * scale );

			Utils.DrawInvBG( sb, rectangle, this.Data.LabelColor );
			sb.Draw( tex, rectangle.Left() + Vector2.UnitX * scale * 8f, new Rectangle?(), Color.White * alpha, 0.0f, new Vector2( 0.0f, (float)(tex.Height / 2) ), scale * 0.8f, SpriteEffects.None, 0.0f );
			Utils.DrawBorderString( sb, this.Data.Label, rectangle.Right() + Vector2.UnitX * scale * -22f, Color.White * alpha, scale * 0.9f, 1f, 0.4f, -1 );
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
