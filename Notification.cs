using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Stock_TN
{
    class Notification
    {
        public static NotifyIcon iconTask = new NotifyIcon();
        private static Notification _instance = null;
        static Sync sync = null;
        static Singleton singleton = null;


        bool error = false;
        public static Notification Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Notification();
                }
                return _instance;
            }
        }

        public void Start()
        {
            sync = Sync.Instance;
            singleton = Singleton.Instance;
            iconTask.Icon = new Icon(Properties.Resources.idle, 40, 40);

            iconTask.Visible = true;
            ContextMenu trayMenu = new ContextMenu();

            trayMenu.MenuItems.Add("Log", new EventHandler(LogIcon));
            trayMenu.MenuItems.Add("Configuración", new EventHandler(Configuracion));
            trayMenu.MenuItems.Add("Reiniciar", new EventHandler(RestarIcon));
            trayMenu.MenuItems.Add("Salir", new EventHandler(ExitIcon));

            iconTask.ContextMenu = trayMenu;
        }

        static void LogIcon(object sender, EventArgs e)
        {
            Process.Start("eventvwr.exe");
        }

        void RestarIcon(object sender, EventArgs e)
        {
            if (sync.running)
            {
                LogHandler.EnviarMsj("", ErrorType.Minor, "Aguardando a que finalice las tareas.", singleton.origenDFTN);
            }

            var cancelado = sync.cancelTimer();

            if (cancelado)
            {
                LogHandler.EnviarMsj("", ErrorType.Minor, "Reiniciando.", singleton.origenDFTN);
                sync.StartSync();
            }
            error = false;

        }
        static void Configuracion(object sender, EventArgs e)
        {
            Configuracion configuracion = new Configuracion();
            configuracion.Show();
        }
        static void ExitIcon(object sender, EventArgs e)
        {
            iconTask.Visible = false;
            LogHandler.EnviarMsj("", ErrorType.Minor, "Finalizando proceso por el usuario.", singleton.origenDFTN);
            Environment.Exit(0);
        }

        public void IdleIcon()
        {
            if (!error)
                iconTask.Icon = new Icon(Properties.Resources.idle, 40, 40);
        }

        public void ProcessingIcon()
        {
            iconTask.Icon = new Icon(Properties.Resources.process, 40, 40);
        }

        public void ErrorIcon(string msg)
        {
            iconTask.Icon = new Icon(Properties.Resources.error, 40, 40);
            iconTask.Visible = true;
            MessageIconTask(msg);
            error = true;

        }

        public void MessageIconTask(string message)
        {
            iconTask.BalloonTipText = message;
            iconTask.BalloonTipIcon = ToolTipIcon.Info;
            iconTask.BalloonTipTitle = singleton.origenDFTN;
            iconTask.ShowBalloonTip(5000);
        }
    }

}