using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SwitchSound
{
    public class CoreAudioApi
    {
        // COM接口定义
        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator
        {
        }

        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(DataFlow dataFlow, DeviceState dwStateMask, out IMMDeviceCollection ppDevices);
            int GetDefaultAudioEndpoint(DataFlow dataFlow, Role role, out IMMDevice ppEndpoint);
            int GetDevice(string pwstrId, out IMMDevice ppDevice);
            int RegisterEndpointNotificationCallback(IMMNotificationClient pClient);
            int UnregisterEndpointNotificationCallback(IMMNotificationClient pClient);
        }

        [ComImport]
        [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceCollection
        {
            int GetCount(out uint pcDevices);
            int Item(uint nDevice, out IMMDevice ppDevice);
        }

        [ComImport]
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(ref Guid iid, uint dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
            int OpenPropertyStore(uint stgmAccess, out IPropertyStore ppProperties);
            int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
            int GetState(out DeviceState pdwState);
        }

        [ComImport]
        [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyStore
        {
            int GetCount(out uint cProps);
            int GetAt(uint iProp, out PropertyKey pkey);
            int GetValue(ref PropertyKey key, out PropVariant pv);
            int SetValue(ref PropertyKey key, ref PropVariant propvar);
            int Commit();
        }

        [ComImport]
        [Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolume
        {
            int RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
            int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
            int GetChannelCount(out uint pnChannelCount);
            int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
            int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
            int GetMasterVolumeLevel(out float pfLevelDB);
            int GetMasterVolumeLevelScalar(out float pfLevel);
            int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
            int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
            int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
            int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
            int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid pguidEventContext);
            int GetMute(out bool pbMute);
            int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
            int VolumeStepUp(ref Guid pguidEventContext);
            int VolumeStepDown(ref Guid pguidEventContext);
            int QueryHardwareSupport(out uint pdwHardwareSupportMask);
            int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
        }

        [ComImport]
        [Guid("657804FA-D6AD-4496-8A60-352752AF4F89")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolumeCallback
        {
            int OnNotify(IntPtr pNotifyData);
        }

        [ComImport]
        [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMNotificationClient
        {
            int OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId, DeviceState dwNewState);
            int OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId);
            int OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId);
            int OnDefaultDeviceChanged(DataFlow flow, Role role, [MarshalAs(UnmanagedType.LPWStr)] string pwstrDefaultDeviceId);
            int OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId, PropertyKey key);
        }

        // 枚举和结构体定义
        public enum DataFlow
        {
            Render = 0,
            Capture = 1,
            All = 2
        }

        public enum Role
        {
            Console = 0,
            Multimedia = 1,
            Communications = 2
        }

        public enum DeviceState
        {
            Active = 0x00000001,
            Disabled = 0x00000002,
            NotPresent = 0x00000004,
            Unplugged = 0x00000008,
            All = 0x0000000F
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PropertyKey
        {
            public Guid fmtid;
            public uint pid;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct PropVariant
        {
            [FieldOffset(0)]
            public ushort vt;
            [FieldOffset(8)]
            public IntPtr pwszVal;
        }

        // 常量定义
        private static readonly PropertyKey PKEY_Device_FriendlyName = new PropertyKey
        {
            fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0),
            pid = 14
        };

        // 公共类和方法
        public class AudioDevice
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool IsDefault { get; set; }
            public DeviceState State { get; set; }
        }

        private IMMDeviceEnumerator deviceEnumerator;

        public CoreAudioApi()
        {
            deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        }

        public List<AudioDevice> GetAudioDevices()
        {
            var devices = new List<AudioDevice>();
            
            try
            {
                deviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active, out IMMDeviceCollection deviceCollection);
                deviceCollection.GetCount(out uint count);

                // 获取默认设备
                deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console, out IMMDevice defaultDevice);
                defaultDevice.GetId(out string defaultId);

                for (uint i = 0; i < count; i++)
                {
                    deviceCollection.Item(i, out IMMDevice device);
                    device.GetId(out string id);
                    device.GetState(out DeviceState state);
                    
                    string name = GetDeviceName(device);
                    
                    devices.Add(new AudioDevice
                    {
                        Id = id,
                        Name = name,
                        IsDefault = id == defaultId,
                        State = state
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取音频设备失败: {ex.Message}");
            }

            return devices;
        }

        public bool SetDefaultAudioDevice(string deviceId)
        {
            try
            {
                // 使用AudioDeviceSwitcher设置默认设备
                return AudioDeviceSwitcher.SetDefaultDevice(deviceId);
            }
            catch (Exception ex)
            {
                throw new Exception($"设置默认音频设备失败: {ex.Message}");
            }
        }

        private string GetDeviceName(IMMDevice device)
        {
            try
            {
                device.OpenPropertyStore(0, out IPropertyStore propertyStore);
                PropertyKey key = PKEY_Device_FriendlyName;
                propertyStore.GetValue(ref key, out PropVariant value);
                return Marshal.PtrToStringUni(value.pwszVal);
            }
            catch
            {
                return "未知设备";
            }
        }
    }
} 