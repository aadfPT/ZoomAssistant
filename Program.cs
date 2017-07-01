using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using ZoomAssistant.Services;

namespace ZoomAssistant
{
	internal class Program
	{
		private static void Main() //string[] args)
		{
			var app = new ZoomAssistant();
			app.Run();
		}
	}

	internal class ZoomAssistant
	{
		public void Run() //string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			using (var mainForm = new Form1())
			using (var hotkeysService = new HotkeysService())
			using (var magnificationService = new MagnificationService())
			using (var pi = new ProcessIcon())
			{
				pi.Display();
				hotkeysService.RegisterHotkey(Keys.V, magnificationService.ResetDesktopZoom, mainForm);
				hotkeysService.RegisterHotkey(Keys.B, magnificationService.DecreaseDesktopZoom, mainForm);
				hotkeysService.RegisterHotkey(Keys.N, magnificationService.IncreaseDesktopZoom, mainForm);
				Application.Run();
			}
		}
	}
}