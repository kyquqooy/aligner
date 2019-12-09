using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjVisionController.Open_eVision
{
    public partial class frmFindBlob : Form
    {
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private clsEasyFindBlob eFindBlob;
        public frmFindBlob()
        {
            InitializeComponent();

            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();
            eFindBlob = new clsEasyFindBlob(picDisplay2);

            dataGridView1.DataSource = eFindBlob.BlobTable;
        }

        private void frmFindBlob_Load(object sender, EventArgs e)
        {

        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                eFindBlob.LoadImage(openFileDialog.FileName);
                eFindBlob.ShowImage();
            }
        }

        private void btnFindBlob_Click(object sender, EventArgs e)
        {
            try
            {
                eFindBlob.FindBlob(59, true, false);
                eFindBlob.GetBlob();
                eFindBlob.ShowResult();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            eFindBlob.ShowResult();
        }
    }
}
