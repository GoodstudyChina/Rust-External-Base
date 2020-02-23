using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Numerics;
using Jupiter;

namespace External_Base
{
    class Program
    {
        public static MemoryModule Memory = new MemoryModule("RustClient");

        public static IntPtr BaseAddress;
        public static Int32 GOMAddress = 0x163d080;
        public static int visiblePlayerList = 0x8;

        static void Main(string[] args)
        {
            
            Process[] process = Process.GetProcessesByName("RustClient");
            if (process.Length > 0) 
            { 
                Process MainProc = process[0];
                foreach (ProcessModule module in MainProc.Modules)
                {
                    if (module.ModuleName.Contains("UnityPlayer"))
                    {
                        BaseAddress = module.BaseAddress;
                    }
                }
            }
            if (BaseAddress != IntPtr.Zero)
            {
                UInt64 GOM = Memory.ReadVirtualMemory<UInt64>((IntPtr)BaseAddress + GOMAddress);
                for (UInt64 Object = Memory.ReadVirtualMemory<UInt64>((IntPtr)GOM + visiblePlayerList); Object != Memory.ReadVirtualMemory<UInt64>((IntPtr)GOM); Object = Memory.ReadVirtualMemory<UInt64>((IntPtr)Object + visiblePlayerList))
                {
                    //Got lazy with naming offsets
                    Int64 GameObject = Memory.ReadVirtualMemory<Int64>((IntPtr)Object + 0x10);
                    ushort Tags = Memory.ReadVirtualMemory<ushort>((IntPtr)GameObject + 0x54);
                    if (Tags == 6)
                    {
                        UInt64 ObjectClass = Memory.ReadVirtualMemory<UInt64>((IntPtr)GameObject + 0x30);
                        UInt64 Entity = Memory.ReadVirtualMemory<UInt64>((IntPtr)ObjectClass + 0x18);

                        UInt64 Player = Memory.ReadVirtualMemory<UInt64>((IntPtr)Entity + 0x28);
                        if (Player != null)
                        {
                            UInt64 Inventory = Memory.ReadVirtualMemory<UInt64>((IntPtr)Player + 0x490);
                            UInt64 Belt = Memory.ReadVirtualMemory<UInt64>((IntPtr)Inventory + 0x28);
                            UInt64 ItemList = Memory.ReadVirtualMemory<UInt64>((IntPtr)Belt + 0x38);
                            UInt64 Items = Memory.ReadVirtualMemory<UInt64>((IntPtr)ItemList + 0x10);

                            for (int ItemsOnBelt = 0; ItemsOnBelt <= 6; ItemsOnBelt++)
                            {
                                try
                                {
                                    if (Items != 0)
                                    {
                                        UInt64 Item = Memory.ReadVirtualMemory<UInt64>((IntPtr)Items + 0x20 + (ItemsOnBelt * 0x8));
                                        UInt64 Held = Memory.ReadVirtualMemory<UInt64>((IntPtr)Item + 0x90);

                                        Memory.WriteVirtualMemory<float>((IntPtr)Held + 0x2D4, -1F); //No Aimcone
                                        Memory.WriteVirtualMemory<float>((IntPtr)Held + 0x2D8, -1F); //No Aimcone

                                        UInt64 RecoilPropert = Memory.ReadVirtualMemory<UInt64>((IntPtr)Held + 0x2C0);

                                        Memory.WriteVirtualMemory<float>((IntPtr)RecoilPropert + 0x18, 0f);
                                        Memory.WriteVirtualMemory<float>((IntPtr)RecoilPropert + 0x1C, 0f);
                                        Memory.WriteVirtualMemory<float>((IntPtr)RecoilPropert + 0x20, 0f);
                                        Memory.WriteVirtualMemory<float>((IntPtr)RecoilPropert + 0x24, 0f);
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
            }
            Thread.Sleep(-1);
        }
       

    }
}
