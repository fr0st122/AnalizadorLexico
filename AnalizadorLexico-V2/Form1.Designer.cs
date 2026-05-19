namespace AnalizadorLexico_V2
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null!;

        private System.Windows.Forms.TableLayoutPanel layoutRoot = null!;
        private System.Windows.Forms.Panel pnlHeader = null!;
        private System.Windows.Forms.Panel pnlTitle = null!;
        private System.Windows.Forms.Label lblTitle = null!;
        private System.Windows.Forms.Label lblSubtitle = null!;
        private System.Windows.Forms.Panel pnlLanguage = null!;
        private System.Windows.Forms.Label lblLanguageTitle = null!;
        private System.Windows.Forms.SplitContainer splitMain = null!;
        private System.Windows.Forms.TableLayoutPanel layoutLeft = null!;
        private System.Windows.Forms.GroupBox grpEntrada = null!;
        private System.Windows.Forms.FlowLayoutPanel pnlActions = null!;
        private System.Windows.Forms.Button btnArchivo = null!;
        private System.Windows.Forms.Button btnAnalizar = null!;
        private System.Windows.Forms.Button btnExport = null!;
        private System.Windows.Forms.TableLayoutPanel layoutRight = null!;
        private System.Windows.Forms.GroupBox grpResumen = null!;
        private System.Windows.Forms.GroupBox grpDiagnosticos = null!;
        private System.Windows.Forms.GroupBox grpTokens = null!;
        private System.Windows.Forms.TextBox txtEntrada = null!;
        private System.Windows.Forms.TextBox txtResumen = null!;
        private System.Windows.Forms.TextBox txtDiagnosticos = null!;
        private System.Windows.Forms.DataGridView dgvTokens = null!;
        private System.Windows.Forms.Label lblLanguageValue = null!;
        private System.Windows.Forms.ComboBox cmbLanguage = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            layoutRoot = new TableLayoutPanel();
            pnlHeader = new Panel();
            pnlTitle = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            pnlLanguage = new Panel();
            lblLanguageTitle = new Label();
            lblLanguageValue = new Label();
            cmbLanguage = new ComboBox();
            splitMain = new SplitContainer();
            layoutLeft = new TableLayoutPanel();
            grpEntrada = new GroupBox();
            txtEntrada = new TextBox();
            pnlActions = new FlowLayoutPanel();
            btnExport = new Button();
            btnAnalizar = new Button();
            btnArchivo = new Button();
            layoutRight = new TableLayoutPanel();
            grpResumen = new GroupBox();
            txtResumen = new TextBox();
            grpDiagnosticos = new GroupBox();
            txtDiagnosticos = new TextBox();
            grpTokens = new GroupBox();
            dgvTokens = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            grpEntrada.SuspendLayout();
            layoutRight.SuspendLayout();
            grpResumen.SuspendLayout();
            grpDiagnosticos.SuspendLayout();
            grpTokens.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTokens).BeginInit();
            SuspendLayout();
            // 
            // layoutRoot
            // 
            layoutRoot.BackColor = Color.FromArgb(20, 22, 29);
            layoutRoot.ColumnCount = 1;
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutRoot.Controls.Add(pnlHeader, 0, 0);
            layoutRoot.Controls.Add(splitMain, 0, 1);
            layoutRoot.Controls.Add(grpTokens, 0, 2);
            layoutRoot.Dock = DockStyle.Fill;
            layoutRoot.Location = new Point(0, 0);
            layoutRoot.Name = "layoutRoot";
            layoutRoot.Padding = new Padding(12);
            layoutRoot.RowCount = 3;
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            layoutRoot.Size = new Size(1280, 840);
            layoutRoot.TabIndex = 0;
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(33, 38, 52);
            pnlHeader.Controls.Add(pnlTitle);
            pnlHeader.Controls.Add(pnlLanguage);
            pnlHeader.Dock = DockStyle.Fill;
            pnlHeader.Location = new Point(12, 12);
            pnlHeader.Margin = new Padding(0, 0, 0, 10);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(14, 10, 14, 10);
            pnlHeader.Size = new Size(1256, 90);
            pnlHeader.TabIndex = 0;
            // 
            // pnlTitle
            // 
            pnlTitle.Controls.Add(lblSubtitle);
            pnlTitle.Controls.Add(lblTitle);
            pnlTitle.Dock = DockStyle.Fill;
            pnlTitle.Location = new Point(14, 10);
            pnlTitle.Name = "pnlTitle";
            pnlTitle.Padding = new Padding(4, 2, 8, 2);
            pnlTitle.Size = new Size(928, 70);
            pnlTitle.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.WhiteSmoke;
            lblTitle.Location = new Point(4, 2);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(350, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Analizador Léxico y Semántico";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblSubtitle.ForeColor = Color.Gainsboro;
            lblSubtitle.Location = new Point(6, 39);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(671, 19);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Selecciona el lenguaje manualmente, resume la lógica principal y muestra tokens con diagnósticos.";
            // 
            // pnlLanguage
            // 
            pnlLanguage.BackColor = Color.FromArgb(43, 50, 66);
            pnlLanguage.Controls.Add(cmbLanguage);
            pnlLanguage.Controls.Add(lblLanguageValue);
            pnlLanguage.Controls.Add(lblLanguageTitle);
            pnlLanguage.Dock = DockStyle.Right;
            pnlLanguage.Location = new Point(942, 10);
            pnlLanguage.Name = "pnlLanguage";
            pnlLanguage.Padding = new Padding(16, 10, 16, 10);
            pnlLanguage.Size = new Size(300, 70);
            pnlLanguage.TabIndex = 1;
            // 
            // lblLanguageTitle
            // 
            lblLanguageTitle.AutoSize = true;
            lblLanguageTitle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblLanguageTitle.ForeColor = Color.Gainsboro;
            lblLanguageTitle.Location = new Point(16, 10);
            lblLanguageTitle.Name = "lblLanguageTitle";
            lblLanguageTitle.Size = new Size(112, 17);
            lblLanguageTitle.TabIndex = 0;
            lblLanguageTitle.Text = "Lenguaje manual";
            // 
            // lblLanguageValue
            // 
            lblLanguageValue.AutoSize = true;
            lblLanguageValue.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblLanguageValue.ForeColor = Color.WhiteSmoke;
            lblLanguageValue.Location = new Point(161, 11);
            lblLanguageValue.Name = "lblLanguageValue";
            lblLanguageValue.Size = new Size(17, 15);
            lblLanguageValue.TabIndex = 1;
            lblLanguageValue.Text = "C#";
            // 
            // cmbLanguage
            // 
            cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanguage.FormattingEnabled = true;
            cmbLanguage.Items.AddRange(new object[] { "Kotlin", "C#", "Java", "Texto" });
            cmbLanguage.Location = new Point(16, 34);
            cmbLanguage.Name = "cmbLanguage";
            cmbLanguage.Size = new Size(268, 23);
            cmbLanguage.TabIndex = 2;
            cmbLanguage.SelectedIndex = 1;
            cmbLanguage.SelectedIndexChanged += cmbLanguage_SelectedIndexChanged;
            // 
            // splitMain
            // 
            splitMain.BackColor = Color.FromArgb(20, 22, 29);
            splitMain.Dock = DockStyle.Fill;
            splitMain.Location = new Point(12, 112);
            splitMain.Margin = new Padding(0, 0, 0, 10);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(layoutLeft);
            splitMain.Panel1MinSize = 360;
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(layoutRight);
            splitMain.Panel2MinSize = 300;
            splitMain.Size = new Size(1256, 402);
            splitMain.SplitterDistance = 700;
            splitMain.SplitterWidth = 8;
            splitMain.TabIndex = 1;
            // 
            // layoutLeft
            // 
            layoutLeft.ColumnCount = 1;
            layoutLeft.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutLeft.Controls.Add(grpEntrada, 0, 0);
            layoutLeft.Controls.Add(pnlActions, 0, 1);
            layoutLeft.Dock = DockStyle.Fill;
            layoutLeft.Location = new Point(0, 0);
            layoutLeft.Name = "layoutLeft";
            layoutLeft.RowCount = 2;
            layoutLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutLeft.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            layoutLeft.Size = new Size(700, 402);
            layoutLeft.TabIndex = 0;
            // 
            // grpEntrada
            // 
            grpEntrada.Controls.Add(txtEntrada);
            grpEntrada.Dock = DockStyle.Fill;
            grpEntrada.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpEntrada.ForeColor = Color.Gainsboro;
            grpEntrada.Location = new Point(0, 0);
            grpEntrada.Margin = new Padding(0, 0, 0, 10);
            grpEntrada.Name = "grpEntrada";
            grpEntrada.Padding = new Padding(12, 30, 12, 12);
            grpEntrada.Size = new Size(700, 334);
            grpEntrada.TabIndex = 0;
            grpEntrada.TabStop = false;
            grpEntrada.Text = "Entrada";
            // 
            // txtEntrada
            // 
            txtEntrada.AcceptsTab = true;
            txtEntrada.BorderStyle = BorderStyle.FixedSingle;
            txtEntrada.Dock = DockStyle.Fill;
            txtEntrada.Location = new Point(12, 30);
            txtEntrada.Multiline = true;
            txtEntrada.Name = "txtEntrada";
            txtEntrada.ScrollBars = ScrollBars.Both;
            txtEntrada.Size = new Size(676, 292);
            txtEntrada.TabIndex = 0;
            txtEntrada.WordWrap = false;
            // 
            // pnlActions
            // 
            pnlActions.Controls.Add(btnExport);
            pnlActions.Controls.Add(btnAnalizar);
            pnlActions.Controls.Add(btnArchivo);
            pnlActions.Dock = DockStyle.Fill;
            pnlActions.FlowDirection = FlowDirection.RightToLeft;
            pnlActions.Location = new Point(0, 344);
            pnlActions.Margin = new Padding(0);
            pnlActions.Name = "pnlActions";
            pnlActions.Size = new Size(700, 58);
            pnlActions.TabIndex = 1;
            pnlActions.WrapContents = false;
            // 
            // btnExport
            // 
            btnExport.Location = new Point(532, 6);
            btnExport.Margin = new Padding(6);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(162, 40);
            btnExport.TabIndex = 2;
            btnExport.Text = "Exportar tokens";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // btnAnalizar
            // 
            btnAnalizar.Location = new Point(393, 6);
            btnAnalizar.Margin = new Padding(6);
            btnAnalizar.Name = "btnAnalizar";
            btnAnalizar.Size = new Size(127, 40);
            btnAnalizar.TabIndex = 1;
            btnAnalizar.Text = "Analizar";
            btnAnalizar.UseVisualStyleBackColor = true;
            btnAnalizar.Click += btnAnalizar_Click;
            // 
            // btnArchivo
            // 
            btnArchivo.Location = new Point(236, 6);
            btnArchivo.Margin = new Padding(6);
            btnArchivo.Name = "btnArchivo";
            btnArchivo.Size = new Size(145, 40);
            btnArchivo.TabIndex = 0;
            btnArchivo.Text = "Cargar archivo";
            btnArchivo.UseVisualStyleBackColor = true;
            btnArchivo.Click += btnArchivo_Click;
            // 
            // layoutRight
            // 
            layoutRight.ColumnCount = 1;
            layoutRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutRight.Controls.Add(grpResumen, 0, 0);
            layoutRight.Controls.Add(grpDiagnosticos, 0, 1);
            layoutRight.Dock = DockStyle.Fill;
            layoutRight.Location = new Point(0, 0);
            layoutRight.Name = "layoutRight";
            layoutRight.RowCount = 2;
            layoutRight.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layoutRight.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layoutRight.Size = new Size(548, 402);
            layoutRight.TabIndex = 0;
            // 
            // grpResumen
            // 
            grpResumen.Controls.Add(txtResumen);
            grpResumen.Dock = DockStyle.Fill;
            grpResumen.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpResumen.ForeColor = Color.Gainsboro;
            grpResumen.Location = new Point(0, 0);
            grpResumen.Margin = new Padding(0, 0, 0, 10);
            grpResumen.Name = "grpResumen";
            grpResumen.Padding = new Padding(12, 30, 12, 12);
            grpResumen.Size = new Size(548, 191);
            grpResumen.TabIndex = 0;
            grpResumen.TabStop = false;
            grpResumen.Text = "Resumen del código";
            // 
            // txtResumen
            // 
            txtResumen.BorderStyle = BorderStyle.FixedSingle;
            txtResumen.Dock = DockStyle.Fill;
            txtResumen.Location = new Point(12, 30);
            txtResumen.Multiline = true;
            txtResumen.Name = "txtResumen";
            txtResumen.ReadOnly = true;
            txtResumen.ScrollBars = ScrollBars.Vertical;
            txtResumen.Size = new Size(524, 149);
            txtResumen.TabIndex = 0;
            // 
            // grpDiagnosticos
            // 
            grpDiagnosticos.Controls.Add(txtDiagnosticos);
            grpDiagnosticos.Dock = DockStyle.Fill;
            grpDiagnosticos.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpDiagnosticos.ForeColor = Color.Gainsboro;
            grpDiagnosticos.Location = new Point(0, 211);
            grpDiagnosticos.Margin = new Padding(0);
            grpDiagnosticos.Name = "grpDiagnosticos";
            grpDiagnosticos.Padding = new Padding(12, 30, 12, 12);
            grpDiagnosticos.Size = new Size(548, 191);
            grpDiagnosticos.TabIndex = 1;
            grpDiagnosticos.TabStop = false;
            grpDiagnosticos.Text = "Diagnósticos";
            // 
            // txtDiagnosticos
            // 
            txtDiagnosticos.BorderStyle = BorderStyle.FixedSingle;
            txtDiagnosticos.Dock = DockStyle.Fill;
            txtDiagnosticos.Location = new Point(12, 30);
            txtDiagnosticos.Multiline = true;
            txtDiagnosticos.Name = "txtDiagnosticos";
            txtDiagnosticos.ReadOnly = true;
            txtDiagnosticos.ScrollBars = ScrollBars.Vertical;
            txtDiagnosticos.Size = new Size(524, 149);
            txtDiagnosticos.TabIndex = 0;
            // 
            // grpTokens
            // 
            grpTokens.Controls.Add(dgvTokens);
            grpTokens.Dock = DockStyle.Fill;
            grpTokens.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpTokens.ForeColor = Color.Gainsboro;
            grpTokens.Location = new Point(12, 534);
            grpTokens.Margin = new Padding(0);
            grpTokens.Name = "grpTokens";
            grpTokens.Padding = new Padding(12, 30, 12, 12);
            grpTokens.Size = new Size(1256, 294);
            grpTokens.TabIndex = 2;
            grpTokens.TabStop = false;
            grpTokens.Text = "Tokens detectados";
            // 
            // dgvTokens
            // 
            dgvTokens.AllowUserToAddRows = false;
            dgvTokens.AllowUserToDeleteRows = false;
            dgvTokens.AllowUserToResizeRows = false;
            dgvTokens.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTokens.Dock = DockStyle.Fill;
            dgvTokens.Location = new Point(12, 30);
            dgvTokens.MultiSelect = false;
            dgvTokens.Name = "dgvTokens";
            dgvTokens.ReadOnly = true;
            dgvTokens.RowHeadersVisible = false;
            dgvTokens.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTokens.Size = new Size(1232, 252);
            dgvTokens.TabIndex = 0;
            dgvTokens.CellFormatting += dgvTokens_CellFormatting;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1280, 840);
            Controls.Add(layoutRoot);
            MinimumSize = new Size(1100, 760);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Analizador Léxico V2";
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            grpEntrada.ResumeLayout(false);
            grpEntrada.PerformLayout();
            layoutRight.ResumeLayout(false);
            grpResumen.ResumeLayout(false);
            grpResumen.PerformLayout();
            grpDiagnosticos.ResumeLayout(false);
            grpDiagnosticos.PerformLayout();
            grpTokens.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTokens).EndInit();
            ResumeLayout(false);
        }
    }
}
