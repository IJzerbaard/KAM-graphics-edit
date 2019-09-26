namespace KAM_Graphics_Editor
{
    partial class BuildingAnimEditor
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
            this.components = new System.ComponentModel.Container();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.checkBoxShowBuilding = new System.Windows.Forms.CheckBox();
            this.checkBoxFlags = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 16;
            this.listBox1.Location = new System.Drawing.Point(0, 0);
            this.listBox1.MaximumSize = new System.Drawing.Size(166, 330);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(166, 330);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.ListBox1_SelectedIndexChanged);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Top;
            this.propertyGrid1.Location = new System.Drawing.Point(166, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(403, 172);
            this.propertyGrid1.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(166, 170);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(400, 400);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // checkBoxShowBuilding
            // 
            this.checkBoxShowBuilding.AutoSize = true;
            this.checkBoxShowBuilding.Checked = true;
            this.checkBoxShowBuilding.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowBuilding.Location = new System.Drawing.Point(12, 336);
            this.checkBoxShowBuilding.Name = "checkBoxShowBuilding";
            this.checkBoxShowBuilding.Size = new System.Drawing.Size(117, 21);
            this.checkBoxShowBuilding.TabIndex = 3;
            this.checkBoxShowBuilding.Text = "Show building";
            this.checkBoxShowBuilding.UseVisualStyleBackColor = true;
            // 
            // checkBoxFlags
            // 
            this.checkBoxFlags.AutoSize = true;
            this.checkBoxFlags.Location = new System.Drawing.Point(12, 363);
            this.checkBoxFlags.Name = "checkBoxFlags";
            this.checkBoxFlags.Size = new System.Drawing.Size(64, 21);
            this.checkBoxFlags.TabIndex = 4;
            this.checkBoxFlags.Text = "Flags";
            this.checkBoxFlags.UseVisualStyleBackColor = true;
            // 
            // BuildingAnimEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 572);
            this.Controls.Add(this.checkBoxFlags);
            this.Controls.Add(this.checkBoxShowBuilding);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.listBox1);
            this.Name = "BuildingAnimEditor";
            this.Text = "BuildingAnimEditor";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox checkBoxShowBuilding;
        private System.Windows.Forms.CheckBox checkBoxFlags;
    }
}