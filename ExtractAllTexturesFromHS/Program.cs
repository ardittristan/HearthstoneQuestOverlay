using System.Text.RegularExpressions;
using TextureExtractor;

string productDb = File.ReadAllText(@"C:\ProgramData\Battle.net\Agent\product.db");

string hsPath = Regex.Match(productDb, @"[ -~]+Hearthstone").Value.Replace('/', '\\');

string assetBundleDir = Path.Combine(hsPath, @"Data\Win");

string extractDir = Path.Combine(ThisAssembly.Project.ProjectPath, @"obj\hs");

string searchPattern = "*.unity3d";

if (args.Length > 0 && args[0].Length > 0)
    searchPattern = args[0];

IEnumerable<string> assetBundles = Directory.EnumerateFiles(assetBundleDir, searchPattern);

Extractor extractor = new(extractDir, "1");

Console.WriteLine(extractor.FindBundle(assetBundleDir, "initial_base_global-*-texture-*.unity3d", "class_druid-icon", true));

Task[] tasks = assetBundles.Select(bundle => extractor.ExtractAsync(bundle, true)).ToArray();

Task whenAll = Task.WhenAll(tasks);

Console.WriteLine();

while (true)
{
    await Task.WhenAny(tasks);

    Console.Write("\r{0:D}/{1:D} completed", tasks.Count(t => t.IsCompleted), tasks.Length);

    if (whenAll.IsCompleted)
        break;
}

string iconBundle =
    new DirectoryInfo(
            Path.GetDirectoryName(
                Directory.GetFiles(extractDir, "class_druid-icon.png", SearchOption.AllDirectories).FirstOrDefault()) ??
            "")
        .Name;

Console.WriteLine();

Console.WriteLine("Found quest icons in '{0}'", iconBundle);