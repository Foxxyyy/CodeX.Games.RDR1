using System;
using System.IO;
using System.Collections.Generic;
using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using static CodeX.Games.RDR1.RPF6.Rpf6Crypto;
using System.Linq;

namespace CodeX.Games.RDR1.RPF6
{
    [TC(typeof(EXP))]
    public class Rpf6File : GameArchive
    {
        public bool EntriesHaveBeenChanged { get; set; } = false;
        public long StartPos { get; set; }
        public uint Version { get; set; } //RPF Version - 0x52504636 (910577746)
        public uint EntryCount { get; set; } //Number of entries
        public uint TOCSize { get; set; } //Size of table of content (EntryCount * 20)
        public uint StringTableOffset { get; set; } //For the Nintendo Switch it is actually used but is not necessary
        public int EncFlag { get; set; } //Encryption flag
        public bool Encrypted
        {
            get => (uint)EncFlag > 0U;
            set
            {
                if (value)
                    EncFlag = -3;
                else
                    EncFlag = 0;
            }
        }

        public Rpf6File(string fpath, string relpath)
        {
            var fi = new FileInfo(fpath);
            Name = fi.Name;
            Path = relpath.ToLowerInvariant();
            FilePath = fpath;
            Size = fi.Length;
        }

        private void ReadHeader(BinaryReader br)
        {
            StartPos = br.BaseStream.Position;
            Version = Swap(br.ReadUInt32());
            EntryCount = Swap(br.ReadUInt32());
            TOCSize = (uint)((((EntryCount << 2) + EntryCount << 2) + 15) & 4294967280L);
            StringTableOffset = Swap(br.ReadUInt32());
            EncFlag = Swap(br.ReadInt32());

            if (Version != 0x52504636)
            {
                var verbytes = BitConverter.GetBytes(Version); Array.Reverse(verbytes);
                var versionstr = BitConverter.ToString(verbytes);
                throw new Exception("Invalid RPF6 archive - found \"" + versionstr + "\" instead.");
            }

            byte[] entriesdata = br.ReadBytes((int)TOCSize);
            if (Encrypted)
            {
                entriesdata = DecryptAES(entriesdata);
            }

            var entriesrdr = new BinaryReader(new MemoryStream(entriesdata));
            AllEntries = new List<GameArchiveEntry>();

            for (uint i = 0; i < EntryCount; i++)
            {
                var e = Rpf6Entry.ReadEntry(this, entriesrdr);
                e.StartIndex = (int)i;
                AllEntries.Add(e);
            }
            CreateDirectories();
        }

        private void CreateDirectories()
        {
            var r = (Rpf6DirectoryEntry)AllEntries[0];
            Root = r;
            Root.Path = Path.ToLowerInvariant();
            var stack = new Stack<Rpf6DirectoryEntry>();
            stack.Push(r);

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                int starti = (int)item.EntriesIndex;
                int endi = (int)(item.EntriesIndex + item.EntriesCount);
                item.Children = new List<Rpf6Entry>();

                for (int i = starti; i < endi; i++)
                {
                    var e = AllEntries[i];
                    e.Parent = item;
                    ((Rpf6Entry)e).EntryParent = item;

                    if (e is Rpf6DirectoryEntry rde)
                    {
                        rde.Path = item.Path + "\\" + rde.NameLower;
                        item.Directories.Add(rde);
                        item.Children.Add(rde);
                        stack.Push(rde);
                    }
                    else if (e is Rpf6FileEntry rfe)
                    {
                        rfe.Path = item.Path + "\\" + rfe.NameLower;
                        item.Files.Add(rfe);
                        item.Children.Add(rfe);
                    }
                }
            }
        }

        public static void RenameArchive(Rpf6File file, string newname)
        {
            //updates all items in the RPF with the new path - no actual file changes made here
            //(since all the paths are generated at runtime and not stored)

            file.Name = newname;
            file.Path = GetParentPath(file.Path) + newname;
            file.FilePath = GetParentPath(file.FilePath) + newname;
            file.UpdatePaths();
        }

        public static void RenameEntry(Rpf6Entry entry, string newname)
        {
            //rename the entry in the RPF header... 
            //also make sure any relevant child paths are updated...

            string dirpath = GetParentPath(entry.Path);

            entry.Name = newname;
            entry.NameOffset = JenkHash.GenHash(newname);
            entry.Path = dirpath + newname;

            string sname = entry.ShortNameLower;
            JenkIndex.Ensure(sname, "RDR1"); //could be anything... but it needs to be there

            var parent = (Rpf6File)entry.Archive;
            string fpath = parent.GetPhysicalFilePath();

            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using var bw = new BinaryWriter(fstream);
                parent.EnsureAllEntries();
                parent.WriteHeader(bw);
            }

