using Dragonfish_TN.Properties;
using ProcesarErrores;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dragonfish_TN
{
	internal class Notification
	{
		public static NotifyIcon iconTask;

		private static Notification _instance;

		private static Sync sync;

		private static Singleton singleton;

		public bool error = false;

		public static Notification Instance
		{
			get
			{
				if (Notification._instance == null)
				{
					Notification._instance = new Notification();
				}
				return Notification._instance;
			}
		}

		static Notification()
		{
			Notification.iconTask = new NotifyIcon();
			Notification._instance = null;
			Notification.sync = null;
			Notification.singleton = null;
		}

		public Notification()
		{
		}

		private void ClickIcon(object sender, EventArgs e)
		{
			this.MessageIconTask(Notification.sync.tareasEnProceso);
		}

		private static void Configuracion(object sender, EventArgs e)
		{
			(new Configuracion()).Show();
		}

		public void ErrorIcon(string msg)
		{
			Notification.iconTask.Icon = new Icon(Resources.error, 40, 40);
			Notification.iconTask.Visible = true;
			this.MessageIconTask(msg);
			this.error = true;
		}

		private static void ExitIcon(object sender, EventArgs e)
		{
			Notification.iconTask.Visible = false;
			LogHandler.EnviarMsj("", ErrorType.Minor, "Finalizando proceso por el usuario.", Notification.singleton.origenDFTN);
			Environment.Exit(0);
		}

		public void IdleIcon()
		{
			if (!this.error)
			{
				Notification.iconTask.Icon = new Icon(Resources.idle, 40, 40);
			}
		}

		private static void LogIcon(object sender, EventArgs e)
		{
			try
			{
				(new VentanaErroresTN(Notification.singleton.log)).Show();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogHandler.EnviarMsj("", ErrorType.Critical, string.Concat("Error al abrir log: \n", exception.Message), Notification.singleton.origenDFTN);
			}
		}

		public void MessageIconTask(string message)
		{
			Notification.iconTask.BalloonTipText = message;
			Notification.iconTask.BalloonTipIcon = ToolTipIcon.Info;
			Notification.iconTask.BalloonTipTitle = "Integración Dragonfish - Tienda Nube";
			Notification.iconTask.ShowBalloonTip(60000);
		}

		public void ProcessingIcon()
		{
			Notification.iconTask.Icon = new Icon(Resources.process, 40, 40);
		}

		private void RestarIcon(object sender, EventArgs e)
		{
			if (!Notification.sync.running)
			{
				LogHandler.EnviarMsj("", ErrorType.Minor, "Reiniciando.", Notification.singleton.origenDFTN);
				Task.Run(() => {
					Notification.sync.cancelTimer();
					Dragonfish_TN.Program.iniciarApp();
				});
			}
			else
			{
				LogHandler.EnviarMsj("", ErrorType.Minor, "Aguardando a que finalicen las tareas.", Notification.singleton.origenDFTN);
				this.MessageIconTask("Aguardando a que finalicen las tareas");
			}
			this.error = false;
		}

		public void Start()
		{
			Notification.sync = Sync.Instance;
			Notification.singleton = Singleton.Instance;
			Notification.iconTask.Icon = new Icon(Resources.idle, 40, 40);
			Notification.iconTask.DoubleClick += new EventHandler(this.ClickIcon);
			Notification.iconTask.Text = "Integración Dragonfish - Tienda Nube";
			Notification.iconTask.Visible = true;
			ContextMenu contextMenu = new ContextMenu();
			contextMenu.MenuItems.Add("Log", new EventHandler(Notification.LogIcon));
			contextMenu.MenuItems.Add("Configuración", new EventHandler(Notification.Configuracion));
			contextMenu.MenuItems.Add("Reiniciar", new EventHandler(this.RestarIcon));
			contextMenu.MenuItems.Add("Salir", new EventHandler(Notification.ExitIcon));
			Notification.iconTask.ContextMenu = contextMenu;
		}
	}
}