using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;

namespace Il2CppDumper
{
    public class ConvertDumper(ILogger logger, IDumpOptions options) : IDisposable
    {

        private Metadata? _metadata;
        private Il2Cpp? _il2Cpp;
        private bool _isInitialized;

       

        public void Initialize(string il2cppPath, string metadataPath)
        {
            _isInitialized = true;
            logger.Info("Initializing metadata...");
            var metadataBytes = File.ReadAllBytes(metadataPath);
            _metadata = new Metadata(new MemoryStream(metadataBytes));
            logger.Info($"Metadata Version: {_metadata.Version}");

            logger.Info("Initializing il2cpp file...");
            var il2cppBytes = File.ReadAllBytes(il2cppPath);
            var il2cppMagic = BitConverter.ToUInt32(il2cppBytes, 0);
            var il2CppMemory = new MemoryStream(il2cppBytes);
            switch (il2cppMagic)
            {
                default:
                    throw new NotSupportedException("ERROR: il2cpp file not supported.");
                case 0x6D736100:
                    var web = new WebAssembly(il2CppMemory);
                    _il2Cpp = web.CreateMemory();
                    break;
                case 0x304F534E:
                    var nso = new NSO(il2CppMemory);
                    _il2Cpp = nso.UnCompress();
                    break;
                case 0x905A4D: //PE
                    _il2Cpp = new PE(il2CppMemory);
                    break;
                case 0x464c457f: //ELF
                    if (il2cppBytes[4] == 2) //ELF64
                    {
                        _il2Cpp = new Elf64(il2CppMemory);
                    }
                    else
                    {
                        _il2Cpp = new Elf(il2CppMemory);
                    }
                    break;
                case 0xCAFEBABE: //FAT Mach-O
                case 0xBEBAFECA:
                    var machofat = new MachoFat(new MemoryStream(il2cppBytes));
                    var index = 0;
                    for (int i = 0; i < machofat.fats.Length; i++)
                    {
                        if (RuntimeInformation.ProcessArchitecture == Architecture.X86
                            && machofat.fats[i].magic != 0xFEEDFACF)
                        {
                            index = i;
                            break;
                        }
                        if (machofat.fats[i].magic == 0xFEEDFACF)
                        {
                            index = i;
                        }
                    }
                    var magic = machofat.fats[index % 2].magic;
                    il2cppBytes = machofat.GetMacho(index % 2);
                    il2CppMemory = new MemoryStream(il2cppBytes);
                    if (magic == 0xFEEDFACF)
                    {
                        goto case 0xFEEDFACF;
                    }
                    else
                    {
                        goto case 0xFEEDFACE;
                    }
                case 0xFEEDFACF: // 64bit Mach-O
                    _il2Cpp = new Macho64(il2CppMemory);
                    break;
                case 0xFEEDFACE: // 32bit Mach-O
                    _il2Cpp = new Macho(il2CppMemory);
                    break;
            }
            var version = options.ForceIl2CppVersion ? options.ForceVersion : _metadata.Version;
            _il2Cpp.SetProperties(version, _metadata.metadataUsagesCount);
            logger.Info($"Il2Cpp Version: {_il2Cpp.Version}");
            if (options.ForceDump || _il2Cpp.CheckDump())
            {
                if (_il2Cpp is ElfBase elf)
                {
                    _isInitialized = false;
                    logger.Warning("Detected this may be a dump file.");
                    //logger.Info("Detected this may be a dump file.");
                    //logger.Info("Input il2cpp dump address or input 0 to force continue:");
                    //var DumpAddr = Convert.ToUInt64(Console.ReadLine(), 16);
                    //if (DumpAddr != 0)
                    //{
                    //    _il2Cpp.ImageBase = DumpAddr;
                    //    _il2Cpp.IsDumped = true;
                    //    if (!options.NoRedirectedPointer)
                    //    {
                    //        elf.Reload();
                    //    }
                    //}
                }
                else
                {
                    _il2Cpp.IsDumped = true;
                }
            }

            logger.Info("Searching...");
            try
            {
                var flag = _il2Cpp.PlusSearch(_metadata.methodDefs.Count(x => x.methodIndex >= 0), _metadata.typeDefs.Length, _metadata.imageDefs.Length);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (!flag && _il2Cpp is PE)
                    {
                        logger.Info("Use custom PE loader");
                        _il2Cpp = PELoader.Load(il2cppPath);
                        _il2Cpp.SetProperties(version, _metadata.metadataUsagesCount);
                        flag = _il2Cpp.PlusSearch(_metadata.methodDefs.Count(x => x.methodIndex >= 0), _metadata.typeDefs.Length, _metadata.imageDefs.Length);
                    }
                }
                if (!flag)
                {
                    flag = _il2Cpp.Search();
                }
                if (!flag)
                {
                    flag = _il2Cpp.SymbolSearch();
                }
                if (!flag)
                {
                    _isInitialized = false;
                    logger.Error("ERROR: Can't use auto mode to process file, try manual mode.");
                    //Console.Write("Input CodeRegistration: ");
                    //var codeRegistration = Convert.ToUInt64(Console.ReadLine(), 16);
                    //Console.Write("Input MetadataRegistration: ");
                    //var metadataRegistration = Convert.ToUInt64(Console.ReadLine(), 16);
                    //_il2Cpp.Init(codeRegistration, metadataRegistration);
                }
                if (_il2Cpp.Version >= 27 && _il2Cpp.IsDumped)
                {
                    var typeDef = _metadata.typeDefs[0];
                    var il2CppType = _il2Cpp.types[typeDef.byvalTypeIndex];
                    _metadata.ImageBase = il2CppType.data.typeHandle - _metadata.header.typeDefinitionsOffset;
                }
            }
            catch (Exception e)
            {
                _isInitialized = false;
                logger.Log(e);
            }
        }

        public void SaveAs(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            if (!_isInitialized)
            {
                return;
            }
            logger.Info("Dumping...");
            var executor = new Il2CppExecutor(_metadata, _il2Cpp);
            var decompiler = new Il2CppDecompiler(executor);
            decompiler.Decompile(options, folder);
            logger.Info("Done!");
            if (options.GenerateStruct)
            {
                logger.Info("Generate struct...");
                var scriptGenerator = new StructGenerator(executor);
                scriptGenerator.WriteScript(folder);
                logger.Info("Done!");
            }
            if (options.GenerateDummyDll)
            {
                logger.Info("Generate dummy dll...");
                DummyAssemblyExporter.Export(executor, folder, options.DummyDllAddToken);
                logger.Info("Done!");
            }
        }

        public void Dispose()
        {
            _il2Cpp?.Dispose();
            _metadata?.Dispose();
        }
    }
}
