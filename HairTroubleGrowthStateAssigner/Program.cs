using System;
using System.IO;
using Mono.Cecil;
using s3pi.Interfaces;

namespace Destrospean.HairTroubleGrowthStateAssigner
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // Get unique name for the assembly and _XML resource
            var assemblyName = "Destrospean_HT" + System.Security.Cryptography.FNV32.GetHash(Guid.NewGuid().ToString());

            // Load the base package and create a new package to clone to
            IPackage basePackage = s3pi.Package.Package.OpenPackage(0, "Destrospean_HT_Base.package"),
            newPackage = s3pi.Package.Package.NewPackage(0);

            // Clone to the new package
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
