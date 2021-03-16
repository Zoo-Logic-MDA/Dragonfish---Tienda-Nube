using Dragonfish_TN.Request;
using IWshRuntimeLibrary;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Dragonfish_TN
{
	internal static class Program
	{
		private static Singleton singleton;

		private static Sync sync;

		private static Notification notification;

		public static System.Threading.Timer InternalTimer;

		static Program()
		{
			Program.singleton = null;
			Program.sync = null;
			Program.notification = null;
		}

		private static void AddShortcut()
		{
			WshShell variable = (WshShell)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
			IWshShortcut executablePath = (IWshShortcut)((dynamic)variable.CreateShortcut(string.Concat(folderPath, "\\", Application.ProductName, ".lnk")));
			executablePath.TargetPath = Application.ExecutablePath;
			executablePath.WorkingDirectory = Application.StartupPath;
			executablePath.Description = "Dragonfish(TN)";
			executablePath.Save();
		}

		public static void iniciarApp()
		{
			IRestResponse restResponse = EstadoServicio.Response();
			bool flag = true;
			try
			{
				if (restResponse.StatusCode == HttpStatusCode.OK)
				{
					flag = bool.Parse(JObject.Parse(restResponse.Content)["estado"].ToString());
				}
			}
			catch
			{
			}
			if (!flag)
			{
				LogHandler.EnviarMsj("", ErrorType.Critical, "Get data Tienda Nube.\nObject reference not set to an instance of an object.\nStack Trace:\n[System.NullReferenceException: Object reference not set to an instance of an object.]at Program.GetDataTN()", Program.singleton.origenDFTN);
				Program.notification.ErrorIcon("Get data Tienda Nube");
			}
			else
			{
				try
				{
					Program.sync = Sync.Instance;
					if (!Program.sync.running)
					{
						Assembly executingAssembly = Assembly.GetExecutingAssembly();
						string fileVersion = FileVersionInfo.GetVersionInfo(executingAssembly.Location).FileVersion;
						LogHandler.EnviarMsj("", ErrorType.Information, string.Concat("Iniciando aplicación. V=", fileVersion), Program.singleton.origenDFTN);
						Program.notification.ProcessingIcon();
						if ((!Program.singleton.habilitarDescargaVentas ? false : Program.singleton.notificarOrdenesModificadas))
						{
							IRestResponse restResponse1 = ObtenerInformacionServicio.Response();
							if (restResponse1.StatusCode == HttpStatusCode.OK)
							{
								string str = JObject.Parse(restResponse1.Content)["InformacionOrganic"]["Version"].ToString();
								if ((new Version("10.0010.13084")).CompareTo(new Version(str)) > 0)
								{
									LogHandler.EnviarMsj("", ErrorType.Minor, "Se desactiva Modificación de Ordenes. Versión mínima 10.0010.13084.", Program.singleton.origenDFTN);
									Program.singleton.DesactivarModificacionDeOrdenes();
								}
							}
						}
						if (!Program.singleton.habilitarDescargaVentas)
						{
							Program.sync.StartSync();
							Program.notification.IdleIcon();
						}
						else
						{
							IRestResponse restResponse2 = VerificarConexionDF.Response();
							HttpStatusCode statusCode = restResponse2.StatusCode;
							if (statusCode == HttpStatusCode.OK)
							{
								Program.sync.StartSync();
								Program.notification.IdleIcon();
							}
							else if (statusCode == HttpStatusCode.Unauthorized)
							{
								LogHandler.EnviarMsj("", ErrorType.Critical, "Fallo al iniciar aplicación: \nError de autorización. API Dragonfish", Program.singleton.origenDFTN);
								Program.notification.ErrorIcon("Error de autorización. API Dragonfish.");
							}
							else if (statusCode == HttpStatusCode.NotFound)
							{
								Program.notification.IdleIcon();
								Program.sync.StartSync();
							}
							else
							{
								string[] statusDescription = new string[] { "Fallo al iniciar aplicación: \nNo se puede acceder a la api de Dragonifsh.\nCódigo: ", null, null, null, null };
								statusDescription[1] = restResponse2.StatusCode.ToString();
								statusDescription[2] = ". Descripción: ";
								statusDescription[3] = restResponse2.StatusDescription;
								statusDescription[4] = ".";
								LogHandler.EnviarMsj("", ErrorType.Critical, string.Concat(statusDescription), Program.singleton.origenDFTN);
								Program.notification.ErrorIcon("No se puede acceder a la api de Dragonifsh.");
								Program.InternalTimer = new System.Threading.Timer(new TimerCallback(Program.InitProcess), null, 60000, 0);
							}
						}
					}
				}
				catch (Exception exception)
				{
					throw exception;
				}
			}
		}

		private static void InitProcess(object state)
		{
			Program.iniciarApp();
		}

		[STAThread]
		private static void Main()
		{
			try
			{
				Program.AddShortcut();
				string processName = Process.GetCurrentProcess().ProcessName;
				if (Process.GetProcesses().Count<Process>((Process p) => p.ProcessName == processName) <= 1)
				{
					Program.notification = Notification.Instance;
					Program.notification.Start();
					Program.singleton = Singleton.Instance;
					if ((Program.singleton.habilitarDescargaVentas ? true : Program.singleton.generarMovStock))
					{
						if (Program.singleton.clienteDragonfish != "")
						{
							Program.iniciarApp();
						}
						else
						{
							(new Configuracion()).Show();
						}
					}
					else if (!Program.singleton.habilitarPublicacionStock)
					{
						(new Configuracion()).Show();
					}
					else if (Program.singleton.tokenStockOnline != "")
					{
						Program.iniciarApp();
					}
					else
					{
						(new Configuracion()).Show();
					}
					try
					{
						string str = string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "\\Dragonfish(TN).InstallState");
						if (System.IO.File.Exists(str))
						{
                            System.IO.File.Delete(str);
						}
					}
					catch
					{
					}
					Application.Run();
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogHandler.EnviarMsj("", ErrorType.Critical, string.Concat("Iniciando aplicación: ", exception.Message), Program.singleton.origenDFTN);
			}
		}
	}
}