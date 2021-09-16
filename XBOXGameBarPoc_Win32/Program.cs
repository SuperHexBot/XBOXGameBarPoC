using System;
using System.IO.MemoryMappedFiles;
using Windows.Storage;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
namespace XBOXGameBarPoC_Win32
{
    static class Program
    {
        static void Main()
        {
            MemoryMappedFile memoryMappedFileCPP;

            while (true)
            {
                try
                {
                    Debugger.Log(0, null, "TryOpen");
                    memoryMappedFileCPP = MemoryMappedFile.OpenExisting($"215a4sd5a4e87d4a54da8sd4a5s4da8s54eda874dsa5");
                    using (MemoryMappedViewAccessor viewAccessor = memoryMappedFileCPP.CreateViewAccessor())
                    {
                        Debugger.Log(0, null, "Opened");
                        string AppSidName = ApplicationData.Current.LocalSettings.Values["AppSID"].ToString();
                        byte[] asciiBytes = Encoding.ASCII.GetBytes(AppSidName);
                        viewAccessor.WriteArray(0, asciiBytes, 0, asciiBytes.Length);
                        Debugger.Log(0, null, AppSidName);
                        break;
                    }

                }
                catch (Exception)
                {
                    // MemoryMappedFile or Mutex has not yet been created
                }
                Thread.Sleep(1000);
            }
        }
    }
}
