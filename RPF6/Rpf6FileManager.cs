using CodeX.Core.Engine;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RSC6;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using TC = System.ComponentModel.TypeConverterAttribute;

namespace CodeX.Games.RDR1.RPF6
{
    public class Rpf6FileManager(RDR1Game game) : FileManager(game)
    {
        public override string ArchiveTypeName => "RPF6";
        public override string ArchiveExtension => ".rpf";
        public Rpf6DataFileMgr DataFileMgr { get; set; }

        public override void InitFileTypes()
        {
            InitGenericFileTypes();
            InitFileType(".rpf", "Rage Package File", FileTypeIcon.Archive);
            InitFileType(".bk2", "Bink Video 2", FileTypeIcon.Movie);
            InitFileType(".dat", "Data File", FileTypeIcon.AudioPlayback, FileTypeAction.ViewXml, true, false, true);
            InitFileType(".nvn", "Compiled Shaders", FileTypeIcon.SystemFile, FileTypeAction.ViewHex);
            InitFileType(".wnm", "Nav Mesh", FileTypeIcon.SystemFile, FileTypeAction.ViewModels, false, false, true);
            InitFileType(".wfd", "Frag Drawable", FileTypeIcon.Piece, FileTypeAction.ViewModels, true, true, true);
            InitFileType(".wft", "Fragment", FileTypeIcon.Piece, FileTypeAction.ViewModels, true, true, true);
            InitFileType(".wvd", "Visual Dictionary", FileTypeIcon.Piece, FileTypeAction.ViewModels, true, true, true);
            InitFileType(".wbd", "Bounds Dictionary", FileTypeIcon.Collisions, FileTypeAction.ViewModels, false, false, true);
            InitFileType(".was", "Animation Set", FileTypeIcon.Animation, FileTypeAction.ViewXml, false, false, true);
            InitFileType(".wat", "Action Tree", FileTypeIcon.SystemFile, FileTypeAction.ViewXml, false, false, true);
            InitFileType(".wsc", "Script", FileTypeIcon.Script);
            InitFileType(".sco", "Unused Script", FileTypeIcon.Script);
            InitFileType(".wsg", "Sector Grass", FileTypeIcon.SystemFile, FileTypeAction.ViewXml, false, false, true);
            InitFileType(".wsi", "Sector Info", FileTypeIcon.Process, FileTypeAction.ViewXml, true, false, true);
            InitFileType(".wcs", "Cover Set", FileTypeIcon.SystemFile);
            InitFileType(".wcg", "Cover Grid", FileTypeIcon.SystemFile, FileTypeAction.ViewXml, false, false, true);
            InitFileType(".wgd", "Gringo Dictionary", FileTypeIcon.SystemFile, FileTypeAction.ViewXml, false, false, true);
            InitFileType(".wsf", "Flash UI", FileTypeIcon.Image, FileTypeAction.ViewTextures, false, false, true);
            InitFileType(".wsp", "Speed Tree", FileTypeIcon.SystemFile, FileTypeAction.ViewHex, false, false, true);
            InitFileType(".sst", "String Table", FileTypeIcon.TextFile, FileTypeAction.ViewXml, true, false, true);
            InitFileType(".wst", "String Table", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".wtb", "Terrain Bounds", FileTypeIcon.Collisions, FileTypeAction.ViewModels, false, false, true);
            InitFileType(".wtd", "Texture Dictionary", FileTypeIcon.Image, FileTypeAction.ViewTextures, true, true, true);
            InitFileType(".wtl", "Terrain World", FileTypeIcon.SystemFile, FileTypeAction.ViewXml, false, false, true);
            InitFileType(".wtx", "Texture Map", FileTypeIcon.Image);
            InitFileType(".wpfl", "Particle Effects Library", FileTypeIcon.Image, FileTypeAction.ViewTextures, false, true, true);
            InitFileType(".wprp", "Prop", FileTypeIcon.SystemFile);
            InitFileType(".wadt", "Animation Dictionary", FileTypeIcon.Animation);
            InitFileType(".wcdt", "Clip Dictionary", FileTypeIcon.Animation, FileTypeAction.ViewXml, false, false, true);
            InitFileType(".wedt", "Expressions Dictionary", FileTypeIcon.TextFile, FileTypeAction.ViewXml, false, false, true);
            InitFileType(".wpdt", "Parametized Motion Dictionary", FileTypeIcon.TextFile);
            InitFileType(".awc", "Audio Wave Container", FileTypeIcon.Audio, FileTypeAction.ViewAudio);
            InitFileType(".cutbin", "Cutscene Binary", FileTypeIcon.XmlFile, FileTypeAction.ViewXml, true);
            InitFileType(".strtbl", "String Table", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".fonttex", "Font Data", FileTypeIcon.Canvas, FileTypeAction.ViewXml, true);
            InitFileType(".tr", "AI Programs", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".csv", "Comma-Separated Values File", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".cfg", "Config File", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".refgroup", "Reference Group", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".fxlist", "Effects List", FileTypeIcon.TextFile, FileTypeAction.ViewText); //ptxFxList
            InitFileType(".modellist", "Models List", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".emitlist", "Emissions List", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".fullfxlist", "Effects List", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".shaderlist", "Shaders List", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".texlist", "Textures List", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".ptxlist", "Effects List", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".xlist", "Streaming List", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".list", "Data List", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".raw", "Raw Texture Data", FileTypeIcon.TextFile);
            InitFileType(".expl", "Explosion Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".vehsim", "Vehicle Simulation", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".vehstuck", "Vehicle Stuck", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".env", "Environment Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".hud", "HUD Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".envclothmanager", "Clothes Update Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".charclothmanager", "Clothes Update Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".traffic", "Actors Traffic Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".mtl", "Material Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".weap", "Weapon Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".fx", "Visual Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".fxm", "Material Visual Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".todlight", "Timecycle Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".ppp", "Post-Processing Pipeline", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".rmptx", "Particle Effects Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".tune", "Game Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".textune", "Texture Data", FileTypeIcon.TextFile, FileTypeAction.ViewText);
        }

        public override void InitCreateInfos()
        {
            InitCreateInfo(".wtd", "WTD", CreateNewWtd);
            InitCreateInfo(".wsi", "WSI", CreateNewWsi);
        }

        public override bool Init()
        {
            JenkIndex.LoadStringsFile("RDR1");
            LoadStartupCache();

            if (Rpf6Crypto.Init(Folder))
            {
                return true;
            }
            return false;
        }

        public override void InitArchives(string[] files)
        {
            foreach (var path in files)
            {
                var relpath = path.Replace(Folder + "\\", "");
                var filepathl = path.ToLowerInvariant();
                var isFile = File.Exists(path); //Could be a folder
                Core.Engine.Console.Write("Rpf6FileManager", Game.GamePathPrefix + relpath + "...");

                if (isFile)
                {
                    if (IsArchive(filepathl))
                    {
                        var archive = GetArchive(path, relpath);
                        if (archive?.AllEntries == null)
                        {
                            continue;
                        }
                        RootArchives.Add(archive);

                        var queue = new Queue<GameArchive>();
                        queue.Enqueue(archive);

                        while (queue.Count > 0)
                        {
                            var a = queue.Dequeue();
                            if (a.Children != null)
                            {
                                foreach (var ca in a.Children)
                                {
                                    queue.Enqueue(ca);
                                }
                            }
                            AllArchives.Add(a);
                        }
                    }
                }
            }
        }

        public override void InitArchivesComplete()
        {
            foreach (var archive in AllArchives)
            {
                if (archive.AllEntries != null)
                {
                    ArchiveDict[archive.Path] = archive;
                    foreach (var entry in archive.AllEntries.Cast<Rpf6Entry>())
                    {
                        if (string.IsNullOrEmpty(entry.ShortNameLower)) continue;
                        JenkIndex.Ensure(entry.ShortNameLower, "RDR1");

                        if (entry is Rpf6FileEntry fe)
                        {
                            EntryDict[fe.Path] = fe;
                        }
                    }
                }
            }

            InitGameFiles();
            if (StartupCacheDirty)
            {
                SaveStartupCache();
            }

            //DoTests();
        }

        private void InitGameFiles()
        {
            Core.Engine.Console.Write("RDR1.InitGameFiles", "Initialising RDR1...");
            if (DataFileMgr != null && !DataFileMgr.UseStartupCache)
            {
                DataFileMgr = null;
            }

            DataFileMgr ??= new Rpf6DataFileMgr(this);
            DataFileMgr.Init();
            Core.Engine.Console.Write("RDR1.InitGameFiles", "RDR1 Initialised.");
        }

        public override void SaveStartupCache()
        {
            var file = StartupUtil.GetFilePath("CodeX.Games.RDR1.startup.dat");
            var strfile = StartupUtil.GetFilePath("CodeX.Games.RDR1.strings.txt");
            var strtime = 0L;

            if (File.Exists(strfile))
            {
                strtime = File.GetLastWriteTime(strfile).ToBinary();
            }

            Core.Engine.Console.Write("Rpf6FileManager", "Building RDR1 startup cache");

            using var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(strtime);

            DataFileMgr.WriteStartupCache(bw);

            var buf = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buf, 0, buf.Length);

            File.WriteAllBytes(file, buf);
        }

