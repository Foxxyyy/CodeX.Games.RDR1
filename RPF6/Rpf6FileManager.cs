using CodeX.Core.Engine;
using CodeX.Core.Utilities;
using CodeX.Games.RDR1.Files;
using CodeX.Games.RDR1.RSC6;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace CodeX.Games.RDR1.RPF6
{
    public class Rpf6FileManager : FileManager
    {
        public override string ArchiveTypeName => "RPF6";
        public override string ArchiveExtension => ".rpf";
        public Rpf6DataFileMgr DataFileMgr { get; set; }
        public List<Rsc6Texture> Textures { get; set; } = new List<Rsc6Texture>();
        private bool ConvertXML { get; set; } //Fix for RenderableCache

        public Rpf6FileManager(RDR1Game game) : base(game)
        {
        }

        public override void InitFileTypes()
        {
            InitGenericFileTypes();
            InitGenericRDR1Types();

            InitFileType(".rpf", "Rage Package File", FileTypeIcon.Archive);
            InitFileType(".bk2", "Bink Video 2", FileTypeIcon.Movie);
            InitFileType(".dat", "Data File", FileTypeIcon.SystemFile);
            InitFileType(".nvn", "Compiled Shaders", FileTypeIcon.SystemFile, FileTypeAction.ViewHex, true, true);
            InitFileType(".cutbin", "Cutscene Binary", FileTypeIcon.XmlFile, FileTypeAction.ViewXml, true, true);
            InitFileType(".wnm", "Nav Mesh", FileTypeIcon.SystemFile);
            InitFileType(".wfd", "Frag Drawable", FileTypeIcon.Piece, FileTypeAction.ViewModels, true, true);
            InitFileType(".wft", "Fragment", FileTypeIcon.Piece, FileTypeAction.ViewModels, true, true);
            InitFileType(".wvd", "Visual Dictionary", FileTypeIcon.Piece, FileTypeAction.ViewModels, true, true);
            InitFileType(".wbd", "Bounds Dictionary", FileTypeIcon.Collisions, FileTypeAction.ViewModels);
            InitFileType(".was", "Anim Set", FileTypeIcon.SystemFile);
            InitFileType(".wat", "Action Tree", FileTypeIcon.SystemFile);
            InitFileType(".wsc", "Script", FileTypeIcon.Script);
            InitFileType(".sco", "Unused Script", FileTypeIcon.Script);
            InitFileType(".wsg", "Sector Grass", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".wsi", "Sector Info", FileTypeIcon.Process, FileTypeAction.ViewText, true);
            InitFileType(".wcs", "Cover Set", FileTypeIcon.SystemFile);
            InitFileType(".wgd", "Gringo Dictionary", FileTypeIcon.SystemFile);
            InitFileType(".wsf", "ScaleForm", FileTypeIcon.Image, FileTypeAction.ViewTextures);
            InitFileType(".wsp", "Speed Tree", FileTypeIcon.SystemFile, FileTypeAction.ViewText);
            InitFileType(".sst", "String Table", FileTypeIcon.TextFile);
            InitFileType(".wst", "String Table", FileTypeIcon.TextFile);
            InitFileType(".wtb", "Terrain Bounds", FileTypeIcon.Collisions);
            InitFileType(".wtd", "Texture Dictionary", FileTypeIcon.Image, FileTypeAction.ViewTextures);
            InitFileType(".wtl", "Terrain World", FileTypeIcon.SystemFile);
            InitFileType(".wtx", "Texture Map", FileTypeIcon.Image);
            InitFileType(".wpfl", "Particle Effects Library", FileTypeIcon.Image, FileTypeAction.ViewTextures);
            InitFileType(".wprp", "Prop", FileTypeIcon.SystemFile);
            InitFileType(".wadt", "Animation Dictionary", FileTypeIcon.Animation);
            InitFileType(".wcdt", "Clip Dictionary", FileTypeIcon.Animation);
            InitFileType(".wedt", "Expressions Dictionary", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".wpdt", "Parametized Motion Dictionary", FileTypeIcon.TextFile);
            InitFileType(".awc", "Audio Wave Container", FileTypeIcon.Audio, FileTypeAction.ViewAudio);
            InitFileType(".strtbl", "String Table", FileTypeIcon.TextFile);
        }

        private void InitGenericRDR1Types()
        {
            InitFileType(".tr", "AI Programs", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".csv", "Comma-Separated Values File", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".cfg", "Config File", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".refgroup", "Reference Group", FileTypeIcon.TextFile, FileTypeAction.ViewText);
            InitFileType(".fxlist", "Effects List", FileTypeIcon.TextFile, FileTypeAction.ViewText);
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
        }

        public override bool Init()
        {
            JenkIndex.LoadStringsFile("RDR1");
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
            //TestAnimations();
        }

        private void InitGameFiles()
        {
            Core.Engine.Console.Write("RDR1.InitGameFiles", "Initialising RDR1...");
            Rsc6BoundsMaterialTypes.Init(this);
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

        public override void Defragment(GameArchive file, Action<string, float> progress = null) //TODO:
        {
            //var f = file as Rpf6File;
            //if (f == null) return;
            //f.Defragment(progress);
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
            }

            var fmtext = "";
            if (file is Rpf6ResourceFileEntry re)
            {
                fmtext = ".rsc";
                var isResFilepack = false;
                var isRawBinaryRes = false;

                switch (re.ResourceType)
                {
                    case Rpf6FileExt.generic:
                    case Rpf6FileExt.wft:
                    case Rpf6FileExt.wvd:
                        isResFilepack = true;
                        break;
                    case Rpf6FileExt.wsi:
                        isRawBinaryRes = true;
                        break;
                    default:
                        throw new NotImplementedException("Sorry, CodeX currently cannot convert RDR1 " + re.ResourceType.ToString() + " files to XML.\nCodeX is a work in progress and this is a planned future feature.");
                }

                if (re.ResourceType == Rpf6FileExt.generic && fileext != ".wfd")
                {
                    newfilename = file.Name;
                    return string.Empty;
                }

                folder = string.IsNullOrEmpty(folder) ? "" : Path.Combine(folder, Path.GetFileNameWithoutExtension(file.Name));
                if (isResFilepack)
                {
                    if (fileext == ".wvd" || fileext == ".wfd")
                    {
                        ConvertXML = true;
                        newfilename = file.Name + ".xml";
                        PiecePack piecePack = LoadPiecePack(file, data, true);
                        ConvertXML = false;

                        if (data != null)
                        {
                            if (piecePack is WvdFile wvdFile)
                                return wvdFile.WriteXml(folder);
                            else if (piecePack is WfdFile wfdFile)
                                return wfdFile.WriteXml(folder);
                        }
                    }
                    else //Waiting for Sollumz to support dexy's new xmls
                    {
                        newfilename = file.Name + ".xml";
                        var fp = LoadFilePack(re, data, false);
                        //if (fp is WvdFile wvd) return XmlMetaNodeWriter.GetXml("RDR1VolumeData", wvd.DrawableDictionary);
                        if (fp is WftFile wft) return XmlMetaNodeWriter.GetXml("RDR1Fragment", wft.Fragment, folder);
                        throw new Exception("There was an error converting the " + re.ResourceType.ToString() + " file to XML.");
                    }
                }
                else if (isRawBinaryRes) //We can atleast use the new xmls for .wsi resources since we don't need Sollumz
                {
                    newfilename = file.Name + ".xml";
                    var wsi = new WsiFile(re);
                    wsi.Load(data);
                    return XmlMetaNodeWriter.GetXml("RDR1SectorInfo", wsi.StreamingItems, folder);
                    throw new Exception("There was an error converting the " + re.ResourceType.ToString() + " file to XML.");
                }
            }

            newfilename = file.Name + fmtext + ".xml";

            var bag = LoadMetadata(file, data);
            if (bag != null)
            {
                infoObject = bag.Owner ?? bag;
                return bag.ToXml();
            }        
            return string.Empty;
        }

        public override byte[] ConvertFromXml(string xml, string filename, string folder = "")
        {
            //Old xmls methods
            if (filename.EndsWith(".wvd.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                WvdFile wvd = new WvdFile(filename);
                var ddsfolder = folder;

                var node = doc.DocumentElement;
                if (node != null)
                {
                    wvd.DrawableDictionary = wvd.ReadXmlNode(node, ddsfolder);
                }
                wvd.Name = filename.Replace(".wvd.xml", ".wvd");
                return wvd.Save();
            }
            if (filename.EndsWith(".wfd.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                WfdFile wfd = new WfdFile(filename);
                var ddsfolder = folder;

                var node = doc.DocumentElement;
                if (node != null)
                {
                    wfd.Drawable = wfd.ReadXmlNode(node, ddsfolder);
                }
                wfd.Name = filename.Replace(".wfd.xml", ".wfd");
                return wfd.Save();
            }
            else if (filename.EndsWith(".wsi.xml"))
            {
                var wsi = new WsiFile(XmlMetaNodeReader.GetMetaNode<Rsc6SectorInfo>(xml));
                return wsi.Save();
            }
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
            if (file is Rpf6ResourceFileEntry entry)
            {
                if (entry != null & entry.FlagInfos.IsResource)
                {
                    if (entry.ResourceType == Rpf6FileExt.wsi) //sector info
                    {
                        var sector = new WsiFile(entry);
                        sector.Load(data);
                        return sector.ToString();
                    }
                    else if (entry.ResourceType == Rpf6FileExt.wsg) //grass
                    {
                        var grass = new WsgFile(entry);
                        grass.Load(data);
                        return grass.ToString();
                    }
                    else if (entry.ResourceType == Rpf6FileExt.wsp) //trees
                    {
                        var tree = new WspFile(entry);
                        tree.Load(data);
                        return tree.ToString();
                    }
                    else if (entry.ResourceType == Rpf6FileExt.wedt) //expressions
                    {
                        var expr = new WedtFile(entry);
                        expr.Load(data);
                        return expr.ToString();
                    }
                }
            }
            return TextUtil.GetUTF8Text(data);
        }

        public override byte[] ConvertFromText(string text, string filename)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        public override TexturePack LoadTexturePack(GameArchiveFileInfo file, byte[] data = null)
        {
            data = EnsureFileData(file, data);

            if (data == null)
                return null;
            if (file is not Rpf6FileEntry entry)
                return null;

            if (file.NameLower.EndsWith(".wtd")) //texture dictionary
            {
                var wtd = new WtdFile(entry);
                wtd.Load(data);
                return wtd;
            }
            else if (file.NameLower.EndsWith(".wsf")) //scaleform (was made for Xbox 360..) - TODO:
            {
                var wsf = new WsfFile(entry);
                wsf.Load(data);
                return wsf;
            }
            return null;
        }

        public override PiecePack LoadPiecePack(GameArchiveFileInfo file, byte[] data = null, bool loadDependencies = false)
        {
            var fp = LoadFilePack(file as Rpf6FileEntry, data, loadDependencies);
            return fp as PiecePack;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private FilePack LoadFilePack(Rpf6FileEntry entry, byte[] data = null, bool loadDependencies = true)
        {
            if (entry == null) return null;

            data = EnsureFileData(entry, data);
            if (data == null) return null;

            switch (entry.ResourceType)
            {
                case Rpf6FileExt.generic: //frag drawable
                    if (!entry.Name.EndsWith(".wfd")) break; //generic can be different resources...
                    var wfd = new WfdFile(entry);
                    wfd.Load(data);
                    if (loadDependencies) LoadDependencies(wfd);
                    return wfd;
                case Rpf6FileExt.wft: //fragments
                    var wft = new WftFile(entry);
                    wft.Load(data);
                    if (loadDependencies) LoadDependencies(wft);
                    return wft;
                case Rpf6FileExt.wvd: //visual dictionary
                    var wvd = new WvdFile(entry);
                    wvd.Load(data);
                    if (loadDependencies) LoadDependencies(wvd);
                    return wvd;
                case Rpf6FileExt.wbd: //bounds dictionary
                    var wbd = new WbdFile(entry);
                    wbd.Load(data);
                    return wbd;
            }
            return null;
        }

        public void LoadDependencies(PiecePack pack)
        {
            if (pack?.Pieces == null)
                return;

            var fileent = pack.FileInfo as Rpf6FileEntry;
            var filehash = (fileent != null) ? fileent.ShortNameHash : 0;
            var fragment = fileent.Name.EndsWith(".wft");
            var visualDict = fileent.Name.EndsWith(".wvd");

            Rsc6Texture[] textures = null;
            if (visualDict)
                textures = LoadVolumeParentTextures(fileent, pack);
            else
                textures = LoadFragParentTextures(fileent);

            if (textures == null || textures.Length == 0)
                return;

            foreach (var kvp in pack.Pieces)
            {
                var piece = kvp.Value;
                foreach (var mesh in piece.AllModels?.SelectMany(p => p.Meshes))
                {
                    for (int i = 0; i < mesh.Textures?.Length; i++)
                    {
                        var texture = mesh.Textures[i];
                        if ((texture == null || texture.Data != null) && !fragment)
                            continue;
                        if (fragment && texture == null)
                            continue;

                        var texName = texture.Name.ToLowerInvariant();
                        foreach (var tex in textures)
                        {
                            if ((bool)(tex?.Name.Contains(texName))) //Assign each shader texture params with the external textures found
                            {
                                mesh.Textures[i] = tex;
                                foreach (var shader in ((Rsc6DrawableBase)piece)?.ShaderGroup.Item?.Shaders.Items)
                                {
                                    foreach (var param in shader?.ParametersList.Item?.Parameters)
                                    {
                                        if (param.DataType != 0 || param.Texture == null)
                                            continue;

                                        if (tex.Name.Contains(param.Texture.Name))
                                        {
                                            param.Texture.Width = tex.Width;
                                            param.Texture.Height = tex.Height;
                                            param.Texture.MipLevels = tex.MipLevels;
                                            param.Texture.Format = tex.Format;
                                            if (ConvertXML) param.Texture.Data = tex.Data; //Fix for RenderableCache
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override AudioPack LoadAudioPack(GameArchiveFileInfo file, byte[] data = null)
        {
            throw new NotImplementedException();
        }

        public static byte[] CreateNewWtd(string name)
        {
            var wtd = new WtdFile(null)
            {
                TextureDictionary = new Rsc6TextureDictionary()
            };
            var data = wtd.Save();
            return data;
        }

        private Rsc6Texture[] LoadVolumeParentTextures(Rpf6FileEntry entry, PiecePack pack = null)
        {
            var wvdParent = entry.Parent.Parent;
            if (entry.Name.StartsWith("tile")) //We're searching for a tile, their textures can be in various dictionaries
            {
                foreach (var drawable in ((WvdFile)pack).DrawableDictionary.Drawables.Items)
                {
                    foreach (var model in drawable.AllModels)
                    {
                        foreach (var mesh in model.Meshes)
                        {
                            foreach (var texture in mesh.Textures)
                            {
                                if (texture == null)
                                    continue;
                                if (texture.Height != 0 || string.IsNullOrEmpty(texture.Name))
                                    continue;
                                if (Textures.Find(item => item.Name == texture.Name) != null)
                                    continue;

                                string desiredWtd = texture.Name.Remove(texture.Name.LastIndexOf(".")).ToLower();
                                var FileManager = Game.GetFileManager() as Rpf6FileManager;
                                var dfm = FileManager.DataFileMgr;
                                var wtdFiles = dfm.StreamEntries[Rpf6FileExt.wtd];

                                foreach (var wtd in wtdFiles)
                                {
                                    if (wtd.Value.NameLower.Contains("_med") || wtd.Value.NameLower.Contains("_low"))
                                        continue;
                                    if (!wtd.Value.NameLower.StartsWith(desiredWtd))
                                        continue;

                                    var r = LoadTexturePack(wtd.Value);
                                    if (r == null)
                                        continue;

                                    foreach (var tex in r.Textures.Values)
                                    {
                                        if (tex == null)
                                            continue;
                                        Textures.Add((Rsc6Texture)tex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (wvdParent != null && !wvdParent.Name.StartsWith("resource_") && wvdParent.Name != "territory_swall_noid") //General static mesh
            {
                //Searching through the wvd parent textures
                foreach (var s in wvdParent.Directories)
                {
                    if (s.NameLower.StartsWith("0x") || s.NameLower.StartsWith("mp_"))
                        continue;
                    if (s.NameLower != wvdParent.NameLower)
                        continue;

                    foreach (var f in s.Files)
                    {
                        if (f.NameLower.Contains("vlow") || f.NameLower.Contains("med") || f.NameLower.EndsWith(".wsi") || f.NameLower.Contains("dlc"))
                            continue;

                        var r = LoadPiecePack(f);
                        var drawables = ((WvdFile)r).DrawableDictionary;

                        if (drawables == null)
                            continue;
                        for (int i = 0; i < drawables.TextureDictionary.Item.Textures.Count; i++)
                        {
                            if (drawables.TextureDictionary.Item.Textures[i] == null)
                                continue;

                            if (Textures.Find(item => item.Name == drawables.TextureDictionary.Item.Textures[i].Name) == null)
                                Textures.Add(drawables.TextureDictionary.Item.Textures[i]);
                        }
                    }
                }

                //Searching through smic_ textures
                var FileManager = Game.GetFileManager() as Rpf6FileManager;
                var dfm = FileManager.DataFileMgr;
                dfm.StreamEntries[Rpf6FileExt.binary].TryGetValue(0x57D7F6EF, out Rpf6FileEntry file); //smictofragmap.txt

                if (file != null)
                {
                    byte[] data = null;
                    data = EnsureFileData(file, data);

                    if (data != null)
                    {
                        string text = ConvertToText(file, data, out string name);
                        string[] result = text.Split(new[] { '\r', '\n' });

                        foreach (string s in result)
                        {
                            if (string.IsNullOrEmpty(s) || s.Contains(":"))
                                continue;

                            string child = s.Substring(s.StartsWith("$") ? 1 : 0, s.IndexOf(" ") - 1).ToLowerInvariant();
                            if (!entry.Name.Contains(child))
                                continue;

                            string smic = s.Substring(s.LastIndexOf("smic_")).ToLowerInvariant();
                            dfm.StreamEntries[Rpf6FileExt.wtd].TryGetValue(JenkHash.GenHash(smic), out Rpf6FileEntry smicFile);

                            if (smicFile != null)
                            {
                                var r = LoadTexturePack(smicFile);
                                if (r == null)
                                    continue;

                                foreach (var tex in r.Textures.Values)
                                {
                                    if (tex == null)
                                        continue;
                                    if (Textures.Find(item => item.Name.Contains(tex.Name)) == null)
                                        Textures.Add((Rsc6Texture)tex);
                                }
                            }
                        }
                    }
                }
            }
            return Textures.ToArray();
        }

        private Rsc6Texture[] LoadFragParentTextures(Rpf6FileEntry entry)
        {
            Textures.Clear();
            var FileManager = Game.GetFileManager() as Rpf6FileManager;
            var dfm = FileManager.DataFileMgr;
            dfm.StreamEntries[Rpf6FileExt.binary].TryGetValue(0x57D7F6EF, out Rpf6FileEntry file); //parse smictofragmap.txt

            if (file != null) //Get the smic dictionary if it exists
            {
                byte[] data = null;
                data = EnsureFileData(file, data);

                if (data != null)
                {
                    string text = ConvertToText(file, data, out string name);
                    string[] result = text.Split(new[] { '\r', '\n' });

                    foreach (string s in result)
                    {
                        if (string.IsNullOrEmpty(s) || s.Contains(':'))
                            continue;

                        string child = s.Substring(s.StartsWith("$") ? 1 : 0, s.IndexOf(" ") - 1).ToLowerInvariant();
                        if (!entry.Name.Contains(child))
                            continue;

                        string smic = s[s.LastIndexOf("smic_")..].ToLowerInvariant();
                        dfm.StreamEntries[Rpf6FileExt.wtd].TryGetValue(JenkHash.GenHash(smic), out Rpf6FileEntry smicFile);

                        if (smicFile != null)
                        {
                            var r = LoadTexturePack(smicFile);
                            if (r == null)
                                continue;

                            foreach (var tex in r.Textures.Values)
                            {
                                if (tex == null)
                                    continue;
                                if (Textures.Find(item => item.Name.Contains(tex.Name)) == null)
                                    Textures.Add((Rsc6Texture)tex);
                            }
                        }
                    }
                }
            }

            if (Textures.Count == 0) //Else try to find a texture dictionary using just the model name
            {
                string desiredWtd = entry.Name.Remove(entry.Name.LastIndexOf(".")).ToLower();
                var wtdFiles = dfm.StreamEntries[Rpf6FileExt.wtd];
                foreach (var wtd in wtdFiles)
                {
                    if (!wtd.Value.NameLower.StartsWith(desiredWtd))
                        continue;

                    var r = LoadTexturePack(wtd.Value);
                    if (r == null)
                        continue;

                    foreach (var tex in r.Textures.Values)
                    {
                        if (tex == null)
                            continue;
                        Textures.Add((Rsc6Texture)tex);
                    }
                }
            }
            return Textures.ToArray();
        }

        public override DataBag2 LoadMetadata(GameArchiveFileInfo file, byte[] data = null)
        {
            data = EnsureFileData(file, data);

            if (data == null)
                return null;

            if (file is Rpf6ResourceFileEntry)
            {
                var id = BufferUtil.ReadUint(data, 0);
                switch (id)
                {
                    default:
                        switch (Path.GetExtension(file.Name).ToLowerInvariant())
                        {
                            case ".xml":
                            case ".meta":
                                return DataBag2.FromXml(TextUtil.GetUTF8Text(data));
                        }
                        break; //unknown?
                    case 0x30464252: //RBF0 
                        var rbf = new RbfFile();
                        rbf.Load(data);
                        return rbf.Bag;
                }
            }
            return null;
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
            using (var ms = new MemoryStream(cmpbuf))
            {
                var br = new BinaryReader(ms);
                var strtimet = br.ReadInt64();

                if (strtimet != strtime)
                {
                    StartupCacheDirty = true; //strings file mismatch, rebuild the startup cache.
                    return;
                }

                StartupCache = new Dictionary<string, GameArchive>();
                var rootcount = br.ReadInt32();
                var roots = new List<GameArchive>();
                for (int i = 0; i < rootcount; i++)
                {
                    var apath = br.ReadStringNullTerminated();
                    var atime = DateTime.FromBinary(br.ReadInt64());
                    var atimetest = File.GetLastWriteTime(apath);

                    var relpath = apath.Replace(Folder + "\\", "");

                    Core.Engine.Console.Write("Rpf6FileManager", Game.GamePathPrefix + relpath);

                    try
                    {
                        var root = new Rpf6File(apath, relpath);
                        root.ReadStartupCache(br);
                        roots.Add(root);

                        if (atime != atimetest)
                            StartupCacheDirty = true;  //don't cache this file since the times don't match
                        else
                            StartupCache[apath] = root;
                    }
                    catch
                    {
                        StartupCacheDirty = true; //some error while loading the cache for this file, abort!
                        return;
                    }
                }
                DataFileMgr = new Rpf6DataFileMgr(this);
                DataFileMgr.ReadStartupCache(br);
            }
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
            Core.Engine.Console.Write("Rpf6FileManager", "Building RDR1 startup cache...");

            using (var ms = new MemoryStream())
            {
                var bw = new BinaryWriter(ms);
                bw.Write(strtime);
                bw.Write(RootArchives.Count);

                foreach (Rpf6File root in RootArchives)
                {
                    var apath = root.FilePath;
                    var atime = File.GetLastWriteTime(apath);
                    bw.WriteStringNullTerminated(apath);
                    bw.Write(atime.ToBinary());
                    root.WriteStartupCache(bw);
                }
                DataFileMgr.WriteStartupCache(bw);

                var buf = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(buf, 0, buf.Length);

                File.WriteAllBytes(file, buf);
            }
        }

        private void TestAnimations()
        {
            var errorfiles = new List<Rpf6FileEntry>();
            var cnt = 0;
            bool testycd = true;
            var savetest = false;

            foreach (var archive in AllArchives)
            {
                var apl = archive.Path.ToLowerInvariant();
                if (archive.AllEntries != null)
                {
                    foreach (var entry in archive.AllEntries)
                    {
                        if (entry is Rpf6FileEntry fe)
                        {
                            //try
                            {
                                var n = fe.NameLower;
                                if (testycd && n.EndsWith(".wcdt"))
                                {
                                    Core.Engine.Console.Write("TestAnimations", fe.Path);
                                    var data = EnsureFileData(fe, null);
                                    if (data != null)
                                    {
                                        var ycd = new WcdtFile(fe);
                                        ycd.Load(data);
                                        if (savetest)
                                        {
                                            /*var data2 = ycd.Save();
                                            var fe2 = CreateFileEntry(fe.Name, fe.Path, ref data2);
                                            var ycd2 = new WedtFile(fe);
                                            ycd2.Load(data2);*/
                                        }
                                    }
                                    cnt++;
                                }
                            }
                            //catch
                            //{
                            //    errorfiles.Add(fe);
                            //}
                        }
                    }
                }
            }
            if (cnt != 0)
            { }
            if (errorfiles.Count > 0)
            { }
        }
    }

    public class Rpf6DataFileMgr
    {
        public Rpf6FileManager FileManager;
        public Dictionary<Rpf6FileExt, Dictionary<JenkHash, Rpf6FileEntry>> StreamEntries;
        public Dictionary<JenkHash, WsiFile> WsiFiles;
        public Dictionary<JenkHash, WspFile> WspFiles;

        public Rpf6DataFileMgr(Rpf6FileManager fman)
        {
            FileManager = fman;
        }

        public void ReadStartupCache(BinaryReader br)
        {

        }

        public void WriteStartupCache(BinaryWriter bw)
        {
            
        }

        public void Init()
        {
            if (StreamEntries != null)
                return;

            StreamEntries = new Dictionary<Rpf6FileExt, Dictionary<JenkHash, Rpf6FileEntry>>();
            WsiFiles = new Dictionary<JenkHash, WsiFile>();
            WspFiles = new Dictionary<JenkHash, WspFile>();

            LoadFiles();
            LoadSectorInfoResources();
            LoadSectorTrees();
        }

        private void LoadFiles()
        {
            var lowLod = RDR1Map.UseLowestLODSetting.GetBool();
            foreach (var archive in FileManager.AllArchives)
            {
                foreach (var file in archive.AllEntries)
                {
                    if (file is not Rpf6FileEntry fe) continue;
                    if (lowLod && fe.Parent.Name == "resource_0") continue;
                    if (!lowLod && fe.Parent.Name == "resource_1") continue;

                    if (fe.FlagInfos.IsResource)
                    {
                        if (!StreamEntries.TryGetValue(fe.ResourceType, out var entries))
                        {
                            entries = new Dictionary<JenkHash, Rpf6FileEntry>();
                            StreamEntries[fe.ResourceType] = entries;
                        }
                        entries[fe.ShortNameHash] = fe;
                    }
                    else if (file.Name.EndsWith(".txt"))
                    {
                        if (!StreamEntries.TryGetValue(Rpf6FileExt.binary, out var entries))
                        {
                            entries = new Dictionary<JenkHash, Rpf6FileEntry>();
                            StreamEntries[Rpf6FileExt.binary] = entries;
                        }
                        entries[fe.ShortNameHash] = fe;
                    }
                }
            }
        }

        private void LoadSectorInfoResources()
        {
            foreach (var se in StreamEntries[Rpf6FileExt.wsi])
            {
                var fe = se.Value;
                var wsidata = fe.Archive.ExtractFile(fe);

                if (wsidata != null)
                {
                    var wsi = new WsiFile(fe);
                    wsi.Load(wsidata);

                    var hash = fe.ShortNameHash;
                    JenkIndex.Ensure(wsi.Name, "RDR1");
                    WsiFiles[hash] = wsi;
                }
            }
        }

        private void LoadSectorTrees()
        {
            foreach (var se in StreamEntries[Rpf6FileExt.wsp])
            {
                var fe = se.Value;
                var wspdata = fe.Archive.ExtractFile(fe);

                if (wspdata != null)
                {
                    var wsp = new WspFile(fe);
                    wsp.Load(wspdata);

                    var hash = fe.ShortNameHash;
                    JenkIndex.Ensure(wsp.Name, "RDR1");
                    WspFiles[hash] = wsp;
                }
            }
        }

        public Rpf6FileEntry TryGetStreamEntry(JenkHash hash, Rpf6FileExt ext)
        {
            if (StreamEntries.TryGetValue(ext, out var entries))
            {
                if (entries.TryGetValue(hash, out var entry))
                {
                    return entry;
                }
            }
            return null;
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
            DataFileMgr = dfm;
            FileManager = dfm.FileManager;
            Name = name;
            PhysicalPath = FileManager.Folder + "\\" + path;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}