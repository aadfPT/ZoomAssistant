using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HidLibrary;

namespace ZoomAssistant
{
    class SpecialKeysService
    {
        private const int VendorId = 0x0B05;
        private  readonly int[] ProductIds = new[] {0x17FD};
        private  List<HidDeviceShell> _devices = new List<HidDeviceShell>();
        private VolumeService _volumeService = new VolumeService();

        public void Register()
        {
            foreach (var productId in ProductIds)
            {
                var devs = HidDevices.Enumerate(VendorId, productId, (ushort)65280).Where(d => d.Capabilities.Usage == -256);
                //var device = HidDevices.Enumerate(VendorId, productId, (ushort)65280).First();
                foreach (var hidDevice in devs)
                {
                    _devices.Add(new HidDeviceShell(hidDevice, _volumeService));
                //    _devices.Add(new HidDeviceShell(device));
                }

                //Console.WriteLine(_devices != null
                //    ? $"Hooked succesfully to {_devices.Count} devices."
                //    : "Could not find any devices.");
            }

        }
    }

    internal class HidDeviceShell
        {
            internal HidDevice Device { get; }
            private bool _attached;
        private VolumeService _volumeService = new VolumeService();

        internal HidDeviceShell(HidDevice hidDevice, VolumeService volumeService)
            {
                Device = hidDevice;
                    Device.OpenDevice();

                    Device.Inserted += DeviceAttachedHandler;
                    Device.Removed += DeviceRemovedHandler;
                    Device.MonitorDeviceEvents = true;
                    Device.ReadReport(OnReport);
                
            }

            private void DeviceAttachedHandler()
            {
                _attached = true;
                //Console.WriteLine("Gamepad attached.");
                Device.ReadReport(OnReport);
            }

            private void DeviceRemovedHandler()
            {
                _attached = false;
                //Console.WriteLine("Gamepad removed.");
            }



            private void OnReport(HidReport report)
            {

                if (_attached == false)
                {
                    return;
                }
                var sum = 0;
                foreach (var slug in report.Data)
                {
                    sum += slug;
                }
                if (sum > 0)
            {
                if ((Control.ModifierKeys & Keys.Control) != 0)
                {
                    switch (report.Data[0])
                    {
                        case 0:
                        {

                            _volumeService.MuteUnmuteAppVolume();
                            break;
                        }
                        case 1:
                        {
                            _volumeService.LowerAppVolume();
                            break;
                        }
                        case 2:
                        {
                            _volumeService.RaiseAppVolume();
                            break;
                        }
                    }
                }
                else
                {
                    switch (report.Data[0])
                    {
                        case 0:
                        {

                            _volumeService.MuteUnmuteSystemVolume();
                            break;
                        }
                        case 1:
                        {
                            _volumeService.LowerSystemVolume();
                            break;
                        }
                        case 2:
                        {
                            _volumeService.RaiseSystemVolume();
                            break;
                        }

                    }
                }
            }

                Device.ReadReport(OnReport);
            }
        }
    }