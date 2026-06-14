namespace personelizintakip
{
    partial class PersonelReport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PersonelReport));
            simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            groupBox1 = new GroupBox();
            label1 = new Label();
            checkedComboBoxEdit1 = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            checkButton1 = new DevExpress.XtraEditors.CheckButton();
            dataGridView1 = new DataGridView();
            groupBox2 = new GroupBox();
            simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)checkedComboBoxEdit1.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // simpleButton1
            // 
            simpleButton1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            simpleButton1.Appearance.Font = new Font("Tahoma", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 162);
            simpleButton1.Appearance.Options.UseFont = true;
            simpleButton1.ImageOptions.Image = (Image)resources.GetObject("simpleButton1.ImageOptions.Image");
            simpleButton1.Location = new Point(1184, 927);
            simpleButton1.Name = "simpleButton1";
            simpleButton1.Size = new Size(132, 48);
            simpleButton1.TabIndex = 0;
            simpleButton1.Text = "Raporla";
            simpleButton1.Click += simpleButton1_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(checkedComboBoxEdit1);
            groupBox1.Controls.Add(checkButton1);
            groupBox1.Location = new Point(42, 36);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(254, 136);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Raporlama Araçları";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(32, 26);
            label1.Name = "label1";
            label1.Size = new Size(133, 15);
            label1.TabIndex = 4;
            label1.Text = "Departman Filtre Seçimi";
            // 
            // checkedComboBoxEdit1
            // 
            checkedComboBoxEdit1.EditValue = "Departman";
            checkedComboBoxEdit1.Location = new Point(33, 48);
            checkedComboBoxEdit1.Name = "checkedComboBoxEdit1";
            checkedComboBoxEdit1.Properties.AllowMultiSelect = true;
            checkedComboBoxEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            checkedComboBoxEdit1.Properties.DropDownRows = 8;
            checkedComboBoxEdit1.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
            checkedComboBoxEdit1.Size = new Size(176, 20);
            checkedComboBoxEdit1.TabIndex = 3;
            // 
            // checkButton1
            // 
            checkButton1.Appearance.BackColor = Color.White;
            checkButton1.Appearance.BorderColor = Color.White;
            checkButton1.Appearance.ForeColor = Color.Black;
            checkButton1.Appearance.Options.UseBackColor = true;
            checkButton1.Appearance.Options.UseBorderColor = true;
            checkButton1.Appearance.Options.UseForeColor = true;
            checkButton1.AppearanceDisabled.BackColor = SystemColors.ButtonFace;
            checkButton1.AppearanceDisabled.BorderColor = Color.White;
            checkButton1.AppearanceDisabled.ForeColor = Color.Black;
            checkButton1.AppearanceDisabled.Options.UseBackColor = true;
            checkButton1.AppearanceDisabled.Options.UseBorderColor = true;
            checkButton1.AppearanceDisabled.Options.UseForeColor = true;
            checkButton1.AppearanceHovered.BackColor = Color.SaddleBrown;
            checkButton1.AppearanceHovered.BorderColor = Color.Tomato;
            checkButton1.AppearanceHovered.ForeColor = Color.Black;
            checkButton1.AppearanceHovered.Options.UseBackColor = true;
            checkButton1.AppearanceHovered.Options.UseBorderColor = true;
            checkButton1.AppearanceHovered.Options.UseForeColor = true;
            checkButton1.AppearancePressed.BackColor = Color.DarkRed;
            checkButton1.AppearancePressed.BorderColor = Color.Maroon;
            checkButton1.AppearancePressed.ForeColor = Color.White;
            checkButton1.AppearancePressed.Options.UseBackColor = true;
            checkButton1.AppearancePressed.Options.UseBorderColor = true;
            checkButton1.AppearancePressed.Options.UseForeColor = true;
            checkButton1.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("checkButton1.ImageOptions.SvgImage");
            checkButton1.Location = new Point(33, 82);
            checkButton1.LookAndFeel.SkinMaskColor = Color.FromArgb(0, 64, 0);
            checkButton1.LookAndFeel.SkinMaskColor2 = Color.Maroon;
            checkButton1.LookAndFeel.UseDefaultLookAndFeel = false;
            checkButton1.Name = "checkButton1";
            checkButton1.Size = new Size(176, 38);
            checkButton1.TabIndex = 2;
            checkButton1.Text = "İşten Ayrılanları Çıkart";
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(3, 19);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(1301, 646);
            dataGridView1.TabIndex = 3;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(dataGridView1);
            groupBox2.Location = new Point(12, 253);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(1307, 668);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Personel Listesi";
            // 
            // simpleButton2
            // 
            simpleButton2.Appearance.Font = new Font("Tahoma", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 162);
            simpleButton2.Appearance.Options.UseFont = true;
            simpleButton2.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("simpleButton2.ImageOptions.SvgImage");
            simpleButton2.Location = new Point(42, 187);
            simpleButton2.Name = "simpleButton2";
            simpleButton2.Size = new Size(110, 48);
            simpleButton2.TabIndex = 3;
            simpleButton2.Text = "Listele";
            simpleButton2.Click += simpleButton2_Click;
            // 
            // PersonelReport
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1331, 991);
            Controls.Add(simpleButton2);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(simpleButton1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "PersonelReport";
            Text = "PersonelReport";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)checkedComboBoxEdit1.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private GroupBox groupBox1;
        private DevExpress.XtraEditors.CheckButton checkButton1;
        private DevExpress.XtraEditors.CheckedComboBoxEdit checkedComboBoxEdit1;
        private DataGridView dataGridView1;
        private GroupBox groupBox2;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
        private Label label1;
    }
}