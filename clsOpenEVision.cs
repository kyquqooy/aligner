using Euresys.Open_eVision_1_2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace prjVisionController.Open_eVision
{
    /// <summary>
    /// ax+by+c = 0
    /// </summary>
    public struct ezLine
    {
        public double a { get; }
        public double b { get; }
        public double c { get; }
        public ezLine(double x, double y, double angle)
        {
            //y-y1 = m(x-x1)
            //y = mx+b
            double m = Math.Tan(angle / 180 * Math.PI);
            double b = y - (m * x);
            //ax+by+c = 0
            this.a = m;
            this.b = -1;
            this.c = b;
        }
        public ezLine(double x1, double y1, double x2, double y2)
        {
            //y-y1 = ((y2-y1)/(x2-x1))*(x-x1)
            //y = mx+b
            double m = (y2 - y1) / (x2 - x1);
            double b = ((x2 * y1) - (x1 * y2)) / (x2 - x1);
            //ax+by+c = 0
            this.a = m;
            this.b = -1;
            this.c = b;
        }
    }

    public struct ezAngle
    {
        public double angle { get; set; }
        public ezAngle(ezLine line_1, ezLine line_2)
        {
            double sqra1 = Math.Pow(line_1.a, 2);
            double sqrb1 = Math.Pow(line_1.b, 2);
            double sqra2 = Math.Pow(line_2.a, 2);
            double sqrb2 = Math.Pow(line_2.b, 2);

            angle = Math.Acos(
                (line_1.a * line_2.a + line_1.b * line_2.b) /
                (Math.Sqrt(sqra1 + sqrb1) *
                Math.Sqrt(sqra2 + sqrb2))) * 180 / Math.PI;
        }
    }

    public struct ezPoint
    {
        public double x { get; set; }
        public double y { get; set; }
        public ezPoint(ezLine line_1, ezLine line_2)
        {
            //解聯立方程式
            double a = line_1.a - line_2.a;
            double c = line_1.c - line_2.c;
            double cornerX = -c / a;
            double cornerY = (line_1.a * cornerX) + line_1.c;

            this.x = cornerX;
            this.y = cornerY;
        }
    }

    public enum CornerLine
    {
        Line_1 = 0,
        Line_2,
    }

    public class clsOpenEVision
    {
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private clsEasyCalibration eCalibration;
        private clsEasyFindPattern eCalibrFindPattern;
        private List<CalibrationPoint> calibrPoint;
        private clsEasyFindPattern eFindPattern;
        private Dictionary<CornerLine, clsEasyFindLine> eFindCorner;
        private Dictionary<CornerLine, ezLine> line;
        private clsEasyFixture eFixture;
        private Image inputImage;

        private readonly string visionPath = Application.StartupPath;
        private readonly string visionFile = "VisionFile";
        private string fileName;
        public clsOpenEVision(PictureBox picDisplay, string fileName)
        {
            this.fileName = fileName;
            Directory.CreateDirectory(visionPath + "\\" + visionFile);
            Directory.CreateDirectory(visionPath + "\\" + visionFile + "\\" + fileName);
            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();

            eCalibration = new clsEasyCalibration(picDisplay);
            eCalibrFindPattern = new clsEasyFindPattern(picDisplay);
            calibrPoint = new List<CalibrationPoint>();
            eFindPattern = new clsEasyFindPattern(picDisplay);

            eFindCorner = new Dictionary<CornerLine, clsEasyFindLine>();
            eFindCorner.Add(CornerLine.Line_1, new clsEasyFindLine(picDisplay));
            eFindCorner.Add(CornerLine.Line_2, new clsEasyFindLine(picDisplay));

            eFixture = new clsEasyFixture(picDisplay);
            eFindCorner[CornerLine.Line_1].Base.Attach(eFixture.Base);
            eFindCorner[CornerLine.Line_2].Base.Attach(eFixture.Base);

            line = new Dictionary<CornerLine, ezLine>();
            line.Add(CornerLine.Line_1, new ezLine(0, 0, 0));
            line.Add(CornerLine.Line_2, new ezLine(0, 0, 0));
        }

        public Image InputImage
        {
            get { return inputImage; }
            set
            {
                if (value != null)
                {
                    EImageBW8 eImageBW8 = ImageToEImageBW8(value);
                    eCalibration.InputImage = eImageBW8;
                    eCalibrFindPattern.InputImage = eImageBW8;
                    eFindPattern.InputImage = eImageBW8;
                    eFindCorner[CornerLine.Line_1].InputImage = eImageBW8;
                    eFindCorner[CornerLine.Line_2].InputImage = eImageBW8;

                    eCalibration.ShowImage(true);
                    eCalibrFindPattern.ShowImage(true);
                    eFindPattern.ShowImage(true);
                    eFindCorner[CornerLine.Line_1].ShowImage(true);
                    eFindCorner[CornerLine.Line_2].ShowImage(true);
                }
                inputImage = value;
            }
        }

        public clsEasyCalibration Calibration { get { return eCalibration; } }

        public clsEasyFindPattern FindPattern { get { return eCalibrFindPattern; } }

        public clsEasyFindPattern CalibrFindPattern { get { return eFindPattern; } }

        public clsEasyFixture Fixture { get { return eFixture; } }

        public Dictionary<CornerLine, clsEasyFindLine> FindCorner { get { return eFindCorner; } }

        public static EImageBW8 ImageToEImageBW8(Image image)
        {
            EImageBW8 eImageBW8 = null;
            try
            {
                eImageBW8 = new EImageBW8();
                Bitmap bitmap = (Bitmap)image;                
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                eImageBW8.SetImagePtr(bitmap.Width, bitmap.Height, bmpData.Scan0);
                bitmap.UnlockBits(bmpData);
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
            return eImageBW8;
        }

        public void Save()
        {
            try
            {
                eCalibration.Save(visionPath + "\\" + visionFile + "\\" + fileName + "\\Calibration");
                eCalibrFindPattern.Save(visionPath + "\\" + visionFile + "\\" + fileName + "\\CalibrationFindPattern");
                eFindPattern.Save(visionPath + "\\" + visionFile + "\\" + fileName + "\\FindPattern");
                eFindCorner[CornerLine.Line_1].Save(visionPath + "\\" + visionFile + "\\" + fileName + "\\FindLine_1");
                eFindCorner[CornerLine.Line_2].Save(visionPath + "\\" + visionFile + "\\" + fileName + "\\FindLine_2");
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

        public void Load()
        {
            try
            {
                eCalibration.Load(visionPath + "\\" + visionFile + "\\" + fileName + "\\Calibration");
                eCalibrFindPattern.Load(visionPath + "\\" + visionFile + "\\" + fileName + "\\CalibrationFindPattern");
                eFindPattern.Load(visionPath + "\\" + visionFile + "\\" + fileName + "\\FindPattern");
                eFindCorner[CornerLine.Line_1].Load(visionPath + "\\" + visionFile + "\\" + fileName + "\\FindLine_1");
                eFindCorner[CornerLine.Line_2].Load(visionPath + "\\" + visionFile + "\\" + fileName + "\\FindLine_2");
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

        public void GetPatternWordResults(ref EPoint wordPoint)
        {
            try
            {
                if (eFindPattern.FindPattern())
                {
                    wordPoint = eCalibration.Fixture.GetPoint(new EPoint(eFindPattern.Results_CenterX, eFindPattern.Results_CenterY), CoordinateHook.Hook);
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

        public void GetPatternWordResults(clsEasyFindPattern findPattern, ref EPoint wordPoint)
        {
            try
            {
                if (findPattern.FindPattern())
                {
                    wordPoint = eCalibration.Fixture.GetPoint(new EPoint(findPattern.Results_CenterX, findPattern.Results_CenterY), CoordinateHook.Hook);
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

        public void GetLineWordResults(CornerLine cornerLine, ref ezLine line)
        {
            try
            {
                float result_CenterX = 0;
                float result_CenterY = 0;
                float result_Angle = 0;
                FindCorner[cornerLine].FindLine(ref result_CenterX, ref result_CenterY, ref result_Angle);

                EPoint localSensor = eFixture.GetPoint(new EPoint(result_CenterX, result_CenterY), CoordinateHook.NotHook);
                EPoint world = eCalibration.Fixture.GetPoint(new EPoint(localSensor.X, localSensor.Y), CoordinateHook.Hook);

                line = new ezLine(world.X, world.Y, result_Angle + eFixture.Angle);
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

        public void GetCornerResults(ref ezPoint corner)
        {
            try
            {
                float result_CenterX_1 = 0;
                float result_CenterY_1 = 0;
                float result_Angle_1 = 0;

                float result_CenterX_2 = 0;
                float result_CenterY_2 = 0;
                float result_Angle_2 = 0;
                FindCorner[CornerLine.Line_1].FindLine(ref result_CenterX_1, ref result_CenterY_1, ref result_Angle_1);
                FindCorner[CornerLine.Line_2].FindLine(ref result_CenterX_2, ref result_CenterY_2, ref result_Angle_2);

                EPoint localSensor_1 = eFixture.GetPoint(new EPoint(result_CenterX_1, result_CenterY_1), CoordinateHook.NotHook);
                EPoint localSensor_2 = eFixture.GetPoint(new EPoint(result_CenterX_2, result_CenterY_2), CoordinateHook.NotHook);
                
                ezLine line_1 = new ezLine(localSensor_1.X, localSensor_1.Y, result_Angle_1 + eFixture.Angle);
                ezLine line_2 = new ezLine(localSensor_2.X, localSensor_2.Y, result_Angle_2 + eFixture.Angle);

                line[CornerLine.Line_1] = line_1;
                line[CornerLine.Line_2] = line_2;
                corner = new ezPoint(line_1, line_2);
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

        public void GetCornerWordResults(ref ezPoint corner)
        {
            try
            {
                //ezLine line_1 = new ezLine(0, 0, 0);
                //ezLine line_2 = new ezLine(0, 0, 0);
                //GetLineWordResults(CornerLine.Line_1, ref line_1);
                //GetLineWordResults(CornerLine.Line_2, ref line_2);
                //line[CornerLine.Line_1] = line_1;
                //line[CornerLine.Line_2] = line_2;
                //corner = new ezPoint(line_1, line_2);
                ezPoint pixelCorner = new ezPoint();
                GetCornerResults(ref pixelCorner);
                EPoint wordPoint = eCalibration.Fixture.GetPoint(new EPoint((float)pixelCorner.x, (float)pixelCorner.y), CoordinateHook.Hook);
                corner = new ezPoint();
                corner.x = wordPoint.X;
                corner.y = wordPoint.Y;
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

        public bool ShowCornerResult(PictureBox display)
        {
            bool reSuccess = false;
            try
            {
                float scalingRatio = ScalingRatio(eFindCorner[CornerLine.Line_1].InputImage, display);
                eFindCorner[CornerLine.Line_1].Base.SetZoom(scalingRatio, scalingRatio);
                eFindCorner[CornerLine.Line_1].Base.SetPan(0, 0);
                eFindCorner[CornerLine.Line_2].Base.SetZoom(scalingRatio, scalingRatio);
                eFindCorner[CornerLine.Line_2].Base.SetPan(0, 0);

                Bitmap bmp = new Bitmap(InputImage.Width, InputImage.Height);
                eFindCorner[CornerLine.Line_1].Base.Draw(Graphics.FromImage(bmp), EDrawingMode.SampledPoints);
                eFindCorner[CornerLine.Line_1].Base.Draw(Graphics.FromImage(bmp), EDrawingMode.Actual);
                eFindCorner[CornerLine.Line_2].Base.Draw(Graphics.FromImage(bmp), EDrawingMode.SampledPoints);
                eFindCorner[CornerLine.Line_2].Base.Draw(Graphics.FromImage(bmp), EDrawingMode.Actual);

                ezPoint ezPoint = new ezPoint();
                GetCornerResults(ref ezPoint);
                EPoint ePoint = new EPoint((float)ezPoint.x, (float)ezPoint.y);
                ELineGauge line_1 = new ELineGauge();
                ELineGauge line_2 = new ELineGauge();
                //line_1.Attach(eFixture.Base);
                //line_2.Attach(eFixture.Base);

                line_1.SetCenterXY(ePoint.X, ePoint.Y);
                line_2.SetCenterXY(ePoint.X, ePoint.Y);
                line_1.Angle = eFindCorner[CornerLine.Line_1].Position_Angle + eFixture.Angle;
                line_2.Angle = eFindCorner[CornerLine.Line_2].Position_Angle + eFixture.Angle;
                
                line_1.SetZoom(scalingRatio, scalingRatio);
                line_2.SetZoom(scalingRatio, scalingRatio);
                line_1.SetPan(0, 0);
                line_2.SetPan(0, 0);
                line_1.Draw(Graphics.FromImage(bmp), new ERGBColor(255, 0, 0), EDrawingMode.Actual);
                line_2.Draw(Graphics.FromImage(bmp), new ERGBColor(255, 0, 0), EDrawingMode.Actual);               

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
    }
}