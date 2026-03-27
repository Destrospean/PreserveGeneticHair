using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using s3pi.Filetable;
using s3pi.Interfaces;

namespace Destrospean.HairTroubleGrowthStateAssigner
{
    public enum HairGrowthStates
    {
        Unknown,
        Any,
        Bald,
        Shaved,
        Short,
        ShortOrAbove,
        Medium,
        MediumOrAbove,
        Long,
        LongOrAbove,
        VeryLong
    }

    class MainClass
    {
        static Dictionary<PackageTag, IPackage> sGameContentPackages;

        static object sLock = new object();

        public static Dictionary<PackageTag, IPackage> GameContentPackages
        {
            get
            {
                if (sGameContentPackages == null)
                {
                    lock (sLock)
                    {
                        sGameContentPackages = new Dictionary<PackageTag, IPackage>();
                        foreach (var game in GameFolders.Games)
                        {
                            var enumerator = game.GameContent.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                sGameContentPackages.Add(enumerator.Current, s3pi.Package.Package.OpenPackage(0, enumerator.Current.Path));
                            }
                        }
                    }
                }
                return sGameContentPackages;
            }
        }

        public static void LoadGameFolders()
        {
            var installDirs = "";
            foreach (var line in File.ReadAllLines("InstallDirs.txt"))
            {
                installDirs += ";" + line;
            }
            GameFolders.InstallDirs = installDirs.Substring(1);
        }

        public static void Main(string[] args)
        {
            LoadGameFolders();

            var casPartPackageResourceIndexEntryTuples = new List<Tuple<IPackage, IResourceIndexEntry>>();
            foreach (var gamePackage in GameContentPackages.Values)
            {
                foreach (var resourceIndexEntry in gamePackage.FindAll(x => x.ResourceType == 0x34AEECB))
                {
                    casPartPackageResourceIndexEntryTuples.Add(Tuple.Create(gamePackage, resourceIndexEntry));
                }
            }

            // Get a unique name for the assembly and _XML resource
            var assemblyName = "Destrospean_HT" + System.Security.Cryptography.FNV32.GetHash(Guid.NewGuid().ToString());

            // Load the base package and create a new package to clone to
            IPackage basePackage = s3pi.Package.Package.OpenPackage(0, "Destrospean_HT_Base.package"),
            newPackage = s3pi.Package.Package.NewPackage(0);

            // Get the assembly and XML
            AssemblyDefinition assembly = null;
            var xmlDocument = new System.Xml.XmlDocument();
            foreach (var resourceIndexEntry in basePackage.FindAll(x => x.Instance == 0x268C5DD8E82D5492))
            {
                switch (resourceIndexEntry.ResourceType)
                {
                    case 0x333406C:
                        xmlDocument.Load(((APackage)basePackage).GetResource(resourceIndexEntry));
                        break;
                    case 0x73FAA07:
                        assembly = AssemblyDefinition.ReadAssembly(((ScriptResource.ScriptResource)s3pi.WrapperDealer.WrapperDealer.GetResource(0, basePackage, resourceIndexEntry)).Assembly.BaseStream);
                        break;
                }
            }

            var rootNode = xmlDocument.SelectSingleNode("HairGrowthStateMap");
            var elements = new List<System.Xml.XmlElement>();
            foreach (System.Xml.XmlElement element in rootNode.ChildNodes)
            {
                if (!elements.Exists(x => element.GetAttribute("CASPartKey") == x.GetAttribute("CASPartKey")))
                {
                    elements.Add(element);
                }
            }
            /*
            foreach (var casPartPackageResourceIndexEntryTuple in casPartPackageResourceIndexEntryTuples)
            {
                var casp = (CASPartResource.CASPartResource)s3pi.WrapperDealer.WrapperDealer.GetResource(0, casPartPackageResourceIndexEntryTuple.Item1, casPartPackageResourceIndexEntryTuple.Item2);
                if (casp.Clothing != CASPartResource.ClothingType.Hair)
                {
                    continue;
                }
                var growthState = "Unknown";
                var name = casp.Unknown1.ToLowerInvariant();
                if (name.Contains("hairless") || name.Contains("none"))
                {
                    growthState = "Bald";
                }
                else if (name.Contains("buzzcut") || name.Contains("stubble"))
                {
                    growthState = "Shaved";
                }
                else if (name.Contains("bald") || name.Contains("bowl") || name.Contains("pixie") || name.Contains("short"))
                {
                    growthState = "Short";
                }
                else if (name.Contains("bob") || name.Contains("mid") || name.Contains("med"))
                {
                    growthState = "Medium";
                }
                else if (name.Contains("bun"))
                {
                    growthState = "MediumOrAbove";
                }
                else if (name.Contains("long"))
                {
                    growthState = "Long";
                }
                else if (name.Contains("pony") || name.Contains("updo"))
                {
                    growthState = "LongOrAbove";
                }
                if (name.Contains("longstraightsidepart") || name.Contains("pulledback") || name.Contains("wraplong"))
                {
                    growthState = "VeryLong";
                }
                var element = xmlDocument.CreateElement("Entry");
                element.SetAttribute("CASPartKey", casPartPackageResourceIndexEntryTuple.Item2.ToString());
                element.SetAttribute("Name", name);
                element.SetAttribute("GrowthState", growthState);
                elements.Add(element);
            }
            */
            elements.Sort((a, b) =>
                {
                    var comparison = ((int)Enum.Parse(typeof(HairGrowthStates), a.GetAttribute("GrowthState"))).CompareTo((int)Enum.Parse(typeof(HairGrowthStates), b.GetAttribute("GrowthState")));
                    if (comparison == 0)
                    {
                        return a.GetAttribute("Name").CompareTo(b.GetAttribute("Name"));
                    }
                    return comparison;
                });
            rootNode.RemoveAll();
            foreach (var element in elements)
            {
                rootNode.AppendChild(element);
            }

            // Return early if no assembly is found
            if (assembly == null)
            {
                return;
            }

            // Rename the assembly for the new package
            assembly.Name.Name = assemblyName;
            assembly.MainModule.Name = assemblyName + ".dll";

            Stream assemblyStream = new MemoryStream(),
            xmlStream = new MemoryStream();

            // Save the assembly with the new name
            assembly.Write(assemblyStream);

            // Save the new XML to a stream
            xmlDocument.Save(xmlStream);

            // Add the resources
            var scriptResourceKey = System.Security.Cryptography.FNV64.GetHash(assemblyName);
            var nameMapResource = new NameMapResource.NameMapResource(0, null);
            nameMapResource.Add(scriptResourceKey, assemblyName);
            newPackage.AddResource(new ResourceKey(0x166038C, 0, 0), nameMapResource.Stream, true);
            newPackage.AddResource(new ResourceKey(0x333406C, 0, scriptResourceKey), xmlStream, true);
            newPackage.AddResource(new ResourceKey(0x73FAA07, 0, scriptResourceKey), new ScriptResource.ScriptResource(0, null)
                {
                    Assembly = new BinaryReader(assemblyStream)
                }.Stream, true);

            // Save the new package with the new name
            newPackage.SaveAs(assemblyName + ".package");
        }
    }
}
