using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using CefSharp;
using CefSharp.Wpf;
using KancolleWindow.Properties;

namespace KancolleWindow
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		// 周りの表示とDMMのダイアログを非表示にする
		private static readonly string mainFrameUserStyleSheet =
			$"var style = document.createElement('style'); style.innerHTML = \"{Settings.Default.UserStyleSheet.Replace("\r\n", " ")}\"; document.body.appendChild(style); "
			+"var script = document.createElement('script'); script.innerHTML = \"try { if (DMM.netgame.reloadDialog) DMM.netgame.reloadDialog = function(){}; } catch(e) { alert('DMMによるページ更新ダイアログの非表示に失敗しました: '+e); }\"; document.body.appendChild(script);";

		// 作戦要綱等のUIを消す
		private static readonly string gameFrameUserStyleSheet = 
			"var style = document.createElement('style'); style.innerHTML = \"#globalNavi{ display:none!important; }\"; document.body.appendChild(style);";

		private static readonly Size KancolleSize = new Size(1200, 720);

		private ChromiumWebBrowser cefBrowser;
		private VolumeManager volumeManager;

		private bool IsMute = false;


		public MainWindow()
		{
			CefInitialize();

			InitializeComponent();

			RestoreSettings();

			LoadIconResource();

			this.Closing += (_, __) =>
			{
				SaveSettings();
				Cef.Shutdown();
			};

			volumeManager = new VolumeManager((uint)Process.GetCurrentProcess().Id);

			cefBrowser = new ChromiumWebBrowser
			{
				Address = Settings.Default.KancolleUrl,
				RequestHandler = new HttpResourceHandler(),
				MenuHandler = new ContextMenuHandler()
			};
			cefBrowser.FrameLoadEnd += (sender, args) =>
			{
				if (args.Frame?.IsMain == true && args.Url == Settings.Default.KancolleUrl) 
				{
					args.Frame.ExecuteJavaScriptAsync(mainFrameUserStyleSheet);
					this.Dispatcher.Invoke(() => SetZoomLevel(null, null));
				}
				else if (args.Frame?.Name == "game_frame")
				{
					args.Frame.ExecuteJavaScriptAsync(gameFrameUserStyleSheet);
				}
			};

			this.SizeChanged += SetZoomLevel;

			DockPanel.Children.Add(cefBrowser);
		}

		private void CefInitialize()
		{
			var cefSettings = new CefSettings();
			cefSettings.CefCommandLineArgs.Add("proxy-pac-url", "file://" + Environment.CurrentDirectory + @"\proxy.pac");
			cefSettings.CachePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\CEF";

			// これをtrueにすることでデバッグ終了時にCefSharp.BrowserSubprocess.exeがゾンビ化しなくなる
			// 参考 : https://github.com/cefsharp/CefSharp/issues/2359
			CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

			Cef.Initialize(cefSettings);

			// 必要かわからないが一応
			Cef.EnableHighDPISupport(); 
		}


		#region イベント関連
		private void Mute_Click(object sender, RoutedEventArgs e)
		{
			volumeManager.ToggleMute();
			IsMute = volumeManager.IsMute;

			MuteImg.Source = ResourceManager.Instance.Icons[(IsMute) ? ResourceManager.IconResource.Mute : ResourceManager.IconResource.VolumeHigh];
			(MuteImg.Parent as Button).ToolTip = "ミュート : " + ((volumeManager.IsMute) ? "ON" : "OFF");
		}

		private void TopMost_Click(object sender, RoutedEventArgs e)
		{
			this.Topmost = !this.Topmost;

			TopmostImg.Source = ResourceManager.Instance.Icons[(this.Topmost) ? ResourceManager.IconResource.TopmostON : ResourceManager.IconResource.TopmostOFF];
			(TopmostImg.Parent as Button).ToolTip = "最前面 : " + ((this.Topmost) ? "ON" : "OFF");
		}

		private void ScreenShot_Click(object sender, RoutedEventArgs e) => Screenshot.TaskScreenshot(cefBrowser);

		private void Reload_Click(object sender, RoutedEventArgs e) => cefBrowser.GetBrowser().Reload();

		/// <summary>
		/// ToolBarTrayのOverflowを削除する
		/// </summary>
		private void ToolBar_Loaded(object sender, RoutedEventArgs e)
		{
			ToolBar toolBar = sender as ToolBar;
			var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
			if (overflowGrid != null)
			{
				overflowGrid.Visibility = Visibility.Collapsed;
			}
		}

		#endregion

		#region private関数
		private void RestoreSettings()
		{
			var s = Settings.Default;
			if (s.WindowLeft >= 0 && s.WindowLeft + s.WindowWidth < SystemParameters.VirtualScreenWidth)
				Left = s.WindowLeft;
			if (s.WindowTop >= 0 && s.WindowTop + s.WindowHeight < SystemParameters.VirtualScreenHeight)
				Top = s.WindowTop;
			if (s.WindowWidth > 0 && s.WindowWidth <= SystemParameters.WorkArea.Width)
				Width = s.WindowWidth;
			if (s.WindowHeight > 0 && s.WindowHeight <= SystemParameters.WorkArea.Height)
				Height = s.WindowHeight;
			if (s.WindowMaximized)
				Loaded += (_, __) => WindowState = WindowState.Maximized;

			this.Topmost = s.IsTopmost;
			IsMute = s.IsMute;
		}

		private void SaveSettings()
		{
			var s = Settings.Default;
			s.WindowMaximized = WindowState == WindowState.Maximized;
			s.WindowLeft = Left;
			s.WindowTop = Top;
			s.WindowWidth = Width;
			s.WindowHeight = Height;

			s.IsTopmost = this.Topmost;
			s.IsMute = IsMute;

			s.Save();
		}

		private void SetZoomLevel(object sender, EventArgs e)
		{
			var host = cefBrowser.GetBrowserHost();
			if (host == null || cefBrowser.Address != Settings.Default.KancolleUrl)
				return;

			double zoomRate = Math.Min(cefBrowser.ActualWidth / KancolleSize.Width, cefBrowser.ActualHeight / KancolleSize.Height);
			cefBrowser.ZoomLevel = Math.Log(zoomRate, 1.2);
		}

		private void LoadIconResource()
		{
			ResourceManager.Instance.Load();

			MuteImg.Source = ResourceManager.Instance.Icons[(IsMute) ? ResourceManager.IconResource.Mute : ResourceManager.IconResource.VolumeHigh];
			TopmostImg.Source = ResourceManager.Instance.Icons[(this.Topmost) ? ResourceManager.IconResource.TopmostON : ResourceManager.IconResource.TopmostOFF];
			ReloadImg.Source = ResourceManager.Instance.Icons[ResourceManager.IconResource.Reload];
			ScshoImg.Source = ResourceManager.Instance.Icons[ResourceManager.IconResource.Screenshot];
			SetImg.Source = ResourceManager.Instance.Icons[ResourceManager.IconResource.Settings];
		}
		#endregion
	}
}
