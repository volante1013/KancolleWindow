using System.Collections.Generic;
using System.Diagnostics;

using CefSharp;
using CefSharp.Handler;

namespace KancolleWindow
{
	public class HttpResourceHandler : DefaultRequestHandler
	{
		private Dictionary<ulong, KcsapiResponseFilter> responseDic = new Dictionary<ulong, KcsapiResponseFilter>();

		public override IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
		{
			//if (request.Url.Contains("/kcs2/index.php"))
			//{
			//	return new PixiSettingsRewriteFilter();
			//}

			if (request.Url.Contains("/kcsapi/") && response.MimeType == "text/plain")
			{
				var filter = new KcsapiResponseFilter();
				responseDic.Add(request.Identifier, filter);
				return filter;
			}

			return base.GetResourceResponseFilter(browserControl, browser, frame, request, response);
		}

		public override bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
		{
			if (!request.Url.Contains("/kcsapi/") || response.MimeType != "text/plain")
				return false;

			//Debug.WriteLine($"response [MimeType:{response.MimeType} StatusCode:{response.StatusCode} StatusText:{response.StatusText} ErrorCode:{response.ErrorCode.ToString()}");
			//Debug.WriteLine($"response.ResponseHeaders : ");
			//foreach (string key in response.ResponseHeaders)
			//{
			//	Debug.WriteLine($"[key,value]=[{key},{response.ResponseHeaders[key]}]");
			//}

			return false;
		}

		public override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
		{
			return CefReturnValue.Continue;
		}

		public override void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
		{
			if (!request.Url.Contains("/kcsapi/") || response.MimeType != "text/plain")
				return;

			Debug.WriteLine($"response [MimeType:{response.MimeType} StatusCode:{response.StatusCode} StatusText:{response.StatusText} ErrorCode:{response.ErrorCode.ToString()}");
			KcsapiResponseFilter filter;
			if(responseDic.TryGetValue(request.Identifier, out filter))
			{
				Debug.WriteLine("\n" + filter.jsonStr);
				var json = Utf8Json.JsonSerializer.Deserialize<dynamic>(filter.jsonStr);
			}
		}
	}
}
