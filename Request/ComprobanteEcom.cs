using Dragonfish_TN;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal static class ComprobanteEcom
	{
		public static IRestResponse Response(JToken detalle)
		{
			IRestResponse restResponse;
			Singleton instance = Singleton.Instance;
			try
			{
				RestClient restClient = new RestClient(string.Concat(instance.urlDragonfish, "/api.Dragonfish/Comprobantesecommerce/"));
				RestRequest restRequest = new RestRequest( RestSharp.Method.POST );
				restRequest.Timeout = 1200000;
				restRequest.AddHeader("idCliente", instance.clienteDragonfish);
				restRequest.AddHeader("Authorization", instance.tokenDragonfish);
				restRequest.AddHeader("BaseDeDatos", instance.baseDeDatos);
				restRequest.AddHeader("Content-Type", "application/json");
				restRequest.AddParameter("application/json", detalle, RestSharp.ParameterType.RequestBody );
				restResponse = restClient.Execute(restRequest);
			}
			catch (Exception exception)
			{
				throw;
			}
			return restResponse;
		}
	}
}