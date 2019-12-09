using Euresys.Open_eVision_1_2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjVisionController.Open_eVision
{
    public class clsEasyFindPattern
    {
        private DataTable patternTable;
        private EImageBW8 inputImage;
        private EROIBW8 trainROI;
        private EImageBW8 trainImage;
        private EFoundPattern[] foundPattern;
        private EDragHandle dragHandle;

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

        public clsEasyFindPattern(PictureBox display) : this(display, new EPatternFinder())
        {
            Base.Interpolate = true;
            Base.MinScore = 0.8f;
            Base.AngleBias = 0;
            Base.AngleTolerance = 25;
            Base.ScaleBias = 100;
            Base.ScaleTolerance = 0f;            
        }

        public clsEasyFindPattern(PictureBox display, EPatternFinder ePatternFinder)
        {
            Display = display;
            Base = ePatternFinder;
            inputImage = new EImageBW8();
            trainROI = new EROIBW8();
            trainImage = new EImageBW8();

            dragHandle = EDragHandle.NoHandle;

            patternTable = new DataTable("PatternTable");
            patternTable.Columns.Add("Index");
            patternTable.Columns.Add("Score");
            patternTable.Columns.Add("Center_X");
            patternTable.Columns.Add("Center_Y");
            patternTable.Columns.Add("Angle (Deg)");
            patternTable.Columns.Add("Scale (%)");
        }

        public EPatternFinder Base { get; }

        public PictureBox Display { get; set; }

        public EImageBW8 InputImage { get { return inputImage; } set { inputImage = value; trainROI.Attach(value); } }

        public EImageBW8 TrainImage { get { return trainImage; } set { trainImage = value; } }

        public DataTable PatternTable { get { return patternTable; } set { patternTable = value; } }

        public int SearchField_MaxInstances { get { return (int)Base.MaxInstances; } }

        public float SearchField_MinScore { get { return Base.MinScore; } }

        public EFindContrastMode SearchField_ContrastMode { get { return Base.ContrastMode; } }

        public float Allowances_AngleBias { get { return Base.AngleBias; } }
    
        public float Allowances_AngleTolerance { get { return Base.AngleTolerance; } }

        public float Allowances_ScaleBias { get { return Base.ScaleBias; } }

        public float Allowances_ScaleTolerance { get { return Base.ScaleTolerance; } }

        public float Results_Score { get { return foundPattern.ElementAt(0).Score; } }

        public EPoint Results_Point { get { return foundPattern.ElementAt(0).Center; } }

        public float Results_CenterX { get { return foundPattern.ElementAt(0).Center.X; } }

        public float Results_CenterY { get { return foundPattern.ElementAt(0).Center.Y; } }

        public float Results_Angle { get { return foundPattern.ElementAt(0).Angle; } }

        public float Results_Scale { get { return foundPattern.ElementAt(0).Scale; } }

        public bool Save(string filePath)
        {
            bool reSuccess = false;
            try
            {
                if (Base.LearningDone)
                {
                    Base.Save(filePath, true);
                    trainROI.Save(filePath + "_ROI", EImageFileType.Euresys);
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

        public bool Load(string filePath)
        {
            bool reSuccess = false;
            try
            {
                if (File.Exists(filePath))
                {
                    Base.Load(filePath);
                    Base.CopyLearntPattern(trainImage);
                    trainROI.Load(filePath + "_ROI");
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
                trainROI.Attach(InputImage);
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
                reSuccess = SetROI(
                    Convert.ToInt32(InputImage.Width / 4),
                    Convert.ToInt32(InputImage.Height / 4),
                    Convert.ToInt32(InputImage.Width / 2),
                    Convert.ToInt32(InputImage.Height / 2));
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

        public bool TrainPattern(EPatternType patternType, float lightBalance = 0)
        {
            bool reSuccess = false;
            try
            {
                reSuccess = TrainPattern(trainROI, ref trainImage, patternType, lightBalance);
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

        public bool TrainPattern(float lightBalance = 0, bool autoTransitionThickness = true, int transitionThickness = 6)
        {
            bool reSuccess = false;
            try
            {
                reSuccess = TrainPattern(trainROI, ref trainImage, EPatternType.ContrastingRegions, lightBalance, autoTransitionThickness, transitionThickness);
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

        public bool FindPattern()
        {
            bool reSuccess = false;
            try
            {
                reSuccess = FindPattern(InputImage, ref foundPattern, Base.MaxInstances, Base.MinScore, Base.ContrastMode, Base.AngleBias, Base.AngleTolerance, Base.ScaleBias, Base.ScaleTolerance, Base.ScaleSearchExtent);
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

        public bool FindPattern(int maxInstances, float minScore, EFindContrastMode contrastMode, float angleTolerance = 25, float scaleTolerance = 0.25f)
        {
            bool reSuccess = false;
            try
            {
                reSuccess = FindPattern(InputImage, ref foundPattern, maxInstances, minScore, contrastMode, 0, angleTolerance, 1, scaleTolerance, 3);
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

        public bool GetPattern()
        {
            bool reSuccess = false;
            try
            {
                reSuccess = GetPattern(foundPattern, ref patternTable);
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

        public bool ShowTrainImage(PictureBox display)
        {
            bool reSuccess = false;
            try
            {
                reSuccess = ShowTrainImage(display, trainImage);
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
                reSuccess = ShowROI(Display, trainROI, true, autoSize);
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
                reSuccess = ShowResult(Display, foundPattern, autoSize);
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
                ShowROI(display,trainROI);

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
                ShowROI(display, trainROI, false);
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

        public bool SetROI(float centerX, float centerY, float width, float height)
        {
            bool reSuccess = false;
            try
            {                
                trainROI.SetPlacement(
                Convert.ToInt32(centerX),
                Convert.ToInt32(centerY),
                Convert.ToInt32(width),
                Convert.ToInt32(height));
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

        private bool TrainPattern(EROIBW8 trainROI, ref EImageBW8 trainImage, EPatternType patternType, float lightBalance = 0, bool autoTransitionThickness = true, int transitionThickness = 6)
        {
            bool reSuccess = false;
            try
            {
                lightBalance = lightBalance > 1 ? 1 : lightBalance;
                lightBalance = lightBalance < -1 ? -1 : lightBalance;

                Base.PatternType = patternType;
                Base.LightBalance = lightBalance;
                Base.AutoTransitionThickness = autoTransitionThickness;
                Base.TransitionThickness = 6;
                Base.Learn(trainROI);

                Base.CopyLearntPattern(trainImage);

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

        private bool FindPattern(EImageBW8 inputImage, ref EFoundPattern[] foundPattern, int maxInstances, float minScore, EFindContrastMode contrastMode, float angleBias = 0, float angleTolerance = 25, float scaleBias = 1, float scaleTolerance = 0.25f, int scaleSearchExtent = 3)
        {
            bool reSuccess = false;
            try
            {
                Base.MaxInstances = (int)maxInstances;
                Base.MinScore = minScore;
                Base.ContrastMode = contrastMode;
                Base.AngleBias = angleBias;//角度偏差
                Base.AngleTolerance = angleTolerance; //角度公差
                Base.ScaleBias = scaleBias;//縮放偏差
                Base.ScaleTolerance = scaleTolerance; //縮放公差
                Base.ScaleSearchExtent = scaleSearchExtent;

                reSuccess = FindPattern(inputImage, ref foundPattern);
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

        private bool FindPattern(EImageBW8 inputImage, ref EFoundPattern[] foundPattern)
        {
            bool reSuccess = false;
            try
            {
                foundPattern = Base.Find(inputImage);
                if (foundPattern != null)
                {
                    if (foundPattern.Length >= 1)
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

        private bool GetPattern(EFoundPattern[] foundPattern, ref DataTable foundTable)
        {
            bool reSuccess = false;
            try
            {
                foundTable.Clear();
                for (int i = 0; i < foundPattern.Length; i++)
                {
                    foundTable.Rows.Add(
                        i + 1,
                        foundPattern.ElementAt(i).Score,
                        foundPattern.ElementAt(i).Center.X,
                        foundPattern.ElementAt(i).Center.Y,
                        foundPattern.ElementAt(i).Angle,
                        foundPattern.ElementAt(i).Scale);
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

        private bool ShowTrainImage(PictureBox display, EImageBW8 trainImage)
        {
            bool reSuccess = false;
            try
            {
                float scalingRatio = ScalingRatio(trainImage, display);
                Bitmap bmp = new Bitmap(display.Width, display.Height);
                trainImage.Draw(Graphics.FromImage(bmp), scalingRatio);

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

        private bool ShowROI(PictureBox display, EROIBW8 roi, bool handles = true, bool autoSize = false)
        {
            bool reSuccess = false;
            try
            {
                Bitmap bmp = new Bitmap(display.Width, display.Height);
                if (autoSize)
                {
                    float scalingRatio = ScalingRatio(InputImage, display);
                    roi.DrawFrame(Graphics.FromImage(bmp), handles, scalingRatio);
                }
                else
                    roi.DrawFrame(Graphics.FromImage(bmp), handles, zoomX, zoomY, (panX - mouseX) * (1 / zoomX), (panY - mouseY) * (1 / zoomY));

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

        private bool ShowResult(PictureBox display, EFoundPattern[] findResult, bool autoSize = false)
        {
            bool reSuccess = false;
            try
            {
                EFoundPattern found;
                Bitmap bmp = new Bitmap(InputImage.Width, InputImage.Height);

                for (int i = 0; i < findResult.Length; i++)
                {
                    found = findResult.ElementAt(i); //把第i個取出來
                    found.DrawFeaturePoints = true; //把特徵點繪製出來，畫得很慢
                    if (autoSize)
                    {
                        float scalingRatio = ScalingRatio(InputImage, display);
                        found.Draw(Graphics.FromImage(bmp), scalingRatio);
                    }
                    else
                        found.Draw(Graphics.FromImage(bmp), zoomX, zoomY, (panX - mouseX) * (1 / zoomX), (panY - mouseY) * (1 / zoomY));
                }

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
                        trainROI.Drag(dragHandle, e.X, e.Y, zoomX, zoomY, (panX - mouseX) * (1 / zoomX), (panY - mouseY) * (1 / zoomY));
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
                    dragHandle = trainROI.HitTest(e.X, e.Y, zoomX, zoomY, (panX - mouseX) * (1 / zoomX), (panY - mouseY) * (1 / zoomY));
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
