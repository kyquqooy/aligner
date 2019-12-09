using Euresys.Open_eVision_1_2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjVisionController.Open_eVision
{ 
    public class clsEasyFindLine
    {
        private DataTable sampleTable;

        private bool isMouseEventPresence = false;
        private bool isDragging = false;
        private float recordX_1 = 0;
        private float recordY_1 = 0;
        private float recordX_2 = 0;
        private float recordY_2 = 0;
        private float mouseX = 0;
        private float mouseY = 0;
        private float panX = 0;
        private float panY = 0;
        private float zoomX = 1;
        private float zoomY = 1;

        public clsEasyFindLine(PictureBox display) : this(display, new ELineGauge())
        {
            Base.Dragable = true;
            Base.Resizable = true;
            Base.Rotatable = true;
            Base.Thickness = 3;
        }

        public clsEasyFindLine(PictureBox display, ELineGauge eLineGauge)
        {
            Display = display;
            Base = eLineGauge;
            InputImage = new EImageBW8();

            zoomX = Base.ZoomX;
            zoomY = Base.ZoomY;

            sampleTable = new DataTable("SampleTable");
            sampleTable.Columns.Add("PointIndex");
            sampleTable.Columns.Add("Center_X");
            sampleTable.Columns.Add("Center_Y");
            sampleTable.Columns.Add("Amplitude");
            sampleTable.Columns.Add("Area");
        }

        public ELineGauge Base { get; }

        public EImageBW8 InputImage { get; set; }

        public PictureBox Display { get; set; }

        public DataTable SampleTable { get { return sampleTable; } }

        #region 參數

        public float Position_CenterX { get { return Base.Center.X; } }

        public float Position_CenterY { get { return Base.Center.Y; } }

        public float Position_Tolerance { get { return Base.Tolerance; } }

        public float Position_Length { get { return Base.Length; } }

        public float Position_Angle { get { return Base.Angle; } }

        public ETransitionType Measurement_TransitionType { get { return Base.TransitionType; } }

        public int Measurement_Thickness { get { return (int)Base.Thickness; } }

        public ETransitionChoice Measurement_TransitionChoice { get { return Base.TransitionChoice; } }

        public int Measurement_TransitionIndex { get { return (int)Base.TransitionIndex; } }

        public int Measurement_Smoothing { get { return (int)Base.Smoothing; } }

        public int Measurement_Threshold { get { return (int)Base.Threshold; } }

        public int Measurement_MinAmplitude { get { return (int)Base.MinAmplitude; } }

        public int Measurement_MinArea { get { return (int)Base.MinArea; } }

        public bool Measurement_HVConstraint { get { return Base.HVConstraint; } }

        public bool Measurement_RectangularSamplingArea { get { return Base.RectangularSamplingArea; } }

        public float Fitting_SamplingStep { get { return Base.SamplingStep; } }

        public bool Fitting_KnownAngle { get { return Base.KnownAngle; } }

        public float Fitting_FilteringThreshold { get { return Base.FilteringThreshold; } }

        public int Fitting_NumFilteringPasses { get { return (int)Base.NumFilteringPasses; } }

        public float Results_CenterX { get { return Base.MeasuredLine.CenterX; } }

        public float Results_CenterY { get { return Base.MeasuredLine.CenterY; } }

        public float Results_Angle { get { return Base.MeasuredLine.Angle; } }
        #endregion

        public bool Save(string filePath)
        {
            bool reSuccess = false;
            try
            {
                Base.Save(filePath);
                reSuccess = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool Load(string filePath)
        {
            bool reSuccess = false;
            try
            {
                if (File.Exists(filePath))
                {
                    Base.Load(filePath);
                    reSuccess = true;
                }
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool LoadImage(string filePath)
        {
            bool reSuccess = false;
            try
            {
                InputImage.Load(filePath);
                SetROI();
                reSuccess = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool SetROIStart()
        {
            bool reSuccess = false;
            try
            {
                reSuccess = SetROIStart(Display);
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool SetROIEnd()
        {
            bool reSuccess = false;
            try
            {
                reSuccess = SetROIEnd(Display);
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool SetROI()
        {
            bool reSuccess = false;
            try
            {
                float centerX = Convert.ToSingle(InputImage.Width) / 2;
                float centerY = Convert.ToSingle(InputImage.Height) / 2;
                float tolerance = Convert.ToSingle(InputImage.Height) / 10;
                float length = Convert.ToSingle(InputImage.Width) / 3;
                reSuccess = SetROI(centerX, centerY, tolerance, length);
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool Hook(EShape Object)
        {
            bool reSuccess = false;
            try
            {

            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool TrainLine(ETransitionType transitionType, int thickness, int threshold, int minAmplitude, int minArea, float samplingStep)
        {
            bool reSuccess = false;
            try
            {
                reSuccess = TrainLine(
                    transitionType,
                    thickness,
                    ETransitionChoice.LargestAmplitude,
                    Measurement_TransitionIndex,
                    Measurement_Smoothing,
                    threshold,
                    minAmplitude,
                    minArea,
                    Measurement_HVConstraint,
                    Measurement_RectangularSamplingArea,
                    samplingStep,
                    Fitting_KnownAngle,
                    Fitting_FilteringThreshold,
                    Fitting_NumFilteringPasses);
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool FindLine(ref ELine line)
        {
            bool reSuccess = false;
            try
            {
                reSuccess = FindLine();
                line = Base.MeasuredLine;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool FindLine(ref float centerX, ref float centerY, ref float angle)
        {
            bool reSuccess = false;
            try
            {
                ELine line = new ELine();
                reSuccess = FindLine(ref line);
                centerX = line.CenterX;
                centerY = line.CenterY;
                angle = line.Angle;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool FindLine()
        {
            bool reSuccess = false;
            try
            {
                reSuccess = FindLine(InputImage);
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool GetSample()
        {
            bool reSuccess = false;
            try
            {
                reSuccess = GetSample(InputImage, ref sampleTable);
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool ShowImage(bool autoSize = false)
        {
            bool reSuccess = false;
            try
            {
                reSuccess = ShowImage(Display, InputImage, autoSize);
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool ShowROI(bool autoSize = false)
        {
            bool reSuccess = false;
            try
            {
                reSuccess = ShowROI(Display, autoSize);
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool ShowResult(bool autoSize = false)
        {
            bool reSuccess = false;
            try
            {
                reSuccess = ShowResult(Display, autoSize);
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        /*==============================================================================================*/

        private bool SetROIStart(PictureBox display)
        {
            bool reSuccess = false;
            try
            {
                if (!isMouseEventPresence)
                {
                    display.MouseDown += PictureBox_MouseDown;
                    display.MouseMove += PictureBox_MouseMove;
                    display.MouseUp += PictureBox_MouseUp;
                    display.MouseWheel += PictureBox_MouseWheel;
                    Display.MouseHover += PictureBox_MouseHover;
                    isMouseEventPresence = true;
                }
                ShowROI(display);
                reSuccess = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        private bool SetROIEnd(PictureBox display)
        {
            bool reSuccess = false;
            try
            {
                if (isMouseEventPresence)
                {
                    display.MouseDown -= PictureBox_MouseDown;
                    display.MouseMove -= PictureBox_MouseMove;
                    display.MouseUp -= PictureBox_MouseUp;
                    display.MouseWheel -= PictureBox_MouseWheel;
                    Display.MouseHover -= PictureBox_MouseHover;
                    isMouseEventPresence = false;
                }
                ShowResult(display);
                reSuccess = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        public bool SetROI(float centerX, float centerY, float tolerance, float length, float angle = 0)
        {
            bool reSuccess = false;
            try
            {
                Base.SetCenterXY(centerX, centerY);
                Base.Tolerance = tolerance;
                Base.Length = length;
                Base.Angle = angle;
                reSuccess = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        private bool TrainLine(
    ETransitionType transitionType,
    int thickness,
    ETransitionChoice transitionChoice,
    int transitionIndex,
    int smoothing,
    int threshold,
    int minAmplitude,
    int minArea,
    bool HVConstraint,
    bool rectangularSamplingArea,
    float samplingStep,
    bool knownAngle,
    float filteringThreshold,
    int numFilteringPasses)
        {
            bool reSuccess = false;
            try
            {
                thickness = thickness < 1 ? 1 : thickness;

                threshold = threshold < 0 ? 0 : threshold;
                threshold = threshold > 255 ? 255 : threshold;

                minAmplitude = minAmplitude < 0 ? 0 : minAmplitude;
                minAmplitude = minAmplitude > 255 ? 255 : minAmplitude;

                minArea = minArea < 0 ? 0 : minArea;

                Base.TransitionType = transitionType;
                Base.Thickness = (int)thickness;
                Base.TransitionChoice = transitionChoice;
                Base.TransitionIndex = (int)transitionIndex;
                Base.Smoothing = (int)smoothing;
                Base.Threshold = (int)threshold;
                Base.MinAmplitude = (int)minAmplitude;
                Base.MinArea = (int)minArea;
                Base.HVConstraint = HVConstraint;
                Base.RectangularSamplingArea = rectangularSamplingArea;

                Base.SamplingStep = samplingStep;
                Base.KnownAngle = knownAngle;
                Base.FilteringThreshold = filteringThreshold;
                Base.NumFilteringPasses = (int)numFilteringPasses;

                reSuccess = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        private bool FindLine(EImageBW8 inputImage)
        {
            bool reSuccess = false;
            try
            {
                Base.Measure(inputImage);
                reSuccess = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        private bool GetSample(EImageBW8 inputImage, ref DataTable result)
        {
            bool reSuccess = false;
            try
            {
                result.Clear();
                EPoint point = new EPoint();
                for (int i = 0; i < Base.NumSamples; i++)
                {
                    Base.MeasureSample(inputImage, i);
                    if (Base.GetSample(point, i))
                    {
                        float centerX = Base.GetMeasuredPoint().X;
                        float centerY = Base.GetMeasuredPoint().Y;
                        int amplitude = Base.GetMeasuredPeak().Amplitude;
                        int area = Base.GetMeasuredPeak().Area;
                        result.Rows.Add(i + 1, centerX, centerY, amplitude, area);
                    }
                }
                reSuccess = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        private bool ShowImage(PictureBox display, EImageBW8 image, bool autoSize = false)
        {
            bool reSuccess = false;
            try
            {
                Bitmap bmp = new Bitmap(image.Width, image.Height);
                if (autoSize)
                {
                    float scalingRatio = ScalingRatio(image, display);
                    image.Draw(Graphics.FromImage(bmp), scalingRatio);
                }
                else
                    image.Draw(Graphics.FromImage(bmp), zoomX, zoomY, panX - mouseX, panY - mouseY);

                if (display.InvokeRequired)
                    display.Invoke(new Action(() => { display.BackgroundImage = bmp; }));
                else
                    display.BackgroundImage = bmp;
                reSuccess = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        private bool ShowROI(PictureBox display, bool autoSize = false)
        {
            bool reSuccess = false;
            try
            {
                if (autoSize)
                {
                    float scalingRatio = ScalingRatio(InputImage, display);
                    Base.SetZoom(scalingRatio, scalingRatio);
                    Base.SetPan(0, 0);
                }
                else
                {
                    Base.SetZoom(zoomX, zoomY);
                    Base.SetPan((panX - mouseX) * (1 / zoomX), (panY - mouseY) * (1 / zoomY));
                }

                Bitmap bmp = new Bitmap(display.Width, display.Height);
                Base.Draw(Graphics.FromImage(bmp), EDrawingMode.Nominal);
                Base.Draw(Graphics.FromImage(bmp), EDrawingMode.SampledPaths);

                if (display.InvokeRequired)
                    display.Invoke(new Action(() => { display.Image = bmp; }));
                else
                    display.Image = bmp;
                reSuccess = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        private bool ShowResult(PictureBox display, bool autoSize = false)
        {
            bool reSuccess = false;
            try
            {
                if (autoSize)
                {
                    float scalingRatio = ScalingRatio(InputImage, display);
                    Base.SetZoom(scalingRatio, scalingRatio);
                    Base.SetPan(0, 0);
                }
                else
                {
                    Base.SetZoom(zoomX, zoomY);
                    Base.SetPan((panX - mouseX) * (1 / zoomX), (panY - mouseY) * (1 / zoomY));
                }
                Bitmap bmp = new Bitmap(InputImage.Width, InputImage.Height);
                Base.Draw(Graphics.FromImage(bmp), EDrawingMode.SampledPoints);
                Base.Draw(Graphics.FromImage(bmp), EDrawingMode.Actual);

                if (display.InvokeRequired)
                    display.Invoke(new Action(() => { display.Image = bmp; }));
                else
                    display.Image = bmp;
                reSuccess = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return reSuccess;
        }

        private float ScalingRatio(EImageBW8 inputImage, PictureBox display)
        {
            float scalingRatio = 1;
            try
            {
                //計算Picturebox與顯示影像的比例，以便將影像縮放並且完整呈現到picturebox上。
                float PictureBoxSizeRatio = (float)display.Width / display.Height;
                float ImageSizeRatio = (float)inputImage.Width / inputImage.Height;
                if (ImageSizeRatio > PictureBoxSizeRatio)
                    scalingRatio = (float)display.Width / inputImage.Width;
                else
                    scalingRatio = (float)display.Height / inputImage.Height;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
            return scalingRatio;
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {                
                if (e.Button == MouseButtons.Right)
                {
                    recordX_2 = e.X - recordX_1;
                    recordY_2 = e.Y - recordY_1;
                }
                isDragging = false;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (isDragging)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        Base.Drag(e.X, e.Y);
                        ShowROI();
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        panX = Convert.ToSingle(e.X);
                        panY = Convert.ToSingle(e.Y);

                        ShowImage();
                        ShowROI();
                    }
                }
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {                
                if (e.Button == MouseButtons.Left)
                {
                    Base.SetCursor(e.X, e.Y);
                    Base.HitTest();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    recordX_1 = e.X;
                    recordY_1 = e.Y;
                    mouseX = e.X - recordX_2;
                    mouseY = e.Y - recordY_2;
                }
                isDragging = true;
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
        }

        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                zoomX += Convert.ToSingle(e.Delta) / 1200;
                zoomY += Convert.ToSingle(e.Delta) / 1200;
                ShowImage();
                ShowROI();
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
        }

        private void PictureBox_MouseHover(object sender, EventArgs e)
        {
            try
            {
                PictureBox pic = (PictureBox)sender;
                pic.Focus();
            }
            catch (EException exc)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, exc.Message, true, true);
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
        }
    }
}
