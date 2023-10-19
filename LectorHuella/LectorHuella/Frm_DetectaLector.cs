using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DPUruNet;

namespace LectorHuella
{
    public partial class Frm_DetectaLector : Form
    {
        private ReaderCollection _readers;


        private Main _sender;

        public Main Sender
        {
            get { return _sender; }
            set { _sender = value; }
        }

        private Reader _reader;

        public Reader CurrentReader
        {
            get { return _reader; }
            set { _reader = value; }
        }
        public Frm_DetectaLector()
        {
            InitializeComponent();
        }
       
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            cboReaders.Text = string.Empty;
            cboReaders.Items.Clear();
            cboReaders.SelectedIndex = -1;

            _readers = ReaderCollection.GetReaders();

            foreach (Reader Reader in _readers)
            {
                cboReaders.Items.Add(Reader.Description.SerialNumber);
            }

            if (cboReaders.Items.Count > 0)
            {
                cboReaders.SelectedIndex = 0;                
                btnSelect.Enabled = true;
            }
            else
            {
                btnSelect.Enabled = false;                
            }
        }

        private void Frm_DetectaLector_Load(object sender, EventArgs e)
        {
            btnRefresh_Click(this, new System.EventArgs());
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (_sender.CurrentReader != null)
            {
                _sender.CurrentReader.Dispose();
                _sender.CurrentReader = null;
            }
            _sender.CurrentReader = _readers[cboReaders.SelectedIndex];
            this.Close();
        }
    }
}
