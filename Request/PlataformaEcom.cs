using Dragonfish_TN;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal static class PlataformaEcom
	{
		public static IRestResponse Response(Method method)
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				RestClient restClient = new RestClient(string.Concat(instance.urlDragonfish, "/api.Dragonfish/Ecommerce/", instance.plataformaEcommerce, "/"));
				RestRequest restRequest = new RestRequest(method);
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