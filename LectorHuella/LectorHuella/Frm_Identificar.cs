using LectorHuella.ClasesPublicas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DPUruNet;

namespace LectorHuella
{
    public partial class Frm_Identificar : Form
    {
        private const int DPFJ_PROBABILITY_ONE = 0x7fffffff;

        private bool reset = false;

        private Thread identifyThreadHandle;

        private Main _sender;

        Consultas _Consultas = new Consultas();

        public Main Sender
        {
            get { return _sender; }
            set { _sender = value; }
        }
        public Frm_Identificar()
        {
            InitializeComponent();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void IdentifyThread()
        {
            Constants.ResultCode result = Constants.ResultCode.DP_DEVICE_FAILURE;

            result = _sender.CurrentReader.Open(Constants.CapturePriority.DP_PRIORITY_COOPERATIVE);

            if (result != Constants.ResultCode.DP_SUCCESS)
            {
                MessageBox.Show("Error:  " + result);
                if (_sender.CurrentReader != null)
                {
                    _sender.CurrentReader.Dispose();
                    _sender.CurrentReader = null;
                }
                return;
            }

            Fmd fmd1 = null;

            SendMessage("Coloque su dedo sobre el lector.");

            //int count = 0;
            while (!reset)
            {
                Fid fid = null;

                if (!_sender.CaptureFinger(ref fid))
                {
                    break;
                }

                if (fid == null)
                {
                    continue;
                }

                SendMessage("Huella capturada identificando...");

                DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(fid, Constants.Formats.Fmd.ANSI);

                if (resultConversion.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    break;
                }

                fmd1 = resultConversion.Data;

                DataTable sql = _Consultas.Huellas();
                if (sql.Rows.Count > 0)
                {
                    // See the SDK documentation for an explanation on threshold scores.
                    int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;

                    Fmd[] fmds = new Fmd[1];
                    //Fmd[] fmds = new Fmd[sql.Rows.Count];
                    //int p = 0;
                    string numero = string.Empty;
                    bool encontro = false;
                    foreach (DataRow item in sql.Rows)
                    {
                        var huella = item["huella"].ToString();
                        numero = item["id"].ToString();
                        var DeserializeXml = Fmd.DeserializeXml(huella);
                        fmds[0] = DeserializeXml;
                        // comparar FMD capturada con los de la base de datos
                        IdentifyResult identifyResult = Comparison.Identify(fmd1, 0, fmds, thresholdScore, 1);

                        if (identifyResult.Indexes.Length > 0)
                        {
                            encontro = true;
                            break;
                        }
                        //p++;
                    }

                    if (encontro)
                    {
                        SendMessage("Huella SI encontrada. Colaborador " + numero);
                    }
                    else
                    {
                        SendMessage("Huella NO encontrada.");
                    }
                    SendMessage("----------------------------.");
                    //if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    //{
                    //    break;
                    //}

                }
                else
                {
                    SendMessage("No se encontraron huellas para comparar, favor de registrar alguna.");
                }
            }

            if (_sender.CurrentReader != null)
                _sender.CurrentReader.Dispose();
        }

        private delegate void SendMessageCallback(string payload);

        private void SendMessage(string payload)
        {
            if (this.txtIdentify.InvokeRequired)
            {
                SendMessageCallback d = new SendMessageCallback(SendMessage);
                this.Invoke(d, new object[] { payload });
            }
            else
            {
                txtIdentify.Text += payload + "\r\n\r\n";
                txtIdentify.SelectionStart = txtIdentify.TextLength;
                txtIdentify.ScrollToCaret();
            }
        }
        private void Frm_Identificar_Load(object sender, EventArgs e)
        {
            reset = false;

            identifyThreadHandle = new Thread(IdentifyThread);
            identifyThreadHandle.IsBackground = true;
            identifyThreadHandle.Start();
        }

        private void Frm_Identificar_Closed(object sender, System.EventArgs e)
        {
            
        }

        private void Frm_Identificar_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_sender.CurrentReader != null)
            {
                reset = true;
                _sender.CurrentReader.CancelCapture();

                if (identifyThreadHandle != null)
                {
                    identifyThreadHandle.Join(5000);
                }
            }

            txtIdentify.Text = string.Empty;
        }
    }
}
