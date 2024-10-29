using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CodeX.Core.Engine;
using CodeX.Core.Editor;
using CodeX.Core.Numerics;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.RSC6;
using CodeX.Games.RDR1.Files;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using static BepuPhysics.Collidables.CompoundBuilder;

namespace CodeX.Games.RDR1.RPF6
{
    public class Rpf6FileManager : FileManager
    {
        public override string ArchiveTypeName => "RPF6";
        public override string ArchiveExtension => ".rpf";

        public Rpf6DataFileMgr DataFileMgr { get; set; }
        public Rpf6Store Store { get; set; }
        public List<Rsc6Texture> Textures { get; set; } = new List<Rsc6Texture>();

        public Rpf6FileManager(RDR1Game game) : base(game)
        {
            Store = new Rpf6Store(this);
        }

        public override void InitFileTypes()
        {
            InitGenericFileTypes();
            InitGenericRDR1Types();

            InitFileType(".rpf", "Rage Package File", FileTypeIcon.Archive);
            InitFileType(".bk2", "Bink Video 2", FileTypeIcon.Movie);
            InitFileType(".dat", "Data File", FileTypeIcon.SystemFile);
            InitFileType(".nvn", "Compiled Shaders", FileTypeIcon.SystemFile, FileTypeAction.ViewHex);
            InitFileType(".wnm", "Nav Mesh", FileTypeIcon.SystemFile);
            InitFileType(".wfd", "Frag Drawable", FileTypeIcon.Piece, FileTypeAction.ViewModels, true, true, true);
            InitFileType(".wft", "Fragment", FileTypeIcon.Piece, FileTypeAction.ViewModels, true, true, true);
            InitFileType(".wvd", "Visual Dictionary", FileTypeIcon.Piece, FileTypeAction.ViewModels, true, true, true);
            InitFileType(".wbd", "Bounds Dictionary", FileTypeIcon.Collisions, FileTypeAction.ViewModels, true, false, true);
            InitFileType(".was", "Animation Set", FileTypeIcon.Animation, FileTypeAction.ViewXml, true, false, true);
            InitFileType(".wat", "Action Tree", FileTypeIcon.SystemFile);
            InitFileType(".wsc", "Script", FileTypeIcon.Script);
            InitFileType(".sco", "Unused Script", FileTypeIcon.Script);
            InitFileType(".wsg", "Sector Grass", FileTypeIcon.SystemFile, FileTypeAction.ViewXml, true, false, true);
            InitFileType(".wsi", "Sector Info", FileTypeIcon.Process, FileTypeAction.ViewXml, true, false, true);
            InitFileType(".wcs", "Cover Set", FileTypeIcon.SystemFile);
            InitFileType(".wcg", "Cover Grid", FileTypeIcon.SystemFile, FileTypeAction.ViewXml, true, false, true);
            InitFileType(".wgd", "Gringo Dictionary", FileTypeIcon.SystemFile, FileTypeAction.ViewXml, true, false, true);
            InitFileType(".wsf", "ScaleForm", FileTypeIcon.Image, FileTypeAction.ViewTextures);
            InitFileType(".wsp", "Speed Tree", FileTypeIcon.SystemFile, FileTypeAction.ViewHex, false, false, true);
            InitFileType(".sst", "String Table", FileTypeIcon.TextFile, FileTypeAction.ViewXml);
            InitFileType(".wst", "String Table", FileTypeIcon.TextFile);
            InitFileType(".wtb", "Terrain Bounds", FileTypeIcon.Collisions, FileTypeAction.ViewModels, true, false, true);
            InitFileType(".wtd", "Texture Dictionary", FileTypeIcon.Image, FileTypeAction.ViewTextures, true, true, true);
            InitFileType(".wtl", "Terrain World", FileTypeIcon.SystemFile);
            InitFileType(".wtx", "Texture Map", FileTypeIcon.Image);
            InitFileType(".wpfl", "Particle Effects Library", FileTypeIcon.Image, FileTypeAction.ViewTextures, true, true, true);
            InitFileType(".wprp", "Prop", FileTypeIcon.SystemFile);
            InitFileType(".wadt", "Animation Dictionary", FileTypeIcon.Animation);
            InitFileType(".wcdt", "Clip Dictionary", FileTypeIcon.Animation, FileTypeAction.ViewXml, true, false, true);
            InitFileType(".wedt", "Expressions Dictionary", FileTypeIcon.TextFile, FileTypeAction.ViewXml, true, false, true);
            InitFileType(".wpdt", "Parametized Motion Dictionary", FileTypeIcon.TextFile);
            InitFileType(".awc", "Audio Wave Container", FileTypeIcon.Audio, FileTypeAction.ViewAudio);
        }

