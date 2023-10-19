using ClasesPublicas;
using DPUruNet;
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

namespace LectorHuella
{
    public partial class Main : Form
    {

        private string _Servidor = string.Empty;
        public Conexion_BD _ConexionBD = null;
        Consultas _Consultas = new Consultas();


        private static Main _instancia;

        public static Main Instancia
        {
            get { return _instancia; }
        }
        public Main()
        {
            InitializeComponent();
            _Servidor = "localhost";
            _instancia = this;
            CrearConexiones();
        }

        private void CrearConexiones()
        {
            string usuario_db = string.Empty;
            string pwd_db = string.Empty;

            usuario_db = "root";
            pwd_db = "987564";
            _ConexionBD = new Conexion_BD(_Servidor, usuario_db, pwd_db);
        }

        

        // When set by child forms, shows s/n and enables buttons.
        public Reader CurrentReader
        {
            get { return currentReader; }
            set
            {
                currentReader = value;
                SendMessage(Action.UpdateReaderState, value);
            }
        }

        private enum Action
        {
            UpdateReaderState
        }

        private delegate void SendMessageCallback(Action state, object payload);

        private void SendMessage(Action state, object payload)
        {
            if (this.txtReaderSelected.InvokeRequired)
            {
                SendMessageCallback d = new SendMessageCallback(SendMessage);
                this.Invoke(d, new object[] { state, payload });
            }
            else
            {
                switch (state)
                {
                    case Action.UpdateReaderState:
                        if ((Reader)payload != null)
                        {
                            txtReaderSelected.Text = ((Reader)payload).Description.SerialNumber;                          
                            btnIdentify.Enabled = true;
                            btnEnroll.Enabled = true;                            
                        }
                        else
                        {
                            txtReaderSelected.Text = String.Empty;                          
                            btnIdentify.Enabled = false;
                            btnEnroll.Enabled = false;                           
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public bool CaptureFinger(ref Fid fid)
        {
            try
            {
                Constants.ResultCode result = currentReader.GetStatus();

                if ((result != Constants.ResultCode.DP_SUCCESS))
                {
                    MessageBox.Show("Get Status Error:  " + result);
                    if (CurrentReader != null)
                    {
                        CurrentReader.Dispose();
                        CurrentReader = null;
                    }
                    return false;
                }

                if ((currentReader.Status.Status == Constants.ReaderStatuses.DP_STATUS_BUSY))
                {
                    Thread.Sleep(50);
                    return true;
                }
                else if ((currentReader.Status.Status == Constants.ReaderStatuses.DP_STATUS_NEED_CALIBRATION))
                {
                    currentReader.Calibrate();
                }
                else if ((currentReader.Status.Status != Constants.ReaderStatuses.DP_STATUS_READY))
                {
                    MessageBox.Show("Get Status:  " + currentReader.Status.Status);
                    if (CurrentReader != null)
                    {
                        CurrentReader.Dispose();
                        CurrentReader = null;
                    }
                    return false;
                }

                CaptureResult captureResult = currentReader.Capture(Constants.Formats.Fid.ANSI, Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT, 5000, currentReader.Capabilities.Resolutions[0]);

                if (captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    MessageBox.Show("Error:  " + captureResult.ResultCode);
                    if (CurrentReader != null)
                    {
                        CurrentReader.Dispose();
                        CurrentReader = null;
                    }
                    return false;
                }

                if (captureResult.Quality == Constants.CaptureQuality.DP_QUALITY_CANCELED)
                {
                    return false;
                }

                if ((captureResult.Quality == Constants.CaptureQuality.DP_QUALITY_NO_FINGER || captureResult.Quality == Constants.CaptureQuality.DP_QUALITY_TIMED_OUT))
                {
                    return true;
                }

                if ((captureResult.Quality == Constants.CaptureQuality.DP_QUALITY_FAKE_FINGER))
                {
                    MessageBox.Show("Quality Error:  " + captureResult.Quality);
                    return true;
                }

                fid = captureResult.Data;

                return true;
            }
            catch
            {
                MessageBox.Show("An error has occurred.");
                if (CurrentReader != null)
                {
                    CurrentReader.Dispose();
                    CurrentReader = null;
                }
                return false;
            }
        }

        private Frm_Enrol _enrollment;
        private void btnEnroll_Click(object sender, EventArgs e)
        {
            if (_enrollment == null)
            {
                _enrollment = new Frm_Enrol();
                _enrollment.Sender = this;
            }

            _enrollment.ShowDialog();

        }

        private Reader currentReader;

        private Frm_DetectaLector _readerSelection;
        private void btnReaderSelect_Click(object sender, EventArgs e)
        {
            if (_readerSelection == null)
            {
                _readerSelection = new Frm_DetectaLector();
                _readerSelection.Sender = this;

            }
            _readerSelection.ShowDialog();

        }

        private Frm_Identificar _identification;
        private void btnIdentify_Click(object sender, EventArgs e)
        {
            if (_identification == null)
            {
                _identification = new Frm_Identificar();
                _identification.Sender = this;
            }

            _identification.ShowDialog();
        }
    }
}
