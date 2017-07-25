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
	class Boundary
	{
		public int w, h;
		Effect boundaryFx;

		public Boundary( int w, int h, ContentManager Content) {
			this.w = w;
			this.h = h;
			boundaryFx = Content.Load<Effect>("boundary");
		}

		public void Render(GraphicsDevice GraphicsDevice, RenderTargetDouble input, float scale, RenderTargetDouble output) {
			boundaryFx.Parameters["read"].SetValue(input.Read);
			boundaryFx.Parameters["gridSize"].SetValue(new Vector2(w, h));
			boundaryFx.Parameters["scale"].SetValue(scale);

			GraphicsDevice.SetRenderTarget(output.Write);
			SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
			DrawLine(spriteBatch, new Rectangle(0, 0, 1, h), new Vector2(1, 0));
			DrawLine(spriteBatch, new Rectangle(w-1, 0, 1, h), new Vector2(-1, 0));
			DrawLine(spriteBatch, new Rectangle(0, h-1, w, 1), new Vector2(0, 1));
			DrawLine(spriteBatch, new Rectangle(0, 0, w, 1), new Vector2(0, -1));
			GraphicsDevice.SetRenderTarget(null);
		}

		public void DrawLine(SpriteBatch spriteBatch, Rectangle r, Vector2 offset) {
			boundaryFx.Parameters["gridOffset"].SetValue(offset);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, boundaryFx);
			spriteBatch.Draw(Game1.textureWhite, r, Color.White);
			spriteBatch.End();
		}
	}
}
