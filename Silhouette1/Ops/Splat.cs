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
	class Splat
	{
		public int w, h;
		Effect splatFx;

		public Splat( int w, int h, ContentManager Content) {
			this.w = w;
			this.h = h;
			splatFx = Content.Load<Effect>("splat");
		}

		public void Render(GraphicsDevice GraphicsDevice, RenderTargetDouble input, Vector3 color, Vector2 point, float radius, RenderTargetDouble output) {
			splatFx.Parameters["read"].SetValue(input.Read);
			splatFx.Parameters["color"].SetValue(color);
			splatFx.Parameters["center"].SetValue(point);
			splatFx.Parameters["gridSize"].SetValue(new Vector2(this.w, this.h));
			splatFx.Parameters["radius"].SetValue(radius);

			GraphicsDevice.SetRenderTarget(output.Write);
			SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
			Rectangle r = new Rectangle(0, 0, this.w, this.h);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, splatFx);
			spriteBatch.Draw(Game1.textureWhite, r, Color.White);
			spriteBatch.End();
			GraphicsDevice.SetRenderTarget(null);
			output.Swap();
		}
	}
}
