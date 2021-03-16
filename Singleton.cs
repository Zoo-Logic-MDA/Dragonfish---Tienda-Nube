using Dragonfish_TN.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Dragonfish_TN
{
	internal class Singleton
	{
		public string log = "log.json";

		public string config = "config.xml";

		public string tokenDragonfish = "";

		public string clienteDragonfish = "";

		public string urlDragonfish = "";

		public string plataformaEcommerce = "";

		public string articuloGenerico = "";

		public int comprobanteSinPago;

		public string tokenTiendaNube = "";

		public string clienteTiendaNube = "";

		public DateTime fechaOrden;

		public DateTime fechaOrdenModificada;

		public string tokenStockOnline = "";

		public string resourceId = "";

		public DateTime fechaStock;

		public DateTime fechaProductos;

		public int startUp = 20;

		public bool habilitarPublicacionStock = false;

		public bool habilitarDescargaVentas = false;

		public bool notificarOrdenesModificadas = false;

		public string rutaListado = "";

		public bool descargarEnvio = true;

		public bool generarMovStock = false;

		public string baseDeDatos;

		private static Singleton _instance;

		public string origenDFTN = "Sistema";

		public string version = "";

		public static Singleton Instance
		{
			get
			{
				if (Singleton._instance == null)
				{
					Singleton._instance = new Singleton();
				}
				return Singleton._instance;
			}
		}

		static Singleton()
		{
			Singleton._instance = null;
		}

		private Singleton()
		{
			try
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
				this.version = versionInfo.FileVersion;
				string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				Directory.CreateDirectory(Path.Combine(folderPath, "Dragonfish(TN)"));
				this.config = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "\\Dragonfish(TN)\\config.xml");
				this.log = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "\\Dragonfish(TN)\\log.json");
				LogHandler.EnviarMsj("", ErrorType.Information, string.Concat("Ruta de la aplicación: ", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "\\Dragonfish(TN)\\"), this.origenDFTN);
				if (!File.Exists(this.config))
				{
					using (StreamWriter streamWriter = new StreamWriter(this.config, true))
					{
						streamWriter.Write(Resources.config);
					}
				}
				this.getXml();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogHandler.EnviarMsj("", ErrorType.Critical, string.Concat("Singleton: \n", exception.Message), this.origenDFTN);
			}
		}

		public void ActualizarFechaOrdenModificada(DateTime fecha)
		{
			try
			{
				if (DateTime.Compare(fecha, this.fechaOrdenModificada) == 1)
				{
					this.fechaOrdenModificada = Convert.ToDateTime(fecha);
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(this.config);
					xmlDocument.SelectSingleNode("Application/FechaOrdenModificada").InnerText = fecha.ToString();
					xmlDocument.Save(this.config);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogHandler.EnviarMsj("", ErrorType.Critical, string.Concat("ActualizarUltimaFechaOrdenModificada: \n", exception.Message), this.origenDFTN);
			}
		}

		public void ActualizarFechaUltimaOrden(DateTime fecha)
		{
			try
			{
				if (DateTime.Compare(fecha, this.fechaOrden) == 1)
				{
					this.fechaOrden = Convert.ToDateTime(fecha);
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(this.config);
					xmlDocument.SelectSingleNode("Application/FechaOrden").InnerText = fecha.ToString();
					xmlDocument.Save(this.config);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogHandler.EnviarMsj("", ErrorType.Critical, string.Concat("ActualizarUltimaFechaOrden: \n", exception.Message), this.origenDFTN);
			}
		}

		public void ActualizarLogErrorOperaciones(JArray logErrores)
		{
			try
			{
				if (File.Exists(this.log))
				{
					File.Delete(this.log);
				}
				using (StreamWriter streamWriter = new StreamWriter(this.log, true))
				{
					streamWriter.Write(logErrores);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogHandler.EnviarMsj("", ErrorType.Critical, string.Concat("ActualizarLogError: \n", exception.Message), this.origenDFTN);
			}
		}

		public void ActualizarTokenzNube(string token)
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(this.config);
				xmlDocument.SelectSingleNode("Application/TokenStockOnline").InnerText = token;
				xmlDocument.Save(this.config);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogHandler.EnviarMsj("", ErrorType.Critical, string.Concat("actualizarTokenzNube: \n", exception.Message), this.origenDFTN);
			}
		}

		public void ActualizarUltimaFechaProducto(DateTime fecha)
		{
			try
			{
				if (DateTime.Compare(fecha, this.fechaProductos) == 1)
				{
					this.fechaProductos = Convert.ToDateTime(fecha);
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(this.config);
					xmlDocument.SelectSingleNode("Application/FechaProductos").InnerText = fecha.ToString();
					xmlDocument.Save(this.config);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogHandler.EnviarMsj("", ErrorType.Critical, string.Concat("actualizarUltimaFechaProductos: \n", exception.Message), this.origenDFTN);
			}
		}

		public void ActualizarUltimaFechaStock(DateTime fecha)
		{
			try
			{
				if (DateTime.Compare(fecha, this.fechaStock) == 1)
				{
					this.fechaStock = Convert.ToDateTime(fecha);
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(this.config);
					xmlDocument.SelectSingleNode("Application/FechaStock").InnerText = fecha.ToString();
					xmlDocument.Save(this.config);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogHandler.EnviarMsj("", ErrorType.Critical, string.Concat("ActualizarUltimaFechaStock: \n", exception.Message), this.origenDFTN);
			}
		}

		private void AgregarElemento(string elemento, string valor)
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(this.config);
				XmlElement xmlElement = xmlDocument.CreateElement(elemento);
				xmlElement.InnerText = valor;
				xmlDocument.DocumentElement.AppendChild(xmlElement);
				xmlDocument.Save(this.config);
			}
			catch (Exception exception)
			{
			}
		}

		public void DesactivarModificacionDeOrdenes()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(this.config);
			xmlDocument.SelectSingleNode("Application/NotificarmeComprobantesCancelados").InnerText = "false";
			xmlDocument.Save(this.config);
		}

		public void getXml()
		{
			try
			{
				if (!File.Exists(this.config))
				{
					using (StreamWriter streamWriter = new StreamWriter(this.config, true))
					{
						streamWriter.Write(Resources.config);
					}
				}
				else
				{
					XElement xElement = XElement.Parse(File.ReadAllText(this.config));
					this.habilitarPublicacionStock = bool.Parse(xElement.Element("HabilitarPublicacionStock").Value);
					this.habilitarDescargaVentas = bool.Parse(xElement.Element("HabilitarDescargaVentas").Value);
					this.startUp = short.Parse(xElement.Element("StartUp").Value);
					this.tokenDragonfish = xElement.Element("TokenDragonfish").Value;
					this.clienteDragonfish = xElement.Element("ClienteDragonfish").Value;
					this.baseDeDatos = xElement.Element("BasesDeDatos").Value;
					this.urlDragonfish = xElement.Element("UrlDragonfish").Value;
					this.plataformaEcommerce = xElement.Element("PlataformaEcommerce").Value;
					this.comprobanteSinPago = int.Parse(xElement.Element("ComprobanteSinPago").Value);
					this.articuloGenerico = xElement.Element("ArticuloGenerico").Value;
					this.tokenTiendaNube = xElement.Element("TokenTiendaNube").Value;
					this.clienteTiendaNube = xElement.Element("ClienteTiendaNube").Value;
					this.fechaOrden = Convert.ToDateTime(xElement.Element("FechaOrden").Value);
					this.fechaProductos = Convert.ToDateTime(xElement.Element("FechaProductos").Value);
					this.tokenStockOnline = xElement.Element("TokenStockOnline").Value;
					this.resourceId = xElement.Element("ResourcesID").Value;
					this.fechaStock = Convert.ToDateTime(xElement.Element("FechaStock").Value);
					try
					{
						this.notificarOrdenesModificadas = bool.Parse(xElement.Element("NotificarmeComprobantesCancelados").Value);
					}
					catch
					{
						this.AgregarElemento("NotificarmeComprobantesCancelados", "False");
					}
					try
					{
						this.generarMovStock = bool.Parse(xElement.Element("GenerarMovStockPorCombinacionInexistente").Value);
					}
					catch
					{
						this.AgregarElemento("GenerarMovStockPorCombinacionInexistente", "False");
					}
					try
					{
						this.descargarEnvio = bool.Parse(xElement.Element("DescargaEnvio").Value);
					}
					catch
					{
						this.AgregarElemento("DescargaEnvio", "True");
					}
					try
					{
						this.rutaListado = xElement.Element("RutaListado").Value;
					}
					catch
					{
						this.AgregarElemento("RutaListado", "");
					}
					try
					{
						this.fechaOrdenModificada = Convert.ToDateTime(xElement.Element("FechaOrdenModificada").Value);
					}
					catch
					{
						this.AgregarElemento("FechaOrdenModificada", DateTime.Now.ToString());
					}
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogHandler.EnviarMsj("", ErrorType.Critical, string.Concat("File.Exists: \n", exception.Message), this.origenDFTN);
			}
		}
	}
}