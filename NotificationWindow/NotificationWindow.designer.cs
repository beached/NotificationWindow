namespace NotificationWindow {
	partial class NotificationWindow {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if( disposing && (components != null) ) {
				components.Dispose( );
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent( ) {
			this.dgvMessages = new System.Windows.Forms.DataGridView();
			((System.ComponentModel.ISupportInitialize)(this.dgvMessages)).BeginInit();
			this.SuspendLayout();
			// 
			// dgvMessages
			// 
			this.dgvMessages.AllowUserToAddRows = false;
			this.dgvMessages.AllowUserToDeleteRows = false;
			this.dgvMessages.AllowUserToResizeColumns = false;
			this.dgvMessages.AllowUserToResizeRows = false;
			this.dgvMessages.BackgroundColor = System.Drawing.SystemColors.Control;
			this.dgvMessages.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dgvMessages.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
			this.dgvMessages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvMessages.ColumnHeadersVisible = false;
			this.dgvMessages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvMessages.Location = new System.Drawing.Point(0, 0);
			this.dgvMessages.Name = "dgvMessages";
			this.dgvMessages.ReadOnly = true;
			this.dgvMessages.ShowCellErrors = false;
			this.dgvMessages.ShowCellToolTips = false;
			this.dgvMessages.ShowEditingIcon = false;
			this.dgvMessages.ShowRowErrors = false;
			this.dgvMessages.Size = new System.Drawing.Size(325, 150);
			this.dgvMessages.TabIndex = 0;
			// 
			// NotificationWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(325, 150);
			this.ControlBox = false;
			this.Controls.Add(this.dgvMessages);
			this.ForeColor = System.Drawing.Color.Black;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(325, 150);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(325, 150);
			this.Name = "NotificationWindow";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "NotificationWindow";
			this.Load += new System.EventHandler(this.NotificationWindow_Load);
			this.Shown += new System.EventHandler(this.NotificationWindow_Shown);
			((System.ComponentModel.ISupportInitialize)(this.dgvMessages)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView dgvMessages;

	}
}