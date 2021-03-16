using Dragonfish_TN;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal class NuevoEnBaseA
	{
		public NuevoEnBaseA()
		{
		}

		public static IRestResponse Response(string comprobante, string codigo, JToken tipoComprobante)
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				RestClient restClient = new RestClient(string.Concat(new string[] { instance.urlDragonfish, "/api.Dragonfish/", comprobante, "/Nuevoenbasea/", codigo }));
				RestRequest restRequest = new RestRequest( RestSharp.Method.POST );
				restRequest.AddHeader("idCliente", instance.clienteDragonfish);
				restRequest.AddHeader("Authorization", instance.tokenDragonfish);
				restRequest.AddHeader("BaseDeDatos", instance.baseDeDatos);
				restRequest.AddHeader("Content-Type", "application/json");
				restRequest.AddParameter("application/json", tipoComprobante, RestSharp.ParameterType.RequestBody );
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