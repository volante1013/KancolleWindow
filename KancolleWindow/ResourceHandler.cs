using CefSharp;
using CefSharp.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KancolleWindow
{
	public class ResourceHandler : DefaultRequestHandler
	{
		public override IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
		{
			if (request.Url.Contains("/kcs2/index.php"))
			{
				return new PixiSettingsRewriteFilter();
			}

			return base.GetResourceResponseFilter(browserControl, browser, frame, request, response);
		}
	}
}
