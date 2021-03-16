using Dragonfish_TN;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal static class Cliente
	{
		public static IRestResponse Response(Method method, JToken datosCliente)
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				RestClient restClient = new RestClient(string.Concat(instance.urlDragonfish, "/api.Dragonfish/Cliente/"));
				RestRequest restRequest = new RestRequest(method);
				if (method == 0)
				{
					restRequest.AddParameter("query", datosCliente["EMail"].ToString());
				}
				restRequest.AddHeader("idCliente", instance.clienteDragonfish);
				restRequest.AddHeader("Authorization", instance.tokenDragonfish);
				restRequest.AddHeader("Content-Type", "application/json");
				restRequest.AddHeader("BaseDeDatos", instance.baseDeDatos);
				if ((method == Method.PUT ? true : method == Method.POST))
				{
					restRequest.AddParameter("application/json", datosCliente, RestSharp.ParameterType.RequestBody);
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