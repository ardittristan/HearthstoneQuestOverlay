using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace HearthMirrorGenerator;

[Generator]
public class Generator : ISourceGenerator
{
    public static readonly string HSDTPath =
        Directory.GetDirectories(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HearthstoneDeckTracker"), "app-*").Last();

    public void Execute(GeneratorExecutionContext context)
    {
        CSharpDecompiler decompiler = new(Path.Combine(HSDTPath, "HearthMirror.dll"),
            new DecompilerSettings
            {
                UsingDeclarations = false,
                NullableReferenceTypes = true
            });

        ExecuteReflection(context, decompiler);
    }

    private static void ExecuteReflection(GeneratorExecutionContext context, CSharpDecompiler decompiler)
    {
        FullTypeName reflectionClassName = new("HearthMirror.Reflection");

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
            }
        }
    }

    private static void AddReflectionSource(GeneratorExecutionContext context, string name, string code)
    {
        context.AddSource($"HearthMirror.Reflection.{name}.g.cs", SourceText.From($@"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HearthMirror;

#nullable disable

namespace HSReflection
{{
public partial class Reflection
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