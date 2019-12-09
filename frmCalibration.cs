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
    public partial class frmCalibration : Form
    {        
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private clsEasyCalibration eCalibration;
        public frmCalibration()
        {
            InitializeComponent();

            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();
            eCalibration = new clsEasyCalibration(picDisplay);

            dataGridView1.DataSource = eCalibration.CalibrTable;
        }

        private void frmCalibration_Load(object sender, EventArgs e)
        {
            InitializeParameter(sender, e);

            //List<CalibrationPoint> calibrPoint = new List<CalibrationPoint>();
            //for (int i = 0; i < 1000; i = i + 100)
            //{
            //    for (int j = 0; j < 1000; j = j + 100)
            //    {
            //        calibrPoint.Add(new CalibrationPoint(i, j, i * 10, j * 10));
            //    }
            //}
            //eCalibration.SetCalibrPoint(calibrPoint.ToArray());
        }

        private void InitializeParameter(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                eCalibration.Save(saveFileDialog.FileName);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                eCalibration.Load(openFileDialog.FileName);
                InitializeParameter(sender, e);
                eCalibration.ShowResult();
            }
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                eCalibration.LoadImage(openFileDialog.FileName);
                eCalibration.ShowImage();
            }
        }

        private void btnCalibration_Click(object sender, EventArgs e)
        {
            eCalibration.Calibration();
            eCalibration.ShowResult();
        }
    }
}
