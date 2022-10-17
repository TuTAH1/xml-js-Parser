namespace xml_js_Parser_Updater
{
	partial class FormUpdater
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lbStatus = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.splMain = new System.Windows.Forms.SplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
			this.splMain.Panel1.SuspendLayout();
			this.splMain.Panel2.SuspendLayout();
			this.splMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// lbStatus
			// 
			this.lbStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbStatus.Location = new System.Drawing.Point(0, 0);
			this.lbStatus.Name = "lbStatus";
			this.lbStatus.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.lbStatus.Size = new System.Drawing.Size(585, 34);
			this.lbStatus.TabIndex = 0;
			this.lbStatus.Text = "Инициализация обновлятеля...";
			this.lbStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// progressBar1
			// 
			this.progressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.progressBar1.Location = new System.Drawing.Point(0, 0);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(585, 33);
			this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progressBar1.TabIndex = 1;
			// 
			// splMain
			// 
			this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splMain.Location = new System.Drawing.Point(0, 0);
			this.splMain.Name = "splMain";
			this.splMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splMain.Panel1
			// 
			this.splMain.Panel1.Controls.Add(this.lbStatus);
			// 
			// splMain.Panel2
			// 
			this.splMain.Panel2.Controls.Add(this.progressBar1);
			this.splMain.Size = new System.Drawing.Size(585, 71);
			this.splMain.SplitterDistance = 34;
			this.splMain.TabIndex = 2;
			// 
			// FormUpdater
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(585, 71);
			this.Controls.Add(this.splMain);
			this.Name = "FormUpdater";
			this.Text = "Обновлятель xml-js парсера";
			this.splMain.Panel1.ResumeLayout(false);
			this.splMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
			this.splMain.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Label lbStatus;
		private ProgressBar progressBar1;
		private SplitContainer splMain;
	}
}