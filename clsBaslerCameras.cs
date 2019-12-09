using PylonC.NET;
using PylonC.NETSupportLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjVisionController.Open_eVision
{
    public class clsBaslerCameras
    {
        private DataTable deviceTable;
        private List<clsBaslerCamera> cameraList;
        public clsBaslerCameras()
        {
            //this.form = form;
            //this.display = display;
            cameraList = new List<clsBaslerCamera>();
            deviceTable = new DataTable("DeviceTable");
            deviceTable.Columns.Add("DeviceIndex");
            deviceTable.Columns.Add("SearchIndex");
            deviceTable.Columns.Add("FullName");
            deviceTable.Columns.Add("Name");
            deviceTable.Columns.Add("Tooltip");
        }

        public clsBaslerCamera this[int cameraIndex]
        {
            get { return cameraList[cameraIndex]; }
        }

        public DataTable DeviceTable { get { return deviceTable; } }

        public int Count { get { return cameraList.Count; } }

        public void Add(PictureBox display)
        {
            try
            {
                cameraList.Add(new clsBaslerCamera(display));
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
        }

        public void SearchDevice()
        {
            try
            {
                foreach (clsBaslerCamera item in cameraList)
                {
                    item.Stop();
                    item.Close();
                }

                DeviceEnumerator.Device[] devices = null;
                SearchDevice(ref devices);

                for (int i = 0; i < Count; i++)
                {
                    if (devices.Length > i)
                        cameraList[i].Device = devices[i];
                }
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
        }

        public void SearchDevice(ref DeviceEnumerator.Device[] devices)
        {
            try
            {
                deviceTable.Clear();
                List<DeviceEnumerator.Device> deviceList = new List<DeviceEnumerator.Device>();
                deviceList = DeviceEnumerator.EnumerateDevices();
                deviceList.Sort((x, y) => { return x.Name.CompareTo(y.Name); });
                foreach (DeviceEnumerator.Device item in deviceList)
                {
                    deviceTable.Rows.Add(
                        deviceTable.Rows.Count + 1,
                        item.Index,
                        item.FullName,
                        item.Name,
                        item.Tooltip);
                }
                devices = deviceList.ToArray();
            }
            catch (Exception ex)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, ex.Message, true, true);
            }
        }

        public class clsBaslerCamera
        {
            private Bitmap m_bitmap = null; /* The bitmap is used for displaying the image. */
            private bool shotEnable = false;
            private bool stopEnable = false;
            private bool searchDeviceEnable = true;

            public event EventHandler InputImageUpData;

            public clsBaslerCamera(PictureBox display)
            {
                Display = display;
                Base = new ImageProvider();
                Base.GrabErrorEvent += new ImageProvider.GrabErrorEventHandler(OnGrabErrorEventCallback);
                Base.DeviceRemovedEvent += new ImageProvider.DeviceRemovedEventHandler(OnDeviceRemovedEventCallback);
                Base.DeviceOpenedEvent += new ImageProvider.DeviceOpenedEventHandler(OnDeviceOpenedEventCallback);
                Base.DeviceClosedEvent += new ImageProvider.DeviceClosedEventHandler(OnDeviceClosedEventCallback);
                Base.GrabbingStartedEvent += new ImageProvider.GrabbingStartedEventHandler(OnGrabbingStartedEventCallback);
                Base.ImageReadyEvent += new ImageProvider.ImageReadyEventHandler(OnImageReadyEventCallback);
                Base.GrabbingStoppedEvent += new ImageProvider.GrabbingStoppedEventHandler(OnGrabbingStoppedEventCallback);
            }

            ~clsBaslerCamera()
            {
                this.Close();
            }

            public ImageProvider Base { get; set; }

            public PictureBox Display { get; set; }

            public Image InputImage { get; private set; }

            public bool IsOpen { get { return Base.IsOpen; } } 

            public DeviceEnumerator.Device Device { get; set; }            

            public void Open()
            {
                try
                {
                    /* Open the image provider using the index from the device data. */
                    if (!IsOpen)
                        Base.Open(Device.Index);
                }
                catch (Exception ex)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, ex.Message + GetBaslerException(), true, true);
                }
            }

            /* Closes the image provider and handles exceptions. */
            public void Close()
            {
                /* Close the image provider. */
                try
                {
                    if (IsOpen)
                    {
                        Stop();
                        Base.Close();
                    }
                }
                catch (Exception ex)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, ex.Message + GetBaslerException(), true, true);
                }
            }

            public bool OneShot()
            {
                bool reSuccess = false;
                try
                {
                    if (shotEnable)
                    {
                        Base.OneShot(); /* Starts the grabbing of one image. */
                        reSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, ex.Message + GetBaslerException(), true, true);
                }
                return reSuccess;
            }

            public bool ContinuousShot()
            {
                bool reSuccess = false;
                try
                {
                    if (shotEnable)
                    {
                        Base.ContinuousShot(); /* Start the grabbing of images until grabbing is stopped. */
                        reSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, ex.Message + GetBaslerException(), true, true);
                }
                return reSuccess;
            }

            public bool Stop()
            {
                /* Stop the grabbing. */
                bool reSuccess = false;
                try
                {
                    if (stopEnable)
                    {
                        Base.Stop();
                        reSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, ex.Message + GetBaslerException(), true, true);
                }
                return reSuccess;
            }

            private bool GetInputImage(ImageProvider.Image image)
            {
                bool reSuccess = false;
                try
                {
                    if (image != null)
                    {
                        Bitmap bImage = null;
                        BitmapFactory.CreateBitmap(out bImage, image.Width, image.Height, image.Color);
                        BitmapFactory.UpdateBitmap(bImage, image.Buffer, image.Width, image.Height, image.Color);
                        InputImage = bImage;
                        reSuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, ex.Message + GetBaslerException(), true, true);
                }
                return reSuccess;
            }

            /* Handles the event related to the occurrence of an error while grabbing proceeds. */
            private void OnGrabErrorEventCallback(Exception grabException, string additionalErrorMessage)
            {
                if (Display.FindForm().InvokeRequired)
                {
                    /* If called from a different thread, we must use the Invoke method to marshal the call to the proper thread. */
                    Display.FindForm().BeginInvoke(new ImageProvider.GrabErrorEventHandler(OnGrabErrorEventCallback), grabException, additionalErrorMessage);
                    return;
                }
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                clsLogFile.LogTryCatch(stackFrames, grabException.Message + additionalErrorMessage, true, true);
            }

            /* Handles the event related to the removal of a currently open device. */
            private void OnDeviceRemovedEventCallback()
            {
                if (Display.FindForm().InvokeRequired)
                {
                    /* If called from a different thread, we must use the Invoke method to marshal the call to the proper thread. */
                    Display.FindForm().BeginInvoke(new ImageProvider.DeviceRemovedEventHandler(OnDeviceRemovedEventCallback));
                    return;
                }
                /* Disable the buttons. */
                shotEnable = false;
                stopEnable = false;
                /* Stops the grabbing of images. */
                Stop();
                ///* Close the image provider. */
                Close();
            }

            /* Handles the event related to a device being open. */
            private void OnDeviceOpenedEventCallback()
            {
                if (Display.FindForm().InvokeRequired)
                {
                    /* If called from a different thread, we must use the Invoke method to marshal the call to the proper thread. */
                    Display.FindForm().BeginInvoke(new ImageProvider.DeviceOpenedEventHandler(OnDeviceOpenedEventCallback));
                    return;
                }
                /* The image provider is ready to grab. Enable the grab buttons. */
                shotEnable = true;
                stopEnable = false;
            }

            /* Handles the event related to a device being closed. */
            private void OnDeviceClosedEventCallback()
            {
                if (Display.FindForm().InvokeRequired)
                {
                    /* If called from a different thread, we must use the Invoke method to marshal the call to the proper thread. */
                    Display.FindForm().BeginInvoke(new ImageProvider.DeviceClosedEventHandler(OnDeviceClosedEventCallback));
                    return;
                }
                /* The image provider is closed. Disable all buttons. */
                shotEnable = false;
                stopEnable = false;
            }

            /* Handles the event related to the image provider executing grabbing. */
            private void OnGrabbingStartedEventCallback()
            {
                if (Display.FindForm().InvokeRequired)
                {
                    /* If called from a different thread, we must use the Invoke method to marshal the call to the proper thread. */
                    Display.FindForm().BeginInvoke(new ImageProvider.GrabbingStartedEventHandler(OnGrabbingStartedEventCallback));
                    return;
                }
                /* Do not update device list while grabbing to avoid jitter because the GUI-Thread is blocked for a short time when enumerating. */
                searchDeviceEnable = false;

                /* The image provider is grabbing. Disable the grab buttons. Enable the stop button. */
                shotEnable = false;
                stopEnable = true;
            }

            /* Handles the event related to an image having been taken and waiting for processing. */
            private void OnImageReadyEventCallback()
            {
                if (Display.FindForm().InvokeRequired)
                {
                    /* If called from a different thread, we must use the Invoke method to marshal the call to the proper thread. */
                    Display.FindForm().BeginInvoke(new ImageProvider.ImageReadyEventHandler(OnImageReadyEventCallback));
                    return;
                }
                try
                {
                    /* Acquire the image from the image provider. Only show the latest image. The camera may acquire images faster than images can be displayed*/
                    ImageProvider.Image image = Base.GetLatestImage();

                    /* Check if the image has been removed in the meantime. */
                    if (image != null)
                    {
                        /* Check if the image is compatible with the currently used bitmap. */
                        if (BitmapFactory.IsCompatible(m_bitmap, image.Width, image.Height, image.Color))
                        {
                            /* Update the bitmap with the image data. */
                            BitmapFactory.UpdateBitmap(m_bitmap, image.Buffer, image.Width, image.Height, image.Color);

                            /* To show the new image, request the display control to update itself. */
                            if (Display.InvokeRequired)
                                Display.Invoke(new Action(() => { Display.Refresh(); }));
                            else
                                Display.Refresh();
                        }
                        else /* A new bitmap is required. */
                        {
                            BitmapFactory.CreateBitmap(out m_bitmap, image.Width, image.Height, image.Color);
                            BitmapFactory.UpdateBitmap(m_bitmap, image.Buffer, image.Width, image.Height, image.Color);

                            Bitmap bitmap;
                            if (Display.InvokeRequired)
                                Display.Invoke(new Action(() =>
                                {
                                /* We have to dispose the bitmap after assigning the new one to the display control. */
                                    bitmap = Display.BackgroundImage as Bitmap;
                                /* Provide the display control with the new bitmap. This action automatically updates the display. */
                                    Display.BackgroundImage = m_bitmap;
                                    if (bitmap != null)
                                    {
                                    /* Dispose the bitmap. */
                                        bitmap.Dispose();
                                    }
                                }));
                            else
                            {
                                /* We have to dispose the bitmap after assigning the new one to the display control. */
                                bitmap = Display.BackgroundImage as Bitmap;
                                /* Provide the display control with the new bitmap. This action automatically updates the display. */
                                Display.BackgroundImage = m_bitmap;
                                if (bitmap != null)
                                {
                                    /* Dispose the bitmap. */
                                    bitmap.Dispose();
                                }
                            }
                        }
                        /* The processing of the image is done. Release the image buffer. */
                        Base.ReleaseImage();
                        /* The buffer can be used for the next image grabs. */
                    }
                    GetInputImage(image);
                    InputImageUpData?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, ex.Message + GetBaslerException(), true, true);
                }
            }

            /* Handles the event related to the image provider having stopped grabbing. */
            private void OnGrabbingStoppedEventCallback()
            {
                if (Display.FindForm().InvokeRequired)
                {
                    /* If called from a different thread, we must use the Invoke method to marshal the call to the proper thread. */
                    Display.FindForm().BeginInvoke(new ImageProvider.GrabbingStoppedEventHandler(OnGrabbingStoppedEventCallback));
                    return;
                }
                /* Enable device list update again */
                searchDeviceEnable = true;

                /* The image provider stopped grabbing. Enable the grab buttons. Disable the stop button. */
                shotEnable = Base.IsOpen;
                stopEnable = false;
            }

            private string GetBaslerException()
            {
                string getBaslerError = Base.GetLastErrorMessage();
                string baslerError = getBaslerError.Length > 0 ? "\n\nLast error message (may not belong to the exception):\n" + getBaslerError : string.Empty;
                return baslerError;
            }
        }
    }
}
