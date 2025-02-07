using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DNWS
{
  class StatPlugin : IPlugin
  {
    protected static Dictionary<String, int> statDictionary = null;
    public StatPlugin()
    {
      if (statDictionary == null)
      {
        statDictionary = new Dictionary<String, int>();

      }
    }

    public void PreProcessing(HTTPRequest request)
    {
      if (statDictionary.ContainsKey(request.Url))
      {
        statDictionary[request.Url] = (int)statDictionary[request.Url] + 1;
      }
      else
      {
        statDictionary[request.Url] = 1;
      }
    }
    public HTTPResponse GetResponse(HTTPRequest request)
    {
      HTTPResponse response = null;
      StringBuilder sb = new StringBuilder();
      sb.Append("<html><body><h1>Stat:</h1>");
      foreach (KeyValuePair<String, int> entry in statDictionary)
      {
        sb.Append(entry.Key + ": " + entry.Value.ToString() + "<br />");
      }
      sb.Append("</body></html>");
      response = new HTTPResponse(200);
      response.body = Encoding.UTF8.GetBytes(sb.ToString());
      return response;
    }

    public HTTPResponse PostProcessing(HTTPResponse response)
    {
      throw new NotImplementedException();
    }
  }
  
  class ClientInfoPlugin : IPlugin
  {
    protected static Dictionary<string, string> clientInfoDictionary = null;

    public ClientInfoPlugin()
    {
      if(clientInfoDictionary==null)
      {
        clientInfoDictionary = new Dictionary<string, string>();    
      }
    }

    public void PreProcessing(HTTPRequest request)
    {
      string requestUrl = request.Url ?? "UnknownUrl";

      string clientInfo = "<b>Request URL:</b> " + requestUrl + "<br />";


      if(!clientInfoDictionary.ContainsKey(requestUrl))
      {
        clientInfoDictionary[requestUrl] = clientInfo;
      }

    }

    public HTTPResponse GetResponse(HTTPRequest request)
    {
      HTTPResponse response = null;
      StringBuilder sb = new StringBuilder();
      sb.Append("<html><body><h1>Client Information:</h1>");

      foreach (KeyValuePair<string, string> entry in clientInfoDictionary)
      {
        sb.Append(entry.Value);
      }

        sb.Append("</body></html>");
        response = new HTTPResponse(200);
        response.body = Encoding.UTF8.GetBytes(sb.ToString());
        return response;

    }

    public HTTPResponse PostProcessing(HTTPResponse response)
    {
      throw new NotImplementedException();
    }

  }
  
class ThreadedServer
{
    private readonly HttpListener _listener;
    private readonly List<IPlugin> _plugins;

    public ThreadedServer(string prefix)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
        _plugins = new List<IPlugin> { new StatPlugin(), new ClientInfoPlugin() };
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("Server started...");
        while (true)
        {
            var context = _listener.GetContext();

            // Create a new thread for each incoming request
            Thread requestThread = new Thread(() => HandleRequest(context));
            requestThread.Start();
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        var request = new HTTPRequest(context.Request.Url.AbsoluteUri);

        // Pre-processing through plugins
        foreach (var plugin in _plugins)
        {
            plugin.PreProcessing(request);
        }

        HTTPResponse response = null;
        if (context.Request.HttpMethod == "GET")
        {
            // Handle GET request through plugins
            foreach (var plugin in _plugins)
            {
                if (plugin is StatPlugin || plugin is ClientInfoPlugin)
                {
                    response = plugin.GetResponse(request);
                }
            }
        }

        // Send the response back to the client
        context.Response.StatusCode = response.StatusCode;
        context.Response.OutputStream.Write(response.body, 0, response.body.Length);
        context.Response.OutputStream.Close();

        // Post-processing through plugins
        foreach (var plugin in _plugins)
        {
            plugin.PostProcessing(response);
        }
    }
}
