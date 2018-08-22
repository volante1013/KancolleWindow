using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KancolleWindow
{
	public class PixiSettingsRewriteFilter : IResponseFilter
	{
		public void Dispose()
		{
		}

		public FilterStatus Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
		{
			if(dataIn == null)
			{
				dataInRead = 0;
				dataOutWritten = 0;

				return FilterStatus.Done;
			}

			using(var reader = new StreamReader(dataIn))
			{
				// WebGLのcanvasでtoDataURL()をすると真っ黒の画像になる
				// (ダブルバッファを使っていてクリアのタイミングで取得しているため)
				// なので、preserveDrawingBuffer=trueにしてクリアしないようにする
				var text = reader.ReadToEnd().Replace("pixi.min.js\"></script>", "pixi.min.js\"></script><script>PIXI.settings.RENDER_OPTIONS.preserveDrawingBuffer = true;</script>");
				var buff = Encoding.UTF8.GetBytes(text);
				dataOut.Write(buff, 0, buff.Length);

				dataInRead = dataIn.Length;
				dataOutWritten = Math.Min(buff.Length, dataOut.Length);
			}

			return FilterStatus.Done;
		}

		public bool InitFilter() => true;
	}
}
