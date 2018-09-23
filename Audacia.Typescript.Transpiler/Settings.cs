using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Audacia.Typescript.Transpiler
{
    public class Settings
    {
        [XmlElement("Output")] 
        public OutputSettings[] Outputs { get; set; }

        public IEnumerable<string> Namespaces => Outputs
            .SelectMany(x => x.Inputs)
            .Where(x => x.Namespaces != null)
            .SelectMany(i => i.Namespaces)
            .Select(n => n.Name);

        private static readonly XmlSerializer Xml = new XmlSerializer(typeof(Settings));

        public static Settings Load(string path)
        {
            if (!File.Exists(path))
            {
                using (TextWriter writer = new StreamWriter(path))
                    Xml.Serialize(writer, Default);

                throw new InvalidDataException("Failed to find a config file at: " 
                    + path + ". A template config file has been automatically generated for you");
            }

            using (var stream = new FileStream(path, FileMode.Open))
                return (Settings) Xml.Deserialize(stream);
        }

        public static Settings Default => new Settings
        {
            Outputs = new[]
            {
                new OutputSettings("../../example/output/file.ts")
                {
                    Inputs = new[] {"../../path/to/example/assembly.dll"}.Select(x => new InputSettings(x)).ToArray()
                },
                new OutputSettings("../../example/output/file.ts")
                {
                    Inputs = new[] {"../../another/example/assembly.dll"}.Select(x => new InputSettings(x)
                    {
                        Namespaces = new[] {"example", "namespace", "specifications "}
                            .Select(y => new NamespaceSettings(y)).ToArray()
                    }).ToArray()
                }
            }
        };
        
        public class InputSettings
        {
            public InputSettings() { }
        
            public InputSettings(string assembly) => Assembly = assembly;

            [XmlAttribute("name")]
            public string Assembly { get; set; }
		
            [XmlElement("Namespace")]
            public NamespaceSettings[] Namespaces { get; set; } = new NamespaceSettings[0];
        }

        public class NamespaceSettings
        {
            public NamespaceSettings() { }

            public NamespaceSettings(string name) => Name = name;

            [XmlAttribute("name")]
            public string Name { get; set; }
        }
    
        public class OutputSettings
        {
            public OutputSettings() { }
        
            public OutputSettings(string path) => Path = path;

            [XmlAttribute("path")]
            public string Path { get; set; }
        
            [XmlElement("Input")]
            public InputSettings[] Inputs { get; set; } = new InputSettings[0];
        }
    }
}