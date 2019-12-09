using PylonC.NETSupportLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjVisionController.Open_eVision
{
    public partial class frmBaslerCamera : Form
    {
        private clsBaslerCameras baslerCameras;
        private DeviceEnumerator.Device[] deviceList;

        private long carry = 100000000;
        private long count_1 = 0;
        private long count_2 = 0;

        private int cameraIndex = 0;
        public frmBaslerCamera()
        {
            InitializeComponent();
            baslerCameras = new clsBaslerCameras();
            dataGridView1.DataSource = baslerCameras.DeviceTable;
        }

        private void frmBaslerCamera_Load(object sender, EventArgs e)
        {
            baslerCameras.Add(picDisplay);
            baslerCameras.Add(picDisplay2);
            baslerCameras.SearchDevice();

            sliderGain.MyImageProvider = baslerCameras[cameraIndex].Base;
            sliderExposureTime.MyImageProvider = baslerCameras[cameraIndex].Base;
            sliderHeight.MyImageProvider = baslerCameras[cameraIndex].Base;
            sliderWidth.MyImageProvider = baslerCameras[cameraIndex].Base;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            baslerCameras.SearchDevice();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            // timer1.Start();
            baslerCameras[cameraIndex].Open();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            // timer1.Stop();
            baslerCameras[cameraIndex].Close();
        }

        private void lvDeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            /////* Close the currently open image provider. */
            /////* Stops the grabbing of images. */
            //baslerCameras.Stop();
            ///* Close the image provider. */
            //baslerCameras.Close();

            ///* Open the selected image provider. */
            //if (deviceListView.SelectedItems.Count > 0)
            //{
            //    /* Get the first selected item. */
            //    ListViewItem item = deviceListView.SelectedItems[0];
            //    /* Get the attached device data. */
            //    DeviceEnumerator.Device device = item.Tag as DeviceEnumerator.Device;
            //    /* Open the image provider using the index from the device data. */
            //    baslerCameras.Open(device);
            //}            
        }

        private void btnOneShot_Click(object sender, EventArgs e)
        {
            baslerCameras[cameraIndex].OneShot();
            Image a = baslerCameras[cameraIndex].InputImage;
        }

        private void btnContinuousShot_Click(object sender, EventArgs e)
        {
            baslerCameras[cameraIndex].ContinuousShot();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            baslerCameras[cameraIndex].Stop();
        }

        private void frmBaslerCamera_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < baslerCameras.Count; i++)
            {
                /* Stops the grabbing of images. */
                baslerCameras[cameraIndex].Stop();
                /* Close the image provider. */
                baslerCameras[cameraIndex].Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            baslerCameras[cameraIndex].OneShot();
            count_1++;
            if (count_1 >= carry)
            {
                count_1 = 0;
                count_2++;
            }

            if (picDisplay2.InvokeRequired)
                picDisplay2.Invoke(new Action(() => { picDisplay2.BackgroundImage = baslerCameras[cameraIndex].InputImage; }));
            else
                picDisplay2.BackgroundImage = baslerCameras[cameraIndex].InputImage;
        }

        private void btnGetImage_Click(object sender, EventArgs e)
        {
            Image a = baslerCameras[cameraIndex].InputImage;
            if (picDisplay2.InvokeRequired)
                picDisplay2.Invoke(new Action(() => { picDisplay2.BackgroundImage = baslerCameras[cameraIndex].InputImage; }));
            else
                picDisplay2.BackgroundImage = baslerCameras[cameraIndex].InputImage;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            cameraIndex = Convert.ToInt32(numericUpDown1.Value);
            sliderGain.MyImageProvider = baslerCameras[cameraIndex].Base;
            sliderExposureTime.MyImageProvider = baslerCameras[cameraIndex].Base;
            sliderHeight.MyImageProvider = baslerCameras[cameraIndex].Base;
            sliderWidth.MyImageProvider = baslerCameras[cameraIndex].Base;
        }
    }
}
