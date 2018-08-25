using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace KancolleWindow
{
	public class ResourceManager
	{
		public static ResourceManager Instance { get; } = new ResourceManager();

		public Dictionary<IconResource, BitmapImage> Icons { get; private set; } = new Dictionary<IconResource, BitmapImage>();

		private bool isLoaded = false;

		public enum IconResource
		{
			None = -1,
			Screenshot,
			Reload,
			TopmostON,
			TopmostOFF,
			Mute,
			VolumeHigh,
			Settings,
		}

		private ResourceManager()
		{
		}

		public bool Load()
		{
			if (isLoaded) return true;

			try
			{
				LoadFromResource();
				isLoaded = true;
				return true;
			}
			catch (Exception e)
			{
				MessageBox.Show("リソースファイルの読み込みに失敗しました。\r\n" + e.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

				FillWithBlankImage();
			}

			return false;
		}

		private void LoadFromResource()
		{
			LoadImage(@"Resources/camera.png", IconResource.Screenshot);
			LoadImage(@"Resources/arrow_repeat.png", IconResource.Reload);
			LoadImage(@"Resources/pin_fill_sharp_circle.png", IconResource.TopmostON);
			LoadImage(@"Resources/pin_sharp_circle.png", IconResource.TopmostOFF);
			LoadImage(@"Resources/volume_close.png", IconResource.Mute);
			LoadImage(@"Resources/volume_high.png", IconResource.VolumeHigh);
			LoadImage(@"Resources/menu_list.png", IconResource.Settings);
		}

		private void LoadImage(string path, IconResource key)
		{
			try
			{
				var bmp = new BitmapImage(new Uri(path, UriKind.Relative));
				Icons.Add(key, bmp);
			}
			catch (Exception e)
			{
				Debug.WriteLine($"画像リソース {path}の読み込みに失敗しました");
				Debug.WriteLine(e.Message);
				Icons.Add(key, CreateBlankImage());
			}
		}

		private void FillWithBlankImage()
		{
			var names = Enum.GetValues(typeof(IconResource));
			foreach(var name in names)
			{
				Icons.Add((IconResource)name, CreateBlankImage());
			}
		}

		private static BitmapImage CreateBlankImage() => new BitmapImage();
	}
}
