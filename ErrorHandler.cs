using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stock_TN
{
    public enum ErrorType
    {
        Critical,
        Minor,
        Information
    }

    class LogHandler
    {
        private const string log = "Dragonfish(TN)"; //Aplicación para Windows en español

        internal static void RegistrarError(string origen, ErrorType type, string msg)
        {
            try
            {
                if (!EventLog.Exists(log))
                {
                    new EventLog(log);
                }
                if (!EventLog.SourceExists(origen))
                {
                    EventLog.CreateEventSource(origen, log);
                }

                if (type == ErrorType.Information)
                    EventLog.WriteEntry(origen, msg, EventLogEntryType.Information);
                else if (type == ErrorType.Minor)
                    EventLog.WriteEntry(origen, msg, EventLogEntryType.Warning);
                else
                    EventLog.WriteEntry(origen, msg, EventLogEntryType.Error);

            }
            catch (Exception e)
            {
                MessageBox.Show("Ejecutar como administrador. \n" + e.Message, origen, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Environment.Exit(0);

            }
        }

        internal static void EnviarMsj(string msgError, ErrorType type, string msg,string origen)
        {
            //registrar error
            RegistrarError(origen, type, msg);

            //mostrar mensaje si es conveniente
            if (type == ErrorType.Critical)
            {
                //MessageBox.Show(msgError, origen, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

    }
}
