using Euresys.Open_eVision_1_2;
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
    public partial class frmFindLine : Form
    {
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private clsEasyFindLine eFindLine;
        public frmFindLine()
        {
            InitializeComponent();
            cboTransitionType.DataSource = Enum.GetValues(typeof(ETransitionType));

            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();
            eFindLine = new clsEasyFindLine(picDisplay);

            dgvSampleTable.DataSource = eFindLine.SampleTable;
        }

        private void frmFindLine_Load(object sender, EventArgs e)
        {
            InitializeParameter(sender, e);
        }

        private void InitializeParameter(object sender, EventArgs e)
        {
            nudPosition_CenterX.ValueChanged -= new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_CenterY.ValueChanged -= new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Tolerance.ValueChanged -= new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Length.ValueChanged -= new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Angle.ValueChanged -= new EventHandler(this.ROISetting_ValueChanged);

            cboTransitionType.SelectedValueChanged -= new EventHandler(this.TrainSetting_ValueChanged);
            nudThickness.ValueChanged -= new EventHandler(this.TrainSetting_ValueChanged);
            nudSamplingStep.ValueChanged -= new EventHandler(this.TrainSetting_ValueChanged);
            nudThreshold.ValueChanged -= new EventHandler(this.TrainSetting_ValueChanged);
            nudMinAmplitude.ValueChanged -= new EventHandler(this.TrainSetting_ValueChanged);
            nudMinArea.ValueChanged -= new EventHandler(this.TrainSetting_ValueChanged);

            nudPosition_CenterX.Value = Convert.ToDecimal(eFindLine.Position_CenterX);
            nudPosition_CenterY.Value = Convert.ToDecimal(eFindLine.Position_CenterY);
            nudPosition_Tolerance.Value = Convert.ToDecimal(eFindLine.Position_Tolerance);
            nudPosition_Length.Value = Convert.ToDecimal(eFindLine.Position_Length);
            nudPosition_Angle.Value = Convert.ToDecimal(eFindLine.Position_Angle);

            cboTransitionType.Text = eFindLine.Measurement_TransitionType.ToString();
            nudThickness.Value = Convert.ToDecimal(eFindLine.Measurement_Thickness);
            nudSamplingStep.Value = Convert.ToDecimal(eFindLine.Fitting_SamplingStep);
            nudThreshold.Value = Convert.ToDecimal(eFindLine.Measurement_Threshold);
            nudMinAmplitude.Value = Convert.ToDecimal(eFindLine.Measurement_MinAmplitude);
            nudMinArea.Value = Convert.ToDecimal(eFindLine.Measurement_MinArea);

            nudPosition_CenterX.ValueChanged += new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_CenterY.ValueChanged += new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Tolerance.ValueChanged += new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Length.ValueChanged += new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Angle.ValueChanged += new EventHandler(this.ROISetting_ValueChanged);

            cboTransitionType.SelectedValueChanged += new EventHandler(this.TrainSetting_ValueChanged);
            nudThickness.ValueChanged += new EventHandler(this.TrainSetting_ValueChanged);
            nudSamplingStep.ValueChanged += new EventHandler(this.TrainSetting_ValueChanged);
            nudThreshold.ValueChanged += new EventHandler(this.TrainSetting_ValueChanged);
            nudMinAmplitude.ValueChanged += new EventHandler(this.TrainSetting_ValueChanged);
            nudMinArea.ValueChanged += new EventHandler(this.TrainSetting_ValueChanged);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                eFindLine.Save(saveFileDialog.FileName);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                eFindLine.Load(openFileDialog.FileName);
                InitializeParameter(sender, e);
            }
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                eFindLine.LoadImage(openFileDialog.FileName);
                eFindLine.ShowImage();
            }           
        }        

        private void ROISetting_ValueChanged(object sender, EventArgs e)
        {
            float centerX = Convert.ToSingle(nudPosition_CenterX.Value);
            float centerY = Convert.ToSingle(nudPosition_CenterY.Value);
            float tolerance = Convert.ToSingle(nudPosition_Tolerance.Value);
            float length = Convert.ToSingle(nudPosition_Length.Value);
            float angle = Convert.ToSingle(nudPosition_Angle.Value);

            eFindLine.SetROI(centerX, centerY, tolerance, length, angle); 
            eFindLine.ShowROI();
        }

        private void TrainSetting_ValueChanged(object sender, EventArgs e)
        {
            ETransitionType transitionType = (ETransitionType)Enum.Parse(typeof(ETransitionType), cboTransitionType.Text, true);
            int thickness = Convert.ToInt32(nudThickness.Value);
            float samplingStep = Convert.ToSingle(nudSamplingStep.Value);
            int threshold = Convert.ToInt32(nudThreshold.Value);
            int minAmplitude = Convert.ToInt32(nudMinAmplitude.Value);
            int minArea = Convert.ToInt32(nudMinArea.Value);

            eFindLine.TrainLine(transitionType, thickness, threshold, minAmplitude, minArea, samplingStep);
            eFindLine.FindLine();
            eFindLine.GetSample();
            eFindLine.ShowROI();
        }

        private void btnSetROIStart_Click(object sender, EventArgs e)
        {
            eFindLine.SetROIStart();          
        }

        private void btnSetROIEnd_Click(object sender, EventArgs e)
        {
            eFindLine.SetROIEnd();
        }

        private void btnTrain_Click(object sender, EventArgs e)
        {
            float result_CenterX = 0;
            float result_CenterY = 0;
            float result_Angle = 0;

            eFindLine.FindLine(ref result_CenterX, ref result_CenterY, ref result_Angle);
            txtResults_CenterX.Text = result_CenterX.ToString("0.000");
            txtResults_CenterY.Text = result_CenterY.ToString("0.000");
            txtResults_Angle.Text = result_Angle.ToString("0.000");
            eFindLine.GetSample();
            eFindLine.ShowResult();
        }

        private void picDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            nudPosition_CenterX.Value = Convert.ToDecimal(eFindLine.Position_CenterX);
            nudPosition_CenterY.Value = Convert.ToDecimal(eFindLine.Position_CenterY);
            nudPosition_Tolerance.Value = Convert.ToDecimal(eFindLine.Position_Tolerance);
            nudPosition_Length.Value = Convert.ToDecimal(eFindLine.Position_Length);
            nudPosition_Angle.Value = Convert.ToDecimal(eFindLine.Position_Angle);
        }

        private void picDisplay_MouseEnter(object sender, EventArgs e)
        {
            nudPosition_CenterX.ValueChanged -= new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_CenterY.ValueChanged -= new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Tolerance.ValueChanged -= new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Length.ValueChanged -= new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Angle.ValueChanged -= new EventHandler(this.ROISetting_ValueChanged);
        }

        private void picDisplay_MouseLeave(object sender, EventArgs e)
        {
            nudPosition_CenterX.ValueChanged += new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_CenterY.ValueChanged += new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Tolerance.ValueChanged += new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Length.ValueChanged += new EventHandler(this.ROISetting_ValueChanged);
            nudPosition_Angle.ValueChanged += new EventHandler(this.ROISetting_ValueChanged);            
        }        
    }
}