            if (entry is Rpf6DirectoryEntry dir)
            {
                //A folder was renamed, make sure all its children's paths get updated
                parent.UpdatePaths(dir);
            }
        }

        private void UpdatePaths(Rpf6DirectoryEntry dir = null)
        {
            //Recursively update paths, including in child RPFs.
            if (dir == null)
            {
                Root.Path = Path.ToLowerInvariant();
                dir = (Rpf6DirectoryEntry)Root;
            }
            
            foreach (var file in dir.Files)
            {
                file.Path = dir.Path + "\\" + file.NameLower;

                if ((file is Rpf6ResourceFileEntry binf) && file.NameLower.EndsWith(".rpf"))
                {
                    if (FindChildArchive(binf) is Rpf6File childrpf)
                    {
                        childrpf.Path = binf.Path;
                        childrpf.FilePath = binf.Path;
                        childrpf.UpdatePaths();
                    }
                }

            }

            foreach (Rpf6DirectoryEntry subdir in dir.Directories.Cast<Rpf6DirectoryEntry>())
            {
                subdir.Path = dir.Path + "\\" + subdir.NameLower;
                UpdatePaths(subdir);
            }
        }

        public static void DeleteEntry(Rpf6Entry entry)
        {
            //Delete this entry from the RPF header.
            //Also remove any references to this item in its parent directory...
            //If this is a directory entry, this will delete the contents first

            var parent = (Rpf6File)entry.Archive;
            string fpath = parent.GetPhysicalFilePath();
            if (!File.Exists(fpath))
            {
                throw new Exception("Root RPF file " + fpath + " does not exist!");
            }

            var entryasdir = entry as Rpf6DirectoryEntry;
            if (entryasdir != null)
            {
                var deldirs = entryasdir.Directories.ToArray();
                var delfiles = entryasdir.Files.ToArray();
                foreach (Rpf6DirectoryEntry deldir in deldirs.Cast<Rpf6DirectoryEntry>())
                {
                    DeleteEntry(deldir);
                }
                foreach (Rpf6FileEntry delfile in delfiles.Cast<Rpf6FileEntry>())
                {
                    DeleteEntry(delfile);
                }
            }

            if (entry.Parent == null)
            {
                throw new Exception("Parent directory is null! This shouldn't happen - please refresh the folder!");
            }

            if (entryasdir != null)
            {
                entry.Parent.Directories.Remove(entryasdir);
                ((Rpf6DirectoryEntry)entry.Parent).Children.Remove(entryasdir);
            }
            if (entry is Rpf6FileEntry entryasfile)
            {
                entry.Parent.Files.Remove(entryasfile);
                ((Rpf6DirectoryEntry)entry.Parent).Children.Remove(entryasfile);

                var child = parent.FindChildArchive(entryasfile);
                if (child != null)
                {
                    parent.Children.Remove(child); //RPF file being deleted...
                }
            }

            using var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite);
            using var bw = new BinaryWriter(fstream);
            parent.EnsureAllEntries();
            parent.WriteHeader(bw);
        }

        private void EnsureAllEntries()
        {
            if (AllEntries == null)
            {
                AllEntries = new List<GameArchiveEntry>(); //Assume this is a new RPF, create the root directory entry
                Root = new Rpf6DirectoryEntry
                {
                    Archive = this,
                    Name = string.Empty,
                    Path = Path.ToLowerInvariant()
                };
            }

            Children ??= new List<GameArchive>();
            var newSupers = new List<GameArchiveEntry>();
            var sortSupers = (Action<GameArchiveEntry>)null;

            sortSupers = entry =>
            {
                if (!newSupers.Contains(entry))
                    newSupers.Add(entry);

                var fileEntry = entry as Rpf6Entry;
                if (fileEntry.IsDirectory)
                {
                    var e = (Rpf6DirectoryEntry)entry;
                    if (e.Children != null && e.Children.Count > 0)
                    {
                        e.StartIndex = newSupers.Count;
                        newSupers.AddRange(e.Children.OrderBy(o => o.NameOffset.Hash));
                        
                        foreach (var child in e.Children)
                            sortSupers(child);
                    }
                }
            };
            sortSupers(AllEntries[0]);
            AllEntries.Clear();

            foreach (Rpf6Entry superEntry in newSupers.Cast<Rpf6Entry>())
            {
                if (superEntry.IsDirectory)
                {
                    var asDirectory = superEntry as Rpf6DirectoryEntry;
                    asDirectory.EntriesIndex = (uint)superEntry.StartIndex;
                    asDirectory.EntriesCount = (uint)superEntry.Children.Count;
                }
                AllEntries.Add(superEntry);
            }
            EntryCount = (uint)AllEntries.Count;
        }

        private void WriteHeader(BinaryWriter bw)
        {
            //Entries may have been updated, so need to do this after ensuring header space
            var tocdata = GetTOCData();
            if (Encrypted)
            {
                tocdata = EncryptAES(tocdata);
            }

            //Now there's enough space, it's safe to write the header data...
            bw.BaseStream.Position = StartPos;
            bw.Write(Swap(Version));
            bw.Write(Swap(EntryCount));
            bw.Write(Swap(StringTableOffset));
            bw.Write(Swap(EncFlag));
            bw.Write(tocdata);
        }

        private byte[] GetTOCData()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);

            foreach (Rpf6Entry entry in AllEntries.Cast<Rpf6Entry>())
            {
                entry.Write(bw);
            }

            byte[] temp = new byte[RoundUp(ms.Position, 16L) - ms.Position];
            bw.Write(temp);

            var buf = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buf, 0, buf.Length);

            return buf;
        }

        private void InsertFileSpace(BinaryWriter bw, Rpf6FileEntry entry)
        {
            long blockcount = entry.GetFileSize();
            long hole = FindHole(blockcount, 0, 0, entry, out long roundup);
            entry.SetOffset(hole + roundup);

            EnsureAllEntries();
            WriteHeader(bw);
        }

        private long FindHole(long reqblocks, long ignorestart, long ignoreend, Rpf6FileEntry e, out long roundup)
        {
            var allfiles = new List<Rpf6FileEntry>();
            foreach (var entry in AllEntries)
            {
                if (entry is Rpf6FileEntry rfe)
                {
                    allfiles.Add(rfe);
                }
            }
            allfiles.Sort((e1, e2) => e1.Offset.CompareTo(e2.Offset));

            (long, Rpf6FileEntry) block = FindEndBlock();

            long length = 0;
            roundup = 0;

            if (!e.FlagInfos.IsResource)
            {
                if (reqblocks >= 131072)
                {
                    length = RoundUp(block.Item1, 2048L) - block.Item1;
                    if (length == 0L)
                        length = 2048L;
                }
                else
                {
                    length = RoundUp(block.Item1, 8L) - block.Item1;
                    if (length == 0L)
                        length = 8L;
                }
            }
            else
            {
                length = RoundUp(block.Item1, 2048L) - block.Item1;
                if (length == 0L)
                    length = 2048L;
            }

            roundup = length;
            return block.Item1;
        }

        private (long, Rpf6FileEntry) FindEndBlock()
        {
            long endblock = 0;
            Rpf6FileEntry lastFile = null;

            foreach (var entry in AllEntries) //Find the next available block after all other files (or after header if there's no files)
            {
                if (entry is Rpf6FileEntry e)
                {
                    long ecnt = e.GetOffset();
                    long eend = ecnt + e.GetFileSize();

                    if (eend > endblock)
                    {
                        endblock = eend;
                        lastFile = e;
                    }
                }
            }

            if (endblock == 0) //Must be no files present, end block comes directly after the header.
            {
                endblock = 671744L;
            }
            return (endblock, lastFile);
        }

        private void UpdateStartPos(long newpos)
        {
            StartPos = newpos;
            if (Children != null)
            {
                foreach (var child in Children.Cast<Rpf6File>()) //Make sure children also get their StartPos updated !
                {
                    if (child.ParentFileInfo is not Rpf6FileEntry cpfe)
                        continue; //Shouldn't really happen...

                    var cpos = StartPos + cpfe.Offset * 8;
                    child.UpdateStartPos(cpos);
                }
            }
        }

        private static string GetParentPath(string path)
        {
            string dirpath = path.Replace('/', '\\');
            int lidx = dirpath.LastIndexOf('\\');
            if (lidx > 0)
            {
                dirpath = dirpath[..(lidx + 1)];
            }
            if (!dirpath.EndsWith("\\"))
            {
                dirpath += "\\";
            }
            return dirpath;
        }

        public override void ReadStructure(BinaryReader br)
        {
            ReadHeader(br);
            Children = new List<GameArchive>();
        }

        public override bool EnsureEditable(Func<string, string, bool> confirm)
        {
            return true;
        }

        public override byte[] ExtractFile(GameArchiveFileInfo f, bool compressed = false)
        {
            try
            {
                using BinaryReader br = new BinaryReader(File.OpenRead(GetPhysicalFilePath()));
                if (f is Rpf6FileEntry rf)
                    return ExtractFileResource(rf, br);
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] ExtractFileResource(Rpf6FileEntry entry, BinaryReader br)
        {
            br.BaseStream.Position = entry.GetOffset();
            byte[] data = br.ReadBytes((int)entry.Size);

            if (!entry.FlagInfos.IsResource && !entry.FlagInfos.IsCompressed)
                return data;

            byte[] resourceData = ResourceInfo.GetDataFromResourceBytes(data);
            if (entry.Size > 0 && entry.FlagInfos.IsCompressed && resourceData == null)
            {
                br.BaseStream.Position = entry.GetOffset();
                byte[] decr = new byte[entry.Size];
                byte[] deflated = DecompressZStandard(br.ReadBytes(decr.Length));
                return deflated;
            }
            return resourceData;
        }

        internal void ReadStartupCache(BinaryReader br)
        {
            StartPos = br.ReadInt64();
            Version = br.ReadUInt32();
            EntryCount = br.ReadUInt32();
            TOCSize = br.ReadUInt32();
            StringTableOffset = br.ReadUInt32();
            EncFlag = br.ReadInt32();

            AllEntries = new List<GameArchiveEntry>();
            var entrydict = new Dictionary<string, GameArchiveFileInfo>();
            for (int i = 0; i < EntryCount; i++)
            {
                var entry = Rpf6Entry.ReadEntry(this, br);
                AllEntries.Add(entry);
                if ((entry is GameArchiveFileInfo finfo) && (finfo.IsArchive))
                {
                    entrydict[finfo.Path.ToLowerInvariant()] = finfo;
                }
            }
            CreateDirectories();
        }

        internal void WriteStartupCache(BinaryWriter bw)
        {
            bw.Write(StartPos);
            bw.Write(Version);
            bw.Write(EntryCount);
            bw.Write(TOCSize);
            bw.Write(StringTableOffset);
            bw.Write(EncFlag);

            for (int i = 0; i < EntryCount; i++)
            {
                (AllEntries[i] as Rpf6Entry)?.Write(bw);
            }
        }

        private void WriteNewArchive(BinaryWriter bw)
        {
            var stream = bw.BaseStream;
            Version = 0x52504636; //'RPF6'
            StartPos = stream.Position;
            EnsureAllEntries();
            WriteHeader(bw);
            Size = stream.Position - StartPos;
        }

        public static Rpf6File CreateNew(string gtafolder, string relpath)
        {
            //Create a new, empty RPF file in the filesystem
            //This will assume that the folder the file is going into already exists!

            string fpath = gtafolder;
            relpath = relpath.Replace("RDR1\\\\", "");
            fpath = fpath.EndsWith("\\") ? fpath : fpath + "\\";
            fpath = fpath + relpath;

            if (File.Exists(fpath))
            {
                throw new Exception("File " + fpath + " already exists!");
            }

            File.Create(fpath).Dispose(); //Just write a placeholder, will fill it out later
            var file = new Rpf6File(fpath, relpath);

            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using (var bw = new BinaryWriter(fstream))
                {
                    file.WriteNewArchive(bw);
                }
            }
            return file;
        }

        public static Rpf6FileEntry CreateFile(Rpf6DirectoryEntry dir, string name, byte[] data, bool overwrite = true)
        {
            string namel = name.ToLowerInvariant();
            if (namel.EndsWith(".rpf"))
            {
                throw new Exception("Cannot import RPF!");
            }

            if (overwrite)
            {
                foreach (Rpf6Entry exfile in dir.Files.Cast<Rpf6Entry>())
                {
                    if (exfile.NameLower == namel)
                    {
                        //File already exists. delete the existing one first!
                        //This should probably be optimised to just replace the existing one...
                        DeleteEntry(exfile);
                        break;
                    }
                }
            }

            var parent = (Rpf6File)dir.Archive;
            string fpath = parent.GetPhysicalFilePath();
            string rpath = dir.Path + "\\" + namel;
            if (!File.Exists(fpath))
            {
                throw new Exception("Root RPF file " + fpath + " does not exist!");
            }

            Rpf6FileEntry entry = null;
            uint len = (uint)data.Length;
            uint identifier = 0;

            if (len >= 16)
                identifier = BitConverter.ToUInt32(data, 0);

            if (identifier == 2235781970U || identifier == 88298322U) //RSC85 or RSC05 header is present... import as resource
            {
                var rentry = new Rpf6ResourceFileEntry();
                var flags = new FlagInfo
                {
                    Flag1 = BitConverter.ToInt32(data, 8),
                    Flag2 = (identifier == 88298322U) ? 0 : BitConverter.ToInt32(data, 12),
                    IsResource = true,
                    IsCompressed = true,
                    IsExtendedFlags = identifier == 2235781970U
                };

                rentry.FlagInfos = flags;
                rentry.Size = len;
                rentry.ResourceType = (Rpf6FileExt)BitConverter.ToInt32(data, 4);
                entry = rentry;
            }

            if (entry == null) //no RSC6 header present, import as a binary file.
            {
                var info = new FlagInfo
                {
                    Flag1 = 0,
                    Flag2 = 0,
                    IsCompressed = false,
                    IsResource = false
                };
                info.SetTotalSize((int)len, 0);

                var bentry = new Rpf6ResourceFileEntry
                {
                    FlagInfos = info,
                    IsEncrypted = false,
                    Size = len,
                };
                entry = bentry;
            }

            entry.Parent = entry.EntryParent = dir;
            entry.Archive = parent;
            entry.Path = rpath;
            entry.Name = name;
            entry.NameOffset = JenkHash.GenHash(name);
            entry.ReadBackFromRPF = false;
            entry.IsDirectory = false;
            entry.CustomDataStream = new MemoryStream(data);
            entry.Entry = entry;

            foreach (var exfile in dir.Files)
            {
                if (exfile.NameLower == entry.NameLower)
                {
                    throw new Exception("File \"" + entry.Name + "\" already exists!");
                }
            }

            dir.Files.Add(entry);
            dir.Children.Add(entry);

            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using var bw = new BinaryWriter(fstream);
                parent.InsertFileSpace(bw, entry);

                long bbeg = parent.StartPos + entry.GetOffset();
                long bend = bbeg + entry.GetFileSize();

                fstream.Position = bbeg;
                fstream.Write(data, 0, data.Length);

                byte[] buffer = new byte[(int)(RoundUp(fstream.Position, 2048L) - fstream.Position)];
                fstream.Write(buffer, 0, buffer.Length);
            }
            return entry;
        }

        public static Rpf6DirectoryEntry CreateDirectory(Rpf6DirectoryEntry dir, string name)
        {
            var parent = (Rpf6File)dir.Archive;
            string namel = name.ToLowerInvariant();
            string fpath = parent.GetPhysicalFilePath();
            string rpath = dir.Path + "\\" + namel;

            if (!File.Exists(fpath))
            {
                throw new Exception("Root RPF file " + fpath + " does not exist!");
            }

            var entry = new Rpf6DirectoryEntry
            {
                Parent = dir,
                Archive = parent,
                Path = rpath,
                Name = name,
                NameOffset = JenkHash.GenHash(name),
                IsDirectory = true,
                Children = new List<Rpf6Entry>()
            };

            foreach (var exdir in dir.Directories)
            {
                if (exdir.NameLower == entry.NameLower)
                {
                    throw new Exception("RPF Directory \"" + entry.Name + "\" already exists!");
                }
            }

            dir.Directories.Add(entry);
            dir.Children.Add(entry);

            using (var fstream = File.Open(fpath, FileMode.Open, FileAccess.ReadWrite))
            {
                using var bw = new BinaryWriter(fstream);
                parent.EnsureAllEntries();
                parent.WriteHeader(bw);
            }
            return entry;
        }
    }

    [TC(typeof(EXP))]
    public abstract class Rpf6Entry : GameArchiveEntryBase, GameArchiveEntry
    {
        public Rpf6Entry Entry { get; set; }
        public long Offset { get; set; }
        public Rpf6DirectoryEntry EntryParent { get; set; }
        public List<Rpf6Entry> Children { get; set; }
        public Stream CustomDataStream { get; set; }
        public bool IsEncrypted { get; set; }
        public uint Flags { get; set; }
        public JenkHash NameOffset { get; set; }
        public bool IsDirectory { get; set; }
        public int StartIndex { get; set; }
        public bool ReadBackFromRPF { get; set; } = true;

        private string _Attributes;
        public override string Attributes
        {
            get
            {
                if (_Attributes == null)
                {
                    _Attributes = "";
                    if (this is Rpf6ResourceFileEntry res && res.FlagInfos.IsResource)
                    {
                        var resf = this as Rpf6FileEntry;
                        _Attributes += "Resource [V." + ((byte)resf.ResourceType).ToString() + "]";
                    }
                    if (IsEncrypted)
                    {
                        if (_Attributes.Length > 0) _Attributes += ", ";
                        _Attributes += "Encrypted";
                    }
                }
                return _Attributes;
            }
        }

        public abstract void Read(BinaryReader r);
        public abstract void Write(BinaryWriter w);

        public static Rpf6Entry ReadEntry(GameArchive archive, BinaryReader br)
        {
            br.BaseStream.Seek(8L, SeekOrigin.Current);
            byte type = br.ReadByte();

            Rpf6Entry entry;
            if (type == 128) //Directory
                entry = new Rpf6DirectoryEntry();
            else //File
                entry = new Rpf6ResourceFileEntry();

            br.BaseStream.Seek(-9L, SeekOrigin.Current);
            entry.Archive = archive;
            entry.Read(br);

            return entry;
        }

        public string GetFilePath()
        {
            var name = JenkIndex.TryGetStringNoCollision(NameOffset);
            if (name != string.Empty)
            {
                var idx = name.LastIndexOf('.');
                if (idx < 0)
                {
                    return name;
                }
                return name;
            }
            return $"0x{NameOffset.Hash:X}";
        }

        public override string ToString()
        {
            return Path;
        }
    }

    [TC(typeof(EXP))]
    public class Rpf6DirectoryEntry : Rpf6Entry, GameArchiveDirectory
    {
        public uint EntriesIndex { get; set; }
        public uint EntriesCount { get; set; }
        public int UNK { get; set; }

        public List<GameArchiveDirectory> Directories { get; set; } = new List<GameArchiveDirectory>();
        public List<GameArchiveFileInfo> Files { get; set; } = new List<GameArchiveFileInfo>();

        public override void Read(BinaryReader r)
        {
            NameOffset = Swap(r.ReadUInt32());
            Flags = Swap(r.ReadUInt32());
            EntriesIndex = (uint)(Swap(r.ReadInt32()) & int.MaxValue);
            EntriesCount = (uint)(Swap(r.ReadInt32()) & 268435455);
            UNK = Swap(r.ReadInt32());
            IsDirectory = true;

            if (NameOffset == 0U)
                Name = "root";
            else
            {
                Path = GetFilePath();
                Name = System.IO.Path.GetFileName(Path);
            }
        }

        public override void Write(BinaryWriter w)
        {
            w.Write(Swap(NameOffset));
            w.Write(Swap(Flags));
            w.Write(Swap((int)(2147483648L | (EntriesIndex & int.MaxValue))));
            w.Write(Swap(EntriesCount & 268435455));
            w.Write(Swap(UNK));
        }

        public override string ToString()
        {
            return "Directory: " + Path;
        }
    }

    [TC(typeof(EXP))]
    public abstract class Rpf6FileEntry : Rpf6Entry, GameArchiveFileInfo
    {
        public FlagInfo FlagInfos;
        public bool IsArchive { get => NameLower?.EndsWith(".rpf") ?? false; }

        public Rpf6FileExt ResourceType
        {
            get => (Rpf6FileExt)((ulong)Offset & byte.MaxValue);
            set => Offset = (Offset & -256L) | (byte)value;
        }

        public override void Read(BinaryReader r)
        {
            NameOffset = Swap(r.ReadUInt32());
            Size = Swap(r.ReadInt32()) & 268435455;
        }

        public override void Write(BinaryWriter w)
        {
            w.Write(Swap(NameOffset));
            w.Write(Swap((int)Size));
        }

        public void SetOffset(long offset)
        {
            if ((ulong)offset % 8UL > 0UL)
            {
                Offset = -1L;
                throw new Exception("INVALID_SET_OFFSET");
            }

            if (FlagInfos.IsResource)
                Offset = offset / 8L | (byte)ResourceType;
            else
                Offset = offset / 8L;
        }

        public long GetOffset()
        {
            return FlagInfos.IsResource ? (Offset & 2147483392L) * 8L : (Offset & int.MaxValue) * 8L;
        }

        public abstract long GetFileSize();
    }

    [TC(typeof(EXP))]
    public class Rpf6ResourceFileEntry : Rpf6FileEntry
    {
        public int VirtualSize
        {
            get
            {
                return FlagInfos.IsRSC05 ? FlagInfos.RSC05_GetTotalVSize : FlagInfos.RSC85_TotalVSize;
            }
        }

        public int PhysicalSize
        {
            get
            {
                return FlagInfos.IsRSC05 ? FlagInfos.RSC05_GetTotalPSize : FlagInfos.RSC85_TotalPSize;
            }
        }

        public override void Read(BinaryReader r)
        {
            base.Read(r);

            Offset = Swap(r.ReadInt32());
            FlagInfos = new FlagInfo()
            {
                Flag1 = Swap(r.ReadInt32()),
                Flag2 = Swap(r.ReadInt32())
            };

            IsDirectory = false;
            Entry = this;

            Path = GetFilePath();
            Name = System.IO.Path.GetFileName(Path);
        }

        public override void Write(BinaryWriter w)
        {
            base.Write(w);
            w.Write(Swap((uint)Offset));
            w.Write(Swap(FlagInfos.Flag1));
            w.Write(Swap(FlagInfos.Flag2));
        }

        public override long GetFileSize()
        {
            return (Size == 0) ? (VirtualSize + PhysicalSize) : Size;
        }

        public override string ToString()
        {
            return "Resource file: " + Path;
        }

        public static Rpf6ResourceFileEntry Create(ref byte[] data, ref int decompressedSize)
        {
            var e = new Rpf6ResourceFileEntry();
            using DataReader reader = new DataReader(new MemoryStream(data), DataEndianess.BigEndian);
            uint rsc6 = reader.ReadUInt32();

            if (rsc6 == 2235781970) //RSC6 header present!
            {
                e.ResourceType = (Rpf6FileExt)reader.ReadInt32();

                FlagInfo flagInfo;
                if (rsc6 == 2235781970U || rsc6 == 2252559186U)
                    flagInfo = new FlagInfo(reader.ReadInt32(), reader.ReadInt32());
                else
                    flagInfo = new FlagInfo(reader.ReadInt32());
                e.FlagInfos = flagInfo;

                byte[] numArray = reader.ReadBytes((int)(reader.Length - reader.Position));
                if (e.ResourceType == Rpf6FileExt.wtd_wtx && e.FlagInfos.IsRSC85)
                {
                    numArray = DecryptAES(numArray);
                }

                using DataReader dReader = new DataReader(new MemoryStream(numArray), DataEndianess.BigEndian);
                data = new byte[flagInfo.BaseResourceSizeP + flagInfo.BaseResourceSizeV];
                decompressedSize = data.Length;

                _ = dReader.ReadUInt32();
                int len = dReader.ReadInt32();
                data = dReader.ReadBytes(len);
            }
            e.Name = "";
            return e;
        }

        public static byte[] AddResourceHeader(byte[] data, uint version, int virtualFlags, int physicalFlags, FlagInfo flag)
        {
            if (data == null) return null;

            bool extendedFlags = flag.IsExtendedFlags;
            byte[] newdata = new byte[data.Length + 16];
            byte[] h1 = BitConverter.GetBytes(extendedFlags ? (uint)2235781970 : (uint)88298322);
            byte[] h2 = BitConverter.GetBytes(version);
            byte[] h3 = BitConverter.GetBytes(virtualFlags);
            byte[] h4 = BitConverter.GetBytes(physicalFlags);

            Buffer.BlockCopy(h1, 0, newdata, 0, 4);
            Buffer.BlockCopy(h2, 0, newdata, 4, 4);
            Buffer.BlockCopy(h3, 0, newdata, 8, 4);

            if (extendedFlags)
            {
                Buffer.BlockCopy(h4, 0, newdata, 12, 4);
            }
            Buffer.BlockCopy(data, 0, newdata, extendedFlags ? 16 : 12, data.Length);
            return newdata;
        }
    }

    public class FlagInfo
    {
        public const uint RSC05Magic = 88298322;
        public const uint RSC06Magic = 105075538;
        public const uint RSC85Magic = 2235781970;
        public const uint RSC86Magic = 2252559186;
        public const int MaxPageSize = 524288;

        public int Flag1 { get; set; }
        public int Flag2 { get; set; }

        public FlagInfo()
        {
        }

        public FlagInfo(int flag) => Flag1 = flag;

        public FlagInfo(int flag1, int flag2)
        {
            Flag1 = flag1;
            Flag2 = flag2;
        }

        #region RSC05
        public int RSC05_VPage4
        {
            get => Flag1 & 1;
            set => Flag1 = Flag1 & -2 | value & 1;
        }

        public int RSC05_VPage3
        {
            get => Flag1 >> 1 & 1;
            set => Flag1 = Flag1 & -3 | (value & 1) << 1;
        }

        public int RSC05_VPage2
        {
            get => Flag1 >> 2 & 1;
            set => Flag1 = Flag1 & -5 | (value & 1) << 2;
        }

        public int RSC05_VPage1
        {
            get => Flag1 >> 3 & 1;
            set => Flag1 = Flag1 & -9 | (value & 1) << 3;
        }

        public int RSC05_VPage0
        {
            get => Flag1 >> 4 & (int)sbyte.MaxValue;
            set => Flag1 = Flag1 & -2033 | (value & (int)sbyte.MaxValue) << 4;
        }

        public int RSC05_VSize
        {
            get => Flag1 >> 11 & 15;
            set => Flag1 = Flag1 & -30721 | (value & 15) << 11;
        }

        public int RSC05_PPage4
        {
            get => Flag1 >> 15 & 1;
            set => Flag1 = Flag1 & -32769 | (value & 1) << 15;
        }

        public int RSC05_PPage3
        {
            get => Flag1 >> 16 & 1;
            set => Flag1 = Flag1 & -65537 | (value & 1) << 16;
        }

        public int RSC05_PPage2
        {
            get => Flag1 >> 17 & 1;
            set => Flag1 = Flag1 & -131073 | (value & 1) << 17;
        }

        public int RSC05_PPage1
        {
            get => Flag1 >> 18 & 1;
            set => Flag1 = Flag1 & -262145 | (value & 1) << 18;
        }

        public int RSC05_PPage0
        {
            get => Flag1 >> 19 & sbyte.MaxValue;
            set => Flag1 = Flag1 & -66584577 | (value & sbyte.MaxValue) << 19;
        }

        public int RSC05_PSize
        {
            get => Flag1 >> 26 & 15;
            set => Flag1 = Flag1 & -1006632961 | (value & 15) << 26;
        }

        public bool RSC05_Compressed
        {
            get => (Flag1 >> 30 & 1) == 1;
            set => Flag1 = Flag1 & -1073741825 | (value ? 1 : 0) << 30;
        }

        public bool RSC05_Resource
        {
            get => (Flag1 >> 31 & 1) == 1;
            set => Flag1 = Flag1 & int.MaxValue | (value ? 1 : 0) << 31;
        }

        public int RSC05_VPageCount => RSC05_VPage4 + RSC05_VPage3 + RSC05_VPage2 + RSC05_VPage1 + RSC05_VPage0;

        public int RSC05_PPageCount => RSC05_PPage4 + RSC05_PPage3 + RSC05_PPage2 + RSC05_PPage1 + RSC05_PPage0;

        public int RSC05_GetTotalVSize => (Flag1 & 2047) << RSC05_VSize + 8;

        public int RSC05_GetTotalPSize => (Flag1 >> 15 & 2047) << RSC05_PSize + 8;

        public int RSC05_GetSizeVPage0 => 4096 << RSC05_VSize;

        public int RSC05_GetSizePPage0 => 4096 << RSC05_PSize;

        public void RSC05_SetMemSizes(int vSize, int pSize) => Flag1 = RSC05_GenerateMemSizes(vSize, pSize);

        public static int RSC05_GenerateMemSizes(int vSize, int pSize)
        {
            int num1 = vSize >> 8;
            int num2 = 0;

            while (num1 > 63)
            {
                if ((uint)(num1 & 1) > 0U)
                    num1 += 2;
                num1 >>= 1;
                ++num2;
            }

            int num3 = pSize >> 8;
            int num4 = 0;

            while (num3 > 63)
            {
                if ((uint)(num3 & 1) > 0U)
                    num3 += 2;
                num3 >>= 1;
                ++num4;
            }
            return num1 | num2 << 11 | num3 << 15 | num4 << 26;
        }
        #endregion

        #region RSC85
        public bool RSC85_bResource
        {
            get => (Flag1 & 2147483648L) == 2147483648L;
            set => Flag1 = SetBit(Flag1, 31, value);
        }

        public int RSC85_VPage0
        {
            get => Flag1 >> 14 & 3;
            set => Flag1 = Flag1 & -3145729 | (value & 3) << 14;
        }

        public int RSC85_VPage1
        {
            get => Flag1 >> 8 & 63;
            set => Flag1 = Flag1 & -16129 | (value & 63) << 8;
        }

        public int RSC85_VPage2
        {
            get => Flag1 & byte.MaxValue;
            set => Flag1 = Flag1 & -256 | value & byte.MaxValue;
        }

        public int RSC85_PPage0
        {
            get => Flag1 >> 28 & 7;
            set => Flag1 = Flag1 & -1879048193 | (value & 7) << 28;
        }

        public int RSC85_PPage1
        {
            get => Flag1 >> 24 & 15;
            set => Flag1 = Flag1 & -251658241 | (value & 15) << 24;
        }

        public int RSC85_PPage2
        {
            get => Flag1 >> 16 & byte.MaxValue;
            set => Flag1 = Flag1 & -16711681 | (value & byte.MaxValue) << 16;
        }

        public bool RSC85_bUseExtendedSize
        {
            get => (Flag2 & 2147483648L) == 2147483648L;
            set => Flag2 = Flag2 & int.MaxValue | (value ? 1 : 0) << 31;
        }

        public int RSC85_ObjectStartPage
        {
            get => Flag2 >> 28 & 7;
            set => Flag2 = Flag2 & -1879048193 | (value & 7) << 28;
        }

        public int RSC85_ObjectStartPageSize
        {
            get => 4096 << RSC85_ObjectStartPage;
            set => RSC85_ObjectStartPage = TrailingZeroes(value) - 12;
        }

        public int RSC85_TotalVSize
        {
            get => (Flag2 & 16383) << 12;
            set => Flag2 = Flag2 & -16384 | value >> 12 & 16383;
        }

        public int RSC85_TotalPSize
        {
            get => (Flag2 >> 14 & 16383) << 12;
            set => Flag2 = Flag2 & -268419073 | (value >> 12 & 16383) << 14;
        }

        public int[] RSC85_PageSizesVirtual
        {
            get
            {
                List<int> intList = new List<int>();
                int num1 = -1;
                int num2 = 524288;
                int num3 = 0;
                int rsC85TotalVsize = RSC85_TotalVSize;
                int c85ObjectStartPage = RSC85_ObjectStartPage;

                int[] numArray = new int[4]
                {
                        RSC85_VPage0,
                        RSC85_VPage1,
                        RSC85_VPage2,
                        int.MaxValue
                };

                for (int index1 = 0; index1 < 4; ++index1)
                {
                    for (int index2 = 0; index2 < numArray[index1] && (uint)rsC85TotalVsize > 0U; ++index2)
                    {
                        while (num2 > rsC85TotalVsize)
                            num2 >>= 1;
                        if (num2 == c85ObjectStartPage && num1 == -1)
                            num1 = num3;
                        num3 += num2;
                        intList.Add(num2);
                        rsC85TotalVsize -= num2;
                    }
                    num2 >>= 1;
                }
                return intList.ToArray();
            }
        }

        public int[] RSC85_PageSizesPhysical
        {
            get
            {
                List<int> intList = new List<int>();
                int num1 = -1;
                int num2 = 524288;
                int num3 = 0;
                int rsC85TotalPsize = RSC85_TotalPSize;
                int c85ObjectStartPage = RSC85_ObjectStartPage;

                int[] numArray = new int[4]
                {
                        RSC85_PPage0,
                        RSC85_PPage1,
                        RSC85_PPage2,
                        int.MaxValue
                };

                for (int index1 = 0; index1 < 4; ++index1)
                {
                    for (int index2 = 0; index2 < numArray[index1] && (uint)rsC85TotalPsize > 0U; ++index2)
                    {
                        while (num2 > rsC85TotalPsize)
                            num2 >>= 1;

                        if (num2 == c85ObjectStartPage && num1 == -1)
                            num1 = num3;

                        num3 += num2;
                        intList.Add(num2);
                        rsC85TotalPsize -= num2;
                    }
                    num2 >>= 1;
                }
                return intList.ToArray();
            }
        }

        public int RSC85_ObjectStart
        {
            get
            {
                int[] pageSizesVirtual = RSC85_PageSizesVirtual;
                int num = 0;
                for (int index = 0; index < pageSizesVirtual.Length; ++index)
                {
                    if (pageSizesVirtual[index] == RSC85_ObjectStartPageSize)
                        return num;
                    num += pageSizesVirtual[index];
                }
                return 0;
            }
        }

        public (int, int, int, int, int, int) RSC85_GenerateMemorySizes(int virt, int phys, int pageBaseSize = 4096)
        {
            int[] numArray = new int[6];
            int num1 = virt;
            int num2 = pageBaseSize << 7;
            int num3 = num2;

            for (int index = 0; index < 3; ++index)
            {
                int num4 = num1 / (num3 >> index);
                num1 -= num4 * (num3 >> index);
                numArray[index] = num4;
            }

            int num5 = num2;
            int num6 = phys;

            for (int index = 0; index < 3; ++index)
            {
                int num4 = num6 / (num5 >> index);
                num6 -= num4 * (num5 >> index);
                numArray[index + 3] = num4;
            }
            return (numArray[0], numArray[1], numArray[2], numArray[3], numArray[4], numArray[5]);
        }

        public void RSC85_SetMemSizes(int totalVirt, int totalPhys, int pageBaseSize = 4096)
        {
            (RSC85_VPage0, RSC85_VPage1, RSC85_VPage2, RSC85_PPage0, RSC85_PPage1, RSC85_PPage2) = RSC85_GenerateMemorySizes(totalVirt, totalPhys, pageBaseSize);
            RSC85_TotalVSize = totalVirt;
            RSC85_TotalPSize = totalPhys;
        }

        public int[] RSC85_GetAvailableObjectStartPage(int pageBaseSize = 4096)
        {
            List<int> intList = new List<int>();
            foreach (int num in RSC85_PageSizesVirtual)
            {
                for (int index = 0; index < 7; ++index)
                {
                    if (pageBaseSize << index == num && !intList.Contains(index))
                        intList.Add(index);
                }
            }
            return intList.ToArray();
        }
        #endregion

        public bool IsResource
        {
            get => RSC05_Resource;
            set => RSC05_Resource = value;
        }

        public bool IsExtendedFlags
        {
            get => RSC85_bUseExtendedSize;
            set => RSC85_bUseExtendedSize = value;
        }

        public bool IsCompressed
        {
            get => !IsExtendedFlags && RSC05_Compressed;
            set
            {
                if (IsExtendedFlags)
                    return;
                RSC05_Compressed = value;
            }
        }

        public bool IsRSC85 => IsExtendedFlags;

        public bool IsRSC05 => !IsExtendedFlags;

        public int DecompressedSize { get; set; }

        public int BaseResourceSizeP => IsExtendedFlags ? RSC85_TotalPSize : RSC05_GetTotalPSize;

        public int BaseResourceSizeV => IsExtendedFlags ? RSC85_TotalVSize : RSC05_GetTotalVSize;

        public int ResourceStart => !IsResource || !IsExtendedFlags ? 0 : RSC85_ObjectStart;

        public int GetTotalSize() => IsResource ? BaseResourceSizeP + BaseResourceSizeV : Flag1 & unchecked((int)3221225471U);

        public void SetTotalSize(int virtOrSize, int phys)
        {
            if (IsResource)
            {
                if (IsExtendedFlags)
                {
                    RSC85_TotalVSize = virtOrSize;
                    RSC85_TotalPSize = phys;
                }
                else RSC85_SetMemSizes(virtOrSize, phys);
            }
            else Flag1 = (int)(Flag1 & 1073741824L | virtOrSize & 3221225471L);
        }

        public static int GetCompactSize(int size)
        {
            int s = size >> 8;
            int i = 0;

            while ((s % 2 == 0) && (s >= 32) && (i < 15))
            {
                i++;
                s >>= 1;
            }
            return ((i & 0xF) << 11) | (s & 0x7FF);
        }

        public static int GetFlags(int sysSegSize, int gpuSegSize)
        {
            return (GetCompactSize(sysSegSize) & 0x7FFF) | (GetCompactSize(gpuSegSize) & 0x7FFF) << 15 | 3 << 30;
        }
    }

    public class ResourceInfo
    {
        public static byte[] GetDataFromResourceBytes(byte[] data) => GetDataFromStream(new MemoryStream(data));

        public static byte[] GetDataFromStream(Stream resourceStream)
        {
            using var br = new DataReader(resourceStream);
            uint num1 = br.ReadUInt32();
            int num2 = br.ReadInt32();
            FlagInfo flagInfo;

            if (num1 == 2235781970U || num1 == 2252559186U)
            {
                flagInfo = new FlagInfo(br.ReadInt32(), br.ReadInt32());
            }
            else
            {
                if (num1 != 88298322U && num1 != 105075538U)
                {
                    return null;
                }
                flagInfo = new FlagInfo(br.ReadInt32());
            }

            byte[] numArray = br.ReadBytes((int)(br.Length - br.Position));
            if (num2 == 2 && flagInfo.IsRSC85)
            {
                numArray = DecryptAES(numArray);
            }

            using var dr = new DataReader(new MemoryStream(numArray));
            int length = (int)dr.Length;
            byte[] data = dr.ReadBytes(length);
            try
            {
                return DecompressZStandard(data);
            }
            catch //Some resources still use zlib (.wtx)
            {
                try
                {
                    return DecompressDeflate(data, flagInfo.BaseResourceSizeP + flagInfo.BaseResourceSizeV, false);
                }
                catch { return null; }
            }
        }
    }

    public enum Rpf6FileExt : byte
    {
        binary = 0, //basically all the other files
        generic = 1, //wst, wfd, wcs & wprp
        wsc = 2,
        was = 6,
        wtd_wtx = 10,
        wedt = 11,
        wsg_wgd = 18,
        wcg = 26,
        wat = 30,
        wbd_wcdt = 31,
        wsf_wnm = 33,
        wtb = 36,
        wpdt = 39,
        wpfl = 50,
        wsp = 116,
        wvd = 133,
        wsi = 134,
        wft = 138
    }
}