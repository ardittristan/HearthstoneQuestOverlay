using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Fastenshtein;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace UpdaterGenerator
{
	[Generator]
	public class Generator : ISourceGenerator
	{

		public void Execute(GeneratorExecutionContext context)
		{
			Levenshtein lev = new(context.Compilation.AssemblyName);
			string namespaceName = context.Compilation.Assembly.NamespaceNames.Select(x => x).Distinct()
				.ToDictionary(namespaceName => namespaceName, namespaceName => lev.DistanceFrom(namespaceName))
				.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

			context.AddSource("Updater.g.cs",
				SourceText.From(
					GetFromResources("Updater.cs").Replace("{{Namespace}}", namespaceName),
					Encoding.UTF8
				)
			);
		}

		private static string GetFromResources(string resourceName)
		{
			Assembly asm = Assembly.GetExecutingAssembly();

			using Stream stream = asm.GetManifestResourceStream(asm.GetName().Name + '.' + resourceName);
			using var reader = new StreamReader(stream!);
			return reader.ReadToEnd();
		}

		public void Initialize(GeneratorInitializationContext context)
		{
			// if (!System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Launch();
		}
	}
}
