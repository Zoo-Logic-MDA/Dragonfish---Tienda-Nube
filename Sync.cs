using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;

namespace Stock_TN
{
    class Sync
    {
        static Singleton singleton = null;
        Notification notification = null;
        public bool running = false;
        JArray arrayProductos = new JArray();
        public static Timer InternalTimer;
        private static Sync _instance = null;
        private string origenStock = "Publicación de Stock";
        private string origenVentas = "Descarga de Operaciones";
        bool primerMandato = true;

        public static Sync Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Sync();
                }
                return _instance;
            }
        }

        public void StartSync()
        {
            singleton = Singleton.Instance;
            notification = Notification.Instance;
            if (!running)
            {
                InternalTimer = new Timer(InitProcess, null, 0, singleton.startUp * 60000);
            }
        }

        private void InitProcess(object state)
        {
            if (!running)
            {
                if (DateTime.Now.Hour < 1 || DateTime.Now.Hour > 7) {
                    running = true;
                    StartProcess();
                }
            }
        }

        public bool cancelTimer()
        {
            while (running)
            {
            }
            try
            {
                InternalTimer.Dispose();
            }
            catch { }

            return true;

        }

        private void StartProcess()
        {
            try
            {
                if (DateTime.Now.Hour == 20 || DateTime.Now.Hour == 8)
                {
                    primerMandato = true;
                }

                LogHandler.EnviarMsj("", ErrorType.Minor, "Proceso Iniciado.", singleton.origenDFTN);

                notification.ProcessingIcon();
                singleton.getXml();

                if (singleton.habilitarDescargaVentas)
                {
                    Console.WriteLine(singleton.fechaOrden.ToString("yyyy-MM-dd HH:mm:ss"));
                    if (singleton.fechaOrden == Convert.ToDateTime("01/01/1900 00:00:00"))
                    {
                        var responseOrders = Orders(Method.GET, 1, 1);

                        if (responseOrders.StatusCode == HttpStatusCode.OK)
                        {
                            singleton.actualizarOrdenes(Convert.ToDateTime(JArray.Parse(responseOrders.Content)[0]["created_at"].ToString()).AddMilliseconds(1));
                        }
                        else
                        {
                            singleton.actualizarOrdenes(DateTime.Now);
                        }
                        singleton.getXml();
                    }

                    if (singleton.plataformaEcommerce != "")
                    {
                        descargarAOperacionDeEcommerce(singleton.basesDeDatos);
                    }
                    else
                    {
                        LogHandler.EnviarMsj("", ErrorType.Critical, "El campo 'Plataforma de Ecommerce' se encuentra vacion en el archivo Config.xml.", origenVentas);
                        notification.ErrorIcon("El campo 'Plataforma de Ecommerce' se encuentra vacion en el archivo Config.xml.");
                        return;
                    }
                }

                singleton.getXml();

                if (singleton.habilitarPublicacionStock)
                {
                    LogHandler.EnviarMsj("", ErrorType.Minor, "Protocolo de espera:\nAguardando publicación a Stock Online.", singleton.origenDFTN);
                    Thread.Sleep(10000);

                    LogHandler.EnviarMsj("", ErrorType.Information, "Obteniendo lista de productos de Tienda Nube.", origenStock);
                    ObtenerListaProductos("");
                    PublicarStock("", !primerMandato);
                    primerMandato = false;
                    arrayProductos.Clear();
                    ComprobarNuevosArticulos();
                    arrayProductos.Clear();

                }

            }catch(Exception e)
            {
                LogHandler.EnviarMsj("", ErrorType.Minor, e.Message, singleton.origenDFTN);

            }
            notification.IdleIcon();
            LogHandler.EnviarMsj("", ErrorType.Minor, "Proceso Finalizado.", singleton.origenDFTN);
            running = false;
            

        }

        private void ObtenerListaProductos(string fecha)
        {
            
            var statusCode = HttpStatusCode.OK;
            var page = 1;
            while (statusCode == HttpStatusCode.OK)
            {
                var responseMefields = Metafield("", fecha, page++);

                if (responseMefields.StatusCode == HttpStatusCode.OK)
                {
                    foreach (var producto in JArray.Parse(responseMefields.Content))
                    {
                        arrayProductos.Add(producto);
                    }
                }
                statusCode = responseMefields.StatusCode;
            }

            //Console.WriteLine(" " + arrayProductos.Count + " productos de Tienda Nube agregados a la lista.");

        }

        private void PublicarStock(string sku,bool filtroFecha)
        {
            //Console.WriteLine("Obteniendo actualizacion de stock de zNube.");
            LogHandler.EnviarMsj("", ErrorType.Information, "Obteniendo actualización de stock de zNube.", origenStock);

            var cantidadDeRegistros = 1;
            var page = 0;
            int combinacionesModificadas = 0;

            var dateTimeConsulta = DateTime.Now;
            while (cantidadDeRegistros>0)
            {
                var responseStock = StockOnline(page++,sku,filtroFecha);
                
                if (responseStock.StatusCode == HttpStatusCode.OK)
                {
                    var resultado = JObject.Parse(responseStock.Content)["Data"]["Stock"].ToString();
                    cantidadDeRegistros = JArray.Parse(resultado).Count;

                    var idProductoAnterior = "";
                    IRestResponse responseVariants = null;

                    foreach (var combinacion in JArray.Parse(resultado))
                    {
                        var articulo = combinacion["ProductId"].ToString();
                        var productID = "";

                        foreach (var producto in arrayProductos)
                        {
                            if (producto["value"].ToString() == articulo)
                            {
                                Console.WriteLine(combinacion["Sku"] + "- Producto " + producto["owner_id"].ToString());
                                productID = producto["owner_id"].ToString();
                                break;
                            }
                        }
                        if (productID != "")
                        {
                            var stock = 0;
                            for (var i = 0; i < JArray.Parse(combinacion["Stock"].ToString()).Count; i++)
                            {
                                stock += int.Parse(combinacion["Stock"][i]["Quantity"].ToString());
                            }
                            var color = "";
                            var talle = "";

                            color = combinacion["Sku"].ToString().Split('#')[1];
                            talle = combinacion["Sku"].ToString().Split('#')[2];

                            if (idProductoAnterior != productID)
                            {
                                idProductoAnterior = productID;
                                responseVariants = Variants(Method.GET, null, productID);
                                Thread.Sleep(3000);
                            }
                            Console.WriteLine("Buscando variantes...");
                            switch (responseVariants.StatusCode)
                            {
                                case HttpStatusCode.OK:
                                    Console.WriteLine(color + "-_-" + talle);

                                    var methodVariant = Method.POST;

                                    JObject JOVariable = new JObject();
                                    JOVariable.Add(new JProperty("stock", stock));
                                    for (var i = 0; i < JArray.Parse(responseVariants.Content).Count; i++)
                                    {
                                        var variante = JArray.Parse(responseVariants.Content)[i];
                                        if (variante["sku"].ToString() == color + "-_-" + talle)
                                        {
                                            Console.WriteLine(combinacion["Sku"] + "- Variante " + variante["id"].ToString());

                                            JOVariable.Add(new JProperty("id", variante["id"].ToString()));
                                            methodVariant = Method.PUT;
                                            break;
                                        }
                                    }



                                    if (!JOVariable.ContainsKey("id") && JOVariable["stock"].ToString() == "0")
                                    {

                                    }
                                    else if (JOVariable.ContainsKey("id"))
                                    {

                                        var newResponseVariant = Variants(methodVariant, JOVariable, productID);
                                        combinacionesModificadas++;
                                        LogHandler.EnviarMsj("", ErrorType.Information, "Actualizando combinación " + combinacion["Sku"] + ": " + newResponseVariant.StatusDescription, origenStock);

                                    }

                                    break;

                                case HttpStatusCode.NotFound:
                                    LogHandler.EnviarMsj("", ErrorType.Minor, "Publicación Stock: " + combinacion["Sku"] + " - " + responseVariants.StatusDescription, origenStock);
                                    break;

                                case HttpStatusCode.Unauthorized:
                                    LogHandler.EnviarMsj("", ErrorType.Minor, "Error de autenticación Stock Online.", origenStock);

                                    notification.ErrorIcon("Error de autenticación Stock Online.");
                                    break;
                                default:
                                    LogHandler.EnviarMsj("", ErrorType.Minor, "Publicación Stock: " + combinacion["Sku"] + " - " + responseVariants.StatusDescription, origenStock);
                                    break;
                            }
                        }
                    }
                    Console.WriteLine("----------------------------------");

                }
            }
            LogHandler.EnviarMsj("", ErrorType.Information, "Se modificaron " + combinacionesModificadas + " combinaciones.", origenStock);
            if (sku == "")
            {
                singleton.actualizarUltimaFechaStock(dateTimeConsulta);
            }
        }

        private void ComprobarNuevosArticulos()
        {
            LogHandler.EnviarMsj("", ErrorType.Information, "Obteniendo lista de nuevos productos de Tienda Nube.", origenStock);

            var dateTimeConsulta = DateTime.Now;

            ObtenerListaProductos(singleton.fechaProductos.ToString());

            if (arrayProductos.Count != 0)
            {
                foreach (var producto in arrayProductos)
                {
                    PublicarStock(producto["value"].ToString(), false);
                }
            }
            else
            {
                LogHandler.EnviarMsj("", ErrorType.Information, "No se encontraron nuevos productos.", origenStock);
            }

            singleton.actualizarUltimaFechaProducto(dateTimeConsulta);
        }

        private void descargarAOperacionDeEcommerce(string baseDeDatos)
        {
            LogHandler.EnviarMsj("", ErrorType.Information, "Obteniendo ventas de Tienda Nube.", origenVentas);
            JArray arrayOrders = new JArray();

            var responseStatus = HttpStatusCode.OK;
            var page = 1;
            while (responseStatus == HttpStatusCode.OK)
            {
                var responseOrders = Orders(Method.GET,page++,100);

                responseStatus = responseOrders.StatusCode;
                if (responseOrders.StatusCode == HttpStatusCode.OK)
                {
                    foreach(var orders in JArray.Parse(responseOrders.Content))
                    {
                        arrayOrders.Add(orders);
                    }

                }
                else if (responseOrders.StatusCode != HttpStatusCode.NotFound)
                {
                    LogHandler.EnviarMsj("", ErrorType.Critical, "Obteniendo ventas de Tienda Nube: " + responseOrders.StatusDescription, origenVentas);
                }

            }
            var cantidadDeOperaciones = 0;
            
            JArray detalle = new JArray();

            if (arrayOrders.Count != 0)
            {
                JArray arrayOrdersSorted = new JArray(arrayOrders.OrderBy(obj => (string)obj["number"]));
                try
                {
                    LogHandler.EnviarMsj("", ErrorType.Information, "Procesando ventas.", origenVentas);

                    string ArticuloEnvio = "";
                    string TipoComprobantePlatafomar = "";
                    string BaseDeDatosPlataforma = "";
                    string ClienteDragonfishPlataforma = "";
                    var responsePlataforma = PlataformaEcommerce(Method.GET, baseDeDatos);

                    if (responsePlataforma.StatusCode == HttpStatusCode.OK)
                    {

                        if (JObject.Parse(responsePlataforma.Content)["Accion"].ToString() != "")
                        {
                            TipoComprobantePlatafomar = JObject.Parse(responsePlataforma.Content)["Accion"].ToString();
                            BaseDeDatosPlataforma = JObject.Parse(responsePlataforma.Content)["BaseDeDatos"].ToString();
                        }
                        ClienteDragonfishPlataforma = JObject.Parse(responsePlataforma.Content)["ClienteDefault"].ToString();
                        ArticuloEnvio = JObject.Parse(responsePlataforma.Content)["Concenvio"].ToString();

                        for (int i = 0; i < arrayOrdersSorted.Count; i++)
                        {
                            var errorOperacion = false;

                            var order = arrayOrdersSorted[i];

                            dynamic operacionObjeto = new JObject();

                            operacionObjeto.BaseDeDatos = BaseDeDatosPlataforma;

                            var numeroOrden = order["number"];
                            var observaciones = "";
                            var existeOperacion = OperacionEcommerce(Method.GET, numeroOrden, null, baseDeDatos).StatusCode;
                            
                            if (existeOperacion == HttpStatusCode.NotFound)
                            {
                                operacionObjeto.Ecommerce = singleton.plataformaEcommerce;
                                operacionObjeto.Numero = order["number"];
                                operacionObjeto.NumeroOperacion = order["number"];

                                var envio = order["billing_address"] + " " + order["billing_number"] + " " + order["billing_floor"] + " - CP:" + order["billing_zipcode"];
                                operacionObjeto.DatosEnvio = envio.Length <= 40 ? envio : envio.Substring(0, 40);

                                var articulosArray = new JArray();

                                var errorProducto = false;
                                JArray productos = (JArray)order["products"];

                                for (int a = 0; a < productos.Count; a++)
                                {
                                    dynamic articuloObjeto = new JObject();

                                    var producto = productos[a];
                                    var articuloResponse = Metafield(producto["product_id"].ToString(), "", 1);
                                
                                    try
                                    {
                                        if (articuloResponse.StatusCode == HttpStatusCode.OK)
                                        {

                                        if (JArray.Parse(articuloResponse.Content).Count != 0)
                                            {
                                                var articulo = JArray.Parse(articuloResponse.Content)[0]["value"];
                                                var sku = producto["sku"].ToString().Replace("-_-", "#");

                                                var color = sku.Split('#')[0];
                                                var talle = sku.Split('#')[1];

                                                articuloObjeto.IDPublicacion = producto["product_id"];
                                                articuloObjeto.Articulo = articulo;
                                                articuloObjeto.Color = color;
                                                articuloObjeto.Talle = talle;
                                                articuloObjeto.Precio = producto["price"];
                                                articuloObjeto.Cantidad = producto["quantity"];

                                                articulosArray.Add(articuloObjeto);
                                            }
                                            else
                                            {
                                                LogHandler.EnviarMsj("", ErrorType.Critical, "Obtener productos del Metafield: \nNúmero orden: " + order["number"].ToString() + "\nProducto " + producto["product_id"] + " inexistente.\n\nOperación cancelada.", origenVentas);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        LogHandler.EnviarMsj("", ErrorType.Critical, "Obtener productos del Metafield: \nNúmero orden: " + order["number"].ToString() + "\n" + e.Message + "\n\nOperación cancelada.", origenVentas);
                                        errorProducto = true;
                                    }
                                }

                                if (errorProducto)
                                {
                                    errorOperacion = true;
                                }


                                if (order["shipping_cost_customer"].ToString() != "0.00")
                                {
                                    dynamic articuloObjeto = new JObject();
                                    articuloObjeto.Articulo = ArticuloEnvio;

                                    articuloObjeto.Precio = order["shipping_cost_customer"];
                                    articuloObjeto.IDPublicacion = "ENVIO";
                                    articuloObjeto.Cantidad = 1;
                                    articulosArray.Add(articuloObjeto);

                                }

                                operacionObjeto.PublicacionDetalle = articulosArray;

                                var cliente = order["customer"];
                                var existeClienteEcom = ClienteEcommerce(Method.GET, null, cliente["id"].ToString(), baseDeDatos);
                                var codClienteEccom = "";
                                dynamic clienteObjeto = new JObject();

                                clienteObjeto.Cuentaecommerce = singleton.plataformaEcommerce;
                                clienteObjeto.ClienteEcommerce = cliente["id"].ToString();
                                clienteObjeto.Nombre = cliente["name"].ToString().Substring(0, Math.Min(40, cliente["name"].ToString().Length));
                                clienteObjeto.Mail = cliente["email"].ToString();
                                clienteObjeto.Telefono = cliente["phone"].ToString();
                                clienteObjeto.NroDoc = cliente["identification"].ToString().Substring(0, Math.Min(40, cliente["identification"].ToString().Length));
                                clienteObjeto.ClienteDragon = "";

                                clienteObjeto.PrimerNombre = cliente["name"].ToString().Substring(0, Math.Min(60, cliente["name"].ToString().Length));
                                clienteObjeto.NroDocumento = cliente["identification"].ToString().Substring(0, Math.Min(10, cliente["identification"].ToString().Length));
                                clienteObjeto.EMail = cliente["email"].ToString();
                                clienteObjeto.Movil = cliente["phone"].ToString().Substring(0, Math.Min(30, cliente["phone"].ToString().Length));

                                if (existeClienteEcom.StatusCode == HttpStatusCode.NotFound)
                                {
                                    var responseCliente = ClienteEcommerce(Method.POST, clienteObjeto, "", baseDeDatos);
                                    if (responseCliente.StatusCode == HttpStatusCode.Created)
                                    {
                                        codClienteEccom = JObject.Parse(responseCliente.Content)["Codigo"].ToString();
                                        operacionObjeto.ClienteEcom = JObject.Parse(responseCliente.Content)["Codigo"].ToString();
                                    }
                                    else
                                    {
                                        LogHandler.EnviarMsj("", ErrorType.Critical, "Error al crear el cliente ecommerce: \nNúmero orden: " + order["number"].ToString() + "\n" + responseCliente.StatusDescription + "\n\nOperación cancelada.", origenVentas);
                                    }
                                }
                                else if (existeClienteEcom.StatusCode == HttpStatusCode.OK)
                                {
                                    codClienteEccom = JObject.Parse(existeClienteEcom.Content)["Resultados"][0]["Codigo"].ToString();
                                    operacionObjeto.ClienteEcom = JObject.Parse(existeClienteEcom.Content)["Resultados"][0]["Codigo"].ToString();
                                    clienteObjeto.ClienteDragon = JObject.Parse(existeClienteEcom.Content)["Resultados"][0]["ClienteDragon"].ToString();
                                    clienteObjeto.EMail = JObject.Parse(existeClienteEcom.Content)["Resultados"][0]["Mail"].ToString();
                                }
                                else
                                {
                                    LogHandler.EnviarMsj("", ErrorType.Critical, "Error al obtener el cliente ecommerce: \nNúmero orden: " + order["number"].ToString() + "\n" + existeClienteEcom.StatusDescription + "\n\nOperación cancelada.", origenVentas);
                                }

                                if (clienteObjeto.ClienteDragon == "")
                                {
                                    var responseBuscarCliente = Cliente(Method.GET, clienteObjeto, singleton.basesDeDatos);
                                    Console.WriteLine(responseBuscarCliente.StatusCode);
                                    
                                    if (responseBuscarCliente.StatusCode == HttpStatusCode.OK)
                                    {
                                        try
                                        {
                                            clienteObjeto.ClienteDragon = JObject.Parse(responseBuscarCliente.Content)["Resultados"][0]["Codigo"];
                                            var res = ClienteEcommerce(Method.PUT, clienteObjeto, codClienteEccom, baseDeDatos);
                                            if (res.StatusCode != HttpStatusCode.OK)
                                            {
                                                LogHandler.EnviarMsj("", ErrorType.Critical, "Error al asignar el cliente Dragonfish al cliente Ecommerce: \nNúmero orden: " + order["number"].ToString() + "\n" + res.StatusDescription, origenVentas);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            operacionObjeto.ClienteDragon = ClienteDragonfishPlataforma;
                                            LogHandler.EnviarMsj("", ErrorType.Critical, "Error al consultar el cliente Dragonfish: \nNúmero orden: " + order["number"].ToString() + "\n" + e.Message, origenVentas);

                                        }
                                    }
                                    else if (responseBuscarCliente.StatusCode == HttpStatusCode.NotFound)
                                    {
                                        var responseCrearCliente = Cliente(Method.POST, clienteObjeto, singleton.basesDeDatos);
                                        if (responseCrearCliente.StatusCode == HttpStatusCode.Created)
                                        {
                                            try
                                            {
                                                clienteObjeto.ClienteDragon = JObject.Parse(responseCrearCliente.Content)["Codigo"];
                                                var res = ClienteEcommerce(Method.PUT, clienteObjeto, codClienteEccom, baseDeDatos);
                                                if (res.StatusCode != HttpStatusCode.OK)
                                                {
                                                    LogHandler.EnviarMsj("", ErrorType.Critical, "Error al asignar el cliente Dragonfish al cliente Ecommerce: \nNúmero orden: " + order["number"].ToString() + "\n" + res.StatusDescription, origenVentas);
                                                }

                                            }
                                            catch (Exception e)
                                            {
                                                operacionObjeto.ClienteDragon = ClienteDragonfishPlataforma;
                                                LogHandler.EnviarMsj("", ErrorType.Minor, "Error al crear el cliente Dragonfish: \nNúmero orden: " + order["number"].ToString() + "\n" + e.Message, origenVentas);

                                            }
                                        }
                                        else
                                        {
                                            operacionObjeto.ClienteDragon = ClienteDragonfishPlataforma;
                                            LogHandler.EnviarMsj("", ErrorType.Minor, "Error al crear el cliente Dragonfish: \nNúmero orden: " + order["number"].ToString() + "\n" + responseCrearCliente.StatusDescription, origenVentas);
                                        }
                                    }
                                    else
                                    {
                                        operacionObjeto.ClienteDragon = ClienteDragonfishPlataforma;
                                        LogHandler.EnviarMsj("", ErrorType.Minor, "Error al consultar el cliente Dragonfish: \nNúmero orden: " + order["number"].ToString() + "\n" + responseBuscarCliente.StatusDescription, origenVentas);
                                    }
                                }

                                var cupones = (JArray)order["coupon"];
                                decimal porcentaje = 0;
                                decimal valorFijo = 0;
                                try
                                {
                                    for (var c = 0; c < cupones.Count; c++)
                                    {
                                        switch (cupones[c]["type"].ToString())
                                        {
                                            case "percentage":
                                                porcentaje += Convert.ToDecimal(cupones[c]["value"].ToString(), new CultureInfo("en-US"));
                                                observaciones +="Codigo de cupon " + cupones[c]["code"].ToString();
                                                break;

                                            case "absolute":
                                                valorFijo = Convert.ToDecimal(cupones[c]["value"].ToString(), new CultureInfo("en-US"));
                                                observaciones += "Codigo de cupon " + cupones[c]["code"].ToString();
                                                break;

                                            case "shipping":
                                                observaciones += "Envio gratis. Codigo de cupon " + cupones[c]["code"].ToString();
                                                break;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    LogHandler.EnviarMsj("", ErrorType.Critical, "Error al procesar descuentos: \nNúmero orden: " + order["number"].ToString() + "\n" + e.Message, origenVentas);
                                    //errorOperacion = true;
                                }
                                
                                
                                operacionObjeto.DescuentoMonto2 = order["discount_coupon"].ToString();
                                
                                operacionObjeto.Obs = observaciones;
                                Console.WriteLine(operacionObjeto);

                                if (order["payment_status"].ToString() != "pending")
                                {
                                    operacionObjeto.Pagado = true;
                                    operacionObjeto.TipoComprobante = TipoComprobantePlatafomar;
                                }
                                else
                                {
                                    if (singleton.comprobanteSinPago == 1)
                                    {
                                        operacionObjeto.TipoComprobante = "Remito";

                                    }
                                    else if (singleton.comprobanteSinPago == 2)
                                    {
                                        operacionObjeto.TipoComprobante = "Pedido";
                                    }

                                    operacionObjeto.Pagado = false;
                                }
                                if (!errorOperacion)
                                {
                                    IRestResponse postOperacion = OperacionEcommerce(Method.POST, operacionObjeto, "", baseDeDatos);

                                    if (postOperacion.StatusCode == HttpStatusCode.Created)
                                    {
                                        detalle.Add(operacionObjeto);
                                        singleton.actualizarOrdenes(Convert.ToDateTime(order["created_at"].ToString()).AddSeconds(1));
                                        cantidadDeOperaciones++;
                                    }
                                    else
                                    {
                                        if (postOperacion.StatusDescription == "La combinación de plataforma y número de operación ya existe")
                                        {
                                            singleton.actualizarOrdenes(Convert.ToDateTime(order["created_at"].ToString()).AddSeconds(1));
                                        }
                                        else
                                        {
                                            LogHandler.EnviarMsj("", ErrorType.Critical, "Error al crear el Operación Ecommerce: \nNúmero orden: " + order["number"].ToString() + "\n" + postOperacion.StatusDescription + "\n\nOperación cancelada.", origenVentas);
                                            notification.MessageIconTask(postOperacion.StatusDescription);
                                            errorOperacion = true;
                                        }
                                    }
                                }
                            }

                            if (errorOperacion)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        LogHandler.EnviarMsj("", ErrorType.Critical, "PlataformaEcommerce: \nNo se encontro la Plataforma " + singleton.plataformaEcommerce, origenVentas);
                        notification.ErrorIcon("PlataformaEcommerce: \nNo se encontro la Plataforma " + singleton.plataformaEcommerce);
                    }
               }
                catch (Exception e)
                {
                    LogHandler.EnviarMsj("", ErrorType.Critical, "descargarAOperacionDeEcommerce: \n" + e.Message, origenVentas);

                }
            }

            switch (cantidadDeOperaciones)
            {
                case 1:
                    LogHandler.EnviarMsj("", ErrorType.Information, "Se descargó " + cantidadDeOperaciones + " Operación Ecommerce.", origenVentas);
                    notification.MessageIconTask("Se descargó " + cantidadDeOperaciones + " Operación Ecommerce.");
                    break;

                case 0:
                    LogHandler.EnviarMsj("", ErrorType.Information, "No se encontraron novedades.", origenVentas);
                    break;

                default:
                    LogHandler.EnviarMsj("", ErrorType.Information, "Se descargaron " + cantidadDeOperaciones + " Operaciones Ecommerce.", origenVentas);
                    notification.MessageIconTask("Se descargaron " + cantidadDeOperaciones + " Operaciones Ecommerce.");
                    break;

            }

            if (cantidadDeOperaciones != 0)
            {
                dynamic herramienta = new JObject();

                herramienta.Ecommerce = singleton.plataformaEcommerce;
                herramienta.DetalleOperaciones = detalle;

                var herramientaEcommerce = HerrmientaEcommerce(herramienta);
                
                if (herramientaEcommerce.StatusCode == HttpStatusCode.Created)
                {
                    LogHandler.EnviarMsj("", ErrorType.Information, "Se creo exitosamente la Herramienta de Generación de Comprobantes.", origenVentas);
                }
                else if (herramientaEcommerce.StatusCode.ToString()=="0")
                {
                    LogHandler.EnviarMsj("", ErrorType.Critical, "Error al crear la Herramienta de Generación de Comprobantes: \n" + herramientaEcommerce.StatusCode + ": Posible TimeOut", origenVentas);
                    notification.MessageIconTask("Error al crear la Herramienta de Generación de Comprobantes.");
                }
                else 
                {
                    Program.ConsoleLog(herramientaEcommerce.Headers[6].ToString());

                    LogHandler.EnviarMsj("", ErrorType.Critical, "Error al crear la Herramienta de Generación de Comprobantes: \n" +  herramientaEcommerce.StatusCode + ": " +  herramientaEcommerce.StatusDescription + "\n" + herramientaEcommerce.Headers[6].ToString(), origenVentas);
                    notification.MessageIconTask("Error al crear la Herramienta de Generación de Comprobantes.");
                }
            }
        }

        private IRestResponse ClienteEcommerce(Method method, JToken datosCliente, string idClienteEcommerce, string baseDeDatos)
        {
            var url = "";
            if(method == Method.PUT)
            {
                url = idClienteEcommerce;
            }

            var cliente = new RestClient(singleton.urlDragonfish + "/api.Dragonfish/ClienteEcommerce/" + url + "/");
            var request = new RestRequest(method);

            if (method == Method.GET)
            {
                request.AddParameter("Cuentaecommerce", singleton.plataformaEcommerce);
                request.AddParameter("ClienteEcommerce", idClienteEcommerce);
            }

            request.AddHeader("idCliente", singleton.clienteDragonfish);
            request.AddHeader("Authorization", singleton.tokenDragonfish);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("BaseDeDatos", baseDeDatos);

            if (method == Method.PUT || method == Method.POST)
            {
                request.AddParameter("application/json", datosCliente, ParameterType.RequestBody);
            };

            return cliente.Execute(request);
        }

        private IRestResponse OperacionEcommerce(Method method, JToken detalleRemito, string numero, string baseDeDatos)
        {
            var pedido = new RestClient(singleton.urlDragonfish + "/api.Dragonfish/Operacionecommerce/");
            var request = new RestRequest(method);

            if (method == Method.GET)
            {
                request.AddParameter("Ecommerce", singleton.plataformaEcommerce);
                request.AddParameter("Numero", numero);
            }
            request.AddHeader("idCliente", singleton.clienteDragonfish);
            request.AddHeader("Authorization", singleton.tokenDragonfish);
            request.AddHeader("BaseDeDatos", baseDeDatos);
            request.AddHeader("Content-Type", "application/json");

            if (method == Method.PUT || method == Method.POST)
            {
                request.AddParameter("application/json", detalleRemito, ParameterType.RequestBody);
            };

            var response = pedido.Execute(request);

            return response;

        }

        private IRestResponse PlataformaEcommerce(Method method, string baseDeDatos)
        {
            var pedido = new RestClient(singleton.urlDragonfish + "/api.Dragonfish/Ecommerce/" + singleton.plataformaEcommerce + "/");
            var request = new RestRequest(method);

            request.AddHeader("idCliente", singleton.clienteDragonfish);
            request.AddHeader("Authorization", singleton.tokenDragonfish);
            request.AddHeader("BaseDeDatos", baseDeDatos);
            request.AddHeader("Content-Type", "application/json");

            var response = pedido.Execute(request);
            return response;

        }

        private IRestResponse HerrmientaEcommerce(JToken detalle)
        {
            var url = new RestClient(singleton.urlDragonfish + "/api.Dragonfish/Comprobantesecommerce/");
            var request = new RestRequest(Method.POST);

            request.Timeout = 20 * 60000;
            request.AddHeader("idCliente", singleton.clienteDragonfish);
            request.AddHeader("Authorization", singleton.tokenDragonfish);
            request.AddHeader("BaseDeDatos", singleton.basesDeDatos);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", detalle, ParameterType.RequestBody);

            var response = url.Execute(request);
            return response;
        }

        private IRestResponse Cliente(Method method, JToken datosCliente, string baseDeDatos)
        {
            var cliente = new RestClient(singleton.urlDragonfish + "/api.Dragonfish/Cliente/");
            var request = new RestRequest(method);
            if (method == Method.GET)
            {
                request.AddParameter("query",datosCliente["EMail"].ToString());
            }
            request.AddHeader("idCliente", singleton.clienteDragonfish);
            request.AddHeader("Authorization", singleton.tokenDragonfish);
            request.AddHeader("Content-Type", "application/json");

            request.AddHeader("BaseDeDatos", baseDeDatos);
            if (method == Method.PUT || method == Method.POST)
            {
                request.AddParameter("application/json", datosCliente, ParameterType.RequestBody);
            };

            return cliente.Execute(request);
        }

        private IRestResponse StockOnline(int page, string producto,bool fechaFiltro)
        {
            var getStockOnline = new RestClient("https://api.znube.com.ar:8081/Omnichannel/GetStock/");
            var request = new RestRequest(Method.GET);

            if(producto != ""){
                request.AddParameter("ProductId", producto);
            }
            else
            {
                if (fechaFiltro)
                {
                    request.AddParameter("pruebaFecha", singleton.fechaStock);
                }

            }

            request.AddParameter("Offset", page+"00");
            request.AddParameter("limit", "100");
            request.AddParameter("Resources",singleton.resourceId);
            request.AddHeader("zNube-token", singleton.tokenStockOnline);
            var response = getStockOnline.Execute(request);

            return response;
        }

        private IRestResponse Orders(Method method,int page,int per_page)
        {
            var client = new RestClient("https://api.tiendanube.com/v1/" + singleton.clienteTiendaNube + "/orders/");
            client.Timeout = -1;
            var request = new RestRequest(method);
            request.AddParameter("per_page", per_page);
            request.AddParameter("page", page);
            request.AddParameter("created_at_min", singleton.fechaOrden.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss"));
            request.AddHeader("User-Agent", " Dragon (brunodecorneliis@gmail.com)");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authentication", singleton.tokenTiendaNube);
            IRestResponse response = client.Execute(request);
            return response;
        }

        private IRestResponse Variants(Method method, JToken detalleProduct, string id)
        {
            var variant = "";
            if (method == Method.PUT || method == Method.DELETE)
            {
                variant = "/" + detalleProduct["id"].ToString();
            }
            var client = new RestClient("https://api.tiendanube.com/v1/" + singleton.clienteTiendaNube + "/products/" + id + "/variants" + variant);
            client.Timeout = -1;
            var request = new RestRequest(method);
            request.AddHeader("User-Agent", " Dragon (brunodecorneliis@gmail.com)");
            request.AddHeader("Authentication", singleton.tokenTiendaNube);
            request.AddHeader("Content-Type", "application/json");

            if (method == Method.PUT || method == Method.POST)
            {
                request.AddParameter("application/json", detalleProduct, ParameterType.RequestBody);
            };

            IRestResponse response = client.Execute(request);

            return response;
        }
       
        private IRestResponse Metafield(string owner_id,string created_at_min,int page)
        {
            var client = new RestClient("https://api.tiendanube.com/v1/" + singleton.clienteTiendaNube + "/metafields/products/");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);

            if (owner_id != "")
            {
                request.AddParameter("owner_id", owner_id);
            }

            if (created_at_min != "")
            {
                request.AddParameter("created_at_min", created_at_min);
            }

            request.AddParameter("key", "ProductId");
            request.AddParameter("per_page", "200");
            request.AddParameter("page", page);
            request.AddHeader("User-Agent", " Dragon (brunodecorneliis@gmail.com)");
            request.AddHeader("Authentication", singleton.tokenTiendaNube);
            request.AddHeader("Content-Type", "application/json");

            IRestResponse response = client.Execute(request);

            return response;
        }

    }
}
