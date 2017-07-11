using CefSharp;
// using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.OffScreen;
using System.Threading;

namespace Silhouette1
{
	public partial class Form1 : Form
	{
		public ChromiumWebBrowser chromeBrowser;
		public Bitmap bitmap;

		public Form1()
		{
			InitializeComponent();	

            CefSettings settings = new CefSettings();
            // Initialize cef with the provided settings
            Cef.Initialize(settings);
            // Create a browser component
            chromeBrowser = new ChromiumWebBrowser("http://localhost/fluids");
			chromeBrowser.Size = new Size(800,480);
			chromeBrowser.FrameLoadEnd += ChromeBrowser_FrameLoadEnd;
            //this.Controls.Add(chromeBrowser);
            //chromeBrowser.Dock = DockStyle.Fill;
		}

		private void ChromeBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
		{
			TakeScreenshot();
		}

		private async Task TakeScreenshot()
		{
			bitmap = await chromeBrowser.ScreenshotAsync();
			Thread.Sleep(30);
			await TakeScreenshot();
		}
	}
}
