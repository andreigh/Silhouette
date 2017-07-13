using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silhouette1.Ops
{
	class Divergence
	{
		public int w, h;
		Effect divergenceFx;

		public Divergence( int w, int h, ContentManager Content) {
			this.w = w;
			this.h = h;
			divergenceFx = Content.Load<Effect>("divergence");
		}

		public void Render(GraphicsDevice GraphicsDevice, RenderTargetDouble velocity, RenderTargetDouble divergence) {
			divergenceFx.Parameters["velocity"].SetValue(velocity.Read);
			divergenceFx.Parameters["gridSize"].SetValue(new Vector2(w, h));
			divergenceFx.Parameters["gridScale"].SetValue(1.0f);

			GraphicsDevice.SetRenderTarget(divergence.Write);
			SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
			Rectangle r = new Rectangle(0, 0, w, h);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, divergenceFx);
			spriteBatch.Draw(Game1.textureWhite, r, Color.White);
			spriteBatch.End();
			GraphicsDevice.SetRenderTarget(null);
		}
	}
}
