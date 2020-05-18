using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

namespace Stock_TN
{
    public partial class Configuracion : Form
    {
        public int startUp;
        public string tokenDragonfish = "";
        public string clienteDragonfish = "";
        public string urlDragonfish = "";
        public string plataformaEcommerce = "";
        public string basesDeDatos;

        public string tokenTiendaNube = "";
        public string clienteTiendaNube = "";

        public string tokenStockOnline = "";
        public string resourceId = "";

        public bool habilitarPublicacionStock = false;
        public bool habilitarDescargaVentas = false;
        Singleton singleton = null;


        public Configuracion()
        {
            singleton = Singleton.Instance;
            InitializeComponent();

            textBoxTokenZnube.Enabled = checkBoxHabilitarStock.Checked;
            textBoxBDZnube.Enabled = checkBoxHabilitarStock.Checked;

            comboBoxComprobanteSinPago.Enabled = checkBoxHabilitarVentas.Checked;
            textBoxBDDF.Enabled = checkBoxHabilitarVentas.Checked;
            textBoxPlataforma.Enabled = checkBoxHabilitarVentas.Checked;

            comboBoxComprobanteSinPago.Items.Insert(0, "");
            comboBoxComprobanteSinPago.Items.Insert(1, "Remito");
            comboBoxComprobanteSinPago.Items.Insert(2, "Pedido");

            textBoxClienteDF.Text = singleton.clienteDragonfish;
            textBoxTokenDF.Text = singleton.tokenDragonfish;
            textBoxUrl.Text = singleton.urlDragonfish;
            textBoxBDDF.Text = singleton.basesDeDatos;
            textBoxPlataforma.Text = singleton.plataformaEcommerce;
            comboBoxComprobanteSinPago.SelectedIndex = singleton.comprobanteSinPago;

            textBoxTokenZnube.Text = singleton.tokenStockOnline;
            textBoxBDZnube.Text = singleton.resourceId;

            textBoxClienteTN.Text = singleton.clienteTiendaNube;
            textBoxTokenTN.Text = singleton.tokenTiendaNube;

            textBoxStartUp.Value = singleton.startUp;
            checkBoxHabilitarStock.Checked = singleton.habilitarPublicacionStock;
            checkBoxHabilitarVentas.Checked = singleton.habilitarDescargaVentas;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void linkLabelVincularTienda_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.tiendanube.com/apps/1425/authorize");
        }

        private void buttonGuardar_Click(object sender, EventArgs e)
        {
            if (!checkBoxHabilitarVentas.Checked && !checkBoxHabilitarStock.Checked)
            {
                MessageBox.Show("Debe activar al menos una configuración.", singleton.origenDFTN, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            bool camposVacios = false;

            if (checkBoxHabilitarVentas.Checked)
            {
                if (comboBoxComprobanteSinPago.SelectedIndex == 0 || textBoxBDDF.Text.ToString() == "" || textBoxPlataforma.Text.ToString() == "")
                {
                    camposVacios = true;
                }
            }

            if (checkBoxHabilitarStock.Checked)
            {
                if(textBoxTokenZnube.Text.ToString() == "" || textBoxBDZnube.Text.ToString() == "")
                {
                    camposVacios = true;
                }
            }

            if(textBoxClienteTN.Text.ToString() == "" || textBoxTokenTN.Text.ToString() == "" || textBoxClienteDF.Text.ToString() == "" || textBoxTokenDF.Text.ToString() == "" || textBoxUrl.Text.ToString() == "" || textBoxClienteTN.Text.ToString() == "" || textBoxTokenTN.Text.ToString() == "")
            {
                camposVacios = true;
            }

            if (!camposVacios)
            {
                actualizarXLM();
                Close();
                Program.iniciarApp();
            }
            else
            {
                MessageBox.Show("Completar todos lo campos.", singleton.origenDFTN, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        public void actualizarXLM()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(singleton.config);

            XmlNode checkBoxHabilitarVentasNode = doc.SelectSingleNode("Application/HabilitarDescargaVentas"); 
            checkBoxHabilitarVentasNode.InnerText = checkBoxHabilitarVentas.Checked.ToString();

            XmlNode checkBoxHabilitarStockNode = doc.SelectSingleNode("Application/HabilitarPublicacionStock");
            checkBoxHabilitarStockNode.InnerText = checkBoxHabilitarStock.Checked.ToString();

            XmlNode textBoxStartUpNode = doc.SelectSingleNode("Application/StartUp"); 
            textBoxStartUpNode.InnerText = textBoxStartUp.Value.ToString();

            XmlNode textBoxClienteDFNode = doc.SelectSingleNode("Application/ClienteDragonfish"); 
            textBoxClienteDFNode.InnerText = textBoxClienteDF.Text.ToString();

            XmlNode textBoxTokenDFLNode = doc.SelectSingleNode("Application/TokenDragonfish"); 
            textBoxTokenDFLNode.InnerText = textBoxTokenDF.Text.ToString();

            XmlNode textBoxUrNode = doc.SelectSingleNode("Application/UrlDragonfish"); 
            textBoxUrNode.InnerText = textBoxUrl.Text.ToString();

            XmlNode textBoxBDDFNode = doc.SelectSingleNode("Application/BasesDeDatos");
            textBoxBDDFNode.InnerText = textBoxBDDF.Text.ToString();

            XmlNode textBoxPlataformaNode = doc.SelectSingleNode("Application/PlataformaEcommerce"); 
            textBoxPlataformaNode.InnerText = textBoxPlataforma.Text.ToString();

            XmlNode comboBoxComprobanteSinPagoNode = doc.SelectSingleNode("Application/ComprobanteSinPago");
            comboBoxComprobanteSinPagoNode.InnerText = comboBoxComprobanteSinPago.SelectedIndex.ToString();

            XmlNode textBoxClienteTNNode = doc.SelectSingleNode("Application/ClienteTiendaNube"); 
            textBoxClienteTNNode.InnerText = textBoxClienteTN.Text.ToString();

            XmlNode textBoxTokenTNNode = doc.SelectSingleNode("Application/TokenTiendaNube"); 
            textBoxTokenTNNode.InnerText = textBoxTokenTN.Text.ToString();

            XmlNode textBoxTokenZnubeNode = doc.SelectSingleNode("Application/TokenStockOnline"); 
            textBoxTokenZnubeNode.InnerText = textBoxTokenZnube.Text.ToString();

            XmlNode textBoxBDZnubeNode = doc.SelectSingleNode("Application/ResourcesID"); 
            textBoxBDZnubeNode.InnerText = textBoxBDZnube.Text.ToString();

            doc.Save(singleton.config);
            singleton.getXml();
        }

        private void checkBoxHabilitarStock_CheckedChanged(object sender, EventArgs e)
        {
            textBoxTokenZnube.Enabled = checkBoxHabilitarStock.Checked;
            textBoxBDZnube.Enabled = checkBoxHabilitarStock.Checked;
        }

        private void checkBoxHabilitarVentas_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxComprobanteSinPago.Enabled = checkBoxHabilitarVentas.Checked;
            textBoxBDDF.Enabled = checkBoxHabilitarVentas.Checked;
            textBoxPlataforma.Enabled = checkBoxHabilitarVentas.Checked;
        }
    }
}
