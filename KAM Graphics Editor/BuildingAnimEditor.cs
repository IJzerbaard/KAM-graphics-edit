using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KAM_Graphics_Editor
{
    public partial class BuildingAnimEditor : Form
    {
        public BuildingAnimEditor()
        {
            InitializeComponent();
        }

        WorkAnimationsList AnimList;

        internal void SetList(WorkAnimationsList foo)
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
    }
}
