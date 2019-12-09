namespace prjVisionController.Open_eVision
{
    partial class frmFindLine
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnSetROIEnd = new System.Windows.Forms.Button();
            this.btnSetROIStart = new System.Windows.Forms.Button();
            this.nudPosition_Angle = new System.Windows.Forms.NumericUpDown();
            this.nudPosition_Length = new System.Windows.Forms.NumericUpDown();
            this.nudPosition_Tolerance = new System.Windows.Forms.NumericUpDown();
            this.nudPosition_CenterY = new System.Windows.Forms.NumericUpDown();
            this.nudPosition_CenterX = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.picDisplay = new System.Windows.Forms.PictureBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.label7 = new System.Windows.Forms.Label();
            this.cboTransitionType = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.nudSamplingStep = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.nudThickness = new System.Windows.Forms.NumericUpDown();
            this.nudThreshold = new System.Windows.Forms.NumericUpDown();
            this.nudMinAmplitude = new System.Windows.Forms.NumericUpDown();
            this.nudMinArea = new System.Windows.Forms.NumericUpDown();
            this.dgvSampleTable = new System.Windows.Forms.DataGridView();
            this.label6 = new System.Windows.Forms.Label();
            this.txtResults_Angle = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.txtResults_CenterX = new System.Windows.Forms.TextBox();
            this.txtResults_CenterY = new System.Windows.Forms.TextBox();
            this.btnTrain = new System.Windows.Forms.Button();
            this.groupBox3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosition_Angle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosition_Length)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosition_Tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosition_CenterY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosition_CenterX)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSamplingStep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudThickness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinAmplitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinArea)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSampleTable)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tabControl1);
            this.groupBox3.Location = new System.Drawing.Point(618, 102);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(554, 447);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "影像工具";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 23);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(548, 421);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnSetROIEnd);
            this.tabPage1.Controls.Add(this.btnSetROIStart);
            this.tabPage1.Controls.Add(this.nudPosition_Angle);
            this.tabPage1.Controls.Add(this.nudPosition_Length);
            this.tabPage1.Controls.Add(this.nudPosition_Tolerance);
            this.tabPage1.Controls.Add(this.nudPosition_CenterY);
            this.tabPage1.Controls.Add(this.nudPosition_CenterX);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabPage1.Location = new System.Drawing.Point(4, 26);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(540, 391);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "ROI";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnSetROIEnd
            // 
            this.btnSetROIEnd.Location = new System.Drawing.Point(409, 322);
            this.btnSetROIEnd.Margin = new System.Windows.Forms.Padding(4);
            this.btnSetROIEnd.Name = "btnSetROIEnd";
            this.btnSetROIEnd.Size = new System.Drawing.Size(92, 40);
            this.btnSetROIEnd.TabIndex = 2;
            this.btnSetROIEnd.Text = "OK";
            this.btnSetROIEnd.UseVisualStyleBackColor = true;
            this.btnSetROIEnd.Click += new System.EventHandler(this.btnSetROIEnd_Click);
            // 
            // btnSetROIStart
            // 
            this.btnSetROIStart.Location = new System.Drawing.Point(409, 255);
            this.btnSetROIStart.Margin = new System.Windows.Forms.Padding(4);
            this.btnSetROIStart.Name = "btnSetROIStart";
            this.btnSetROIStart.Size = new System.Drawing.Size(92, 40);
            this.btnSetROIStart.TabIndex = 2;
            this.btnSetROIStart.Text = "ROI";
            this.btnSetROIStart.UseVisualStyleBackColor = true;
            this.btnSetROIStart.Click += new System.EventHandler(this.btnSetROIStart_Click);
            // 
            // nudPosition_Angle
            // 
            this.nudPosition_Angle.DecimalPlaces = 3;
            this.nudPosition_Angle.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudPosition_Angle.Location = new System.Drawing.Point(192, 244);
            this.nudPosition_Angle.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.nudPosition_Angle.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.nudPosition_Angle.Name = "nudPosition_Angle";
            this.nudPosition_Angle.Size = new System.Drawing.Size(101, 30);
            this.nudPosition_Angle.TabIndex = 1;
            // 
            // nudPosition_Length
            // 
            this.nudPosition_Length.DecimalPlaces = 3;
            this.nudPosition_Length.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudPosition_Length.Location = new System.Drawing.Point(192, 190);
            this.nudPosition_Length.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudPosition_Length.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this.nudPosition_Length.Name = "nudPosition_Length";
            this.nudPosition_Length.Size = new System.Drawing.Size(101, 30);
            this.nudPosition_Length.TabIndex = 1;
            // 
            // nudPosition_Tolerance
            // 
            this.nudPosition_Tolerance.DecimalPlaces = 3;
            this.nudPosition_Tolerance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudPosition_Tolerance.Location = new System.Drawing.Point(192, 136);
            this.nudPosition_Tolerance.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudPosition_Tolerance.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this.nudPosition_Tolerance.Name = "nudPosition_Tolerance";
            this.nudPosition_Tolerance.Size = new System.Drawing.Size(101, 30);
            this.nudPosition_Tolerance.TabIndex = 1;
            // 
            // nudPosition_CenterY
            // 
            this.nudPosition_CenterY.DecimalPlaces = 3;
            this.nudPosition_CenterY.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudPosition_CenterY.Location = new System.Drawing.Point(192, 82);
            this.nudPosition_CenterY.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudPosition_CenterY.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this.nudPosition_CenterY.Name = "nudPosition_CenterY";
            this.nudPosition_CenterY.Size = new System.Drawing.Size(101, 30);
            this.nudPosition_CenterY.TabIndex = 1;
            // 
            // nudPosition_CenterX
            // 
            this.nudPosition_CenterX.DecimalPlaces = 3;
            this.nudPosition_CenterX.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudPosition_CenterX.Location = new System.Drawing.Point(192, 28);
            this.nudPosition_CenterX.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudPosition_CenterX.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this.nudPosition_CenterX.Name = "nudPosition_CenterX";
            this.nudPosition_CenterX.Size = new System.Drawing.Size(101, 30);
            this.nudPosition_CenterX.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(37, 250);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 19);
            this.label5.TabIndex = 0;
            this.label5.Text = "Angle：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 196);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 19);
            this.label4.TabIndex = 0;
            this.label4.Text = "Length：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 19);
            this.label3.TabIndex = 0;
            this.label3.Text = "Tolerance：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 19);
            this.label2.TabIndex = 0;
            this.label2.Text = "Center Y：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Center X：";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnTrain);
            this.tabPage2.Controls.Add(this.dgvSampleTable);
            this.tabPage2.Controls.Add(this.nudMinArea);
            this.tabPage2.Controls.Add(this.nudMinAmplitude);
            this.tabPage2.Controls.Add(this.nudThreshold);
            this.tabPage2.Controls.Add(this.nudThickness);
            this.tabPage2.Controls.Add(this.nudSamplingStep);
            this.tabPage2.Controls.Add(this.cboTransitionType);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.label12);
            this.tabPage2.Controls.Add(this.label11);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabPage2.Location = new System.Drawing.Point(4, 26);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(540, 391);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Measurement";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.txtResults_Angle);
            this.tabPage3.Controls.Add(this.txtResults_CenterY);
            this.tabPage3.Controls.Add(this.txtResults_CenterX);
            this.tabPage3.Controls.Add(this.label6);
            this.tabPage3.Controls.Add(this.label13);
            this.tabPage3.Controls.Add(this.label14);
            this.tabPage3.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabPage3.Location = new System.Drawing.Point(4, 26);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(540, 391);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Result";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnLoadImage);
            this.groupBox2.Controls.Add(this.btnLoad);
            this.groupBox2.Controls.Add(this.btnSave);
            this.groupBox2.Location = new System.Drawing.Point(618, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(554, 84);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "影像來源";
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.Location = new System.Drawing.Point(15, 27);
            this.btnLoadImage.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(92, 40);
            this.btnLoadImage.TabIndex = 2;
            this.btnLoadImage.Text = "載入圖檔";
            this.btnLoadImage.UseVisualStyleBackColor = true;
            this.btnLoadImage.Click += new System.EventHandler(this.btnLoadImage_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(243, 27);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(92, 40);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "讀取";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(131, 27);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(92, 40);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "儲存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.picDisplay);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(600, 537);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "操作畫面";
            // 
            // picDisplay
            // 
            this.picDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picDisplay.Location = new System.Drawing.Point(3, 23);
            this.picDisplay.Margin = new System.Windows.Forms.Padding(4);
            this.picDisplay.Name = "picDisplay";
            this.picDisplay.Size = new System.Drawing.Size(594, 511);
            this.picDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picDisplay.TabIndex = 0;
            this.picDisplay.TabStop = false;
            this.picDisplay.MouseEnter += new System.EventHandler(this.picDisplay_MouseEnter);
            this.picDisplay.MouseLeave += new System.EventHandler(this.picDisplay_MouseLeave);
            this.picDisplay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picDisplay_MouseMove);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(144, 19);
            this.label7.TabIndex = 1;
            this.label7.Text = "Transition Type：";
            // 
            // cboTransitionType
            // 
            this.cboTransitionType.FormattingEnabled = true;
            this.cboTransitionType.Location = new System.Drawing.Point(166, 20);
            this.cboTransitionType.Name = "cboTransitionType";
            this.cboTransitionType.Size = new System.Drawing.Size(101, 27);
            this.cboTransitionType.TabIndex = 2;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 65);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(101, 19);
            this.label8.TabIndex = 1;
            this.label8.Text = "Thickness：";
            // 
            // nudSamplingStep
            // 
            this.nudSamplingStep.DecimalPlaces = 3;
            this.nudSamplingStep.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudSamplingStep.Location = new System.Drawing.Point(166, 100);
            this.nudSamplingStep.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudSamplingStep.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this.nudSamplingStep.Name = "nudSamplingStep";
            this.nudSamplingStep.Size = new System.Drawing.Size(101, 30);
            this.nudSamplingStep.TabIndex = 3;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(285, 24);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(101, 19);
            this.label9.TabIndex = 1;
            this.label9.Text = "Threshold：";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(285, 65);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(140, 19);
            this.label10.TabIndex = 1;
            this.label10.Text = "Min Amplitude：";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(285, 106);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(98, 19);
            this.label11.TabIndex = 1;
            this.label11.Text = "Min Area：";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 106);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(134, 19);
            this.label12.TabIndex = 1;
            this.label12.Text = "Sampling Step：";
            // 
            // nudThickness
            // 
            this.nudThickness.Location = new System.Drawing.Point(166, 59);
            this.nudThickness.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudThickness.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudThickness.Name = "nudThickness";
            this.nudThickness.Size = new System.Drawing.Size(101, 30);
            this.nudThickness.TabIndex = 3;
            this.nudThickness.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // nudThreshold
            // 
            this.nudThreshold.Location = new System.Drawing.Point(423, 18);
            this.nudThreshold.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudThreshold.Name = "nudThreshold";
            this.nudThreshold.Size = new System.Drawing.Size(101, 30);
            this.nudThreshold.TabIndex = 3;
            // 
            // nudMinAmplitude
            // 
            this.nudMinAmplitude.Location = new System.Drawing.Point(423, 59);
            this.nudMinAmplitude.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudMinAmplitude.Name = "nudMinAmplitude";
            this.nudMinAmplitude.Size = new System.Drawing.Size(101, 30);
            this.nudMinAmplitude.TabIndex = 3;
            // 
            // nudMinArea
            // 
            this.nudMinArea.Location = new System.Drawing.Point(423, 100);
            this.nudMinArea.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudMinArea.Name = "nudMinArea";
            this.nudMinArea.Size = new System.Drawing.Size(101, 30);
            this.nudMinArea.TabIndex = 3;
            // 
            // dgvSampleTable
            // 
            this.dgvSampleTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSampleTable.Location = new System.Drawing.Point(6, 193);
            this.dgvSampleTable.Name = "dgvSampleTable";
            this.dgvSampleTable.RowTemplate.Height = 24;
            this.dgvSampleTable.Size = new System.Drawing.Size(528, 192);
            this.dgvSampleTable.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(37, 150);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 19);
            this.label6.TabIndex = 2;
            this.label6.Text = "Angle：";
            // 
            // txtResults_Angle
            // 
            this.txtResults_Angle.Location = new System.Drawing.Point(136, 145);
            this.txtResults_Angle.Name = "txtResults_Angle";
            this.txtResults_Angle.Size = new System.Drawing.Size(76, 30);
            this.txtResults_Angle.TabIndex = 8;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(37, 42);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(94, 19);
            this.label14.TabIndex = 4;
            this.label14.Text = "Center X：";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(37, 96);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(94, 19);
            this.label13.TabIndex = 3;
            this.label13.Text = "Center Y：";
            // 
            // txtResults_CenterX
            // 
            this.txtResults_CenterX.Location = new System.Drawing.Point(136, 37);
            this.txtResults_CenterX.Name = "txtResults_CenterX";
            this.txtResults_CenterX.Size = new System.Drawing.Size(76, 30);
            this.txtResults_CenterX.TabIndex = 8;
            // 
            // txtResults_CenterY
            // 
            this.txtResults_CenterY.Location = new System.Drawing.Point(136, 91);
            this.txtResults_CenterY.Name = "txtResults_CenterY";
            this.txtResults_CenterY.Size = new System.Drawing.Size(76, 30);
            this.txtResults_CenterY.TabIndex = 8;
            // 
            // btnTrain
            // 
            this.btnTrain.Location = new System.Drawing.Point(423, 147);
            this.btnTrain.Name = "btnTrain";
            this.btnTrain.Size = new System.Drawing.Size(101, 30);
            this.btnTrain.TabIndex = 5;
            this.btnTrain.Text = "OK";
            this.btnTrain.UseVisualStyleBackColor = true;
            this.btnTrain.Click += new System.EventHandler(this.btnTrain_Click);
            // 
            // frmFindLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 561);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmFindLine";
            this.Text = "frmFindLine";
            this.Load += new System.EventHandler(this.frmFindLine_Load);
            this.groupBox3.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosition_Angle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosition_Length)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosition_Tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosition_CenterY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPosition_CenterX)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSamplingStep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudThickness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinAmplitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinArea)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSampleTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox picDisplay;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nudPosition_Angle;
        private System.Windows.Forms.NumericUpDown nudPosition_Length;
        private System.Windows.Forms.NumericUpDown nudPosition_Tolerance;
        private System.Windows.Forms.NumericUpDown nudPosition_CenterY;
        private System.Windows.Forms.NumericUpDown nudPosition_CenterX;
        private System.Windows.Forms.Button btnSetROIEnd;
        private System.Windows.Forms.Button btnSetROIStart;
        private System.Windows.Forms.ComboBox cboTransitionType;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown nudSamplingStep;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown nudThickness;
        private System.Windows.Forms.NumericUpDown nudMinArea;
        private System.Windows.Forms.NumericUpDown nudMinAmplitude;
        private System.Windows.Forms.NumericUpDown nudThreshold;
        private System.Windows.Forms.DataGridView dgvSampleTable;
        private System.Windows.Forms.Button btnTrain;
        private System.Windows.Forms.TextBox txtResults_Angle;
        private System.Windows.Forms.TextBox txtResults_CenterY;
        private System.Windows.Forms.TextBox txtResults_CenterX;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
    }
}