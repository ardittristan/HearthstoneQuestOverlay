// ReSharper disable CheckNamespace
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Windows.MainWindowControls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Core = Hearthstone_Deck_Tracker.API.Core;
using Path = System.Windows.Shapes.Path;
using FsPath = System.IO.Path;

internal class Updater
{
    private readonly IUpdater _plugin;
    private string _updateUrl = null!;
    private readonly string _pluginPath;
    private MenuItem _pluginUpdateMenuItem = null!;
    private ProgressDialogController _progressDialog = null!;
    private const string UserAgent = "Hearthstone Deck Tracker plugin update checker";
    private Version? _webVersion;

    internal Updater(IUpdater plugin)
    {
        _plugin = plugin;

        _pluginPath = FsPath.Combine(Config.Instance.ConfigDir, "Plugins",
            FsPath.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location
                    .Split(new[] { "Plugins" }, StringSplitOptions.RemoveEmptyEntries)
                    .Last()
                    .Substring(1)
            ) ?? string.Empty);
    }

    internal void CheckUpdate()
    {
        Release? release = GetReleaseInfo();
        if (release == null)
            return;

        _webVersion = Version.Parse(release.TagName);
        if (_webVersion.CompareTo(_plugin.Version) <= 0)
            return;

        _updateUrl = release.Assets.First(a => a.Name.EndsWith(".zip")).BrowserDownloadUrl;

        AttachToWindow();

        _pluginUpdateMenuItem.Click += (_, _) => { ShowUpdateDialog(); };
    }

    private async Task DoUpdate()
    {
        _progressDialog = await SetupProgressDialog();
        string zipPath = FsPath.Combine(FsPath.GetTempPath(), _plugin.Name + ".zip");

        using (WebClient client = new())
        {
            client.DownloadProgressChanged += DownloadProgressChanged;
            client.DownloadFileCompleted += DownloadFileCompleted;
            client.DownloadFileAsync(new Uri(_updateUrl), zipPath);
            while (client.IsBusy) DoEvents();
        }

        Directory.EnumerateFiles(_pluginPath, "*.dll").AsParallel().ForAll(File.Delete);
        ZipFile.ExtractToDirectory(zipPath, new DirectoryInfo(_pluginPath).Parent!.FullName);
        File.Delete(zipPath);
    }

    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        // ReSharper disable once PossibleLossOfFraction
        double percentage = e.BytesReceived / e.TotalBytesToReceive;

        _progressDialog.SetProgress(percentage);
        _progressDialog.SetMessage(percentage.ToString("P1", new NumberFormatInfo { PercentSymbol = "%" }));
    }

    private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) => _progressDialog.CloseAsync();

    private static async Task<ProgressDialogController> SetupProgressDialog()
    {
        MetroDialogSettings settings = new()
        {
            AnimateHide = false,
            AnimateShow = false
        };

        ProgressDialogController progressDialog =
            await Core.MainWindow.ShowProgressAsync("Downloading Update", "0%", false, settings);
        progressDialog.Minimum = 0;
        progressDialog.Maximum = 1;

        return progressDialog;
    }

    private void AttachToWindow()
    {
        if (LogicalTreeHelper.FindLogicalNode(Core.MainWindow, "MainWindowMenu")
            is not MainWindowMenuView mainWindowMenuView) return;
        Menu mainWindowMenu = (Menu)mainWindowMenuView.Content;

        _pluginUpdateMenuItem = new MenuItem
        {
            Header = _plugin.Name + " Update"
        };

        if (mainWindowMenu.Items.Cast<MenuItem>().All(x => x.Header as string != "UPDATE AVAILABLE"))
        {
            Path menuIcon = new()
            {
                Width = 32,
                Height = 32,
                Stretch = Stretch.Fill,
                Data = Geometry.Parse(
                    "F1 M21,10.12H14.22L16.96,7.3C14.23,4.6 9.81,4.5 7.08,7.2C4.35,9.91 4.35,14.28 7.08,17C9.81,19.7 14.23,19.7 16.96,17C18.32,15.65 19,14.08 19,12.1H21C21,14.08 20.12,16.65 18.36,18.39C14.85,21.87 9.15,21.87 5.64,18.39C2.14,14.92 2.11,9.28 5.62,5.81C9.13,2.34 14.76,2.34 18.27,5.81L21,3V10.12M12.5,8V12.25L16,14.33L15.28,15.54L11,13V8H12.5Z"),
                Fill = ((((mainWindowMenu
                                    .Items
                                    .Cast<MenuItem>()
                                    .First(x => x.Header as string == "HSREPLAY.NET")
                                    .Icon as Rectangle)!
                                .OpacityMask as VisualBrush)!
                            .Visual as Canvas)!
                        .Children[0] as Path)!
                    .Fill
            };
            Canvas.SetLeft(menuIcon, 20);
            Canvas.SetTop(menuIcon, 20);

            MenuItem updateItem = new()
            {
                Header = "UPDATE AVAILABLE",
                Items = { _pluginUpdateMenuItem },
                Icon = new Rectangle
                {
                    Fill = (mainWindowMenu.Items.Cast<MenuItem>()
                        .First(x => x.Header as string == "HSREPLAY.NET").Icon as Rectangle)!.Fill,
                    OpacityMask = new VisualBrush
                    {
                        Visual = new Canvas
                        {
                            Width = 76,
                            Height = 76,
                            Clip = Geometry.Parse("F1 M 0,0L 76,0L 76,76L 0,76L 0,0"),
                            Children = { menuIcon }
                        }
                    },
                    Height = 16,
                    Width = 16
                }
            };
            mainWindowMenu.Items.Add(updateItem);
        }
        else
        {
            mainWindowMenu.Items.Cast<MenuItem>().First(x => x.Header as string == "UPDATE AVAILABLE").Items
                .Add(_pluginUpdateMenuItem);
        }
    }

    internal async void ShowUpdateDialog()
    {
        MetroDialogSettings metroDialogSettings = new()
        {
            AffirmativeButtonText = "Update",
            NegativeButtonText = "Cancel",
            FirstAuxiliaryButtonText = "View on website",
            AnimateHide = false,
            AnimateShow = false,
            DefaultButtonFocus = MessageDialogResult.Affirmative,
            ColorScheme = MetroDialogColorScheme.Theme,
            DialogResultOnCancel = MessageDialogResult.Negative
        };

        MessageDialogResult result = await Core.MainWindow.ShowMessageAsync("update", GetChangelog(),
            MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, metroDialogSettings);

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (result)
        {
            case MessageDialogResult.Affirmative:
                await DoUpdate();

                MetroDialogSettings restartDialogSettings = new()
                {
                    AffirmativeButtonText = "Restart",
                    NegativeButtonText = "Cancel",
                    AnimateHide = false,
                    AnimateShow = false,
                    DefaultButtonFocus = MessageDialogResult.Affirmative,
                    ColorScheme = MetroDialogColorScheme.Theme,
                    DialogResultOnCancel = MessageDialogResult.Negative
                };

                MessageDialogResult restartResult = await Core.MainWindow.ShowMessageAsync("Notice",
                    "Please restart your Hearthstone Deck Tracker to finish updating!",
                    MessageDialogStyle.AffirmativeAndNegative, restartDialogSettings);

                if (restartResult == MessageDialogResult.Affirmative)
                {
                    string? codeBase = Assembly.GetEntryAssembly()?.CodeBase;
                    if (codeBase != null)
                    {
                        Process.Start(codeBase);
                        Task.Delay(250).Wait();
                        Application.Current.Shutdown();
                    }
                }

                break;
            case MessageDialogResult.FirstAuxiliary:
                Process.Start("https://github.com/" + _plugin.GithubRepo + "/releases/latest");
                break;
        }
    }

    private Release? GetReleaseInfo()
    {
        HttpWebRequest request =
            (HttpWebRequest)WebRequest.Create($"https://api.github.com/repos/{_plugin.GithubRepo}/releases/latest");
        request.UserAgent = UserAgent;
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        using Stream? dataStream = response.GetResponseStream();
        if (dataStream == null) return null;

        using StreamReader reader = new(dataStream);

        string strResponse = reader.ReadToEnd();

        return JsonConvert.DeserializeObject<Release>(strResponse);
    }

    private string GetChangelog()
    {
        string output = string.Empty;
        if (_webVersion == null) return output;
        try
        {
            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create(
                    $"https://github.com/{_plugin.GithubRepo}/compare/{_plugin.Version}...{_webVersion}");
            request.UserAgent = UserAgent;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using Stream? dataStream = response.GetResponseStream();
            if (dataStream == null) return output;

            using StreamReader reader = new(dataStream);

            string strResponse = reader.ReadToEnd();

            static string Escape(string str) => str.Replace("/", @"\/");

            MatchCollection matches = Regex.Matches(strResponse,
                $$"""href="\/{{Escape(_plugin.GithubRepo)}}\/commit\/(?<commit>[a-f0-9]{7})[a-f0-9]+">(?<message>.*?)<\/a>""");

            output = string.Join("\n",
                matches.Cast<Match>().Select(m =>
                    $"[{m.Groups["commit"].Value}] {WebUtility.HtmlDecode(m.Groups["message"].Value)}").ToArray());
        }
        catch
        {
            // ignored
        }

        return output;
    }

    private static void DoEvents()
    {
        DispatcherFrame frame = new();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            new DispatcherOperationCallback(
                f =>
                {
                    (f as DispatcherFrame)!.Continue = false;
                    return null;
                }), frame);
        Dispatcher.PushFrame(frame);
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy),
        MissingMemberHandling = MissingMemberHandling.Ignore)]
    private class Release
    {
        public required string TagName { get; init; }
        [JsonIgnore] public IReadOnlyList<ReleaseAsset> Assets => PAssets.AsReadOnly();

        [JsonProperty(PropertyName = "assets")]
        internal required List<ReleaseAsset> PAssets { get; init; }
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy),
        MissingMemberHandling = MissingMemberHandling.Ignore)]
    private class ReleaseAsset
    {
        public required string Name { get; init; }
        public required string BrowserDownloadUrl { get; init; }
    }

    internal interface IUpdater : IPlugin
    {
        string GithubRepo { get; }
    }
}