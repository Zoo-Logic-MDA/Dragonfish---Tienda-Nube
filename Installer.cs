using IWshRuntimeLibrary;
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Dragonfish_TN
{
	[RunInstaller(true)]
	public class Installer : System.Configuration.Install.Installer
	{
		private IContainer components = null;

		public Installer()
		{
		}

		private static void AddShortcut()
		{
			WshShell variable = (WshShell)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
			IWshShortcut directoryName = (IWshShortcut)((dynamic)variable.CreateShortcut(string.Concat(folderPath, "\\Dragonfish(TN).lnk")));
			directoryName.TargetPath = string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "\\Dragonfish(TN).exe");
			directoryName.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			directoryName.Description = "Dragonfish(TN)";
			directoryName.Save();
		}

		public override void Commit(IDictionary savedState)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if ((!disposing ? false : this.components != null))
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
		}

		public override void Install(IDictionary savedState)
		{
			this.StartUp();
		}

		private void MyInstaller_Committed(object sender, InstallEventArgs e)
		{
		}

		private void MyInstaller_Committing(object sender, InstallEventArgs e)
		{
		}

		public override void Rollback(IDictionary savedState)
		{
		}

		private void StartUp()
		{
			try
			{
				Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
				Process.Start(string.Concat(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "\\Dragonfish(TN).exe"));
			}
			catch
			{
			}
		}
	}
}