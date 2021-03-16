using Dragonfish_TN;
using RestSharp;
using System;

namespace Dragonfish_TN.Request
{
	internal static class StockOnline
	{
		public static IRestResponse Response(int page, string producto, bool fechaFiltro)
		{
			IRestResponse restResponse;
			try
			{
				Singleton instance = Singleton.Instance;
				RestClient restClient = new RestClient("https://api.znube.com.ar:8081/Omnichannel/GetStock/");
				RestRequest restRequest = new RestRequest(0);
				if (producto != "")
				{
					restRequest.AddParameter("ProductId", producto);
				}
				if (fechaFiltro)
				{
					DateTime dateTime = instance.fechaStock.AddHours(3);
					restRequest.AddParameter("fromLastUpdateDate", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
				}
				restRequest.AddParameter("Offset", string.Concat(page.ToString(), "00"));
				restRequest.AddParameter("limit", "100");
				restRequest.AddParameter("Resources", instance.resourceId);
				restRequest.AddHeader("zNube-token", instance.tokenStockOnline);
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