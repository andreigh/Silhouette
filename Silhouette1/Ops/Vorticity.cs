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
	class Vorticity
	{
		public int w, h;
		Effect vorticityFx;

		public Vorticity( int w, int h, ContentManager Content) {
			this.w = w;
			this.h = h;
			vorticityFx = Content.Load<Effect>("vorticity");
		}

		public void Render(GraphicsDevice GraphicsDevice, RenderTargetDouble velocity, RenderTargetDouble output) {
			vorticityFx.Parameters["velocity"].SetValue(velocity.Read);
			vorticityFx.Parameters["gridSize"].SetValue(new Vector2(this.w, this.h));
			vorticityFx.Parameters["gridScale"].SetValue(1.0f);

			GraphicsDevice.SetRenderTarget(output.Write);
			SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
			Rectangle r = new Rectangle(0, 0, this.w, this.h);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, vorticityFx);
			spriteBatch.Draw(Game1.textureWhite, r, Color.White);
			spriteBatch.End();
			GraphicsDevice.SetRenderTarget(null);
			output.Swap();
		}
	}
}
