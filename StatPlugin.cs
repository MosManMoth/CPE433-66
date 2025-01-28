using System;
using System.Collections.Generic;
using System.Text;

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
}
