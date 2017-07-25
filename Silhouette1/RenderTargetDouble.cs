using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silhouette1
{
	class RenderTargetDouble
	{
		public RenderTarget2D Read, Write;

		public RenderTargetDouble(GraphicsDevice device, int w, int h) {
			Read = new RenderTarget2D(device, w, h, false, SurfaceFormat.Vector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			Write = new RenderTarget2D(device, w, h, false, SurfaceFormat.Vector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}

		public void Swap() {
			RenderTarget2D c = Read;
			Read = Write;
			Write = c;
		}
	}
}
