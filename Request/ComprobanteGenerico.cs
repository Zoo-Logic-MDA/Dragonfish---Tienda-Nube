using Dragonfish_TN;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal class ComprobanteGenerico
	{
		public ComprobanteGenerico()
		{
		}

		public static IRestResponse Response(string comprobante, string codigo)
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				RestClient restClient = new RestClient(string.Concat(new string[] { instance.urlDragonfish, "/api.Dragonfish/", comprobante, "/", codigo, "/" }));
				RestRequest restRequest = new RestRequest(0);
				restRequest.AddHeader("idCliente", instance.clienteDragonfish);
				restRequest.AddHeader("Authorization", instance.tokenDragonfish);
				restRequest.AddHeader("BaseDeDatos", instance.baseDeDatos);
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