        private void InitGenericRDR1Types()
        {
            InitFileType(".cutbin", "Cutscene Binary", FileTypeIcon.XmlFile, FileTypeAction.ViewXml, true);
            InitFileType(".strtbl", "String Table", FileTypeIcon.TextFile, FileTypeAction.ViewXml, true);
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
            this.LoadStartupCache();

            if (Rpf6Crypto.Init())
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
            if (EntryDict.Count != 0)
                return;

            foreach (var archive in AllArchives)
            {
                if (archive.AllEntries != null)
                {
                    ArchiveDict[archive.Path] = archive;
                    foreach (Rpf6Entry entry in archive.AllEntries.Cast<Rpf6Entry>())
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
            DoTests();
        }

        private void InitGameFiles()
        {
            Core.Engine.Console.Write("RDR1.InitGameFiles", "Initialising RDR1...");
            DataFileMgr ??= new Rpf6DataFileMgr(this);
            DataFileMgr.Init();
            Core.Engine.Console.Write("RDR1.InitGameFiles", "RDR1 Initialised.");
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

        public override GameArchive CreateArchive(GameArchiveDirectory dir, string name)
        {
            throw new Exception("An archive cannot contain another archive"); //Nope, not in RDR1
        }

        public override GameArchive CreateArchive(string gamefolder, string relpath)
        {
            return Rpf6File.CreateNew(gamefolder, relpath);
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

        public override void Defragment(GameArchive file, Action<string, float> progress = null) //TODO: Add defragmentation
        {
            
        }

        public override string ConvertToXml(GameArchiveFileInfo file, byte[] data, out string newfilename, out object infoObject, string folder = "")
        {
            infoObject = null;
            var fileext = Path.GetExtension(file.Name).ToLowerInvariant();

            switch (fileext)
            {
                case ".fonttex":
                    var fonttex = new FonttexFile();
                    fonttex.Load(data);
                    newfilename = file.Name + ".xml";
                    return XmlMetaNodeWriter.GetXml("RDR1FontTex", fonttex, GetXmlFileFolder(file, folder));
                case ".xml":
                case ".meta":
                    newfilename = file.Name;
                    return TextUtil.GetUTF8Text(data);

                case ".wft": return ConvertToXml<WftFile>(file, data, out newfilename, "RDR1Fragment", GetXmlFileFolder(file, folder));
                case ".wfd": return ConvertToXml<WfdFile>(file, data, out newfilename, "RDR1FragDrawable", GetXmlFileFolder(file, folder));
                case ".wvd": return ConvertToXml<WvdFile>(file, data, out newfilename, "RDR1VisualDictionary", GetXmlFileFolder(file, folder));
                case ".wsi": return ConvertToXml<WsiFile>(file, data, out newfilename, "RDR1SectorInfo");
                case ".wcdt": return ConvertToXml<WcdtFile>(file, data, out newfilename, "RDR1ClipDictionary");
                case ".wedt": return ConvertToXml<WedtFile>(file, data, out newfilename, "RDR1ExpressionsDictionary");
                case ".wtd": return ConvertToXml<WtdFile>(file, data, out newfilename, "RDR1TextureDictionary", GetXmlFileFolder(file, folder));
                case ".was": return ConvertToXml<WasFile>(file, data, out newfilename, "RDR1AnimationSet");
                case ".wsg": return ConvertToXml<WsgFile>(file, data, out newfilename, "RDR1SectorGrass");
                case ".wcg": return ConvertToXml<WcgFile>(file, data, out newfilename, "RDR1CoverGrid");
                case ".wbd": return ConvertToXml<WbdFile>(file, data, out newfilename, "RDR1BoundsDictionary");
                case ".sst": return ConvertToXml<SstFile>(file, data, out newfilename, "RDR1StringTable");
                case ".wtb": return ConvertToXml<WtbFile>(file, data, out newfilename, "RDR1TerritoryBounds");
                case ".wgd": return ConvertToXml<WgdFile>(file, data, out newfilename, "RDR1GringoDictionary");
                case ".wpfl": return ConvertToXml<WpflFile>(file, data, out newfilename, "RDR1ParticleEffectsLibrary", GetXmlFileFolder(file, folder));
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
            if (filename.EndsWith(".strtbl.xml"))
            {
                var strtbl = new StrtblFile(xml);
                return strtbl.Save();
            }
            else if(filename.EndsWith(".fonttex.xml"))
            {
                var fonttex = XmlMetaNodeReader.GetMetaNode<FonttexFile>(xml, null, folder);
                return fonttex?.Save();
            }
            else if (filename.EndsWith(".wvd.xml")) return ConvertFromXml<WvdFile>(xml, folder);
            else if (filename.EndsWith(".wfd.xml")) return ConvertFromXml<WfdFile>(xml, folder);
            else if (filename.EndsWith(".wft.xml")) return ConvertFromXml<WftFile>(xml, folder);
            else if (filename.EndsWith(".wtd.xml")) return ConvertFromXml<WtdFile>(xml, folder);
            else if (filename.EndsWith(".wsi.xml")) return ConvertFromXml<WsiFile>(xml);
            else if (filename.EndsWith(".wsg.xml")) return ConvertFromXml<WsgFile>(xml);
            else if (filename.EndsWith(".wbd.xml")) return ConvertFromXml<WbdFile>(xml);
            else if (filename.EndsWith(".wedt.xml")) return ConvertFromXml<WedtFile>(xml);
            else if (filename.EndsWith(".wtb.xml")) return ConvertFromXml<WtbFile>(xml);
            return null;
        }

        public override string GetXmlFormatName(string filename, out int trimlength)
        {
            trimlength = 4;
            return "RSC XML";
        }

        public override string ConvertToText(GameArchiveFileInfo file, byte[] data, out string newfilename)
        {
            newfilename = file.Name;
            return TextUtil.GetUTF8Text(data);
        }

        public override byte[] ConvertFromText(string text, string filename)
        {
            return Encoding.UTF8.GetBytes(text);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FilePack LoadFilePack(Rpf6FileEntry entry, byte[] data, bool loadDependencies = true)
        {
            if (entry == null) return null;
            data = EnsureFileData(entry, data);
            if (data == null) return null;

            var enl = entry.NameLower;
            if (enl.EndsWith(".wft")) //Fragments
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
            else if (entry.Name.EndsWith(".sst")) //Stringtable
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
            return null;
        }

        public void LoadDependencies(PiecePack pack)
        {
            if (pack?.Pieces == null) return;

            Textures.Clear();
            var fileent = pack.FileInfo as Rpf6FileEntry;
            var filehash = fileent?.ShortNameHash ?? 0;
            var fragment = fileent?.Name.EndsWith(".wft") ?? false;
            var visualDict = fileent?.Name.EndsWith(".wvd") ?? false;

            Rsc6Texture[] textures = visualDict ? LoadVisualDictTextures(fileent, pack) : LoadFragmentTextures(fileent);
            if (textures == null || textures.Length == 0) return;

            var texturesDict = textures.GroupBy(t => t.Name.ToLowerInvariant()).ToDictionary(g => g.Key, g => g.First()); //Dictionary for fast lookups

            //Parallelize piece processing
            Parallel.ForEach(pack.Pieces.Values, piece =>
            {
                var allMeshes = piece?.AllModels?.SelectMany(p => p.Meshes).ToArray();
                if (allMeshes == null || allMeshes.Length == 0) return;

                Parallel.ForEach(allMeshes, mesh =>
                {
                    if (mesh?.Textures == null) return;

                    for (int i = 0; i < mesh.Textures.Length; i++)
                    {
                        var texture = mesh.Textures[i];
                        if ((texture == null || texture.Data != null) && !fragment) continue;
                        if (fragment && texture == null) continue;

                        var texName = texture.Name.ToLowerInvariant();
                        var tex = texturesDict.Values.FirstOrDefault(t => t.Name.ToLowerInvariant().Contains(texName));
                        if (tex == null) continue;

                        var previousTexture = Rsc6Texture.Create(mesh.Textures[i]); //Create copy of the original low-res texture
                        mesh.Textures[i] = tex;

                        //Update shader parameters
                        foreach (var shader in ((Rsc6Drawable)piece)?.ShaderGroup.Item?.Shaders.Items)
                        {
                            foreach (var param in shader?.ParametersList.Item?.Parameters)
                            {
                                if (param.DataType != 0 || param.Texture == null) continue;

                                if (tex.Name.Contains(param.Texture.Name.ToLowerInvariant()))
                                {
                                    param.Texture.Width = tex.Width;
                                    param.Texture.Height = tex.Height;
                                    param.Texture.MipLevels = tex.MipLevels;
                                    param.Texture.Format = tex.Format;
                                    param.Texture.Pack = tex.Pack;

                                    //Preserve original low-res texture for texture editor preview
                                    if (param.Texture.Data != null)
                                    {
                                        lock (piece.TexturePack.Textures)
                                        {
                                            piece.TexturePack.Textures[param.Texture.Name] = previousTexture;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                });
            });
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
            return hs.Count > 0 ? hs.ToArray() : null;
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
                    case ".wbd": return Rsc6DataReader.Analyze<Rsc6BoundsDictionary>(rfe, data);
                    case ".sst": return Rsc6DataReader.Analyze<Rsc6StringTable>(rfe, data);
                    case ".wedt": return Rsc6DataReader.Analyze<Rsc6ExpressionDictionary>(rfe, data);
                    case ".wgd": return Rsc6DataReader.Analyze<Rsc6GringoDictionary>(rfe, data);
                    case ".wpfl": return Rsc6DataReader.Analyze<Rsc6ParticleEffects>(rfe, data);
                }
            }
            return new Tuple<string>("Unable to analyze file.");
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
                StreamingItems = new Rsc6SectorInfo()
            };
            var data = wsi.Save();
            return data;
        }

        Rsc6Texture[] LoadVisualDictTextures(Rpf6FileEntry entry, PiecePack pack = null)
        {
            var FileManager = Game.GetFileManager() as Rpf6FileManager;
            var dfm = FileManager.DataFileMgr;
            var wvdParent = entry.Parent.Parent;

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
                                if (Textures.Find(item => item.Name == texture.Name) != null) continue;

                                string desiredWtd = texture.Name.Remove(texture.Name.LastIndexOf(".")).ToLower();
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
                                        Textures.Add((Rsc6Texture)tex);
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
                            if (visualDict.TextureDictionary.Item.Textures[i] == null) continue;
                            if (Textures.Find(item => item.Name == visualDict.TextureDictionary.Item.Textures[i].Name) == null)
                            {
                                Textures.Add(visualDict.TextureDictionary.Item.Textures[i]);
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
                                if (Textures.Find(item => item.Name.Contains(tex.Name.ToLowerInvariant())) == null)
                                {
                                    Textures.Add((Rsc6Texture)tex);
                                }
                            }
                        }
                    }
                }
            }

            //Add also the textures from swAll.wtd
            Textures.AddRange(dfm.SwAll.ToArray());

            return Textures.ToArray();
        }

        private Rsc6Texture[] LoadFragmentTextures(Rpf6FileEntry entry)
        {
            Textures.Clear();
            var texturesHashSet = new HashSet<Rsc6Texture>();
            var FileManager = Game.GetFileManager() as Rpf6FileManager;
            var dfm = FileManager.DataFileMgr;

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
            string desiredWtd = entry.Name.Remove(entry.Name.LastIndexOf(".")).ToLower();
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

            return texturesHashSet.ToArray();
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
                            case ".strtbl":
                                var strtbl = new StrtblFile(entry);
                                strtbl.Load(data);
                                return strtbl;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rpf6FileExt GetRpf6FileExt(string filename)
        {
            var extstr = Path.GetExtension(filename).Replace(".", "").ToLowerInvariant();
            Enum.TryParse<Rpf6FileExt>(extstr, out var ext);
            return ext;
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
            Store.LoadStartupCache(br);
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

            Store.SaveStartupCache(bw);

            var buf = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buf, 0, buf.Length);

            File.WriteAllBytes(file, buf);
        }

        public override T LoadMetaNode<T>(GameArchiveFileInfo file, byte[] data = null)
        {
            throw new NotImplementedException();
        }

        private void DoTests()
        {
            bool animations = false, shaders = false, terrainBounds = false, grass = false, strtable = false, fragments = false, expressions = false;
            var listShaders = new List<Rsc6ShaderFX>();
            var listIds = new List<string>();

            if (!animations && !shaders && !terrainBounds && !grass && !strtable && !fragments && !expressions) return;
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
                        if (strtable && n.EndsWith(".strtbl") && archive.Name == "strings_switch.rpf")
                        {
                            Core.Engine.Console.Write("StringTable", fe.Path);
                            var data = EnsureFileData(fe, null);
                            if (data != null)
                            {
                                var strtbl = new StrtblFile(fe);
                                strtbl.Load(data);
                            }
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
                            prams.AppendChild(CreateVectorItemElement(doc, name, parameter.Vector.Vector));
                        else
                            prams.AppendChild(CreateArrayItemElement(doc, name, (int)type * 16, parameter.Array.Array));
                    }
                }
                //doc.Save(@"C:\Users\fumol\AppData\Roaming\Blender Foundation\Blender\4.0\scripts\addons\SollumzRDR-wvd\cwxml\RDR1Shaders.xml");
            }
        }

        public XmlElement CreateElement(XmlDocument doc, string name, string value)
        {
            XmlElement element = doc.CreateElement(name);
            element.InnerText = value;
            return element;
        }

        public XmlElement CreateTextureItemElement(XmlDocument doc, string name)
        {
            XmlElement element = doc.CreateElement("Item");
            element.SetAttribute("name", name);
            element.SetAttribute("type", "Texture");
            return element;
        }

        public XmlElement CreateVectorItemElement(XmlDocument doc, string name, Vector4 vector)
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

        public XmlElement CreateArrayItemElement(XmlDocument doc, string name, int length, Vector4[] array)
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
    }

    public class Rpf6DataFileMgr
    {
        public Rpf6FileManager FileManager;
        public List<Rpf6FileEntry> AllEntries;
        public Dictionary<Rpf6FileExt, Dictionary<JenkHash, Rpf6FileEntry>> StreamEntries;
        public Dictionary<JenkHash, WasFile> WasFiles;
        public Dictionary<JenkHash, WsiFile> WsiFiles;
        public Dictionary<JenkHash, WspFile> WspFiles;
        public Dictionary<JenkHash, WsgFile> WsgFiles;
        public List<Rsc6Texture> SwAll; //For storing textures from swAll.wtd
        public List<Rsc6Texture> FragTextures; //For storing textures from fragmentTextureList.wtd
        public Dictionary<string, string[]> SmicMapping; //For the texture mapping from smicToFragMap.txt

        public Rpf6DataFileMgr(Rpf6FileManager fman)
        {
            this.FileManager = fman;
        }

        public void Init()
        {
            if (this.StreamEntries != null) return;

            this.AllEntries = new List<Rpf6FileEntry>();
            this.StreamEntries = new Dictionary<Rpf6FileExt, Dictionary<JenkHash, Rpf6FileEntry>>();
            this.WasFiles = new Dictionary<JenkHash, WasFile>();
            this.WsiFiles = new Dictionary<JenkHash, WsiFile>();
            this.WspFiles = new Dictionary<JenkHash, WspFile>();
            this.WsgFiles = new Dictionary<JenkHash, WsgFile>();
            this.SwAll = new List<Rsc6Texture>();
            this.FragTextures = new List<Rsc6Texture>();
            this.SmicMapping = new Dictionary<string, string[]>();

            this.LoadFiles();
            this.LoadResidentTextures();
            this.LoadSmicMap();

            if (RDR1Map.LoadingMap)
            {
                this.LoadAnimations();
                this.LoadSectorInfoData();
                this.LoadTrees();
                this.LoadGrass();
            }
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
                            entries = new Dictionary<JenkHash, Rpf6FileEntry>();
                            this.StreamEntries[fe.ResourceType] = entries;
                        }
                        entries[hash] = fe;
                    }
                    else if (file.Name.EndsWith(".txt") || file.Name.EndsWith(".vehsim") || (file.Name.EndsWith(".bin") && file.Parent.Name == "fragments"))
                    {
                        if (!this.StreamEntries.TryGetValue(Rpf6FileExt.binary, out var entries))
                        {
                            entries = new Dictionary<JenkHash, Rpf6FileEntry>();
                            this.StreamEntries[Rpf6FileExt.binary] = entries;
                        }
                        entries[fe.ShortNameHash] = fe;
                    }
                }
            }
        }

