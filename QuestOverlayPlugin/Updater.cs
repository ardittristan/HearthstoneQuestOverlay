using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Windows.MainWindowControls;
using MahApps.Metro.Controls.Dialogs;
using Core = Hearthstone_Deck_Tracker.API.Core;
using Path = System.Windows.Shapes.Path;
using FsPath = System.IO.Path;

#nullable enable

namespace QuestOverlayPlugin
{
	internal class Updater
	{
		private readonly IUpdater _plugin;
		private string _updateUrl = null!;
		private readonly string _pluginPath;
		private MenuItem _pluginUpdateMenuItem = null!;
		private ProgressDialogController _progressDialog = null!;

		internal Updater(IUpdater plugin)
		{
			_plugin = plugin;

			_pluginPath = FsPath.Combine(Config.Instance.ConfigDir, "Plugins",
				FsPath.GetDirectoryName(Assembly.GetExecutingAssembly().Location
					.Split(new[] { "Plugins" }, StringSplitOptions.RemoveEmptyEntries).Last().Substring(1)) ??
				string.Empty);
		}

		internal void CheckUpdate()
		{
			string webInfo = GetWebInfo();
			if (webInfo.Length == 0) return;

			Version webVersion =
				Version.Parse(new Regex(@"(?<=^ *?<title>.*?)[0-9]+\.[0-9]+\.[0-9]+(\.[0-9]+)?", RegexOptions.Multiline)
					.Match(webInfo).Value);
			if (webVersion.CompareTo(_plugin.Version) <= 0) return;

			_updateUrl = "https://github.com" +
						 new Regex(@"(?<=href="")[^\n\r]*?zip", RegexOptions.Multiline).Match(webInfo).Value;

			AttachToWindow();

			_pluginUpdateMenuItem.Click += (sender, args) =>
			{
				ShowUpdateDialog();
			};
		}

		private async Task DoUpdate()
		{
			_progressDialog = await SetupProgressDialog();
			using (WebClient client = new WebClient())
			{
				client.DownloadProgressChanged += DownloadProgressChanged;
				client.DownloadFileCompleted += DownloadFileCompleted;
				client.DownloadFileAsync(new Uri(_updateUrl),
					FsPath.Combine(FsPath.GetTempPath(), _plugin.Name + ".zip"));
				while (client.IsBusy) DoEvents();
			}
			Directory.EnumerateFiles(_pluginPath, "*.dll").All(f =>
			{
				File.Delete(f);
				return true;
			});
			ZipFile.ExtractToDirectory(FsPath.Combine(FsPath.GetTempPath(), _plugin.Name + ".zip"),
				new DirectoryInfo(_pluginPath).Parent!.FullName);
		}

		private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			double bytesIn = double.Parse(e.BytesReceived.ToString());
			double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
			double percentage = bytesIn / totalBytes;

			_progressDialog.SetProgress(percentage);
			_progressDialog.SetMessage(percentage.ToString("P1", new NumberFormatInfo { PercentSymbol = "%" }));
		}

		private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			_progressDialog.CloseAsync();
		}

		private async Task<ProgressDialogController> SetupProgressDialog()
		{
			MetroDialogSettings settings = new MetroDialogSettings
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
			MainWindowMenuView? mainWindowMenuView =
				(MainWindowMenuView?)LogicalTreeHelper.FindLogicalNode(Core.MainWindow, "MainWindowMenu");
			if (mainWindowMenuView == null) return;
			Menu mainWindowMenu = (Menu)mainWindowMenuView.Content;

			_pluginUpdateMenuItem = new MenuItem
			{
				Header = _plugin.Name + " Update"
			};

			if (mainWindowMenu.Items.Cast<MenuItem>().All(x => (string)x.Header != "UPDATE AVAILABLE"))
			{
				Path menuIcon = new Path
				{
					Width = 32,
					Height = 32,
					Stretch = Stretch.Fill,
					Data = Geometry.Parse(
						"F1 M21,10.12H14.22L16.96,7.3C14.23,4.6 9.81,4.5 7.08,7.2C4.35,9.91 4.35,14.28 7.08,17C9.81,19.7 14.23,19.7 16.96,17C18.32,15.65 19,14.08 19,12.1H21C21,14.08 20.12,16.65 18.36,18.39C14.85,21.87 9.15,21.87 5.64,18.39C2.14,14.92 2.11,9.28 5.62,5.81C9.13,2.34 14.76,2.34 18.27,5.81L21,3V10.12M12.5,8V12.25L16,14.33L15.28,15.54L11,13V8H12.5Z"),
					Fill = ((Path)((Canvas)((VisualBrush)((Rectangle)mainWindowMenu.Items.Cast<MenuItem>()
						.First(x => (string)x.Header == "HSREPLAY.NET").Icon).OpacityMask).Visual).Children[0]).Fill
				};
				Canvas.SetLeft(menuIcon, 20);
				Canvas.SetTop(menuIcon, 20);

				MenuItem updateItem = new MenuItem
				{
					Header = "UPDATE AVAILABLE",
					Items = { _pluginUpdateMenuItem },
					Icon = new Rectangle
					{
						Fill = ((Rectangle)mainWindowMenu.Items.Cast<MenuItem>().First(x => (string)x.Header == "HSREPLAY.NET").Icon).Fill,
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
				mainWindowMenu.Items.Cast<MenuItem>().First(x => (string)x.Header == "Plugin Update Available").Items
					.Add(_pluginUpdateMenuItem);
			}
		}

		internal async void ShowUpdateDialog()
		{
			MetroDialogSettings metroDialogSettings = new MetroDialogSettings
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

			MessageDialogResult result = await Core.MainWindow.ShowMessageAsync("update", "message",
				MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, metroDialogSettings);

			switch (result)
			{
				case MessageDialogResult.Affirmative:
					await DoUpdate();

					MetroDialogSettings restartDialogSettings = new MetroDialogSettings
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

		private string GetWebInfo()
		{
			WebRequest request = WebRequest.Create("https://github.com/" + _plugin.GithubRepo + "/releases/latest");
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			Stream? dataStream = response.GetResponseStream();
			if (dataStream == null) return "";

			StreamReader reader = new StreamReader(dataStream);

			return reader.ReadToEnd();
		}

		private static void DoEvents()
		{
			DispatcherFrame frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
				new DispatcherOperationCallback(
					delegate (object f)
					{
						((DispatcherFrame)f).Continue = false;
						return null;
					}), frame);
			Dispatcher.PushFrame(frame);
		}

		internal interface IUpdater : IPlugin
		{
			string GithubRepo
			{
				get;
			}
		}
	}
}

#nullable restore