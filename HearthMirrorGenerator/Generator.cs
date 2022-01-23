using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace HearthMirrorGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        public static readonly string HSDTPath = Directory
            .GetDirectories(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "HearthstoneDeckTracker"), "app-*").First();

        public void Execute(GeneratorExecutionContext context)
        {
            CSharpDecompiler decompiler = new CSharpDecompiler(Path.Combine(HSDTPath, "HearthMirror.dll"),
                new DecompilerSettings
                {
                    UsingDeclarations = false,
                    NullableReferenceTypes = true
                });

            FullTypeName reflectionClassName = new FullTypeName("HearthMirror.Reflection");

            ITypeDefinition typeInfo = decompiler.TypeSystem.MainModule.Compilation.FindType(reflectionClassName)
                .GetDefinition();

            if (typeInfo == null) return;

            foreach (IMethod method in typeInfo.Methods)
            {
                switch (method.Name)
                {
                    case "TryGetInternal":
                        AddSource(context, method.Name, decompiler.DecompileAsString(method.MetadataToken)
                                .Replace("HearthMirror.Reflection.", ""));
                        break;
                    case "GetService":
                        AddSource(context, method.Name, decompiler.DecompileAsString(method.MetadataToken));
                        break;
                    case "Reinitialize":
                        AddSource(context, method.Name, decompiler.DecompileAsString(method.MetadataToken));
                        break;
                }
            }
        }

        private static void AddSource(GeneratorExecutionContext context, string name, string code)
        {
            context.AddSource($"HearthMirror.{name}.cs", SourceText.From($@"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HearthMirror;

#nullable disable

namespace HSReflection
{{
public static partial class Reflection
{{
{code}
}}
}}

#nullable restore
", Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached) Debugger.Launch();
//#endif 
        }
    }
}
