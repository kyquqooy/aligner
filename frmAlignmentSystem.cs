using Euresys.Open_eVision_1_2;
using prjAlarmMessage;
using prjVisionController.Open_eVision;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAAAlignmentSystem
{
    public enum VisionSetting
    {
        Camera_1 = 0,
        Camera_2,
    }

    public partial class frmAlignmentSystem : Form
    {
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private clsBaslerCameras baslerCamera;
        private Dictionary<VisionSetting, clsOpenEVision> eVision;
        private Dictionary<VisionSetting, bool> imageUpDataFlag;
        private VisionSetting visionSetting;
        private CornerLine cornerLine;
        private System.Windows.Forms.Timer tmrContinuousTrigger;
        private System.Windows.Forms.Timer tmrProcess;
        private DataTable alignmentTable;
        private bool prCalibrFlag = false;
        private bool prCalibrCameraPosition = false;
        private bool prCalibrSingleCameraPosition = false;
        private bool prRecordingGesture = false;
        private bool prGetNowPosition = false;
        private bool prMoveTargetPosition = false;
        private bool prRobotAlign = false;

        private EPoint[] alignResult = new EPoint[4];

        public frmAlignmentSystem()
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();
            eVision = new Dictionary<VisionSetting, clsOpenEVision>();
            eVision.Add(VisionSetting.Camera_1, new clsOpenEVision(picDisplay_1, "Camera_1"));
            eVision.Add(VisionSetting.Camera_2, new clsOpenEVision(picDisplay_2, "Camera_2"));
            visionSetting = VisionSetting.Camera_1;
            cornerLine = CornerLine.Line_1;
            cboLineTransitionType.DataSource = Enum.GetValues(typeof(ETransitionType));

            imageUpDataFlag = new Dictionary<VisionSetting, bool>();
            imageUpDataFlag.Add(VisionSetting.Camera_1, false);
            imageUpDataFlag.Add(VisionSetting.Camera_2, false);

            baslerCamera = new clsBaslerCameras();
            baslerCamera.Add(picDisplay_1);
            baslerCamera.Add(picDisplay_2);
            baslerCamera[0].InputImageUpData += FrmAlignmentSystem_InputImageUpData_0;
            baslerCamera[1].InputImageUpData += FrmAlignmentSystem_InputImageUpData_1;

            tmrContinuousTrigger = new System.Windows.Forms.Timer();
            tmrContinuousTrigger.Interval = 10;
            tmrContinuousTrigger.Tick += TmrContinuousTrigger_Tick;

            tmrProcess = new System.Windows.Forms.Timer();
            tmrProcess.Interval = 1;
            tmrProcess.Tick += TmrProcess_Tick;
            tmrProcess.Start();

            dgvCalibr.DataSource = eVision[VisionSetting.Camera_1].Calibration.CalibrTable;

            alignmentTable = new DataTable("alignmentTable");
            alignmentTable.Columns.Add("相機1_X");
            alignmentTable.Columns.Add("相機1_Y");
            alignmentTable.Columns.Add("相機2_X");
            alignmentTable.Columns.Add("相機2_Y");
            alignmentTable.Columns.Add("角度");
            alignmentTable.Columns.Add("對角長度");
            alignmentTable.Columns.Add("長度");
            alignmentTable.Columns.Add("寬度");

            dgvAlignResult.DataSource = alignmentTable;
        }

        public delegate int delDO(int type, bool isOn);
        public delegate ushort delGoToAbsPosition(double x, double y, double z, double θ, bool Action = true, double tTimeOut = 30);
        public delegate ushort delGetTablePosition(ref double x, ref double y, ref double z, ref double θ, bool Action = true);
        public delGoToAbsPosition GoToAbsPosition { get; set; }
        public delGetTablePosition GetTablePosition { get; set; }
        public delDO DO { get; set; }

        public double Z_Up { get; set; } = 20;

        double tableNow_X = 0;
        double tableNow_Y = 0;
        double tableNow_Z = 0;
        double tableNow_θ = 0;

        private void FrmAlignmentSystem_InputImageUpData_0(object sender, EventArgs e)
        {
            eVision[VisionSetting.Camera_1].InputImage = baslerCamera[0].InputImage;
            imageUpDataFlag[VisionSetting.Camera_1] = true;
        }

        private void FrmAlignmentSystem_InputImageUpData_1(object sender, EventArgs e)
        {
            eVision[VisionSetting.Camera_2].InputImage = baslerCamera[1].InputImage;
            imageUpDataFlag[VisionSetting.Camera_2] = true;
        }

        private void frmAlignmentSystem_Load(object sender, EventArgs e)
        {
            pnlOperator.Dock = DockStyle.Fill;
            pnlVisionSetting.Dock = DockStyle.Fill;
            pnlCalibration.Dock = DockStyle.Fill;
            pnlSetting.Dock = DockStyle.Fill;

            baslerCamera.SearchDevice();
            for (int i = 0; i < baslerCamera.Count; i++)
                baslerCamera[i].Open();

            VisionParameterUpData();
        }

        private void frmAlignmentSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            tmrProcess.Stop();
            for (int i = 0; i < baslerCamera.Count; i++)
                baslerCamera[i].Close();
        }

        private void VisionParameterUpData()
        {
            dgvCalibr.DataSource = eVision[visionSetting].Calibration.CalibrTable;

            nudCalibrFindScore.Value = Convert.ToDecimal(eVision[visionSetting].CalibrFindPattern.SearchField_MinScore);
            nudCalibrAngleTolerance.Value = Convert.ToDecimal(eVision[visionSetting].CalibrFindPattern.Allowances_AngleTolerance);
            nudCalibrScaleTolerance.Value = Convert.ToDecimal(eVision[visionSetting].CalibrFindPattern.Allowances_ScaleTolerance);
            eVision[visionSetting].CalibrFindPattern.ShowTrainImage(picVisionCalibr);

            nudPatternFindScore.Value = Convert.ToDecimal(eVision[visionSetting].FindPattern.SearchField_MinScore);
            nudPatternAngleTolerance.Value = Convert.ToDecimal(eVision[visionSetting].FindPattern.Allowances_AngleTolerance);
            nudPatternScaleTolerance.Value = Convert.ToDecimal(eVision[visionSetting].FindPattern.Allowances_ScaleTolerance);
            eVision[visionSetting].FindPattern.ShowTrainImage(picVisionPattern);

            #region Vision Corner           
            cboLineTransitionType.SelectedValueChanged -= new EventHandler(this.Line_TrainSetting_ValueChanged);
            nudLineThickness.ValueChanged -= new EventHandler(this.Line_TrainSetting_ValueChanged);
            nudLineSamplingStep.ValueChanged -= new EventHandler(this.Line_TrainSetting_ValueChanged);
            nudLineThreshold.ValueChanged -= new EventHandler(this.Line_TrainSetting_ValueChanged);

            cboLineTransitionType.Text = eVision[visionSetting].FindCorner[cornerLine].Measurement_TransitionType.ToString();
            nudLineThickness.Value = Convert.ToDecimal(eVision[visionSetting].FindCorner[cornerLine].Measurement_Thickness);
            nudLineSamplingStep.Value = Convert.ToDecimal(eVision[visionSetting].FindCorner[cornerLine].Fitting_SamplingStep);
            nudLineThreshold.Value = Convert.ToDecimal(eVision[visionSetting].FindCorner[cornerLine].Measurement_Threshold);

            cboLineTransitionType.SelectedValueChanged += new EventHandler(this.Line_TrainSetting_ValueChanged);
            nudLineThickness.ValueChanged += new EventHandler(this.Line_TrainSetting_ValueChanged);
            nudLineSamplingStep.ValueChanged += new EventHandler(this.Line_TrainSetting_ValueChanged);
            nudLineThreshold.ValueChanged += new EventHandler(this.Line_TrainSetting_ValueChanged);
            #endregion
        }

        /// <summary>
        /// 頁面切換
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPageChange_Click(object sender, EventArgs e)
        {
            pnlOperator.Visible = false;
            pnlVisionSetting.Visible = false;
            pnlCalibration.Visible = false;
            pnlSetting.Visible = false;

            Button btn = (Button)sender;
            string selectButton = btn.Name.Substring(7);
            TableLayoutPanel pnl = (TableLayoutPanel)pnlPageBase.Controls["pnl" + selectButton];

            switch (pnl.Name)
            {
                case "pnlOperator":
                    pnl.Controls.Add(gbxTitleDisplay_1, 0, 0);
                    pnl.Controls.Add(gbxTitleDisplay_2, 1, 0);
                    break;
                case "pnlVisionSetting":
                    switch (visionSetting)
                    {
                        case VisionSetting.Camera_1:
                            pnlOperator.Controls.Add(gbxTitleDisplay_2, 1, 0);
                            pnlVisionSetting.Controls.Add(gbxTitleDisplay_1, 0, 1);
                            break;
                        case VisionSetting.Camera_2:
                            pnlOperator.Controls.Add(gbxTitleDisplay_1, 0, 0);
                            pnlVisionSetting.Controls.Add(gbxTitleDisplay_2, 0, 1);
                            break;
                    }
                    pnlVisionSetting.Controls.Add(gbxVisionSelect, 1, 0);
                    radVisionCamera_1.CheckedChanged -= new EventHandler(Display_Changed);
                    radVisionCamera_2.CheckedChanged -= new EventHandler(Display_Changed);
                    radVisionCamera_1.CheckedChanged += new EventHandler(Display_Changed);
                    radVisionCamera_2.CheckedChanged += new EventHandler(Display_Changed);
                    break;
                case "pnlCalibration":
                    pnl.Controls.Add(gbxTitleDisplay_1, 0, 0);
                    pnl.Controls.Add(gbxTitleDisplay_2, 0, 1);
                    pnlCalibrationParameter.Controls.Add(gbxVisionSelect, 0, 0);
                    radVisionCamera_1.CheckedChanged -= new EventHandler(Display_Changed);
                    radVisionCamera_2.CheckedChanged -= new EventHandler(Display_Changed);
                    break;
                case "pnlSetting":

                    break;
                default:
                    MessageBox.Show("pnlPage Name Error");
                    break;
            }
            pnl.Visible = true;
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Image fileImage = Image.FromFile(openFileDialog.FileName);
                eVision[visionSetting].InputImage = fileImage;
                switch (visionSetting)
                {
                    case VisionSetting.Camera_1:
                        picDisplay_1.Image = fileImage;
                        break;
                    case VisionSetting.Camera_2:
                        picDisplay_2.Image = fileImage;
                        break;
                }
            }
        }

        private void btnContinuousTrigger_Click(object sender, EventArgs e)
        {
            if (btnContinuousTrigger.Text == "連續取像")
            {
                tmrContinuousTrigger.Start();
                btnContinuousTrigger.Text = "停止取像";
            }
            else
            {
                tmrContinuousTrigger.Stop();
                btnContinuousTrigger.Text = "連續取像";
            }
        }

        private void TmrContinuousTrigger_Tick(object sender, EventArgs e)
        {
            try
            {
                baslerCamera[(int)visionSetting].OneShot();
            }
            catch (EException exc)
            {
                tmrContinuousTrigger.Stop();
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                tmrContinuousTrigger.Stop();
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
        }

        private void btnTriggerCamera_Click(object sender, EventArgs e)
        {
            //for (int i = 0; i < baslerCamera.Count; i++)
            //{
            //    baslerCamera[i].OneShot();
            //    Thread.Sleep(200);
            //}
            baslerCamera[(int)visionSetting].OneShot();
        }

        private void Vision_Changed(object sender, EventArgs e)
        {
            RadioButton rad = (RadioButton)sender;
            switch (rad.Name)
            {
                case "radVisionCamera_1":
                    if (rad.Checked)
                        visionSetting = VisionSetting.Camera_1;
                    break;
                case "radVisionCamera_2":
                    if (rad.Checked)
                        visionSetting = VisionSetting.Camera_2;
                    break;
                default:
                    MessageBox.Show("radVisionCamera Name Error");
                    break;
            }
            VisionParameterUpData();
        }

        private void radCornerLine_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rad = (RadioButton)sender;
            switch (rad.Name)
            {
                case "radCornerLine_1":
                    if (rad.Checked)
                        cornerLine = CornerLine.Line_1;
                    break;
                case "radCornerLine_2":
                    if (rad.Checked)
                        cornerLine = CornerLine.Line_2;
                    break;
                default:
                    MessageBox.Show("CornerLine Name Error");
                    break;
            }
            VisionParameterUpData();
        }

        private void Display_Changed(object sender, EventArgs e)
        {
            RadioButton rad = (RadioButton)sender;
            switch (rad.Name)
            {
                case "radVisionCamera_1":
                    if (rad.Checked)
                    {
                        pnlOperator.Controls.Add(gbxTitleDisplay_2, 1, 0);
                        pnlVisionSetting.Controls.Add(gbxTitleDisplay_1, 0, 1);
                    }
                    break;
                case "radVisionCamera_2":
                    if (rad.Checked)
                    {
                        pnlOperator.Controls.Add(gbxTitleDisplay_1, 0, 0);
                        pnlVisionSetting.Controls.Add(gbxTitleDisplay_2, 0, 1);
                    }
                    break;
                default:
                    MessageBox.Show("radVisionCamera Name Error");
                    break;
            }
        }

        private void btnCalibrLoadROI_Click(object sender, EventArgs e)
        {
            eVision[visionSetting].CalibrFindPattern.ShowImage();
            eVision[visionSetting].CalibrFindPattern.SetROI();
            eVision[visionSetting].CalibrFindPattern.SetROIStart();
        }

        private void btnCalibrSetROI_Click(object sender, EventArgs e)
        {
            eVision[visionSetting].CalibrFindPattern.ShowImage();
            eVision[visionSetting].CalibrFindPattern.SetROIStart();
        }

        private void btnCalibrTrain_Click(object sender, EventArgs e)
        {
            eVision[visionSetting].CalibrFindPattern.SetROIEnd();
            bool isOK= eVision[visionSetting].CalibrFindPattern.TrainPattern(EPatternType.ConsistentEdges);
            eVision[visionSetting].CalibrFindPattern.ShowTrainImage(picVisionCalibr);

            float minScore = Convert.ToSingle(nudCalibrFindScore.Value);
            float angleTolerance = Convert.ToSingle(nudCalibrAngleTolerance.Value);
            float scaleTolerance = Convert.ToSingle(nudCalibrScaleTolerance.Value);
            eVision[visionSetting].CalibrFindPattern.FindPattern(1, minScore, EFindContrastMode.Any, angleTolerance, scaleTolerance);
            eVision[visionSetting].CalibrFindPattern.GetPattern();
            eVision[visionSetting].CalibrFindPattern.ShowResult();
        }

        private void btnPatternLoadROI_Click(object sender, EventArgs e)
        {
            eVision[visionSetting].FindPattern.ShowImage();
            eVision[visionSetting].FindPattern.SetROI();
            eVision[visionSetting].FindPattern.SetROIStart();
        }

        private void btnPatternSetROI_Click(object sender, EventArgs e)
        {
            eVision[visionSetting].FindPattern.ShowImage();
            eVision[visionSetting].FindPattern.SetROIStart();
        }

        private void btnPatternTrain_Click(object sender, EventArgs e)
        {
            eVision[visionSetting].FindPattern.SetROIEnd();
            eVision[visionSetting].FindPattern.TrainPattern(EPatternType.ConsistentEdges);
            eVision[visionSetting].FindPattern.ShowTrainImage(picVisionPattern);

            float minScore = Convert.ToSingle(nudPatternFindScore.Value);
            float angleTolerance = Convert.ToSingle(nudPatternAngleTolerance.Value);
            float scaleTolerance = Convert.ToSingle(nudPatternScaleTolerance.Value);
            if (eVision[visionSetting].FindPattern.FindPattern(1, minScore, EFindContrastMode.Normal, angleTolerance, scaleTolerance))
            {
                eVision[visionSetting].Fixture.SetOrigin(
                                    -eVision[visionSetting].FindPattern.Results_CenterX,
                                    -eVision[visionSetting].FindPattern.Results_CenterY,
                                    eVision[visionSetting].FindPattern.Results_Angle);
                eVision[visionSetting].FindPattern.GetPattern();
                eVision[visionSetting].FindPattern.ShowResult();

                EPoint resultsPoint = eVision[visionSetting].Calibration.Base.SensorToWorld(
                    new EPoint(eVision[visionSetting].FindPattern.Results_CenterX,
                               eVision[visionSetting].FindPattern.Results_CenterY));

                labAlignResult_X.Text = resultsPoint.X.ToString("0.000");
                labAlignResult_Y.Text = resultsPoint.Y.ToString("0.000");
                labAlignResult_θ.Text = eVision[visionSetting].FindPattern.Results_Angle.ToString("0.000");
            }
        }

        private void btnCornerLoadROI_Click(object sender, EventArgs e)
        {
            eVision[visionSetting].FindCorner[cornerLine].ShowImage();
            eVision[visionSetting].FindCorner[cornerLine].SetROI();
            eVision[visionSetting].FindCorner[cornerLine].SetROIStart();
        }

        private void btnCornerSetROI_Click(object sender, EventArgs e)
        {
            eVision[visionSetting].FindCorner[cornerLine].ShowImage();
            eVision[visionSetting].FindCorner[cornerLine].SetROIStart();
        }

        private void btnCornerTrain_Click(object sender, EventArgs e)
        {
            eVision[visionSetting].FindCorner[cornerLine].SetROIEnd();

            eVision[visionSetting].FindCorner[cornerLine].FindLine();
            eVision[visionSetting].FindCorner[cornerLine].GetSample();
            eVision[visionSetting].FindCorner[cornerLine].ShowResult();
        }

        private void Line_TrainSetting_ValueChanged(object sender, EventArgs e)
        {
            ETransitionType transitionType = (ETransitionType)Enum.Parse(typeof(ETransitionType), cboLineTransitionType.Text, true);
            int thickness = Convert.ToInt32(nudLineThickness.Value);
            float samplingStep = Convert.ToSingle(nudLineSamplingStep.Value);
            int threshold = Convert.ToInt32(nudLineThreshold.Value);

            int minAmplitude = eVision[visionSetting].FindCorner[cornerLine].Measurement_MinAmplitude;
            int minArea = eVision[visionSetting].FindCorner[cornerLine].Measurement_MinArea;

            eVision[visionSetting].FindCorner[cornerLine].TrainLine(transitionType, thickness, threshold, minAmplitude, minArea, samplingStep);
            eVision[visionSetting].FindCorner[cornerLine].FindLine();
            eVision[visionSetting].FindCorner[cornerLine].GetSample();
            eVision[visionSetting].FindCorner[cornerLine].ShowROI();
        }

        private void btnVisionSave_Click(object sender, EventArgs e)
        {
            eVision[VisionSetting.Camera_1].Save();
            eVision[VisionSetting.Camera_2].Save();
        }

        private void btnVisionLoad_Click(object sender, EventArgs e)
        {
            eVision[VisionSetting.Camera_1].Load();
            eVision[VisionSetting.Camera_2].Load();
            VisionParameterUpData();
        }

        private void btnCalibr_Click(object sender, EventArgs e)
        {
            AutoCalibr_ChkRet(visionSetting, 0, 0, 0, 0, 0, 0, false);
            prCalibrFlag = true;
        }

        private void btnCalibrStop_Click(object sender, EventArgs e)
        {
            prCalibrFlag = false;
            prRobotAlign = false;
        }

        private void btnCalibrCameraPosition_Click(object sender, EventArgs e)
        {
            AutoCalibrRotate_ChkRet(alignPose.X, alignPose.Y, 140, alignPose.θ, 0, 0, 0, false);
            prCalibrCameraPosition = true;
            //AutoCalibrRotate_ChkRet((VisionSetting)visionSetting, 0, 0, 0, 0, 0, 0, false);
            //prCalibrSingleCameraPosition = true;
        }

        private void btnCalibrCameraPositionStop_Click(object sender, EventArgs e)
        {
            prCalibrCameraPosition = false;
            prCalibrSingleCameraPosition = false;
            prRecordingGesture = false;
            prGetNowPosition = false;
            prMoveTargetPosition = false;
            prRobotAlign = false;
        }
        
        
        private XYθ alignPose = new XYθ(-525.932, -1091.929, 92.5);
        //private XYθ alignPose = new XYθ(-522.736, -1079.791, 92.5);
        private void btnRecordingGesture_Click(object sender, EventArgs e)
        {
            RecordingGesture_ChkRet(alignPose.X, alignPose.Y, 140 + Z_Up, alignPose.θ, false);
            prRecordingGesture = true;
        }

        private void btnAlignmentPosition_Click(object sender, EventArgs e)
        {
            GetNowPosition_ChkRet(false);
            prGetNowPosition = true;
        }

        private void btnAlignmentTest_Click(object sender, EventArgs e)
        {
            MoveTargetPosition_ChkRet(false);
            prMoveTargetPosition = true;
        }

        XYθ robotAlignPosition = new XYθ(383.000, -1077.000, 92.500);
        private void btnRobotAlign_Click(object sender, EventArgs e)
        {
            RobotAlign_ChkRet(robotAlignPosition.X, robotAlignPosition.Y, robotAlignPosition.θ, false);
            prRobotAlign = true;
        }

        private void TmrProcess_Tick(object sender, EventArgs e)
        {
            if (prCalibrFlag)
            {
                if (AutoCalibr_ChkRet(visionSetting, alignPose.X, alignPose.Y, 140, alignPose.θ, Convert.ToInt32(nudCalibrColumnStep.Value), Convert.ToInt32(nudCalibrRowStep.Value)) == conProg.process.Success)
                {
                    AutoCalibr_ChkRet(visionSetting, 0, 0, 0, 0, 0, 0, false);
                    prCalibrFlag = false;
                }
            }
            if (prCalibrCameraPosition)
            {
                if (AutoCalibrRotate_ChkRet(alignPose.X, alignPose.Y, 140, alignPose.θ,
                    Convert.ToDouble(nudCalibrWidth.Value),
                    Convert.ToDouble(nudCalibrHeight.Value),
                    Convert.ToDouble(nudCalibrRotationAngle.Value)) == conProg.process.Success)
                {
                    AutoCalibrRotate_ChkRet(alignPose.X, alignPose.Y, 140, alignPose.θ, 0, 0, 0, false);
                    prCalibrCameraPosition = false;
                }
            }
            if (prCalibrSingleCameraPosition)
            {
                if (AutoCalibrRotate_ChkRet(visionSetting, alignPose.X, alignPose.Y, 140, alignPose.θ, 1, Convert.ToDouble(nudCalibrRotationAngle.Value)) == conProg.process.Success)
                {
                    AutoCalibrRotate_ChkRet(visionSetting, alignPose.X, alignPose.Y, 140, alignPose.θ, 1, Convert.ToDouble(nudCalibrRotationAngle.Value), false);
                    prCalibrSingleCameraPosition = false;
                }
            }
            if (prRecordingGesture)
            {
                if (RecordingGesture_ChkRet(alignPose.X, alignPose.Y, 140 + Z_Up, alignPose.θ) == conProg.process.Success)
                {
                    RecordingGesture_ChkRet(alignPose.X, alignPose.Y, 140 + Z_Up, alignPose.θ, false);
                    prRecordingGesture = false;
                }
            }
            if (prGetNowPosition)
            {
                if (GetNowPosition_ChkRet() == conProg.process.Success)
                {
                    GetNowPosition_ChkRet(false);
                    prGetNowPosition = false;
                }
            }
            if (prMoveTargetPosition)
            {
                if (MoveTargetPosition_ChkRet() == conProg.process.Success)
                {
                    MoveTargetPosition_ChkRet(false);
                    prMoveTargetPosition = false;
                }
            }
            if (prRobotAlign)
            {
                if (RobotAlign_ChkRet(robotAlignPosition.X, robotAlignPosition.Y, robotAlignPosition.θ) == conProg.process.Success)
                {
                    RobotAlign_ChkRet(robotAlignPosition.X, robotAlignPosition.Y, robotAlignPosition.θ, false);
                    prRobotAlign = false;
                }
            }
        }

        int GoToAbsPosition_AutoUp_ChkRet_p = 0;
        private object Lock_GoToAbsPosition_AutoUp_ChkRet = new object();
        public ushort GoToAbsPosition_AutoUp_ChkRet(double nowX, double nowY, double nowZ, double nowθ, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_GoToAbsPosition_AutoUp_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    GoToAbsPosition_AutoUp_ChkRet_p = 9999;

                switch (GoToAbsPosition_AutoUp_ChkRet_p)
                {
                    case 0://初始化
                        GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ, false);
                        GoToAbsPosition(nowX, nowY, nowZ, nowθ, false);
                        GoToAbsPosition_AutoUp_ChkRet_p = 10;
                        break;
                    case 10://取得目前位置，判斷是否上升平移
                        if (GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ) == conProg.process.Success)
                        {
                            GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ, false);
                            if (tableNow_X == nowX && tableNow_Y == nowY && tableNow_Z == nowZ && tableNow_θ == nowθ)
                                GoToAbsPosition_AutoUp_ChkRet_p = 40;
                            else
                                GoToAbsPosition_AutoUp_ChkRet_p = 20;
                        }
                        break;
                    case 20://上升
                        if (GoToAbsPosition(tableNow_X, tableNow_Y, nowZ + Z_Up, tableNow_θ) == conProg.process.Success)
                        {
                            GoToAbsPosition(tableNow_X, tableNow_Y, nowZ + Z_Up, tableNow_θ, false);
                            GoToAbsPosition_AutoUp_ChkRet_p = 30;
                        }
                        break;
                    case 30://平移
                        if (GoToAbsPosition(nowX, nowY, nowZ + Z_Up, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition(nowX, nowY, nowZ + Z_Up, nowθ, false);
                            GoToAbsPosition_AutoUp_ChkRet_p = 40;
                        }
                        break;
                    case 40://下降
                        if (GoToAbsPosition(nowX, nowY, nowZ, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition(nowX, nowY, nowZ, nowθ, false);
                            GoToAbsPosition_AutoUp_ChkRet_p = 9999;
                        }
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            GoToAbsPosition_AutoUp_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            GoToAbsPosition_AutoUp_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            GoToAbsPosition_AutoUp_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    GoToAbsPosition_AutoUp_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }

        #region 相機座標系校正(9點校正)  
        DelayTime dtmrAutoCalibrPoint = new DelayTime();
        Point_f[] AutoCalibrPoint;
        List<CalibrationPoint> AutoCalibrTable = new List<CalibrationPoint>();
        int RunAutoCalibrPointIndex = 0;
        double moveX = 0;
        double moveY = 0;
        int AutoCalibr_ChkRet_p = 0;
        private object Lock_AutoCalibr_ChkRet = new object();
        public ushort AutoCalibr_ChkRet(VisionSetting visionSetting, double nowX, double nowY, double nowZ, double nowθ, int widthCount, int heightCount, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_AutoCalibr_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    AutoCalibr_ChkRet_p = 9999;

                switch (AutoCalibr_ChkRet_p)
                {
                    case 0://初始化
                        AffineTransformation_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ, false);
                        CalibrSinglePoint_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ, ref point, false);
                        GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);
                        RunAutoCalibrPointIndex = 0;
                        AutoCalibrTable.Clear();
                        AutoCalibr_ChkRet_p = 2;
                        break;
                    case 2:
                        if (GetTablePosition(ref tableNow_X,ref tableNow_Y,ref tableNow_Z,ref tableNow_θ) == conProg.process.Success)
                        {
                            GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ, false);
                            AutoCalibr_ChkRet_p = 3;
                        }
                        break;
                    case 3:
                        if (GoToAbsPosition(tableNow_X, tableNow_Y, 300, tableNow_θ) == conProg.process.Success)
                        {
                            GoToAbsPosition(tableNow_X, tableNow_Y, 300, tableNow_θ, false);
                            AutoCalibr_ChkRet_p = 4;
                        }
                        break;
                    case 4:
                        if (GoToAbsPosition(nowX, nowY, 300, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition(nowX, nowY, 300, nowθ, false);
                            AutoCalibr_ChkRet_p = 5;
                        }
                        break;
                    case 5:
                        if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);
                            DO(2, false);
                            DO(1, true);
                            dtmrAutoCalibrPoint.Start(2000);
                            AutoCalibr_ChkRet_p = 6;
                        }
                        break;
                    case 6:
                        if (dtmrAutoCalibrPoint.IsTimeOut)
                            AutoCalibr_ChkRet_p = 10;
                        break;
                    case 10://求映射係數，自動尋找教導位置
                        if (AffineTransformation_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ) == conProg.process.Success)
                        {
                            AffineTransformation_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ, false);
                            GetCalibrPoint(
                                ref AutoCalibrPoint,
                                eVision[visionSetting].FindPattern.InputImage.Width,
                                eVision[visionSetting].FindPattern.InputImage.Height,
                                eVision[visionSetting].FindPattern.TrainImage.Width,
                                eVision[visionSetting].FindPattern.TrainImage.Height,
                                widthCount,
                                heightCount);
                            AutoCalibr_ChkRet_p = 20;
                        }
                        break;
                    case 20://代入映射係數，求移動位置
                        moveX =
                            affineParameter.a * AutoCalibrPoint[RunAutoCalibrPointIndex].X +
                            affineParameter.b * AutoCalibrPoint[RunAutoCalibrPointIndex].Y +
                            affineParameter.c;
                        moveY =
                            affineParameter.d * AutoCalibrPoint[RunAutoCalibrPointIndex].X +
                            affineParameter.e * AutoCalibrPoint[RunAutoCalibrPointIndex].Y +
                            affineParameter.f;
                        if (RunAutoCalibrPointIndex == 0 || RunAutoCalibrPointIndex % widthCount == 0)
                            AutoCalibr_ChkRet_p = 25;
                        else
                            AutoCalibr_ChkRet_p = 30;
                        break;
                    case 25:
                        double x =
                            affineParameter.a * (AutoCalibrPoint[RunAutoCalibrPointIndex].X - 10) +
                            affineParameter.b * (AutoCalibrPoint[RunAutoCalibrPointIndex].Y - 10) +
                            affineParameter.c;
                        double y =
                            affineParameter.d * (AutoCalibrPoint[RunAutoCalibrPointIndex].X - 10) +
                            affineParameter.e * (AutoCalibrPoint[RunAutoCalibrPointIndex].Y - 10) +
                            affineParameter.f;
                        if (GoToAbsPosition_AutoUp_ChkRet(x, y, nowZ, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(x, y, nowZ, nowθ, false);
                            AutoCalibr_ChkRet_p = 30;
                        }
                        break;
                    case 30://移動、拍照、FindPattern
                        if (CalibrSinglePoint_ChkRet(visionSetting, moveX, moveY, nowZ, nowθ, ref point) == conProg.process.Success)
                        {
                            CalibrSinglePoint_ChkRet(visionSetting, moveX, moveY, nowZ, nowθ, ref point, false);
                            AutoCalibrTable.Add(new CalibrationPoint(point.X, point.Y, moveX, moveY));
                            RunAutoCalibrPointIndex++;
                            if (RunAutoCalibrPointIndex >= AutoCalibrPoint.Length)
                                AutoCalibr_ChkRet_p = 40;
                            else
                                AutoCalibr_ChkRet_p = 20;
                        }
                        break;
                    case 40:
                        if (CalibrSinglePoint_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ, ref point) == conProg.process.Success)
                        {
                            CalibrSinglePoint_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ, ref point, false);
                            eVision[visionSetting].Calibration.SetCalibrPoint(AutoCalibrTable.ToArray());
                            eVision[visionSetting].Calibration.Calibration();
                            eVision[visionSetting].Calibration.ShowImage(true);
                            eVision[visionSetting].Calibration.ShowResult(true);

                            //DO(1, false);
                            //DO(2, true);
                            dtmrAutoCalibrPoint.Start(50);
                            AutoCalibr_ChkRet_p = 50;
                        }
                        break;
                    case 50:
                        if (dtmrAutoCalibrPoint.IsTimeOut)
                        {
                            //DO(2, false);
                            if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ + Z_Up, nowθ) == conProg.process.Success)
                            {
                                GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ + Z_Up, nowθ, false);
                                AutoCalibr_ChkRet_p = 9999;
                            }
                        } 
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            AutoCalibr_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            AutoCalibr_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            AutoCalibr_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    AutoCalibr_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }

        List<EPoint> AutoCalibrPointList = new List<EPoint>();
        EPoint point = new EPoint();
        int targetPixel = 100;
        double pixeScale = 1;
        AffineParameter affineParameter = new AffineParameter();
        int AffineTransformation_ChkRet_p = 0;
        private object Lock_AffineTransformation_ChkRet = new object();
        public ushort AffineTransformation_ChkRet(VisionSetting visionSetting, double nowX, double nowY,double nowZ, double nowθ, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_AffineTransformation_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    AffineTransformation_ChkRet_p = 9999;

                double moveDistanceTarget = 2;
                double moveDistance = moveDistanceTarget * pixeScale;
                switch (AffineTransformation_ChkRet_p)
                {
                    case 0://初始化
                        AutoCalibrPointList.Clear();
                        CalibrSinglePoint_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ, ref point, false);
                        pixeScale = 1;
                        AffineTransformation_ChkRet_p = 10;
                        break;
                    case 10://旋轉、比例校正，原點                        
                        if (CalibrSinglePoint_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ, ref point) == conProg.process.Success)
                        {
                            CalibrSinglePoint_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ, ref point, false);
                            AutoCalibrPointList.Add(point);
                            AffineTransformation_ChkRet_p = 20;
                        }
                        break;
                    case 20://旋轉、比例校正，X方向移動
                        if (CalibrSinglePoint_ChkRet(visionSetting, nowX + moveDistance, nowY, nowZ, nowθ, ref point) == conProg.process.Success)
                        {
                            CalibrSinglePoint_ChkRet(visionSetting, nowX + moveDistance, nowY, nowZ, nowθ, ref point, false);
                            AutoCalibrPointList.Add(point);
                            AffineTransformation_ChkRet_p = 30;
                        }
                        break;
                    case 30://判斷Pixel移動量是否充足
                        EPoint subPoint = AutoCalibrPointList[1] - AutoCalibrPointList[0];
                        double pixelLength = Math.Sqrt((subPoint.X * subPoint.X) + (subPoint.Y * subPoint.Y));
                        pixeScale = targetPixel / pixelLength;
                        if (pixeScale > 1)
                            AffineTransformation_ChkRet_p = 40;
                        else
                            AffineTransformation_ChkRet_p = 50;
                        break;
                    case 40://移動量不足，調整比例；旋轉、比例校正，X方向移動
                        if (CalibrSinglePoint_ChkRet(visionSetting, nowX + moveDistance, nowY, nowZ, nowθ, ref point) == conProg.process.Success)
                        {
                            CalibrSinglePoint_ChkRet(visionSetting, nowX + moveDistance, nowY, nowZ, nowθ, ref point, false);
                            AutoCalibrPointList.RemoveAt(AutoCalibrPointList.Count - 1);
                            AutoCalibrPointList.Add(point);
                            AffineTransformation_ChkRet_p = 50;
                        }
                        break;
                    case 50://旋轉、比例校正，Y方向移動
                        if (CalibrSinglePoint_ChkRet(visionSetting, nowX, nowY + moveDistance, nowZ, nowθ, ref point) == conProg.process.Success)
                        {
                            CalibrSinglePoint_ChkRet(visionSetting, nowX, nowY + moveDistance, nowZ, nowθ, ref point, false);
                            AutoCalibrPointList.Add(point);
                            AffineTransformation_ChkRet_p = 60;
                        }
                        break;
                    case 60:
                        AffineTransformation(
                            ref affineParameter,
                            new TransformationCoordinate(AutoCalibrPointList[0].X, AutoCalibrPointList[0].Y, nowX, nowY),
                            new TransformationCoordinate(AutoCalibrPointList[1].X, AutoCalibrPointList[1].Y, nowX + moveDistance, nowY),
                            new TransformationCoordinate(AutoCalibrPointList[2].X, AutoCalibrPointList[2].Y, nowX, nowY + moveDistance));

                        AffineTransformation_ChkRet_p = 9999;
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            AffineTransformation_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            AffineTransformation_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            AffineTransformation_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    AffineTransformation_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }

        int CalibrSinglePoint_ChkRet_p = 0;
        private object Lock_CalibrSinglePoint_ChkRet = new object();
        public ushort CalibrSinglePoint_ChkRet(VisionSetting visionSetting, double nowX, double nowY, double nowZ, double nowθ, ref EPoint patternCenter, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_CalibrSinglePoint_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    CalibrSinglePoint_ChkRet_p = 9999;

                switch (CalibrSinglePoint_ChkRet_p)
                {
                    case 0://初始化
                        GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);
                        TriggerCamera_ChkRet(visionSetting, false);
                        CalibrSinglePoint_ChkRet_p = 10;
                        break;
                    case 10://移動
                        if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);
                            CalibrSinglePoint_ChkRet_p = 20;
                        }
                        break;
                    case 20://拍照     
                        if (TriggerCamera_ChkRet(visionSetting) == conProg.process.Success)
                        {
                            TriggerCamera_ChkRet(visionSetting, false);
                            CalibrSinglePoint_ChkRet_p = 30;
                        }
                        break;
                    case 30://FindPattern
                        if (eVision[visionSetting].FindPattern.FindPattern())
                        {
                            //eVision[visionSetting].Fixture.SetOrigin(
                            //    -eVision[visionSetting].FindPattern.Results_CenterX,
                            //    -eVision[visionSetting].FindPattern.Results_CenterY,
                            //    eVision[visionSetting].FindPattern.Results_Angle);
                            //ezPoint corner = new ezPoint();
                            //eVision[visionSetting].GetCornerResults(ref corner);
                            //eVision[visionSetting].FindCorner[CornerLine.Line_1].ShowImage(true);
                            //switch (visionSetting)
                            //{
                            //    case VisionSetting.Camera_1:
                            //        eVision[visionSetting].ShowCornerResult(picDisplay_1);
                            //        break;
                            //    case VisionSetting.Camera_2:
                            //        eVision[visionSetting].ShowCornerResult(picDisplay_2);
                            //        break;
                            //}
                            //patternCenter = new EPoint((float)corner.x, (float)corner.y);

                            eVision[visionSetting].FindPattern.ShowImage(true);
                            eVision[visionSetting].FindPattern.ShowResult(true);
                            patternCenter = eVision[visionSetting].FindPattern.Results_Point;
                            CalibrSinglePoint_ChkRet_p = 9999;
                        }
                        else
                        {
                            strError = "CalibrSinglePoint_找不到特徵";
                            CalibrSinglePoint_ChkRet_p = 8000;
                        }
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            CalibrSinglePoint_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            CalibrSinglePoint_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            CalibrSinglePoint_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    CalibrSinglePoint_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }

        DelayTime dtmrTrigger = new DelayTime();
        int TriggerCamera_ChkRet_p = 0;
        private object Lock_TriggerCamera_ChkRet = new object();
        public ushort TriggerCamera_ChkRet(VisionSetting visionSetting, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_TriggerCamera_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    TriggerCamera_ChkRet_p = 9999;

                switch (TriggerCamera_ChkRet_p)
                {
                    case 0://初始化
                        imageUpDataFlag[visionSetting] = false;
                        dtmrTrigger.Start(timeOut);
                        TriggerCamera_ChkRet_p = 10;
                        break;
                    case 10://拍照
                        if (dtmrTriggerBoth.IsTimeOut)
                        {
                            strError += visionSetting.ToString();
                            strError += " Trigger Time out";
                            TriggerCamera_ChkRet_p = 8000;
                        }
                        if (baslerCamera[(int)visionSetting].OneShot())
                        {
                            strError = string.Empty;
                            dtmrTriggerBoth.Start(timeOut);
                            TriggerCamera_ChkRet_p = 20;
                        }
                        break;
                    case 20://拍照是否成功
                        if (dtmrTrigger.IsTimeOut)
                        {
                            strError = "Get Image Time out";
                            TriggerCamera_ChkRet_p = 8000;
                        }
                        if (imageUpDataFlag[visionSetting])
                        {
                            eVision[visionSetting].Calibration.InputImage.Save("C:\\Users\\Experiment\\Desktop\\Image_" + RunAutoCalibrPointIndex + ".bmp");
                            strError = string.Empty;
                            TriggerCamera_ChkRet_p = 9999;
                        }
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            TriggerCamera_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            TriggerCamera_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            TriggerCamera_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    TriggerCamera_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }

        public static double GetMatrixValue_3_3(double[,] matrix)
        {
            List<double> equation = new List<double>();
            equation.Add(matrix[0, 0] * matrix[1, 1] * matrix[2, 2]);
            equation.Add(matrix[1, 0] * matrix[2, 1] * matrix[0, 2]);
            equation.Add(matrix[2, 0] * matrix[0, 1] * matrix[1, 2]);
            equation.Add(-(matrix[0, 0] * matrix[2, 1] * matrix[1, 2]));
            equation.Add(-(matrix[1, 0] * matrix[0, 1] * matrix[2, 2]));
            equation.Add(-(matrix[2, 0] * matrix[1, 1] * matrix[0, 2]));
            return equation.Sum();
        }
        /// <summary>
        /// Cramer's rule公式，求三元一次方程係數
        /// aX + bY + cZ = X'
        /// </summary>
        /// <param name="matrixData">aX + bY + cZ = X'</param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public static void Cramers_Rule(ref double a, ref double b, ref double c, double[,] matrixData)
        {
            List<double> equation = new List<double>();
            double[,] matrix_0 = new double[matrixData.GetLength(0), matrixData.GetLength(1)];
            double[,] matrix_1 = new double[matrixData.GetLength(0), matrixData.GetLength(1)];
            double[,] matrix_2 = new double[matrixData.GetLength(0), matrixData.GetLength(1)];
            double[,] matrix_3 = new double[matrixData.GetLength(0), matrixData.GetLength(1)];
            for (int i = 0; i < matrixData.GetLength(0); i++)
            {
                for (int j = 0; j < matrixData.GetLength(1); j++)
                {
                    matrix_0[i, j] = Convert.ToDouble(matrixData.GetValue(i, j));
                    matrix_1[i, j] = Convert.ToDouble(matrixData.GetValue(i, j));
                    matrix_2[i, j] = Convert.ToDouble(matrixData.GetValue(i, j));
                    matrix_3[i, j] = Convert.ToDouble(matrixData.GetValue(i, j));
                }
            }
            equation.Add(GetMatrixValue_3_3(matrix_0));

            matrix_1[0, 0] = Convert.ToDouble(matrixData.GetValue(3, 0));
            matrix_1[0, 1] = Convert.ToDouble(matrixData.GetValue(3, 1));
            matrix_1[0, 2] = Convert.ToDouble(matrixData.GetValue(3, 2));
            equation.Add(GetMatrixValue_3_3(matrix_1));
            a = equation[1] / equation[0];

            matrix_2[1, 0] = Convert.ToDouble(matrixData.GetValue(3, 0));
            matrix_2[1, 1] = Convert.ToDouble(matrixData.GetValue(3, 1));
            matrix_2[1, 2] = Convert.ToDouble(matrixData.GetValue(3, 2));
            equation.Add(GetMatrixValue_3_3(matrix_2));
            b = equation[2] / equation[0];

            matrix_3[2, 0] = Convert.ToDouble(matrixData.GetValue(3, 0));
            matrix_3[2, 1] = Convert.ToDouble(matrixData.GetValue(3, 1));
            matrix_3[2, 2] = Convert.ToDouble(matrixData.GetValue(3, 2));
            equation.Add(GetMatrixValue_3_3(matrix_3));
            c = equation[3] / equation[0];
        }

        public static bool AffineTransformation(ref AffineParameter affineParameter, params TransformationCoordinate[] coordinate)
        {
            bool reSuccess = false;
            if (coordinate.Length >= 3)
            {
                double[] _affineParameter = new double[6];
                double[,] matrixData = new double[4, 3];

                for (int i = 0; i < 3; i++)
                {
                    matrixData[0, i] = coordinate[i].Before.X;
                    matrixData[1, i] = coordinate[i].Before.Y;
                    matrixData[2, i] = 1;
                    matrixData[3, i] = coordinate[i].After.X;
                }
                Cramers_Rule(ref _affineParameter[0], ref _affineParameter[1], ref _affineParameter[2], matrixData);

                for (int i = 0; i < 3; i++)
                {
                    matrixData[0, i] = coordinate[i].Before.X;
                    matrixData[1, i] = coordinate[i].Before.Y;
                    matrixData[2, i] = 1;
                    matrixData[3, i] = coordinate[i].After.Y;
                }
                Cramers_Rule(ref _affineParameter[3], ref _affineParameter[4], ref _affineParameter[5], matrixData);

                affineParameter.a = _affineParameter[0];
                affineParameter.b = _affineParameter[1];
                affineParameter.c = _affineParameter[2];
                affineParameter.d = _affineParameter[3];
                affineParameter.e = _affineParameter[4];
                affineParameter.f = _affineParameter[5];

                reSuccess = true;
            }
            return reSuccess;
        }

        public static void GetCalibrPoint(ref Point_f[] calibrPoint, int imageWidth, int imageHeight, int patternWidth, int patternHeight, int widthCount, int heightCount, int retractionPixel = 50)
        {
            List<double> equation = new List<double>();
            List<Point_f> _calibrPoint = new List<Point_f>();
            equation.Add((patternWidth / 2) + retractionPixel);//[0]，CalibrPoint，第1個X位置
            equation.Add((patternHeight / 2) + retractionPixel);//[1]，CalibrPoint，第1個Y位置
            equation.Add(imageWidth - (patternWidth / 2) - retractionPixel);//[2]，CalibrPoint，最後X位置
            equation.Add(imageHeight - (patternHeight / 2) - retractionPixel);//[3]，CalibrPoint，最後Y位置
            equation.Add((equation[2] - equation[0]) / (widthCount - 1));//[4]，X間距
            equation.Add((equation[3] - equation[1]) / (heightCount - 1));//[5]，Y間距

            for (int i = 0; i < heightCount; i++)
                for (int j = 0; j < widthCount; j++)
                    _calibrPoint.Add(new Point_f(equation[0] + (equation[4] * j), equation[1] + (equation[5] * i)));
            calibrPoint = _calibrPoint.ToArray();
        }

        public static double? LawOfSines(double SideLength, double θ, double parameter, LawOfSinesResultType resultType)
        {
            double? result = null;
            if (θ > 0 && θ < 180)
            {
                double x = SideLength / Math.Sin(θ / 180 * Math.PI);
                switch (resultType)
                {
                    case LawOfSinesResultType.θ:
                        result = Math.Asin(parameter / x) / Math.PI * 180;
                        break;
                    case LawOfSinesResultType.SideLength:
                        //if (parameter > 0 && parameter < 180)
                        //    result = x * Math.Sin(parameter / 180 * Math.PI);

                        double radius = (SideLength / 2) / Math.Sin((θ / 2) * Math.PI / 180);
                        result = radius;
                        break;
                }
            }
            return result;
        }
        #endregion

        #region 相機位置校正(旋轉中心，無需任何參數)
        DelayTime dtmrAutoCalibrRotate = new DelayTime();
        List<EPoint> rotatePoint = new List<EPoint>();
        XYθ[] calibrRotatePoint;
        XYθ[] calibrRotateMovePoint;
        DataTable dtCalibrRotate = new DataTable("CalibrRotate");
        double calibrRotateRadius = 0;
        double calibrRotateθ = 0;
        int calibrRotateIndex = 0;
        int calibrRepeatabilityIndex = 0;
        int AutoCalibrRotate_ChkRet_p = 0;
        public ushort AutoCalibrRotate_ChkRet(VisionSetting visionSetting, double nowX, double nowY, double nowZ, double nowθ, int rotateCount, double rotateRange, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_AutoCalibrRotate_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    AutoCalibrRotate_ChkRet_p = 9999;

                float pixelX = Convert.ToSingle(eVision[visionSetting].Calibration.CalibrTable.Rows[(eVision[visionSetting].Calibration.CalibrTable.Rows.Count - 1) / 2]["Pixel_X"]);
                float pixelY = Convert.ToSingle(eVision[visionSetting].Calibration.CalibrTable.Rows[(eVision[visionSetting].Calibration.CalibrTable.Rows.Count - 1) / 2]["Pixel_Y"]);
                EPoint calibrCenter = eVision[visionSetting].Calibration.Fixture.GetPoint(new EPoint(pixelX, pixelY), CoordinateHook.Hook);

                switch (AutoCalibrRotate_ChkRet_p)
                {
                    case 0://初始化       
                        eVision[visionSetting].Calibration.Fixture.SetOrigin(0, 0);
                        rotatePoint.Clear();
                        cameraPosition.Clear();
                        calibrRotateIndex = 0;
                        calibrRepeatabilityIndex = 0;
                        CalibrRotateSinglePoint_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ, false);

                        dtCalibrRotate.Clear();
                        if (dtCalibrRotate.Columns.Count <= 0)
                        {
                            dtCalibrRotate.Columns.Add("Radius");
                            dtCalibrRotate.Columns.Add("θ");
                            dtCalibrRotate.Columns.Add("Deviation");
                        }

                        AutoCalibrRotate_ChkRet_p = 2;
                        break;
                    case 2:
                        if (GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ) == conProg.process.Success)
                        {
                            GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ, false);
                            AutoCalibrRotate_ChkRet_p = 3;
                        }
                        break;
                    case 3:
                        if (GoToAbsPosition(tableNow_X, tableNow_Y, 300, tableNow_θ) == conProg.process.Success)
                        {
                            GoToAbsPosition(tableNow_X, tableNow_Y, 300, tableNow_θ, false);
                            AutoCalibrRotate_ChkRet_p = 4;
                        }
                        break;
                    case 4:
                        if (GoToAbsPosition(nowX, nowY, 300, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition(nowX, nowY, 300, nowθ, false);
                            AutoCalibrRotate_ChkRet_p = 5;
                        }
                        break;
                    case 5:
                        if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);
                            DO(2, false);
                            DO(1, true);
                            dtmrAutoCalibrRotate.Start(2000);
                            AutoCalibrRotate_ChkRet_p = 6;
                        }
                        break;
                    case 6:
                        if (dtmrAutoCalibrRotate.IsTimeOut)
                            AutoCalibrRotate_ChkRet_p = 10;
                        break;
                    case 10://移動、拍照、回傳交點(原點)
                        if (CalibrRotateSinglePoint_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ) == conProg.process.Success)
                        {
                            rotatePoint.Add(cornerSingle);
                            CalibrRotateSinglePoint_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ, false);
                            AutoCalibrRotate_ChkRet_p = 20;
                        }
                        break;
                    case 20://移動、拍照、回傳交點(小角度)
                        if (CalibrRotateSinglePoint_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ + (rotateRange)) == conProg.process.Success)
                        {
                            rotatePoint.Add(cornerSingle);
                            CalibrRotateSinglePoint_ChkRet(visionSetting, nowX, nowY, nowZ, nowθ + (rotateRange), false);
                            AutoCalibrRotate_ChkRet_p = 30;
                        }
                        break;
                    case 30:
                        Vector vector = new Vector(rotatePoint[0], rotatePoint[1]);
                        double θ = vector.DirectionAngle - (rotateRange / 2);
                        double? radius = LawOfSines(vector.Length, rotateRange, ((double)180 - rotateRange) / 2, LawOfSinesResultType.SideLength);
                        GetCalibrRotatePoint(ref calibrRotatePoint, ref calibrRotateMovePoint, calibrCenter, radius.Value, θ, rotateRange * 2);

                        AutoCalibrRotate_ChkRet_p = 35;
                        break;
                    case 35:
                        XYθ xyθ1 = calibrRotateMovePoint[1];
                        if (CalibrRotateSinglePoint_ChkRet(visionSetting, xyθ1.X, xyθ1.Y, nowZ, nowθ + xyθ1.θ) == conProg.process.Success)
                        {
                            CalibrRotateSinglePoint_ChkRet(visionSetting, xyθ1.X, xyθ1.Y, nowZ, nowθ + xyθ1.θ, false);
                            AutoCalibrRotate_ChkRet_p = 40;
                        }
                        break;
                    case 40:
                        XYθ xyθ = calibrRotateMovePoint[calibrRotateIndex];
                        if (CalibrRotateSinglePoint_ChkRet(visionSetting, xyθ.X, xyθ.Y, nowZ, nowθ + xyθ.θ) == conProg.process.Success)
                        {
                            calibrRotateIndex++;
                            rotatePoint.Add(cornerSingle);
                            CalibrRotateSinglePoint_ChkRet(visionSetting, xyθ.X, xyθ.Y, nowZ, nowθ + xyθ.θ, false);
                            if (calibrRotateIndex >= calibrRotateMovePoint.Length)
                                AutoCalibrRotate_ChkRet_p = 50;
                            else
                                AutoCalibrRotate_ChkRet_p = 40;
                        }
                        break;
                    case 50:
                        int rpi = calibrRepeatabilityIndex * 3;

                        EPoint point_1_1 = new EPoint(
                            Convert.ToSingle(rotatePoint[rpi + 2].X - rotatePoint[rpi + 3].X),
                            Convert.ToSingle(rotatePoint[rpi + 2].Y - rotatePoint[rpi + 3].Y));
                        EPoint point_2_1 = new EPoint(
                            Convert.ToSingle(rotatePoint[rpi + 4].X - rotatePoint[rpi + 3].X),
                            Convert.ToSingle(rotatePoint[rpi + 4].Y - rotatePoint[rpi + 3].Y));

                        EPoint point_1 = new EPoint(
                            Convert.ToSingle(calibrRotatePoint[0].X + point_1_1.X),
                            Convert.ToSingle(calibrRotatePoint[0].Y + point_1_1.Y));
                        EPoint point_2 = new EPoint(
                            Convert.ToSingle(calibrRotatePoint[2].X + point_2_1.X),
                            Convert.ToSingle(calibrRotatePoint[2].Y + point_2_1.Y));

                        Vector vector_2 = new Vector(point_1, point_2);
                        double? radius_2 = LawOfSines(vector_2.Length, rotateRange * 2, ((double)180 - (rotateRange * 2)) / 2, LawOfSinesResultType.SideLength);
                        GetCalibrRotatePoint(ref calibrRotatePoint, ref calibrRotateMovePoint, calibrCenter, radius_2.Value, vector_2.DirectionAngle, rotateRange * 2);

                        double deviation = Math.Sqrt(Math.Pow(rotatePoint[rpi + 4].X - rotatePoint[rpi + 2].X, 2) + Math.Pow(rotatePoint[rpi + 4].Y - rotatePoint[rpi + 2].Y, 2));
                        //double deviation = Math.Sqrt(Math.Pow(point_2.X - point_1.X, 2) + Math.Pow(point_2.Y - point_1.Y, 2));
                        dtCalibrRotate.Rows.Add(radius_2.Value, vector_2.DirectionAngle, deviation);

                        calibrRepeatabilityIndex++;
                        if (calibrRepeatabilityIndex >= rotateCount)
                        {
                            dtCalibrRotate.DefaultView.Sort = "Deviation ASC";
                            calibrRotateRadius = Convert.ToDouble(dtCalibrRotate.DefaultView[0]["Radius"]);
                            calibrRotateθ = Convert.ToDouble(dtCalibrRotate.DefaultView[0]["θ"]);
                            AutoCalibrRotate_ChkRet_p = 60;
                        }
                        else
                        {
                            calibrRotateIndex = 0;
                            AutoCalibrRotate_ChkRet_p = 35;
                        }

                        break;
                    case 60:
                        if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);

                            double camera_X = calibrRotateRadius * Math.Cos(calibrRotateθ / 180 * Math.PI);
                            double camera_Y = calibrRotateRadius * Math.Sin(calibrRotateθ / 180 * Math.PI);

                            eVision[visionSetting].Calibration.Fixture.SetOrigin(
                                        Convert.ToSingle(calibrCenter.X - alignPose.X + camera_X),
                                        Convert.ToSingle(calibrCenter.Y - alignPose.Y + camera_Y));

                            switch (visionSetting)
                            {
                                case VisionSetting.Camera_1:
                                    nudCamera_1_Position_X.Value = Convert.ToDecimal(-eVision[visionSetting].Calibration.Fixture.OriginX);
                                    nudCamera_1_Position_Y.Value = Convert.ToDecimal(-eVision[visionSetting].Calibration.Fixture.OriginY);
                                    break;
                                case VisionSetting.Camera_2:
                                    nudCamera_2_Position_X.Value = Convert.ToDecimal(-eVision[visionSetting].Calibration.Fixture.OriginX);
                                    nudCamera_2_Position_Y.Value = Convert.ToDecimal(-eVision[visionSetting].Calibration.Fixture.OriginY);
                                    break;
                            }

                            DO(1, false);
                            DO(2, true);
                            dtmrAutoCalibrRotate.Start(50);
                            AutoCalibrRotate_ChkRet_p = 70;
                        }
                        break;
                    case 70:
                        if (dtmrAutoCalibrRotate.IsTimeOut)
                        {
                            DO(2, false);
                            if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ + Z_Up, nowθ) == conProg.process.Success)
                            {
                                GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ + Z_Up, nowθ, false);
                                AutoCalibrRotate_ChkRet_p = 9999;
                            }
                        }
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            AutoCalibrRotate_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            AutoCalibrRotate_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            AutoCalibrRotate_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    AutoCalibrRotate_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }

        EPoint cornerSingle = new EPoint();
        int CalibrRotateSinglePoint_ChkRet_p = 0;
        private object Lock_CalibrRotateSinglePoint_ChkRet = new object();
        public ushort CalibrRotateSinglePoint_ChkRet(VisionSetting visionSetting, double nowX, double nowY,double nowZ, double nowθ, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_CalibrRotateSinglePoint_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    CalibrRotateSinglePoint_ChkRet_p = 9999;

                switch (CalibrRotateSinglePoint_ChkRet_p)
                {
                    case 0://初始化
                        GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);
                        TriggerCamera_ChkRet(visionSetting, false);
                        CalibrRotateSinglePoint_ChkRet_p = 10;
                        break;
                    case 10://移動
                        if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);
                            CalibrRotateSinglePoint_ChkRet_p = 20;
                        }
                        break;
                    case 20://拍照     
                        if (TriggerCamera_ChkRet(visionSetting) == conProg.process.Success)
                        {
                            TriggerCamera_ChkRet(visionSetting, false);
                            CalibrRotateSinglePoint_ChkRet_p = 40;
                        }
                        break;
                    case 30:
                        CalibrRotateSinglePoint_ChkRet_p = 40;
                        break;
                    case 40://FindCorner
                        if (eVision[visionSetting].CalibrFindPattern.FindPattern())
                        {
                            //eVision[visionSetting].Fixture.SetOrigin(
                            //    -eVision[visionSetting].FindPattern.Results_CenterX,
                            //    -eVision[visionSetting].FindPattern.Results_CenterY,
                            //    eVision[visionSetting].FindPattern.Results_Angle);
                            //ezPoint zCorner = new ezPoint();
                            //eVision[visionSetting].GetCornerWordResults(ref zCorner);
                            //cornerSingle = new EPoint(Convert.ToSingle(zCorner.x), Convert.ToSingle(zCorner.y));
                            //eVision[visionSetting].FindCorner[CornerLine.Line_1].ShowImage(true);
                            //PictureBox pic = visionSetting == VisionSetting.Camera_1 ? picDisplay_1 : picDisplay_2;
                            //eVision[visionSetting].ShowCornerResult(pic);

                            eVision[visionSetting].GetPatternWordResults(eVision[visionSetting].CalibrFindPattern, ref cornerSingle);
                            eVision[visionSetting].CalibrFindPattern.ShowResult(true);

                            CalibrRotateSinglePoint_ChkRet_p = 9999;
                        }
                        else
                        {
                            strError = visionSetting.ToString() + "找不到特徵";
                            CalibrRotateSinglePoint_ChkRet_p = 8000;
                        }
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            CalibrRotateSinglePoint_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            CalibrRotateSinglePoint_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            CalibrRotateSinglePoint_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    CalibrSinglePoint_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }

        public static void GetCalibrRotatePoint(ref XYθ[] calibrPoint, ref XYθ[] calibrMovePoint, EPoint rotateCenter, double radius, double θ, double rotateRange, double rotateCount = 3)
        {
            List<XYθ> _calibrPoint = new List<XYθ>();
            List<XYθ> _calibrPointMove = new List<XYθ>();
            double θStart = θ - (rotateRange / 2);
            double θRange = rotateRange / (rotateCount - 1);
            for (int i = 0; i < rotateCount; i++)
            {
                double _θ = θStart + (θRange * i);
                double x = radius * Math.Cos(_θ / 180 * Math.PI);
                double y = radius * Math.Sin(_θ / 180 * Math.PI);
                _calibrPoint.Add(new XYθ(x, y, _θ));
            }
            calibrPoint = _calibrPoint.ToArray();

            XYθ calibrPointMoveCenter = _calibrPoint[(_calibrPoint.Count - 1) / 2];
            calibrPointMoveCenter.X += rotateCenter.X;
            calibrPointMoveCenter.Y += rotateCenter.Y;
            for (int i = 0; i < rotateCount; i++)
            {
                double x = calibrPointMoveCenter.X - _calibrPoint[i].X;
                double y = calibrPointMoveCenter.Y - _calibrPoint[i].Y;
                double _θ = _calibrPoint[i].θ - calibrPointMoveCenter.θ;
                _calibrPointMove.Add(new XYθ(x, y, _θ));
            }
            calibrMovePoint = _calibrPointMove.ToArray();
        }
        #endregion

        #region 相機位置校正(旋轉中心)

        EPoint calibrRotateBothPattern_1 = new EPoint();
        EPoint calibrRotateBothPattern_2 = new EPoint();
        List<EPoint> rotatePoint_1 = new List<EPoint>();
        List<EPoint> rotatePoint_2 = new List<EPoint>();
        List<CameraPosition> cameraPosition = new List<CameraPosition>();
        private object Lock_AutoCalibrRotate_ChkRet = new object();
        public ushort AutoCalibrRotate_ChkRet(double nowX, double nowY, double nowZ, double nowθ, double calibrWidth, double calibrHeight, double rotateRange, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_AutoCalibrRotate_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    AutoCalibrRotate_ChkRet_p = 9999;

                EPoint calibrRotatePoint_1 = new EPoint();
                EPoint calibrRotatePoint_2 = new EPoint();
                double diagonalLength = Math.Sqrt(Math.Pow(calibrWidth, 2) + Math.Pow(calibrHeight, 2));

                //float Camera_1_PixelX = Convert.ToSingle(eVision[VisionSetting.Camera_1].Calibration.CalibrTable.Rows[(eVision[VisionSetting.Camera_1].Calibration.CalibrTable.Rows.Count - 1) / 2]["Pixel_X"]);
                //float Camera_1_PixelY = Convert.ToSingle(eVision[VisionSetting.Camera_1].Calibration.CalibrTable.Rows[(eVision[VisionSetting.Camera_1].Calibration.CalibrTable.Rows.Count - 1) / 2]["Pixel_Y"]);
                float Camera_1_PixelX = 800;
                float Camera_1_PixelY = 600;
                EPoint Camera_1_CalibrCenter = eVision[VisionSetting.Camera_1].Calibration.Fixture.GetPoint(new EPoint(Camera_1_PixelX, Camera_1_PixelY), CoordinateHook.Hook);

                //float Camera_2_PixelX = Convert.ToSingle(eVision[VisionSetting.Camera_2].Calibration.CalibrTable.Rows[(eVision[VisionSetting.Camera_2].Calibration.CalibrTable.Rows.Count - 1) / 2]["Pixel_X"]);
                //float Camera_2_PixelY = Convert.ToSingle(eVision[VisionSetting.Camera_2].Calibration.CalibrTable.Rows[(eVision[VisionSetting.Camera_2].Calibration.CalibrTable.Rows.Count - 1) / 2]["Pixel_Y"]);
                float Camera_2_PixelX = 800;
                float Camera_2_PixelY = 600;
                EPoint Camera_2_CalibrCenter = eVision[VisionSetting.Camera_2].Calibration.Fixture.GetPoint(new EPoint(Camera_2_PixelX, Camera_2_PixelY), CoordinateHook.Hook);


                switch (AutoCalibrRotate_ChkRet_p)
                {
                    case 0://初始化       
                        eVision[VisionSetting.Camera_1].Calibration.Fixture.SetOrigin(0, 0);
                        eVision[VisionSetting.Camera_2].Calibration.Fixture.SetOrigin(0, 0);
                        rotatePoint_1.Clear();
                        rotatePoint_2.Clear();
                        cameraPosition.Clear();
                        CalibrRotateBothPoint_ChkRet(nowX, nowY, nowZ, nowθ, ref calibrRotatePoint_1, ref calibrRotatePoint_2, false);
                        AutoCalibrRotate_ChkRet_p = 2;
                        break;
                    case 2:
                        if (GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ) == conProg.process.Success)
                        {
                            GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ, false);
                            AutoCalibrRotate_ChkRet_p = 3;
                        }
                        break;
                    case 3:
                        if (GoToAbsPosition(tableNow_X, tableNow_Y, 300, tableNow_θ) == conProg.process.Success)
                        {
                            GoToAbsPosition(tableNow_X, tableNow_Y, 300, tableNow_θ, false);
                            AutoCalibrRotate_ChkRet_p = 4;
                        }
                        break;
                    case 4:
                        if (GoToAbsPosition(nowX, nowY, 300, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition(nowX, nowY, 300, nowθ, false);
                            AutoCalibrRotate_ChkRet_p = 5;
                        }
                        break;
                    case 5:
                        if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);
                            DO(2, false);
                            DO(1, true);
                            dtmrAutoCalibrRotate.Start(2000);
                            AutoCalibrRotate_ChkRet_p = 6;
                        }
                        break;
                    case 6:
                        if (dtmrAutoCalibrRotate.IsTimeOut)
                            AutoCalibrRotate_ChkRet_p = 10;
                        break;
                    case 10:
                        if (CalibrRotateBothPoint_ChkRet(nowX, nowY, nowZ, nowθ, ref calibrRotatePoint_1, ref calibrRotatePoint_2) == conProg.process.Success)
                        {
                            CalibrRotateBothPoint_ChkRet(nowX, nowY, nowZ, nowθ, ref calibrRotatePoint_1, ref calibrRotatePoint_2, false);
                            rotatePoint_1.Add(calibrRotatePoint_1);
                            rotatePoint_2.Add(calibrRotatePoint_2);

                            eVision[VisionSetting.Camera_1].GetPatternWordResults(eVision[VisionSetting.Camera_1].FindPattern, ref calibrRotateBothPattern_1);
                            eVision[VisionSetting.Camera_2].GetPatternWordResults(eVision[VisionSetting.Camera_2].FindPattern, ref calibrRotateBothPattern_2);

                            AutoCalibrRotate_ChkRet_p = 20;
                        }
                        break;
                    case 20:
                        if (CalibrRotateBothPoint_ChkRet(nowX, nowY, nowZ, nowθ + rotateRange, ref calibrRotatePoint_1, ref calibrRotatePoint_2) == conProg.process.Success)
                        {
                            CalibrRotateBothPoint_ChkRet(nowX, nowY, nowZ, nowθ, ref calibrRotatePoint_1, ref calibrRotatePoint_2, false);
                            rotatePoint_1.Add(calibrRotatePoint_1);
                            rotatePoint_2.Add(calibrRotatePoint_2);
                            AutoCalibrRotate_ChkRet_p = 30;
                        }
                        break;
                    case 30:
                        if (CalibrRotateBothPoint_ChkRet(nowX, nowY, nowZ, nowθ + 180, ref calibrRotatePoint_1, ref calibrRotatePoint_2) == conProg.process.Success)
                        {
                            CalibrRotateBothPoint_ChkRet(nowX, nowY, nowZ, nowθ, ref calibrRotatePoint_1, ref calibrRotatePoint_2, false);
                            rotatePoint_1.Add(calibrRotatePoint_1);
                            rotatePoint_2.Add(calibrRotatePoint_2);
                            AutoCalibrRotate_ChkRet_p = 40;
                        }
                        break;
                    case 40:
                        if (CalibrRotateBothPoint_ChkRet(nowX, nowY, nowZ, nowθ + 180 + rotateRange, ref calibrRotatePoint_1, ref calibrRotatePoint_2) == conProg.process.Success)
                        {
                            CalibrRotateBothPoint_ChkRet(nowX, nowY, nowZ, nowθ, ref calibrRotatePoint_1, ref calibrRotatePoint_2, false);
                            rotatePoint_1.Add(calibrRotatePoint_1);
                            rotatePoint_2.Add(calibrRotatePoint_2);
                            AutoCalibrRotate_ChkRet_p = 50;
                        }
                        break;
                    case 50:
                        double cameraNowθ_1_X = (rotatePoint_1[0].X + rotatePoint_1[2].X) / 2;
                        double cameraNowθ_1_Y = (rotatePoint_1[0].Y + rotatePoint_1[2].Y) / 2;
                        double cameraNowθ_2_X = (rotatePoint_2[0].X + rotatePoint_2[2].X) / 2;
                        double cameraNowθ_2_Y = (rotatePoint_2[0].Y + rotatePoint_2[2].Y) / 2;
                        double cameraRotateRange_1_X = (rotatePoint_1[1].X + rotatePoint_1[3].X) / 2;
                        double cameraRotateRange_1_Y = (rotatePoint_1[1].Y + rotatePoint_1[3].Y) / 2;
                        double cameraRotateRange_2_X = (rotatePoint_2[1].X + rotatePoint_2[3].X) / 2;
                        double cameraRotateRange_2_Y = (rotatePoint_2[1].Y + rotatePoint_2[3].Y) / 2;

                        /*============================================================================================================*/

                        double vectorX = /*rotatePoint_1[1].X - rotatePoint_1[0].X;//*/ cameraRotateRange_1_X - cameraNowθ_1_X;
                        double vectorY = /*rotatePoint_1[1].Y - rotatePoint_1[0].Y;//*/ cameraRotateRange_1_Y - cameraNowθ_1_Y;
                        double cameraPosition_1_angle = Math.Atan2(vectorY, vectorX) / Math.PI * 180 - 90 - (rotateRange / 2);
                        cameraPosition.Add(new CameraPosition(
                            nowX,
                            nowY,
                            cameraNowθ_1_X,
                            cameraNowθ_1_Y,
                            diagonalLength / 2,
                            cameraPosition_1_angle));

                        vectorX = /*rotatePoint_2[1].X - rotatePoint_2[0].X;//*/ cameraRotateRange_2_X - cameraNowθ_2_X;
                        vectorY = /*rotatePoint_2[1].Y - rotatePoint_2[0].Y;//*/ cameraRotateRange_2_Y - cameraNowθ_2_Y;
                        double cameraPosition_2_angle = Math.Atan2(vectorY, vectorX) / Math.PI * 180 - 90 - (rotateRange / 2);
                        cameraPosition.Add(new CameraPosition(
                            nowX,
                            nowY,
                            cameraNowθ_2_X,
                            cameraNowθ_2_Y,
                            diagonalLength / 2,
                            cameraPosition_2_angle));

                        /*============================================================================================================*/
                        EPoint subPatternCorner_1 = rotatePoint_1[0] - calibrRotateBothPattern_1;
                        EPoint subPatternCorner_2 = rotatePoint_2[0] - calibrRotateBothPattern_2;

                        eVision[VisionSetting.Camera_1].Calibration.Fixture.SetOrigin(
                            Convert.ToSingle(cameraPosition[0].World_X + (alignPose.X - cameraNowθ_1_X) + (Camera_1_CalibrCenter.X - alignPose.X) - Camera_1_CalibrCenter.X),
                            Convert.ToSingle(cameraPosition[0].World_Y + (alignPose.Y - cameraNowθ_1_Y) + (Camera_1_CalibrCenter.Y - alignPose.Y) - Camera_1_CalibrCenter.Y));

                        eVision[VisionSetting.Camera_2].Calibration.Fixture.SetOrigin(
                            Convert.ToSingle(cameraPosition[1].World_X + (alignPose.X - cameraNowθ_2_X) + (Camera_2_CalibrCenter.X - alignPose.X) - Camera_2_CalibrCenter.X),
                            Convert.ToSingle(cameraPosition[1].World_Y + (alignPose.Y - cameraNowθ_2_Y) + (Camera_2_CalibrCenter.Y - alignPose.Y) - Camera_2_CalibrCenter.Y));

                        /*============================================================================================================*/
                        nudCamera_1_Position_X.Value = Convert.ToDecimal(-eVision[VisionSetting.Camera_1].Calibration.Fixture.OriginX);
                        nudCamera_1_Position_Y.Value = Convert.ToDecimal(-eVision[VisionSetting.Camera_1].Calibration.Fixture.OriginY);
                        nudCamera_2_Position_X.Value = Convert.ToDecimal(-eVision[VisionSetting.Camera_2].Calibration.Fixture.OriginX);
                        nudCamera_2_Position_Y.Value = Convert.ToDecimal(-eVision[VisionSetting.Camera_2].Calibration.Fixture.OriginY);

                        AutoCalibrRotate_ChkRet_p = 55;
                        break;
                    case 55:
                        if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ - 180 - rotateRange) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ - 180 - rotateRange, false);
                            AutoCalibrRotate_ChkRet_p = 60;
                        }
                        break;
                    case 60:
                        if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);
                            DO(1, false);
                            DO(2, true);
                            dtmrAutoCalibrRotate.Start(100);
                            AutoCalibrRotate_ChkRet_p = 70;
                        }
                        break;
                    case 70:
                        if (dtmrAutoCalibrRotate.IsTimeOut)
                        {
                            DO(2, false);
                            if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ + Z_Up, nowθ) == conProg.process.Success)
                            {
                                GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ + Z_Up, nowθ, false);
                                AutoCalibrRotate_ChkRet_p = 9999;
                            }
                        }
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            AutoCalibrRotate_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            AutoCalibrRotate_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            AutoCalibrRotate_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    AutoCalibrRotate_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }

        List<EPoint> cornerBoth = new List<EPoint>();

        int CalibrRotateBothPoint_ChkRet_p = 0;
        private object Lock_CalibrRotateBothPoint_ChkRet = new object();
        public ushort CalibrRotateBothPoint_ChkRet(double nowX, double nowY,double nowZ, double nowθ, ref EPoint corner_1, ref EPoint corner_2, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_CalibrRotateBothPoint_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    CalibrRotateBothPoint_ChkRet_p = 9999;

                switch (CalibrRotateBothPoint_ChkRet_p)
                {
                    case 0://初始化
                        cornerBoth.Clear();
                        GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);
                        TriggerBothCamera_ChkRet(false);
                        CalibrRotateBothPoint_ChkRet_p = 10;
                        break;
                    case 10://移動
                        if (GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(nowX, nowY, nowZ, nowθ, false);
                            CalibrRotateBothPoint_ChkRet_p = 20;
                        }
                        break;
                    case 20://拍照     
                        if (TriggerBothCamera_ChkRet() == conProg.process.Success)
                        {
                            TriggerBothCamera_ChkRet(false);
                            CalibrRotateBothPoint_ChkRet_p = 40;
                        }
                        break;
                    case 30:
                        CalibrRotateBothPoint_ChkRet_p = 40;
                        break;
                    case 40://FindCorner_1
                        if (eVision[VisionSetting.Camera_1].FindPattern.FindPattern())
                        {
                            eVision[VisionSetting.Camera_1].Fixture.SetOrigin(
                                -eVision[VisionSetting.Camera_1].FindPattern.Results_CenterX,
                                -eVision[VisionSetting.Camera_1].FindPattern.Results_CenterY,
                                eVision[VisionSetting.Camera_1].FindPattern.Results_Angle);  
                            ezPoint corner = new ezPoint();
                            eVision[VisionSetting.Camera_1].GetCornerWordResults(ref corner);
                            cornerBoth.Add(new EPoint(Convert.ToSingle(corner.x), Convert.ToSingle(corner.y)));
                            eVision[VisionSetting.Camera_1].FindCorner[CornerLine.Line_1].ShowImage(true);
                            eVision[VisionSetting.Camera_1].ShowCornerResult(picDisplay_1);

                            EPoint corner1 = new EPoint();
                            eVision[VisionSetting.Camera_1].GetPatternWordResults(eVision[VisionSetting.Camera_1].FindPattern, ref corner1);
                            //eVision[VisionSetting.Camera_1].FindPattern.ShowResult(true);
                            //cornerBoth.Add(corner1);

                            ezPoint ezPoint = new ezPoint();
                            eVision[VisionSetting.Camera_1].GetCornerResults(ref ezPoint);

                            CalibrRotateBothPoint_ChkRet_p = 50;
                        }
                        else
                        {
                            strError = "相機1_找不到特徵";
                            CalibrRotateBothPoint_ChkRet_p = 8000;
                        }
                        break;
                    case 50://FindCorner_2
                        if (eVision[VisionSetting.Camera_2].FindPattern.FindPattern())
                        {
                            eVision[VisionSetting.Camera_2].Fixture.SetOrigin(
                                -eVision[VisionSetting.Camera_2].FindPattern.Results_CenterX,
                                -eVision[VisionSetting.Camera_2].FindPattern.Results_CenterY,
                                eVision[VisionSetting.Camera_2].FindPattern.Results_Angle);
                            ezPoint corner = new ezPoint();
                            eVision[VisionSetting.Camera_2].GetCornerWordResults(ref corner);
                            cornerBoth.Add(new EPoint(Convert.ToSingle(corner.x), Convert.ToSingle(corner.y)));
                            eVision[VisionSetting.Camera_2].FindCorner[CornerLine.Line_1].ShowImage(true);
                            eVision[VisionSetting.Camera_2].ShowCornerResult(picDisplay_2);

                            EPoint corner1 = new EPoint();
                            eVision[VisionSetting.Camera_2].GetPatternWordResults(eVision[VisionSetting.Camera_2].FindPattern, ref corner1);
                            //eVision[VisionSetting.Camera_2].FindPattern.ShowResult(true);
                            //cornerBoth.Add(corner);

                            ezPoint ezPoint = new ezPoint();
                            eVision[VisionSetting.Camera_2].GetCornerResults(ref ezPoint);

                            CalibrRotateBothPoint_ChkRet_p = 60;
                        }
                        else
                        {
                            strError = "相機2_找不到特徵";
                            CalibrRotateBothPoint_ChkRet_p = 8000;
                        }
                        break;
                    case 60:
                        if (cornerBoth.Count >= 2)
                        {
                            corner_1 = cornerBoth[0];
                            corner_2 = cornerBoth[1];
                        }
                        CalibrRotateBothPoint_ChkRet_p = 9999;
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            CalibrRotateBothPoint_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            CalibrRotateBothPoint_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化   
                        if (cornerBoth.Count >= 2)
                        {
                            corner_1 = cornerBoth[0];
                            corner_2 = cornerBoth[1];
                        }
                        nValue = conProg.process.Success;
                        if (Action == false)
                            CalibrRotateBothPoint_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    CalibrSinglePoint_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }

        DelayTime dtmrTriggerBoth = new DelayTime();
        bool isTriggerCamera_1;
        bool isTriggerCamera_2;
        int TriggerBothCamera_ChkRet_p = 0;
        private object Lock_TriggerBothCamera_ChkRet = new object();
        public ushort TriggerBothCamera_ChkRet(bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_TriggerBothCamera_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    TriggerBothCamera_ChkRet_p = 9999;

                switch (TriggerBothCamera_ChkRet_p)
                {
                    case 0://初始化
                        isTriggerCamera_1 = false;
                        isTriggerCamera_2 = false;
                        imageUpDataFlag[VisionSetting.Camera_1] = false;
                        imageUpDataFlag[VisionSetting.Camera_2] = false;
                        dtmrTriggerBoth.Start(timeOut);
                        TriggerBothCamera_ChkRet_p = 10;
                        break;
                    case 10://拍照                        
                        if (!isTriggerCamera_1)
                        {
                            if (baslerCamera[(int)VisionSetting.Camera_1].OneShot())
                                isTriggerCamera_1 = true;
                        }
                        if (!isTriggerCamera_2)
                        {
                            if (baslerCamera[(int)VisionSetting.Camera_2].OneShot())
                                isTriggerCamera_2 = true;
                        }

                        if (dtmrTriggerBoth.IsTimeOut)
                        {
                            if (!isTriggerCamera_1)
                                strError += "Camera_1 ";
                            if (!isTriggerCamera_2)
                                strError += "Camera_2 ";
                            strError += "Trigger Time out";
                            TriggerBothCamera_ChkRet_p = 8000;
                        }
                        if (isTriggerCamera_1 && isTriggerCamera_2)
                        {
                            strError = string.Empty;
                            dtmrTriggerBoth.Start(timeOut);
                            TriggerBothCamera_ChkRet_p = 20;
                        }
                        break;
                    case 20://拍照是否成功
                        if (dtmrTriggerBoth.IsTimeOut)
                        {
                            if (!imageUpDataFlag[VisionSetting.Camera_1])
                                strError += "Camera_1 ";
                            if (!imageUpDataFlag[VisionSetting.Camera_2])
                                strError += "Camera_2 ";
                            strError += "Get Image Time out";
                            TriggerBothCamera_ChkRet_p = 8000;
                        }
                        if (imageUpDataFlag[VisionSetting.Camera_1] && imageUpDataFlag[VisionSetting.Camera_2])
                        {
                            strError = string.Empty;
                            eVision[VisionSetting.Camera_1].InputImage.Save("C:\\Users\\Experiment\\Desktop\\Camera_1_Image_" + rotatePoint_1.Count + ".bmp");
                            eVision[VisionSetting.Camera_2].InputImage.Save("C:\\Users\\Experiment\\Desktop\\Camera_2_Image_" + rotatePoint_2.Count + ".bmp");
                            TriggerBothCamera_ChkRet_p = 9999;
                        }
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            TriggerBothCamera_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            TriggerBothCamera_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            TriggerBothCamera_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    TriggerBothCamera_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }
        #endregion

        #region 記錄姿態
        int RecordingGesture_ChkRet_p = 0;
        private object Lock_RecordingGesture_ChkRet = new object();
        public ushort RecordingGesture_ChkRet(double nowX, double nowY,double nowZ, double nowθ, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_RecordingGesture_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    RecordingGesture_ChkRet_p = 9999;

                EPoint calibrRotatePoint_1 = new EPoint();
                EPoint calibrRotatePoint_2 = new EPoint();

                switch (RecordingGesture_ChkRet_p)
                {
                    case 0://初始化
                        GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ, false);
                        CalibrRotateBothPoint_ChkRet(nowX, nowY, nowZ, nowθ, ref calibrRotatePoint_1, ref calibrRotatePoint_2, false);
                        RecordingGesture_ChkRet_p = 2;
                        break;
                    case 2:
                        if (GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ) == conProg.process.Success)
                        {
                            GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ, false);
                            RecordingGesture_ChkRet_p = 10;
                        }
                        break;
                    case 3:
                        if (GoToAbsPosition(tableNow_X, tableNow_Y, 300, tableNow_θ) == conProg.process.Success)
                        {
                            GoToAbsPosition(tableNow_X, tableNow_Y, 300, tableNow_θ, false);
                            RecordingGesture_ChkRet_p = 4;
                        }
                        break;
                    case 4:
                        if (GoToAbsPosition(nowX, nowY, 300, nowθ) == conProg.process.Success)
                        {
                            GoToAbsPosition(nowX, nowY, 300, nowθ, false);
                            RecordingGesture_ChkRet_p = 10;
                        }
                        break;
                    case 10:
                        if (CalibrRotateBothPoint_ChkRet(tableNow_X, tableNow_Y, tableNow_Z, tableNow_θ, ref calibrRotatePoint_1, ref calibrRotatePoint_2) == conProg.process.Success)
                        {
                            CalibrRotateBothPoint_ChkRet(tableNow_X, tableNow_Y, tableNow_Z, tableNow_θ, ref calibrRotatePoint_1, ref calibrRotatePoint_2, false);

                            EPoint camera_1 = calibrRotatePoint_1;
                            EPoint camera_2 = calibrRotatePoint_2;
                            txtRecordingGesture_X.Text = ( ((camera_1.X + camera_2.X) / 2)).ToString();
                            txtRecordingGesture_Y.Text = ( ((camera_1.Y + camera_2.Y) / 2)).ToString();
                            txtRecordingGesture_θ.Text = ( (Math.Atan2(camera_1.Y - camera_2.Y, camera_1.X - camera_2.X) / Math.PI * 180)).ToString();
                            txtDiagonalLength.Text = Math.Sqrt(Math.Pow(camera_2.Y - camera_1.Y, 2) + Math.Pow(camera_2.X - camera_1.X, 2)).ToString();

                            alignResult[0] = new EPoint(camera_1.X, camera_1.Y);
                            alignResult[1] = new EPoint(camera_2.X, camera_2.Y);                            

                            txtCamera_1_X.Text = camera_1.X.ToString("0.000");
                            txtCamera_1_Y.Text = camera_1.Y.ToString("0.000");
                            txtCamera_2_X.Text = camera_2.X.ToString("0.000");
                            txtCamera_2_Y.Text = camera_2.Y.ToString("0.000");

                            RecordingGesture_ChkRet_p = 9999;
                        }
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            RecordingGesture_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            RecordingGesture_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            RecordingGesture_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    RecordingGesture_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }
        #endregion
        
        int GetNowPosition_ChkRet_p = 0;
        private object Lock_GetNowPosition_ChkRet = new object();
        public ushort GetNowPosition_ChkRet( bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_GetNowPosition_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    GetNowPosition_ChkRet_p = 9999;

                nudCamera_1_Position_X.Value = Convert.ToDecimal(-eVision[VisionSetting.Camera_1].Calibration.Fixture.OriginX);
                nudCamera_1_Position_Y.Value = Convert.ToDecimal(-eVision[VisionSetting.Camera_1].Calibration.Fixture.OriginY);                      
                nudCamera_2_Position_X.Value = Convert.ToDecimal(-eVision[VisionSetting.Camera_2].Calibration.Fixture.OriginX);
                nudCamera_2_Position_Y.Value = Convert.ToDecimal(-eVision[VisionSetting.Camera_2].Calibration.Fixture.OriginY);


                switch (GetNowPosition_ChkRet_p)
                {
                    case 0://初始化
                        TriggerBothCamera_ChkRet(false);
                        cornerBoth.Clear();
                        GetNowPosition_ChkRet_p = 10;
                        break;
                    case 10://拍照     
                        if (TriggerBothCamera_ChkRet() == conProg.process.Success)
                        {
                            TriggerBothCamera_ChkRet(false);
                            GetNowPosition_ChkRet_p = 20;
                        }
                        break;
                    case 20://FindCorner_1
                        if (eVision[VisionSetting.Camera_1].FindPattern.FindPattern())
                        {
                            eVision[VisionSetting.Camera_1].Fixture.SetOrigin(
                                -eVision[VisionSetting.Camera_1].FindPattern.Results_CenterX,
                                -eVision[VisionSetting.Camera_1].FindPattern.Results_CenterY,
                                eVision[VisionSetting.Camera_1].FindPattern.Results_Angle);
                            ezPoint corner = new ezPoint();
                            eVision[VisionSetting.Camera_1].GetCornerWordResults(ref corner);
                            cornerBoth.Add(new EPoint(Convert.ToSingle(corner.x), Convert.ToSingle(corner.y)));
                            eVision[VisionSetting.Camera_1].FindCorner[CornerLine.Line_1].ShowImage(true);
                            eVision[VisionSetting.Camera_1].ShowCornerResult(picDisplay_1);

                            //EPoint corner = new EPoint();
                            //eVision[VisionSetting.Camera_1].GetPatternWordResults(eVision[VisionSetting.Camera_1].FindPattern, ref corner);
                            //eVision[VisionSetting.Camera_1].FindPattern.ShowResult(true);
                            //cornerBoth.Add(corner);

                            GetNowPosition_ChkRet_p = 30;
                        }
                        else
                        {
                            strError = "相機1_找不到特徵";
                            GetNowPosition_ChkRet_p = 8000;
                        }
                        break;
                    case 30://FindCorner_2
                        if (eVision[VisionSetting.Camera_2].FindPattern.FindPattern())
                        {
                            eVision[VisionSetting.Camera_2].Fixture.SetOrigin(
                                -eVision[VisionSetting.Camera_2].FindPattern.Results_CenterX,
                                -eVision[VisionSetting.Camera_2].FindPattern.Results_CenterY,
                                eVision[VisionSetting.Camera_2].FindPattern.Results_Angle);
                            ezPoint corner = new ezPoint();
                            eVision[VisionSetting.Camera_2].GetCornerWordResults(ref corner);
                            cornerBoth.Add(new EPoint(Convert.ToSingle(corner.x), Convert.ToSingle(corner.y)));
                            eVision[VisionSetting.Camera_2].FindCorner[CornerLine.Line_1].ShowImage(true);
                            eVision[VisionSetting.Camera_2].ShowCornerResult(picDisplay_2);

                            //EPoint corner = new EPoint();
                            //eVision[VisionSetting.Camera_2].GetPatternWordResults(eVision[VisionSetting.Camera_2].FindPattern, ref corner);
                            //eVision[VisionSetting.Camera_2].FindPattern.ShowResult(true);
                            //cornerBoth.Add(corner);

                            GetNowPosition_ChkRet_p = 40;
                        }
                        else
                        {
                            strError = "相機2_找不到特徵";
                            GetNowPosition_ChkRet_p = 8000;
                        }
                        break;
                    case 40:
                        EPoint camera_1 = cornerBoth[0];
                        EPoint camera_2 = cornerBoth[1];
                        txtAlignmentPosition_X.Text = ( ((camera_1.X + camera_2.X) / 2)).ToString();
                        txtAlignmentPosition_Y.Text = ( ((camera_1.Y + camera_2.Y) / 2)).ToString();
                        txtAlignmentPosition_θ.Text = ( (Math.Atan2(camera_1.Y - camera_2.Y, camera_1.X - camera_2.X) / Math.PI * 180)).ToString();
                        txtDiagonalLength.Text = Math.Sqrt(Math.Pow(camera_2.Y - camera_1.Y, 2) + Math.Pow(camera_2.X - camera_1.X, 2)).ToString();

                        alignResult[2] = new EPoint(camera_1.X, camera_1.Y);
                        alignResult[3] = new EPoint(camera_2.X, camera_2.Y);

                        double angle= Math.Atan2(camera_1.Y - camera_2.Y, camera_1.X - camera_2.X) / Math.PI * 180;
                        double length = Math.Sqrt(Math.Pow(camera_2.Y - camera_1.Y, 2) + Math.Pow(camera_2.X - camera_1.X, 2));

                        //double r1 = Math.Sqrt(Math.Pow(camera_1.X - alignPose.X, 2) + Math.Pow(camera_1.Y - alignPose.Y, 2));
                        //double r2 = Math.Sqrt(Math.Pow(camera_2.X - alignPose.X, 2) + Math.Pow(camera_2.Y - alignPose.Y, 2));
                        double calibr_width = Convert.ToDouble(nudCalibrWidth.Value);
                        double calibr_height = Convert.ToDouble(nudCalibrHeight.Value);
                        double calibr_Angle = Math.Atan2(calibr_height, calibr_width) / Math.PI * 180;

                        double r1 = length * Math.Cos(calibr_Angle * Math.PI / 180);
                        double r2 = length * Math.Sin(calibr_Angle * Math.PI / 180);

                        alignmentTable.Rows.Add(
                            camera_1.X.ToString("0.000"),
                            camera_1.Y.ToString("0.000"),
                            camera_2.X.ToString("0.000"),
                            camera_2.Y.ToString("0.000"),
                            angle.ToString("0.000"),
                            length.ToString("0.000"),
                            r1.ToString("0.000"),
                            r2.ToString("0.000"));

                        double Camera_1_X_Target = Convert.ToDouble(txtCamera_1_X.Text);
                        double Camera_1_Y_Target = Convert.ToDouble(txtCamera_1_Y.Text);
                        double Camera_2_X_Target = Convert.ToDouble(txtCamera_2_X.Text);
                        double Camera_2_Y_Target = Convert.ToDouble(txtCamera_2_Y.Text);

                        txtCamera_1_Xerror.Text = (camera_1.X - Camera_1_X_Target).ToString("0.000");
                        txtCamera_1_Yerror.Text = (camera_1.Y - Camera_1_Y_Target).ToString("0.000");
                        txtCamera_2_Xerror.Text = (camera_2.X - Camera_2_X_Target).ToString("0.000");
                        txtCamera_2_Yerror.Text = (camera_2.Y - Camera_2_Y_Target).ToString("0.000");


                        double angle_1 = Convert.ToDouble(txtRecordingGesture_θ.Text);
                        textBox1.Text = (angle - angle_1).ToString("0.000");

                        GetNowPosition_ChkRet_p = 9999;
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            GetNowPosition_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            GetNowPosition_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            GetNowPosition_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    GetNowPosition_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }

        double tableMoveX=0;
        double tableMoveY=0;
        double tableMoveZ = 0;
        double tableMoveθ=0;
        int MoveTargetPosition_ChkRet_p = 0;
        private object Lock_MoveTargetPosition_ChkRet = new object();
        public ushort MoveTargetPosition_ChkRet(bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_MoveTargetPosition_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    MoveTargetPosition_ChkRet_p = 9999;

                switch (MoveTargetPosition_ChkRet_p)
                {
                    case 0://初始化
                        GoToAbsPosition_AutoUp_ChkRet(tableMoveX, tableMoveY, tableMoveZ, tableMoveθ, false);
                        MoveTargetPosition_ChkRet_p = 10;
                        break;
                    case 10:
                        double tableNow_X = 0;
                        double tableNow_Y = 0;
                        double tableNow_Z = 0;
                        double tableNow_θ = 0;
                        GetTablePosition(ref tableNow_X, ref tableNow_Y,ref tableNow_Z, ref tableNow_θ);

                        double alignNow_θ = Convert.ToDouble(txtAlignmentPosition_θ.Text);
                        double alignTarget_θ = Convert.ToDouble(txtRecordingGesture_θ.Text);

                        double afterRotation_θ_1 = (Math.Atan2(alignResult[0].Y - alignPose.Y, alignResult[0].X - alignPose.X) / Math.PI * 180);
                        double radius_1 = Math.Sqrt(Math.Pow(alignResult[2].X - tableNow_X, 2) + Math.Pow(alignResult[2].Y - tableNow_Y, 2));
                        double afterRotation_X_1 = radius_1 * (Math.Cos(afterRotation_θ_1 / 180 * Math.PI));
                        double afterRotation_Y_1 = radius_1 * (Math.Sin(afterRotation_θ_1 / 180 * Math.PI));

                        double afterRotation_θ_2 = (Math.Atan2(alignResult[1].Y - alignPose.Y, alignResult[1].X - alignPose.X) / Math.PI * 180);
                        double radius_2 = Math.Sqrt(Math.Pow(alignResult[3].X - tableNow_X, 2) + Math.Pow(alignResult[3].Y - tableNow_Y, 2));
                        double afterRotation_X_2 = radius_2 * (Math.Cos(afterRotation_θ_2 / 180 * Math.PI));
                        double afterRotation_Y_2 = radius_2 * (Math.Sin(afterRotation_θ_2 / 180 * Math.PI));

                        double afterRotation_X_Center = (afterRotation_X_1 + afterRotation_X_2) / 2;
                        double afterRotation_Y_Center = (afterRotation_Y_1 + afterRotation_Y_2) / 2;

                        double beforeRotation_X_Center = (alignResult[0].X + alignResult[1].X) / 2;
                        double beforeRotation_Y_Center = (alignResult[0].Y + alignResult[1].Y) / 2;

                        ezAngle angle = new ezAngle(
                            new ezLine(alignResult[0].X, alignResult[0].Y, alignResult[1].X, alignResult[1].Y),
                            new ezLine(alignResult[2].X, alignResult[2].Y, alignResult[3].X, alignResult[3].Y));

                        tableMoveX =  (beforeRotation_X_Center - afterRotation_X_Center);
                        tableMoveY =  (beforeRotation_Y_Center - afterRotation_Y_Center);
                        tableMoveθ = tableNow_θ + (alignTarget_θ - alignNow_θ);

                        MoveTargetPosition_ChkRet_p = 20;
                        break;
                    case 20://移動
                        if (GoToAbsPosition_AutoUp_ChkRet(tableMoveX, tableMoveY, tableMoveZ, tableMoveθ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(tableMoveX, tableMoveY, tableMoveZ, tableMoveθ, false);
                            MoveTargetPosition_ChkRet_p = 9999;
                        }
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            MoveTargetPosition_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            MoveTargetPosition_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            MoveTargetPosition_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    MoveTargetPosition_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }
        
        XYθ alignTerage = new XYθ();
        DelayTime dtmrRobotAlign = new DelayTime();
        int RobotAlign_ChkRet_p = 0;
        private object Lock_RobotAlign_ChkRet = new object();
        public ushort RobotAlign_ChkRet(double align_X,double align_Y,double align_θ, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_RobotAlign_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    RobotAlign_ChkRet_p = 9999;

                switch (RobotAlign_ChkRet_p)
                {
                    case 0://初始化
                        GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ, false);
                        GoToAbsPosition_AutoUp_ChkRet(align_X, align_Y, 280, align_θ, false);
                        GetNowPosition_ChkRet(false);
                        RobotAlign_ChkRet_p = 10;
                        break;
                    case 10://取得目前位置
                        if (GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ) == conProg.process.Success)
                        {
                            GetTablePosition(ref tableNow_X, ref tableNow_Y, ref tableNow_Z, ref tableNow_θ, false);
                            RobotAlign_ChkRet_p = 20;
                        }
                        break;
                    case 20://移動至目前位置上方
                        if (GoToAbsPosition(tableNow_X, tableNow_Y, 280, tableNow_θ) == conProg.process.Success)
                        {
                            GoToAbsPosition(tableNow_X, tableNow_Y, 280, tableNow_θ, false);
                            RobotAlign_ChkRet_p = 30;
                        }
                        break;
                    case 30://移動至待命位上方
                        if (GoToAbsPosition(align_X, align_Y, 280, align_θ) == conProg.process.Success)
                        {
                            GoToAbsPosition(align_X, align_Y, 280, align_θ, false);
                            RobotAlign_ChkRet_p = 40;
                        }
                        break;
                    case 40://拍照取得物料位置
                        if (GetNowPosition_ChkRet() == conProg.process.Success)
                        {
                            GetNowPosition_ChkRet(false);
                            RobotAlign_ChkRet_p = 50;
                        }
                        break;
                    case 50://移動至物料位置上方
                        EPoint camera_1 = cornerBoth[0];
                        EPoint camera_2 = cornerBoth[1];
                        alignTerage.X = (camera_1.X + camera_2.X) / 2;
                        alignTerage.Y = (camera_1.Y + camera_2.Y) / 2;
                        alignTerage.θ = alignPose.θ + Convert.ToDouble(textBox1.Text);
                        if (GoToAbsPosition(alignTerage.X, alignTerage.Y, 280, alignTerage.θ ) == conProg.process.Success)
                        {
                            GoToAbsPosition(alignTerage.X, alignTerage.Y, 280, alignTerage.θ , false);
                            RobotAlign_ChkRet_p = 60;
                        }
                        break;
                    case 60://移動至物料位置，吸真空
                        if (GoToAbsPosition_AutoUp_ChkRet(alignTerage.X, alignTerage.Y, 140, alignTerage.θ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(alignTerage.X, alignTerage.Y, 140, alignTerage.θ, false);
                            DO(2, false);
                            DO(1, true);
                            dtmrRobotAlign.Start(2000);
                            RobotAlign_ChkRet_p = 70;
                        }
                        break;
                    case 70://移動至物料位置上方
                        if(dtmrRobotAlign.IsTimeOut)
                        {
                            if (GoToAbsPosition(alignTerage.X, alignTerage.Y, 280, alignTerage.θ) == conProg.process.Success)
                            {
                                GoToAbsPosition(alignTerage.X, alignTerage.Y, 280, alignTerage.θ, false);
                                RobotAlign_ChkRet_p = 80;
                            }
                        }
                        break;
                    case 80://移動至待命位上方
                        if (GoToAbsPosition(robotAlignPosition.X, robotAlignPosition.Y, 280, robotAlignPosition.θ) == conProg.process.Success)
                        {
                            GoToAbsPosition(robotAlignPosition.X, robotAlignPosition.Y, 280, robotAlignPosition.θ, false);
                            RobotAlign_ChkRet_p = 90;
                        }
                        break;
                    case 90://移動至待命位，破真空
                        if (GoToAbsPosition_AutoUp_ChkRet(robotAlignPosition.X, robotAlignPosition.Y, 140, robotAlignPosition.θ) == conProg.process.Success)
                        {
                            GoToAbsPosition_AutoUp_ChkRet(robotAlignPosition.X, robotAlignPosition.Y, 140, robotAlignPosition.θ, false);
                            DO(1, false);
                            DO(2, true);
                            dtmrRobotAlign.Start(100);
                            RobotAlign_ChkRet_p = 100;
                        }
                        break;
                    case 100://移動至待命位上方
                        if (dtmrRobotAlign.IsTimeOut)
                        {
                            DO(2, false);
                            if (GoToAbsPosition(robotAlignPosition.X, robotAlignPosition.Y, 280, robotAlignPosition.θ) == conProg.process.Success)
                            {
                                GoToAbsPosition(robotAlignPosition.X, robotAlignPosition.Y, 280, robotAlignPosition.θ, false);                                
                                RobotAlign_ChkRet_p = 9999;
                            }
                        }
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            RobotAlign_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            RobotAlign_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            RobotAlign_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    RobotAlign_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    if (AlarmMessage.Alarm_SW == false)
                    {
                        if (CheckAlarm == true)
                            AlarmMessage.ShowAlarm(MethodBase.GetCurrentMethod().Name, 0, false);
                    }
                }
                return nValue;
            }
        }

        public class DelayTime
        {
            private System.Timers.Timer tmrDelay;
            public DelayTime()
            {
                tmrDelay = new System.Timers.Timer();
                tmrDelay.AutoReset = false;//設置是執行一次（false）還是一直執行(true)； 
                tmrDelay.Elapsed += TimeElapsed_EventProcessor;
            }
            /// <summary>
            /// 時間是否計時完成
            /// </summary>
            public bool IsTimeOut { get; private set; }
            /// <summary>
            /// 計時開始
            /// </summary>
            /// <param name="msec">計時時間(毫秒)</param>
            public void Start(double msec)
            {
                tmrDelay.Stop();
                IsTimeOut = false;
                tmrDelay.Interval = msec;
                tmrDelay.Start();
            }

            private void TimeElapsed_EventProcessor(object sender, System.Timers.ElapsedEventArgs e)
            {
                System.Timers.Timer tmr = (System.Timers.Timer)sender;
                tmr.Stop();
                IsTimeOut = true;
            }
        }

        private void btnVisionPatternResult_Click(object sender, EventArgs e)
        {
            if (eVision[visionSetting].FindPattern.FindPattern())
            {
                eVision[visionSetting].Fixture.SetOrigin(
                                    -eVision[visionSetting].FindPattern.Results_CenterX,
                                    -eVision[visionSetting].FindPattern.Results_CenterY,
                                    eVision[visionSetting].FindPattern.Results_Angle);
                eVision[visionSetting].FindPattern.GetPattern();
                eVision[visionSetting].FindPattern.ShowResult(true);

                //EPoint resultsPoint = eVision[visionSetting].Calibration.Base.SensorToWorld(
                //    new EPoint(eVision[visionSetting].FindPattern.Results_CenterX,
                //               eVision[visionSetting].FindPattern.Results_CenterY));

                EPoint resultsPoint = new EPoint();
                eVision[visionSetting].GetPatternWordResults(ref resultsPoint);

                labAlignResult_X.Text = resultsPoint.X.ToString("0.000");
                labAlignResult_Y.Text = resultsPoint.Y.ToString("0.000");
                labAlignResult_θ.Text = eVision[visionSetting].FindPattern.Results_Angle.ToString("0.000");
            }
        }

        private void btnVisionCornerResult_Click(object sender, EventArgs e)
        {
            if (eVision[visionSetting].FindPattern.FindPattern())
            {
                eVision[visionSetting].Fixture.SetOrigin(
                    -eVision[visionSetting].FindPattern.Results_CenterX,
                    -eVision[visionSetting].FindPattern.Results_CenterY,
                    eVision[visionSetting].FindPattern.Results_Angle);
                ezPoint corner = new ezPoint();
                eVision[visionSetting].GetCornerWordResults(ref corner);
                eVision[visionSetting].FindCorner[cornerLine].ShowImage(true);
                switch(visionSetting)
                {
                    case VisionSetting.Camera_1:
                        eVision[visionSetting].ShowCornerResult(picDisplay_1);
                        break;
                    case VisionSetting.Camera_2:
                        eVision[visionSetting].ShowCornerResult(picDisplay_2);
                        break;
                }       
                labCornerResult_X.Text = corner.x.ToString("0.000");
                labCornerResult_Y.Text = corner.y.ToString("0.000");
            }
        }

        private void btnbtnRecordingGesture_2_Click(object sender, EventArgs e)
        {
            btnRecordingGesture_Click(sender, e);
        }

        private void btnbtnAlignmentPosition_2_Click(object sender, EventArgs e)
        {
            btnAlignmentPosition_Click(sender, e);
        }

        private void btnbtnAlignmentTest_2_Click(object sender, EventArgs e)
        {
            btnAlignmentTest_Click(sender, e);
        }

        private void btnClearAlignResult_Click(object sender, EventArgs e)
        {
            alignmentTable.Clear();
        }

        private void btnSaveAlignResult_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                alignmentTable.WriteXmlSchema(saveFileDialog.FileName+ "_AlignResultSchema.xml");
                alignmentTable.WriteXml(saveFileDialog.FileName+ "_AlignResult.xml");
            }
        }        
    }

    public enum Robot_DO
    {
        /// <summary>
        /// 真空_吸
        /// </summary>
        Vacuum_Suction = 1,
        /// <summary>
        /// 真空_破
        /// </summary>
        Vacuum_Broken = 2,
    }

    public enum LawOfSinesResultType
    {
        θ = 0,
        SideLength,
    }

    public struct TransformationCoordinate
    {
        public Point_f Before { get; set; }
        public Point_f After { get; set; }
        public TransformationCoordinate(Point_f before, Point_f after)
        {
            Before = before;
            After = after;
        }

        public TransformationCoordinate(double before_X, double before_Y, double after_X, double after_Y)
            : this(new Point_f(before_X, before_Y), new Point_f(after_X, after_Y))
        {
        }
    }

    public struct Point_f
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point_f(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public struct AffineParameter
    {
        public double a { get; set; }
        public double b { get; set; }
        public double c { get; set; }
        public double d { get; set; }
        public double e { get; set; }
        public double f { get; set; }
    }

    public struct CameraPosition
    {
        public double World_X { get; set; }
        public double World_Y { get; set; }

        public CameraPosition(
            double calibr_center_X,
            double calibr_center_Y,
            double corner_X,
            double corner_Y,
            double r,
            double θ)
        {
            double point_X = r * Math.Cos(θ / 180 * Math.PI);
            double point_Y = r * Math.Sin(θ / 180 * Math.PI);

            World_X = calibr_center_X + point_X;// + (calibr_center_X - corner_X);
            World_Y = calibr_center_Y + point_Y;// + (calibr_center_Y - corner_Y);
        }
    }

    public struct Vector
    {
        public double Length { get; set; }
        public double Angle { get; set; }
        public double DirectionAngle { get; set; }

        public Vector(EPoint firstPoint,EPoint secondPoint)
        {
            double vector_X = secondPoint.X - firstPoint.X;
            double vector_Y = secondPoint.Y - firstPoint.Y;
            Angle = Math.Atan2(vector_Y, vector_X) / Math.PI * 180;
            Length = Math.Sqrt(Math.Pow(vector_X, 2) + Math.Pow(vector_Y, 2));
            DirectionAngle = Angle - 90;            
        }
    }

    public struct XYθ
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double θ { get; set; }
        public XYθ(double x,double y,double _θ)
        {
            X = x;
            Y = y;
            θ = _θ;
        }
    }
}
