using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.IO;
using System.Threading;
using MaCRo.Config;

using MaCRo.Core;

namespace MaCRo
{
    public class Program
    {
        public static void Main()
        {
            Engine.getInstance().Run();
        }



    }
}
