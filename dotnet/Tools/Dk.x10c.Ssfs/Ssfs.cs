using System;
using System.Collections.Generic;
using System.IO;

namespace Dk.x10c.Ssfs
{
    public class Ssfs
    {
        public Ssfs()
            : this(new Disk())
        {
        }

        public Ssfs(string path)
            : this(new Disk(path))
        {
        }

        public Ssfs(Stream s)
            : this(new Disk(s))
        {
        }

        public Ssfs(Disk disk)
        {
            this.Disk = disk;
        }

        public Disk Disk { get; set; }

        public bool ReadOnly
        {
            get
            {
                return Disk.ReadOnly;
            }
            set
            {
                Disk.ReadOnly = value;
            }
        }

        public void Format(ushort[] bootloader = null)
        {
            formatDisk(Disk, bootloader);
        }

        public void AddFile(string path, ushort[] contents)
        {
            addFileToDisk(Disk, path, contents);
        }

        public List<FileEntry> ListFiles()
        {
            return listDiskFiles(Disk);
        }

        public IEnumerable<FileEntry> IterFiles()
        {
            return iterDiskFiles(Disk);
        }

        public void WriteTo(string path, bool raw = false)
        {
            Disk.WriteTo(path, raw);
        }

        public void WriteTo(Stream s, bool raw = false)
        {
            Disk.WriteTo(s, raw);
        }

        private void formatDisk(Disk disk, ushort[] bootloader)
        {
            // How many sectors will the bootloader require?
            var bootsecs = 0;
            if (bootloader != null)
                bootsecs = (32 + bootloader.Length + disk.SectorSize - 1)
                    / disk.SectorSize;

            // Where will the directory listing go?
            var dirsec = bootsecs + 1;

            // Zero out the directory sectors.
            for (var i = 0; i < 8; ++i)
                disk.Sector[dirsec + i].Fill(0);

            // Link unused sectors together.
            var secs = disk.Sectors;
            for (var i = dirsec + 8; i < secs; ++i)
            {
                disk.Sector[i].Fill(0);
                if (i < secs - 1)
                    disk.Sector[i].Set(0, (ushort)(i + 1));
            }

            // Write header
            var header = disk.Sector[0];
            header.Set(0, 0);
            header.Set(1, 0);
            "SSFS".PackInto(header, 2);
            header.Set(4, 0x0100);
            header.Set(5, (ushort)(dirsec + 8));
            header.Set(6, (ushort)dirsec);
            
            // Write bootloader.
            if (bootloader != null)
            {
                header.Set(0, BootJump0);
                header.Set(1, BootJump1);
                bootloader.CopyTo(disk.Words, 32);
            }

            // Done!
        }

        public struct FileEntry
        {
            public string Path;
            public ushort StartSector, Length;
        }

        private List<FileEntry> listDiskFiles(Disk disk)
        {
            var l = new List<FileEntry>();
            foreach (var entry in iterDiskFiles(disk))
            {
                l.Add(entry);
            }
            return l;
        }

        private IEnumerable<FileEntry> iterDiskFiles(Disk disk)
        {
            var dirstart = disk.Words[6] * disk.SectorSize;
            var stop = dirstart + 512 * 8;
            for (var i = dirstart; i < stop; i += 8)
            {
                if (disk.Words[i] == 0)
                    break;
                else if (disk.Words[i] == 0xffff)
                    continue;

                var name = disk.Words.UnpackString(i, 4);
                var ext = disk.Words.UnpackString(i+4, 2);

                var path = name + (ext.Length > 0 ? "." + ext : "");

                yield return new FileEntry()
                {
                    Path = path,
                    StartSector = disk.Words[i+6],
                    Length = disk.Words[i+7],
                };
            }
        }

        private void addFileToDisk(Disk disk, string filename, ushort[] contents)
        {
            if (contents.Length > 0xffff)
                throw new SsfsException("file too long");

            // How many sectors do we need?
            var numsecs = (contents.Length + disk.SectorSize - 1) / disk.SectorSize;

            var startsec = 0;

            if (numsecs > 0)
            {
                // Find that many free sectors.
                var secs = new ushort[numsecs];
                {
                    var cur = disk.Words[5];
                    var sec = 0;
                    while (cur != 0 && sec < numsecs)
                    {
                        secs[sec++] = cur;
                        cur = disk.Sector[cur].Get(0);
                    }

                    if (sec != numsecs)
                        throw new SsfsException("not enough free sectors");
                }

                startsec = secs[0];

                // Write data to sectors.
                {
                    var coff = 0;
                    for (var s = 0; s < numsecs; ++s)
                    {
                        var sec = disk.Sector[secs[s]];
                        var stop = Math.Min(coff + 511, contents.Length);
                        for (var i = 1; coff < stop; ++i, ++coff)
                            sec.Set(i, contents[coff]);
                    }
                }

                // Unlink last sector, fix first free sector.
                disk.Words[5] = disk.Sector[secs[numsecs - 1]].Get(0);
                disk.Sector[secs[numsecs - 1]].Set(0, 0);
            }

            // Find a free directory entry.
            var dirent = -1;
            var dirstart = disk.Words[6] * disk.SectorSize;
            for (int i = 0; i < MaxDirEnts; ++i)
            {
                if (disk.Words[dirstart + 8 * i] == 0)
                {
                    dirent = i;
                    break;
                }
            }

            if (dirent < 0)
                throw new SsfsException("no free directory entries");

            // Write directory entry
            var direntoff = dirstart + dirent * 8;
            for (var i = 0; i < 6; ++i)
                disk.Words[direntoff + i] = 0;

            {
                var name = System.IO.Path.GetFileNameWithoutExtension(filename).ToUpper();
                var ext = System.IO.Path.GetExtension(filename).ToUpper();

                // Remove leading dot.
                if (ext.Length > 0) ext = ext.Substring(1);

                if (name.Length > 8)
                    throw new SsfsException("file name too long");
                if (ext.Length > 4)
                    throw new SsfsException("file extension too long");

                name.PackInto(disk.Words, direntoff);
                ext.PackInto(disk.Words, direntoff + 4);

                disk.Words[direntoff + 6] = (ushort)startsec;
                disk.Words[direntoff + 7] = (ushort)contents.Length;
            }
        }

        private const ushort BootJump0 = 0xff82;
        private const ushort BootJump1 = 0x0000;

        private const int HeaderSize = 32;
        private const int MaxDirEnts = 512;
    }

    [Serializable]
    public class SsfsException : Exception
    {
        public SsfsException() { }
        public SsfsException(string message) : base(message) { }
        public SsfsException(string message, Exception inner) : base(message, inner) { }
        protected SsfsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}

static class Extensions
{
    public static ushort Get(this ArraySegment<ushort> a, int index)
    {
        if (!(0 <= index && index < a.Count))
            throw new IndexOutOfRangeException();
        return a.Array[a.Offset + index];
    }

    public static void Set(this ArraySegment<ushort> a, int index, ushort value)
    {
        if (!(0 <= index && index < a.Count))
            throw new IndexOutOfRangeException();
        a.Array[a.Offset + index] = value;
    }

    public static void Fill(this ArraySegment<ushort> a, ushort fill)
    {
        var stop = a.Offset + a.Count;
        for (var i = a.Offset; i < stop; ++i)
            a.Array[i] = fill;
    }
}