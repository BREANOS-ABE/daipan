//-----------------------------------------------------------------------

// <copyright file="ClassDefinition.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Monday, December 3, 2018 3:34:35 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CyPAN.RuntimeAssemblyBuilder
{
    /// <summary>
    /// Represents a class under the context of the CyPAN RuntimeAssemblyBuilder.
    /// </summary>
    public class ClassDefinition
    {
        #region Properties
        /// <summary>
        /// The namespace containing the class
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// The class name
        /// </summary>
        public string ClassName { get; set; }
        /// <summary>
        /// The using lines added prior to the class definition
        /// </summary>
        public List<string> UsingLines { get; private set; }
        /// <summary>
        /// Getter method for the head of the class, consisting of the usings, namespace- and class openings
        /// </summary>
        public string Head
        {
            get
            {
                return $"namespace {Namespace}\n{{\n" +
                 $"\tpublic static class {ClassName}\n\t{{";
            }
        }
        /// <summary>
        /// The methods defined in that class in pure c# source form
        /// </summary>
        public Dictionary<string, string> Methods { get; set; }
        /// <summary>
        /// Locations of assemblies required for building
        /// </summary>
        public List<string> AssemblyLocations { get; set; }
        #endregion
        #region c'tor
        /// <summary>
        /// constructor
        /// </summary>
        public ClassDefinition()
        {
            Methods = new Dictionary<string, string>();
            UsingLines = new List<string>();
            AssemblyLocations = new List<string>();

            AddUsing("System");
            AddUsing("System.Linq");

            AddAssemblyLocation(typeof(object).GetTypeInfo().Assembly.Location);
            AddAssemblyLocation(typeof(System.Linq.Enumerable).Assembly.Location);
            AddAssemblyLocation(Assembly.Load("System.Runtime").Location);
        }
        #endregion
        #region methods
        /// <summary>
        /// Adds a given location to the list of used assemblies if it hasn't already been added
        /// </summary>
        /// <param name="location"></param>
        public void AddAssemblyLocation(string location)
        {
            if (!AssemblyLocations.Contains(location))
                AssemblyLocations.Add(location);
        }
        /// <summary>
        /// transforms a given string "X" into "using X;" and adds it to the list of using lines
        /// </summary>
        /// <param name="usingCode">the using to be added</param>
        public void AddUsing(string usingCode)
        {
            UsingLines.Add($"using {usingCode};");
        }
        /// <summary>
        /// adds/replaces a method definition+body to the method dictionary
        /// </summary>
        /// <param name="isPublic">whether the method is public</param>
        /// <param name="isStatic">whether the method is static</param>
        /// <param name="returnType">return type of the method, e.g. System.Boolean</param>
        /// <param name="methodName">the name of the method by which to call it, e.g. Foo</param>
        /// <param name="methodBody">the body of the method, e.g. return 1;</param>
        /// <param name="parameters">the method's parameters as tuples of the parameter type full name and the parameter name</param>
        public void SetMethod(bool isPublic, bool isStatic, string returnType, string methodName, string methodBody, params (string type,string paramName)[] parameters)
        {
            Methods[methodName] = $@"{(isPublic ? "public " : "")}{(isStatic ? "static " : "")}{returnType} {methodName}({((parameters == null || parameters.Length <= 0) ? ("") : (string.Join(", ", parameters.Select(p => p.type + " " + p.paramName))))})
{{
{methodBody}
}}";
        }
        /// <summary>
        /// override to string method, returns the generated source code of the class
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var codeSb = new StringBuilder();
            foreach (var usingLine in UsingLines)
            {
                codeSb.AppendLine(usingLine);
            }
            codeSb.AppendLine();
            codeSb.AppendLine(Head);
            foreach (var method in Methods.Values)
            {
                var methodLines = method.Split('\n');
                foreach (var methodLine in methodLines)
                {
                    codeSb.AppendLine($"\t\t{methodLine}");
                }
            }
            codeSb.AppendLine("\t}");
            codeSb.AppendLine("}");
            return codeSb.ToString();
        }
        /// <summary>
        /// Attempts compilation of the aggregated source code and will return the compiled assembly's byte array (may be null) and the EmitResult from the compilation
        /// </summary>
        /// <returns>ValueTuple&lt;byte,EmitResult&gt; of the compiled assembly's bytes and the result of the compilation</returns>
        public (byte[] bytes, EmitResult result) CompileToAssembly()
        {
            SyntaxTree stree = CSharpSyntaxTree.ParseText(ToString());
            MetadataReference[] references = AssemblyLocations.Select(aloc => MetadataReference.CreateFromFile(aloc)).ToArray();
            CSharpCompilation compilation = CSharpCompilation.Create(Namespace, syntaxTrees: new[] { stree },
               references: references,
               options:
               new CSharpCompilationOptions
               (OutputKind.DynamicallyLinkedLibrary,
               optimizationLevel: OptimizationLevel.Release, platform: Platform.X64));
            using (var ms = new MemoryStream())
            {

                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    return (null, result);
                }
                else
                {
                    var bytes = ms.GetBuffer();
                    Assembly assembly = Assembly.Load(bytes);
                    return (bytes, result);
                }
            }
        }
        #endregion
    }
}
