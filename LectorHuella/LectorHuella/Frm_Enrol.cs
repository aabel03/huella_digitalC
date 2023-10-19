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
    public partial class Frm_Enrol : Form
    {

        private bool reset = false;

        private Thread enrollThreadHandle;

        private Main _sender;
        public Main Sender
        {
            get { return _sender; }
            set { _sender = value; }
        }
        public Frm_Enrol()
        {
            InitializeComponent();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Frm_Enrol_Load(object sender, EventArgs e)
        {
            reset = false;

            enrollThreadHandle = new Thread(EnrollThread);
            enrollThreadHandle.IsBackground = true;
            enrollThreadHandle.Start();
        }

        int count = 0;
        private IEnumerable<Fmd> CaptureAndExtractFmd()
        {
            while (!reset)
            {
                DataResult<Fmd> resultConversion;

                try
                {
                    if (count >= 8)
                    {
                        SendMessage("El registro no tuvo exito.  Favor de volver a intentar.");
                        count = 0;
                        break;
                    }

                    Fid fid = null;
                    if (!_sender.CaptureFinger(ref fid))
                    {
                        break;
                    }

                    if (fid == null)
                    {
                        continue;
                    }

                    count++;

                    resultConversion = FeatureExtraction.CreateFmdFromFid(fid, Constants.Formats.Fmd.ANSI);

                    if (resultConversion.ResultCode == Constants.ResultCode.DP_SUCCESS)
                    {
                        var serializarFmd = Fmd.SerializeXml(resultConversion.Data);                   
                        SendMessage("Huella capturada.  \r\nCount:  " + (count));                       
                    }

                    if (resultConversion.ResultCode != Constants.ResultCode.DP_SUCCESS)
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    break;
                }

                yield return resultConversion.Data;
            }
        }

        private void EnrollThread()
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

            count = 0;
            while (!reset)
            {
                if (_sender.CurrentReader == null)
                {
                    break;
                }

                SendMessage("Coloque su dedo sobre el lector.");

                DataResult<Fmd> resultEnrollment = DPUruNet.Enrollment.CreateEnrollmentFmd(Constants.Formats.Fmd.ANSI, CaptureAndExtractFmd());

                if (resultEnrollment.ResultCode == Constants.ResultCode.DP_SUCCESS)
                {
                    var serializarFmd = Fmd.SerializeXml(resultEnrollment.Data);
                    var deserializarFmd = Fmd.DeserializeXml(serializarFmd);
                    SendMessage("Se ha registrado correctamente, ahora puede indentificar.");                  
                    string h = Convert.ToBase64String(resultEnrollment.Data.Bytes);
                    bool inserto = Main.Instancia._ConexionBD.EjecutarComandoSql("INSERT INTO huellas.huellas (huella) VALUES ('" + serializarFmd + "');");
                    count = 0;
                }
            }

            if (_sender.CurrentReader != null)
                _sender.CurrentReader.Dispose();
        }

        private delegate void SendMessageCallback(string payload);

        private void SendMessage(string payload)
        {
            if (this.txtEnroll.InvokeRequired)
            {
                SendMessageCallback d = new SendMessageCallback(SendMessage);
                this.Invoke(d, new object[] { payload });
            }
            else
            {
                txtEnroll.Text += payload + "\r\n\r\n";
                txtEnroll.SelectionStart = txtEnroll.TextLength;
                txtEnroll.ScrollToCaret();
            }
        }

        private void Frm_Enrol_Closed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void Frm_Enrol_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_sender.CurrentReader != null)
            {
                reset = true;
                _sender.CurrentReader.CancelCapture();

                if (enrollThreadHandle != null)
                {
                    enrollThreadHandle.Join(5000);
                }
            }

            txtEnroll.Text = string.Empty;
        }
    }
}
