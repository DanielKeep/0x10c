using System;
using System.Collections.Generic;
using System.IO;

// System.IO.Compress is complete garbage.
using Ionic.Zlib;

namespace Dk.x10c.Ssfs
{
    public class Disk
    {
        public Disk()
        {
            _words = new ushort[1440 * 512];
        }

        public Disk(string path)
            : this()
        {
            using (var fs = new FileStream(path, FileMode.Open))
                readFromStream(fs);
        }

        public Disk(Stream disk)
            : this()
        {
            readFromStream(disk);
        }

        public ushort[] Words
        {
            get
            {
                return _words;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return _readOnly;
            }
            set
            {
                _readOnly = value;
            }
        }

        public int SectorSize
        {
            get { return 512; }
        }

        public int Sectors
        {
            get { return 1440; }
        }

        public DiskSectors Sector
        {
            get
            {
                return new DiskSectors(this, 512);
            }
        }

        public void ReadFrom(string path, bool raw = false)
        {
            using (var fs = new FileStream(path, FileMode.Open))
                ReadFrom(fs, raw);
        }

        public void ReadFrom(Stream s, bool raw = false)
        {
            readFromStream(s);
        }

        public void WriteTo(string path, bool raw = false)
        {
            using (var fs = new FileStream(path, FileMode.Create))
                WriteTo(fs, raw);
        }

        public void WriteTo(Stream s, bool raw = false)
        {
            writeToStream(s, raw, false, ReadOnly);
        }

        private ushort[] _words;
        private bool _readOnly = false;

        private void readFromStream(Stream s)
        {
            var headers = new Dictionary<string, string>();
            ushort[] disk;

            disk = BinaryImage.ReadImage(s, headers);
            
            if (disk.Length != 1440 * 512)
                throw new InvalidDataException("disk image not expected size");

            _words = disk;
            _readOnly = false;

            foreach (var kv in headers)
            {
                switch (kv.Key)
                {
                    case "type":
                        if (kv.Value.ToLower() != "floppy")
                            throw new InvalidDataException("disk is not a floppy");
                        break;

                    case "access":
                        switch (kv.Value.ToLower())
                        {
                            case "read-write":
                                _readOnly = false;
                                break;

                            case "read-only":
                                _readOnly = true;
                                break;
                        }
                        break;
                }
            }
        }

        private void writeToStream(Stream s, bool raw, bool le, bool readOnly)
        {
            if (raw)
            {
                BinaryImage.WriteRawImage(s, _words,
                    le ? ByteOrder.LittleEndian : ByteOrder.BigEndian);
            }
            else
            {
                var headers = new Dictionary<string, string>()
                {
                    {"Type", "Floppy"},
                    {"Access", readOnly ? "Read-Only" : "Read-Write"},
                };
                BinaryImage.WriteImage(s, _words, headers);
            }
        }

        private void readFromStreamOld(Stream s)
        {
            using (var tr = new StreamReader(s, System.Text.Encoding.ASCII))
            {
                // Defaults
                bool deflate = true;
                bool base64 = true;
                _readOnly = false;
                int contLength = -1;

                // These are used to control big/little endian reading mode.
                int lb = 0, hb = 1;

                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0)
                        break;

                    var parts = line.Split(headerSeps, 2, StringSplitOptions.None);

                    switch (parts[0].ToLower())
                    {
                        case "access":
                            if (parts.Length > 1)
                            {
                                switch (parts[1].ToLower())
                                {
                                    case "read-only":
                                        _readOnly = true;
                                        break;

                                    case "read-write":
                                        _readOnly = false;
                                        break;
                                }
                            }
                            break;

                        case "byte-order":
                                if (parts.Length > 1)
                                {
                                    switch (parts[1].ToLower())
                                    {
                                        case "little-endian":
                                            lb = 0;
                                            hb = 1;
                                            break;

                                        case "big-endian":
                                            lb = 1;
                                            hb = 0;
                                            break;
                                    }
                                }
                                break;

                        case "compression":
                                if (parts.Length > 1)
                                {
                                    switch (parts[1].ToLower())
                                    {
                                        case "none":
                                            deflate = false;
                                            break;

                                        case "zlib":
                                            deflate = true;
                                            break;
                                    }
                                }
                                break;

                        case "encoding":
                                if (parts.Length > 1)
                                {
                                    switch (parts[1].ToLower())
                                    {
                                        case "none":
                                            base64 = false;
                                            break;

                                        case "base64":
                                            base64 = true;
                                            break;
                                    }
                                }
                                break;

                        case "content-length":
                                if (parts.Length > 1)
                                {
                                    if (!int.TryParse(parts[1], out contLength))
                                        contLength = -1;
                                }
                                break;
                    }
                }

