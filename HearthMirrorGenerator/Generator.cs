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
        FullTypeName localReflectionProxyClassName = new("HearthMirror.LocalReflectionProxy`1");

        ITypeDefinition localReflectionProxyTypeInfo =
            decompiler.TypeSystem.MainModule.Compilation.FindType(localReflectionProxyClassName).GetDefinition();

        if (localReflectionProxyTypeInfo == null) return;

        AddLocalReflectionProxySource(context, localReflectionProxyTypeInfo.Name,
            decompiler.DecompileAsString(localReflectionProxyTypeInfo.MetadataToken)
                .Replace("HearthMirror.Reflection", "Reflection"));
    }

    private static void AddLocalReflectionProxySource(GeneratorExecutionContext context, string name, string code)
    {
        context.AddSource($"HearthMirror.{name}.g.cs", SourceText.From($@"
namespace QuestOverlayPlugin.HSReflection
{{
{code}
}}
", Encoding.UTF8));
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        //#if DEBUG
        //            if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();
        //#endif 
    }
}