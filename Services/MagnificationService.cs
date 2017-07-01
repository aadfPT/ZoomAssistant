using System;
using System.ComponentModel;
using PInvoke;

namespace ZoomAssistant.Services
{
	internal class MagnificationService : IDisposable
	{
		private readonly float ScalingIncrement = 0.04f;

		private float CurrentZoom { get; set; } = 1.0f;
		private User32.SafeHookHandle MouseHook { get; set; }

		public MagnificationService()
		{
			PInvoke.Magnification.MagInitialize();
			var moduleHandle = PInvoke.Kernel32.GetModuleHandle(null);
			MouseHook = PInvoke.User32.SetWindowsHookEx(User32.WindowsHookType.WH_MOUSE_LL
			, MouseMovementHook, moduleHandle.DangerousGetHandle(), 0);
		}


		private int MouseMovementHook(int ncode, IntPtr wparam, IntPtr lparam)
		{
			if (ncode >= 0 && wparam == (IntPtr)PInvoke.User32.WindowMessage.WM_MOUSEMOVE)
			{
				UpdateScreen();
			}
			return User32.CallNextHookEx(MouseHook.DangerousGetHandle(), ncode, wparam, lparam);
		}

		public void DecreaseMagnification()
		{
			SetMagnification(Math.Max(CurrentZoom - ScalingIncrement, 1.0f));
		}

		public void IncreaseMagnification()
		{
			SetMagnification(CurrentZoom + ScalingIncrement);
		}

		public void ResetMagnification()
		{
			SetMagnification(1.0f);
		}

		private void SetMagnification(float magnificationFactor)
		{
			if (!(magnificationFactor >= 1.0)) return;
			CurrentZoom = magnificationFactor;
			UpdateScreen();
		}

		public void UpdateScreen()
		{
			//The following is necessary to follow the cursor
			var screenWidth = PInvoke.User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN);
			var cursorPosition = PInvoke.User32.GetCursorPos();
			var xView = (int)Math.Max(Math.Min(cursorPosition.x - screenWidth * 1.0 / CurrentZoom / 2.0, screenWidth - screenWidth / CurrentZoom), 0);
			var screenHeight = PInvoke.User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN);
			var yView = (int)Math.Max(Math.Min(cursorPosition.y - screenHeight * 1.0 / CurrentZoom / 2.0, screenHeight - screenHeight / CurrentZoom), 0);

			PInvoke.Magnification.MagSetFullscreenTransform(CurrentZoom, xView, yView);
		}

		public void Dispose()
		{
			PInvoke.Magnification.MagUninitialize();
			if (!MouseHook.IsClosed) MouseHook.Close();
			MouseHook.Dispose();
		}

		internal void DecreaseDesktopZoom(object sender, HandledEventArgs e)
		{
			DecreaseMagnification();
		}

		internal void IncreaseDesktopZoom(object sender, HandledEventArgs e)
		{
			IncreaseMagnification();
		}

		internal void ResetDesktopZoom(object sender, HandledEventArgs e)
		{
			ResetMagnification();
		}
	}
}