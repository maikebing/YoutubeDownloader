using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
namespace YoutubeDownloader.Core.Utils;

public static class Http
{
    public static HttpClient Client { get; set; }=CreateClient();

    public static HttpClient CreateClient(HttpMessageHandler? handler = null) =>
        new(handler ?? new SocketsHttpHandler(), true)
        {
            DefaultRequestHeaders =
            {
                // Required by some of the services we're using
                UserAgent =
                {
                    new ProductInfoHeaderValue(
                        "YoutubeDownloader",
                        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
                    )
                }
            }
        };
    public static HttpClient CreateClient(Action<SocketsHttpHandler> _setup)
    {
        var hander = new SocketsHttpHandler();
        var client = CreateClient(hander);
        if (_setup != null) _setup.Invoke(hander);
        return client;
    }

    public static HttpClient CreateClient(bool useProxy, string proxyAddress)
    {
        return CreateClient(hander => hander.WithProxy(useProxy, proxyAddress));
    }

    public static void  WithProxy(this SocketsHttpHandler handler, bool useProxy, string proxyAddress)
    {
        if (useProxy && Uri.TryCreate(proxyAddress, UriKind.RelativeOrAbsolute, out Uri? result))
        {
            handler.Proxy = CreateProxy(true, result);
            handler.UseProxy = true;
        }
        else
        {
            handler.UseProxy = false;
        }
    }
    internal static System.Net.WebProxy? CreateProxy(bool _useproxy, Uri _uri)
    {
        var uri = new UriBuilder(_uri);
        System.Net.WebProxy? proxy = null;
        if (_useproxy)
        {
            var scheme = uri.Scheme.ToLower();
            switch (scheme)
            {
                case "socks4":
                case "socks5":
                    proxy = new System.Net.WebProxy(uri.Host, uri.Port);
                    break;
                case "http":
                case "https":
                default:
                    proxy = new System.Net.WebProxy(uri.ToString());
                    break;
            }
            if (!string.IsNullOrEmpty(uri.UserName) && !string.IsNullOrEmpty(uri.Password))
            {
                proxy.Credentials = new NetworkCredential(uri.UserName, uri.Password);
            }
        }
        return proxy;
    }
}