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
	class Gradient
	{
		public int w, h;
		Effect gradientFx;

		public Gradient( int w, int h, ContentManager Content) {
			this.w = w;
			this.h = h;
			gradientFx = Content.Load<Effect>("gradient");
		}

		public void Render(GraphicsDevice GraphicsDevice, RenderTargetDouble p, RenderTargetDouble w, RenderTargetDouble output) {
			gradientFx.Parameters["p"].SetValue(p.Read);
			gradientFx.Parameters["w"].SetValue(w.Read);
			gradientFx.Parameters["gridSize"].SetValue(new Vector2(this.w, this.h));
			gradientFx.Parameters["gridScale"].SetValue(1.0f);

			GraphicsDevice.SetRenderTarget(output.Write);
			SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
			Rectangle r = new Rectangle(0, 0, this.w, this.h);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, gradientFx);
			spriteBatch.Draw(Game1.textureWhite, r, Color.White);
			spriteBatch.End();
			GraphicsDevice.SetRenderTarget(null);
			output.Swap();
		}
	}
}
