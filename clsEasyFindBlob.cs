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
    public class clsEasyFindBlob
    {
        private DataTable blobTable;
        private EImageEncoder eImageEncoder;
        private EObjectSelection eObjectSelection;

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

        public clsEasyFindBlob(PictureBox display) : this(display, new ECodedImage2())
        {
            eObjectSelection.FeretAngle = 0.00f;
        }

        public clsEasyFindBlob(PictureBox display, ECodedImage2 eCodedImage2)
        {
            Display = display;
            Base = eCodedImage2;
            InputImage = new EImageBW8();
            eImageEncoder = new EImageEncoder();
            eObjectSelection = new EObjectSelection();

            blobTable = new DataTable("BlobTable");
            blobTable.Columns.Add("Index");
            blobTable.Columns.Add("Area");
            blobTable.Columns.Add("Center_X");
            blobTable.Columns.Add("Center_Y");

            Display.MouseDown += PictureBox_MouseDown;
            Display.MouseMove += PictureBox_MouseMove;
            Display.MouseUp += PictureBox_MouseUp;
            Display.MouseWheel += PictureBox_MouseWheel;
            Display.MouseHover += PictureBox_MouseHover;
        }

        public ECodedImage2 Base { get; private set; }

        public PictureBox Display { get; set; }

        public EImageBW8 InputImage { get; set; }

        public DataTable BlobTable { get { return blobTable; } }

        public bool Save(string filePath)
        {
            bool reSuccess = false;
            try
            {
                //Base.Save(filePath);
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
                    //Base.Load(filePath);
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

        public bool FindBlob(byte threshold = 128, bool findBlack = true, bool findWhite = false)
        {
            bool reSuccess = false;
            try
            {
                reSuccess = FindBlob(InputImage, threshold, findBlack, findWhite);
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

        public bool GetBlob()
        {
            bool reSuccess = false;
            try
            {
                reSuccess = GetBlob(InputImage, ref blobTable);
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

        public bool ShowImage()
        {
            bool reSuccess = false;
            try
            {
                reSuccess = ShowImage(Display, InputImage);
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

        public bool ShowResult()
        {
            bool reSuccess = false;
            try
            {
                reSuccess = ShowResult(Display);
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

        private bool FindBlob(EImageBW8 inputImage, byte threshold = 128, bool findBlack = true, bool findWhite = false)
        {
            bool reSuccess = false;
            try
            {
                eImageEncoder.GrayscaleSingleThresholdSegmenter.BlackLayerEncoded = findBlack;
                eImageEncoder.GrayscaleSingleThresholdSegmenter.WhiteLayerEncoded = findWhite;
                eImageEncoder.SegmentationMethod = ESegmentationMethod.GrayscaleSingleThreshold;
                eImageEncoder.GrayscaleSingleThresholdSegmenter.Mode = EGrayscaleSingleThreshold.Absolute;
                eImageEncoder.GrayscaleSingleThresholdSegmenter.AbsoluteThreshold = threshold;
                eImageEncoder.Encode(inputImage, Base);
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

        private bool GetBlob(EImageBW8 inputImage, ref DataTable blobTable)
        {
            bool reSuccess = false;
            try
            {
                eObjectSelection.Clear();
                eObjectSelection.AddObjects(Base);
                eObjectSelection.AttachedImage = inputImage;

                blobTable.Clear();

                ECodedElement blob;
                for (int i = 0; i < eObjectSelection.ElementCount; i++)
                {
                    blob = eObjectSelection.GetElement(i);
                    int area = blob.Area;
                    float centerX = blob.GravityCenterX;
                    float centerY = blob.GravityCenterY;

                    blobTable.Rows.Add(i + 1, area, centerX, centerY);
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

        private bool ShowImage(PictureBox display, EImageBW8 image)
        {
            bool reSuccess = false;
            try
            {
                Bitmap bmp = new Bitmap(image.Width, image.Height);
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

        private bool ShowResult(PictureBox display)
        {
            bool reSuccess = false;
            try
            {
                Bitmap bmp = new Bitmap(InputImage.Width, InputImage.Height);
                Base.Draw(Graphics.FromImage(bmp), eObjectSelection, zoomX, zoomY, (panX - mouseX) * (1 / zoomX), (panY - mouseY) * (1 / zoomY));

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

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {

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