        public override void LoadStartupCache()
        {
            var file = StartupUtil.GetFilePath("CodeX.Games.RDR1.startup.dat");
            if (File.Exists(file) == false)
            {
                StartupCacheDirty = true;
                return;
            }

            var strfile = StartupUtil.GetFilePath("CodeX.Games.RDR1.strings.txt");
            var strtime = 0L;
            if (File.Exists(strfile))
            {
                strtime = File.GetLastWriteTime(strfile).ToBinary();
            }

            Core.Engine.Console.Write("Rpf6FileManager", "Loading RDR1 startup cache...");

            var cmpbuf = File.ReadAllBytes(file);
            using var ms = new MemoryStream(cmpbuf);
            var br = new BinaryReader(ms);
            var strtimet = br.ReadInt64();

            if (strtimet != strtime)
            {
                StartupCacheDirty = true; //strings file mismatch, rebuild the startup cache.
                return;
            }

            DataFileMgr = new Rpf6DataFileMgr(this);
            DataFileMgr.ReadStartupCache(br);
        }

        public override bool IsArchive(string filename)
        {
            return filename.EndsWith(ArchiveExtension);
        }

        public override GameArchive GetArchive(string path, string relpath)
        {
            if ((StartupCache != null) && StartupCache.TryGetValue(path, out GameArchive archive))
            {
                return archive;
            }
            var rpf = new Rpf6File(path, relpath);
            rpf.ReadStructure();
            return rpf;
        }

        public override GameArchive CreateArchive(string gamefolder, string relpath)
        {
            return Rpf6File.CreateNew(gamefolder, relpath);
        }

        public override GameArchive CreateArchive(GameArchiveDirectory dir, string name)
        {
            throw new Exception("An archive cannot contain another archive"); //Nope, not in RDR1
        }

        public override GameArchiveFileInfo CreateFile(GameArchiveDirectory dir, string name, byte[] data, bool overwrite = true)
        {
            return Rpf6File.CreateFile(dir as Rpf6DirectoryEntry, name, data, overwrite);
        }

        public override GameArchiveDirectory CreateDirectory(GameArchiveDirectory dir, string name)
        {
            return Rpf6File.CreateDirectory(dir as Rpf6DirectoryEntry, name);
        }

        public override GameArchiveFileInfo CreateFileEntry(string name, string path, ref byte[] data)
        {
            GameArchiveFileInfo e;
            uint rsc6 = (data?.Length > 4) ? BitConverter.ToUInt32(data, 0) : 0;
            if (rsc6 == 1381188485) //RSC6 header present! Create RpfResourceFileEntry and decompress data...
            {
                int length = 0;
                e = Rpf6ResourceFileEntry.Create(ref data, ref length);
                data = Rpf6Crypto.DecompressZStandard(data);
            }
            else
            {
                var info = new FlagInfo
                {
                    Flag1 = 0,
                    Flag2 = 0,
                    IsCompressed = false,
                    IsResource = false
                };
                info.SetTotalSize(data.Length, 0);

                var be = new Rpf6ResourceFileEntry
                {
                    FlagInfos = info,
                    IsEncrypted = false,
                    Size = (uint)data.Length
                };
                e = be;
            }
            e.Name = name;
            e.Path = path;
            return e;
        }

        public override void RenameArchive(GameArchive file, string newname)
        {
            Rpf6File.RenameArchive(file as Rpf6File, newname);
        }

        public override void RenameEntry(GameArchiveEntry entry, string newname)
        {
            Rpf6File.RenameEntry(entry as Rpf6Entry, newname);
        }

        public override void DeleteEntry(GameArchiveEntry entry)
        {
            Rpf6File.DeleteEntry(entry as Rpf6Entry);
        }

        public override void Defragment(GameArchive file, Action<string, float> progress = null, bool recursive = true)
        {
            return;
        }

