using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemTrayApp;
using ZoomAssistant.Properties;

namespace ZoomAssistant
{
	class ProcessIcon : IDisposable
	{
		//Source: http://www.codeproject.com/Articles/290013/Formless-System-Tray-Application
		private NotifyIcon _ni = new NotifyIcon();

		/// <summary>
		/// Displays the icon in the system tray.
		/// </summary>
		public void Display()
		{
			// Put the icon in the system tray and allow it react to mouse clicks.			
			//_ni.MouseClick += new MouseEventHandler(ni_MouseClick);
			_ni.Icon = Resources.SystemTrayApp;
			_ni.Text = "Zoom Assistant";
			_ni.Visible = true;

			// Attach a context menu.
			_ni.ContextMenuStrip = new ContextMenus().Create();
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		public void Dispose()
		{
			// When the application closes, this will remove the icon from the system tray immediately.
			_ni.Dispose();
		}

		/// <summary>
		/// Handles the MouseClick event of the ni control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		//void ni_MouseClick(object sender, MouseEventArgs e)
		//{
		//    // Handle mouse button clicks.
		//    if (e.Button == MouseButtons.Left)
		//    {
		//        // Start Windows Explorer.
		//        Process.Start("explorer", null);
		//    }
		//}
	}
}
