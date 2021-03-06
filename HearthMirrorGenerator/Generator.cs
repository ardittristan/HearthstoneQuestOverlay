using System;
using System.IO;
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
        public static readonly string HSDTPath =
            Directory.GetDirectories(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "HearthstoneDeckTracker"), "app-*")[
                Directory.GetDirectories(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "HearthstoneDeckTracker"), "app-*").Length - 1];

        public void Execute(GeneratorExecutionContext context)
        {
            CSharpDecompiler decompiler = new CSharpDecompiler(Path.Combine(HSDTPath, "HearthMirror.dll"),
                new DecompilerSettings
                {
                    UsingDeclarations = false,
                    NullableReferenceTypes = true
                });

            ExecuteReflection(context, decompiler);
            ExecuteMirror(context, decompiler);
        }

        private static void ExecuteMirror(GeneratorExecutionContext context, CSharpDecompiler decompiler)
        {
            FullTypeName mirrorClassName = new FullTypeName("HearthMirror.Mirror");

            ITypeDefinition typeInfo =
                decompiler.TypeSystem.MainModule.Compilation.FindType(mirrorClassName).GetDefinition();

            if (typeInfo == null) return;

            foreach (IProperty property in typeInfo.Properties)
            {
                switch (property.Name)
                {
                    case "Root":
                        AddMirrorSource(context, "BgsClient",
                            decompiler.DecompileAsString(property.MetadataToken)
                                .Replace("Root", "BgsClient")
                                .RegexReplace("_root([^_])", "_bgsClient$1")
                                .Replace("Assembly-CSharp", "blizzard.bgsclient"));
                        break;
                }
            }
        }

        private static void ExecuteReflection(GeneratorExecutionContext context, CSharpDecompiler decompiler)
        {
            FullTypeName reflectionClassName = new FullTypeName("HearthMirror.Reflection");

            ITypeDefinition typeInfo = decompiler.TypeSystem.MainModule.Compilation.FindType(reflectionClassName)
                .GetDefinition();

            if (typeInfo == null) return;

            foreach (IMethod method in typeInfo.Methods)
            {
                switch (method.Name)
                {
                    case "TryGetInternal":
                        AddReflectionSource(context, method.Name, decompiler.DecompileAsString(method.MetadataToken)
                            .Replace("HearthMirror.Reflection.", ""));
                        break;
                    case "GetService":
                    case "Reinitialize":
                    case "GetLocalization":
                        AddReflectionSource(context, method.Name,
                            decompiler.DecompileAsString(method.MetadataToken));
                        break;
                }
            }
        }

        private static void AddMirrorSource(GeneratorExecutionContext context, string name, string code)
        {
            context.AddSource($"HearthMirror.Mirror.{name}.cs", SourceText.From($@"
#nullable disable

namespace HSReflection
{{
internal partial class CustomMirror
{{
{code}
}}
}}

#nullable restore
", Encoding.UTF8));
        }

        private static void AddReflectionSource(GeneratorExecutionContext context, string name, string code)
        {
            context.AddSource($"HearthMirror.Reflection.{name}.cs", SourceText.From($@"
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
            //            if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();
            //#endif 
        }
    }
}