        public override string ConvertToXml(GameArchiveFileInfo file, byte[] data, out string newfilename, out object infoObject, string folder = "")
        {
            infoObject = null;
            var fileext = Path.GetExtension(file.Name).ToLowerInvariant();

            switch (fileext)
            {
                case ".xml":
                case ".meta":
                    newfilename = file.Name;
                    return TextUtil.GetUTF8Text(data);
                case ".fonttex": return ConvertToXml<FonttexFile>(file, data, out newfilename, "RDR1Fonttex", GetXmlFileFolder(file, folder));
                case ".wft": return ConvertToXml<WftFile>(file, data, out newfilename, "RDR1Fragment", GetXmlFileFolder(file, folder));
                case ".wfd": return ConvertToXml<WfdFile>(file, data, out newfilename, "RDR1FragDrawable", GetXmlFileFolder(file, folder));
                case ".wvd": return ConvertToXml<WvdFile>(file, data, out newfilename, "RDR1VisualDictionary", GetXmlFileFolder(file, folder));
                case ".wsi": return ConvertToXml<WsiFile>(file, data, out newfilename, "RDR1SectorInfo");
                case ".wtl": return ConvertToXml<WtlFile>(file, data, out newfilename, "RDR1TerrainWorld", GetXmlFileFolder(file, folder));
                case ".wtd": return ConvertToXml<WtdFile>(file, data, out newfilename, "RDR1TextureDictionary", GetXmlFileFolder(file, folder));
                case ".dat": return ConvertToXml<AudioDatFile>(file, data, out newfilename, "RDR1AudioDat");
            }

            newfilename = file.Name + ".xml";
            var pack = LoadDataBagPack(file, data);

            if (pack != null)
            {
                infoObject = pack;
                return pack.Bag.ToXml();
            }
            return "Sorry, CodeX currently cannot convert this file to XML.";
        }

        public override byte[] ConvertFromXml(string xml, string filename, string folder = "")
        {
            if (filename.EndsWith(".fonttex.xml")) return ConvertFromXml<FonttexFile>(xml, folder);
            else if (filename.EndsWith(".wvd.xml")) return ConvertFromXml<WvdFile>(xml, folder);
            else if (filename.EndsWith(".wfd.xml")) return ConvertFromXml<WfdFile>(xml, folder);
            else if (filename.EndsWith(".wtd.xml")) return ConvertFromXml<WtdFile>(xml, folder);
            else if (filename.EndsWith(".wsi.xml")) return ConvertFromXml<WsiFile>(xml);
            return null;
        }

        public override string GetXmlFormatName(string filename, out int trimlength)
        {
            trimlength = 4;
            return "RSC XML";
        }

        public override string ConvertToText(GameArchiveFileInfo file, byte[] data, out string newfilename)
        {
            var ext = Path.GetExtension(file.Name).ToLowerInvariant();
            newfilename = Path.GetFileNameWithoutExtension(file.Name) + ".strtbl";

            switch (ext)
            {
                case ".sst":
                case ".wst":
                    var sst = new SstFile(file);
                    sst.Load(data);
                    return sst.ToReadableText();
                case ".strtbl":
                    var strtbl = new StrtblFile();
                    strtbl.Load(data);
                    return strtbl.ToReadableText();
                default:
                    newfilename = file.Name + ".txt";
                    return TextUtil.GetUTF8Text(data);
            }
        }

        public override byte[] ConvertFromText(string text, string filename)
        {
            var ext = Path.GetExtension(filename).ToLowerInvariant();
            switch (ext)
            {
                case ".sst":
                case ".wst":
                    var sst = new SstFile();
                    sst.FromReadableText(text);
                    return sst.Save();
                case ".strtbl":
                    var strtbl = new StrtblFile();
                    strtbl.FromReadableText(text);
                    return strtbl.Save();
                default:
                    return Encoding.UTF8.GetBytes(text);
            }
        }

        public override T LoadMetaNode<T>(GameArchiveFileInfo file, byte[] data = null)
        {
            throw new NotImplementedException();
        }

        public override DataBagPack LoadDataBagPack(GameArchiveFileInfo file, byte[] data = null)
        {
            data = EnsureFileData(file, data);
            if (data == null) return null;

            if (file is Rpf6ResourceFileEntry entry)
            {
                var id = BufferUtil.ReadUint(data, 0);
                switch (id)
                {
                    default:
                        switch (Path.GetExtension(file.Name).ToLowerInvariant())
                        {
                            case ".xml":
                            case ".meta":
                                return DataBagPack.FromXml(file, TextUtil.GetUTF8Text(data));
                        }
                        break; //unknown?
                    case 0x30464252: //RBF0
                        var rbf = new RbfFile();
                        rbf.Load(data);
                        return rbf;
                }
            }
            return null;
        }

        public override TexturePack LoadTexturePack(GameArchiveFileInfo file, byte[] data = null)
        {
            data = EnsureFileData(file, data);
            if (data == null) return null;
            if (file is not Rpf6FileEntry entry) return null;

            if (file.NameLower.EndsWith(".wtd")) //Texture dictionary
            {
                var wtd = new WtdFile(entry);
                wtd.Load(data);
                return wtd;
            }
            else if (file.NameLower.EndsWith(".wsf")) //Flash
            {
                var wsf = new WsfFile(entry);
                wsf.Load(data);
                return wsf;
            }
            else if (file.NameLower.EndsWith(".wpfl")) //Particle Effects Library
            {
                var wpfl = new WpflFile(entry);
                wpfl.Load(data);
                return wpfl;
            }
            return null;
        }

        public override PiecePack LoadPiecePack(GameArchiveFileInfo file, byte[] data = null, bool loadDependencies = false)
        {
            var fp = LoadFilePack(file as Rpf6FileEntry, data, loadDependencies);
            return fp as PiecePack;
        }

        public override AudioPack LoadAudioPack(GameArchiveFileInfo file, byte[] data = null)
        {
            throw new NotImplementedException();
        }

