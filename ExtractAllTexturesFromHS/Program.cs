using System.Text.RegularExpressions;
using TextureExtractor;

string productDb = File.ReadAllText(@"C:\ProgramData\Battle.net\Agent\product.db");

string hsPath = Regex.Match(productDb, @"[ -~]+Hearthstone").Value.Replace('/', '\\');

string assetBundleDir = Path.Combine(hsPath, @"Data\Win");

string extractDir = Path.Combine(ThisAssembly.Project.ProjectPath, @"obj\hs");

IEnumerable<string> assetBundles = Directory.EnumerateFiles(assetBundleDir, "*.unity3d");

Extractor extractor = new(extractDir, "1");

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