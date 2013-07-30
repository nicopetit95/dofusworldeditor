using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace Api.Manager
{
    class Utilities
    {
        private static Random _randomizer = new Random();

        public static string GetMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    nic.OperationalStatus == OperationalStatus.Up)
                {
                    var macAdd = nic.GetPhysicalAddress().ToString();
                    var macAddLen = nic.GetPhysicalAddress().ToString().Length;

                    string str = "";

                    for (int i = 0; i < macAddLen; i += 2)
                        str = string.Concat(str, "-", macAdd.Substring(i, 2));

                    return str.Substring(1);
                }
            }
            return null;
        }

        public static char[] Hash = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's',
                't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
                'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'};

        public static string RandomString(int lenght)
        {
            var str = string.Empty;

            for (var i = 1; i <= lenght; i++)
            {
                var randomInt = _randomizer.Next(0, Hash.Length);
                str += Hash[randomInt];
            }

            return str;
        }
    }
}
