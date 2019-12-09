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
    public struct CalibrationPoint
    {
        public float Pixel_X { get; set; }
        public float Pixel_Y { get; set; }
        public float World_X { get; set; }
        public float World_Y { get; set; }

        public CalibrationPoint(float pixel_X, float pixel_Y, float world_X, float world_Y)
        {
            Pixel_X = pixel_X;
            Pixel_Y = pixel_Y;
            World_X = world_X;
            World_Y = world_Y;
        }

        public CalibrationPoint(double pixel_X, double pixel_Y, double world_X, double world_Y)
        {
            Pixel_X = Convert.ToSingle(pixel_X);
            Pixel_Y = Convert.ToSingle(pixel_Y);
            World_X = Convert.ToSingle(world_X);
            World_Y = Convert.ToSingle(world_Y);
        }
    }

    public class clsEasyCalibration
    {
        private DataTable calibrTable;
        private EFrameShape eFrameShape;

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

        public clsEasyCalibration(PictureBox display)
            : this(display, new clsEasyCalibration(display, new EWorldShape(), new clsEasyFixture(display)))
        {
            Base.Dragable = true;
            Base.Resizable = true;
            Base.Rotatable = true;
        }

        public clsEasyCalibration(PictureBox display, clsEasyCalibration eCalibration) 
            : this(display, eCalibration.Base, eCalibration.Fixture)
        {
        }

        public clsEasyCalibration(PictureBox display, EWorldShape eWorldShape, clsEasyFixture eFixture)
        {
            Display = display;
            Base = eWorldShape;
            Fixture = eFixture;
            Fixture.Hook(Base, 0, 0, 0);
            InputImage = new EImageBW8();

            calibrTable = new DataTable("CalibrTable");
            calibrTable.Columns.Add("Index");
            calibrTable.Columns.Add("Pixel_X");
            calibrTable.Columns.Add("Pixel_Y");
            calibrTable.Columns.Add("World_X");
            calibrTable.Columns.Add("World_Y");

            //Display.MouseDown += PictureBox_MouseDown;
            //Display.MouseMove += PictureBox_MouseMove;
            //Display.MouseUp += PictureBox_MouseUp;
            //Display.MouseWheel += PictureBox_MouseWheel;
            //Display.MouseHover += PictureBox_MouseHover;
        }

        public EWorldShape Base { get; }

        public EImageBW8 InputImage { get; set; }

        public PictureBox Display { get; set; }

        public DataTable CalibrTable { get { return calibrTable; } }

        public clsEasyFixture Fixture { get; }

        public bool Save(string filePath)
        {
            bool reSuccess = false;
            try
            {
                Base.Save(filePath);
                Fixture.Save(filePath + "_Fixture");
                calibrTable.WriteXmlSchema(filePath + "_SchemaPoint.xml");
                calibrTable.WriteXml(filePath + "_Point.xml");
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
                    Fixture.Load(filePath + "_Fixture");
                    calibrTable.Clear();
                    calibrTable.ReadXmlSchema(filePath + "_SchemaPoint.xml");
                    calibrTable.ReadXml(filePath + "_Point.xml");
                    if (calibrTable.Rows.Count >= 4)
                    {
                        Calibration();
                        reSuccess = true;
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
            return reSuccess;
        }

        public bool LoadImage(string filePath)
        {
            bool reSuccess = false;
            try
            {
                InputImage.Load(filePath);
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

        public bool SetCalibrPoint(CalibrationPoint[] calibrPoint)
        {
            bool reSuccess = false;
            try
            { 
                calibrTable.Clear();

                for (int i = 0; i < calibrPoint.Length; i++)
                {
                    float pixel_X = calibrPoint[i].Pixel_X;
                    float pixel_Y = calibrPoint[i].Pixel_Y;
                    float world_X = calibrPoint[i].World_X;
                    float world_Y = calibrPoint[i].World_Y;

                    calibrTable.Rows.Add(calibrTable.Rows.Count + 1, pixel_X, pixel_Y, world_X, world_Y);
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

        public bool Calibration(ECalibrationMode CalibrMode = ECalibrationMode.Quadratic)
        {
            bool reSuccess = false;
            try
            {
                Base.EmptyLandmarks();
                for (int i = 0; i < calibrTable.Rows.Count; i++)
                {
                    float pixel_X = Convert.ToSingle(calibrTable.Rows[i][1]);
                    float pixel_Y = Convert.ToSingle(calibrTable.Rows[i][2]);
                    float world_X = Convert.ToSingle(calibrTable.Rows[i][3]);
                    float world_Y = Convert.ToSingle(calibrTable.Rows[i][4]);

                    Base.AddLandmark(new EPoint(pixel_X, pixel_Y), new EPoint(world_X, world_Y));
                }
                Base.Calibrate((int)CalibrMode);
                Base.AutoCalibrateLandmarks(true);                
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

        public bool ShowImage(bool autoSize = true)
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

        public bool ShowResult(bool autoSize = true)
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
        private object ShowImage_Lock = new object();

        private bool ShowImage(PictureBox display, EImageBW8 image, bool autoSize = true)
        {
            lock (ShowImage_Lock)
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
        }

        private object ShowResult_Lock = new object();

        private bool ShowResult(PictureBox display, bool autoSize = true)
        {
            lock (ShowResult_Lock)
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
                    Base.DrawLandmarks(Graphics.FromImage(bmp));

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

        private object PictureBox_MouseUp_Lock = new object();

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            lock (PictureBox_MouseUp_Lock)
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
        }

        private object PictureBox_MouseMove_Lock = new object();

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            lock (PictureBox_MouseMove_Lock)
            {
                try
                {
                    if (isDragging)
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            //Base.DragLandmark(e.X, e.Y);
                            //ShowResult();
                        }
                        else if (e.Button == MouseButtons.Right)
                        {
                            panX = Convert.ToSingle(e.X);
                            panY = Convert.ToSingle(e.Y);

                            ShowImage();
                            ShowResult();
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
        }

        private object PictureBox_MouseDown_Lock = new object();

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            lock (PictureBox_MouseDown_Lock)
            {
                try
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        //Base.SetCursor(e.X, e.Y);
                        //Base.HitLandmarks();
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
        }

        private object PictureBox_MouseWheel_Lock = new object();

        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            lock (PictureBox_MouseWheel_Lock)
            {
                try
                {
                    zoomX += Convert.ToSingle(e.Delta) / 1200;
                    zoomY += Convert.ToSingle(e.Delta) / 1200;
                    ShowImage();
                    ShowResult();
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
