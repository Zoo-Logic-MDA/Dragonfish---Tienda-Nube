using Dragonfish_TN;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal class ObtenerInformacionServicio
	{
		public ObtenerInformacionServicio()
		{
		}

		public static IRestResponse Response()
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				RestClient restClient = new RestClient(string.Concat(instance.urlDragonfish, "/api.Dragonfish/ObtenerInformacionServicio"));
				RestRequest restRequest = new RestRequest(0);
				restRequest.AddHeader("Content-Type", "application/json");
				restResponse = restClient.Execute(restRequest);
			}
			catch (Exception exception)
			{
				throw exception;
			}
			return restResponse;
		}
	}
}