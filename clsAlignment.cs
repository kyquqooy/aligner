using prjSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prjAlignment
{
    public enum AlignmentType
    {
        XYθ = 0,
        UVW
    }

    public enum MotorType
    {
        X_Axis = 0,
        Y_Axis,
        θ_Axis
    }
    /// <summary>
    /// 旋轉方向
    /// </summary>
    public enum RotateDirection
    {
        /// <summary>
        /// 正轉
        /// </summary>
        Forward = 0,
        /// <summary>
        /// 反轉
        /// </summary>
        Reverse
    }

    public class clsAlignment
    {        
        private Dictionary<MotorType, MotorParameters> motors;        
        private MotorStatus motorStatus = new MotorStatus();
        private clsCognexAS200 alignmentSensor;
        CognexAS200Coordinate outputCoordinate = new CognexAS200Coordinate(true);
        CognexAS200CommandStatus status = CognexAS200CommandStatus.Fail;
        private int nTmrCheckStatus_f = 0;
        private int GoToAbsPosition_ChkRet_p = 0;
        private int AlignmentCalibration_ChkRet_p = 0;
        private System.Windows.Forms.Timer moveDelayTime = new System.Windows.Forms.Timer();
        private bool moveDelayTimeFlag = false;
        private DataTable calibrationTable;
        public clsAlignment()
        {
            MotorTable = new MotorTable();
            motors = new Dictionary<MotorType, MotorParameters>();
            motors.Add(MotorType.X_Axis, new MotorParameters());
            motors.Add(MotorType.Y_Axis, new MotorParameters());
            motors.Add(MotorType.θ_Axis, new MotorParameters());

            if (!File.Exists(MotorTable.FileName + ".xml"))
                CreateMotorTable();
            LoadMotorTable();
            InitializeEcat();
            motorStatus = new MotorStatus();
            motorStatus.Initialize();

            RunMotor = MotorType.X_Axis;
            RunPoint = 0;

            moveDelayTime.Tick += MoveDelayTime_Tick;
            moveDelayTime.Interval = 1000;

            alignmentSensor = new clsCognexAS200();
            alignmentSensor.SocketPara.InitializeConnect_ChkRet(false);
            while (alignmentSensor.SocketPara.InitializeConnect_ChkRet() != conProg.process.Success)
                System.Windows.Forms.Application.DoEvents();

            calibrationTable = new DataTable();
            calibrationTable.Columns.Add("X_Axis");
            calibrationTable.Columns.Add("Y_Axis");
            calibrationTable.Columns.Add("θ_Axis");
        }

        private void MoveDelayTime_Tick(object sender, EventArgs e)
        {
            moveDelayTimeFlag = true;
        }

        /// <summary>
        /// 馬達參數紀錄表
        /// </summary>
        public MotorTable MotorTable { get; set; }
        /// <summary>
        /// 執行馬達
        /// </summary>
        public MotorType RunMotor { get; set; }
        /// <summary>
        /// 執行點位
        /// </summary>
        public ushort RunPoint { get; set; }
        /// <summary>
        /// 馬達狀態
        /// </summary>
        public MotorStatus MotorStatus
        {
            get { return motorStatus; }
        }
        /// <summary>
        /// 對位感測器
        /// </summary>
        public clsCognexAS200 AlignmentSensor
        {
            get { return alignmentSensor; }
        }

        public Dictionary<MotorType, MotorParameters> Motors
        {
            get { return motors; }
        }

        public DataTable CalibrationTable
        {
            get { return calibrationTable; }
        }
        /// <summary>
        /// 創建馬達參數表初始資料
        /// </summary>
        private void CreateMotorTable()
        {
            MotorTable.AddDevice(0, 1, 0, "X_Axis", 0, 1, 1, 3000);
            MotorTable.AddDevice(0, 2, 0, "Y_Axis", 0, 1, 1, 3000);
            MotorTable.AddDevice(0, 3, 0, "θ_Axis", 0, 1, 1, 3000);
            MotorTable.AddPoint("X_Axis", "Home", MotorTablePointType.Forward, 0, 0, 50, 0.5, 0.5);
            MotorTable.AddPoint("Y_Axis", "Home", MotorTablePointType.Forward, 0, 0, 50, 0.5, 0.5);
            MotorTable.AddPoint("θ_Axis", "Home", MotorTablePointType.Forward, 0, 0, 50, 0.5, 0.5);
            MotorTable.Save();
        }
        /// <summary>
        /// 讀取馬達參數表
        /// </summary>
        private void LoadMotorTable()
        {
            MotorTable.Load();
            MotorParameters temporaryStorage_1 = new MotorParameters();
            MotorParameters temporaryStorage_2 = new MotorParameters();
            MotorParameters temporaryStorage_3 = new MotorParameters();
            MotorTable.GetMotorParameter("X_Axis", ref temporaryStorage_1);
            motors[MotorType.X_Axis] = temporaryStorage_1;
            MotorTable.GetMotorParameter("Y_Axis", ref temporaryStorage_2);
            motors[MotorType.Y_Axis] = temporaryStorage_2;
            MotorTable.GetMotorParameter("θ_Axis", ref temporaryStorage_3);
            motors[MotorType.θ_Axis] = temporaryStorage_3;
        }
        /// <summary>
        /// 初始化Ecat
        /// </summary>
        public void InitializeEcat()
        {
            try
            {
                while (clsECatFunctions.clsEcatFunction.EcatInitial() != conProg.process.Success)
                    System.Windows.Forms.Application.DoEvents();

                MotorType[] motorType = new MotorType[3] { MotorType.X_Axis, MotorType.Y_Axis, MotorType.θ_Axis };
                foreach (MotorType item in motorType)
                {
                    int uIndex = 0;
                    //Get VenderID
                    foreach (ushort MapNodeID in clsECatFunctions.clsEcatFunction.Ecat_DeltaEcatCard[motors[item].ECatAxisDevice.CardNum].MapNodeID_List)
                    {
                        if (motors[item].ECatAxisDevice.NodeID == MapNodeID)
                        {
                            motors[item].ECatAxisDevice.SeqNodeID = clsECatFunctions.clsEcatFunction.Ecat_DeltaEcatCard[motors[item].ECatAxisDevice.CardNum].SeqNodeID_List[uIndex];
                            motors[item].ECatAxisDevice.uVendorID = clsECatFunctions.clsEcatFunction.Ecat_DeltaEcatCard[motors[item].ECatAxisDevice.CardNum].VenderID_List[uIndex];
                            break;
                        }
                        uIndex += 1;
                    }
                }
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }

        public void ShowPos(MotorType motorType, ref double feedback)
        {
            int nCmd = 0, nPos = 0;
            motors[motorType].ECAT_Slave_Motion_Get_Command(ref nCmd);
            motors[motorType].ECAT_Slave_Motion_Get_Position(ref nPos);
            double actualUnit = motors[motorType].ECatAxisDevice.MM_PerRev / motors[motorType].ECatAxisDevice.Pulse_PerRev;

            feedback = (double)nPos * actualUnit;
        }
        /// <summary>
        /// 伺服開啟_關閉
        /// </summary>
        public void Servo_On_Off()
        {
            try
            {
                ushort uCheckOnOff = 0;
                ushort StatusWord = 0;
                //===== Get the Motor's Status =====
                motors[this.RunMotor].ECAT_Slave_Motion_Get_StatusWord(ref StatusWord);
                //=== Check Servo ON/OFF
                uCheckOnOff = (StatusWord & 0x2) == 0x2 ? (ushort)0 : (ushort)1;

                motors[this.RunMotor].ECAT_Slave_Motion_Set_Svon(uCheckOnOff);
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }

        public void Servo_On_Off(MotorType motorType)
        {
            try
            {
                ushort uCheckOnOff = 0;
                ushort StatusWord = 0;
                //===== Get the Motor's Status =====
                motors[motorType].ECAT_Slave_Motion_Get_StatusWord(ref StatusWord);
                //=== Check Servo ON/OFF
                uCheckOnOff = (StatusWord & 0x2) == 0x2 ? (ushort)0 : (ushort)1;

                motors[motorType].ECAT_Slave_Motion_Set_Svon(uCheckOnOff);
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }

        public void Servo_On_Off(MotorType motorType, bool isOn)
        {
            try
            {
                ushort uCheckOnOff = 0;
                ushort StatusWord = 0;
                //===== Get the Motor's Status =====
                motors[motorType].ECAT_Slave_Motion_Get_StatusWord(ref StatusWord);
                //=== Check Servo ON/OFF
                uCheckOnOff = (StatusWord & 0x2) == 0x2 ? (ushort)0 : (ushort)1;

                uCheckOnOff = isOn ? (ushort)1 : (ushort)0;
                motors[motorType].ECAT_Slave_Motion_Set_Svon(uCheckOnOff);
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }
        /// <summary>
        /// 緊急停止
        /// </summary>
        public void Emg_Stop()
        {
            try
            {
                motors[this.RunMotor].ECAT_Slave_Motion_Emg_stop();
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
        }

        public void Emg_Stop(MotorType motorType)
        {
            try
            {
                motors[motorType].ECAT_Slave_Motion_Emg_stop();
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
        }
        /// <summary>
        /// 清除Alarm
        /// </summary>
        public void Ralm()
        {
            try
            {
                motors[this.RunMotor].ECAT_Slave_Motion_Ralm();
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }
        /// <summary>
        /// 位置命令清除
        /// </summary>
        public void PositionReset()
        {
            try
            {
                motors[this.RunMotor].ECAT_Slave_Motion_Set_Position(0);
                motors[this.RunMotor].ECAT_Slave_Motion_Set_Command(0);
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
        }
        /// <summary>
        /// 速度控制
        /// </summary>
        /// <param name="rotateDirection">移動方向</param>
        /// <param name="iniVelocity">初始速度</param>
        /// <param name="maxVelocity">最大速度</param>
        /// <param name="tAcc">加速度</param>
        /// <param name="m_curve">加速模式</param>
        public void AxisPVMove(RotateDirection rotateDirection, int iniVelocity, int maxVelocity, double tAcc, ushort m_curve = 2)
        {
            try
            {
                double actualUnit = motors[this.RunMotor].ECatAxisDevice.Pulse_PerRev / motors[this.RunMotor].ECatAxisDevice.MM_PerRev;

                ushort dir = (ushort)rotateDirection;
                iniVelocity = Convert.ToInt32(iniVelocity * actualUnit);
                maxVelocity = Convert.ToInt32(maxVelocity * actualUnit);             
                
                motors[this.RunMotor].ECAT_Slave_CSP_Start_V_Move(dir, iniVelocity, maxVelocity, tAcc, m_curve);
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }

        public void AxisPVMove(MotorType motorType, RotateDirection rotateDirection, int iniVelocity, int maxVelocity, double tAcc, ushort m_curve = 2)
        {
            try
            {
                double actualUnit = motors[motorType].ECatAxisDevice.Pulse_PerRev / motors[motorType].ECatAxisDevice.MM_PerRev;

                ushort dir = (ushort)rotateDirection;
                iniVelocity = Convert.ToInt32(iniVelocity * actualUnit);
                maxVelocity = Convert.ToInt32(maxVelocity * actualUnit);

                motors[motorType].ECAT_Slave_CSP_Start_V_Move(dir, iniVelocity, maxVelocity, tAcc, m_curve);
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }
        /// <summary>
        /// 位置控制
        /// </summary>
        /// <param name="dist">移動距離</param>
        /// <param name="iniVelocity">初始速度</param>
        /// <param name="maxVelocity">最大速度</param>
        /// <param name="tAcc">加速度</param>
        /// <param name="tDec">減速度</param>
        /// <param name="m_abs">移動類型</param>
        /// <param name="m_curve">加速模式</param>
        public void AxisPPMove(double dist, int iniVelocity, int maxVelocity, double tAcc, double tDec, ushort m_abs = 0, ushort m_curve = 2)
        {
            try
            {
                double actualUnit = motors[this.RunMotor].ECatAxisDevice.Pulse_PerRev / motors[this.RunMotor].ECatAxisDevice.MM_PerRev;

                dist = Convert.ToInt32(dist * actualUnit);
                iniVelocity = Convert.ToInt32(iniVelocity * actualUnit);
                maxVelocity = Convert.ToInt32(maxVelocity * actualUnit);

                motors[this.RunMotor].ECAT_Slave_CSP_Start_Move((int)dist, iniVelocity, maxVelocity, iniVelocity, tAcc, tDec, m_curve, m_abs);
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }

        public void AxisPPMove(MotorType motorType, double dist, int iniVelocity, int maxVelocity, double tAcc, double tDec, ushort m_abs = 0, ushort m_curve = 2)
        {
            try
            {
                double actualUnit = motors[motorType].ECatAxisDevice.Pulse_PerRev / motors[motorType].ECatAxisDevice.MM_PerRev;

                dist = Convert.ToInt32(dist * actualUnit);
                iniVelocity = Convert.ToInt32(iniVelocity * actualUnit);
                maxVelocity = Convert.ToInt32(maxVelocity * actualUnit);

                motors[motorType].ECAT_Slave_CSP_Start_Move((int)dist, iniVelocity, maxVelocity, iniVelocity, tAcc, tDec, m_curve, m_abs);
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }
        /// <summary>
        /// 移動紀錄位置
        /// </summary>
        /// <param name="point">馬達點位</param>
        public void GoPoint(ushort point)
        {
            try
            {
                motors[this.RunMotor].ECAT_Slave_CSP_GoToPosition(point, false);
                while (motors[this.RunMotor].ECAT_Slave_CSP_GoToPosition(point) != conProg.process.Success)
                    System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }
        /// <summary>
        /// 原點復歸
        /// </summary>
        /// <param name="zero_Org">復歸模式</param>
        public void GoHome(ushort zero_Org)
        {
            try
            {
                motors[this.RunMotor].ECAT_Slave_GoHome(zero_Org, false);
                while (motors[this.RunMotor].ECAT_Slave_GoHome(zero_Org) != conProg.process.Success)
                    System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }

        public void GoHome(MotorType motorType, ushort zero_Org)
        {
            try
            {
                motors[motorType].ECAT_Slave_GoHome(zero_Org, false);
                while (motors[motorType].ECAT_Slave_GoHome(zero_Org) != conProg.process.Success)
                    System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception err)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
        }
        /// <summary>
        /// 輪巡馬達狀態
        /// </summary>
        /// <param name="isCatch"></param>
        public void tmrGetMotorStatus_Tick(ref bool isCatch)
        {
            isCatch = false;
            //Timer重入保護-檢查Timer內程式碼是否正在執行
            if (Interlocked.Exchange(ref nTmrCheckStatus_f, 1) == 1)
                return;
            try
            {
                ushort nodeID = motors[this.RunMotor].ECatAxisDevice.NodeID;
                ushort seqNodeID = motors[this.RunMotor].ECatAxisDevice.SeqNodeID;
                uint vendorID = motors[this.RunMotor].ECatAxisDevice.uVendorID;                

                bool bMotorExist_f = false;
                //只掃描初始化NodeID清單中的馬達, 若清單中沒有則Return
                for (int i = 0; i < clsECatFunctions.clsEcatFunction.Ecat_DeltaEcatCard[0].Motor_NodeID_Alias.Length; i++) //比對存在的馬達清單
                {
                    if (nodeID == clsECatFunctions.clsEcatFunction.Ecat_DeltaEcatCard[0].Motor_NodeID_Alias[i])
                    {
                        bMotorExist_f = true;
                        break;
                    }
                }
                if (bMotorExist_f == false)
                {
                    Interlocked.Exchange(ref nTmrCheckStatus_f, 0);
                    return;
                }

                //===== Check Alarm =====//
                int AlarmCode = 0;
                if (vendorID == clsECatFunctions.clsEcatFunction.Vender_Delta)
                {
                    ushort Page = 0, Index = 1;
                    motors[this.RunMotor].Ecat_Slave_DeltaServo_Read_Parameter(Page, Index, ref AlarmCode);
                }
                else if (vendorID == clsECatFunctions.clsEcatFunction.Vender_Yaskawa)
                {
                    ushort Index = 0x603F, SubIndex = 0;
                    ushort DataSize = 2;
                    byte[] Data = new byte[2];

                    motors[this.RunMotor].ECAT_Slave_SDO_Read_Message(Index, SubIndex, DataSize, ref Data);
                    AlarmCode = Data[0] + (Data[1] * 256);
                }
                motorStatus.AlarmCode = AlarmCode;

                //===== Get Status Word =====//
                ushort StatusWord = 0;
                motors[this.RunMotor].ECAT_Slave_Motion_Get_StatusWord(ref StatusWord);
                //Ready to switch ON           
                motorStatus.IsMotorReady = (StatusWord & 0x1) == 0x1 ? true : false;
                // Servo-ON          
                motorStatus.IsServoOn = (StatusWord & 0x2) == 0x2 ? true : false;

                //===== Get Limit&Home Status=====//
                byte[] iStatus = new byte[4];
                motors[this.RunMotor].ECAT_Slave_SDO_Read_Message(0x60FD, 0, 4, ref iStatus);
                //Negative limit switch
                motorStatus.IsNegativeLimit = (iStatus[0] & 0x01) == 0x01 ? true : false;
                //Positive limit switch
                motorStatus.IsPositiveLimit = (iStatus[0] & 0x02) == 0x02 ? true : false;
                //Home Switch
                motorStatus.IsHome = (iStatus[0] & 0x04) == 0x04 ? true : false;
                //Status; 0 = Finish or Not Move; 1 = In Homing; 2 = Homing is terminated; 3 = Error occurred in homing
                ushort hStatus = 0;
                motors[this.RunMotor].ECAT_Slave_Home_Status(ref hStatus);
                motorStatus.HomeStatus = hStatus;
                ushort Mdone = 0;
                motors[this.RunMotor].ECAT_Slave_Motion_Get_Mdone(ref Mdone);
                motorStatus.MotionDone = Mdone;

                //===== Get Command/Position =====
                int nCmd = 0, nPos = 0;
                motors[this.RunMotor].ECAT_Slave_Motion_Get_Command(ref nCmd);
                motors[this.RunMotor].ECAT_Slave_Motion_Get_Position(ref nPos);
                double actualUnit = motors[this.RunMotor].ECatAxisDevice.MM_PerRev / motors[this.RunMotor].ECatAxisDevice.Pulse_PerRev;

                motorStatus.Command = (double)nCmd * actualUnit;
                motorStatus.Feedback = (double)nPos * actualUnit;
                motorStatus.ErrorCounter = motorStatus.Command - motorStatus.Feedback;
            }
            catch (Exception err)
            {
                isCatch = true;
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, err.Message, true, true);
            }
            //Timer重入保護-Reset
            Interlocked.Exchange(ref nTmrCheckStatus_f, 0);
        }

        private object Lock_GoToAbsPosition = new object();
        /// <summary>
        /// 對位輸入座標
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="θ">角度</param>
        /// <param name="point">點位</param>
        /// <param name="Action">流程操作</param>
        /// <returns></returns>
        public ushort GoToAbsPosition(double x, double y, double θ, ushort point, bool Action = true)
        {
            lock (Lock_GoToAbsPosition)
            {
                ushort nValue = conProg.process.Busy;
                if (RunMode.Pause_f == true)
                    return nValue;
                if (AlarmMessage.Alarm_SW)
                    return nValue;
                if (Action == false)
                    GoToAbsPosition_ChkRet_p = 9999;
                ushort reValue = conProg.process.Success;
                switch (GoToAbsPosition_ChkRet_p)
                {
                    case 0:
                        reValue += motors[MotorType.X_Axis].ECAT_Slave_CSP_GoToAbsPosition(x, point, false);
                        reValue += motors[MotorType.Y_Axis].ECAT_Slave_CSP_GoToAbsPosition(y, point, false);
                        reValue += motors[MotorType.θ_Axis].ECAT_Slave_CSP_GoToAbsPosition(θ, point, false);
                        if (reValue == conProg.process.Success)
                            GoToAbsPosition_ChkRet_p = 10;
                        break;
                    case 10:
                        if (motors[MotorType.X_Axis].ECAT_Slave_CSP_GoToAbsPosition(x, point) == conProg.process.Success)
                            GoToAbsPosition_ChkRet_p = 20;
                        break;
                    case 20:
                        if (motors[MotorType.Y_Axis].ECAT_Slave_CSP_GoToAbsPosition(y, point) == conProg.process.Success)
                            GoToAbsPosition_ChkRet_p = 30;
                        break;
                    case 30:
                        if (motors[MotorType.θ_Axis].ECAT_Slave_CSP_GoToAbsPosition(θ, point) == conProg.process.Success)
                            GoToAbsPosition_ChkRet_p = 40;
                        break;
                    case 40:
                        moveDelayTimeFlag = false;
                        GoToAbsPosition_ChkRet_p = 50;
                        break;
                    case 50:
                        moveDelayTime.Start();
                        GoToAbsPosition_ChkRet_p = 60;
                        break;
                    case 60:
                        if (moveDelayTimeFlag)
                            GoToAbsPosition_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            GoToAbsPosition_ChkRet_p = 0;
                        break;
                }
                return nValue;
            }
        }

        private object Lock_AlignmentCalibration_ChkRet = new object();
        /// <summary>
        /// AlignmentSensor校正
        /// </summary>
        /// <param name="selectCamera"></param>
        /// <param name="nowX"></param>
        /// <param name="nowY"></param>
        /// <param name="nowθ"></param>
        /// <param name="point"></param>
        /// <param name="Action"></param>
        /// <param name="CheckAlarm"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public ushort AlignmentCalibration_ChkRet(SelectCamera selectCamera, double nowX, double nowY, double nowθ, ushort point, bool Action = true, bool CheckAlarm = true, double timeOut = 3000)
        {
            lock (Lock_AlignmentCalibration_ChkRet)
            {
                ushort nValue = conProg.process.Busy;
                string strError = string.Empty;

                if (RunMode.Pause_f == true || AlarmMessage.Alarm_SW)
                    return nValue;

                if (Action == false)
                    AlignmentCalibration_ChkRet_p = 9999;

                SelectFeature selectFeature = SelectFeature.Feature_1;
                CognexAS200Coordinate inputCoordinate = new CognexAS200Coordinate();
                inputCoordinate.X = nowX;
                inputCoordinate.Y = nowY;
                inputCoordinate.Z = 0;
                inputCoordinate.A = nowθ;
                inputCoordinate.B = 0;
                inputCoordinate.C = 0;                              

                switch (AlignmentCalibration_ChkRet_p)
                {
                    case 0:
                        nValue = conProg.process.Busy;
                        calibrationTable.Clear();
                        if (GoToAbsPosition(inputCoordinate.X, inputCoordinate.Y, inputCoordinate.A, point, false) == conProg.process.Success)
                            AlignmentCalibration_ChkRet_p = 10;
                        break;
                    case 10:
                        if (GoToAbsPosition(inputCoordinate.X, inputCoordinate.Y, inputCoordinate.A, point) == conProg.process.Success)
                        {
                            calibrationTable.Rows.Add(inputCoordinate.X, inputCoordinate.Y, inputCoordinate.A);
                            AlignmentCalibration_ChkRet_p = 20;
                        }                           
                        break;
                    case 20:
                        if (alignmentSensor.Functions.AutoCalibrationBegin_ChkRet(selectCamera, selectFeature, inputCoordinate, ref status, ref outputCoordinate, false) == conProg.process.Success)
                            AlignmentCalibration_ChkRet_p = 30;
                        break;
                    case 30:
                        if (alignmentSensor.Functions.AutoCalibrationBegin_ChkRet(selectCamera, selectFeature, inputCoordinate, ref status, ref outputCoordinate) == conProg.process.Success)
                        {
                            if (status == CognexAS200CommandStatus.Fail)
                            {
                                strError = "AutoCalibrationBegin_失敗";
                                AlignmentCalibration_ChkRet_p = 8000;
                            }
                            else                                
                                AlignmentCalibration_ChkRet_p = 40;
                        }
                        break;
                    case 40:
                        if (GoToAbsPosition(outputCoordinate.X, outputCoordinate.Y, outputCoordinate.A, point, false) == conProg.process.Success)
                            AlignmentCalibration_ChkRet_p = 50;
                        break;
                    case 50:
                        if (GoToAbsPosition(outputCoordinate.X, outputCoordinate.Y, outputCoordinate.A, point) == conProg.process.Success)
                        {
                            calibrationTable.Rows.Add(outputCoordinate.X, outputCoordinate.Y, outputCoordinate.A);
                            AlignmentCalibration_ChkRet_p = 60;
                        }
                        break;
                    case 60:
                        if (alignmentSensor.Functions.AutoCalibration_ChkRet(selectCamera, selectFeature, outputCoordinate, ref status, ref outputCoordinate, false) == conProg.process.Success)
                            AlignmentCalibration_ChkRet_p = 70;
                        break;
                    case 70:
                        if (alignmentSensor.Functions.AutoCalibration_ChkRet(selectCamera, selectFeature, outputCoordinate, ref status, ref outputCoordinate) == conProg.process.Success)
                        {
                            switch (status)
                            {
                                case CognexAS200CommandStatus.Fail:
                                    strError = "AutoCalibration_失敗";
                                    AlignmentCalibration_ChkRet_p = 8000;
                                    break;
                                case CognexAS200CommandStatus.Success:
                                    AlignmentCalibration_ChkRet_p = 9999;
                                    break;
                                case CognexAS200CommandStatus.KeepExecution:
                                    AlignmentCalibration_ChkRet_p = 40;
                                    break;
                            }
                        }
                        break;
                    case 8000: //報警流程選擇
                        nValue = conProg.process.Fail;
                        if (AlarmMessage.Alarm_ButtonSelection == conProg.btnRetry)
                            AlignmentCalibration_ChkRet_p = 0;
                        else if (AlarmMessage.Alarm_ButtonSelection == conProg.btnIgnore)
                            AlignmentCalibration_ChkRet_p = 9999;
                        break;
                    case 9999: //流程初始化 
                        nValue = conProg.process.Success;
                        if (Action == false)
                            AlignmentCalibration_ChkRet_p = 0;
                        break;
                }

                if (strError != string.Empty) //顯示報警
                {
                    AlignmentCalibration_ChkRet_p = CheckAlarm == true ? 8000 : 9999;
                    DeviceAlarmMessage(MethodBase.GetCurrentMethod().Name, strError, false, CheckAlarm);
                }
                return nValue;
            }
        }
        /// <summary>
        /// 報警模組
        /// </summary>
        /// <param name="strFunctionName">造成錯誤的方法</param>
        /// <param name="strErr">錯誤原因</param>
        /// <param name="btnRetryOnly">是否沒有忽略選項</param>
        /// <param name="CheckAlarm">是否檢查錯誤</param>
        private void DeviceAlarmMessage(string strFunctionName, string strErr, bool btnRetryOnly = true, bool CheckAlarm = true)
        {
            if (AlarmMessage.Alarm_SW == true)
                return;

            string strAlarmMag = "ErrorType：clsAlignment\r\n" +
                "ErrorFunction：" + strFunctionName + "\r\n" +
                "ErrorMessage：" + strErr;
            if (CheckAlarm == true)
                AlarmMessage.ShowAlarm(strAlarmMag, 0, btnRetryOnly);
            else
                AlarmMessage.AlarmHistoryRecord(strAlarmMag);
        }
    }
    public struct MotorStatus
    {
        public int AlarmCode { get; internal set; }
        public bool IsMotorReady { get; internal set; }
        public bool IsServoOn { get; internal set; }
        public bool IsNegativeLimit { get; internal set; }
        public bool IsPositiveLimit { get; internal set; }
        public bool IsHome { get; internal set; }
        public ushort HomeStatus { get; internal set; }
        public ushort MotionDone { get; internal set; }
        public double Command { get; internal set; }
        public double Feedback { get; internal set; }
        public double ErrorCounter { get; internal set; }
        public void Initialize()
        {
            AlarmCode = 0;
            IsMotorReady = false;
            IsServoOn = false;
            IsNegativeLimit = true;
            IsPositiveLimit = true;
            IsHome = false;
            HomeStatus = 0;
            MotionDone = 0;
            Command = 0;
            Feedback = 0;
            ErrorCounter = 0;
        }
    }
    /// <summary>
    /// 馬達參數表_點位類型
    /// </summary>
    public enum MotorTablePointType
    {
        /// <summary>
        /// 馬達移動_絕對式
        /// </summary>
        [Description("Abs")]    
        Abs = 0,
        /// <summary>
        /// 馬達移動_相對式
        /// </summary>
        [Description("Rel")]
        Rel,
        /// <summary>
        /// 原點復歸_正轉
        /// </summary>
        [Description("+")]
        Forward,
        /// <summary>
        /// 原點復歸_反轉
        /// </summary>
        [Description("-")]
        Reverse
    }
    public class MotorTable
    {
        private DataSet motorCollection;
        private string fileName;
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        public MotorTable(string fileName = "MotorParameter")
        {
            this.fileName = fileName;
            motorCollection = new DataSet("MotorCollection");
            CreateMotorTable();
        }
        /// <summary>
        /// 馬達資料庫
        /// </summary>
        public DataSet MotorDatabase
        {
            get { return motorCollection; }
            set { motorCollection = value; }
        }
        /// <summary>
        /// 檔案名稱
        /// </summary>
        public string FileName
        {
            get { return fileName; }
        }
        /// <summary>
        /// 創建馬達資料表
        /// </summary>
        private void CreateMotorTable()
        {
            try
            {
                if (!motorCollection.Tables.Contains("Device"))
                {
                    motorCollection.Tables.Add("Device");
                    motorCollection.Tables["Device"].Columns.Add("CardID");
                    motorCollection.Tables["Device"].Columns.Add("NodeID");
                    motorCollection.Tables["Device"].Columns.Add("SlotID");
                    motorCollection.Tables["Device"].Columns.Add("AxisName");
                    motorCollection.Tables["Device"].Columns.Add("StationNo");
                    motorCollection.Tables["Device"].Columns.Add("PulsePerRev");
                    motorCollection.Tables["Device"].Columns.Add("MilliMeterPerRev");
                    motorCollection.Tables["Device"].Columns.Add("MaxRPM");
                }
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
        }
        /// <summary>
        /// 增加馬達
        /// </summary>
        /// <param name="cardID">卡號</param>
        /// <param name="nodeID">站號</param>
        /// <param name="slotID">子站號</param>
        /// <param name="axisName">馬達代號</param>
        /// <param name="stationNo">站別</param>
        /// <param name="pulsePerRev">多少脈衝一圈</param>
        /// <param name="milliMeterPerRev">多少mm一圈</param>
        /// <param name="maxRPM">最大轉速</param>
        /// <returns></returns>
        public bool AddDevice(ushort cardID, ushort nodeID, ushort slotID, string axisName, int stationNo = 0, double pulsePerRev = 1, double milliMeterPerRev = 1, int maxRPM = 3000)
        {
            bool isAdd = false;
            try
            {
                DataRow[] drArray = motorCollection.Tables["Device"].Select(
                    "CardID = " + cardID + " and " +
                    "NodeID = " + nodeID + " and " +
                    "SlotID = " + slotID + " and " +
                    "AxisName = '" + axisName + "'");
                if (drArray.Length == 0)
                {
                    if (!motorCollection.Tables.Contains(axisName))
                    {
                        motorCollection.Tables["Device"].Rows.Add(
                        cardID,
                        nodeID,
                        slotID,
                        axisName,
                        stationNo,
                        pulsePerRev,
                        milliMeterPerRev,
                        maxRPM);

                        motorCollection.Tables.Add(axisName);
                        motorCollection.Tables[axisName].Columns.Add("Description");
                        motorCollection.Tables[axisName].Columns.Add("Type");
                        motorCollection.Tables[axisName].Columns.Add("Position");
                        motorCollection.Tables[axisName].Columns.Add("iniVelocity");
                        motorCollection.Tables[axisName].Columns.Add("maxVelocity");
                        motorCollection.Tables[axisName].Columns.Add("tAcc");
                        motorCollection.Tables[axisName].Columns.Add("tDec");
                        isAdd = true;
                    }
                }
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
            return isAdd;
        }
        /// <summary>
        /// 移除馬達
        /// </summary>
        /// <param name="axisName">馬達代號</param>
        public void RemoveDevice(string axisName)
        {
            try
            {
                DataRow[] drArray = motorCollection.Tables["Device"].Select("AxisName = '" + axisName + "'");
                foreach (DataRow item in drArray)
                    motorCollection.Tables["Device"].Rows.Remove(item);
                motorCollection.Tables.Remove(axisName);
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
        }
        /// <summary>
        /// 更改馬達代號
        /// </summary>
        /// <param name="oldAxisName">舊代號</param>
        /// <param name="newAxisName">新代號</param>
        /// <returns></returns>
        public bool ChangeDevice(string oldAxisName, string newAxisName)
        {
            bool isChange = false;
            try
            {
                DataRow[] drArray = motorCollection.Tables["Device"].Select("AxisName = '" + oldAxisName + "'");
                if (drArray.Length > 0)
                {
                    if (motorCollection.Tables.Contains(oldAxisName))
                    {
                        drArray[0]["AxisName"] = newAxisName;
                        motorCollection.Tables[oldAxisName].TableName = newAxisName;
                        isChange = true;
                    }
                }
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
            return isChange;
        }
        /// <summary>
        /// 增加馬達點位
        /// </summary>
        /// <param name="axisName">馬達代號</param>
        /// <param name="description">點位描素</param>
        /// <param name="pointType">點位類型</param>
        /// <param name="position">位置</param>
        /// <param name="iniVelocity">初始速度</param>
        /// <param name="maxVelocity">最大速度</param>
        /// <param name="tAcc">加速度</param>
        /// <param name="tDec">減速度</param>
        /// <returns></returns>
        public bool AddPoint(string axisName, string description, MotorTablePointType pointType = MotorTablePointType.Abs, double position = 0, double iniVelocity = 0, double maxVelocity = 0, double tAcc = 0.1, double tDec = 0.1)
        {
            bool isAdd = false;
            try
            {
                if (motorCollection.Tables.Contains(axisName))
                {
                    DataRow[] drArray = motorCollection.Tables[axisName].Select("Description = '" + description + "'");
                    if (drArray.Length == 0)
                    {
                        motorCollection.Tables[axisName].Rows.Add(
                        description,
                        pointType.ToString(),
                        position,
                        iniVelocity,
                        maxVelocity,
                        tAcc,
                        tDec);

                        isAdd = true;
                    }
                }
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
            return isAdd;
        }
        /// <summary>
        /// 移除馬達點位
        /// </summary>
        /// <param name="axisName">馬達代號</param>
        /// <param name="description">點位描素</param>
        public void RemovePoint(string axisName, string description)
        {
            try
            {
                if (motorCollection.Tables.Contains(axisName))
                {
                    DataRow[] drArray = motorCollection.Tables[axisName].Select("Description = '" + description + "'");
                    foreach (DataRow item in drArray)
                        motorCollection.Tables[axisName].Rows.Remove(item);
                }
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
        }
        /// <summary>
        /// 移除馬達點位
        /// </summary>
        /// <param name="axisName">馬達代號</param>
        /// <param name="index">索引</param>
        public void RemovePoint(string axisName, int index)
        {
            try
            {
                if (motorCollection.Tables.Contains(axisName))
                    if (motorCollection.Tables[axisName].Rows.Count > index)
                        motorCollection.Tables[axisName].Rows.RemoveAt(index);
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
        }
        /// <summary>
        /// 更改馬達點位
        /// </summary>
        /// <param name="axisName">馬達代號</param>
        /// <param name="oldDescription">舊點位描素</param>
        /// <param name="newDescription">新點位描素</param>
        /// <returns></returns>
        public bool ChangePoint(string axisName, string oldDescription, string newDescription)
        {
            bool isChange = false;
            try
            {
                if (motorCollection.Tables.Contains(axisName))
                {
                    DataRow[] drArray = motorCollection.Tables[axisName].Select("Description = '" + oldDescription + "'");
                    foreach (DataRow item in drArray)
                    {
                        motorCollection.Tables[axisName].Rows.Remove(item);
                        item["Description"] = newDescription;
                        motorCollection.Tables[axisName].Rows.Add(item);
                    }
                    isChange = true;
                }
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
            return isChange;
        }
        /// <summary>
        /// 儲存
        /// </summary>
        public void Save()
        {
            try
            {
                motorCollection.WriteXmlSchema(fileName + "Schema.xml");
                motorCollection.WriteXml(fileName + ".xml");
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
        }
        /// <summary>
        /// 載入
        /// </summary>
        public void Load()
        {
            try
            {
                motorCollection.Clear();
                motorCollection.ReadXmlSchema(fileName + "Schema.xml");
                motorCollection.ReadXml(fileName + ".xml");
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
        }
        /// <summary>
        /// 讀取馬達參數
        /// </summary>
        /// <param name="axisName">馬達代號</param>
        /// <param name="motorParameters">馬達參數</param>
        /// <returns></returns>
        public bool GetMotorParameter(string axisName, ref MotorParameters motorParameters)
        {
            bool isGet = false;
            try
            {
                DataRow[] drArray = motorCollection.Tables["Device"].Select("AxisName = '" + axisName + "'");
                if (drArray.Length == 1)
                {
                    motorParameters.ECatAxisDevice.CardNum = Convert.ToUInt16(drArray[0]["CardID"]);
                    motorParameters.ECatAxisDevice.NodeID = Convert.ToUInt16(drArray[0]["NodeID"]);
                    motorParameters.ECatAxisDevice.SlotID = Convert.ToUInt16(drArray[0]["SlotID"]);
                    motorParameters.ECatAxisDevice.MotorName = Convert.ToString(drArray[0]["AxisName"]);
                    motorParameters.ECatAxisDevice.StationNo = Convert.ToUInt16(drArray[0]["StationNo"]);
                    motorParameters.ECatAxisDevice.Pulse_PerRev = Convert.ToUInt32(drArray[0]["PulsePerRev"]);
                    motorParameters.ECatAxisDevice.MM_PerRev = Convert.ToSingle(drArray[0]["MilliMeterPerRev"]);
                    motorParameters.ECatAxisDevice.MaxRPM = Convert.ToUInt32(drArray[0]["MaxRPM"]);

                    if (motorCollection.Tables.Contains(axisName))
                    {
                        DataTable pointTable = motorCollection.Tables[axisName];
                        motorParameters.RunParameter = new MotorParameters.MoveParameters[pointTable.Rows.Count];
                        for (int i = 0; i < motorParameters.RunParameter.Length; i++)
                        {
                            motorParameters.RunParameter[i] = new MotorParameters.MoveParameters();
                            motorParameters.RunParameter[i].DirType = Convert.ToString(pointTable.Rows[i]["Type"]);
                            motorParameters.RunParameter[i].Position = Convert.ToDouble(pointTable.Rows[i]["Position"]);
                            motorParameters.RunParameter[i].iniVelocity = Convert.ToDouble(pointTable.Rows[i]["iniVelocity"]);
                            motorParameters.RunParameter[i].maxVelocity = Convert.ToDouble(pointTable.Rows[i]["maxVelocity"]);
                            motorParameters.RunParameter[i].tAcc = Convert.ToDouble(pointTable.Rows[i]["tAcc"]);
                            motorParameters.RunParameter[i].tDec = Convert.ToDouble(pointTable.Rows[i]["tDec"]);
                        }
                        isGet = true;
                    }
                }
            }
            catch (Exception e)
            {
                StackFrame[] stackFrames = new StackTrace(true).GetFrames();
                LogFile.LogTryCatch(stackFrames, e.Message, true, true);
            }
            return isGet;
        }
    }
}

