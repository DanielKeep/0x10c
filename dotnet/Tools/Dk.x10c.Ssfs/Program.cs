using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NDesk.Options;

namespace Dk.x10c.Ssfs
{
    class SsfsTool
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var cmd = args[0];
            var arglist = new List<string>(args.Length - 1);
            for (var i = 1; i < args.Length; ++i)
                arglist.Add(args[i]);

            switch (cmd.ToLower())
            {
                case "create":
                    CreateCmd(arglist);
                    break;

                case "list":
                    ListCmd(arglist);
                    break;

                case "--help":
                    ShowHelp();
                    return;

                default:
                    Console.Error.WriteLine("Unknown command {0}.", cmd);
                    ShowCommands();
                    break;
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: ssfs COMMAND ...");
            ShowCommands();
        }

        static void ShowCommandHelp(string cmdUsage, string desc, OptionSet opt)
        {
            Console.WriteLine("Usage: ssfs {0}", cmdUsage);
            Console.WriteLine(desc);
            Console.WriteLine();
            Console.WriteLine("Options:");
            opt.WriteOptionDescriptions(Console.Out);
        }

        static void ShowCommandError(string cmd, string message)
        {
            Console.WriteLine("ssfs {0}: {1}", cmd, message);
            Console.WriteLine("Try `ssfs {0} --help' for more information.", cmd);
        }

