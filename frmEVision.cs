using Euresys.Open_eVision_1_2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjVisionController.Open_eVision
{
    /// <summary>
    /// ax+by+c = 0
    /// </summary>
    //public struct Line
    //{
    //    public double a { get; }
    //    public double b { get; }
    //    public double c { get; }
    //    public Line(double x, double y, double angle)
    //    {
    //        //y-y1 = m(x-x1)
    //        //y = mx+b
    //        double m = Math.Tan(angle / 180 * Math.PI);
    //        double b = y - (m * x);
    //        //ax+by+c = 0
    //        this.a = m;
    //        this.b = -1;
    //        this.c = b;
    //    }
    //}

    //public struct Point
    //{
    //    public double x { get; }
    //    public double y { get; }
    //    public Point(Line line_1, Line line_2)
    //    {
    //        //解聯立方程式
    //        double a = line_1.a - line_2.a;
    //        double c = line_1.c - line_2.c;
    //        double cornerX = -c / a;
    //        double cornerY = (line_1.a * cornerX) + line_1.c;

    //        this.x = cornerX;
    //        this.y = cornerY;
    //    }
    //}

    public partial class frmEVision : Form
    {
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private clsEasyCalibration eCalibration;
        private clsEasyFindPattern eCalibrFindPattern;
        private List<CalibrationPoint> calibrPoint;
        private clsEasyFindPattern eFindPattern;
        private clsEasyFindLine eFindLine_1;
        private clsEasyFindLine eFindLine_2;
        private clsEasyFixture eFixture;
        private ezLine line_1;
        private ezLine line_2;

        private readonly string visionPath = Application.StartupPath;
        private readonly string visionFile = "VisionFile";
        public frmEVision()
        {
            InitializeComponent();
            cboCalibrFindMode.DataSource = Enum.GetValues(typeof(EFindContrastMode));
            cboFindMode.DataSource = Enum.GetValues(typeof(EFindContrastMode));
            cboLine_1_TransitionType.DataSource = Enum.GetValues(typeof(ETransitionType));
            cboLine_2_TransitionType.DataSource = Enum.GetValues(typeof(ETransitionType));

            Directory.CreateDirectory(visionPath + "\\" + visionFile);
            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();

            eCalibration = new clsEasyCalibration(picDisplay);
            eCalibrFindPattern = new clsEasyFindPattern(picDisplay);
            calibrPoint = new List<CalibrationPoint>();
            eFindPattern = new clsEasyFindPattern(picDisplay);

            eFindLine_1 = new clsEasyFindLine(picDisplay);
            eFindLine_2 = new clsEasyFindLine(picDisplay);

            eFixture = new clsEasyFixture(picDisplay);
            eFindLine_1.Base.Attach(eFixture.Base);
            eFindLine_2.Base.Attach(eFixture.Base);

            line_1 = new ezLine(0, 0, 0);
            line_2 = new ezLine(0, 0, 0);

            dgvCalibrPointTable.DataSource = eCalibration.CalibrTable;
            dgvCalibrPatternTable.DataSource = eCalibrFindPattern.PatternTable;
            dgvPatternTable.DataSource= eFindPattern.PatternTable;
            dgvLine_1_SampleTable.DataSource = eFindLine_1.SampleTable;
            dgvLine_2_SampleTable.DataSource = eFindLine_2.SampleTable;
        }

        private void frmEVision_Load(object sender, EventArgs e)
        {
            InitializeCalibrationParameter(sender, e);
            InitializeFindCornerParameter(sender, e);
        }

        private void InitializeCalibrationParameter(object sender, EventArgs e)
        {
            nudCalibrFindScore.Value = Convert.ToDecimal(eCalibrFindPattern.SearchField_MinScore);
            cboCalibrFindMode.Text = eCalibrFindPattern.SearchField_ContrastMode.ToString();
            nudCalibrAngleTolerance.Value = Convert.ToDecimal(eCalibrFindPattern.Allowances_AngleTolerance);
            nudCalibrScaleTolerance.Value = Convert.ToDecimal(eCalibrFindPattern.Allowances_ScaleTolerance);

            nudFindCount.Value = Convert.ToDecimal(eFindPattern.SearchField_MaxInstances);
            nudFindScore.Value = Convert.ToDecimal(eFindPattern.SearchField_MinScore);
            cboFindMode.Text = eFindPattern.SearchField_ContrastMode.ToString();
            nudAngleTolerance.Value = Convert.ToDecimal(eFindPattern.Allowances_AngleTolerance);
            nudScaleTolerance.Value = Convert.ToDecimal(eFindPattern.Allowances_ScaleTolerance);
        }

        private void InitializeFindCornerParameter(object sender, EventArgs e)
        {
            nudLine_1_Position_CenterX.ValueChanged -= new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_CenterY.ValueChanged -= new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Tolerance.ValueChanged -= new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Length.ValueChanged -= new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Angle.ValueChanged -= new EventHandler(this.Line_1_ROISetting_ValueChanged);

            cboLine_1_TransitionType.SelectedValueChanged -= new EventHandler(this.Line_1_TrainSetting_ValueChanged);
            nudLine_1_Thickness.ValueChanged -= new EventHandler(this.Line_1_TrainSetting_ValueChanged);
            nudLine_1_SamplingStep.ValueChanged -= new EventHandler(this.Line_1_TrainSetting_ValueChanged);
            nudLine_1_Threshold.ValueChanged -= new EventHandler(this.Line_1_TrainSetting_ValueChanged);
            nudLine_1_MinAmplitude.ValueChanged -= new EventHandler(this.Line_1_TrainSetting_ValueChanged);
            nudLine_1_MinArea.ValueChanged -= new EventHandler(this.Line_1_TrainSetting_ValueChanged);

            nudLine_2_Position_CenterX.ValueChanged -= new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_CenterY.ValueChanged -= new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Tolerance.ValueChanged -= new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Length.ValueChanged -= new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Angle.ValueChanged -= new EventHandler(this.Line_2_ROISetting_ValueChanged);

            cboLine_2_TransitionType.SelectedValueChanged -= new EventHandler(this.Line_2_TrainSetting_ValueChanged);
            nudLine_2_Thickness.ValueChanged -= new EventHandler(this.Line_2_TrainSetting_ValueChanged);
            nudLine_2_SamplingStep.ValueChanged -= new EventHandler(this.Line_2_TrainSetting_ValueChanged);
            nudLine_2_Threshold.ValueChanged -= new EventHandler(this.Line_2_TrainSetting_ValueChanged);
            nudLine_2_MinAmplitude.ValueChanged -= new EventHandler(this.Line_2_TrainSetting_ValueChanged);
            nudLine_2_MinArea.ValueChanged -= new EventHandler(this.Line_2_TrainSetting_ValueChanged);

            nudLine_1_Position_CenterX.Value = Convert.ToDecimal(eFindLine_1.Position_CenterX);
            nudLine_1_Position_CenterY.Value = Convert.ToDecimal(eFindLine_1.Position_CenterY);
            nudLine_1_Position_Tolerance.Value = Convert.ToDecimal(eFindLine_1.Position_Tolerance);
            nudLine_1_Position_Length.Value = Convert.ToDecimal(eFindLine_1.Position_Length);
            nudLine_1_Position_Angle.Value = Convert.ToDecimal(eFindLine_1.Position_Angle);

            cboLine_1_TransitionType.Text = eFindLine_1.Measurement_TransitionType.ToString();
            nudLine_1_Thickness.Value = Convert.ToDecimal(eFindLine_1.Measurement_Thickness);
            nudLine_1_SamplingStep.Value = Convert.ToDecimal(eFindLine_1.Fitting_SamplingStep);
            nudLine_1_Threshold.Value = Convert.ToDecimal(eFindLine_1.Measurement_Threshold);
            nudLine_1_MinAmplitude.Value = Convert.ToDecimal(eFindLine_1.Measurement_MinAmplitude);
            nudLine_1_MinArea.Value = Convert.ToDecimal(eFindLine_1.Measurement_MinArea);

            nudLine_2_Position_CenterX.Value = Convert.ToDecimal(eFindLine_2.Position_CenterX);
            nudLine_2_Position_CenterY.Value = Convert.ToDecimal(eFindLine_2.Position_CenterY);
            nudLine_2_Position_Tolerance.Value = Convert.ToDecimal(eFindLine_2.Position_Tolerance);
            nudLine_2_Position_Length.Value = Convert.ToDecimal(eFindLine_2.Position_Length);
            nudLine_2_Position_Angle.Value = Convert.ToDecimal(eFindLine_2.Position_Angle);

            cboLine_2_TransitionType.Text = eFindLine_2.Measurement_TransitionType.ToString();
            nudLine_2_Thickness.Value = Convert.ToDecimal(eFindLine_2.Measurement_Thickness);
            nudLine_2_SamplingStep.Value = Convert.ToDecimal(eFindLine_2.Fitting_SamplingStep);
            nudLine_2_Threshold.Value = Convert.ToDecimal(eFindLine_2.Measurement_Threshold);
            nudLine_2_MinAmplitude.Value = Convert.ToDecimal(eFindLine_2.Measurement_MinAmplitude);
            nudLine_2_MinArea.Value = Convert.ToDecimal(eFindLine_2.Measurement_MinArea);

            nudLine_1_Position_CenterX.ValueChanged += new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_CenterY.ValueChanged += new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Tolerance.ValueChanged += new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Length.ValueChanged += new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Angle.ValueChanged += new EventHandler(this.Line_1_ROISetting_ValueChanged);

            cboLine_1_TransitionType.SelectedValueChanged += new EventHandler(this.Line_1_TrainSetting_ValueChanged);
            nudLine_1_Thickness.ValueChanged += new EventHandler(this.Line_1_TrainSetting_ValueChanged);
            nudLine_1_SamplingStep.ValueChanged += new EventHandler(this.Line_1_TrainSetting_ValueChanged);
            nudLine_1_Threshold.ValueChanged += new EventHandler(this.Line_1_TrainSetting_ValueChanged);
            nudLine_1_MinAmplitude.ValueChanged += new EventHandler(this.Line_1_TrainSetting_ValueChanged);
            nudLine_1_MinArea.ValueChanged += new EventHandler(this.Line_1_TrainSetting_ValueChanged);

            nudLine_2_Position_CenterX.ValueChanged += new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_CenterY.ValueChanged += new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Tolerance.ValueChanged += new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Length.ValueChanged += new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Angle.ValueChanged += new EventHandler(this.Line_2_ROISetting_ValueChanged);

            cboLine_2_TransitionType.SelectedValueChanged += new EventHandler(this.Line_2_TrainSetting_ValueChanged);
            nudLine_2_Thickness.ValueChanged += new EventHandler(this.Line_2_TrainSetting_ValueChanged);
            nudLine_2_SamplingStep.ValueChanged += new EventHandler(this.Line_2_TrainSetting_ValueChanged);
            nudLine_2_Threshold.ValueChanged += new EventHandler(this.Line_2_TrainSetting_ValueChanged);
            nudLine_2_MinAmplitude.ValueChanged += new EventHandler(this.Line_2_TrainSetting_ValueChanged);
            nudLine_2_MinArea.ValueChanged += new EventHandler(this.Line_2_TrainSetting_ValueChanged);
        }

        private void Line_1_ROISetting_ValueChanged(object sender, EventArgs e)
        {
            float centerX = Convert.ToSingle(nudLine_1_Position_CenterX.Value);
            float centerY = Convert.ToSingle(nudLine_1_Position_CenterY.Value);
            float tolerance = Convert.ToSingle(nudLine_1_Position_Tolerance.Value);
            float length = Convert.ToSingle(nudLine_1_Position_Length.Value);
            float angle = Convert.ToSingle(nudLine_1_Position_Angle.Value);

            eFindLine_1.SetROI(centerX, centerY, tolerance, length, angle);
            eFindLine_1.ShowROI();
        }

        private void Line_2_ROISetting_ValueChanged(object sender, EventArgs e)
        {
            float centerX = Convert.ToSingle(nudLine_2_Position_CenterX.Value);
            float centerY = Convert.ToSingle(nudLine_2_Position_CenterY.Value);
            float tolerance = Convert.ToSingle(nudLine_2_Position_Tolerance.Value);
            float length = Convert.ToSingle(nudLine_2_Position_Length.Value);
            float angle = Convert.ToSingle(nudLine_2_Position_Angle.Value);

            eFindLine_2.SetROI(centerX, centerY, tolerance, length, angle);
            eFindLine_2.ShowROI();
        }

        private void Line_1_TrainSetting_ValueChanged(object sender, EventArgs e)
        {
            ETransitionType transitionType = (ETransitionType)Enum.Parse(typeof(ETransitionType), cboLine_1_TransitionType.Text, true);
            int thickness = Convert.ToInt32(nudLine_1_Thickness.Value);
            float samplingStep = Convert.ToSingle(nudLine_1_SamplingStep.Value);
            int threshold = Convert.ToInt32(nudLine_1_Threshold.Value);
            int minAmplitude = Convert.ToInt32(nudLine_1_MinAmplitude.Value);
            int minArea = Convert.ToInt32(nudLine_1_MinArea.Value);

            eFindLine_1.TrainLine(transitionType, thickness, threshold, minAmplitude, minArea, samplingStep);
            eFindLine_1.FindLine();
            eFindLine_1.GetSample();
            eFindLine_1.ShowROI();
        }

        private void Line_2_TrainSetting_ValueChanged(object sender, EventArgs e)
        {
            ETransitionType transitionType = (ETransitionType)Enum.Parse(typeof(ETransitionType), cboLine_2_TransitionType.Text, true);
            int thickness = Convert.ToInt32(nudLine_2_Thickness.Value);
            float samplingStep = Convert.ToSingle(nudLine_2_SamplingStep.Value);
            int threshold = Convert.ToInt32(nudLine_2_Threshold.Value);
            int minAmplitude = Convert.ToInt32(nudLine_2_MinAmplitude.Value);
            int minArea = Convert.ToInt32(nudLine_2_MinArea.Value);

            eFindLine_2.TrainLine(transitionType, thickness, threshold, minAmplitude, minArea, samplingStep);
            eFindLine_2.FindLine();
            eFindLine_2.GetSample();
            eFindLine_2.ShowROI();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {            
            eCalibration.Save(visionPath + "\\" + visionFile + "\\Calibration");
            eCalibrFindPattern.Save(visionPath + "\\" + visionFile + "\\CalibrationFindPattern");
            eFindPattern.Save(visionPath + "\\" + visionFile + "\\FindPattern");
            eFindLine_1.Save(visionPath + "\\" + visionFile + "\\FindLine_1");
            eFindLine_2.Save(visionPath + "\\" + visionFile + "\\FindLine_2");
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            eCalibration.Load(visionPath + "\\" + visionFile + "\\Calibration");
            eCalibration.ShowResult();
            eCalibrFindPattern.Load(visionPath + "\\" + visionFile + "\\CalibrationFindPattern");
            eCalibrFindPattern.ShowTrainImage(picCalibrTrainImage);
            eFindPattern.Load(visionPath + "\\" + visionFile + "\\FindPattern");
            eFindPattern.ShowTrainImage(picTrainImage);
            eFindLine_1.Load(visionPath + "\\" + visionFile + "\\FindLine_1");
            eFindLine_2.Load(visionPath + "\\" + visionFile + "\\FindLine_2");

            frmEVision_Load(sender, e);
        }

        private void btnLoadIamge_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                eCalibration.LoadImage(openFileDialog.FileName);
                eCalibrFindPattern.InputImage = eCalibration.InputImage;
                eFindPattern.InputImage= eCalibration.InputImage;
                eFindLine_1.InputImage = eCalibration.InputImage;
                eFindLine_2.InputImage = eCalibration.InputImage;
                eCalibration.ShowImage();                
            }
        }        

        private void btnLoadCalibrTrainImage_Click(object sender, EventArgs e)
        {
            eCalibrFindPattern.SetROI();
            eCalibrFindPattern.SetROIStart();
        }

        private void btnSetCalibrTrainROI_Click(object sender, EventArgs e)
        {
            eCalibrFindPattern.SetROIStart();
        }

        private void btnTrainCalibrImage_Click(object sender, EventArgs e)
        {
            eCalibrFindPattern.SetROIEnd();
            eCalibrFindPattern.TrainPattern(EPatternType.ConsistentEdges);
            eCalibrFindPattern.ShowTrainImage(picCalibrTrainImage);
        }

        private void btnFindCalibrPattern_Click(object sender, EventArgs e)
        {
            eCalibrFindPattern.SetROIEnd();

            float minScore = Convert.ToSingle(nudCalibrFindScore.Value);
            EFindContrastMode contrastMode = (EFindContrastMode)Enum.Parse(typeof(EFindContrastMode), cboCalibrFindMode.Text, true);
            float angleTolerance = Convert.ToSingle(nudCalibrAngleTolerance.Value);
            float scaleTolerance = Convert.ToSingle(nudCalibrScaleTolerance.Value);
            eCalibrFindPattern.FindPattern(1, minScore, contrastMode, angleTolerance, scaleTolerance);
            eCalibrFindPattern.GetPattern();
            eCalibrFindPattern.ShowResult();
        }

        private void btnAddCalibrPoint_Click(object sender, EventArgs e)
        {
            CalibrationPoint point = new CalibrationPoint();
            point.Pixel_X = eCalibrFindPattern.Results_CenterX;
            point.Pixel_Y = eCalibrFindPattern.Results_CenterY;
            point.World_X = 0;
            point.World_Y = 0;
            calibrPoint.Add(point);
            eCalibration.SetCalibrPoint(calibrPoint.ToArray());
        }

        private void btnRemoveCalibr_Click(object sender, EventArgs e)
        {
            calibrPoint.Clear();
            eCalibration.CalibrTable.Clear();
        }

        private void btnCalibration_Click(object sender, EventArgs e)
        {
            eCalibration.Calibration();
        }

        private void btnLoadCalibrImage_Click(object sender, EventArgs e)
        {
            //string[] strArray = Directory.GetFiles(visionPath + "\\" + visionFile + "\\CalibrationImage", "*.bmp");
            //for (int i = 0; i < strArray.Length; i++)
            //{
            //    eCalibrFindPattern.LoadImage(strArray[i]);

            //    float minScore = Convert.ToSingle(nudFindScore.Value);
            //    EFindContrastMode contrastMode = (EFindContrastMode)Enum.Parse(typeof(EFindContrastMode), cboFindMode.Text, true);
            //    float angleTolerance = Convert.ToSingle(nudAngleTolerance.Value);
            //    float scaleTolerance = Convert.ToSingle(nudScaleTolerance.Value);
            //    eCalibrFindPattern.FindPattern(1, minScore, contrastMode, angleTolerance, scaleTolerance);
            //    eCalibrFindPattern.GetPattern();

            //    btnAddCalibrPoint_Click(sender, e);
            //}

            StreamReader streamReader = new StreamReader(visionPath + "\\" + visionFile + "\\CalibrationImage\\CalibrationPoint.txt");
            calibrPoint.Clear();
            string[] point = new string[5];
            do
            {
                point = streamReader.ReadLine().Split('\t');
                calibrPoint.Add(new CalibrationPoint(
                    Convert.ToSingle(point[0]),
                    Convert.ToSingle(point[1]),
                    Convert.ToSingle(point[2]),
                    Convert.ToSingle(point[3])));
            } while (!streamReader.EndOfStream);
            eCalibration.SetCalibrPoint(calibrPoint.ToArray());
            eCalibration.Calibration();
            eCalibration.ShowResult();
        }

        private void btnLoadTrainImage_Click(object sender, EventArgs e)
        {
            eFindPattern.SetROI();
            eFindPattern.SetROIStart();
        }

        private void btnSetTrainROI_Click(object sender, EventArgs e)
        {
            eFindPattern.SetROIStart();
        }

        private void btnTrainImage_Click(object sender, EventArgs e)
        {
            eFindPattern.SetROIEnd();
            eFindPattern.TrainPattern(EPatternType.ConsistentEdges);
            eFindPattern.ShowTrainImage(picTrainImage);
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

            EPoint world = eCalibration.Base.SensorToWorld(new EPoint(eFindPattern.Results_CenterX, eFindPattern.Results_CenterY));
            eFindPattern.PatternTable.Rows.Add(
                eFindPattern.PatternTable.Rows.Count + 1,
                eFindPattern.Results_Score,
                world.X,
                world.Y,
                eFindPattern.Results_Angle,
                eFindPattern.Results_Scale);

            eFindPattern.ShowResult();            

            eFixture.SetOrigin(eFindPattern.Results_CenterX, eFindPattern.Results_CenterY, eFindPattern.Results_Angle);
        }

        private void btnLine_1_LoadROI_Click(object sender, EventArgs e)
        {
            eFindLine_1.ShowImage();
            eFindLine_1.SetROI();
            eFindLine_1.SetROIStart();
        }

        private void btnLine_1_SetROI_Click(object sender, EventArgs e)
        {
            eFindLine_1.ShowImage();
            eFindLine_1.SetROIStart();
        }

        private void btnLine_1_Train_Click(object sender, EventArgs e)
        {
            eFindLine_1.SetROIEnd();
        }

        private void btnLine_1_GetSample_Click(object sender, EventArgs e)
        {
            eFindLine_1.SetROIEnd();

            float result_CenterX = 0;
            float result_CenterY = 0;
            float result_Angle = 0;

            eFindLine_1.FindLine(ref result_CenterX, ref result_CenterY, ref result_Angle);

            EPoint localSensor = eFixture.GetPoint(new EPoint(result_CenterX, result_CenterY), CoordinateHook.NotHook);
            EPoint world = eCalibration.Fixture.GetPoint(new EPoint(localSensor.X, localSensor.Y), CoordinateHook.Hook);

            txtLine_1_Results_Center.Text = "X：" + world.X.ToString("0.000") + " " + "Y：" + world.Y.ToString("0.000");
            txtLine_1_Results_Angle.Text = result_Angle.ToString("0.000");
            eFindLine_1.GetSample();
            eFindLine_1.ShowResult();

            line_1 = new ezLine(world.X, world.Y, result_Angle);
        }

        private void btnLine_2_LoadROI_Click(object sender, EventArgs e)
        {
            eFindLine_2.ShowImage();
            eFindLine_2.SetROI();
            eFindLine_2.SetROIStart();
        }

        private void btnLine_2_SetROI_Click(object sender, EventArgs e)
        {
            eFindLine_2.ShowImage();
            eFindLine_2.SetROIStart();
        }

        private void btnLine_2_Train_Click(object sender, EventArgs e)
        {
            eFindLine_2.SetROIEnd();
        }

        private void btnLine_2_GetSample_Click(object sender, EventArgs e)
        {
            eFindLine_2.SetROIEnd();

            float result_CenterX = 0;
            float result_CenterY = 0;
            float result_Angle = 0;

            eFindLine_2.FindLine(ref result_CenterX, ref result_CenterY, ref result_Angle);

            EPoint localSensor = eFixture.GetPoint(new EPoint(result_CenterX, result_CenterY), CoordinateHook.NotHook);
            EPoint world = eCalibration.Fixture.GetPoint(new EPoint(localSensor.X, localSensor.Y), CoordinateHook.Hook);

            txtLine_2_Results_Center.Text = "X：" + world.X.ToString("0.000") + " " + "Y：" + world.Y.ToString("0.000");
            txtLine_2_Results_Angle.Text = result_Angle.ToString("0.000");
            eFindLine_2.GetSample();
            eFindLine_2.ShowResult();

            line_2 = new ezLine(world.X, world.Y, result_Angle);
        }

        private void picDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            nudLine_1_Position_CenterX.Value = Convert.ToDecimal(eFindLine_1.Position_CenterX);
            nudLine_1_Position_CenterY.Value = Convert.ToDecimal(eFindLine_1.Position_CenterY);
            nudLine_1_Position_Tolerance.Value = Convert.ToDecimal(eFindLine_1.Position_Tolerance);
            nudLine_1_Position_Length.Value = Convert.ToDecimal(eFindLine_1.Position_Length);
            nudLine_1_Position_Angle.Value = Convert.ToDecimal(eFindLine_1.Position_Angle);

            nudLine_2_Position_CenterX.Value = Convert.ToDecimal(eFindLine_2.Position_CenterX);
            nudLine_2_Position_CenterY.Value = Convert.ToDecimal(eFindLine_2.Position_CenterY);
            nudLine_2_Position_Tolerance.Value = Convert.ToDecimal(eFindLine_2.Position_Tolerance);
            nudLine_2_Position_Length.Value = Convert.ToDecimal(eFindLine_2.Position_Length);
            nudLine_2_Position_Angle.Value = Convert.ToDecimal(eFindLine_2.Position_Angle);
        }

        private void picDisplay_MouseEnter(object sender, EventArgs e)
        {
            nudLine_1_Position_CenterX.ValueChanged -= new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_CenterY.ValueChanged -= new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Tolerance.ValueChanged -= new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Length.ValueChanged -= new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Angle.ValueChanged -= new EventHandler(this.Line_1_ROISetting_ValueChanged);

            nudLine_2_Position_CenterX.ValueChanged -= new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_CenterY.ValueChanged -= new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Tolerance.ValueChanged -= new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Length.ValueChanged -= new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Angle.ValueChanged -= new EventHandler(this.Line_2_ROISetting_ValueChanged);
        }

        private void picDisplay_MouseLeave(object sender, EventArgs e)
        {
            nudLine_1_Position_CenterX.ValueChanged += new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_CenterY.ValueChanged += new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Tolerance.ValueChanged += new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Length.ValueChanged += new EventHandler(this.Line_1_ROISetting_ValueChanged);
            nudLine_1_Position_Angle.ValueChanged += new EventHandler(this.Line_1_ROISetting_ValueChanged);

            nudLine_2_Position_CenterX.ValueChanged += new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_CenterY.ValueChanged += new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Tolerance.ValueChanged += new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Length.ValueChanged += new EventHandler(this.Line_2_ROISetting_ValueChanged);
            nudLine_2_Position_Angle.ValueChanged += new EventHandler(this.Line_2_ROISetting_ValueChanged);
        }

        private void btnGetCorner_Click(object sender, EventArgs e)
        {
            ezPoint cornerPoint = new ezPoint(line_1, line_2);

            txtCenter_Results_CenterX.Text = cornerPoint.x.ToString("0.000");
            txtCenter_Results_CenterY.Text = cornerPoint.y.ToString("0.000");
        }
    }
}
