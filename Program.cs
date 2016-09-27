// ***********************************************************************
// Assembly         : dotnet-csres
// Author           : bruno
// Created          : 09-22-2016
//
// Last Modified By : Bruno Lauze
// Last Modified On : 09-22-2016
// ***********************************************************************
// <copyright file="Program.cs" company="">
//     Copyright Bruno Lauze(c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.DotNet.Tools.Resgen
{
    /// <summary>
    /// Class Program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            var template = GetTemplate();
            if (args.Length == 0) return;
            var vals = args[0].Split(',');
            if (vals.Length != 3) return;
            var sourceFile = new FileInfo(vals[0]);
            var outputFile = new FileInfo(vals[1]);
            var nameSpace = vals[2];
            var keys = new HashSet<string>();
            using (var input = sourceFile.OpenRead())
            {
                var document = XDocument.Load(input);
                var data = document.Root.Elements("data");
                if (data.Any())
                {
                    foreach (var e in data)
                    {
                        var name = e.Attribute("name").Value;
                        keys.Add(name);
                    }
                }
            }

            var sb = new StringBuilder();

            foreach(var key in keys)
            {
                sb.AppendLine("");
                sb.AppendLine($"\tinternal static string {key}");
                sb.AppendLine("\t{");
                sb.AppendLine("\t\tget");
                sb.AppendLine("\t\t{");
                sb.AppendLine($"\t\t\treturn SR.GetResourceString(\"{key}\", null);");
                sb.AppendLine("\t\t}");
                sb.AppendLine("\t}");
                sb.AppendLine("");
            }
            
            File.WriteAllText(outputFile.FullName, template.Replace("%NAMESPACE%", nameSpace)
                .Replace("%RESKEYS%", sb.ToString()));
        }

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string GetTemplate()
        {
            var stream = typeof(Program).GetTypeInfo().Assembly.GetManifestResourceStream("Microsoft.DotNet.Tools.Resgen.SR.txt");
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

    }
}
