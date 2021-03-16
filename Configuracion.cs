using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;

namespace Dragonfish_TN
{
	public class Configuracion : Form
	{
		private Singleton singleton = null;

		private IContainer components = null;

		private CheckBox checkBoxHabilitarStock;

		private CheckBox checkBoxHabilitarVentas;

		private Button buttonGuardar;

		private Label label3;

		private Label label4;

		private TextBox textBoxTokenZnube;

		private TextBox textBoxBDZnube;

		private LinkLabel linkLabelVincularTienda;

		private TextBox textBoxTokenTN;

		private Label label9;

		private TextBox textBoxClienteTN;

		private Label label8;

		private Label label10;

		private NumericUpDown textBoxStartUp;

		private TextBox textBoxTokenDF;

		private Label label2;

		private Label label5;

		private TextBox textBoxUrl;

		private Label label1;

		private Label label6;

		private TextBox textBoxBDDF;

		private Label label7;

		private TextBox textBoxPlataforma;

		private PictureBox pictureBox1;

		private Label label11;

		private Label label12;

		private PictureBox pictureBox2;

		private Label label13;

		private PictureBox pictureBox3;

		private Label label14;

		private PictureBox pictureBox4;

		private Label label15;

		private ComboBox comboBoxComprobanteSinPago;

		private TextBox textBoxClienteDF;

		private TextBox textBoxArticulo;

		private Label label16;

		private CheckBox checkBoxDescargarEnvio;

		private Label label17;

		private Label label18;

		private Label label19;

		private Label label20;

		private CheckBox checkBoxMovStock;

		private Label label21;

		private CheckBox checkBoxNotificarComprobantesCancelados;

		private Label label22;

		private Button buttonBuscarCarpeta;

		private TextBox textBoxRuta;

		public Configuracion()
		{
			this.singleton = Singleton.Instance;
			this.InitializeComponent();
			this.textBoxTokenZnube.Enabled = this.checkBoxHabilitarStock.Checked;
			this.textBoxBDZnube.Enabled = this.checkBoxHabilitarStock.Checked;
			this.textBoxPlataforma.Enabled = this.checkBoxHabilitarVentas.Checked;
			this.textBoxArticulo.Enabled = this.checkBoxHabilitarVentas.Checked;
			this.comboBoxComprobanteSinPago.Enabled = this.checkBoxHabilitarVentas.Checked;
			this.checkBoxDescargarEnvio.Enabled = this.checkBoxHabilitarVentas.Checked;
			this.checkBoxNotificarComprobantesCancelados.Enabled = this.checkBoxHabilitarVentas.Checked;
			this.comboBoxComprobanteSinPago.Items.Insert(0, "Respetar parametros de la Plataforma Ecommerce");
			this.comboBoxComprobanteSinPago.Items.Insert(1, "Remito");
			this.comboBoxComprobanteSinPago.Items.Insert(2, "Pedido");
			this.textBoxClienteDF.Text = this.singleton.clienteDragonfish;
			this.textBoxTokenDF.Text = this.singleton.tokenDragonfish;
			this.textBoxUrl.Text = this.singleton.urlDragonfish;
			this.textBoxBDDF.Text = this.singleton.baseDeDatos;
			this.textBoxArticulo.Text = this.singleton.articuloGenerico;
			this.textBoxPlataforma.Text = this.singleton.plataformaEcommerce;
			this.comboBoxComprobanteSinPago.SelectedIndex = this.singleton.comprobanteSinPago;
			this.textBoxTokenZnube.Text = this.singleton.tokenStockOnline;
			this.textBoxBDZnube.Text = this.singleton.resourceId;
			this.textBoxClienteTN.Text = this.singleton.clienteTiendaNube;
			this.textBoxTokenTN.Text = this.singleton.tokenTiendaNube;
			this.textBoxStartUp.Value = this.singleton.startUp;
			this.checkBoxHabilitarStock.Checked = this.singleton.habilitarPublicacionStock;
			this.checkBoxHabilitarVentas.Checked = this.singleton.habilitarDescargaVentas;
			this.checkBoxNotificarComprobantesCancelados.Checked = this.singleton.notificarOrdenesModificadas;
			this.textBoxRuta.Text = this.singleton.rutaListado;
			this.checkBoxDescargarEnvio.Checked = this.singleton.descargarEnvio;
			this.checkBoxMovStock.Checked = this.singleton.generarMovStock;
		}