                byte[] cbytes, bytes;

                if (base64)
                {
                    if (contLength >= 0)
                    {
                        var chs = new char[contLength];
                        tr.Read(chs, 0, contLength);
                        cbytes = Convert.FromBase64CharArray(chs, 0, contLength);
                    }
                    else
                    {
                        var encoded = tr.ReadToEnd();
                        cbytes = Convert.FromBase64String(encoded);
                    }
                }
                else
                {
                    if (contLength >= 0)
                    {
                        var chs = new char[contLength];
                        tr.Read(chs, 0, contLength);
                        cbytes = new byte[contLength];
                        for (var i = 0; i < contLength; ++i)
                            cbytes[i] = (byte)chs[i];
                    }
                    else
                    {
                        var encoded = tr.ReadToEnd();
                        cbytes = new byte[encoded.Length];
                        for (var i = 0; i < encoded.Length; ++i)
                            cbytes[i] = (byte)encoded[i];
                    }
                }

                bytes = new byte[1440 * 1024];

                if (deflate)
                {
                    using (var cmpr = new ZlibStream(new MemoryStream(cbytes), CompressionMode.Decompress))
                    {
                        if (cmpr.Read(bytes, 0, bytes.Length) != bytes.Length)
                            throw new NotImplementedException("lazy programmer");
                    }
                }
                else
                {
                    bytes = cbytes;
                }

                if ((bytes.Length & 1) == 1)
                    throw new InvalidDataException("malformed disk image: last word is incomplete");

                if ((bytes.Length != (_words.Length * 2)))
                    throw new InvalidDataException("disk image not expected size");

                for (int i = 0; i < _words.Length; ++i)
                {
                    var j = 2*i;
                    _words[i] = (ushort)(bytes[j + lb] + (bytes[j + hb] << 8));
                }
            }
        }

        private void writeToStreamOld(Stream s, bool le, bool readOnly)
        {
            bool compress = true;
            bool base64 = true;

            using (var tw = new StreamWriter(s, System.Text.Encoding.ASCII))
            {
                tw.NewLine = "\n";
                tw.WriteLine("Type: Floppy");

                if (readOnly)
                    tw.WriteLine("Access: Read-Only");
                else
                    tw.WriteLine("Access: Read-Write");

                if (le)
                    tw.WriteLine("Byte-Order: Little-Endian");
                else
                    tw.WriteLine("Byte-Order: Big-Endian");

                int lb = 0, hb = 1;
                if (!le)
                {
                    lb = 1;
                    hb = 0;
                }

                byte[] bytes, cbytes;

                bytes = new byte[1440 * 1024];
                for (int i = 0; i < _words.Length; ++i)
                {
                    var j = 2 * i;
                    bytes[j + lb] = (byte)_words[i];
                    bytes[j + hb] = (byte)(_words[i] >> 8);
                }

                if (compress)
                {
                    tw.WriteLine("Compression: Zlib");
                    using (var ms = new MemoryStream())
                    {
                        using (var cmpr = new ZlibStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression, leaveOpen: true))
                        {
                            cmpr.Write(bytes, 0, bytes.Length);
                            cmpr.Flush();
                        }
                        cbytes = ms.ToArray();
                    }
                }
                else
                {
                    tw.WriteLine("Compression: None");
                    cbytes = bytes;
                }

                if (base64)
                {
                    var encoded = Convert.ToBase64String(cbytes, Base64FormattingOptions.InsertLineBreaks);

                    tw.WriteLine("Encoding: Base64");
                    tw.WriteLine("Content-Length: {0}", encoded.Length);
                    tw.WriteLine();
                    tw.WriteLine(encoded);
                }
                else
                {
                    tw.WriteLine("Encoding: None");
                    tw.WriteLine("Content-Length: {0}", cbytes.Length);
                    tw.WriteLine();
                    tw.Flush();
                    tw.BaseStream.Write(bytes, 0, cbytes.Length);
                }
            }
        }

        static Disk()
        {
            headerSeps = new char[] { ':' };
        }

        private static readonly char[] headerSeps;
    }

    public struct DiskSectors
    {
        internal DiskSectors(Disk disk, int sectorWords) : this()
        {
            this.SectorSize = sectorWords;
            this.Disk = disk;
        }

        public Disk Disk
        {
            get;
            private set;
        }

        public int SectorSize
        {
            get;
            private set;
        }

        public ArraySegment<ushort> this[int sector]
        {
            get
            {
                return new ArraySegment<ushort>(Disk.Words, sector * SectorSize, SectorSize);
            }
        }
    }
}
