using RestSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace Stock_TN
{


    static class Program
    {
        static Singleton singleton = null;
        static Sync sync = null;
        static Notification notification = null;

        static void Main()
        {
            string thisprocessname = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
            {
                return;
            }
                       
            notification = Notification.Instance;
            notification.Start();
            singleton = Singleton.Instance;


            if (singleton.clienteDragonfish == "")
            {
                Configuracion configuracion = new Configuracion();
                configuracion.Show();
            }
            else
            {
                iniciarApp();
            }

            Application.Run();
        }

        static public void iniciarApp()
        {
            LogHandler.EnviarMsj("", ErrorType.Information, "Iniciando aplicación.", singleton.origenDFTN);

            notification.ProcessingIcon();
            
            sync = Sync.Instance;            

            var response = verificarConexionDF();

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    LogHandler.EnviarMsj("", ErrorType.Critical, "Fallo al iniciar aplicación: \nError de autorización. API Dragonfish", singleton.origenDFTN);
                    notification.ErrorIcon("Error de autorización. API Dragonfish.");
                    break;

                case HttpStatusCode.OK:
                    sync.StartSync();
                    notification.IdleIcon();

                    break;

                case HttpStatusCode.NotFound:
                    notification.IdleIcon();
                    sync.StartSync();
                    break;

                default:
                    LogHandler.EnviarMsj("", ErrorType.Critical, "Fallo al iniciar aplicación: \nNo se puede acceder a la api de Dragonifsh.\nCódigo: " + response.StatusCode + ". Descripción: " + response.StatusDescription+".", singleton.origenDFTN);
                    notification.ErrorIcon("No se puede acceder a la api de Dragonifsh.");

                    break;
            }
        }

        static public void ConsoleLog(string Message)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(singleton.log, true))
                {
                    file.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "-" + Message);
                }
            }
            catch
            {

            }
        }

        static public IRestResponse verificarConexionDF()
        {
            var postArticulos = new RestClient(singleton.urlDragonfish + "/api.Dragonfish/Articulo/SEÑA/");
            var request = new RestRequest(Method.GET);

            request.AddHeader("BaseDeDatos", singleton.basesDeDatos);
            request.AddHeader("idCliente", singleton.clienteDragonfish);
            request.AddHeader("Authorization", singleton.tokenDragonfish);

            var response = postArticulos.Execute(request);

            return response;
        }


    }
}
