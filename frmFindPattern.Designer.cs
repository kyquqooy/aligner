namespace prjVisionController
{
    partial class frmFindPattern
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLoadTrainImage = new System.Windows.Forms.Button();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.picTrainPattern = new System.Windows.Forms.PictureBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnSettingROI = new System.Windows.Forms.Button();
            this.cboFindMode = new System.Windows.Forms.ComboBox();
            this.nudScaleTolerance = new System.Windows.Forms.NumericUpDown();
            this.nudAngleTolerance = new System.Windows.Forms.NumericUpDown();
            this.nudFindScore = new System.Windows.Forms.NumericUpDown();
            this.nudFindCount = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnFindPattern = new System.Windows.Forms.Button();
            this.btnTrainImage = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.picDisplay1 = new System.Windows.Forms.PictureBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.picTrainPattern)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScaleTolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAngleTolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFindScore)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFindCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoadTrainImage
            // 
            this.btnLoadTrainImage.Location = new System.Drawing.Point(8, 196);
            this.btnLoadTrainImage.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoadTrainImage.Name = "btnLoadTrainImage";
            this.btnLoadTrainImage.Size = new System.Drawing.Size(81, 40);
            this.btnLoadTrainImage.TabIndex = 1;
            this.btnLoadTrainImage.Text = "載入影像";
            this.btnLoadTrainImage.UseVisualStyleBackColor = true;
            this.btnLoadTrainImage.Click += new System.EventHandler(this.btnLoadTrainImage_Click);
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
            // picTrainPattern
            // 
            this.picTrainPattern.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picTrainPattern.Location = new System.Drawing.Point(8, 8);
            this.picTrainPattern.Margin = new System.Windows.Forms.Padding(4);
            this.picTrainPattern.Name = "picTrainPattern";
            this.picTrainPattern.Size = new System.Drawing.Size(256, 180);
            this.picTrainPattern.TabIndex = 0;
            this.picTrainPattern.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 23);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(548, 421);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnSettingROI);
            this.tabPage1.Controls.Add(this.cboFindMode);
            this.tabPage1.Controls.Add(this.nudScaleTolerance);
            this.tabPage1.Controls.Add(this.nudAngleTolerance);
            this.tabPage1.Controls.Add(this.nudFindScore);
            this.tabPage1.Controls.Add(this.nudFindCount);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.btnFindPattern);
            this.tabPage1.Controls.Add(this.btnTrainImage);
            this.tabPage1.Controls.Add(this.btnLoadTrainImage);
            this.tabPage1.Controls.Add(this.dataGridView1);
            this.tabPage1.Controls.Add(this.picTrainPattern);
            this.tabPage1.Location = new System.Drawing.Point(4, 26);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(540, 391);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "PatternMatch";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnSettingROI
            // 
            this.btnSettingROI.Location = new System.Drawing.Point(96, 195);
            this.btnSettingROI.Name = "btnSettingROI";
            this.btnSettingROI.Size = new System.Drawing.Size(81, 40);
            this.btnSettingROI.TabIndex = 6;
            this.btnSettingROI.Text = "調整ROI";
            this.btnSettingROI.UseVisualStyleBackColor = true;
            this.btnSettingROI.Click += new System.EventHandler(this.btnSettingROI_Click);
            // 
            // cboFindMode
            // 
            this.cboFindMode.FormattingEnabled = true;
            this.cboFindMode.Location = new System.Drawing.Point(365, 79);
            this.cboFindMode.Name = "cboFindMode";
            this.cboFindMode.Size = new System.Drawing.Size(104, 24);
            this.cboFindMode.TabIndex = 5;
            // 
            // nudScaleTolerance
            // 
            this.nudScaleTolerance.DecimalPlaces = 1;
            this.nudScaleTolerance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudScaleTolerance.Location = new System.Drawing.Point(365, 150);
            this.nudScaleTolerance.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudScaleTolerance.Name = "nudScaleTolerance";
            this.nudScaleTolerance.Size = new System.Drawing.Size(67, 27);
            this.nudScaleTolerance.TabIndex = 4;
            this.nudScaleTolerance.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // nudAngleTolerance
            // 
            this.nudAngleTolerance.DecimalPlaces = 1;
            this.nudAngleTolerance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudAngleTolerance.Location = new System.Drawing.Point(365, 114);
            this.nudAngleTolerance.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.nudAngleTolerance.Name = "nudAngleTolerance";
            this.nudAngleTolerance.Size = new System.Drawing.Size(67, 27);
            this.nudAngleTolerance.TabIndex = 4;
            // 
            // nudFindScore
            // 
            this.nudFindScore.DecimalPlaces = 3;
            this.nudFindScore.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudFindScore.Location = new System.Drawing.Point(365, 42);
            this.nudFindScore.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFindScore.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.nudFindScore.Name = "nudFindScore";
            this.nudFindScore.Size = new System.Drawing.Size(67, 27);
            this.nudFindScore.TabIndex = 4;
            // 
            // nudFindCount
            // 
            this.nudFindCount.Location = new System.Drawing.Point(365, 6);
            this.nudFindCount.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudFindCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFindCount.Name = "nudFindCount";
            this.nudFindCount.Size = new System.Drawing.Size(67, 27);
            this.nudFindCount.TabIndex = 4;
            this.nudFindCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(271, 155);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 16);
            this.label5.TabIndex = 3;
            this.label5.Text = "縮放範圍：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(271, 119);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "角度範圍：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(271, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 16);
            this.label3.TabIndex = 3;
            this.label3.Text = "比對模式：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(271, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "查找分數：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(271, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 16);
            this.label1.TabIndex = 3;
            this.label1.Text = "尋找數量：";
            // 
            // btnFindPattern
            // 
            this.btnFindPattern.Location = new System.Drawing.Point(441, 196);
            this.btnFindPattern.Name = "btnFindPattern";
            this.btnFindPattern.Size = new System.Drawing.Size(92, 40);
            this.btnFindPattern.TabIndex = 2;
            this.btnFindPattern.Text = "特徵搜尋";
            this.btnFindPattern.UseVisualStyleBackColor = true;
            this.btnFindPattern.Click += new System.EventHandler(this.btnFindPattern_Click);
            // 
            // btnTrainImage
            // 
            this.btnTrainImage.Location = new System.Drawing.Point(184, 195);
            this.btnTrainImage.Margin = new System.Windows.Forms.Padding(4);
            this.btnTrainImage.Name = "btnTrainImage";
            this.btnTrainImage.Size = new System.Drawing.Size(81, 40);
            this.btnTrainImage.TabIndex = 1;
            this.btnTrainImage.Text = "訓練影像";
            this.btnTrainImage.UseVisualStyleBackColor = true;
            this.btnTrainImage.Click += new System.EventHandler(this.btnTrainImage_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(8, 243);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(525, 141);
            this.dataGridView1.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 26);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(540, 391);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Calibration";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.picDisplay1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(600, 537);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "操作畫面";
            // 
            // picDisplay1
            // 
            this.picDisplay1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picDisplay1.Location = new System.Drawing.Point(3, 23);
            this.picDisplay1.Margin = new System.Windows.Forms.Padding(4);
            this.picDisplay1.Name = "picDisplay1";
            this.picDisplay1.Size = new System.Drawing.Size(594, 511);
            this.picDisplay1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picDisplay1.TabIndex = 0;
            this.picDisplay1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnLoadImage);
            this.groupBox2.Controls.Add(this.btnLoad);
            this.groupBox2.Controls.Add(this.btnSave);
            this.groupBox2.Location = new System.Drawing.Point(618, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(554, 84);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "影像來源";
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
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tabControl1);
            this.groupBox3.Location = new System.Drawing.Point(618, 102);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(554, 447);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "影像工具";
            // 
            // frmFindPattern
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 561);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmFindPattern";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.frmFindPattern_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picTrainPattern)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudScaleTolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAngleTolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFindScore)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFindCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnLoadTrainImage;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.PictureBox picTrainPattern;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btnTrainImage;
        private System.Windows.Forms.Button btnFindPattern;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboFindMode;
        private System.Windows.Forms.NumericUpDown nudScaleTolerance;
        private System.Windows.Forms.NumericUpDown nudAngleTolerance;
        private System.Windows.Forms.NumericUpDown nudFindScore;
        private System.Windows.Forms.NumericUpDown nudFindCount;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.PictureBox picDisplay1;
        private System.Windows.Forms.Button btnSettingROI;
    }
}

