using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silhouette1
{
	class CustomGraphicsDeviceManager : GraphicsDeviceManager
	{
		public CustomGraphicsDeviceManager(Game game) : base(game) {
		}

		protected override void RankDevices(List<GraphicsDeviceInformation> foundDevices)
		{
			base.RankDevices(foundDevices);
		}
	}
}
