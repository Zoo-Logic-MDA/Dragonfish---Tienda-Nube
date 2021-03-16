using Dragonfish_TN;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal static class Order
	{
		public static IRestResponse Response(Method method, int page, int per_page, string dateCreated, string dateUpdated)
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				RestClient restClient = new RestClient(string.Concat("https://api.tiendanube.com/v1/", instance.clienteTiendaNube, "/orders/"));
				restClient.Timeout = -1;
				RestRequest restRequest = new RestRequest(method);
				restRequest.AddParameter("per_page", per_page);
				restRequest.AddParameter("page", page);
				restRequest.AddParameter("created_at_min", dateCreated);
				restRequest.AddParameter("updated_at_min", dateUpdated);
				restRequest.AddHeader("User-Agent", " Dragon (brunodecorneliis@gmail.com)");
				restRequest.AddHeader("Content-Type", "application/json");
				restRequest.AddHeader("Authentication", instance.tokenTiendaNube);
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