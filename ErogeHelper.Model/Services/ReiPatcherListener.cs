using System.Net;
using System.Reactive.Linq;

namespace ErogeHelper.Model.Services;

public static class ReiPatcherListener
{
    public static void Start()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:40298/");
        try
        {
            listener.Start();
        } 
        catch (HttpListenerException ex)
        {
            Splat.LogHost.Default.Debug(ex.Message);
            // Toast listening failed
            return;
        }

        while (true)
        {
            var context = listener.GetContext();
            
            HttpListenerRequest request = context.Request;

            var responseBody = "NoContent";
            if (request.HttpMethod == "GET" && request.QueryString.AllKeys.Contains("text"))
            {
                var text = request.QueryString["text"]!;
                Splat.LogHost.Default.Debug(text);
                responseBody = text;
            }

            SetResponse(context.Response, responseBody);
        }
    }

    private static void SetResponse(HttpListenerResponse response, string body)
    {
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = "text/html";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(body);
        response.ContentLength64 = buffer.Length;
        using var output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
    }
}
