using Dragonfish_TN;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal static class MovStock
	{
		public static IRestResponse Response(JToken detalle)
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				RestClient restClient = new RestClient(string.Concat(instance.urlDragonfish, "/api.Dragonfish/Movimientodestock/"));
				RestRequest restRequest = new RestRequest( RestSharp.Method.POST );
				restRequest.AddHeader("idCliente", instance.clienteDragonfish);
				restRequest.AddHeader("Authorization", instance.tokenDragonfish);
				restRequest.AddHeader("BaseDeDatos", instance.baseDeDatos);
				restRequest.AddHeader("Content-Type", "application/json");
				restRequest.AddParameter("application/json", detalle, RestSharp.ParameterType.RequestBody );
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