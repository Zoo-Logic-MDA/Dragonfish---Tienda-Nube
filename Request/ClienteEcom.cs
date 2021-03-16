using Dragonfish_TN;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal static class ClienteEcom
	{
		public static IRestResponse Response(Method method, JToken datosCliente, string idClienteEcommerce)
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				string str = "";
				if (method == Method.PUT)
				{
					str = idClienteEcommerce;
				}
				RestClient restClient = new RestClient(string.Concat(instance.urlDragonfish, "/api.Dragonfish/ClienteEcommerce/", str, "/"));
				RestRequest restRequest = new RestRequest(method);
				if (method == Method.GET)
				{
					restRequest.AddParameter("Cuentaecommerce", instance.plataformaEcommerce);
					restRequest.AddParameter("ClienteEcommerce", idClienteEcommerce);
				}
				restRequest.AddHeader("idCliente", instance.clienteDragonfish);
				restRequest.AddHeader("Authorization", instance.tokenDragonfish);
				restRequest.AddHeader("Content-Type", "application/json");
				restRequest.AddHeader("BaseDeDatos", instance.baseDeDatos);
                if ( ( method == Method.PUT ? true : method == Method.POST ) )
                {
                    restRequest.AddParameter( "application/json", datosCliente, RestSharp.ParameterType.RequestBody );
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