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
			foreach( var d in foundDevices.ToList())
				if(d.Adapter.DeviceName == @"\\.\DISPLAY2") {
					foundDevices.Remove(d);
					foundDevices.Insert(0, d);
					break;
				}
		}

		protected override void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
		{
			base.OnPreparingDeviceSettings(sender, e);
		}
	}
}
