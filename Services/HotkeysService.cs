using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace ZoomAssistant.Services
{
	internal class HotkeysService : IDisposable
	{
		private readonly List<Hotkey> _registeredHotkeys = new List<Hotkey>();


		internal void RegisterHotkey(Keys key, HandledEventHandler pressedWithAlt, Form placeholder)
		{

			var withWinHotkey = new Hotkey
			{
				KeyCode = key,
				Alt = true
			};
			withWinHotkey.Pressed += pressedWithAlt;

			if (!withWinHotkey.GetCanRegister(placeholder))
			{
				Console.WriteLine(
												"Whoops, looks like attempts to register will fail or throw an exception, show an error/visual user feedback");
				return;
			}
			withWinHotkey.Register(placeholder);

			_registeredHotkeys.Add(withWinHotkey);
		}

		public void Dispose()
		{
			foreach (var hk in _registeredHotkeys.Where(hk => hk.Registered))
			{
				hk.Unregister();
			}
		}
	}
}