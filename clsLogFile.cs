using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prjAlarmMessageUndone
{
    public class clsLogFile
    {
        /// <summary>
        /// 紀錄儲存路徑
        /// </summary>
        public static string FilePath { get; set; } = System.Windows.Forms.Application.StartupPath;
        
        /// <summary>
        /// 取得軟體版本
        /// </summary>
        /// <returns></returns>
        public static string ProgramVersion()
        {
            string strMainProgramFileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            return FileVersionInfo.GetVersionInfo(strMainProgramFileName).FileVersion.ToString();
        }

        private static object Lock_LogTryCatch = new object();
        /// <summary>
        /// TryCatch報警
        /// </summary>
        /// <param name="stackFrames">堆疊物件</param>
        /// <param name="strErrMsg">錯誤資訊</param>
        /// <param name="bShowMsgBox">是否顯示錯誤視窗</param>
        /// <param name="bRecord">是否紀錄錯誤訊息</param>
        public static void LogTryCatch(StackFrame[] stackFrames, string strErrMsg, bool bShowMsgBox = true, bool bRecord = true)
        {
            lock (Lock_LogTryCatch)
            {
                string strFileName = "File Name : " + Path.GetFileName(stackFrames[0].GetFileName()) + "\r\n";
                string strFuncName = "Function Name : " + stackFrames[0].GetMethod().Name + "\r\n";
                string strLine = "Line : " + stackFrames[0].GetFileLineNumber().ToString() + "\r\n\r\n";
                string strCallerFileName = "Caller File Name : " + Path.GetFileName(stackFrames[1].GetFileName()) + "\r\n";
                string strCallerFuncName = "Caller Function Name : " + stackFrames[1].GetMethod().Name + "\r\n";
                string strCallerLine = "Caller Line : " + stackFrames[1].GetFileLineNumber().ToString() + "\r\n";
                strErrMsg = strErrMsg + "\r\n\r\n" + strFileName + strFuncName + strLine + strCallerFileName + strCallerFuncName + strCallerLine;
                string strProgramName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                string strProgramVersion = "Version : " + ProgramVersion() + "\r\n";

                if (bShowMsgBox == true)
                    System.Windows.Forms.MessageBox.Show(strProgramVersion + strErrMsg, strProgramName, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                if (bRecord == true)
                    clsAlarmMessage.AlarmHistoryRecord(strErrMsg);
            }
        }

        private static object Lock_FileRecord = new object();
        /// <summary>
        /// 文件記錄
        /// </summary>
        /// <param name="strDirectory">文件存放的目錄</param>
        /// <param name="recordTime">紀錄時間</param>
        /// <param name="strContext">紀錄內容</param>
        public static void FileRecord(string strDirectory, DateTime recordTime, string strContext)
        {
            lock (Lock_FileRecord)
            {
                try
                {
                    //建立目錄資料夾，如果已有此資料夾，CreateDirectory 不會執行任何動作 
                    System.IO.Directory.CreateDirectory(FilePath + "\\" + strDirectory);

                    //提供建立、複製、刪除、移動和開啟檔案的屬性和執行個體方法
                    //紀錄儲存路徑\\目錄名稱\\目錄名稱+現在時間.txt
                    FileInfo f = new FileInfo(FilePath + "\\" + strDirectory + "\\" + strDirectory + "_" + recordTime.ToString("yyyy-MM-dd") + ".txt");

                    //儲存檔案
                    StreamWriter sw = f.AppendText();
                    sw.Write(strContext);
                    sw.Flush();
                    sw.Close();
                }
                catch (Exception e)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, e.Message, true, false);//當紀錄有問題只show message box(不做紀錄)
                }
            }
        }

        private static object Lock_LogFileRecord = new object();
        /// <summary>
        /// Log記錄
        /// </summary>
        /// <param name="strDirectory">儲存目錄</param>
        /// <param name="strContext">Log內容</param>
        /// <param name="bRepaceNewLine">是否換行</param>
        /// <param name="strColName">欄位名稱，沒輸入則無欄位名稱，只在剛建立時有效</param>
        public static void LogFileRecord(string strDirectory, string strContext, bool bRepaceNewLine = true, string strColName = "")
        {
            lock (Lock_LogFileRecord)
            {
                try
                {
                    //取得目前時間
                    DateTime recordTime = DateTime.Now;

                    //建立儲存資訊
                    string strRepaceNewLine = bRepaceNewLine ? strContext.Replace("\r", string.Empty).Replace("\n", string.Empty) : strContext;
                    strContext = "V" + ProgramVersion() + ",\t" + recordTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + ",\t" + strRepaceNewLine + "\r\n";

                    //新增首列欄位名稱(若沒輸入欄位名稱則不新增欄位列)
                    bool isFileExists = File.Exists(FilePath + "\\" + strDirectory + "\\" + strDirectory + "_" + recordTime.ToString("yyyy-MM-dd") + ".txt");
                    if (isFileExists == false && strColName != "")
                        strContext = "Version\tDate\t" + strColName + "\r\n" + strContext;

                    //文件記錄
                    FileRecord(strDirectory, recordTime, strContext);
                }
                catch (Exception e)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, e.Message, true, false);//當紀錄有問題只show message box(不做紀錄)
                }
            }
        }        

        private static object Lock_LogFileCheck = new object();
        /// <summary>
        /// 移除過舊Log檔案
        /// </summary>
        /// <param name="strDirectory">儲存目錄</param>
        /// <param name="OverDay">指定移除的時間</param>
        public static void LogFileCheck(string strDirectory, ushort OverDay = 20)
        {
            lock (Lock_LogFileCheck)
            {
                try
                {
                    //取得目前時間
                    DateTime myDate = DateTime.Now;

                    DirectoryInfo di = new DirectoryInfo(FilePath + "\\" + strDirectory);
                    //檢查目錄是否存在
                    if (Directory.Exists(FilePath + "\\" + strDirectory))
                    {
                        foreach (var fi in di.GetFiles(strDirectory + "_" + "*.txt"))
                        {
                            //移除過舊檔案
                            string strNewSortFile = fi.Name.Substring(strDirectory.Length + 1, 10);// 2018-03-31 : 10 characters
                            if ((myDate.Date - DateTime.Parse(strNewSortFile)).Days > OverDay)
                                File.Delete(fi.Directory + "\\" + fi.Name);
                        }
                    }
                }
                catch (Exception e)
                {
                    StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                    clsLogFile.LogTryCatch(stackFrames, e.Message, true, true);
                }
            }
        }
    }
}
