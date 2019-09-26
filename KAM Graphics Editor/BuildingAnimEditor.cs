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
    public partial class BuildingAnimEditor : Form
    {
        internal BuildingAnimEditor(WorkAnimationsList foo)
        {
            InitializeComponent();
            SetList(foo);
            timer1.Enabled = true;
        }

        WorkAnimationsList AnimList;

        void SetList(WorkAnimationsList foo)
        {
            AnimList = foo;
            listBox1.SuspendLayout();
            listBox1.Items.Clear();
            foreach (var item in foo.list)
                listBox1.Items.Add(item.Name);
            listBox1.SelectedIndex = 0;
            listBox1.ResumeLayout();
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = AnimList.list[listBox1.SelectedIndex];
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
            dp.Stone = checkBoxShowBuilding.Checked;
            dp.Flags = checkBoxFlags.Checked;

            switch (listBox1.SelectedIndex)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    dp.WorkIndex = listBox1.SelectedIndex + 2;
                    break;
                case 5:
                    dp.Smoke = true;
                    break;
                case 7:
                    dp.WorkIndex = 1;
                    break;
                case 6:
                default:
                    // manual drawing
                    AnimList.owner.Draw(dp, d, time);
                    var anim = AnimList.list[listBox1.SelectedIndex];
                    if (anim.SpriteCount != 0)
                        Form1.drawSprite(d, 2, anim.SpriteList[time % anim.SpriteCount], anim.Offset.X, anim.Offset.Y);
                    goto skip;
            }
            AnimList.owner.Draw(dp, d, time);
        skip:

            bm.UnlockBits(d);
            pictureBox1.Image = bm;
            time++;
        }
    }
}
