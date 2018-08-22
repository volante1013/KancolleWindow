using System;

namespace KancolleWindow
{
	public class ScreenshotRequest
	{
		public string Id { get; }
		public ScreenshotRequest()
		{
			Id = $"ssReq{DateTimeOffset.Now.Ticks}";
		}

		public void Complete(string dataUrl) => Screenshot.SaveScreenshot(dataUrl);
	}
}
