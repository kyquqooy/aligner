using Euresys.Open_eVision_1_2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjVisionController.Open_eVision
{
    public enum CoordinateHook
    {
        Hook = 0,
        NotHook
    }

    public class clsEasyFixture
    {
        public clsEasyFixture(PictureBox display) : this(display, new EFrameShape())
        {

        }

        public clsEasyFixture(PictureBox display, EFrameShape eFrameShape)
        {
            Base = eFrameShape;
        }

        public EFrameShape Base { get; set; }

        public float OriginX { get { return Base.CenterX; } }

        public float OriginY { get { return Base.CenterY; } }

        public float Angle { get { return Base.Angle; } }

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

        public bool Hook(EShape eShape, float originX = 0, float originY = 0, float angle = 0)
        {
            bool reSuccess = false;
            try
            {
                Base.Attach(eShape);
                reSuccess = SetOrigin(originX, originY, angle);
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

        public bool Clear()
        {
            bool reSuccess = false;
            try
            {
                Base.Detach();
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

        public EPoint GetPoint(EPoint inputPoint, CoordinateHook coordinateHook = CoordinateHook.Hook)
        {
            EPoint outputPoint = new EPoint();
            try
            {
                switch (coordinateHook)
                {
                    case CoordinateHook.Hook:
                        outputPoint = Base.SensorToLocal(inputPoint);
                        break;
                    case CoordinateHook.NotHook:
                        outputPoint = Base.LocalToSensor(inputPoint);
                        break;
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
            return outputPoint;
        }

        public bool SetOrigin(float originX = 0, float originY = 0, float angle = 0)
        {
            bool reSuccess = false;
            try
            {
                Base.SetCenterXY(-originX, -originY);
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
    }
}