        private void LoadResidentTextures()
        {
            //Load the textures from swAll.wtd
            this.StreamEntries[Rpf6FileExt.wtd_wtx].TryGetValue(0x5D960088, out Rpf6FileEntry swall);
            if (swall != null)
            {
                var r = this.FileManager.LoadTexturePack(swall);
                if (r != null)
                {
                    foreach (var tex in r.Textures.Values)
                    {
                        if (tex == null) continue;
                        if (this.SwAll.Find(item => item.Name.Contains(tex.Name.ToLowerInvariant())) == null)
                        {
                            this.SwAll.Add((Rsc6Texture)tex);
                        }
                    }
                }
            }

            //Load the textures from fragmentTextureList.wtd
            this.StreamEntries[Rpf6FileExt.wtd_wtx].TryGetValue(0x4D6D7386, out Rpf6FileEntry texList);
            if (texList != null)
            {
                var r = this.FileManager.LoadTexturePack(texList);
                if (r != null)
                {
                    foreach (var tex in r.Textures.Values)
                    {
                        if (tex == null) continue;
                        if (this.FragTextures.Find(item => item.Name.Contains(tex.Name.ToLowerInvariant())) == null)
                        {
                            this.FragTextures.Add((Rsc6Texture)tex);
                        }
                    }
                }
            }
        }

