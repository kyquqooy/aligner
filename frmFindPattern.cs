using Euresys.Open_eVision_1_2;
using prjVisionController.Open_eVision;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjVisionController
{
    public partial class frmFindPattern : Form
    {
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private clsEasyFindPattern eFindPattern;
        public frmFindPattern()
        {
            InitializeComponent();
            cboFindMode.DataSource = Enum.GetValues(typeof(EFindContrastMode));

            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();
            eFindPattern = new clsEasyFindPattern(picDisplay1);
            
            dataGridView1.DataSource = eFindPattern.PatternTable;            
        }

        private void frmFindPattern_Load(object sender, EventArgs e)
        {
            InitializeParameter(sender, e);
        }

        private void InitializeParameter(object sender, EventArgs e)
        {
            nudFindCount.Value = Convert.ToDecimal(eFindPattern.SearchField_MaxInstances);
            nudFindScore.Value = Convert.ToDecimal(eFindPattern.SearchField_MinScore);
            cboFindMode.Text = eFindPattern.SearchField_ContrastMode.ToString();
            nudAngleTolerance.Value= Convert.ToDecimal(eFindPattern.Allowances_AngleTolerance);
            nudScaleTolerance.Value= Convert.ToDecimal(eFindPattern.Allowances_ScaleTolerance);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                eFindPattern.Save(saveFileDialog.FileName);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                eFindPattern.Load(openFileDialog.FileName);
                eFindPattern.ShowTrainImage(picTrainPattern);
                InitializeParameter(sender, e);
            }
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                eFindPattern.LoadImage(openFileDialog.FileName);
                eFindPattern.ShowImage();
            }
        }

        private void btnLoadTrainImage_Click(object sender, EventArgs e)
        {
            eFindPattern.SetROI();
            eFindPattern.SetROIStart();
        }

        private void btnSettingROI_Click(object sender, EventArgs e)
        {
            eFindPattern.SetROIStart();
        }

        private void btnTrainImage_Click(object sender, EventArgs e)
        {
            eFindPattern.SetROIEnd();
            eFindPattern.TrainPattern(EPatternType.ConsistentEdges);
            eFindPattern.ShowTrainImage(picTrainPattern);
        }        

        private void btnFindPattern_Click(object sender, EventArgs e)
        {
            eFindPattern.SetROIEnd();

            int maxInstances = Convert.ToInt32(nudFindCount.Value);
            float minScore = Convert.ToSingle(nudFindScore.Value);
            EFindContrastMode contrastMode = (EFindContrastMode)Enum.Parse(typeof(EFindContrastMode), cboFindMode.Text, true);
            float angleTolerance = Convert.ToSingle(nudAngleTolerance.Value);
            float scaleTolerance = Convert.ToSingle(nudScaleTolerance.Value);
            eFindPattern.FindPattern(maxInstances, minScore, contrastMode, angleTolerance, scaleTolerance);
            eFindPattern.GetPattern();
            eFindPattern.ShowResult();
        }        
    }
}