		public void actualizarXLM()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(this.singleton.config);
			XmlNode str = xmlDocument.SelectSingleNode("Application/HabilitarDescargaVentas");
			bool @checked = this.checkBoxHabilitarVentas.Checked;
			str.InnerText = @checked.ToString();
			XmlNode xmlNodes = xmlDocument.SelectSingleNode("Application/NotificarmeComprobantesCancelados");
			@checked = this.checkBoxNotificarComprobantesCancelados.Checked;
			xmlNodes.InnerText = @checked.ToString();
			XmlNode str1 = xmlDocument.SelectSingleNode("Application/RutaListado");
			str1.InnerText = this.textBoxRuta.Text.ToString();
			XmlNode xmlNodes1 = xmlDocument.SelectSingleNode("Application/DescargaEnvio");
			@checked = this.checkBoxDescargarEnvio.Checked;
			xmlNodes1.InnerText = @checked.ToString();
			XmlNode str2 = xmlDocument.SelectSingleNode("Application/GenerarMovStockPorCombinacionInexistente");
			@checked = this.checkBoxMovStock.Checked;
			str2.InnerText = @checked.ToString();
			XmlNode xmlNodes2 = xmlDocument.SelectSingleNode("Application/HabilitarPublicacionStock");
			@checked = this.checkBoxHabilitarStock.Checked;
			xmlNodes2.InnerText = @checked.ToString();
			XmlNode str3 = xmlDocument.SelectSingleNode("Application/StartUp");
			str3.InnerText = this.textBoxStartUp.Value.ToString();
			XmlNode xmlNodes3 = xmlDocument.SelectSingleNode("Application/ClienteDragonfish");
			xmlNodes3.InnerText = this.textBoxClienteDF.Text.ToString();
			XmlNode str4 = xmlDocument.SelectSingleNode("Application/TokenDragonfish");
			str4.InnerText = this.textBoxTokenDF.Text.ToString();
			XmlNode xmlNodes4 = xmlDocument.SelectSingleNode("Application/UrlDragonfish");
			xmlNodes4.InnerText = this.textBoxUrl.Text.ToString();
			XmlNode str5 = xmlDocument.SelectSingleNode("Application/BasesDeDatos");
			str5.InnerText = this.textBoxBDDF.Text.ToString();
			XmlNode xmlNodes5 = xmlDocument.SelectSingleNode("Application/ArticuloGenerico");
			xmlNodes5.InnerText = this.textBoxArticulo.Text.ToString();
			XmlNode str6 = xmlDocument.SelectSingleNode("Application/PlataformaEcommerce");
			str6.InnerText = this.textBoxPlataforma.Text.ToString();
			XmlNode xmlNodes6 = xmlDocument.SelectSingleNode("Application/ComprobanteSinPago");
			xmlNodes6.InnerText = this.comboBoxComprobanteSinPago.SelectedIndex.ToString();
			XmlNode str7 = xmlDocument.SelectSingleNode("Application/ClienteTiendaNube");
			str7.InnerText = this.textBoxClienteTN.Text.ToString();
			XmlNode xmlNodes7 = xmlDocument.SelectSingleNode("Application/TokenTiendaNube");
			xmlNodes7.InnerText = this.textBoxTokenTN.Text.ToString();
			XmlNode str8 = xmlDocument.SelectSingleNode("Application/TokenStockOnline");
			str8.InnerText = this.textBoxTokenZnube.Text.ToString();
			XmlNode xmlNodes8 = xmlDocument.SelectSingleNode("Application/ResourcesID");
			xmlNodes8.InnerText = this.textBoxBDZnube.Text.ToString();
			xmlDocument.Save(this.singleton.config);
			this.singleton.getXml();
		}

