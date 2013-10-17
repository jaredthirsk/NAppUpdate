﻿using System;
using AppUpdate.Common;
using AppUpdate.Utils;
using System.Net;
using System.IO;
using System.Net.Cache;

namespace AppUpdate.Sources
{
	public class SimpleWebSource : IUpdateSource
	{
		public IWebProxy Proxy { get; set; }
		public string FeedUrl { get; set; }

		public SimpleWebSource()
		{
			Proxy = null;
		}

		public SimpleWebSource(string feedUrl)
		{
			FeedUrl = feedUrl;
			Proxy = null;
		}

		#region IUpdateSource Members

		public string GetUpdatesFeed()
		{
			string data = string.Empty;

			var request = WebRequest.Create(FeedUrl);
            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Pragma", "no-cache");
            request.Headers.Add("Pragma", "no-store");
			request.Method = "GET";
			request.Proxy = Proxy;
			using (var response = request.GetResponse())
			{
				var stream = response.GetResponseStream();

				if (stream != null)
					using (var reader = new StreamReader(stream, true))
					{
						data = reader.ReadToEnd();
					}
			}

			return data;
		}

		public bool GetData(string url, string baseUrl, Action<UpdateProgressInfo> onProgress, ref string tempLocation)
		{
			FileDownloader fd;
			// A baseUrl of http://testserver/somefolder with a file linklibrary.dll was resulting in a webrequest to http://testserver/linklibrary
			// The trailing slash is required for the Uri parser to resolve correctly.
			if (!string.IsNullOrEmpty(baseUrl) && !baseUrl.EndsWith("/")) baseUrl += "/";
			if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
				fd = new FileDownloader(url);
			else if (Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
				fd = new FileDownloader(new Uri(new Uri(baseUrl, UriKind.Absolute), url));
			else
				fd = string.IsNullOrEmpty(baseUrl) ? new FileDownloader(url) : new FileDownloader(new Uri(new Uri(baseUrl), url));

			fd.Proxy = Proxy;

			if (string.IsNullOrEmpty(tempLocation) || !Directory.Exists(Path.GetDirectoryName(tempLocation)))
				// WATCHOUT!!! Files downloaded to a path specified by GetTempFileName may be deleted on
				// application restart, and as such cannot be relied on for cold updates, only for hot-swaps or
				// files requiring pre-processing
				tempLocation = Path.GetTempFileName();

			return fd.DownloadToFile(tempLocation, onProgress);
		}

		#endregion
	}
}
