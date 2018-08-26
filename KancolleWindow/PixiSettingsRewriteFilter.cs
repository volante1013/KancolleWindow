using System;
using System.IO;
using System.Text;

using CefSharp;

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

	public class KcsapiResponseFilter : IResponseFilter
	{
		public string jsonStr { get; private set; } = "";
		//public List<string> jsonList = new List<string>();

		public FilterStatus Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
		{
			if(dataIn == null)
			{
				dataInRead = 0;
				dataOutWritten = 0;

				return FilterStatus.Done;
			}

			FilterStatus status = FilterStatus.Done;

			int count = 0;
			if (dataIn.Length <= dataOut.Length)
			{
				dataInRead = dataIn.Length;
				dataOutWritten = dataIn.Length;
				dataIn.CopyTo(dataOut);

				count = (int)dataIn.Length;
			}
			else
			{
				// 参考 => https://github.com/cefsharp/CefSharp/issues/2066
				var data = new byte[dataOut.Length];
				dataIn.Seek(0, SeekOrigin.Begin);
				dataIn.Read(data, 0, data.Length);
				dataOut.Write(data, 0, data.Length);

				dataInRead = dataOut.Length;
				dataOutWritten = dataOut.Length;

				status = FilterStatus.NeedMoreData;

				count = (int)dataOut.Length;
			}

			// 読み取り
			dataIn.Seek(0, SeekOrigin.Begin);
			using (var reader = new StreamReader(dataIn))
			{
				char[] c = new char[dataOut.Length];
				reader.Read(c, 0, count);
				string text = new string(c).Replace("\0", "");
				jsonStr += (text.StartsWith("svdata=")) ? text.Substring(7) : text;
				//jsonList.Add(text);
			}

			return status;
		}

		public bool InitFilter() => true;

		public void Dispose(){}
	}
}
