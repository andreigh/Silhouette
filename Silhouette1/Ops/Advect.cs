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
	class Advect
	{
		public float dissipation = 0.998f;
		public int w, h;
		public float timestep;
		Effect advect;

		public Advect( int w, int h, float timestep, ContentManager Content) {
			this.w = w;
			this.h = h;
			this.timestep = timestep;
			advect = Content.Load<Effect>("advect");
		}

		public void Render(GraphicsDevice GraphicsDevice, RenderTargetDouble velocity, RenderTargetDouble advected, RenderTargetDouble output) {
			advect.Parameters["velocity"].SetValue(velocity.Read);
			advect.Parameters["advected"].SetValue(advected.Read);
			advect.Parameters["gridSize"].SetValue(new Vector2(w, h));
			advect.Parameters["gridScale"].SetValue(1.0f);
			advect.Parameters["timestep"].SetValue(timestep);
			advect.Parameters["dissipation"].SetValue(dissipation);

			GraphicsDevice.SetRenderTarget(output.Write);
			SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
			Rectangle r = new Rectangle(0, 0, w, h);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, advect);
			spriteBatch.Draw(Game1.textureWhite, r, Color.White);
			spriteBatch.End();
			GraphicsDevice.SetRenderTarget(null);
		}
	}
}