		private void buttonBuscarCarpeta_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
			{
				RootFolder = Environment.SpecialFolder.Desktop,
				ShowNewFolderButton = true
			};
			if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				this.textBoxRuta.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private void buttonGuardar_Click(object sender, EventArgs e)
		{
			if ((this.checkBoxHabilitarVentas.Checked ? true : this.checkBoxHabilitarStock.Checked))
			{
				bool flag = false;
				if (this.checkBoxHabilitarVentas.Checked)
				{
					if ((this.textBoxBDDF.Text.ToString() == "" || this.textBoxPlataforma.Text.ToString() == "" || this.textBoxArticulo.Text.ToString() == "" || this.textBoxClienteTN.Text.ToString() == "" || this.textBoxTokenTN.Text.ToString() == "" || this.textBoxClienteDF.Text.ToString() == "" || this.textBoxTokenDF.Text.ToString() == "" || this.textBoxUrl.Text.ToString() == "" || this.textBoxClienteTN.Text.ToString() == "" ? true : this.textBoxTokenTN.Text.ToString() == ""))
					{
						flag = true;
					}
				}
				if (this.checkBoxHabilitarStock.Checked)
				{
					if ((this.textBoxTokenZnube.Text.ToString() == "" ? true : this.textBoxBDZnube.Text.ToString() == ""))
					{
						flag = true;
					}
				}
				if (flag)
				{
					MessageBox.Show("Completar todos lo campos.", this.singleton.origenDFTN, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				else
				{
					this.actualizarXLM();
					base.Close();
					Program.iniciarApp();
				}
			}
			else
			{
				MessageBox.Show("Debe activar al menos una configuración.", this.singleton.origenDFTN, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void checkBoxHabilitarStock_CheckedChanged(object sender, EventArgs e)
		{
			this.textBoxTokenZnube.Enabled = this.checkBoxHabilitarStock.Checked;
			this.textBoxBDZnube.Enabled = this.checkBoxHabilitarStock.Checked;
			this.checkBoxMovStock.Enabled = this.checkBoxHabilitarStock.Checked;
		}

		private void checkBoxHabilitarVentas_CheckedChanged(object sender, EventArgs e)
		{
			this.textBoxPlataforma.Enabled = this.checkBoxHabilitarVentas.Checked;
			this.textBoxArticulo.Enabled = this.checkBoxHabilitarVentas.Checked;
			this.comboBoxComprobanteSinPago.Enabled = this.checkBoxHabilitarVentas.Checked;
			this.checkBoxDescargarEnvio.Enabled = this.checkBoxHabilitarVentas.Checked;
			this.checkBoxNotificarComprobantesCancelados.Enabled = this.checkBoxHabilitarVentas.Checked;
		}

		protected override void Dispose(bool disposing)
		{
			if ((!disposing ? false : this.components != null))
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			if (base.WindowState == FormWindowState.Minimized)
			{
				base.Hide();
			}
		}

		private void InitializeComponent()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Configuracion));
			this.checkBoxHabilitarStock = new CheckBox();
			this.checkBoxHabilitarVentas = new CheckBox();
			this.buttonGuardar = new Button();
			this.label3 = new Label();
			this.label4 = new Label();
			this.textBoxTokenZnube = new TextBox();
			this.textBoxBDZnube = new TextBox();
			this.linkLabelVincularTienda = new LinkLabel();
			this.textBoxTokenTN = new TextBox();
			this.label9 = new Label();
			this.textBoxClienteTN = new TextBox();
			this.label8 = new Label();
			this.label10 = new Label();
			this.textBoxStartUp = new NumericUpDown();
			this.textBoxTokenDF = new TextBox();
			this.label2 = new Label();
			this.label5 = new Label();
			this.textBoxUrl = new TextBox();
			this.label1 = new Label();
			this.label6 = new Label();
			this.textBoxBDDF = new TextBox();
			this.label7 = new Label();
			this.textBoxPlataforma = new TextBox();
			this.pictureBox1 = new PictureBox();
			this.label11 = new Label();
			this.label12 = new Label();
			this.pictureBox2 = new PictureBox();
			this.label13 = new Label();
			this.pictureBox3 = new PictureBox();
			this.label14 = new Label();
			this.pictureBox4 = new PictureBox();
			this.label15 = new Label();
			this.comboBoxComprobanteSinPago = new ComboBox();
			this.textBoxClienteDF = new TextBox();
			this.textBoxArticulo = new TextBox();
			this.label16 = new Label();
			this.checkBoxDescargarEnvio = new CheckBox();
			this.label17 = new Label();
			this.label18 = new Label();
			this.label19 = new Label();
			this.label20 = new Label();
			this.checkBoxMovStock = new CheckBox();
			this.label21 = new Label();
			this.checkBoxNotificarComprobantesCancelados = new CheckBox();
			this.label22 = new Label();
			this.buttonBuscarCarpeta = new Button();
			this.textBoxRuta = new TextBox();
			((ISupportInitialize)this.textBoxStartUp).BeginInit();
			((ISupportInitialize)this.pictureBox1).BeginInit();
			((ISupportInitialize)this.pictureBox2).BeginInit();
			((ISupportInitialize)this.pictureBox3).BeginInit();
			((ISupportInitialize)this.pictureBox4).BeginInit();
			base.SuspendLayout();
			this.checkBoxHabilitarStock.AutoSize = true;
			this.checkBoxHabilitarStock.CheckAlign = ContentAlignment.MiddleRight;
			this.checkBoxHabilitarStock.Location = new Point(655, 410);
			this.checkBoxHabilitarStock.Name = "checkBoxHabilitarStock";
			this.checkBoxHabilitarStock.Size = new System.Drawing.Size(15, 14);
			this.checkBoxHabilitarStock.TabIndex = 50;
			this.checkBoxHabilitarStock.UseVisualStyleBackColor = true;
			this.checkBoxHabilitarStock.CheckedChanged += new EventHandler(this.checkBoxHabilitarStock_CheckedChanged);
			this.checkBoxHabilitarVentas.AutoSize = true;
			this.checkBoxHabilitarVentas.CheckAlign = ContentAlignment.MiddleRight;
			this.checkBoxHabilitarVentas.Location = new Point(254, 410);
			this.checkBoxHabilitarVentas.Name = "checkBoxHabilitarVentas";
			this.checkBoxHabilitarVentas.Size = new System.Drawing.Size(15, 14);
			this.checkBoxHabilitarVentas.TabIndex = 41;
			this.checkBoxHabilitarVentas.UseVisualStyleBackColor = true;
			this.checkBoxHabilitarVentas.CheckedChanged += new EventHandler(this.checkBoxHabilitarVentas_CheckedChanged);
			this.buttonGuardar.FlatAppearance.BorderSize = 0;
			this.buttonGuardar.ForeColor = Color.Black;
			this.buttonGuardar.Location = new Point(596, 493);
			this.buttonGuardar.Name = "buttonGuardar";
			this.buttonGuardar.Size = new System.Drawing.Size(75, 23);
			this.buttonGuardar.TabIndex = 70;
			this.buttonGuardar.Text = "Guardar";
			this.buttonGuardar.UseVisualStyleBackColor = true;
			this.buttonGuardar.Click += new EventHandler(this.buttonGuardar_Click);
			this.label3.AutoSize = true;
			this.label3.Location = new Point(14, 209);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(102, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "Token Stock Online";
			this.label4.AutoSize = true;
			this.label4.Location = new Point(14, 230);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(98, 13);
			this.label4.TabIndex = 8;
			this.label4.Text = "ID's Base de Datos";
			this.textBoxTokenZnube.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxTokenZnube.Location = new Point(136, 206);
			this.textBoxTokenZnube.Name = "textBoxTokenZnube";
			this.textBoxTokenZnube.Size = new System.Drawing.Size(530, 20);
			this.textBoxTokenZnube.TabIndex = 20;
			this.textBoxBDZnube.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxBDZnube.Location = new Point(136, 227);
			this.textBoxBDZnube.Name = "textBoxBDZnube";
			this.textBoxBDZnube.Size = new System.Drawing.Size(530, 20);
			this.textBoxBDZnube.TabIndex = 21;
			this.linkLabelVincularTienda.AutoSize = true;
			this.linkLabelVincularTienda.Location = new Point(16, 344);
			this.linkLabelVincularTienda.Name = "linkLabelVincularTienda";
			this.linkLabelVincularTienda.Size = new System.Drawing.Size(81, 13);
			this.linkLabelVincularTienda.TabIndex = 32;
			this.linkLabelVincularTienda.TabStop = true;
			this.linkLabelVincularTienda.Text = "Vincular Tienda";
			this.linkLabelVincularTienda.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabelVincularTienda_LinkClicked);
			this.textBoxTokenTN.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxTokenTN.Location = new Point(136, 316);
			this.textBoxTokenTN.Name = "textBoxTokenTN";
			this.textBoxTokenTN.Size = new System.Drawing.Size(530, 20);
			this.textBoxTokenTN.TabIndex = 31;
			this.label9.AutoSize = true;
			this.label9.Location = new Point(16, 319);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(38, 13);
			this.label9.TabIndex = 2;
			this.label9.Text = "Token";
			this.textBoxClienteTN.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxClienteTN.Location = new Point(136, 295);
			this.textBoxClienteTN.Name = "textBoxClienteTN";
			this.textBoxClienteTN.Size = new System.Drawing.Size(530, 20);
			this.textBoxClienteTN.TabIndex = 30;
			this.label8.AutoSize = true;
			this.label8.Location = new Point(15, 298);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(39, 13);
			this.label8.TabIndex = 0;
			this.label8.Text = "Cliente";
			this.label10.AutoSize = true;
			this.label10.Location = new Point(417, 450);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(113, 13);
			this.label10.TabIndex = 18;
			this.label10.Text = "Intervalo de Ejecución";
			this.textBoxStartUp.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxStartUp.Increment = new decimal(new int[] { 10, 0, 0, 0 });
			this.textBoxStartUp.InterceptArrowKeys = false;
			this.textBoxStartUp.Location = new Point(633, 448);
			this.textBoxStartUp.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
			this.textBoxStartUp.Name = "textBoxStartUp";
			this.textBoxStartUp.Size = new System.Drawing.Size(38, 20);
			this.textBoxStartUp.TabIndex = 60;
			this.textBoxStartUp.Value = new decimal(new int[] { 50, 0, 0, 0 });
			this.textBoxTokenDF.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxTokenDF.Location = new Point(136, 51);
			this.textBoxTokenDF.Name = "textBoxTokenDF";
			this.textBoxTokenDF.Size = new System.Drawing.Size(530, 20);
			this.textBoxTokenDF.TabIndex = 4;
			this.label2.AutoSize = true;
			this.label2.Location = new Point(13, 54);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(38, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Token";
			this.label5.AutoSize = true;
			this.label5.Location = new Point(14, 75);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(29, 13);
			this.label5.TabIndex = 13;
			this.label5.Text = "URL";
			this.textBoxUrl.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxUrl.Location = new Point(136, 72);
			this.textBoxUrl.Name = "textBoxUrl";
			this.textBoxUrl.Size = new System.Drawing.Size(530, 20);
			this.textBoxUrl.TabIndex = 5;
			this.label1.AutoSize = true;
			this.label1.Location = new Point(14, 33);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Cliente";
			this.label6.AutoSize = true;
			this.label6.Location = new Point(14, 96);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(77, 13);
			this.label6.TabIndex = 15;
			this.label6.Text = "Base de Datos";
			this.textBoxBDDF.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxBDDF.Location = new Point(136, 93);
			this.textBoxBDDF.Name = "textBoxBDDF";
			this.textBoxBDDF.Size = new System.Drawing.Size(530, 20);
			this.textBoxBDDF.TabIndex = 6;
			this.label7.AutoSize = true;
			this.label7.Location = new Point(14, 137);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(116, 13);
			this.label7.TabIndex = 17;
			this.label7.Text = "Plataforma Ecommerce";
			this.textBoxPlataforma.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxPlataforma.Location = new Point(136, 135);
			this.textBoxPlataforma.Name = "textBoxPlataforma";
			this.textBoxPlataforma.Size = new System.Drawing.Size(530, 20);
			this.textBoxPlataforma.TabIndex = 8;
			this.pictureBox1.BackColor = Color.FromArgb(0, 192, 192);
			this.pictureBox1.Location = new Point(66, 18);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(600, 1);
			this.pictureBox1.TabIndex = 35;
			this.pictureBox1.TabStop = false;
			this.label11.AutoSize = true;
			this.label11.BackColor = Color.Transparent;
			this.label11.ForeColor = Color.FromArgb(0, 192, 192);
			this.label11.Location = new Point(7, 9);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(58, 13);
			this.label11.TabIndex = 36;
			this.label11.Text = "Dragonfish";
			this.label12.AutoSize = true;
			this.label12.BackColor = Color.Transparent;
			this.label12.ForeColor = Color.FromArgb(0, 192, 192);
			this.label12.Location = new Point(7, 186);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(38, 13);
			this.label12.TabIndex = 38;
			this.label12.Text = "zNube";
			this.pictureBox2.BackColor = Color.FromArgb(0, 192, 192);
			this.pictureBox2.Location = new Point(47, 195);
			this.pictureBox2.Margin = new System.Windows.Forms.Padding(0);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(619, 1);
			this.pictureBox2.TabIndex = 37;
			this.pictureBox2.TabStop = false;
			this.label13.AutoSize = true;
			this.label13.BackColor = Color.Transparent;
			this.label13.ForeColor = Color.FromArgb(0, 192, 192);
			this.label13.Location = new Point(7, 275);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(69, 13);
			this.label13.TabIndex = 40;
			this.label13.Text = "Tienda Nube";
			this.pictureBox3.BackColor = Color.FromArgb(0, 192, 192);
			this.pictureBox3.Location = new Point(47, 284);
			this.pictureBox3.Margin = new System.Windows.Forms.Padding(0);
			this.pictureBox3.Name = "pictureBox3";
			this.pictureBox3.Size = new System.Drawing.Size(619, 1);
			this.pictureBox3.TabIndex = 39;
			this.pictureBox3.TabStop = false;
			this.label14.AutoSize = true;
			this.label14.BackColor = Color.Transparent;
			this.label14.ForeColor = Color.FromArgb(0, 192, 192);
			this.label14.Location = new Point(7, 381);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(44, 13);
			this.label14.TabIndex = 42;
			this.label14.Text = "General";
			this.pictureBox4.BackColor = Color.FromArgb(0, 192, 192);
			this.pictureBox4.Location = new Point(47, 390);
			this.pictureBox4.Margin = new System.Windows.Forms.Padding(0);
			this.pictureBox4.Name = "pictureBox4";
			this.pictureBox4.Size = new System.Drawing.Size(619, 1);
			this.pictureBox4.TabIndex = 41;
			this.pictureBox4.TabStop = false;
			this.label15.AutoSize = true;
			this.label15.Location = new Point(14, 160);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(116, 13);
			this.label15.TabIndex = 43;
			this.label15.Text = "Comprobante Sin Pago";
			this.comboBoxComprobanteSinPago.DropDownStyle = ComboBoxStyle.DropDownList;
			this.comboBoxComprobanteSinPago.FormattingEnabled = true;
			this.comboBoxComprobanteSinPago.Location = new Point(136, 157);
			this.comboBoxComprobanteSinPago.Name = "comboBoxComprobanteSinPago";
			this.comboBoxComprobanteSinPago.Size = new System.Drawing.Size(530, 21);
			this.comboBoxComprobanteSinPago.TabIndex = 9;
			this.textBoxClienteDF.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxClienteDF.ForeColor = SystemColors.WindowText;
			this.textBoxClienteDF.Location = new Point(136, 30);
			this.textBoxClienteDF.Name = "textBoxClienteDF";
			this.textBoxClienteDF.Size = new System.Drawing.Size(530, 20);
			this.textBoxClienteDF.TabIndex = 3;
			this.textBoxArticulo.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxArticulo.Location = new Point(136, 114);
			this.textBoxArticulo.Name = "textBoxArticulo";
			this.textBoxArticulo.Size = new System.Drawing.Size(530, 20);
			this.textBoxArticulo.TabIndex = 7;
			this.label16.AutoSize = true;
			this.label16.Location = new Point(14, 116);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(88, 13);
			this.label16.TabIndex = 54;
			this.label16.Text = "Articulo Generico";
			this.checkBoxDescargarEnvio.AutoSize = true;
			this.checkBoxDescargarEnvio.CheckAlign = ContentAlignment.MiddleRight;
			this.checkBoxDescargarEnvio.Location = new Point(254, 430);
			this.checkBoxDescargarEnvio.Name = "checkBoxDescargarEnvio";
			this.checkBoxDescargarEnvio.Size = new System.Drawing.Size(15, 14);
			this.checkBoxDescargarEnvio.TabIndex = 42;
			this.checkBoxDescargarEnvio.UseVisualStyleBackColor = true;
			this.label17.AutoSize = true;
			this.label17.Location = new Point(417, 411);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(149, 13);
			this.label17.TabIndex = 56;
			this.label17.Text = "Habilitar Publicación de Stock";
			this.label18.AutoSize = true;
			this.label18.Location = new Point(16, 410);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(145, 13);
			this.label18.TabIndex = 57;
			this.label18.Text = "Habilitar Descarga de Ventas";
			this.label19.AutoSize = true;
			this.label19.Location = new Point(16, 430);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(218, 13);
			this.label19.TabIndex = 58;
			this.label19.Text = "Incluir Envio en las Operaciones Ecommerce";
			this.label20.AutoSize = true;
			this.label20.Location = new Point(417, 430);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(225, 13);
			this.label20.TabIndex = 60;
			this.label20.Text = "Generar Movimiento por Combinación Faltante";
			this.checkBoxMovStock.AutoSize = true;
			this.checkBoxMovStock.CheckAlign = ContentAlignment.MiddleRight;
			this.checkBoxMovStock.Location = new Point(655, 430);
			this.checkBoxMovStock.Name = "checkBoxMovStock";
			this.checkBoxMovStock.Size = new System.Drawing.Size(15, 14);
			this.checkBoxMovStock.TabIndex = 51;
			this.checkBoxMovStock.UseVisualStyleBackColor = true;
			this.label21.AutoSize = true;
			this.label21.Location = new Point(16, 450);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(93, 13);
			this.label21.TabIndex = 72;
			this.label21.Text = "Modificar Ordenes";
			this.checkBoxNotificarComprobantesCancelados.AutoSize = true;
			this.checkBoxNotificarComprobantesCancelados.CheckAlign = ContentAlignment.MiddleRight;
			this.checkBoxNotificarComprobantesCancelados.Location = new Point(254, 450);
			this.checkBoxNotificarComprobantesCancelados.Name = "checkBoxNotificarComprobantesCancelados";
			this.checkBoxNotificarComprobantesCancelados.Size = new System.Drawing.Size(15, 14);
			this.checkBoxNotificarComprobantesCancelados.TabIndex = 43;
			this.checkBoxNotificarComprobantesCancelados.UseVisualStyleBackColor = true;
			this.label22.AutoSize = true;
			this.label22.Location = new Point(16, 470);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(67, 13);
			this.label22.TabIndex = 73;
			this.label22.Text = "Ruta Listado";
			this.label22.Visible = false;
			this.buttonBuscarCarpeta.Image = (Image)componentResourceManager.GetObject("buttonBuscarCarpeta.Image");
			this.buttonBuscarCarpeta.Location = new Point(254, 473);
			this.buttonBuscarCarpeta.Name = "buttonBuscarCarpeta";
			this.buttonBuscarCarpeta.Size = new System.Drawing.Size(19, 19);
			this.buttonBuscarCarpeta.TabIndex = 45;
			this.buttonBuscarCarpeta.UseVisualStyleBackColor = true;
			this.buttonBuscarCarpeta.Visible = false;
			this.buttonBuscarCarpeta.Click += new EventHandler(this.buttonBuscarCarpeta_Click);
			this.textBoxRuta.BorderStyle = BorderStyle.FixedSingle;
			this.textBoxRuta.Location = new Point(89, 470);
			this.textBoxRuta.Name = "textBoxRuta";
			this.textBoxRuta.Size = new System.Drawing.Size(159, 20);
			this.textBoxRuta.TabIndex = 44;
			this.textBoxRuta.Visible = false;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = Color.White;
			base.ClientSize = new System.Drawing.Size(691, 535);
			base.Controls.Add(this.textBoxRuta);
			base.Controls.Add(this.buttonBuscarCarpeta);
			base.Controls.Add(this.label22);
			base.Controls.Add(this.label21);
			base.Controls.Add(this.checkBoxNotificarComprobantesCancelados);
			base.Controls.Add(this.label20);
			base.Controls.Add(this.checkBoxMovStock);
			base.Controls.Add(this.label19);
			base.Controls.Add(this.label18);
			base.Controls.Add(this.label17);
			base.Controls.Add(this.checkBoxDescargarEnvio);
			base.Controls.Add(this.textBoxArticulo);
			base.Controls.Add(this.label16);
			base.Controls.Add(this.comboBoxComprobanteSinPago);
			base.Controls.Add(this.label15);
			base.Controls.Add(this.textBoxStartUp);
			base.Controls.Add(this.checkBoxHabilitarStock);
			base.Controls.Add(this.label10);
			base.Controls.Add(this.label14);
			base.Controls.Add(this.checkBoxHabilitarVentas);
			base.Controls.Add(this.pictureBox4);
			base.Controls.Add(this.linkLabelVincularTienda);
			base.Controls.Add(this.textBoxTokenTN);
			base.Controls.Add(this.label13);
			base.Controls.Add(this.label9);
			base.Controls.Add(this.pictureBox3);
			base.Controls.Add(this.textBoxClienteTN);
			base.Controls.Add(this.textBoxBDZnube);
			base.Controls.Add(this.label4);
			base.Controls.Add(this.label8);
			base.Controls.Add(this.textBoxTokenZnube);
			base.Controls.Add(this.label3);
			base.Controls.Add(this.label12);
			base.Controls.Add(this.pictureBox2);
			base.Controls.Add(this.label11);
			base.Controls.Add(this.pictureBox1);
			base.Controls.Add(this.textBoxPlataforma);
			base.Controls.Add(this.label7);
			base.Controls.Add(this.textBoxBDDF);
			base.Controls.Add(this.label6);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.buttonGuardar);
			base.Controls.Add(this.textBoxUrl);
			base.Controls.Add(this.textBoxTokenDF);
			base.Controls.Add(this.textBoxClienteDF);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.label5);
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(707, 574);
			base.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(707, 574);
			base.Name = "Configuracion";
			this.Text = "Integración Venta y Stock";
			((ISupportInitialize)this.textBoxStartUp).EndInit();
			((ISupportInitialize)this.pictureBox1).EndInit();
			((ISupportInitialize)this.pictureBox2).EndInit();
			((ISupportInitialize)this.pictureBox3).EndInit();
			((ISupportInitialize)this.pictureBox4).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void linkLabelVincularTienda_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.OpenUrl("https://www.tiendanube.com/apps/1425/authorize");
		}

		private void OpenUrl(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo("cmd", string.Concat("/c start ", url))
					{
						CreateNoWindow = true
					});
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Process.Start("xdg-open", url);
				}
				else if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					throw;
				}
				else
				{
					Process.Start("open", url);
				}
			}
		}
	}
}