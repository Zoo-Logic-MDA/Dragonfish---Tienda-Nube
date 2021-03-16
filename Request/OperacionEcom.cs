using Dragonfish_TN;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal static class OperacionEcom
	{
		public static IRestResponse Response(Method method, JToken detalleRemito, string numero)
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				RestClient restClient = new RestClient(string.Concat(instance.urlDragonfish, "/api.Dragonfish/Operacionecommerce/"));
				RestRequest restRequest = new RestRequest(method);
				if (method == 0)
				{
					restRequest.AddParameter("Ecommerce", instance.plataformaEcommerce);
					restRequest.AddParameter("Numero", numero);
				}
				restRequest.AddHeader("idCliente", instance.clienteDragonfish);
				restRequest.AddHeader("Authorization", instance.tokenDragonfish);
				restRequest.AddHeader("BaseDeDatos", instance.baseDeDatos);
				restRequest.AddHeader("Content-Type", "application/json");
				if ((method == Method.PUT ? true : method == Method.POST))
				{
					restRequest.AddParameter("application/json", detalleRemito, ParameterType.RequestBody );
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