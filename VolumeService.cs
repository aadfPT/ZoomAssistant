using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Vannatech.CoreAudio.Interfaces;

namespace ZoomAssistant
{
	class VolumeService
	{
		private IMMDevice _speakers;
		private IAudioEndpointVolume _aepv;

		public VolumeService()
		{
			// get the speakers (1st render + multimedia) device
			var deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());

			deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out _speakers);

			object o;
			//retrieve the actual interface instance to retrieve the volume information from.
			var guid = typeof(IAudioEndpointVolume).GUID;
			_speakers.Activate(ref guid, 0, IntPtr.Zero, out o);
			_aepv = (IAudioEndpointVolume)o;
			Marshal.ReleaseComObject(deviceEnumerator);
		}

		#region support

		[ComImport]
		[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
		internal class MMDeviceEnumerator
		{
		}

		internal enum EDataFlow
		{
			eRender,
			eCapture,
			eAll,
			EDataFlow_enum_count
		}

		internal enum ERole
		{
			eConsole,
			eMultimedia,
			eCommunications,
			ERole_enum_count
		}

		[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IMMDeviceEnumerator
		{
			int NotImpl1();

			[PreserveSig]
			int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

			// the rest is not implemented
		}

		[Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IMMDevice
		{
			[PreserveSig]
			int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams,
				[MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

			// the rest is not implemented
		}

		[Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IAudioSessionManager2
		{
			int NotImpl1();
			int NotImpl2();

			[PreserveSig]
			int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);

			// the rest is not implemented
		}

		[Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IAudioSessionEnumerator
		{
			[PreserveSig]
			int GetCount(out int SessionCount);

			[PreserveSig]
			int GetSession(int SessionCount, out IAudioSessionControl2 Session);
		}

		[Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IAudioSessionControl
		{
			int NotImpl1();

			[PreserveSig]
			int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

			// the rest is not implemented
		}

		[Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface ISimpleAudioVolume
		{
			[PreserveSig]
			int SetMasterVolume(float fLevel, ref Guid EventContext);

			[PreserveSig]
			int GetMasterVolume(out float pfLevel);

			[PreserveSig]
			int SetMute(bool bMute, ref Guid EventContext);

			[PreserveSig]
			int GetMute(out bool pbMute);
		}

		#endregion

		//public void ControlApp(uint pid, int percentage)
		//{
		//    foreach (var otherPid in EnumerateApplications())
		//    {
		//        if (otherPid != pid) continue;
		//        // display mute state & volume level (% of master)
		//        Console.WriteLine("Mute:" + GetApplicationMute(app));
		//        Console.WriteLine("Volume:" + GetApplicationVolume(app));

		//        // mute the application
		//        SetApplicationMute(app, true);

		//        // set the volume to half of master volume (50%)
		//        SetApplicationVolume(app, 50);
		//    }
		//}

		public int? GetApplicationVolume(uint pid)
		{
			var volume = GetVolumeObject(pid);
			if (volume == null)
				return null;

			float level;
			volume.GetMasterVolume(out level);
			return Convert.ToInt32(level * 100);
		}

		public bool? GetApplicationMute(uint pid)
		{
			var volume = GetVolumeObject(pid);
			if (volume == null)
				return null;

			bool mute;
			volume.GetMute(out mute);
			return mute;
		}

		public void SetApplicationVolume(uint pid, int level)
		{
			ISimpleAudioVolume volume = GetVolumeObject(pid);
			if (volume == null)
				return;

			Guid guid = Guid.Empty;
			volume.SetMasterVolume((float)level / 100, ref guid);
		}

		public void SetApplicationMute(uint pid, bool mute)
		{
			ISimpleAudioVolume volume = GetVolumeObject(pid);
			if (volume == null)
				return;

			Guid guid = Guid.Empty;
			volume.SetMute(mute, ref guid);
		}

		public IEnumerable<uint> EnumerateApplications()
		{
			// activate the session manager. we need the enumerator
			var IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
			object o;
			_speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
			var mgr = (IAudioSessionManager2)o;

			// enumerate sessions for on this device
			IAudioSessionEnumerator sessionEnumerator;
			mgr.GetSessionEnumerator(out sessionEnumerator);
			int count;
			sessionEnumerator.GetCount(out count);

			for (var i = 0; i < count; i++)
			{
				IAudioSessionControl2 ctl;
				sessionEnumerator.GetSession(i, out ctl);
				uint pid;
				ctl.GetProcessId(out pid);
				yield return pid;
				Marshal.ReleaseComObject(ctl);
			}
			Marshal.ReleaseComObject(sessionEnumerator);
			Marshal.ReleaseComObject(mgr);
		}

		private ISimpleAudioVolume GetVolumeObject(uint pid)
		{
			// activate the session manager. we need the enumerator
			var IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
			object o;
			_speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
			var mgr = (IAudioSessionManager2)o;

			// enumerate sessions for on this device
			IAudioSessionEnumerator sessionEnumerator;
			mgr.GetSessionEnumerator(out sessionEnumerator);
			int count;
			sessionEnumerator.GetCount(out count);

			// search for an audio session with the required name
			// NOTE: we could also use the process id instead of the app name (with IAudioSessionControl2)
			ISimpleAudioVolume volumeControl = null;
			for (var i = 0; i < count; i++)
			{
				IAudioSessionControl2 ctl;
				sessionEnumerator.GetSession(i, out ctl);
				uint dn;
				ctl.GetProcessId(out dn);
				if (pid == dn)
				{
					volumeControl = ctl as ISimpleAudioVolume;
					break;
				}
				Marshal.ReleaseComObject(ctl);
			}
			Marshal.ReleaseComObject(sessionEnumerator);
			Marshal.ReleaseComObject(mgr);
			return volumeControl;
		}

		public void MuteUnmuteAppVolume()
		{
			var pid = ProcessesService.GetActiveProcessID();
			if (!pid.HasValue) return;
			SetApplicationMute(pid.Value, !(GetApplicationMute(pid.Value) ?? false));
		}

		public void RaiseAppVolume()
		{
			var pid = ProcessesService.GetActiveProcessID();
			if (!pid.HasValue) return;
			var vol = GetApplicationVolume(pid.Value);
			vol = Math.Min(100, vol.GetValueOrDefault(0) + 5);
			SetApplicationVolume(pid.Value, vol.Value);
		}

		public void LowerAppVolume()
		{
			var pid = ProcessesService.GetActiveProcessID();
			if (!pid.HasValue) return;
			var vol = GetApplicationVolume(pid.Value);
			vol = Math.Max(0, vol.GetValueOrDefault(0) - 5);
			SetApplicationVolume(pid.Value, vol.Value);
		}

		public void MuteUnmuteSystemVolume()
		{
			bool muteState;
			_aepv.GetMute(out muteState);
			_aepv.SetMute(!muteState, new System.Guid());
		}

		public void RaiseSystemVolume()
		{
			_aepv.SetMute(false, new System.Guid());
			for (var i = 0; i < 6; i++)
			{
				_aepv.VolumeStepUp(new System.Guid());
			}
		}

		public void LowerSystemVolume()
		{
			_aepv.SetMute(false, new System.Guid());
			for (var i = 0; i < 4; i++)
			{
				_aepv.VolumeStepDown(new System.Guid());
			}
		}
	}
}