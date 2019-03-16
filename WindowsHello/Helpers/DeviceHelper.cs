using System;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace WindowsHello.Helpers
{
    public class DeviceHelper
    {
        public static Guid GetDeviceId()
        {
            //Get the Device ID to pass to the server
            EasClientDeviceInformation deviceInformation = new EasClientDeviceInformation();
            return deviceInformation.Id;
        }
    }
}