        public override object AnalyzeFile(GameArchiveFileInfo file, byte[] data = null)
        {
            data = EnsureFileData(file, data);
            if (data == null) return new Tuple<string>("Couldn't load file!");

            var ext = Path.GetExtension(file.NameLower);
            if (file is Rpf6ResourceFileEntry rfe)
            {
                switch (ext)
                {
                    case ".wvd": return Rsc6DataReader.Analyze<Rsc6VisualDictionary>(rfe, data);
                    case ".wft": return Rsc6DataReader.Analyze<Rsc6Fragment>(rfe, data);
                    case ".wfd": return Rsc6DataReader.Analyze<Rsc6FragDrawable>(rfe, data);
                    case ".wtd": return Rsc6DataReader.Analyze<Rsc6TextureDictionary>(rfe, data);
                    case ".wcdt": return Rsc6DataReader.Analyze<Rsc6ClipDictionary>(rfe, data);
                    case ".was": return Rsc6DataReader.Analyze<Rsc6AnimationSet>(rfe, data);
                    case ".wsg": return Rsc6DataReader.Analyze<Rsc6SectorGrass>(rfe, data);
                    case ".wsp": return Rsc6DataReader.Analyze<Rsc6TreeForestGrid>(rfe, data);
                    case ".wcg": return Rsc6DataReader.Analyze<Rsc6CombatCoverGrid>(rfe, data);
                    case ".wsi": return Rsc6DataReader.Analyze<Rsc6SectorInfo>(rfe, data);
                    case ".wtb": return Rsc6DataReader.Analyze<Rsc6TerrainBound>(rfe, data);
                    case ".wtl": return Rsc6DataReader.Analyze<Rsc6TerrainWorldResource>(rfe, data);
                    case ".wbd": return Rsc6DataReader.Analyze<Rsc6BoundsDictionary>(rfe, data);
                    case ".wedt": return Rsc6DataReader.Analyze<Rsc6ExpressionDictionary>(rfe, data);
                    case ".wgd": return Rsc6DataReader.Analyze<Rsc6GringoDictionary>(rfe, data);
                    case ".wat": return Rsc6DataReader.Analyze<Rsc6ActionTree>(rfe, data);
                    case ".wpfl": return Rsc6DataReader.Analyze<Rsc6ParticleEffects>(rfe, data);
                    case ".wnm": return Rsc6DataReader.Analyze<Rsc6Navmesh>(rfe, data);
                    case ".wsf": return Rsc6DataReader.Analyze<Rsc6ScaleFormFile>(rfe, data);
                    case ".wst":
                    case ".sst":
                        return Rsc6DataReader.Analyze<Rsc6StringTable>(rfe, data);
                }
            }
            return new Tuple<string>("Unable to analyze file.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FilePack LoadFilePack(Rpf6FileEntry entry, byte[] data, bool loadDependencies = true)
        {
            if (entry == null) return null;
            data = EnsureFileData(entry, data);
            if (data == null) return null;

            var enl = entry.NameLower;
            if (entry.Name.EndsWith(".fonttex")) //Fonttex
            {
                var sst = new FonttexFile();
                sst.Load(data);
                return sst;
            }
            else if (enl.EndsWith(".wft")) //Fragments
            {
                var wft = new WftFile(entry);
                wft.Load(data);
                if (loadDependencies) LoadDependencies(wft);
                return wft;
            }
            else if (enl.EndsWith(".wvd")) //Visual dictionary
            {
                var wvd = new WvdFile(entry);
                wvd.Load(data);
                if (loadDependencies) LoadDependencies(wvd);
                return wvd;
            }
            else if (enl.EndsWith(".wsi")) //Sector info
            {
                var wsi = new WsiFile(entry);
                wsi.Load(data);
                return wsi;
            }
            else if (enl.EndsWith(".wsg")) //Grass
            {
                var wsg = new WsgFile(entry);
                wsg.Load(data);
                return wsg;
            }
            else if (enl.EndsWith(".wsp")) //Trees
            {
                var wsp = new WspFile(entry);
                wsp.Load(data);
                return wsp;
            }
            else if (enl.EndsWith(".wcg")) //Cover grid
            {
                var wcg = new WcgFile(entry);
                wcg.Load(data);
                return wcg;
            }
            else if (enl.EndsWith(".wtb")) //Territory bounds
            {
                var wtb = new WtbFile(entry);
                wtb.Load(data);
                return wtb;
            }
            else if (enl.EndsWith(".wedt")) //Expressions
            {
                var wedt = new WedtFile(entry);
                wedt.Load(data);
                return wedt;
            }
            else if (enl.EndsWith(".was")) //Animations set
            {
                var was = new WasFile(entry);
                was.Load(data);
                return was;
            }
            else if (enl.EndsWith(".wtd")) //Texture dictionary
            {
                var wtd = new WtdFile(entry);
                wtd.Load(data);
                return wtd;
            }
            else if (enl.EndsWith(".wfd")) //Frag drawables
            {
                var wfd = new WfdFile(entry);
                wfd.Load(data);
                if (loadDependencies) LoadDependencies(wfd);
                return wfd;
            }
            else if (entry.Name.EndsWith(".sst") || entry.Name.EndsWith(".wst")) //Stringtable
            {
                var sst = new SstFile(entry);
                sst.Load(data);
                return sst;
            }
            else if (entry.Name.EndsWith(".wbd")) //Bounds dictionary
            {
                var wbd = new WbdFile(entry);
                wbd.Load(data);
                return wbd;
            }
            else if (entry.Name.EndsWith(".wcdt")) //Clips dictionary
            {
                var wcdt = new WcdtFile(entry);
                wcdt.Load(data);
                return wcdt;
            }
            else if (entry.Name.EndsWith(".wnm")) //Nav mesh
            {
                var wnm = new WnmFile(entry);
                wnm.Load(data);
                return wnm;
            }
            else if (entry.Name.EndsWith(".wtl")) //Terrain lods
            {
                var wtl = new WtlFile();
                wtl.Load(data);
                return wtl;
            }
            return null;
        }

        public static byte[] CreateNewWtd(string name)
        {
            var wtd = new WtdFile()
            {
                TextureDictionary = new Rsc6TextureDictionary()
            };
            var data = wtd.Save();
            return data;
        }

        public static byte[] CreateNewWsi(string name)
        {
            var wsi = new WsiFile()
            {
                SectorInfo = new Rsc6SectorInfo()
            };
            var data = wsi.Save();
            return data;
        }

        public void LoadDependencies(PiecePack pack)
        {
            if (pack?.Pieces == null) return;

            var fileent = pack.FileInfo as Rpf6FileEntry;
            var isVisualDict = fileent?.Name.EndsWith(".wvd") ?? false;
            var textures = isVisualDict ? LoadVisualDictTextures(fileent, pack) : LoadFragmentTextures(fileent);
            if (textures == null || textures.Length == 0) return;

            var dict = new Dictionary<JenkHash, Rsc6Texture>();
            foreach (var tex in textures)
            {
                if (tex?.Name == null) continue;
                var name = tex.Name.ToLowerInvariant().Replace(".dds", "");

                if (name.StartsWith("memory:"))
                {
                    name = name[(name.LastIndexOf(':') + 1)..];
                }

                var hash = JenkHash.GenHash(name);
                dict[hash] = tex;
            }

            foreach (var piece in pack.Pieces.Values)
            {
                if (piece == null) continue;
                if (piece is Rsc6Drawable drawable)
                {
                    drawable.ApplyTextures(dict);
                }
            }
        }

        public override GameArchiveFileInfo[] GetDependencyList(PiecePack pack)
        {
            if (pack?.Pieces == null || DataFileMgr == null) return null;
            var hs = new HashSet<GameArchiveFileInfo>();

            foreach (var kvp in pack.Pieces)
            {
                if (kvp.Value == null) continue;
                foreach (var model in kvp.Value.AllModels)
                {
                    if (model == null) continue;
                    foreach (var mesh in model.Meshes)
                    {
                        if (mesh == null) continue;
                        foreach (var tex in mesh.Textures)
                        {
                            if (tex == null) continue;
                            hs.Add(tex.Pack.FileInfo);
                        }
                    }
                }
            }
            return hs.Count > 0 ? [.. hs] : null;
        }

        private Rsc6Texture[] LoadVisualDictTextures(Rpf6FileEntry entry, PiecePack pack = null)
        {
            //TODO: get rid of this and load dependencies properly

            var FileManager = Game.GetFileManager() as Rpf6FileManager;
            var dfm = FileManager.DataFileMgr;
            var wvdParent = entry.Parent?.Parent;
            if (wvdParent == null) return null;

            var textures = new List<Rsc6Texture>();
            if (entry.Name.StartsWith("tile")) //We're searching for a tile, their textures can be in various dictionaries
            {
                foreach (var drawable in ((WvdFile)pack).VisualDictionary.Drawables.Items)
                {
                    foreach (var model in drawable.AllModels)
                    {
                        foreach (var mesh in model.Meshes)
                        {
                            foreach (var texture in mesh.Textures)
                            {
                                if (texture == null) continue;
                                if (texture.Height != 0 || string.IsNullOrEmpty(texture.Name)) continue;
                                if (textures.Find(item => item.Name == texture.Name) != null) continue;

                                string desiredWtd = texture.Name.Remove(texture.Name.LastIndexOf('.')).ToLower();
                                var wtdFiles = dfm.StreamEntries[Rpf6FileExt.wtd_wtx];

                                foreach (var wtd in wtdFiles)
                                {
                                    if (wtd.Value.NameLower.Contains("_med") || wtd.Value.NameLower.Contains("_low")) continue;
                                    if (!wtd.Value.NameLower.StartsWith(desiredWtd)) continue;

                                    var r = LoadTexturePack(wtd.Value);
                                    if (r == null) continue;

                                    foreach (var tex in r.Textures.Values)
                                    {
                                        if (tex == null) continue;
                                        textures.Add((Rsc6Texture)tex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (wvdParent != null && !wvdParent.Name.StartsWith("resource_") && wvdParent.Name != "territory_swall_noid") //Visual dictionaries (not including tiles)
            {
                //Searching textures from the #vd parent sector
                foreach (var s in wvdParent.Directories)
                {
                    if (s.NameLower.StartsWith("0x") || s.NameLower.StartsWith("mp_")) continue;
                    if (s.NameLower != wvdParent.NameLower) continue;

                    foreach (var f in s.Files)
                    {
                        if (f.NameLower.Contains("vlow") || f.NameLower.Contains("med") || f.NameLower.EndsWith(".wsi") || f.NameLower.Contains("dlc")) continue;
                        var r = LoadPiecePack(f);
                        var visualDict = ((WvdFile)r).VisualDictionary;

                        if (visualDict == null) continue;
                        for (int i = 0; i < visualDict.TextureDictionary.Item.Textures.Count; i++)
                        {
                            var tex = visualDict.TextureDictionary.Item.Textures[i];
                            if (tex == null) continue;
                            if (textures.Find(item => item.Name == tex.Name) == null)
                            {
                                textures.Add(visualDict.TextureDictionary.Item.Textures[i]);
                            }
                        }
                    }
                }

                //Get textures from smicMapping if the current model is referenced from it
                foreach (var kv in dfm.SmicMapping)
                {
                    if (!entry.Name.Contains(kv.Key)) continue;
                    foreach (var smic in kv.Value)
                    {
                        if (dfm.StreamEntries[Rpf6FileExt.wtd_wtx].TryGetValue(JenkHash.GenHash(smic.ToLower()), out Rpf6FileEntry smicFile))
                        {
                            if (smicFile == null || smicFile.Name.Contains("blur")) continue;
                            var r = LoadTexturePack(smicFile);
                            if (r == null) continue;

                            foreach (var tex in r.Textures.Values)
                            {
                                if (tex == null) continue;
                                if (textures.Find(item => item.Name.Contains(tex.Name, StringComparison.InvariantCultureIgnoreCase)) == null)
                                {
                                    textures.Add((Rsc6Texture)tex);
                                }
                            }
                        }
                    }
                }
            }

            //Add also the textures from swAll.wtd
            textures.AddRange([.. dfm.SwAll]);

            return [.. textures];
        }

        private Rsc6Texture[] LoadFragmentTextures(Rpf6FileEntry entry)
        {
            //TODO: get rid of this and load dependencies properly

            var texturesHashSet = new HashSet<Rsc6Texture>();
            var FileManager = Game.GetFileManager() as Rpf6FileManager;
            var dfm = FileManager.DataFileMgr;
            if (dfm == null || dfm.StreamEntries.Count == 0) return null;

            //Get textures from smicMapping if the current entry is referenced from it
            foreach (var kv in dfm.SmicMapping)
            {
                if (!entry.Name.Contains(kv.Key)) continue;
                foreach (var smic in kv.Value)
                {
                    if (dfm.StreamEntries[Rpf6FileExt.wtd_wtx].TryGetValue(JenkHash.GenHash(smic.ToLower()), out Rpf6FileEntry smicFile))
                    {
                        if (smicFile == null || smicFile.Name.Contains("blur")) continue;
                        var r = LoadTexturePack(smicFile);
                        if (r == null) continue;

                        foreach (var tex in r.Textures.Values)
                        {
                            if (tex != null && !texturesHashSet.Contains(tex))
                            {
                                texturesHashSet.Add((Rsc6Texture)tex);
                            }
                        }
                    }
                }
            }

            //Try finding #td textures based of the entry name
            string desiredWtd = entry.Name.Remove(entry.Name.LastIndexOf('.')).ToLower();
            var wtdFiles = dfm.StreamEntries[Rpf6FileExt.wtd_wtx];

            Parallel.ForEach(wtdFiles.Values, wtd =>
            {
                if (!wtd.NameLower.StartsWith(desiredWtd) || wtd.NameLower.Contains("_medlod")) return;
                var r = LoadTexturePack(wtd);
                if (r == null) return;

                lock (texturesHashSet)
                {
                    foreach (var tex in r.Textures.Values)
                    {
                        if (tex != null && !texturesHashSet.Contains(tex))
                        {
                            texturesHashSet.Add((Rsc6Texture)tex);
                        }
                    }
                }
            });

            //Add the textures from fragmentTextureList
            texturesHashSet.UnionWith(dfm.FragTextures);
            return [.. texturesHashSet];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rpf6FileExt GetRpf6FileExt(string filename)
        {
            var extstr = Path.GetExtension(filename).Replace(".", "").ToLowerInvariant();
            if (Enum.TryParse(extstr, out Rpf6FileExt ext))
            {
                return ext;
            }
            return Rpf6FileExt.generic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JenkHash GetRpf6FileHash(string filename)
        {
            return new JenkHash(Path.GetFileNameWithoutExtension(filename).ToLowerInvariant());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetRpf6FileHashExt(string filename, out JenkHash hash, out Rpf6FileExt ext)
        {
            hash = GetRpf6FileHash(filename);
            ext = GetRpf6FileExt(filename);
        }

        public override string GetProjectFileGamePath(string fullpath)
        {
            if (DataFileMgr == null) return base.GetProjectFileGamePath(fullpath);
            GetRpf6FileHashExt(fullpath, out var hash, out var ext);
            var streamentry = DataFileMgr.TryGetStreamEntry(hash, ext);
            if (streamentry != null)
            {
                return streamentry.PathLower;
            }
            return base.GetProjectFileGamePath(fullpath);
        }

        public override void UpdateProjectItem(ProjectItem item, ProjectItemAction action)
        {
            base.UpdateProjectItem(item, action);
            var dfm = DataFileMgr;
            if (dfm == null) return;

            var gamepath = item.GamePath;
            if (string.IsNullOrEmpty(gamepath)) return;
            GetRpf6FileHashExt(gamepath, out var hash, out var ext);

            if (action == ProjectItemAction.LoadFile)
            {
                JenkIndex.Ensure(Path.GetFileNameWithoutExtension(gamepath).ToLowerInvariant(), "RDR1"); //Make sure project file names have hashes in the index

                if (item.FileInfo is not Rpf6FileEntry entry) return;
                item.OrigFile ??= dfm.TryGetStreamEntry(hash, ext);
                dfm.AddStreamEntry(hash, ext, entry);
            }

            if (action == ProjectItemAction.UnloadFile)
            {
                if (item.OrigFile is Rpf6FileEntry orig)
                {
                    dfm.AddStreamEntry(hash, ext, orig);
                }
                else
                {
                    orig = dfm.TryGetStreamEntry(hash, ext);
                    if (orig == item.FileInfo)
                    {
                        dfm.RemoveStreamEntry(hash, ext);
                    }
                }
            }
        }

        private void DoTests()
        {
            bool animations = false, shaders = false, terrainBounds = false, sectorInfo = false, grass = false, strtable = false, fragments = false, expressions = false, scripts = false;
            var listShaders = new List<Rsc6ShaderFX>();
            var listIds = new List<string>();
            var objects = new List<string>();

            if (!animations && !shaders && !terrainBounds && !sectorInfo && !grass && !strtable && !fragments && !expressions && !scripts) return;
            foreach (var archive in AllArchives)
            {
                var apl = archive.Path.ToLowerInvariant();
                if (archive.AllEntries == null) continue;

                foreach (var entry in archive.AllEntries)
                {
                    if (entry is Rpf6FileEntry fe)
                    {
                        var n = fe.NameLower;
                        if (animations && n.EndsWith(".wcdt"))
                        {
                            Core.Engine.Console.Write("ClipDictionary", fe.Path);
                            var data = EnsureFileData(fe, null);
                            if (data != null)
                            {
                                var wcdt = new WcdtFile(fe);
                                wcdt.Load(data);
                            }
                        }
                        if (scripts && n.EndsWith(".wsc"))
                        {
                            if (!fe.EntryParent.EntryParent.Name.Contains("rcm") && !fe.EntryParent.EntryParent.Name.Contains("missions")) continue;
                            Core.Engine.Console.Write("Scripts", fe.Path);

                            Debug.WriteLine($"\"{fe.Name.Replace(".wsc", "")}_layout\",");
                        }
                        if (terrainBounds && n.EndsWith(".wtb"))
                        {
                            Core.Engine.Console.Write("TerrainBounds", fe.Path);
                            var data = EnsureFileData(fe, null);
                            if (data != null)
                            {
                                var wtb = new WtbFile(fe);
                                wtb.Load(data);
                            }
                        }
                        if (sectorInfo && n.EndsWith(".wsi"))
                        {
                            Core.Engine.Console.Write("SectorInfo", fe.Path);
                            var data = EnsureFileData(fe, null);
                            if (data != null)
                            {
                                var wsi = new WsiFile(fe);
                                wsi.Load(data);

                                var si = wsi.SectorInfo;
                                if (si != null)
                                {
                                    var bbMin = new Vector3(si.BoundMin.Y, si.BoundMin.Z, si.BoundMin.X);
                                    var bbMax = new Vector3(si.BoundMax.Y, si.BoundMax.Z, si.BoundMax.X);
                                    Debug.WriteLine(string.Format("sectorsAABB.new(vector3({0}, {1}, {2}), vector3({3}, {4}, {5}), \"{6}\"),",
                                        bbMin.X.ToString().Replace(',', '.'),
                                        bbMin.Y.ToString().Replace(',', '.'),
                                        bbMin.Z.ToString().Replace(',', '.'),
                                        bbMax.X.ToString().Replace(',', '.'),
                                        bbMax.Y.ToString().Replace(',', '.'),
                                        bbMax.Z.ToString().Replace(',', '.'),
                                        si.ToString()));
                                }
                            }
                        }
                        if (grass && n.EndsWith(".wsg"))
                        {
                            Core.Engine.Console.Write("GrassRes", fe.Path);
                            var data = EnsureFileData(fe, null);
                            if (data != null)
                            {
                                var wsg = new WsgFile(fe);
                                wsg.Load(data);
                            }
                        }
                        if (strtable && n.EndsWith(".strtbl") && archive.Name == "strings_d11generic.rpf" /*&& fe.Parent.Name == "strings"*/)
                        {
                            Core.Engine.Console.Write("StringTable", fe.Path);
                            var data = EnsureFileData(fe, null);
                            if (data != null)
                            {
                                var strtbl = new StrtblFile();
                                strtbl.Load(data);
                            }
                            //var xml = ConvertToXml(fe, data, out string filepath, out object lol);
                            //File.WriteAllText(@"C:\Users\fumol\OneDrive\Bureau\xml\" + filepath, xml);
                        }
                        else if (fragments && n.EndsWith(".wft"))
                        {
                            Core.Engine.Console.Write("Fragment", fe.Path);
                            var data = EnsureFileData(fe, null);
                            if (data != null)
                            {
                                var wft = new WftFile(fe);
                                wft.Load(data);
                            }
                        }
                        else if (expressions && n.EndsWith(".wedt"))
                        {
                            Core.Engine.Console.Write("ExpressionsDictionary", fe.Path);
                            var data = EnsureFileData(fe, null);
                            if (data != null)
                            {
                                var wedt = new WedtFile(fe);
                                wedt.Load(data);
                            }
                        }
                        else if (shaders)
                        {
                            Core.Engine.Console.Write("VisualDictionary", fe.Path);
                            if (n.EndsWith(".wvd"))
                            {
                                var data = EnsureFileData(fe, null);
                                if (data != null)
                                {
                                    var wvd = new WvdFile(fe);
                                    wvd.Load(data);

                                    if (wvd != null)
                                    {
                                        foreach (var piece in wvd.Pieces.Values.Cast<Rsc6Drawable>())
                                        {
                                            var sGroup = piece?.ShaderGroup.Item;
                                            foreach (var shader in sGroup.Shaders.Items)
                                            {
                                                if (shader.Name.Str == string.Empty) continue;
                                                if (!listShaders.Any(s => s.Name == shader.Name))
                                                {
                                                    listShaders.Add(shader);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (n.EndsWith(".wft"))
                            {
                                var data = EnsureFileData(fe, null);
                                if (data != null)
                                {
                                    try
                                    {
                                        var wft = new WftFile(fe);
                                        wft.Load(data);

                                        if (wft != null)
                                        {
                                            foreach (var piece in wft.Pieces.Values.Cast<Rsc6Drawable>())
                                            {
                                                var sGroup = piece?.ShaderGroup.Item;
                                                foreach (var shader in sGroup.Shaders.Items)
                                                {
                                                    if (shader.Name.Str == string.Empty) continue;
                                                    if (!listShaders.Any(s => s.Name == shader.Name))
                                                    {
                                                        listShaders.Add(shader);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                            else if (n.EndsWith(".wfd"))
                            {
                                var data = EnsureFileData(fe, null);
                                if (data != null)
                                {
                                    var wfd = new WfdFile(fe);
                                    wfd.Load(data);

                                    if (wfd != null)
                                    {
                                        foreach (var piece in wfd.Pieces.Values.Cast<Rsc6Drawable>())
                                        {
                                            var sGroup = piece?.ShaderGroup.Item;
                                            foreach (var shader in sGroup.Shaders.Items)
                                            {
                                                if (shader.Name.Str == string.Empty) continue;
                                                if (!listShaders.Any(s => s.Name == shader.Name))
                                                {
                                                    listShaders.Add(shader);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            XmlElement CreateElement(XmlDocument doc, string name, string value)
            {
                XmlElement element = doc.CreateElement(name);
                element.InnerText = value;
                return element;
            }

            XmlElement CreateTextureItemElement(XmlDocument doc, string name)
            {
                XmlElement element = doc.CreateElement("Item");
                element.SetAttribute("name", name);
                element.SetAttribute("type", "Texture");
                return element;
            }

            XmlElement CreateVectorItemElement(XmlDocument doc, string name, Vector4 vector)
            {
                XmlElement element = doc.CreateElement("Item");
                element.SetAttribute("length", "16");
                element.SetAttribute("name", name);
                element.SetAttribute("type", "CBuffer");
                element.SetAttribute("value_type", "float4");
                element.SetAttribute("w", vector.W.ToString().Replace(",", "."));
                element.SetAttribute("x", vector.X.ToString().Replace(",", "."));
                element.SetAttribute("y", vector.Y.ToString().Replace(",", "."));
                element.SetAttribute("z", vector.Z.ToString().Replace(",", "."));
                return element;
            }

            XmlElement CreateArrayItemElement(XmlDocument doc, string name, int length, Vector4[] array)
            {
                string[] temp = Array.ConvertAll(array, v => $"{v.W.ToString().Replace(",", ".")}, {v.X.ToString().Replace(",", ".")}, {v.Y.ToString().Replace(",", ".")}, {v.Z.ToString().Replace(",", ".")}");
                var values = string.Join(", ", temp);

                XmlElement element = doc.CreateElement("Item");
                element.SetAttribute("length", length.ToString());
                element.SetAttribute("name", name);
                element.SetAttribute("type", "CBuffer");
                element.SetAttribute("value_type", $"float{length / 16}x4");
                element.SetAttribute("value", values);
                return element;
            }

            if (shaders && listShaders.Count > 0)
            {
                var doc = new XmlDocument();
                doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));

                var root = doc.CreateElement("RDR1Shaders");
                doc.AppendChild(root);

                foreach (var shader in listShaders)
                {
                    var item = doc.CreateElement("Item");
                    root.AppendChild(item);

                    item.AppendChild(CreateElement(doc, "Name", shader.Name.Str));
                    item.AppendChild(CreateElement(doc, "DrawBucket", shader.RenderBucket.ToString()));

                    var prams = doc.CreateElement("Params");
                    item.AppendChild(prams);

                    foreach (var parameter in shader.ParametersList.Item?.Parameters)
                    {
                        var type = parameter.DataType;
                        var name = parameter.Hash.ToString();

                        if (type == 0)
                            prams.AppendChild(CreateTextureItemElement(doc, name));
                        else if (type == (Rsc6ShaderParamType)1)
                            prams.AppendChild(CreateVectorItemElement(doc, name, parameter.Vector));
                        else
                            prams.AppendChild(CreateArrayItemElement(doc, name, (int)type * 16, parameter.Array));
                    }
                }
                //doc.Save(@"C:\Users\fumol\AppData\Roaming\Blender Foundation\Blender\4.0\scripts\addons\SollumzRDR-wvd\cwxml\RDR1Shaders.xml");
            }
        }
    }

    public class Rpf6DataFileMgr
    {
        public Rpf6FileManager FileManager;
        public List<Rpf6FileEntry> AllEntries;
        public Dictionary<Rpf6FileExt, Dictionary<JenkHash, Rpf6FileEntry>> StreamEntries;
        public List<Rsc6TextureDictionary> ResidentTxds = new();
        public List<Rsc6Texture> SwAll; //For storing textures from swAll.wtd //TODO: get rid of this and load dependencies properly
        public List<Rsc6Texture> FragTextures; //For storing textures from fragmentTextureList.wtd //TODO: get rid of this and load dependencies properly
        public Dictionary<string, string[]> SmicMapping; //For the texture mapping from smicToFragMap.txt
        public bool UseStartupCache = false;

        public Rpf6DataFileMgr(Rpf6FileManager fman)
        {
            FileManager = fman;
        }

        public void Init()
        {
            if (StreamEntries != null) return;

            AllEntries = [];
            StreamEntries = [];
            SwAll = [];
            FragTextures = [];
            SmicMapping = [];

            LoadFiles();
            if (StreamEntries.Count > 0)
            {
                LoadResidentTextures();
                LoadSmicMap();
            }
        }

        public void WriteStartupCache(BinaryWriter bw)
        {
            Core.Engine.Console.Write("Rpf6DataFileMgr", "Building RDR1 startup cache - Materials");
            var mats = Rsc6BoundsMaterialTypes.GetMaterials(FileManager); //TODO: is this really necessary?
            var materials = new List<Rpf6MaterialStoreItem>();
            for (int i = 0; i < mats.Length; i++)
            {
                var mat = mats[i];
                if (mat == null) continue;

                var item = new Rpf6MaterialStoreItem()
                {
                    Name = mat.MaterialName,
                    Color = mat.Colour
                };
                materials.Add(item);
            }

            bw.Write(materials.Count);
            foreach (var item in materials)
            {
                bw.WriteStringNullTerminated(item.Name);
                bw.Write((int)item.Color);
            }
        }

        public void ReadStartupCache(BinaryReader br)
        {
            var matItems = new List<Rpf6MaterialStoreItem>();
            var matCount = br.ReadInt32();
            for (int i = 0; i < matCount; i++)
            {
                var item = new Rpf6MaterialStoreItem
                {
                    Name = br.ReadStringNullTerminated(),
                    Color = new Colour(br.ReadInt32())
                };
                matItems.Add(item);
            }
            if (matItems.Count > 0)
            {
                var mats = new Rsc6BoundsMaterialData[matItems.Count];
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = new Rsc6BoundsMaterialData()
                    {
                        MaterialName = matItems[i].Name,
                        Colour = matItems[i].Color
                    };
                    mats[i] = m;
                }
                Rsc6BoundsMaterialTypes.Materials = [.. mats];
            }
            UseStartupCache = true;
        }

        private void LoadFiles()
        {
            foreach (var archive in this.FileManager.AllArchives)
            {
                if (archive.Path.StartsWith("backup")) continue;
                foreach (var file in archive.AllEntries)
                {
                    if (file is not Rpf6FileEntry fe) continue;
                    this.AllEntries.Add((Rpf6FileEntry)file);

                    if (fe.FlagInfos.IsResource)
                    {
                        var hash = fe.ShortNameHash;
                        if (fe.ResourceType == Rpf6FileExt.wvd)
                        {
                            if (fe.Parent.Name == "resource_1")
                                continue;
                            else if (fe.Parent.Name == "resource_2")
                                hash = new JenkHash(fe.Name + "_lod2");
                            else if (fe.Parent.Name == "resource_3")
                                hash = new JenkHash(fe.Name + "_lod3");
                        }

                        if (!this.StreamEntries.TryGetValue(fe.ResourceType, out var entries))
                        {
                            entries = [];
                            this.StreamEntries[fe.ResourceType] = entries;
                        }
                        entries[hash] = fe;
                    }
                    else if (file.Name.EndsWith(".txt") || file.Name.EndsWith(".vehsim") || (file.Name.EndsWith(".bin") && file.Parent.Name == "fragments"))
                    {
                        if (!this.StreamEntries.TryGetValue(Rpf6FileExt.binary, out var entries))
                        {
                            entries = [];
                            this.StreamEntries[Rpf6FileExt.binary] = entries;
                        }
                        entries[fe.ShortNameHash] = fe;
                    }
                }
            }
        }

        private void LoadResidentTextures()
        {
            Core.Engine.Console.Write("Rpf6DataFileMgr", "Loading resident textures...");

            //Load textures from swAll.wtd
            StreamEntries[Rpf6FileExt.wtd_wtx].TryGetValue(0x5D960088, out Rpf6FileEntry swall);
            if (swall != null)
            {
                var r = FileManager.LoadTexturePack(swall);
                if (r != null)
                {
                    var swAllTxd = (r as WtdFile)?.TextureDictionary;
                    if (swAllTxd != null)
                    {
                        ResidentTxds.Add(swAllTxd);
                    }
                    foreach (var tex in r.Textures.Values)
                    {
                        if (tex == null) continue;
                        if (SwAll.Find(item => item.Name.Contains(tex.Name, StringComparison.InvariantCultureIgnoreCase)) == null)
                        {
                            SwAll.Add((Rsc6Texture)tex);
                        }
                    }
                }
            }

            //Load textures from fragmentTextureList.wtd
            StreamEntries[Rpf6FileExt.wtd_wtx].TryGetValue(0x4D6D7386, out Rpf6FileEntry texList);
            if (texList != null)
            {
                var r = FileManager.LoadTexturePack(texList);
                if (r != null)
                {
                    var txd = (r as WtdFile)?.TextureDictionary;
                    if (txd != null)
                    {
                        ResidentTxds.Add(txd);
                    }
                    foreach (var tex in r.Textures.Values)
                    {
                        if (tex == null) continue;
                        if (FragTextures.Find(item => item.Name.Contains(tex.Name, StringComparison.InvariantCultureIgnoreCase)) == null)
                        {
                            FragTextures.Add((Rsc6Texture)tex);
                        }
                    }
                }
            }
        }

        private void LoadSmicMap()
        {
            Core.Engine.Console.Write("Rpf6DataFileMgr", "Loading smic map...");
            this.StreamEntries[Rpf6FileExt.binary].TryGetValue(0x57D7F6EF, out Rpf6FileEntry file); //smicToFragMap.txt
            if (file != null)
            {
                byte[] data = null;
                data = this.FileManager.EnsureFileData(file, data);

                if (data != null)
                {
                    string text = this.FileManager.ConvertToText(file, data, out string name);
                    string[] result = text.Split(new[] { '\r', '\n' });

                    foreach (string s in result)
                    {
                        if (string.IsNullOrEmpty(s) || s.Contains(':')) continue;
                        bool dollar = s.StartsWith('$');
                        string child = s[(dollar ? 1 : 0)..s.IndexOf(' ')].ToLowerInvariant();
                        JenkIndex.Ensure(child);
                        int smicIndex = s.IndexOf("smic_");
                        var smicContent = s[smicIndex..].Split(" ");
                        SmicMapping[child] = smicContent;
                    }
                }
            }
        }

        public Rpf6FileEntry TryGetStreamEntry(JenkHash hash, Rpf6FileExt ext)
        {
            if (this.StreamEntries.TryGetValue(ext, out var entries))
            {
                if (entries.TryGetValue(hash, out var entry))
                {
                    return entry;
                }
            }
            return null;
        }

        public void AddStreamEntry(JenkHash hash, Rpf6FileExt ext, Rpf6FileEntry entry)
        {
            if ((int)ext < StreamEntries.Values.Count)
            {
                StreamEntries[ext][hash] = entry;
            }
        }

        public void RemoveStreamEntry(JenkHash hash, Rpf6FileExt ext)
        {
            if ((int)ext < StreamEntries.Values.Count)
            {
                StreamEntries[ext].Remove(hash);
            }
        }
    }

    [TC(typeof(EXP))]
    public struct Rpf6MaterialStoreItem
    {
        public string Name;
        public Colour Color;

        public override readonly string ToString()
        {
            return $"{Name} : {Color}";
        }
    }
}