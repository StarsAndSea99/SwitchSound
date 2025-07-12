using System;
using System.Runtime.InteropServices;

namespace SwitchSound
{
    [ComImport, Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
    internal class PolicyConfigClient
    {
    }

    [ComImport, Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPolicyConfigClient
    {
        [PreserveSig]
        int GetMixFormat(string pwstrDeviceId, IntPtr ppFormat);

        [PreserveSig]
        int GetDeviceFormat(string pwstrDeviceId, bool bDefault, IntPtr ppFormat);

        [PreserveSig]
        int ResetDeviceFormat(string pwstrDeviceId);

        [PreserveSig]
        int SetDeviceFormat(string pwstrDeviceId, IntPtr pEndpointFormat, IntPtr MixFormat);

        [PreserveSig]
        int GetProcessingPeriod(string pwstrDeviceId, bool bDefault, IntPtr pmftDefaultPeriod, IntPtr pmftMinimumPeriod);

        [PreserveSig]
        int SetProcessingPeriod(string pwstrDeviceId, IntPtr pmftPeriod);

        [PreserveSig]
        int GetShareMode(string pwstrDeviceId, IntPtr pMode);

        [PreserveSig]
        int SetShareMode(string pwstrDeviceId, IntPtr mode);

        [PreserveSig]
        int GetPropertyValue(string pwstrDeviceId, bool bFxStore, ref CoreAudioApi.PropertyKey key, out CoreAudioApi.PropVariant pv);

        [PreserveSig]
        int SetPropertyValue(string pwstrDeviceId, bool bFxStore, ref CoreAudioApi.PropertyKey key, ref CoreAudioApi.PropVariant pv);

        [PreserveSig]
        int SetDefaultEndpoint(string pwstrDeviceId, CoreAudioApi.Role role);

        [PreserveSig]
        int SetEndpointVisibility(string pwstrDeviceId, bool bVisible);
    }

    public class AudioDeviceSwitcher
    {
        public static bool SetDefaultDevice(string deviceId)
        {
            try
            {
                var policyConfig = new PolicyConfigClient() as IPolicyConfigClient;
                if (policyConfig != null)
                {
                    int result = policyConfig.SetDefaultEndpoint(deviceId, CoreAudioApi.Role.Console);
                    return result == 0;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
} 