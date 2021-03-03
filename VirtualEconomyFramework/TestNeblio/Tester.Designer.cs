namespace TestNeblio
{
    partial class Tester
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
            this.split = new System.Windows.Forms.SplitContainer();
            this.cboConnection = new System.Windows.Forms.ComboBox();
            this.Tests = new System.Windows.Forms.DataGridView();
            this.function = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.parameters = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Action = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Console = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.split)).BeginInit();
            this.split.Panel1.SuspendLayout();
            this.split.Panel2.SuspendLayout();
            this.split.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Tests)).BeginInit();
            this.SuspendLayout();
            // 
            // split
            // 
            this.split.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split.Location = new System.Drawing.Point(0, 0);
            this.split.Name = "split";
            this.split.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // split.Panel1
            // 
            this.split.Panel1.Controls.Add(this.cboConnection);
            this.split.Panel1.Controls.Add(this.Tests);
            // 
            // split.Panel2
            // 
            this.split.Panel2.Controls.Add(this.Console);
            this.split.Size = new System.Drawing.Size(973, 575);
            this.split.SplitterDistance = 436;
            this.split.SplitterWidth = 6;
            this.split.TabIndex = 0;
            // 
            // cboConnection
            // 
            this.cboConnection.Dock = System.Windows.Forms.DockStyle.Top;
            this.cboConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cboConnection.FormattingEnabled = true;
            this.cboConnection.ItemHeight = 16;
            this.cboConnection.Location = new System.Drawing.Point(0, 0);
            this.cboConnection.Name = "cboConnection";
            this.cboConnection.Size = new System.Drawing.Size(973, 24);
            this.cboConnection.TabIndex = 1;
            // 
            // Tests
            // 
            this.Tests.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Tests.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Tests.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.function,
            this.parameters,
            this.Action});
            this.Tests.Location = new System.Drawing.Point(2, 41);
            this.Tests.Name = "Tests";
            this.Tests.Size = new System.Drawing.Size(971, 393);
            this.Tests.TabIndex = 0;
            this.Tests.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Tests_CellContentClick);
            // 
            // function
            // 
            this.function.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.function.HeaderText = "Test Name";
            this.function.MaxDropDownItems = 32;
            this.function.Name = "function";
            this.function.Width = 180;
            // 
            // parameters
            // 
            this.parameters.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.parameters.HeaderText = "Parameters";
            this.parameters.Name = "parameters";
            // 
            // Action
            // 
            this.Action.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Action.HeaderText = "Action";
            this.Action.Name = "Action";
            this.Action.Width = 50;
            // 
            // Console
            // 
            this.Console.BackColor = System.Drawing.Color.Black;
            this.Console.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Console.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Console.ForeColor = System.Drawing.Color.White;
            this.Console.Location = new System.Drawing.Point(0, 0);
            this.Console.Multiline = true;
            this.Console.Name = "Console";
            this.Console.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Console.Size = new System.Drawing.Size(973, 133);
            this.Console.TabIndex = 0;
            // 
            // Tester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(973, 575);
            this.Controls.Add(this.split);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "Tester";
            this.Text = "Universal Tester";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Tester_FormClosing);
            this.Shown += new System.EventHandler(this.Tester_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Tester_KeyDown);
            this.split.Panel1.ResumeLayout(false);
            this.split.Panel2.ResumeLayout(false);
            this.split.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split)).EndInit();
            this.split.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Tests)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer split;
        private System.Windows.Forms.TextBox Console;
        private System.Windows.Forms.DataGridView Tests;
        private System.Windows.Forms.ComboBox cboConnection;
        private System.Windows.Forms.DataGridViewComboBoxColumn function;
        private System.Windows.Forms.DataGridViewTextBoxColumn parameters;
        private System.Windows.Forms.DataGridViewButtonColumn Action;
    }
}

