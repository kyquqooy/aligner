using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjAlarmMessageUndone
{
    public partial class frmMessageBox : Form
    {
        public frmMessageBox()
        {
            InitializeComponent();
            DateTime myDate = DateTime.Now;
            string str1 = myDate.ToString("yyyy-MM-dd");
            string str2 = myDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
            DateTime myDateOnly = myDate.Date;
            string str3 = myDateOnly.ToString("yyyy-MM-dd");
        }
    }
}
