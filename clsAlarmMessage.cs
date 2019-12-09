using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjAlarmMessageUndone
{
    public class clsAlarmMessage
    {
        public static frmMessageBox errForm = new frmMessageBox();
        public static ListBox lstAlarm { get; set; } = null;
        protected internal static string AlarmFile { get; set; } = "AlarmHistory";

        //public static bool Alarm_SW;
        //public static uint Alarm_ButtonSelection;
        //public static string strAlarm_Context;
        //public static bool Alarm_RetryOnly_f;
        //public static bool Alarm_IgnoreDoing_f;
        //public static ushort Alarm_Station;
        //public static bool Alarm_BuzzerOff_f;
        //public static bool Alarm_ExitProgram_f;

        private static object Lock_AlarmHistoryRecord = new object();
        /// <summary>
        /// Alarm記錄
        /// </summary>
        /// <param name="strContext">Alarm內容</param>
        public static void AlarmHistoryRecord(string strContext)
        {
            lock (Lock_AlarmHistoryRecord)
            {
                try
                {
                    //取得目前時間
                    DateTime recordTime = DateTime.Now;

                    //建立儲存資訊
                    strContext = "V" + clsLogFile.ProgramVersion() + ",\t" + recordTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + ",\tAlarm,\t" + strContext.Replace("\r", " ").Replace("\n", " ") + "\r\n";

                    //文件記錄
                    clsLogFile.FileRecord(AlarmFile, recordTime, strContext);

                    if (lstAlarm != null) //有listbox時才更新listbox
                    {
                        strContext = strContext.Replace("\t", " ");

                        if (lstAlarm.InvokeRequired == true) //當跨執行緒存取UI時, 須使用委派
                        {
                            lstAlarm.Invoke(new MethodInvoker(delegate
                            {
                                lstAlarm.Items.Add(strContext);
                                lstAlarm.SelectedIndex = lstAlarm.Items.Count - 1;
                            }));
                        }
                        else
                        {
                            lstAlarm.Items.Add(strContext);
                            lstAlarm.SelectedIndex = lstAlarm.Items.Count - 1;
                        }
                    }
                }
                catch (Exception e)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, e.Message, true, false);//當紀錄有問題只show message box(不做紀錄)
                }
            }
        }

        private static object Lock_AlarmHistoryCheck = new object();
        /// <summary>
        /// 移除過舊Alarm檔案
        /// </summary>
        /// <param name="OverDay">指定移除的時間</param>
        public static void AlarmHistoryCheck(ushort OverDay = 30)
        {
            lock (Lock_AlarmHistoryCheck)
            {
                try
                {
                    clsLogFile.LogFileCheck(AlarmFile, OverDay);
                }
                catch (Exception e)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, e.Message, true, true);
                }
            }
        }

       /* private static object Lock_ShowAlarm = new object();
        public static void ShowAlarm(string strErrorMessage, ushort nStation, bool btnRetryOnly = false)
        {
            lock (Lock_ShowAlarm)
            {
                try
                {
                    Alarm_SW = true;
                    Alarm_ButtonSelection = 0;
                    Alarm_RetryOnly_f = btnRetryOnly;
                    Alarm_IgnoreDoing_f = false;
                    Alarm_Station = nStation;
                    strAlarm_Context = strErrorMessage;
                    AlarmHistoryRecord(strErrorMessage);

                    //==== Show the Error Message Box
                    if (errForm.InvokeRequired == true) //不同執行緒存取UI時, 須使用委派
                    {
                        errForm.Invoke(new MethodInvoker(delegate
                        {
                            errForm.ControlBox = false;
                            errForm.Show();
                        }));
                    }
                    else
                    {
                        errForm.ControlBox = false;
                        errForm.Show();
                    }
                }
                catch (Exception e)//ObjectDisposedException : Do nothing
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, e.Message, true, true);
                }
            }
        }*/
    }
}
