using System;

namespace Silhouette1
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
		[STAThread]
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
				Form1 form = new Form1();
				form.Show();
				game.form = form;
				game.Run();
            }
        }
    }
#endif
}

