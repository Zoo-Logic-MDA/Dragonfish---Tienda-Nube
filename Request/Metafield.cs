using Dragonfish_TN;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal static class Metafield
	{
		public static IRestResponse Response(string owner_id, string created_at_min, int page)
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				RestClient restClient = new RestClient(string.Concat("https://api.tiendanube.com/v1/", instance.clienteTiendaNube, "/metafields/products/"));
				restClient.Timeout = -1;
				RestRequest restRequest = new RestRequest(0);
				if (owner_id != "")
				{
					restRequest.AddParameter("owner_id", owner_id);
				}
				if (created_at_min != "")
				{
					restRequest.AddParameter("created_at_min", created_at_min);
				}
				restRequest.AddParameter("key", "ProductId");
				restRequest.AddParameter("per_page", "200");
				restRequest.AddParameter("page", page);
				restRequest.AddHeader("User-Agent", " Dragon (brunodecorneliis@gmail.com)");
				restRequest.AddHeader("Authentication", instance.tokenTiendaNube);
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