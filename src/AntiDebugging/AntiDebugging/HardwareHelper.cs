using System.Diagnostics.CodeAnalysis;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace AntiDebugging
{
    public static class HardwareHelper
    {
        private static string FingerPrint { get; set; }

        public static IPAddress Ip()
        {
            var wc = new WebClient();
            var strInternetProtocol = wc.DownloadString("https://ipv4.wtfismyip.com/text");
            if (IPAddress.TryParse(strInternetProtocol.Trim(), out var ip))
                return ip;
            return null;
        }

        public static string HardwareUniqueId()
        {
            // this is shit that normally doesn't get spoofed, i modified it so you don't always need to keep resetting hwid
            return FingerPrint ??= GetMd5Hash("CPU >> " + CpuId() + "\nVIDEO >> " + VideoId());
        }

        //Return a hardware identifier
        [SuppressMessage("ReSharper", "PossibleInvalidCastExceptionInForeachLoop")]
        private static string Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            var result = "";
            var mc = new ManagementClass(wmiClass);
            var moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo[wmiMustBeTrue].ToString() == "True")
                {
                    //Only get the first one
                    if (result == "")
                    {
                        try
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
            return result;
        }
        private static string GetMd5Hash(this string s)
        {
            using MD5 sec = new MD5CryptoServiceProvider();
            var enc = new ASCIIEncoding();
            var bt = enc.GetBytes(s);
            return GetHexString(sec.ComputeHash(bt));
        }
        private static string GetHexString(this byte[] bt)
        {
            var s = string.Empty;
            for (var i = 0; i < bt.Length; i++)
            {
                var b = bt[i];
                int n, n1, n2;
                n = b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + 'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + 'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }

        /// <summary>
        /// Return a hardware identifier
        /// </summary>
        [SuppressMessage("ReSharper", "PossibleInvalidCastExceptionInForeachLoop")]
        private static string Identifier(string wmiClass, string wmiProperty)
        {
            var result = "";
            var mc = new ManagementClass(wmiClass);
            var moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                //Only get the first one
                if (result == "")
                {
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            return result;
        }
        private static string CpuId()
        {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as it is very time consuming
            var retVal = Identifier("Win32_Processor", "UniqueId");
            if (retVal == "") //If no UniqueID, use ProcessorID
            {
                retVal = Identifier("Win32_Processor", "ProcessorId");
                if (retVal == "") //If no ProcessorId, use Name
                {
                    retVal = Identifier("Win32_Processor", "Name");
                    if (retVal == "") //If no Name, use Manufacturer
                    {
                        retVal = Identifier("Win32_Processor", "Manufacturer");
                    }

                }
            }
            return retVal;
        }
        /// <summary>
        /// BIOS Identifier
        /// </summary>
        private static string BiosId()
        {
            return Identifier("Win32_BIOS", "Manufacturer")
            + Identifier("Win32_BIOS", "SMBIOSBIOSVersion")
            + Identifier("Win32_BIOS", "IdentificationCode")
            + Identifier("Win32_BIOS", "SerialNumber")
            + Identifier("Win32_BIOS", "ReleaseDate")
            + Identifier("Win32_BIOS", "Version");
        }
        /// <summary>
        /// Main physical hard drive ID
        /// </summary>
        private static string DiskId()
        {
            return Identifier("Win32_DiskDrive", "Model")
            + Identifier("Win32_DiskDrive", "Manufacturer")
            + Identifier("Win32_DiskDrive", "Signature")
            + Identifier("Win32_DiskDrive", "TotalHeads");
        }
        /// <summary>
        /// Motherboard ID
        /// </summary>
        private static string BaseId()
        {
            return Identifier("Win32_BaseBoard", "Model")
            + Identifier("Win32_BaseBoard", "Manufacturer")
            + Identifier("Win32_BaseBoard", "Name")
            + Identifier("Win32_BaseBoard", "SerialNumber");
        }
        /// <summary>
        /// Primary video controller ID
        /// </summary>
        private static string VideoId()
        {
            return Identifier("Win32_VideoController", "DriverVersion") + Identifier("Win32_VideoController", "Name");
        }
        /// <summary>
        /// First enabled network card ID
        /// </summary>
        private static string MacId()
        {
            return Identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
        }
    }
}