        private void LoadSmicMap()
        {
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
                        bool dollar = s.StartsWith("$");
                        string child = s[(dollar ? 1 : 0)..s.IndexOf(" ")].ToLowerInvariant();

                        int smicIndex = s.IndexOf("smic_");
                        var smicContent = s[smicIndex..].Split(" ");
                        SmicMapping[child] = smicContent;
                    }
                }
            }
        }

        private void LoadAnimations()
        {
            foreach (var se in this.StreamEntries[Rpf6FileExt.was])
            {
                var fe = se.Value;
                var wasdata = fe.Archive.ExtractFile(fe);

                if (wasdata != null)
                {
                    var was = new WasFile(fe);
                    was.Load(wasdata);

                    var hash = fe.ShortNameHash;
                    JenkIndex.Ensure(was.Name, "RDR1");
                    this.WasFiles[hash] = was;
                }
            }
        }

        private void LoadSectorInfoData()
        {
            foreach (var se in this.StreamEntries[Rpf6FileExt.wsi])
            {
                var fe = se.Value;
                var wsidata = fe.Archive.ExtractFile(fe);

                if (wsidata != null)
                {
                    var wsi = new WsiFile(fe);
                    wsi.Load(wsidata);

                    var hash = fe.ShortNameHash;
                    JenkIndex.Ensure(wsi.Name, "RDR1");
                    this.WsiFiles[hash] = wsi;
                }
            }
        }

        private void LoadTrees()
        {
            foreach (var se in this.StreamEntries[Rpf6FileExt.wsp])
            {
                var fe = se.Value;
                var wspdata = fe.Archive.ExtractFile(fe);

                if (wspdata != null)
                {
                    var wsp = new WspFile(fe);
                    wsp.Load(wspdata);

                    var hash = fe.ShortNameHash;
                    JenkIndex.Ensure(wsp.Name, "RDR1");
                    this.WspFiles[hash] = wsp;
                }
            }
        }

        private void LoadGrass()
        {
            foreach (var se in this.StreamEntries[Rpf6FileExt.wsg_wgd])
            {
                var fe = se.Value;
                if (!fe.Name.EndsWith(".wsg")) continue;
                var wsgdata = fe.Archive.ExtractFile(fe);

                if (wsgdata != null)
                {
                    var wsg = new WsgFile(fe);
                    wsg.Load(wsgdata);

                    var hash = fe.ShortNameHash;
                    JenkIndex.Ensure(wsg.Name, "RDR1");
                    this.WsgFiles[hash] = wsg;
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

    public class Rpf6DataFileDevice
    {
        public Rpf6DataFileMgr DataFileMgr;
        public Rpf6FileManager FileManager;
        public string Name;
        public string PhysicalPath;

        public Rpf6DataFileDevice(Rpf6DataFileMgr dfm, string name, string path)
        {
            this.DataFileMgr = dfm;
            this.FileManager = dfm.FileManager;
            this.Name = name;
            this.PhysicalPath = FileManager.Folder + "\\" + path;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class Rpf6Store
    {
        public Rpf6FileManager FileMan;
        public ConcurrentDictionary<string, Rpf6StoreItem> BoundsDict;
        public ConcurrentDictionary<string, Rpf6StoreItem> TerritoryBoundsDict;
        public ConcurrentDictionary<string, Rpf6StoreItem> TilesDict;

        public Rpf6Store(Rpf6FileManager fileman)
        {
            FileMan = fileman;
        }

        public void SaveStartupCache(BinaryWriter bw)
        {
            var materials = new List<Rpf6MaterialStoreItem>();
            var bounds = new ConcurrentBag<Rpf6StoreItem>();
            var territoryBounds = new ConcurrentBag<Rpf6StoreItem>();
            var tiles = new ConcurrentBag<Rpf6StoreItem>();
            var wbds = FileMan.DataFileMgr.StreamEntries[Rpf6FileExt.wbd_wcdt];
            var wtbs = FileMan.DataFileMgr.StreamEntries[Rpf6FileExt.wtb];
            var wvds = FileMan.DataFileMgr.StreamEntries[Rpf6FileExt.wvd];

            Core.Engine.Console.Write("Rpf6FileManager", "Building RDR1 startup cache - Materials");
            var mats = Rsc6BoundsMaterialTypes.GetMaterials(FileMan);
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

            Core.Engine.Console.Write("Rpf6FileManager", "Building RDR1 startup cache - Bounds dictionary");
            Parallel.ForEach(wbds, kv =>
            {
                var value = kv.Value;
                if (value == null || !value.Name.EndsWith(".wbd") || value.NameLower.Contains("props")) return;
                var pack = FileMan.LoadPiecePack(value);

                if (pack != null)
                {
                    var wbd = (WbdFile)pack;
                    var item = new Rpf6StoreItem
                    {
                        Path = value.PathLower,
                        Hash = value.ShortNameHash,
                        Box = wbd.BoundingBox
                    };
                    bounds.Add(item);
                }
            });

            Core.Engine.Console.Write("Rpf6FileManager", "Building RDR1 startup cache - Territory bounds");
            Parallel.ForEach(wtbs, kv =>
            {
                var value = kv.Value;
                if (value == null) return;
                var pack = FileMan.LoadPiecePack(value);

                if (pack != null)
                {
                    var wtb = (WtbFile)pack;
                    var item = new Rpf6StoreItem
                    {
                        Path = value.PathLower,
                        Hash = value.ShortNameHash,
                        Box = wtb.BoundingBox
                    };
                    territoryBounds.Add(item);
                }
            });

            Core.Engine.Console.Write("Rpf6FileManager", "Building RDR1 startup cache - Visual dictionary");
            Parallel.ForEach(wvds, kv =>
            {
                var value = kv.Value;
                if (value == null || (!value.NameLower.StartsWith("tile_") && RDR1Map.ShouldSkipVisualDict(value))) return;
                var pack = FileMan.LoadPiecePack(value);

                if (pack != null)
                {
                    var wvd = (WvdFile)pack;
                    var hash = new JenkHash(value.ShortNameLower);
                    var item = new Rpf6StoreItem
                    {
                        Path = value.PathLower,
                        Hash = hash,
                        Box = wvd.BoundingBox
                    };
                    tiles.Add(item);
                }
            });

            //Serialize the items
            SerializeItems(bw, bounds.ToList());
            SerializeItems(bw, territoryBounds.ToList());
            SerializeItems(bw, tiles.ToList());
            SerializeItems(bw, null, materials.ToList());

            //Populate dictionaries (using ConcurrentDictionary to ensure thread-safety)
            BoundsDict = new ConcurrentDictionary<string, Rpf6StoreItem>(bounds.Select(item => new KeyValuePair<string, Rpf6StoreItem>(item.Path, item)));
            TerritoryBoundsDict = new ConcurrentDictionary<string, Rpf6StoreItem>(territoryBounds.Select(item => new KeyValuePair<string, Rpf6StoreItem>(item.Path, item)));
            TilesDict = new ConcurrentDictionary<string, Rpf6StoreItem>(tiles.Select(item => new KeyValuePair<string, Rpf6StoreItem>(item.Path, item)));
        }

        public void LoadStartupCache(BinaryReader br)
        {
            var matItems = new List<Rpf6MaterialStoreItem>();
            BoundsDict = new ConcurrentDictionary<string, Rpf6StoreItem>();
            TerritoryBoundsDict = new ConcurrentDictionary<string, Rpf6StoreItem>();
            TilesDict = new ConcurrentDictionary<string, Rpf6StoreItem>();

            DeserializeItems(br, BoundsDict);
            DeserializeItems(br, TerritoryBoundsDict);
            DeserializeItems(br, TilesDict);
            DeserializeItems(br, null, matItems);

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
                Rsc6BoundsMaterialTypes.Materials = mats.ToList();
            }
        }

        public static void SerializeItems(BinaryWriter bw, List<Rpf6StoreItem> list, List<Rpf6MaterialStoreItem> mats = null)
        {
            if (list != null)
            {
                bw.Write(list.Count);
                foreach (var item in list)
                {
                    bw.WriteStringNullTerminated(item.Path);
                    bw.Write(item.Hash);
                    bw.Write(item.Box.Minimum.X);
                    bw.Write(item.Box.Minimum.Y);
                    bw.Write(item.Box.Minimum.Z);
                    bw.Write(item.Box.Maximum.X);
                    bw.Write(item.Box.Maximum.Y);
                    bw.Write(item.Box.Maximum.Z);
                }
            }
            else
            {
                bw.Write(mats.Count);
                foreach (var item in mats)
                {
                    bw.WriteStringNullTerminated(item.Name);
                    bw.Write((int)item.Color);
                }
            }
        }

        public static void DeserializeItems(BinaryReader br, ConcurrentDictionary<string, Rpf6StoreItem> dict, List<Rpf6MaterialStoreItem> mats = null)
        {
            if (dict != null)
            {
                var itemCount = br.ReadInt32();
                for (int i = 0; i < itemCount; i++)
                {
                    var item = new Rpf6StoreItem
                    {
                        Path = br.ReadStringNullTerminated(),
                        Hash = br.ReadUInt32()
                    };
                    item.Box.Minimum.X = br.ReadSingle();
                    item.Box.Minimum.Y = br.ReadSingle();
                    item.Box.Minimum.Z = br.ReadSingle();
                    item.Box.Maximum.X = br.ReadSingle();
                    item.Box.Maximum.Y = br.ReadSingle();
                    item.Box.Maximum.Z = br.ReadSingle();
                    dict[item.Path] = item;
                }
            }
            else
            {
                var matCount = br.ReadInt32();
                for (int i = 0; i < matCount; i++)
                {
                    var item = new Rpf6MaterialStoreItem
                    {
                        Name = br.ReadStringNullTerminated(),
                        Color = new Colour(br.ReadInt32())
                    };
                    mats.Add(item);
                }
            }
        }
    }

    [TC(typeof(EXP))] public struct Rpf6StoreItem
    {
        public string Path;
        public JenkHash Hash;
        public BoundingBox Box;

        public override readonly string ToString()
        {
            return $"{Hash} : {Box}";
        }
    }

    [TC(typeof(EXP))] public struct Rpf6MaterialStoreItem
    {
        public string Name;
        public Colour Color;

        public override readonly string ToString()
        {
            return $"{Name} : {Color}";
        }
    }
}