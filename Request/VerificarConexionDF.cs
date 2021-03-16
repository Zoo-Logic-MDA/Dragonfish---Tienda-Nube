using Dragonfish_TN;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal static class VerificarConexionDF
	{
		public static IRestResponse Response()
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				RestClient restClient = new RestClient(string.Concat(instance.urlDragonfish, "/api.Dragonfish/Articulo/SEÑA/"));
				RestRequest restRequest = new RestRequest(0);
				restRequest.AddHeader("BaseDeDatos", instance.baseDeDatos);
				restRequest.AddHeader("idCliente", instance.clienteDragonfish);
				restRequest.AddHeader("Authorization", instance.tokenDragonfish);
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