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
	class VorticityConfinement
	{
		public int w, h;
		public float timestep;
		public float epsilon = 2.4414e-4f;
		public float curl = 0.3f;
		Effect effect;

		public VorticityConfinement( Effect effect, int w, int h, float timestep) {
			this.w = w;
			this.h = h;
			this.timestep = timestep;
			this.effect = effect;
		}

		public void Render(GraphicsDevice GraphicsDevice, RenderTargetDouble velocity, RenderTargetDouble vorticity, RenderTargetDouble output) {
			effect.Parameters["velocity"].SetValue(velocity.Read);
			effect.Parameters["vorticity"].SetValue(vorticity.Read);
			effect.Parameters["gridSize"].SetValue(new Vector2(w, h));
			effect.Parameters["gridScale"].SetValue(1.0f);
			effect.Parameters["timestep"].SetValue(timestep);
			effect.Parameters["epsilon"].SetValue(epsilon);
			effect.Parameters["curl"].SetValue(new Vector2(this.curl, this.curl));

			GraphicsDevice.SetRenderTarget(output.Write);
			SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
			Rectangle r = new Rectangle(0, 0, w, h);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, effect);
			spriteBatch.Draw(Game1.textureWhite, r, Color.White);
			spriteBatch.End();
			GraphicsDevice.SetRenderTarget(null);
			output.Swap();
		}
	}
}
