using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Dragonfish_TN.Properties
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
	internal class Resources
	{
		private static System.Resources.ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		internal static Bitmap carpetalistado
		{
			get
			{
				return (Bitmap)Resources.ResourceManager.GetObject("carpetalistado", Resources.resourceCulture);
			}
		}

		internal static string config
		{
			get
			{
				return Resources.ResourceManager.GetString("config", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		internal static Icon error
		{
			get
			{
				return (Icon)Resources.ResourceManager.GetObject("error", Resources.resourceCulture);
			}
		}

		internal static Icon idle
		{
			get
			{
				return (Icon)Resources.ResourceManager.GetObject("idle", Resources.resourceCulture);
			}
		}

		internal static Icon process
		{
			get
			{
				return (Icon)Resources.ResourceManager.GetObject("process", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static System.Resources.ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new System.Resources.ResourceManager("Dragonfish_TN.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		internal Resources()
		{
		}
	}
}