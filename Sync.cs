using Dragonfish_TN.Request;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Dragonfish_TN
{
    internal class Sync
    {
        private static Singleton singleton;

        private Notification notification = null;

        public bool running = false;

        private JArray arrayProductos = new JArray();

        public static Timer InternalTimer;

        private static Sync _instance;

        private string origenStock = "Publicación de Stock";

        private string origenVentas = "Descarga de Operaciones";

        public string tareasEnProceso = "Aguardando Tareas";

        private JArray arrayProductosVariantesInexistentes = new JArray();

        private JArray arrayProductosMovStock = new JArray();

        private bool primerMandato = true;

        private bool protPrimerMandatoNoActivado = true;

        private string ArticuloEnvioPlataforma = "";

        private string TipoComprobantePlatafomar = "";

        private string BaseDeDatosPlataforma = "";

        private string ClienteDragonfishPlataforma = "";

        public static Sync Instance
        {
            get
            {
                if ( Sync._instance == null )
                {
                    Sync._instance = new Sync();
                }
                return Sync._instance;
            }
        }

        static Sync()
        {
            Sync.singleton = null;
            Sync._instance = null;
        }

        public Sync()
        {
        }

        private string BuscarDescripcionComprobante( string codigo )
        {
            string str;
            string str1 = codigo;
            if ( str1 == null )
            {
                str = "null";
                return str;
            }
            else if ( str1 == "1" )
            {
                str = "FACTURA";
            }
            else if ( str1 == "2" )
            {
                str = "TICKETFACTURA";
            }
            else if ( str1 == "11" )
            {
                str = "REMITO";
            }
            else if ( str1 == "12" )
            {
                str = "DEVOLUCION";
            }
            else if ( str1 == "23" )
            {
                str = "PEDIDO";
            }
            else
            {
                if ( str1 != "27" )
                {
                    str = "null";
                    return str;
                }
                str = "FACTURAELECTRONICA";
            }
            return str;
        }

        public void cancelTimer()
        {
            try
            {
                Sync.InternalTimer.Dispose();
                while ( this.running )
                {
                }
                LogHandler.EnviarMsj( "", ErrorType.Minor, "Reiniciando.", Sync.singleton.origenDFTN );
                Task.Run( () => Program.iniciarApp() );
            }
            catch
            {
            }
        }

        private void ComprobarNuevosArticulos()
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime dateTime = Sync.singleton.fechaProductos.AddHours( 3 );
                this.ObtenerListaProductos( dateTime.ToString( "yyyy-MM-dd HH:mm:ss" ) );
                if ( this.arrayProductos.Count != 0 )
                {
                    LogHandler.EnviarMsj( "", ErrorType.Information, "Obteniendo lista de nuevos productos de Tienda Nube.", this.origenStock );
                    foreach ( JToken arrayProducto in this.arrayProductos )
                    {
                        this.PublicarStock( arrayProducto["value"].ToString(), false );
                    }
                }
                Sync.singleton.ActualizarUltimaFechaProducto( now );
            }
            catch ( Exception exception )
            {
                throw exception;
            }
        }

        private void ControlarErrorOrdenes( string orden, string error )
        {
            try
            {
                string str = "[]";
                if ( !File.Exists( Sync.singleton.log ) )
                {
                    Sync.singleton.ActualizarLogErrorOperaciones( new JArray() );
                }
                else
                {
                    str = File.ReadAllText( Sync.singleton.log );
                }
                JArray jArray = JArray.Parse( str );
                bool flag = false;
                if ( jArray.Count == 0 )
                {
                    JObject jObject = new JObject();
                    jObject.Add( new JProperty( "Orden", orden ) );
                    jObject.Add( new JProperty( "Error", new JArray( error ) ) );
                    jArray.Add( jObject );
                }
                else
                {
                    int num = 0;
                    while ( num < jArray.Count )
                    {
                        if ( jArray[num]["Orden"].ToString() != orden )
                        {
                            num++;
                        }
                        else
                        {
                            JArray item = (JArray)jArray[num]["Error"];
                            item.Add( error );
                            jArray[num]["Error"] = JToken.FromObject( item );
                            flag = true;
                            break;
                        }
                    }
                }
                if ( !flag )
                {
                    JObject jObject1 = new JObject();
                    jObject1.Add( new JProperty( "Orden", orden ) );
                    jObject1.Add( new JProperty( "Error", new JArray( error ) ) );
                    jArray.Add( jObject1 );
                }
                Sync.singleton.ActualizarLogErrorOperaciones( jArray );
            }
            catch ( Exception exception1 )
            {
                Exception exception = exception1;
                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "ControlarErrorOrdenes: \n", exception.Message ), this.origenVentas );
            }
        }

        private void crearCombinacion( string sku )
        {
            HttpStatusCode statusCode;
            bool flag = true;
            foreach ( JToken jToken in this.arrayProductosMovStock )
            {
                if ( jToken.ToString() == sku )
                {
                    flag = false;
                }
            }
            if ( flag )
            {
                this.arrayProductosMovStock.Add( sku );
                if ( ConsultaStockYPrecios.Response( sku.Replace( "#", "!" ) ).StatusCode == HttpStatusCode.NotFound )
                {
                    LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( "Se genera Movimiento de Stock para crear combinación ", sku ), this.origenStock );
                    try
                    {
                        string str = sku.Split( new char[] { '#' } )[0];
                        string str1 = sku.Split( new char[] { '#' } )[1];
                        string str2 = sku.Split( new char[] { '#' } )[2];
                        dynamic jObject = new JObject();
                        jObject.Articulo = str;
                        jObject.Color = str1;
                        jObject.Talle = str2;
                        jObject.Cantidad = 1;
                        JArray jArray = new JArray();
                        jArray.Add( jObject );
                        dynamic obj = new JObject();
                        obj.OrigenDestino = Sync.singleton.baseDeDatos;
                        obj.Tipo = 1;
                        obj.MovimientoDetalle = jArray;
                        obj.Observacion = "Movimiento generado automáticamente por api.\nDescarga de Ordenes de Tienda Nube.";
                        IRestResponse restResponse = (IRestResponse)MovStock.Response( obj );
                        if ( restResponse.StatusCode != HttpStatusCode.Created )
                        {
                            string[] statusDescription = new string[] { "Error al crear Movimiento de Stock: \nSKU ", sku, "\n", null, null };
                            statusCode = restResponse.StatusCode;
                            statusDescription[3] = statusCode.ToString();
                            statusDescription[4] = restResponse.StatusDescription;
                            LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( statusDescription ), this.origenVentas );
                        }
                        else
                        {
                            dynamic jObject1 = new JObject();
                            jObject1.OrigenDestino = Sync.singleton.baseDeDatos;
                            jObject1.Tipo = 2;
                            jObject1.MovimientoDetalle = jArray;
                            jObject1.Observacion = "Movimiento generado automáticamente por api.\nDescarga de Ordenes de Tienda Nube.";
                            IRestResponse restResponse1 = (IRestResponse)MovStock.Response( jObject1 );
                            if ( restResponse1.StatusCode != HttpStatusCode.Created )
                            {
                                string[] strArrays = new string[] { "Error al crear Movimiento de Stock: \nSKU ", sku, "\n", null, null };
                                statusCode = restResponse1.StatusCode;
                                strArrays[3] = statusCode.ToString();
                                strArrays[4] = restResponse1.StatusDescription;
                                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( strArrays ), this.origenVentas );
                            }
                        }
                    }
                    catch ( Exception exception1 )
                    {
                        Exception exception = exception1;
                        LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Error al crear Movimiento de Stock: \nSKU ", sku, "\n", exception.Message ), this.origenVentas );
                    }
                }
            }
        }

        private void DescargarOrdenes( string dateCreated, string dateUpdated )
        {
            if ( dateUpdated == "" )
            {
                LogHandler.EnviarMsj( "", ErrorType.Information, "Obteniendo ordenes de Tienda Nube.", this.origenVentas );
            }
            else
            {
                LogHandler.EnviarMsj( "", ErrorType.Information, "Obteniendo ordenes modificadas de Tienda Nube.", this.origenVentas );
            }
            JArray jArray = new JArray();
            HttpStatusCode statusCode = HttpStatusCode.OK;
            int num = 1;
            DateTime now = DateTime.Now;
            while ( statusCode == HttpStatusCode.OK )
            {
                int num1 = num;
                num = num1 + 1;
                IRestResponse restResponse = Order.Response( 0, num1, 10, dateCreated, dateUpdated );
                statusCode = restResponse.StatusCode;
                if ( statusCode == HttpStatusCode.OK )
                {
                    foreach ( JToken jToken in JArray.Parse( restResponse.Content ) )
                    {
                        if ( ( jToken["status"].ToString() != "cancelled" ? true : dateUpdated != "" ) )
                        {
                            jArray.Add( jToken );
                        }
                    }
                    Thread.Sleep( 10000 );
                }
            }
            if ( jArray.Count == 0 )
            {
                if ( dateUpdated == "" )
                {
                    LogHandler.EnviarMsj( "", ErrorType.Information, "No se encontraron comprobantes.", this.origenVentas );
                }
                else
                {
                    LogHandler.EnviarMsj( "", ErrorType.Information, "No se encontraron comprobantes modificados.", this.origenVentas );
                }
            }
            else if ( dateUpdated != "" )
            {
                this.ModificarOrdenes( jArray );
                Sync.singleton.ActualizarFechaOrdenModificada( now );
            }
            else if ( dateCreated != "" )
            {
                this.GrabarOrdenes( jArray );
            }
        }

        private void GrabarOrdenes( JArray arrayOrders )
        {
            HttpStatusCode statusCode;
            string str;
            string str1;
            string str2;
            string str3;
            string str4;
            string str5;
            string str6;
            string str7;
            string str8;
            string str9;
            string str10;
            string str11;
            string str12;
            string str13;
            string str14;
            string str15;
            string str16;
            string str17;
            string str18;
            string str19;
            string str20;
            string str21;
            string str22;
            string str23;
            string str24;
            int num = 0;
            JArray jArray = new JArray();
            JArray jArray1 = new JArray(
                from obj in arrayOrders
                orderby (int)obj["number"]
                select obj );


            jArray1[0]["number"].ToString();

            try
            {
                LogHandler.EnviarMsj( "", ErrorType.Information, "Procesando ventas.", this.origenVentas );
                int num1 = 0;
                while ( num1 < jArray1.Count )
                {
                    bool flag = false;
                    JToken item = jArray1[num1];
                    dynamic jObject = new JObject();
                    jObject.BaseDeDatos = this.BaseDeDatosPlataforma;
                    string str25 = item["number"].ToString();
                    JToken jToken = item["number"];
                    if ( jToken != null )
                    {
                        str = jToken.ToString();
                    }
                    else
                    {
                        str = null;
                    }
                    string str26 = string.Concat( "Orden nº: ", str );
                    if ( OperacionEcom.Response( 0, null, str25 ).StatusCode == HttpStatusCode.NotFound )
                    {
                        jObject.Ecommerce = Sync.singleton.plataformaEcommerce;
                        jObject.Numero = item["number"];
                        jObject.NumeroOperacion = item["number"];
                        try
                        {
                            if ( ( item["shipping_address"] == null ? false : item["shipping_address"].ToString() != "" ) )
                            {
                                string[] strArrays = new string[15];
                                JToken item1 = item["shipping_address"]["address"];
                                if ( item1 != null )
                                {
                                    str14 = item1.ToString();
                                }
                                else
                                {
                                    str14 = null;
                                }
                                strArrays[0] = str14;
                                strArrays[1] = " ";
                                JToken jToken1 = item["shipping_address"]["number"];
                                if ( jToken1 != null )
                                {
                                    str15 = jToken1.ToString();
                                }
                                else
                                {
                                    str15 = null;
                                }
                                strArrays[2] = str15;
                                strArrays[3] = " ";
                                JToken item2 = item["shipping_address"]["floor"];
                                if ( item2 != null )
                                {
                                    str16 = item2.ToString();
                                }
                                else
                                {
                                    str16 = null;
                                }
                                strArrays[4] = str16;
                                strArrays[5] = " - CP:";
                                JToken jToken2 = item["shipping_address"]["zipcode"];
                                if ( jToken2 != null )
                                {
                                    str17 = jToken2.ToString();
                                }
                                else
                                {
                                    str17 = null;
                                }
                                strArrays[6] = str17;
                                strArrays[7] = " ";
                                JToken item3 = item["shipping_address"]["city"];
                                if ( item3 != null )
                                {
                                    str18 = item3.ToString();
                                }
                                else
                                {
                                    str18 = null;
                                }
                                strArrays[8] = str18;
                                strArrays[9] = " ";
                                JToken jToken3 = item["shipping_address"]["province"];
                                if ( jToken3 != null )
                                {
                                    str19 = jToken3.ToString();
                                }
                                else
                                {
                                    str19 = null;
                                }
                                strArrays[10] = str19;
                                strArrays[11] = ".Nombre: ";
                                JToken item4 = item["shipping_address"]["name"];
                                if ( item4 != null )
                                {
                                    str20 = item4.ToString();
                                }
                                else
                                {
                                    str20 = null;
                                }
                                strArrays[12] = str20;
                                strArrays[13] = ".Tel: ";
                                JToken jToken4 = item["shipping_address"]["phone"];
                                if ( jToken4 != null )
                                {
                                    str21 = jToken4.ToString();
                                }
                                else
                                {
                                    str21 = null;
                                }
                                strArrays[14] = str21;
                                string str27 = string.Concat( strArrays );
                                dynamic obj1 = jObject;
                                str22 = ( str27.Length <= 100 ? str27 : str27.Substring( 0, 100 ) );
                                obj1.DatosEnvio = str22;
                            }
                        }
                        catch ( Exception exception1 )
                        {
                            Exception exception = exception1;
                            this.ControlarErrorOrdenes( item["number"].ToString(), string.Concat( "Error al generar datos de envio: ", exception.Message ) );
                            LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Error al generar datos de envio: \nNúmero orden: ", item["number"].ToString(), "\n", exception.Message ), this.origenVentas );
                        }
                        try
                        {
                            string[] strArrays1 = new string[7];
                            JToken item5 = item["billing_address"];
                            if ( item5 != null )
                            {
                                str1 = item5.ToString();
                            }
                            else
                            {
                                str1 = null;
                            }
                            strArrays1[0] = str1;
                            strArrays1[1] = " ";
                            JToken jToken5 = item["billing_number"];
                            if ( jToken5 != null )
                            {
                                str2 = jToken5.ToString();
                            }
                            else
                            {
                                str2 = null;
                            }
                            strArrays1[2] = str2;
                            strArrays1[3] = " ";
                            JToken item6 = item["billing_floor"];
                            if ( item6 != null )
                            {
                                str3 = item6.ToString();
                            }
                            else
                            {
                                str3 = null;
                            }
                            strArrays1[4] = str3;
                            strArrays1[5] = " - CP:";
                            JToken jToken6 = item["billing_zipcode"];
                            if ( jToken6 != null )
                            {
                                str4 = jToken6.ToString();
                            }
                            else
                            {
                                str4 = null;
                            }
                            strArrays1[6] = str4;
                            string str28 = string.Concat( strArrays1 );
                            dynamic obj2 = jObject;
                            str5 = ( str28.Length <= 40 ? str28 : str28.Substring( 0, 40 ) );
                            obj2.DatosPago = str5;
                        }
                        catch ( Exception exception3 )
                        {
                            Exception exception2 = exception3;
                            this.ControlarErrorOrdenes( item["number"].ToString(), string.Concat( "Error al generar datos de pago: ", exception2.Message ) );
                            LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Error al generar datos de pago: \nNúmero orden: ", item["number"].ToString(), "\n", exception2.Message ), this.origenVentas );
                        }
                        JArray jArray2 = new JArray();
                        JArray jArray3 = (JArray)item["products"];
                        for ( int i = 0; i < jArray3.Count; i++ )
                        {
                            bool flag1 = false;
                            dynamic jObject1 = new JObject();
                            JToken item7 = jArray3[i];
                            IRestResponse restResponse = Metafield.Response( item7["product_id"].ToString(), "", 1 );
                            try
                            {
                                while ( restResponse.StatusCode != HttpStatusCode.OK )
                                {
                                    this.ProtocoloNoHacerNada();
                                    restResponse = Metafield.Response( item7["product_id"].ToString(), "", 1 );
                                }
                                if ( JArray.Parse( restResponse.Content ).Count == 0 )
                                {
                                    string str29 = item["number"].ToString();
                                    string[] strArrays2 = new string[] { "Obtener producto de Tienda Nube: Producto ", null, null, null, null };
                                    JToken jToken7 = item7["product_id"];
                                    if ( jToken7 != null )
                                    {
                                        str6 = jToken7.ToString();
                                    }
                                    else
                                    {
                                        str6 = null;
                                    }
                                    strArrays2[1] = str6;
                                    strArrays2[2] = ". Descripcion ";
                                    JToken item8 = item7["name"];
                                    if ( item8 != null )
                                    {
                                        str7 = item8.ToString();
                                    }
                                    else
                                    {
                                        str7 = null;
                                    }
                                    strArrays2[3] = str7;
                                    strArrays2[4] = " inexistente.";
                                    this.ControlarErrorOrdenes( str29, string.Concat( strArrays2 ) );
                                    string[] strArrays3 = new string[] { "Obtener productos del Metafield: \nNúmero orden: ", item["number"].ToString(), "\nProducto ", null, null };
                                    JToken jToken8 = item7["product_id"];
                                    if ( jToken8 != null )
                                    {
                                        str8 = jToken8.ToString();
                                    }
                                    else
                                    {
                                        str8 = null;
                                    }
                                    strArrays3[3] = str8;
                                    strArrays3[4] = " inexistente.";
                                    LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( strArrays3 ), this.origenVentas );
                                    flag1 = true;
                                }
                                else
                                {
                                    JToken item9 = JArray.Parse( restResponse.Content )[0]["value"];
                                    string str30 = item7["sku"].ToString().Replace( "-_-", "#" );
                                    string str31 = str30.Split( new char[] { '#' } )[0];
                                    string str32 = str30.Split( new char[] { '#' } )[1];
                                    jObject1.IDPublicacion = item7["product_id"];
                                    jObject1.Articulo = item9;
                                    jObject1.Color = str31;
                                    jObject1.Talle = str32;
                                    jObject1.Precio = item7["price"];
                                    jObject1.Cantidad = item7["quantity"];
                                    jArray2.Add( jObject1 );
                                }
                            }
                            catch ( Exception exception5 )
                            {
                                Exception exception4 = exception5;
                                string str33 = item["number"].ToString();
                                string[] message = new string[] { "Obtener producto de Tienda Nube: Producto ", null, null, null, null, null };
                                JToken jToken9 = item7["product_id"];
                                if ( jToken9 != null )
                                {
                                    str23 = jToken9.ToString();
                                }
                                else
                                {
                                    str23 = null;
                                }
                                message[1] = str23;
                                message[2] = ". Descripcion ";
                                JToken item10 = item7["name"];
                                if ( item10 != null )
                                {
                                    str24 = item10.ToString();
                                }
                                else
                                {
                                    str24 = null;
                                }
                                message[3] = str24;
                                message[4] = " ";
                                message[5] = exception4.Message;
                                this.ControlarErrorOrdenes( str33, string.Concat( message ) );
                                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Obtener productos del Metafield: \nNúmero orden: ", item["number"].ToString(), "\n", exception4.Message ), this.origenVentas );
                                flag1 = true;
                            }
                            if ( flag1 )
                            {
                                jObject1.IDPublicacion = item7["product_id"];
                                jObject1.Articulo = Sync.singleton.articuloGenerico;
                                jObject1.Precio = item7["price"];
                                jObject1.Cantidad = item7["quantity"];
                                jArray2.Add( jObject1 );
                            }
                        }
                        try
                        {
                            if ( Sync.singleton.descargarEnvio )
                            {
                                if ( item["shipping"].ToString() != "mercado-envios" )
                                {
                                    if ( item["shipping_cost_customer"].ToString() != "0.00" )
                                    {
                                        dynamic articuloEnvioPlataforma = new JObject();
                                        articuloEnvioPlataforma.Articulo = this.ArticuloEnvioPlataforma;
                                        articuloEnvioPlataforma.Precio = item["shipping_cost_customer"];
                                        articuloEnvioPlataforma.IDPublicacion = "ENVIO";
                                        articuloEnvioPlataforma.Cantidad = 1;
                                        jArray2.Add( articuloEnvioPlataforma );
                                    }
                                }
                            }
                        }
                        catch ( Exception exception7 )
                        {
                            Exception exception6 = exception7;
                            this.ControlarErrorOrdenes( item["number"].ToString(), string.Concat( "Error al procesar Envio: ", exception6.Message ) );
                            LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Error al procesar Envio: \nNúmero orden: ", item["number"].ToString(), "\n", exception6.Message ), this.origenVentas );
                        }
                        jObject.PublicacionDetalle = jArray2;
                        try
                        {
                            JToken jToken10 = item["customer"];
                            IRestResponse restResponse1 = ClienteEcom.Response( 0, null, jToken10["id"].ToString() );
                            string str34 = "";
                            dynamic jObject2 = new JObject();
                            jObject2.Cuentaecommerce = Sync.singleton.plataformaEcommerce;
                            jObject2.ClienteEcommerce = jToken10["id"].ToString();
                            jObject2.Nombre = jToken10["name"].ToString().Substring( 0, Math.Min( 40, jToken10["name"].ToString().Length ) );
                            jObject2.Mail = jToken10["email"].ToString();
                            jObject2.Telefono = jToken10["phone"].ToString();
                            jObject2.NroDoc = jToken10["identification"].ToString().Substring( 0, Math.Min( 40, jToken10["identification"].ToString().Length ) );
                            jObject2.ClienteDragon = "";
                            jObject2.PrimerNombre = jToken10["name"].ToString().Substring( 0, Math.Min( 60, jToken10["name"].ToString().Length ) );
                            jObject2.NroDocumento = jToken10["identification"].ToString().Substring( 0, Math.Min( 10, jToken10["identification"].ToString().Length ) );
                            jObject2.EMail = jToken10["email"].ToString();
                            jObject2.Movil = jToken10["phone"].ToString().Substring( 0, Math.Min( 30, jToken10["phone"].ToString().Length ) );
                            if ( ( item["billing_address"] == null ? false : item["billing_address"].ToString() != "" ) )
                            {
                                dynamic obj3 = jObject2;
                                str13 = ( item["billing_address"].ToString().Length <= 70 ? item["billing_address"].ToString() : item["billing_address"].ToString().Substring( 0, 70 ) );
                                obj3.Calle = str13;
                            }
                            if ( ( item["billing_number"] == null ? false : item["billing_number"].ToString() != "" ) )
                            {
                                try
                                {
                                    dynamic obj4 = jObject2;
                                    str12 = ( item["billing_number"].ToString().Length <= 5 ? item["billing_number"].ToString() : item["billing_number"].ToString().Substring( 0, 5 ) );
                                    obj4.Numero = int.Parse( str12 );
                                }
                                catch
                                {
                                }
                            }
                            if ( ( item["billing_floor"] == null ? false : item["billing_floor"].ToString() != "" ) )
                            {
                                dynamic obj6 = jObject2;
                                str11 = ( item["billing_floor"].ToString().Length <= 3 ? item["billing_floor"].ToString() : item["billing_floor"].ToString().Substring( 0, 3 ) );
                                obj6.Piso = str11;
                            }
                            if ( ( item["billing_locality"] == null ? false : item["billing_locality"].ToString() != "" ) )
                            {
                                dynamic obj7 = jObject2;
                                str10 = ( item["billing_locality"].ToString().Length <= 70 ? item["billing_locality"].ToString() : item["billing_locality"].ToString().Substring( 0, 70 ) );
                                obj7.Localidad = str10;
                            }
                            if ( ( item["billing_zipcode"] == null ? false : item["billing_zipcode"].ToString() != "" ) )
                            {
                                dynamic obj8 = jObject2;
                                str9 = ( item["billing_zipcode"].ToString().Length <= 8 ? item["billing_zipcode"].ToString() : item["billing_zipcode"].ToString().Substring( 0, 8 ) );
                                obj8.CodigoPostal = str9;
                            }
                            if ( ( item["billing_province"] == null ? false : item["billing_province"].ToString() != "" ) )
                            {
                                string str35 = item["billing_province"].ToString();
                                if ( str35 != null )
                                {
                                    if ( str35 == "Buenos Aires" )
                                    {
                                        jObject2.Provincia = "01";
                                        goto Label0;
                                    }
                                    else
                                    {
                                        if ( str35 != "Capital Federal" )
                                        {
                                            goto Label2;
                                        }
                                        jObject2.Provincia = "00";
                                        goto Label0;
                                    }
                                }
                                Label2:
                                string str36 = "";
                                try
                                {
                                    str36 = JObject.Parse( Provincias.Response( item["billing_province"].ToString() ).Content )["Resultados"][0]["Codigo"].ToString();
                                }
                                catch
                                {
                                }
                                jObject2.Provincia = str36;
                            }
                            Label0:
                            if ( restResponse1.StatusCode == HttpStatusCode.NotFound )
                            {
                                dynamic obj10 = ClienteEcom.Response( RestSharp.Method.POST, jObject2, "" );
                                if ( obj10.StatusCode != 201 )
                                {
                                    this.ControlarErrorOrdenes( item["number"].ToString(), "Error al crear el cliente ecommerce: " + obj10.StatusCode + "-" + obj10.StatusDescription );
                                    LogHandler.EnviarMsj( "", 0, string.Concat( "Error al crear el cliente ecommerce: \nNúmero orden: ", item["number"].ToString(), "\n" ) + obj10.StatusCode + "-" + obj10.StatusDescription, this.origenVentas );
                                }
                                else
                                {
                                    str34 = (string)JObject.Parse( obj10.Content )["Codigo"].ToString();
                                    dynamic obj11 = jObject;
                                    obj11.ClienteEcom = JObject.Parse( obj10.Content )["Codigo"].ToString();
                                }
                            }
                            else if ( restResponse1.StatusCode != HttpStatusCode.OK )
                            {
                                string str37 = item["number"].ToString();
                                statusCode = restResponse1.StatusCode;
                                this.ControlarErrorOrdenes( str37, string.Concat( "Error al obtener el cliente ecommerce: ", statusCode.ToString(), "-", restResponse1.StatusDescription ) );
                                string[] statusDescription = new string[] { "Error al obtener el cliente ecommerce: \nNúmero orden: ", item["number"].ToString(), "\n", null, null, null };
                                statusCode = restResponse1.StatusCode;
                                statusDescription[3] = statusCode.ToString();
                                statusDescription[4] = "-";
                                statusDescription[5] = restResponse1.StatusDescription;
                                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( statusDescription ), this.origenVentas );
                            }
                            else
                            {
                                str34 = JObject.Parse( restResponse1.Content )["Resultados"][0]["Codigo"].ToString();
                                jObject.ClienteEcom = JObject.Parse( restResponse1.Content )["Resultados"][0]["Codigo"].ToString();
                                jObject2.ClienteDragon = JObject.Parse( restResponse1.Content )["Resultados"][0]["ClienteDragon"].ToString();
                                jObject2.EMail = JObject.Parse( restResponse1.Content )["Resultados"][0]["Mail"].ToString();
                            }
                            if ( jObject2.ClienteDragon == "" )
                            {
                                dynamic obj12 = Cliente.Response( 0, jObject2 );
                                if ( obj12.StatusCode == 200 )
                                {
                                    try
                                    {
                                        dynamic obj13 = jObject2;
                                        obj13.ClienteDragon = JObject.Parse( obj12.Content )["Resultados"][0]["Codigo"];
                                        dynamic obj14 = ClienteEcom.Response( RestSharp.Method.PUT, jObject2, str34 );
                                        if ( obj14.StatusCode != 200 )
                                        {
                                            LogHandler.EnviarMsj( "", 0, string.Concat( "Error al asignar el cliente Dragonfish al cliente Ecommerce: \nNúmero orden: ", item["number"].ToString(), "\n" ) + obj14.StatusCode + "-" + obj14.StatusDescription, this.origenVentas );
                                        }
                                    }
                                    catch ( Exception exception9 )
                                    {
                                        Exception exception8 = exception9;
                                        jObject.ClienteDragon = this.ClienteDragonfishPlataforma;
                                        LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Error al obtener el cliente Dragonfish: \nNúmero orden: ", item["number"].ToString(), "\n", exception8.Message ), this.origenVentas );
                                    }
                                }
                                else if ( obj12.StatusCode != 404 )
                                {
                                    jObject.ClienteDragon = this.ClienteDragonfishPlataforma;
                                    LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( "Error al buscar el cliente Dragonfish: \nNúmero orden: ", item["number"].ToString(), "\n" ) + obj12.StatusDescription, this.origenVentas );
                                }
                                else
                                {
                                    dynamic obj15 = Cliente.Response( RestSharp.Method.POST, jObject2 );
                                    if ( obj15.StatusCode != 201 )
                                    {
                                        jObject.ClienteDragon = this.ClienteDragonfishPlataforma;
                                        this.ControlarErrorOrdenes( item["number"].ToString(), "Error al crear el cliente Dragonfish: " + obj15.StatusDescription );
                                        LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( "Error al crear el cliente Dragonfish: \nNúmero orden: ", item["number"].ToString(), "\n" ) + obj15.StatusDescription, this.origenVentas );
                                    }
                                    else
                                    {
                                        try
                                        {
                                            dynamic obj16 = jObject2;
                                            obj16.ClienteDragon = JObject.Parse( obj15.Content )["Codigo"];
                                            IRestResponse restResponse2 = (IRestResponse)ClienteEcom.Response( RestSharp.Method.PUT, jObject2, str34 );
                                            if ( restResponse2.StatusCode != HttpStatusCode.OK )
                                            {
                                                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Error al asignar el cliente Dragonfish al cliente Ecommerce: \nNúmero orden: ", item["number"].ToString(), "\n", restResponse2.StatusDescription ), this.origenVentas );
                                            }
                                        }
                                        catch ( Exception exception11 )
                                        {
                                            Exception exception10 = exception11;
                                            jObject.ClienteDragon = this.ClienteDragonfishPlataforma;
                                            this.ControlarErrorOrdenes( item["number"].ToString(), string.Concat( "Error al crear el cliente Dragonfish: ", exception10.Message ) );
                                            LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( "Error al crear el cliente Dragonfish: \nNúmero orden: ", item["number"].ToString(), "\n", exception10.Message ), this.origenVentas );
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            jObject.ClienteDragon = this.ClienteDragonfishPlataforma;
                            this.ControlarErrorOrdenes( item["number"].ToString(), string.Concat( "Error al generar cliente: Número orden: ", item["number"].ToString(), ". No se completaron los datos de Cliente" ) );
                            LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( "Error al generar cliente: \nNúmero orden: ", item["number"].ToString(), "\nNo se completaron los datos de Cliente" ), this.origenVentas );
                        }
                        JArray jArray4 = (JArray)item["coupon"];
                        try
                        {
                            for ( int j = 0; j < jArray4.Count; j++ )
                            {
                                string str38 = jArray4[j]["type"].ToString();
                                string str39 = str38;
                                if ( str39 != null )
                                {
                                    if ( str39 == "percentage" )
                                    {
                                        str26 = string.Concat( str26, Environment.NewLine, "Codigo de cupon ", jArray4[j]["code"].ToString() );
                                    }
                                    else if ( str39 == "absolute" )
                                    {
                                        str26 = string.Concat( str26, Environment.NewLine, "Codigo de cupon ", jArray4[j]["code"].ToString() );
                                    }
                                    else if ( str39 == "shipping" )
                                    {
                                        str26 = string.Concat( str26, Environment.NewLine, "Envio gratis. Codigo de cupon ", jArray4[j]["code"].ToString() );
                                    }
                                }
                            }
                        }
                        catch ( Exception exception13 )
                        {
                            Exception exception12 = exception13;
                            this.ControlarErrorOrdenes( item["number"].ToString(), string.Concat( "Error al procesar descuentos: ", exception12.Message ) );
                            LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Error al procesar descuentos: \nNúmero orden: ", item["number"].ToString(), "\n", exception12.Message ), this.origenVentas );
                        }
                        JArray jArray5 = (JArray)item["promotional_discount"]["promotions_applied"];
                        try
                        {
                            for ( int k = 0; k < jArray5.Count; k++ )
                            {
                                str26 = string.Concat( str26, Environment.NewLine, "Promoción ", jArray5[k]["scope_value_name"].ToString() );
                            }
                        }
                        catch ( Exception exception15 )
                        {
                            Exception exception14 = exception15;
                            this.ControlarErrorOrdenes( item["number"].ToString(), string.Concat( "Error al procesar promoción: ", exception14.Message ) );
                            LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Error al procesar promoción: \nNúmero orden: ", item["number"].ToString(), "\n", exception14.Message ), this.origenVentas );
                        }
                        jObject.DescuentoMonto2 = item["discount"].ToString();
                        jObject.Obs = str26;
                        if ( item["payment_status"].ToString() == "pending" )
                        {
                            switch ( Sync.singleton.comprobanteSinPago )
                            {
                                case 0:
                                    {
                                        jObject.TipoComprobante = this.TipoComprobantePlatafomar;
                                        break;
                                    }
                                case 1:
                                    {
                                        jObject.TipoComprobante = "Remito";
                                        break;
                                    }
                                case 2:
                                    {
                                        jObject.TipoComprobante = "Pedido";
                                        break;
                                    }
                            }
                            jObject.Pagado = false;
                        }
                        else
                        {
                            jObject.Pagado = true;
                            jObject.TipoComprobante = this.TipoComprobantePlatafomar;
                        }
                        if ( !flag )
                        {
                            IRestResponse restResponse3 = (IRestResponse)OperacionEcom.Response( RestSharp.Method.POST, jObject, "" );
                            if ( restResponse3.StatusCode == HttpStatusCode.Created )
                            {
                                string str40 = item["created_at"].ToString().Replace( "+0000", "+0300" );
                                Singleton singleton = Sync.singleton;
                                DateTime dateTime = Convert.ToDateTime( str40 );
                                singleton.ActualizarFechaUltimaOrden( dateTime.AddSeconds( 1 ) );
                                jArray.Add( jObject );
                                num++;
                            }
                            else if ( restResponse3.StatusCode.ToString() == "0" )
                            {
                                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Error al crear el Operación Ecommerce: \nNúmero orden: ", item["number"].ToString(), "\nTime-Out API Dragonfish" ), this.origenVentas );
                                this.ControlarErrorOrdenes( item["number"].ToString(), "Time-Out API Dragonfish" );
                            }
                            else if ( restResponse3.StatusDescription != "La combinación de plataforma y número de operación ya existe" )
                            {
                                string[] statusDescription1 = new string[] { "Error al crear el Operación Ecommerce: \nNúmero orden: ", item["number"].ToString(), "\n", null, null };
                                statusCode = restResponse3.StatusCode;
                                statusDescription1[3] = statusCode.ToString();
                                statusDescription1[4] = restResponse3.StatusDescription;
                                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( statusDescription1 ), this.origenVentas );
                                this.ControlarErrorOrdenes( item["number"].ToString(), restResponse3.StatusDescription );
                            }
                        }
                    }
                    if ( !flag )
                    {
                        num1++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch ( Exception exception17 )
            {
                Exception exception16 = exception17;
                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "descargarAOperacionDeEcommerce: \n", exception16.Message ), this.origenVentas );
                throw exception16;
            }
            int num2 = num;
            if ( num2 == 0 )
            {
                LogHandler.EnviarMsj( "", ErrorType.Information, "No se encontraron novedades.", this.origenVentas );
            }
            else if ( num2 == 1 )
            {
                LogHandler.EnviarMsj( "", ErrorType.Information, string.Concat( "Se descargó ", num.ToString(), " Operación Ecommerce." ), this.origenVentas );
                this.notification.MessageIconTask( string.Concat( "Se descargó ", num.ToString(), " Operación Ecommerce." ) );
            }
            else
            {
                LogHandler.EnviarMsj( "", ErrorType.Information, string.Concat( "Se descargaron ", num.ToString(), " Operaciones Ecommerce." ), this.origenVentas );
                this.notification.MessageIconTask( string.Concat( "Se descargaron ", num.ToString(), " Operaciones Ecommerce." ) );
            }
            try
            {
                if ( num != 0 )
                {
                    JArray jArray6 = new JArray();
                    this.ProcesarHerramienta( jArray, jArray6 );
                    if ( jArray6.Count != 0 )
                    {
                        this.ProcesarHerramienta( jArray6, new JArray() );
                    }
                }
            }
            catch ( Exception exception19 )
            {
                Exception exception18 = exception19;
                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Enviar Operaciones a Procesar: \n", exception18.Message ), this.origenVentas );
            }
        }

        private void InitProcess( object state )
        {
            if ( !this.running )
            {
                if ( ( DateTime.Now.Hour < 1 ? true : DateTime.Now.Hour > 7 ) )
                {
                    this.running = true;
                    this.StartProcess();
                }
            }
        }

        public static void ListadoOrdenesModificadas( string date, string tipo, string Message )
        {
            try
            {
                using ( StreamWriter streamWriter = new StreamWriter( string.Concat( tipo, "-", date, ".csv" ), true ) )
                {
                    DateTime now = DateTime.Now;
                    streamWriter.WriteLine( string.Concat( now.ToString( "yyyy-MM-dd HH:mm:ss" ), ";", Message ) );
                }
            }
            catch ( Exception exception )
            {
                string message = exception.Message;
            }
        }

        private void ModificarOrdenes( JArray arrayOrders )
        {
            bool flag;
            foreach ( JToken arrayOrder in arrayOrders )
            {
                if ( arrayOrder["status"].ToString() == "cancelled" )
                {
                    IRestResponse restResponse = OperacionEcom.Response( 0, null, arrayOrder["number"].ToString() );
                    if ( restResponse.StatusCode == HttpStatusCode.OK )
                    {
                        try
                        {
                            JToken item = JObject.Parse( restResponse.Content )["Resultados"][0]["CompAfec"];
                            if ( item.ToString() != "[]" )
                            {
                                item[0]["Afecta"].ToString();
                                string str = item[0]["TipoComprobante"].ToString();
                                string str1 = item[0]["tipoCompCatacter"].ToString();
                                dynamic jObject = new JObject();
                                jObject.TipoEntidadAfectada = str;
                                jObject.IdAplicacionCliente = "TN";
                                IRestResponse restResponse1 = (IRestResponse)NuevoEnBaseA.Response( "Devolucion", item[0]["Afecta"].ToString(), jObject );
                                if ( ( restResponse1.StatusCode == HttpStatusCode.OK ? false : restResponse1.StatusDescription != "El comprobante no posee artículos pendientes" ) )
                                {
                                    this.ProtocoloNoHacerNada();
                                    restResponse1 = (IRestResponse)NuevoEnBaseA.Response( "Devolucion", item[0]["Afecta"].ToString(), jObject );
                                    if ( restResponse1.StatusCode != HttpStatusCode.OK )
                                    {
                                        LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( "Error al procesar Cancelacion en base a ", str1, "\n", restResponse1.StatusDescription ), this.origenVentas );
                                    }
                                }
                                continue;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                if ( ( arrayOrder["payment_status"].ToString() != "paid" ? false : arrayOrder["status"].ToString() != "cancelled" ) )
                {
                    IRestResponse restResponse2 = OperacionEcom.Response( 0, null, arrayOrder["number"].ToString() );
                    if ( restResponse2.StatusCode == HttpStatusCode.OK )
                    {
                        try
                        {
                            if ( !bool.Parse( JObject.Parse( restResponse2.Content )["Resultados"][0]["Pagado"].ToString() ) )
                            {
                                JToken jToken = JObject.Parse( restResponse2.Content )["Resultados"][0]["CompAfec"];
                                string str2 = jToken[0]["TipoComprobante"].ToString();
                                string str3 = jToken[0]["tipoCompCatacter"].ToString();
                                if ( jToken.ToString() == "[]" )
                                {
                                    flag = false;
                                }
                                else
                                {
                                    flag = ( str2 == "11" ? true : str2 == "23" );
                                }
                                if ( flag )
                                {
                                    dynamic jObject1 = new JObject();
                                    jObject1.TipoEntidadAfectada = str2;
                                    jObject1.IdAplicacionCliente = "TN";
                                    IRestResponse tipoComprobantePlatafomar = (IRestResponse)NuevoEnBaseA.Response( this.TipoComprobantePlatafomar, jToken[0]["Afecta"].ToString(), jObject1 );
                                    if ( ( tipoComprobantePlatafomar.StatusCode == HttpStatusCode.OK ? false : tipoComprobantePlatafomar.StatusDescription != "El comprobante no posee artículos pendientes" ) )
                                    {
                                        this.ProtocoloNoHacerNada();
                                        tipoComprobantePlatafomar = (IRestResponse)NuevoEnBaseA.Response( "Devolucion", jToken[0]["Afecta"].ToString(), jObject1 );
                                        if ( tipoComprobantePlatafomar.StatusCode != HttpStatusCode.OK )
                                        {
                                            LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( new string[] { "Error al procesar ", this.TipoComprobantePlatafomar, " en base a ", str3, "\n", tipoComprobantePlatafomar.StatusDescription } ), this.origenVentas );
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private bool ObtenerDatosPlataforma()
        {
            bool flag;
            IRestResponse restResponse = PlataformaEcom.Response( 0 );
            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "PlataformaEcommerce: \nNo se encontro la Plataforma ", Sync.singleton.plataformaEcommerce ), this.origenVentas );
                this.notification.ErrorIcon( string.Concat( "PlataformaEcommerce: \nNo se encontro la Plataforma ", Sync.singleton.plataformaEcommerce ) );
                flag = false;
            }
            else
            {
                try
                {
                    this.TipoComprobantePlatafomar = JObject.Parse( restResponse.Content )["Accion"].ToString();
                    if ( this.TipoComprobantePlatafomar == "" )
                    {
                        LogHandler.EnviarMsj( "", ErrorType.Minor, "El campo 'Acciones' se encuentra vacio en la Plataforma Ecommerce.", Sync.singleton.origenDFTN );
                        flag = false;
                        return flag;
                    }
                }
                catch
                {
                    flag = false;
                    return flag;
                }
                try
                {
                    this.BaseDeDatosPlataforma = JObject.Parse( restResponse.Content )["BaseDeDatos"].ToString();
                }
                catch
                {
                    flag = false;
                    return flag;
                }
                try
                {
                    this.ClienteDragonfishPlataforma = JObject.Parse( restResponse.Content )["ClienteDefault"].ToString();
                    if ( this.ClienteDragonfishPlataforma == "" )
                    {
                        LogHandler.EnviarMsj( "", ErrorType.Minor, "El campo 'Cliente Dragonfish' se encuentra vacio en la Plataforma Ecommerce.", Sync.singleton.origenDFTN );
                    }
                }
                catch
                {
                    flag = false;
                    return flag;
                }
                try
                {
                    this.ArticuloEnvioPlataforma = JObject.Parse( restResponse.Content )["Concenvio"].ToString();
                    if ( this.ArticuloEnvioPlataforma == "" )
                    {
                        LogHandler.EnviarMsj( "", ErrorType.Minor, "El campo 'Concepto para Envio' se encuentra vacio en la Plataforma Ecommerce.", Sync.singleton.origenDFTN );
                    }
                }
                catch
                {
                    flag = false;
                    return flag;
                }
                flag = true;
            }
            return flag;
        }

        private void ObtenerListaProductos( string fecha )
        {
            try
            {
                int num = 1;
                int count = 1;
                while ( count > 0 )
                {
                    int num1 = num;
                    num = num1 + 1;
                    IRestResponse restResponse = Metafield.Response( "", fecha, num1 );
                    if ( restResponse.StatusCode == HttpStatusCode.OK )
                    {
                        count = JArray.Parse( restResponse.Content ).Count;
                        foreach ( JToken jToken in JArray.Parse( restResponse.Content ) )
                        {
                            this.arrayProductos.Add( jToken );
                        }
                    }
                    else if ( restResponse.StatusCode != HttpStatusCode.NotFound )
                    {
                        LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( "Error al obteniendo informacion de Tienda Nube:\n", restResponse.StatusDescription ), this.origenStock );
                        count = 0;
                    }
                    else
                    {
                        count = 0;
                    }
                    Thread.Sleep( 3000 );
                }
            }
            catch ( Exception exception )
            {
                throw exception;
            }
        }

        private void ProcesarHerramienta( JArray detalle, JArray operacionesConError )
        {
            int num = 0;
            JArray jArray = new JArray();
            try
            {
                for ( int i = 0; i < detalle.Count; i++ )
                {
                    jArray.Add( detalle[i] );
                    num++;
                    if ( ( num == 30 ? true : i == detalle.Count - 1 ) )
                    {
                        dynamic jObject = new JObject();
                        jObject.Ecommerce = Sync.singleton.plataformaEcommerce;
                        jObject.DetalleOperaciones = jArray;
                        dynamic obj = ComprobanteEcom.Response( jObject );
                        num = 0;
                        jArray.Clear();
                        if ( obj.StatusCode == 201 )
                        {
                            string str = (string)JObject.Parse( obj.Content )["Numero"].ToString();
                            JArray jArray1 = (JArray)JObject.Parse( obj.Content )["DetalleOperaciones"];
                            try
                            {
                                int num1 = 0;
                                while ( true )
                                {
                                    if ( num1 >= jObject["DetalleOperaciones"].Count )
                                    {
                                        break;
                                    }
                                    bool flag = false;
                                    foreach ( JObject jObject1 in jArray1 )
                                    {
                                        if ( jObject1["NumeroOperacion"].ToString() == jObject["DetalleOperaciones"][num1]["NumeroOperacion"].ToString() )
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                    if ( !flag )
                                    {
                                        operacionesConError.Add( jObject["DetalleOperaciones"][num1] );
                                    }
                                    num1++;
                                }
                            }
                            catch ( Exception exception1 )
                            {
                                Exception exception = exception1;
                                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Verificando operaciones en comprobantes: \n", exception.Message ), this.origenVentas );
                            }
                            LogHandler.EnviarMsj( "", ErrorType.Information, string.Concat( "Se creo la Herramienta de Generación de Comprobantes número ", str ), this.origenVentas );
                        }
                        else if ( obj.StatusCode == 413 )
                        {
                            LogHandler.EnviarMsj( "", 0, "Error al crear la Herramienta de Generación de Comprobantes: \n" + obj.StatusCode + ": " + obj.StatusDescription + ".", this.origenVentas );
                        }
                        else if ( obj.StatusCode.ToString() != "0" )
                        {
                            LogHandler.EnviarMsj( "", 0, "Error al crear la Herramienta de Generación de Comprobantes: \n" + obj.Headers[4].ToString(), this.origenVentas );
                            string[] strArrays = (string[])obj.Headers[4].ToString().Replace( "\\r\\n", "#" ).Split( '#' );
                            this.notification.MessageIconTask( "Error al procesar algunas operaciones. Consulte el log de errores." );
                            try
                            {
                                for ( int j = 0; j < strArrays.Count<string>() - 1; j++ )
                                {
                                    if ( j % 2 == 0 )
                                    {
                                        strArrays[j] = strArrays[j].Replace( "InformacionAdicional=Operación nro: ", "" ).Replace( "Operación nro: ", "" );
                                        this.ControlarErrorOrdenes( strArrays[j], strArrays[j + 1].Replace( "\\t", "" ) );
                                    }
                                }
                            }
                            catch ( Exception exception3 )
                            {
                                Exception exception2 = exception3;
                                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Capturando errores de operaciones en comprobantes: \n", exception2.Message ), this.origenVentas );
                            }
                        }
                        else
                        {
                            LogHandler.EnviarMsj( "", 0, "Error al crear la Herramienta de Generación de Comprobantes: \n" + obj.StatusCode + ": Posible TimeOut.", this.origenVentas );
                        }
                    }
                }
            }
            catch ( Exception exception5 )
            {
                Exception exception4 = exception5;
                LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "ProcesarHerramienta: \n", exception4.Message ), this.origenVentas );
            }
        }

        private void ProductosConVariantesInexistentes()
        {
            if ( this.arrayProductosVariantesInexistentes.Count != 0 )
            {
                LogHandler.EnviarMsj( "", ErrorType.Information, "Obteniendo productos de Tienda Nube por Variante Inexistente.", this.origenStock );
                JArray jArray = new JArray( this.arrayProductosVariantesInexistentes );
                this.arrayProductosVariantesInexistentes.Clear();
                this.ProtocoloNoHacerNada();
                for ( int i = 0; i < jArray.Count; i++ )
                {
                    IRestResponse restResponse = Metafield.Response( jArray[i].ToString(), "", 1 );
                    if ( restResponse.StatusCode != HttpStatusCode.OK )
                    {
                        bool flag = true;
                        foreach ( JToken arrayProductosVariantesInexistente in this.arrayProductosVariantesInexistentes )
                        {
                            if ( arrayProductosVariantesInexistente.ToString() == jArray[i].ToString() )
                            {
                                flag = false;
                            }
                        }
                        if ( flag )
                        {
                            this.arrayProductosVariantesInexistentes.Add( jArray[i].ToString() );
                        }
                    }
                    else
                    {
                        foreach ( JToken jToken in JArray.Parse( restResponse.Content ) )
                        {
                            this.arrayProductos.Add( jToken );
                        }
                    }
                }
                if ( this.arrayProductos.Count != 0 )
                {
                    foreach ( JToken arrayProducto in this.arrayProductos )
                    {
                        this.PublicarStock( arrayProducto["value"].ToString(), false );
                    }
                }
            }
        }

        private void ProtocoloNoHacerNada()
        {
            LogHandler.EnviarMsj( "", ErrorType.Minor, "Protocolo No Hacer Nada: Activado.", Sync.singleton.origenDFTN );
            Thread.Sleep( 10000 );
        }

        private void ProtocoloPrimerMandato()
        {
            if ( ( DateTime.Now.Hour == 20 ? false : DateTime.Now.Hour != 8 ) )
            {
                this.protPrimerMandatoNoActivado = true;
            }
            else if ( this.protPrimerMandatoNoActivado )
            {
                LogHandler.EnviarMsj( "", ErrorType.Minor, "Protocolo Primer Mandato: Activado", this.origenStock );
                this.primerMandato = true;
                this.protPrimerMandatoNoActivado = false;
            }
        }

        private void PublicarStock( string sku, bool filtroFecha )
        {
            HttpStatusCode statusCode;
            string str;
            string str1;
            try
            {
                LogHandler.EnviarMsj( "", ErrorType.Information, "Obteniendo actualización de stock de zNube.", this.origenStock );
                int count = 1;
                int num = 0;
                int num1 = 0;
                bool flag = false;
                DateTime now = DateTime.Now;
                int num2 = 0;
                int num3 = 1;
                while ( count > 0 )
                {
                    Thread.Sleep( 10000 );
                    int num4 = num;
                    num = num4 + 1;
                    IRestResponse restResponse = StockOnline.Response( num4, sku, filtroFecha );
                    if ( restResponse.StatusCode == HttpStatusCode.OK )
                    {
                        string str2 = restResponse.Headers[1].ToString().Replace( "zNube-token=", "" );
                        Sync.singleton.tokenStockOnline = str2;
                        if ( JObject.Parse( restResponse.Content )["Status"].ToString() != "OK" )
                        {
                            LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( "Fallo al acceder a Stock Online: \nDescripción: ", JObject.Parse( restResponse.Content )["Errors"][0]["Message"].ToString(), "." ), this.origenStock );
                            this.notification.ErrorIcon( "No se pudo obtener el Stock.\nAPI ZNube." );
                            break;
                        }
                        else
                        {
                            if ( num2 == 0 )
                            {
                                num2 = int.Parse( JObject.Parse( restResponse.Content )["Data"]["TotalSku"].ToString() );
                            }
                            string str3 = JObject.Parse( restResponse.Content )["Data"]["Stock"].ToString();
                            count = JArray.Parse( str3 ).Count;
                            string str4 = "";
                            IRestResponse restResponse1 = null;
                            foreach ( JToken jToken in JArray.Parse( str3 ) )
                            {
                                if ( filtroFecha )
                                {
                                    int num5 = num3;
                                    num3 = num5 + 1;
                                    int num6 = num5;
                                    LogHandler.EnviarMsj( "", ErrorType.Information, string.Concat( "Verificando set de combinaciones ", num6.ToString(), " de ", num2.ToString() ), this.origenStock );
                                }
                                string str5 = jToken["ProductId"].ToString();
                                string str6 = "";
                                foreach ( JToken arrayProducto in this.arrayProductos )
                                {
                                    if ( arrayProducto["value"].ToString() == str5 )
                                    {
                                        str6 = arrayProducto["owner_id"].ToString();
                                        break;
                                    }
                                }
                                if ( str6 != "" )
                                {
                                    int num7 = 0;
                                    for ( int i = 0; i < JArray.Parse( jToken["Stock"].ToString() ).Count; i++ )
                                    {
                                        num7 += int.Parse( jToken["Stock"][i]["Quantity"].ToString() );
                                    }
                                    string str7 = "";
                                    string str8 = "";
                                    str7 = jToken["Sku"].ToString().Split( new char[] { '#' } )[1];
                                    str8 = jToken["Sku"].ToString().Split( new char[] { '#' } )[2];
                                    if ( num7 < 0 )
                                    {
                                        num7 = 0;
                                    }
                                    if ( str4 != str6 )
                                    {
                                        str4 = str6;
                                        restResponse1 = Variants.Response( 0, null, str6 );
                                        Thread.Sleep( 3000 );
                                    }
                                    HttpStatusCode httpStatusCode = restResponse1.StatusCode;
                                    if ( httpStatusCode == HttpStatusCode.OK )
                                    {
                                        JObject jObject = new JObject();
                                        jObject.Add( new JProperty( "stock", (object)num7 ) );
                                        int num8 = 0;
                                        while ( num8 < JArray.Parse( restResponse1.Content ).Count )
                                        {
                                            JToken item = JArray.Parse( restResponse1.Content )[num8];
                                            if ( item["sku"].ToString() != string.Concat( str7, "-_-", str8 ) )
                                            {
                                                num8++;
                                            }
                                            else
                                            {
                                                jObject.Add( new JProperty( "id", item["id"].ToString() ) );
                                                break;
                                            }
                                        }
                                        if ( ( jObject.ContainsKey( "id" ) ? true : jObject["stock"].ToString() != "0" ) )
                                        {
                                            if ( !jObject.ContainsKey( "id" ) )
                                            {
                                                bool flag1 = true;
                                                foreach ( JToken arrayProductosVariantesInexistente in this.arrayProductosVariantesInexistentes )
                                                {
                                                    if ( arrayProductosVariantesInexistente.ToString() == str6 )
                                                    {
                                                        flag1 = false;
                                                    }
                                                }
                                                if ( flag1 )
                                                {
                                                    LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( "Agregando artículo ", str5, " a la lista de consultas posteriores por combinación inexistente en Tienda Nube." ), this.origenStock );
                                                    this.arrayProductosVariantesInexistentes.Add( str6 );
                                                }
                                                if ( Sync.singleton.generarMovStock )
                                                {
                                                    this.crearCombinacion( jToken["Sku"].ToString() );
                                                }
                                            }
                                            else if ( jObject.ContainsKey( "id" ) )
                                            {
                                                IRestResponse restResponse2 = Variants.Response( RestSharp.Method.PUT, jObject, str6 );
                                                num1++;
                                                if ( restResponse2.StatusCode != HttpStatusCode.OK )
                                                {
                                                    bool flag2 = true;
                                                    foreach ( JToken arrayProductosVariantesInexistente1 in this.arrayProductosVariantesInexistentes )
                                                    {
                                                        if ( arrayProductosVariantesInexistente1.ToString() == str6 )
                                                        {
                                                            flag2 = false;
                                                        }
                                                    }
                                                    if ( flag2 )
                                                    {
                                                        LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( "Agregando artículo ", str5, " a la lista de consultas posteriores por combinación inexistente en Tienda Nube." ), this.origenStock );
                                                        this.arrayProductosVariantesInexistentes.Add( str6 );
                                                    }
                                                    if ( Sync.singleton.generarMovStock )
                                                    {
                                                        this.crearCombinacion( jToken["Sku"].ToString() );
                                                    }
                                                    LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( new string[] { "Actualización #", num1.ToString(), " - ", jToken["Sku"].ToString(), "\nRespuesta: ", restResponse2.StatusDescription, "\n\nContenido: \n", restResponse2.Content } ), this.origenStock );
                                                }
                                                else if ( filtroFecha )
                                                {
                                                }
                                            }
                                        }
                                    }
                                    else if ( httpStatusCode == HttpStatusCode.Unauthorized )
                                    {
                                        LogHandler.EnviarMsj( "", ErrorType.Minor, "Error de autenticación Stock Online.", this.origenStock );
                                        this.notification.ErrorIcon( "Error de autenticación Stock Online." );
                                    }
                                    else if ( httpStatusCode == HttpStatusCode.NotFound )
                                    {
                                        JToken item1 = jToken["Sku"];
                                        if ( item1 != null )
                                        {
                                            str = item1.ToString();
                                        }
                                        else
                                        {
                                            str = null;
                                        }
                                        LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( "Publicación Stock: ", str, " - ", restResponse1.StatusDescription ), this.origenStock );
                                    }
                                    else
                                    {
                                        JToken jToken1 = jToken["Sku"];
                                        if ( jToken1 != null )
                                        {
                                            str1 = jToken1.ToString();
                                        }
                                        else
                                        {
                                            str1 = null;
                                        }
                                        LogHandler.EnviarMsj( "", ErrorType.Minor, string.Concat( "Publicación Stock: ", str1, " - ", restResponse1.StatusDescription ), this.origenStock );
                                    }
                                }
                            }
                        }
                    }
                    else if ( restResponse.StatusCode != HttpStatusCode.InternalServerError )
                    {
                        if ( restResponse.StatusDescription != "Gateway Time-out" )
                        {
                            string[] statusDescription = new string[] { "Fallo al acceder a Stock Online: \nCódigo: ", null, null, null, null };
                            statusCode = restResponse.StatusCode;
                            statusDescription[1] = statusCode.ToString();
                            statusDescription[2] = ". Descripción: ";
                            statusDescription[3] = restResponse.StatusDescription;
                            statusDescription[4] = ".";
                            LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( statusDescription ), this.origenStock );
                            this.notification.ErrorIcon( "No se pudo obtener el Stock.\nAPI ZNube." );
                        }
                        else
                        {
                            flag = true;
                            string[] strArrays = new string[] { "Fallo al acceder a Stock Online: \nCódigo: ", null, null, null, null };
                            statusCode = restResponse.StatusCode;
                            strArrays[1] = statusCode.ToString();
                            strArrays[2] = ". Descripción: ";
                            strArrays[3] = restResponse.StatusDescription;
                            strArrays[4] = ".\nSe reintentará más tarde.";
                            LogHandler.EnviarMsj( "", ErrorType.Critical, string.Concat( strArrays ), this.origenStock );
                        }
                        break;
                    }
                    else
                    {
                        LogHandler.EnviarMsj( "", ErrorType.Critical, "Fallo al acceder a Stock Online: \nError de autorización, error de token. API ZNube.", this.origenStock );
                        this.notification.ErrorIcon( "No se pudo obtener el Stock.\nAPI ZNube." );
                        break;
                    }
                }
                Sync.singleton.ActualizarTokenzNube( Sync.singleton.tokenStockOnline );
                LogHandler.EnviarMsj( "", ErrorType.Information, string.Concat( "Se modificaron ", num1.ToString(), " combinaciones." ), this.origenStock );
                if ( sku == "" )
                {
                    if ( !flag )
                    {
                        Sync.singleton.ActualizarUltimaFechaStock( now );
                    }
                    else
                    {
                        Sync.singleton.ActualizarUltimaFechaStock( Convert.ToDateTime( "01/01/1900 00:00:00" ) );
                    }
                }
            }
            catch ( Exception exception1 )
            {
                Exception exception = exception1;
                LogHandler.EnviarMsj( "", ErrorType.Information, string.Concat( "Publicar Stock:\n ", exception.Message ), this.origenStock );
            }
        }

        private void StartProcess()
        {
            DateTime dateTime;
            try
            {
                LogHandler.EnviarMsj( "", ErrorType.Minor, "Proceso Iniciado.", Sync.singleton.origenDFTN );
                this.notification.ProcessingIcon();
                Sync.singleton.getXml();
                if ( Sync.singleton.habilitarDescargaVentas )
                {
                    this.tareasEnProceso = "Descargando Operaciones Ecommerce";
                    if ( Sync.singleton.fechaOrden == Convert.ToDateTime( "01/01/1900 00:00:00" ) )
                    {
                        dateTime = Sync.singleton.fechaOrden.AddHours( 3 );
                        IRestResponse restResponse = Order.Response( 0, 1, 1, dateTime.ToString( "yyyy-MM-dd HH:mm:ss" ), "" );
                        if ( restResponse.StatusCode != HttpStatusCode.OK )
                        {
                            Sync.singleton.ActualizarFechaUltimaOrden( DateTime.Now );
                        }
                        else
                        {
                            string str = JArray.Parse( restResponse.Content )[0]["created_at"].ToString().Replace( "+0000", "+0300" );
                            Singleton singleton = Sync.singleton;
                            dateTime = Convert.ToDateTime( str );
                            singleton.ActualizarFechaUltimaOrden( dateTime.AddSeconds( 1 ) );
                        }
                        Sync.singleton.getXml();
                    }
                    if ( Sync.singleton.plataformaEcommerce == "" )
                    {
                        LogHandler.EnviarMsj( "", ErrorType.Critical, "El campo 'Plataforma de Ecommerce' se encuentra vacion en el archivo Config.xml.", this.origenVentas );
                        this.notification.ErrorIcon( "El campo 'Plataforma de Ecommerce' se encuentra vacion en el archivo Config.xml." );
                        return;
                    }
                    else if ( this.ObtenerDatosPlataforma() )
                    {
                        if ( Sync.singleton.notificarOrdenesModificadas )
                        {
                            dateTime = Sync.singleton.fechaOrdenModificada.AddHours( 3 );
                            this.DescargarOrdenes( "", dateTime.ToString( "yyyy-MM-dd HH:mm:ss" ) );
                        }
                        dateTime = Sync.singleton.fechaOrden.AddHours( 3 );
                        this.DescargarOrdenes( dateTime.ToString( "yyyy-MM-dd HH:mm:ss" ), "" );
                    }
                }
                Sync.singleton.getXml();
                if ( ( !Sync.singleton.habilitarDescargaVentas ? false : Sync.singleton.habilitarPublicacionStock ) )
                {
                    this.ProtocoloNoHacerNada();
                }
                if ( Sync.singleton.habilitarPublicacionStock )
                {
                    this.ProtocoloPrimerMandato();
                    this.tareasEnProceso = "Publicando Stock en la Tienda";
                    if ( !this.primerMandato )
                    {
                        this.ComprobarNuevosArticulos();
                        this.arrayProductos.Clear();
                        this.ProductosConVariantesInexistentes();
                        this.arrayProductos.Clear();
                    }
                    LogHandler.EnviarMsj( "", ErrorType.Information, "Obteniendo lista de productos de Tienda Nube.", this.origenStock );
                    this.ObtenerListaProductos( "" );
                    this.PublicarStock( "", !this.primerMandato );
                    this.arrayProductos.Clear();
                    this.primerMandato = false;
                }
            }
            catch ( Exception exception1 )
            {
                Exception exception = exception1;
                LogHandler.EnviarMsj( "", ErrorType.Critical, exception.Message, Sync.singleton.origenDFTN );
            }
            this.notification.IdleIcon();
            LogHandler.EnviarMsj( "", ErrorType.Minor, "Proceso Finalizado.", Sync.singleton.origenDFTN );
            this.running = false;
            this.tareasEnProceso = "Aguardando Tareas";
        }

        public void StartSync()
        {
            Sync.singleton = Singleton.Instance;
            this.notification = Notification.Instance;
            if ( !this.running )
            {
                Sync.InternalTimer = new Timer( new TimerCallback( this.InitProcess ), null, 0, Sync.singleton.startUp * 60000 );
            }
        }
    }
}