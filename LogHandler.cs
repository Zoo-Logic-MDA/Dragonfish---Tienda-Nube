using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dragonfish_TN
{
	internal class LogHandler
	{
		private const string log = "Dragonfish(TN)";

		public LogHandler()
		{
		}

		internal static void EnviarMsj(string msgError, ErrorType type, string msg, string origen)
		{
			try
			{
				LogHandler.RegistrarError(origen, type, msg);
				if ((type != ErrorType.Critical ? false : msgError != ""))
				{
					Task.Run(() => MessageBox.Show(msgError, string.Concat("Dragonfish - Tienda Nube: ", origen), MessageBoxButtons.OK, MessageBoxIcon.Exclamation));
				}
			}
			catch
			{
			}
		}

		internal static void RegistrarError(string origen, ErrorType type, string msg)
		{
			try
			{
				if (!EventLog.Exists("Dragonfish(TN)"))
				{
					EventLog eventLog = new EventLog("Dragonfish(TN)");
				}
				if (!EventLog.SourceExists(origen))
				{
					EventLog.CreateEventSource(origen, "Dragonfish(TN)");
				}
				if (type == ErrorType.Information)
				{
					EventLog.WriteEntry(origen, msg, EventLogEntryType.Information);
				}
				else if (type != ErrorType.Minor)
				{
					EventLog.WriteEntry(origen, msg, EventLogEntryType.Error);
				}
				else
				{
					EventLog.WriteEntry(origen, msg, EventLogEntryType.Warning);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				MessageBox.Show(string.Concat("Ejecutar como administrador. \n", exception.Message), origen, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
	}
}