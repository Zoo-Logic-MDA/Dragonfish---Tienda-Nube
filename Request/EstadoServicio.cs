using Dragonfish_TN;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal class EstadoServicio
	{
		public EstadoServicio()
		{
		}

		public static IRestResponse Response()
		{
			IRestResponse restResponse;
            try
            {
                Singleton instance = Singleton.Instance;
                RestClient restClient = new RestClient( "http://underdesign.com.ar/api/estadoservicio.php" );
                RestRequest restRequest = new RestRequest( Method.POST );
                restRequest.AddHeader( "Content-Type", "application/json" );
                restRequest.AddParameter( "application/json", string.Concat( new string[] { "{\r\n    \r\n    \"app\":\"dragonfish\",\r\n    \"store_id\":\"", instance.clienteTiendaNube, "\",\r\n    \"version\":\"", instance.version, "\"\r\n}" } ), RestSharp.ParameterType.RequestBody );
                restResponse = restClient.Execute( restRequest );
            }
            catch ( Exception exception )
            {
                throw exception;
            }
			return restResponse;
		}
	}
}