using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KAM_Graphics_Editor
{
    public partial class UnitAnimEditor : Form
    {
        internal UnitAnimEditor(UnitActAnimationsList foo)
        {
            InitializeComponent();
            SetList(foo);
            timer1.Enabled = true;
        }

        UnitActAnimationsList AnimList;

        void SetList(UnitActAnimationsList foo)
        {
            AnimList = foo;
            listBox1.SuspendLayout();
            listBox1.Items.Clear();
            foreach (var item in foo.Acts)
                listBox1.Items.Add(item.Name);
            listBox1.SelectedIndex = 0;
            listBox1.ResumeLayout();
            listBox2.SelectedIndex = 0;
        }

        Bitmap bm = new Bitmap(400, 400);
        int time;

        private unsafe void Timer1_Tick(object sender, EventArgs e)
        {
            var d = bm.LockBits(new Rectangle(0, 0, 400, 400), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            for (int i = 0; i < d.Height; i++)
            {
                int* ptr = (int*)(d.Scan0 + i * d.Stride);
                for (int j = 0; j < d.Width; j++)
                    *ptr++ = 0;
            }

            DrawParams dp = new DrawParams();
            dp.SelectedAct = Math.Max(0, listBox1.SelectedIndex) * 8 + Math.Max(0, listBox2.SelectedIndex);
            dp.WholeUnit = checkBox1.Checked;

            AnimList.Owner.Draw(dp, d, time);

            bm.UnlockBits(d);
            pictureBox1.Image = bm;
            time++;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = AnimList.Acts[Math.Max(0, listBox1.SelectedIndex)];
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = AnimList.Acts[Math.Max(0, listBox1.SelectedIndex)];
        }
    }
}
