using System.Text.RegularExpressions;
using TextureExtractor;

string productDb = File.ReadAllText(@"C:\ProgramData\Battle.net\Agent\product.db");

string hsPath = Regex.Match(productDb, @"[ -~]+Hearthstone").Value.Replace('/', '\\');

string assetBundleDir = Path.Combine(hsPath, @"Data\Win");

List<string> assetBundles = Directory.EnumerateFiles(assetBundleDir, "*.unity3d").ToList();

Extractor extractor = new(Path.Combine(ThisAssembly.Project.ProjectPath, @"obj\hs"), "1");

IEnumerable<Task> tasks = assetBundles.Select(bundle => extractor.ExtractAsync(bundle));
await Task.WhenAll(tasks);
