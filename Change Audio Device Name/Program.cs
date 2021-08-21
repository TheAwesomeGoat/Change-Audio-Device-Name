using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Change_Audio_Device_Name
{
    class Program
    {
        struct MMDEVAPI
        {
            public string Capabilities;
            public string ClassGUID;
            public string CompatibleIDs;
            public string ConfigFlags;
            public string ContainerID;
            public string DeviceDesc;
            public string Driver;
            public string FriendlyName;
            public string HardwareID;
            public string Mfg;
            public RegistryKey Render;
        }

        static string RenderPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render"; 
        static RegistryKey MMDEVAPIKey = Registry.LocalMachine.CreateSubKey(@"SYSTEM\ControlSet001\Enum\SWD\MMDEVAPI", false);
        static void Main(string[] args)
        {
            Dictionary<string, MMDEVAPI> AudioDevices = new Dictionary<string, MMDEVAPI>();
            foreach (var item in MMDEVAPIKey.GetSubKeys())
            {
                if (item.SubKeyName != "MicrosoftGSWavetableSynth")
                {
                    string RenderKeyPath = $@"{RenderPath}\{item.SubKeyName.Split('.').Last()}\Properties";
                    AudioDevices.Add(item.SubKeyName, new MMDEVAPI
                    {
                        Capabilities = item.SubKeyValue.GetKeyValue("Capabilities"),
                        ClassGUID = item.SubKeyValue.GetKeyValue("ClassGUID"),
                        CompatibleIDs = item.SubKeyValue.GetKeyValue("CompatibleIDs"),
                        ConfigFlags = item.SubKeyValue.GetKeyValue("ConfigFlags"),
                        ContainerID = item.SubKeyValue.GetKeyValue("ContainerID"),
                        DeviceDesc = item.SubKeyValue.GetKeyValue("DeviceDesc"),
                        Driver = item.SubKeyValue.GetKeyValue("Driver"),
                        FriendlyName = item.SubKeyValue.GetKeyValue("FriendlyName"),
                        HardwareID = item.SubKeyValue.GetKeyValue("HardwareID"),
                        Mfg = item.SubKeyValue.GetKeyValue("Mfg"),
                        Render = Registry.LocalMachine.OpenSubKey(RenderKeyPath, false) // no permission to read????
                    });
                    //Console.WriteLine(RenderKeyPath);
                }
            }
            foreach (var AudioDevice in AudioDevices)
            {
                string ControllerName = AudioDevice.Value.Render.GetKeyValue("{b3f8fa53-0004-438e-9003-51a46e139bfc},6"); // audio device name between ()
                string DeviceName = AudioDevice.Value.Render.GetKeyValue("{a45c254e-df1c-4efd-8020-67d146a850e0},2"); // audio device name
                Console.WriteLine($"{DeviceName} ({ControllerName})");
            }
            // ^^^^^^^^^^^^^^^^^^^^^^^^^ useless
            Console.ReadKey();
        }
    }

    public static class Extensions
    {
        public struct SubKey
        {
            public string SubKeyName;
            public RegistryKey SubKeyValue;
        }

        public static string GetKeyValue(this RegistryKey key, string name)
        {
            if (key == null)
                return "null";
            var item = key.GetValue(name);
            if (item != null)
                return item.ToString();
            return "null";
        }

        public static List<SubKey> GetSubKeys(this RegistryKey key)
        {
            List<SubKey> SubKeys = new List<SubKey>();
            foreach (var item in key.GetSubKeyNames())
            {
                SubKeys.Add(new SubKey 
                { 
                    SubKeyName = item, 
                    SubKeyValue = key.OpenSubKey(item) 
                });
            }
            return SubKeys;
        }
    }

}
