using Dragonfish_TN;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal static class Variants
	{
		public static IRestResponse Response(Method method, JToken detalleProduct, string id)
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				string str = "?per_page=100";
				if ((method == Method.PUT ? true : method == Method.DELETE))
				{
					str = string.Concat("/", detalleProduct["id"].ToString());
				}
				RestClient restClient = new RestClient(string.Concat(new string[] { "https://api.tiendanube.com/v1/", instance.clienteTiendaNube, "/products/", id, "/variants", str }));
				restClient.Timeout =-1;
				RestRequest restRequest = new RestRequest(method);
				restRequest.AddHeader("User-Agent", " Dragon (brunodecorneliis@gmail.com)");
				restRequest.AddHeader("Authentication", instance.tokenTiendaNube);
				restRequest.AddHeader("Content-Type", "application/json");
				if ((method == Method.PUT ? true : method == Method.POST))
				{
					restRequest.AddParameter("application/json", detalleProduct, ParameterType.RequestBody );
				}
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