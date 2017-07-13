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
	class Jacobi
	{
		public int w, h;
		Effect jacobiFx;
		public float alpha = -1, beta = 4;
		int iterations = 50;

		public Jacobi( Effect effect, int w, int h, ContentManager Content) {
			this.w = w;
			this.h = h;
			jacobiFx = effect;
		}

		public void Render(GraphicsDevice GraphicsDevice, RenderTargetDouble x, RenderTargetDouble b, RenderTargetDouble output) {
			for (var i = 0; i < iterations; i++) {
				jacobiFx.Parameters["x"].SetValue(x.Read);
				jacobiFx.Parameters["b"].SetValue(b.Read);
				jacobiFx.Parameters["gridSize"].SetValue(new Vector2(w, h));
				jacobiFx.Parameters["alpha"].SetValue(alpha);
				jacobiFx.Parameters["beta"].SetValue(beta);

				GraphicsDevice.SetRenderTarget(output.Write);
				SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
				Rectangle r = new Rectangle(0, 0, w, h);
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, jacobiFx);
				spriteBatch.Draw(Game1.textureWhite, r, Color.White);
				spriteBatch.End();
				GraphicsDevice.SetRenderTarget(null);
			}
		}
	}
}
