using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using CefSharp;
using CefSharp.Wpf;
using KancolleWindow.Properties;

namespace KancolleWindow
{
	public static class Screenshot
	{
		public static void TaskScreenshot(ChromiumWebBrowser browser)
		{
			IFrame canvas;
			if(!browser.TryGetKancolleCanvas(out canvas))
			{
				Debug.WriteLine("艦これのcanvas要素が見つかりません");
				return;
			}

			var request = new ScreenshotRequest();
			var script = $@"
(async function()
{{
	await CefSharp.BindObjectAsync('{request.Id}');
	var canvas = document.querySelector('canvas');
	requestAnimationFrame(() =>
	{{
		var dataUrl = canvas.toDataURL('image/png');
		{request.Id}.complete(dataUrl);
	}});
}})();
";
			browser.JavascriptObjectRepository.Register(request.Id, request, true);
			canvas.ExecuteJavaScriptAsync(script);
		}

		public static void SaveScreenshot(string dataUrl)
		{
			var image = DataUrlToImage(dataUrl);
			var filePath = Path.Combine(Settings.Default.ScreenshotSavePath, $"{DateTimeOffset.Now.LocalDateTime.ToString("yyyyMMdd-HHmmssff")}.png");
			image.Save(filePath, ImageFormat.Png);
		}

		private static bool TryGetKancolleCanvas(this ChromiumWebBrowser webBrowser, out IFrame canvas)
		{
			var browser = webBrowser.GetBrowser();
			var gameFrame = browser.GetFrame("game_frame");
			canvas = browser.GetFrameIdentifiers()
				.Select(f => browser.GetFrame(f))
				.Where(f => f.Parent?.Identifier == gameFrame.Identifier)
				.FirstOrDefault(f => f.Url.Contains("/kcs2/index.php"));

			return canvas != null;
		}

		private static Image DataUrlToImage(string dataUrl)
		{
			var array = dataUrl.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 2) throw new Exception($"無効な形式です: {dataUrl}");

			var bytes = Convert.FromBase64String(array[1]); // array[1] = base64
			using(var ms = new MemoryStream(bytes))
			{
				return Image.FromStream(ms);
			}
		}
	}
}
