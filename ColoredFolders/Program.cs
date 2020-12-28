using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace ColoredFolders
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
                return;

            bool found1 = false;
            string[] files = Directory.GetFiles(args[1]);
            foreach (string file in files)
            {
                if (file.Contains("desktop.ini"))
                {
                    found1 = true;
                    string[] cont = File.ReadAllLines(new DirectoryInfo(args[1]).FullName + "\\desktop.ini");
                    bool found2 = false;
                    for (int i = 1; i < cont.Length; i++)
                    {
                        if (cont[i].StartsWith("IconResource"))
                        {
                            found2 = true;
                            string newLine = "IconResource=" + Environment.CurrentDirectory + "\\ColoredFolders.exe," + args[0];

                            string[] arrLine = System.IO.File.ReadAllLines(new DirectoryInfo(args[1]).FullName + "\\desktop.ini");
                            arrLine[i] = newLine;
                            FileInfo fileinfo = new FileInfo(new DirectoryInfo(args[1]).FullName + "\\desktop.ini");
                            FileAttributes fa = fileinfo.Attributes;
                            FileStream fs = fileinfo.OpenWrite();
                            StreamWriter sw = new StreamWriter(fs);
                            
                            foreach (string line in arrLine)
                            {
                                sw.WriteLine(line);
                            }
                            sw.Flush();
                            sw.Close();
                            fs.Close();
                            fileinfo.Attributes = FileAttributes.Hidden | FileAttributes.Archive | FileAttributes.System;
                            RefreshIconCache();
                        }
                    }
                    if (found2)
                        return;

                    bool found3 = false;
                    for (int i = 1; i < cont.Length; i++)
                    {
                        if (cont[i].StartsWith("[.ShellClassInfo]"))
                        {
                            found3 = true;
                            string[] arrLine = File.ReadAllLines(new DirectoryInfo(args[1]).FullName + "\\desktop.ini");
                            FileInfo fileinfo = new FileInfo(new DirectoryInfo(args[1]).FullName + "\\desktop.ini");
                            FileAttributes fa = fileinfo.Attributes;
                            FileStream fs = fileinfo.OpenWrite();
                            StreamWriter sw = new StreamWriter(fs);

                            foreach (string line in arrLine)
                            {
                                sw.WriteLine(line);
                                if (line.StartsWith("[.ShellClassInfo]"))
                                    sw.WriteLine("IconResource=" + Environment.CurrentDirectory + "\\ColoredFolders.exe," + args[0]);
                            }
                            sw.Flush();
                            sw.Close();
                            fs.Close();
                            fileinfo.Attributes = FileAttributes.Hidden | FileAttributes.Archive | FileAttributes.System;
                            RefreshIconCache();
                            return;
                        }
                    }
                    if (found3 == false)
                    {
                        string[] arrLine = File.ReadAllLines(new DirectoryInfo(args[1]).FullName + "\\desktop.ini");
                        FileInfo fileinfo = new FileInfo(new DirectoryInfo(args[1]).FullName + "\\desktop.ini");
                        FileAttributes fa = fileinfo.Attributes;
                        FileStream fs = fileinfo.OpenWrite();
                        StreamWriter sw = new StreamWriter(fs);

                        sw.WriteLine("[.ShellClassInfo]");
                        sw.WriteLine("IconResource=" + Environment.CurrentDirectory + "\\ColoredFolders.exe," + args[0]);
                        foreach (string line in arrLine)
                        {
                            sw.WriteLine(line);
                        }
                        sw.Flush();
                        sw.Close();
                        fs.Close();
                        fileinfo.Attributes = FileAttributes.Hidden | FileAttributes.Archive | FileAttributes.System;
                        RefreshIconCache();
                    }
                }
            }
            if (found1)
                return;

            FileInfo fileinfo2 = new FileInfo(new DirectoryInfo(args[1]).FullName + "\\desktop.ini");
            FileAttributes fa2 = fileinfo2.Attributes;
            FileStream fs2 = fileinfo2.OpenWrite();
            StreamWriter sw2 = new StreamWriter(fs2);

            sw2.WriteLine("[.ShellClassInfo]");
            sw2.WriteLine("IconResource=" + Environment.CurrentDirectory + "\\ColoredFolders.exe," + args[0]);
            sw2.WriteLine("[ViewState]");
            sw2.WriteLine("Mode=");
            sw2.WriteLine("Vid=");
            sw2.WriteLine("FolderType=Documents");
            sw2.Flush();
            sw2.Close();
            fs2.Close();
            fileinfo2.Attributes = FileAttributes.Hidden | FileAttributes.Archive | FileAttributes.System;
            RefreshIconCache();
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(
            int windowHandle,
            int Msg,
            int wParam,
            String lParam,
            SendMessageTimeoutFlags flags,
            int timeout,
            out int result);
        [Flags]
        enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8
        }

        static void RefreshIconCache()
        {

            // get the the original Shell Icon Size registry string value
            RegistryKey k = Registry.CurrentUser.OpenSubKey("Control Panel").OpenSubKey("Desktop").OpenSubKey("WindowMetrics", true);
            Object OriginalIconSize = k.GetValue("Shell Icon Size");

            // set the Shell Icon Size registry string value
            k.SetValue("Shell Icon Size", (Convert.ToInt32(OriginalIconSize) + 1).ToString());
            k.Flush(); k.Close();

            // broadcast WM_SETTINGCHANGE to all window handles
            int res = 0;
            SendMessageTimeout(0xffff, 0x001A, 0, "", SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 5000, out res);
            //SendMessageTimeout(HWD_BROADCAST,WM_SETTINGCHANGE,0,"",SMTO_ABORTIFHUNG,5 seconds, return result to res)

            // set the Shell Icon Size registry string value to original value
            k = Registry.CurrentUser.OpenSubKey("Control Panel").OpenSubKey("Desktop").OpenSubKey("WindowMetrics", true);
            k.SetValue("Shell Icon Size", OriginalIconSize);
            k.Flush(); k.Close();

            SendMessageTimeout(0xffff, 0x001A, 0, "", SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 5000, out res);

        }

    }
}