        static void ShowCommands()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine( "  create    - Create a new disk image.");
            Console.WriteLine( "  list      - List files on a disk.");
        }

        #if DISABLED
        static void MainTest(string[] args)
        {
            {
                var disk = new Disk();
                "Hello, World!".PackInto(disk.Words, 0);
                disk.WriteTo("hello.disk.txt");
                disk.WriteTo("hello.disk.bin", raw: true);
            }

            int __break = 1;

            {
                var disk = new Disk("hello.disk.txt");
                var msg = disk.Words.UnpackString(0);
                Console.WriteLine("Message: \"{0}\"", msg);
            }

            {
                var disk = new Disk();
                disk.ReadFrom("hello.disk.bin", raw: true);
                var msg = disk.Words.UnpackString(0);
                Console.WriteLine("Message: \"{0}\"", msg);
            }

            {
                var fs = new Ssfs();

                fs.Format(BinaryImage.ReadImage("boot/bootload.bin"));
                fs.AddFile("kernel.sys", BinaryImage.ReadImage("boot/kernel.sys"));
                fs.AddFile("hmd2043.drv", BinaryImage.ReadImage("boot/hmd2043.drv"));
                fs.AddFile("ssfs.drv", BinaryImage.ReadImage("boot/ssfs.drv"));
                fs.AddFile("shell.sys", BinaryImage.ReadImage("boot/shell.sys"));
                fs.AddFile("shell.ro", BinaryImage.ReadImage("boot/shell.ro"));

                fs.WriteTo("boot.disk.txt");
                fs.WriteTo("boot.disk.bin", raw: true);
            }

            {
                var fs = new Ssfs("boot.disk.txt");
                Console.WriteLine("List of files in boot.disk.txt:");
                foreach (var file in fs.IterFiles())
                    Console.WriteLine(" {0}", file.Path);
            }
        }
        #endif

        static void CreateCmd(IEnumerable<string> args)
        {
            Access access = Access.ReadWrite;
            string bootloader = "";
            //ByteOrder byteOrder = ByteOrder.BigEndian;
            //Compression compression = Compression.Zlib;
            //var encoding = Dk.x10c.Encoding.Base64;
            List<string> priorityList = new List<string>();
            //bool raw = false;
            bool help = false;

            string disk = null,
                   root = null;

            var opt = new OptionSet()
            {
                {"a|access=", "the {ACCESS} permissions on the disk.",
                    (Access a) => access = a},
                {"b|bootloader=", "the {FILE} to use as a boot loader.",
                    a => bootloader = a},
                /*{"byte-order=", "{ENDIANNESS} of raw output.",
                    (ByteOrder a) => byteOrder = a},*/
                /*{"c|compression=", "{COMPRESSION} type to use.",
                    (Compression a) => compression = a},*/
                /*{"e|encoding=", "{ENCODING} type to use.",
                    (Dk.x10c.Encoding a) => encoding = a},*/
                {"p|priority-list=", "{FILE} containing list of files to add to filesystem first.",
                    a => UpdatePriorityList(priorityList, a)},
                {"help", "This message.",
                    a => help = (a != null)},
            };

            List<string> pargs = null;
            try
            {
                pargs = opt.Parse(args);
            }
            catch (OptionException e)
            {
                ShowCommandError("create", e.Message);
                return;
            }

            if (!help)
                switch (pargs.Count)
                {
                    case 1:
                        // ssfs create [OPTIONS] ROOT
                        root = pargs[0];
                        disk = Path.ChangeExtension(root, "disk");
                        break;

                    case 2:
                        // ssfs create [OPTIONS] ROOT DISK
                        root = pargs[0];
                        disk = pargs[1];
                        break;

                    default:
                        ShowCommandError("create", "invalid number of arguments");
                        return;
                }

            if (help)
            {
                ShowCommandHelp("create [OPTIONS] ROOT [DISK]",
                    "Creates a new SSFS disk image using the files in directory ROOT.",
                    opt);
                return;
            }

            ushort[] blData = null;
            if (bootloader != "")
                blData = BinaryImage.ReadImage(bootloader);

            var fs = new Ssfs();
            fs.Format(blData);

            var prioritySet = new HashSet<string>(priorityList);

            foreach (var name in priorityList)
            {
                var path = Path.Combine(root, name);
                
                ushort[] data = null;
                //*
                try
                {
                    data = BinaryImage.ReadImage(path);
                }
                catch (BiefException e)
                {
                    Console.Error.Write("Error occured while reading file {0}: ", name);
                    Console.Error.WriteLine(e.Message);
                    Environment.Exit(1);
                }
                /*/
                data = BinaryImage.ReadImage(path);
                //*/

                try
                {
                    fs.AddFile(name, data);
                }
                catch (SsfsException e)
                {
                    Console.Error.Write("Error occurred while adding file {0}: ", name);
                    Console.Error.WriteLine(e.Message);
                    Environment.Exit(1);
                }
            }

            foreach (var name in from f in Directory.EnumerateFiles(root)
                                 where !prioritySet.Contains(Path.GetFileName(f))
                                 select Path.GetFileName(f))
            {
                var path = Path.Combine(root, name);
                
                ushort[] data = null;
                //*
                try
                {
                    data = BinaryImage.ReadImage(path);
                }
                catch (BiefException e)
                {
                    Console.Error.Write("Error occured while reading file {0}: ", name);
                    Console.Error.WriteLine(e.Message);
                    Environment.Exit(1);
                }
                /*/
                data = BinaryImage.ReadImage(path);
                //*/

                try
                {
                    fs.AddFile(name, data);
                }
                catch (SsfsException e)
                {
                    Console.Error.Write("Error occurred while adding file {0}: ", name);
                    Console.Error.WriteLine(e.Message);
                    Environment.Exit(1);
                }
            }

            fs.Disk.WriteTo(disk);
        }

        static void ListCmd(IEnumerable<string> args)
        {
            bool help = false;
            string disk = null;

            var opt = new OptionSet()
            {
                {"help", "This message.",
                    a => help = (a != null)},
            };

            List<string> pargs = null;
            try
            {
                pargs = opt.Parse(args);
            }
            catch (OptionException e)
            {
                ShowCommandError("list", e.Message);
                return;
            }

            if (!help)
                switch (pargs.Count)
                {
                    case 1:
                        // ssfs list DISK
                        disk = pargs[0];
                        break;

                    default:
                        ShowCommandError("list", "invalid number of arguments");
                        return;
                }

            if (help)
            {
                ShowCommandHelp("ssfs list DISK",
                    "Lists the contents of the given DISK image.",
                    opt);
                return;
            }

            var fs = new Ssfs(disk);
            var files = fs.ListFiles();
            files.Sort((a, b) => a.Path.CompareTo(b.Path));
            foreach (var file in files)
                Console.WriteLine(file.Path);
        }

        static void UpdatePriorityList(List<string> list, string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                using (var tr = new StreamReader(fs, System.Text.Encoding.UTF8))
                {
                    string line;
                    while ((line = tr.ReadLine()) != null)
                    {
                        var trimmed = line.Trim();
                        if (trimmed.Length > 0)
                            list.Add(line.Trim());
                    }
                }
            }
        }
    }
}
