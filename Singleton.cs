using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Stock_TN
{
    class Singleton
    {
        Notification notification = null;

        public string log = "log.txt";
        public string config = "config.xml";
        public string tokenDragonfish = "";
        public string clienteDragonfish = "";
        public string urlDragonfish = "";
        public string plataformaEcommerce = "";
        public int comprobanteSinPago;

        public string tokenTiendaNube = "";
        public string clienteTiendaNube = "";
        public DateTime fechaOrden;

        public string tokenStockOnline = "";
        public string resourceId = "";

        public DateTime fechaStock;
        public DateTime fechaProductos;
        
        public int startUp;
        public bool habilitarPublicacionStock = false;
        public bool habilitarDescargaVentas = false;

        public string basesDeDatos;
        private static Singleton _instance = null;
        public string origenDFTN = "Sistema";

        private Singleton()
        {
            notification = Notification.Instance;
            getXml();
        }

        public static Singleton Instance
        {
            get
            {
                // The first call will create the one and only instance.
                if (_instance == null)
                {
                    _instance = new Singleton();
                }

                // Every call afterwards will return the single instance created above.
                return _instance;
            }
        }

        public void getXml()
        {
            
                config = Application.StartupPath + "\\config.xml";
                if (File.Exists(config))
                {
                    string value = File.ReadAllText(config);
                    XElement xmldoc = XElement.Parse(value);


                    habilitarPublicacionStock = bool.Parse(xmldoc.Element("HabilitarPublicacionStock").Value);
                    habilitarDescargaVentas = bool.Parse(xmldoc.Element("HabilitarDescargaVentas").Value);

                    startUp = short.Parse(xmldoc.Element("StartUp").Value);

                    tokenDragonfish = xmldoc.Element("TokenDragonfish").Value;
                    clienteDragonfish = xmldoc.Element("ClienteDragonfish").Value;
                    basesDeDatos = xmldoc.Element("BasesDeDatos").Value;
                    urlDragonfish = xmldoc.Element("UrlDragonfish").Value;
                    plataformaEcommerce = xmldoc.Element("PlataformaEcommerce").Value;
                    comprobanteSinPago = int.Parse(xmldoc.Element("ComprobanteSinPago").Value);

                    tokenTiendaNube = xmldoc.Element("TokenTiendaNube").Value;
                    clienteTiendaNube = xmldoc.Element("ClienteTiendaNube").Value;
                    fechaOrden = Convert.ToDateTime(xmldoc.Element("FechaOrden").Value);
                    fechaProductos = Convert.ToDateTime(xmldoc.Element("FechaProductos").Value);

                    tokenStockOnline = xmldoc.Element("TokenStockOnline").Value;
                    resourceId = xmldoc.Element("ResourcesID").Value;
                    fechaStock = Convert.ToDateTime(xmldoc.Element("FechaStock").Value);

                }
                else
                {
                    LogHandler.EnviarMsj("", ErrorType.Critical, "Archivo config.xml no existe", origenDFTN);
                    notification.ErrorIcon("Servicio Detenido");
                }
           

        }

        public void actualizarOrdenes(DateTime fecha)
        {
            try
            {
                if (DateTime.Compare(fecha, fechaOrden) == 1)
                {
                    fechaStock = Convert.ToDateTime(fecha);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(config);

                    XmlNode node = doc.SelectSingleNode("Application/FechaOrden"); // [index of user node]
                    node.InnerText = fecha.ToString();

                    doc.Save(config);
                }
            }
            catch (Exception e)
            {
                LogHandler.EnviarMsj("", ErrorType.Critical, "actualizarUltimaFechaOrden: \n" + e.Message, origenDFTN);

            }
        }

        public void actualizarUltimaFechaStock(DateTime fecha)
        {
            try
            {
                if (DateTime.Compare(fecha, fechaStock) == 1)
                {
                    fechaStock = Convert.ToDateTime(fecha);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(config);

                    XmlNode node = doc.SelectSingleNode("Application/FechaStock"); // [index of user node]
                    node.InnerText = fecha.ToString();

                    doc.Save(config);
                }
            }
            catch (Exception e)
            {
                LogHandler.EnviarMsj("", ErrorType.Critical, "actualizarUltimaFechaStock: \n" + e.Message, origenDFTN);

            }
        }
       
        public void actualizarUltimaFechaProducto(DateTime fecha)
        {
            try
            {
                if (DateTime.Compare(fecha, fechaProductos) == 1)
                {
                    fechaProductos = Convert.ToDateTime(fecha);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(config);

                    XmlNode node = doc.SelectSingleNode("Application/FechaProductos"); // [index of user node]
                    node.InnerText = fecha.ToString();

                    doc.Save(config);
                }
            }
            catch (Exception e)
            {
                LogHandler.EnviarMsj("", ErrorType.Critical, "actualizarUltimaFechaProductos: \n" + e.Message, origenDFTN);
            }
        }

    }
}
