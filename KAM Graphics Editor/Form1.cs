using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace KAM_Graphics_Editor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }



        static Form1()
        {
            var t = @"Sawmill
Iron Smithy
Weapon Smithy
Coal Mine
Iron Mine
Gold Mine
Fisherman's Hut
Bakery
Farm
Woodcutter's
Armor Smithy
Storehouse
Stables
School
Quarry
Metallurgist's
Swine Farm
Watchtower
Town Hall
Weapons Workshop
Armor Workshop
Barracks
Mill
Vehicles Workshop
Butcher's
Tannery
null
Inn
Vineyard";
            BuildingNames = t.Split('\n');
            for (int i = 0; i < BuildingNames.Length; i++)
                BuildingNames[i] = BuildingNames[i].Trim();
        }

        public static string[] BuildingNames;
        string KAMDataFolder;

        void findKAMfolder()
        {
            string path = @"C:\Program Files (x86)\KaM - The Peasants Rebellion";
            if (Directory.Exists(path) &&
                Directory.Exists(Path.Combine(path, "data")))
                KAMDataFolder = path;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            findKAMfolder();
            if (KAMDataFolder == null)
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    string path = folderBrowserDialog1.SelectedPath;
                    if (Directory.Exists(path) &&
                        Directory.Exists(Path.Combine(path, "data")))
                        KAMDataFolder = path;
                }
            }

            if (KAMDataFolder == null)
            {
                if (MessageBox.Show("K&M data folder not found.", "Error", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    Application.Exit();
                    return;
                }
            }
            else
                label1.Text += "  " + KAMDataFolder;

            backgroundWorker1.RunWorkerAsync();
        }

        internal static int[] palette;

        void loadpalette()
        {
            var bm = Properties.Resources.pal;
            var d = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            palette = new int[256];
            System.Runtime.InteropServices.Marshal.Copy(d.Scan0 + 64 * d.Stride, palette, 0, 256);
            bm.UnlockBits(d);
        }

        List<IEntity> entities = new List<IEntity>();

        void loadHouses()
        {
            List<IEntity> entities = new List<IEntity>();
            string defines = Path.Combine(KAMDataFolder, "data", "defines", "houses.dat");
            using (var fs = File.OpenRead(defines))
            {
                BinaryReader r = new BinaryReader(fs);

                for (int i = 0; i < 30; i++)
                {
                    Animal a = new Animal("Animal " + i);
                    entities.Add(a);
                    a.RawOffset = r.BaseStream.Position;
                    a.Anim = r.ReadInt16s(30);
                    a.AnimLength = r.ReadUInt16();
                    a.Offset = new Point(r.ReadInt32(), r.ReadInt32());
                }

                animals = new List<Animal>(entities.Cast<Animal>());

                for (int i = 0; i < 29; i++)
                {
                    Building b = new Building(BuildingNames[i]);
                    entities.Add(b);
                    b.RawOffset = r.BaseStream.Position;
                    b.StoneTexture = r.ReadInt16();
                    b.WoodTexture = r.ReadInt16();
                    b.WoodenConstructionMaskTexture = r.ReadInt16();
                    b.StoneConstructionMaskTexture = r.ReadInt16();
                    b.SupplyResourcesIn = r.ReadInt16s(20);
                    b.SupplyResourcesOut = r.ReadInt16s(20);

                    for (int j = 0; j < 19; j++)
                    {
                        b.WorkAnim[j] = r.ReadInt16s(30);
                        b.WorkAnimLength[j] = r.ReadUInt16();
                        b.WorkAnimX[j] = r.ReadInt32();
                        b.WorkAnimY[j] = r.ReadInt32();
                    }

                    b.WoodSteps = r.ReadUInt16();
                    b.StoneSteps = r.ReadUInt16();
                    b.a1 = r.ReadUInt16();
                    b.EntranceOffsetX = r.ReadSByte();
                    b.EntranceOffsetY = r.ReadSByte();
                    b.EntranceOffsetPxX = r.ReadSByte();
                    b.EntranceOffsetPxY = r.ReadSByte();
                    b.BuildArea = r.ReadBytes(100);
                    b.WoodCost = r.ReadByte();
                    b.StoneCost = r.ReadByte();
                    b.BuildSupply = new Point[12];
                    for (int j = 0; j < 12; j++)
                    {
                        b.BuildSupply[j].X = r.ReadInt32();
                        b.BuildSupply[j].Y = r.ReadInt32();
                    }
                    b.a5 = r.ReadUInt16();
                    b.SizeArea = r.ReadUInt16();
                    b.SizeX = r.ReadSByte();
                    b.SizeY = r.ReadSByte();
                    b.SizeX2 = r.ReadSByte();
                    b.SizeY2 = r.ReadSByte();
                    b.WorkerWork = r.ReadUInt16();
                    b.WorkerRest = r.ReadUInt16();
                    b.ResInput = r.ReadBytes(4);
                    b.ResOutput = r.ReadBytes(4);
                    b.ResProductionX = r.ReadSByte();
                    b.MaxHealth = r.ReadUInt16();
                    b.Sight = r.ReadUInt16();
                    b.OwnerType = r.ReadByte();
                    b.Unknown = r.ReadBytes(36);
                }
            }

            this.entities.AddRange(entities);

            treeView1.Invoke(new Action(() =>
            {
                treeView1.BeginUpdate();
                try
                {
                    var houseroot = treeView1.Nodes.Add("Houses");
                    var animalsroot = houseroot.Nodes.Add("Pigs and Horses");
                    foreach (var item in entities)
                    {
                        var root = item is Animal ? animalsroot : houseroot;
                        var t = root.Nodes.Add(item.Name);
                        t.Tag = item;
                    }
                }
                finally
                {
                    treeView1.EndUpdate();
                }
            }));
        }

        void loadMapelem()
        {
            List<IEntity> entities = new List<IEntity>();
            string defines = Path.Combine(KAMDataFolder, "data", "defines", "mapelem.dat");
            using (var fs = File.OpenRead(defines))
            {
                BinaryReader r = new BinaryReader(fs);

                for (int i = 0; i < 254; i++)
                {
                    Mapelem d = new Mapelem("Mapelem " + i);
                    entities.Add(d);
                    d.RawOffset = fs.Position;
                    d.Anim = r.ReadInt16s(30);
                    d.AnimLength = r.ReadUInt16();
                    r.ReadUInt64();
                    d.Choppable = r.ReadInt32() == 1;
                    d.Walkable = r.ReadInt32() == 0;
                    d.BuildableOnEntireTile = r.ReadInt32() == 1;
                    d.Unknown = r.ReadInt32();
                    d.Growable = r.ReadInt32() == 1;
                    d.KeepPlantingDistance = r.ReadInt32() == 1;
                    d.TreeTrunk = r.ReadByte();
                    d.Buildable = r.ReadInt32() == 1;
                }
            }

            this.entities.AddRange(entities);

            treeView1.Invoke(new Action(() =>
            {
                treeView1.BeginUpdate();
                try
                {
                    var houseroot = treeView1.Nodes.Add("Mapelements");
                    foreach (var item in entities)
                    {
                        var t = houseroot.Nodes.Add(item.Name);
                        t.Tag = item;
                    }
                }
                finally
                {
                    treeView1.EndUpdate();
                }
            }));
        }

        internal class frame
        {
            public ushort W, H;
            public int X, Y;
            public frame OldVersion;
            public long RawOffset;
            public byte[] Raw;
        }

        internal static frame[][] allRX = new frame[5][];

        frame[] loadRX(string name)
        {
            frame[] rx;
            string path = Path.Combine(KAMDataFolder, "data", "gfx", "res", name);
            using (var fs = File.OpenRead(path))
            {
                BinaryReader r = new BinaryReader(fs);
                rx = new frame[r.ReadInt32()];
                var exists = r.ReadBytes(rx.Length);
                for (int i = 0; i < rx.Length; i++)
                {
                    if (exists[i] != 0)
                    {
                        rx[i] = new frame();
                        rx[i].RawOffset = r.BaseStream.Position;
                        rx[i].W = r.ReadUInt16();
                        rx[i].H = r.ReadUInt16();
                        rx[i].X = r.ReadInt32();
                        rx[i].Y = r.ReadInt32();
                        rx[i].Raw = r.ReadBytes(rx[i].W * rx[i].H);
                    }
                }
            }

            return rx;
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                loadpalette();
                allRX[2] = loadRX("houses.rx");
                allRX[3] = loadRX("trees.rx");
                allRX[4] = loadRX("units.rx");

                loadHouses();
                loadMapelem();
            }
            catch
            { }
        }

        internal static List<Animal> animals;

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var item = e.Node.Tag as IEntity;
            if (item == null)
                return;
            selected = item;
            time = 0;
            item.SwitchViewTo(this);
            timer1.Start();
        }

        IEntity selected = null;
        int time;

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (selected != null)
            {
                drawFrame(time, selected);
                time++;
            }
        }

        public static unsafe void drawSprite(BitmapData d, int rx, short sprIdx, int posx, int posy)
        {
            if (sprIdx < 0)
                return;
            frame f = allRX[rx][sprIdx] ?? new frame();
            drawFrame(d, posx, posy, f);
        }

        internal static unsafe void drawFrame(BitmapData d, int posx, int posy, frame f)
        {
            for (int y = 0; y < f.H; y++)
            {
                int* ptr = (int*)(d.Scan0 + (y + 200 + f.Y + posy) * d.Stride) + (200 + f.X + posx);
                if (y + 200 + f.Y + posy < 0 || y + 200 + f.Y + posy >= 400)
                    continue;

                int i = y * f.W;

                int xstart = 0;
                if (200 + f.X + posx < 0)
                {
                    xstart = -(200 + f.X + posx);
                    i = xstart;
                }
                int xend = f.W;
                if (200 + f.X + posx + xend > 400)
                    xend -= 400 - (200 + f.X + posx + xend);

                for (int x = xstart; x < xend; x++)
                {
                    var p = f.Raw[i++];
                    if (p != 0)
                        ptr[x] = palette[p];
                }
            }
        }

        public static unsafe void drawSpriteMasked(BitmapData d, int rx, short sprIdx, int posx, int posy, short maskIdx, int steps)
        {
            if (sprIdx < 0 || maskIdx < 0)
                return;
            frame f = allRX[rx][sprIdx] ?? new frame();
            frame mask = allRX[rx][maskIdx] ?? new frame();
            for (int y = 0; y < f.H; y++)
            {
                int* ptr = (int*)(d.Scan0 + (y + 200 + f.Y + posy) * d.Stride) + (200 + f.X + posx);
                if (y + 200 + f.Y + posy < 0 || y + 200 + f.Y + posy >= 400)
                    continue;

                int i = y * f.W;

                int xstart = 0;
                if (200 + f.X + posx < 0)
                {
                    xstart = -(200 + f.X + posx);
                    i = xstart;
                }
                int xend = f.W;
                if (200 + f.X + posx + xend > 400)
                    xend -= 400 - (200 + f.X + posx + xend);

                for (int x = xstart; x < xend; x++)
                {
                    int mx = x + (f.X - mask.X);
                    int my = y + (f.Y - mask.Y);
                    var maskval = (mx >= 0 && mx < mask.W && my >= 0 && my < mask.H) ? mask.Raw[mx + my * mask.W] : 0;
                    var p = f.Raw[i++];
                    if (p != 0 && maskval < steps)
                        ptr[x] = palette[p];
                }
            }
        }

        Bitmap[] bms = new Bitmap[2] { new Bitmap(400, 400), new Bitmap(400, 400) };
        int bmindex = 0;

        DrawParams getDrawParams()
        {
            DrawParams dp = new DrawParams();
            dp.MainForm = this;
            dp.Stone = true;
            dp.Flags = flagCheckBox.Checked;
            dp.Smoke = smokeCheckBox.Checked;
            dp.WorkIndex = comboBox1.SelectedIndex;
            dp.Fires = fireCheckBoxes.CheckedIndices.Cast<int>().ToList();
            dp.Res = resourcesCheckBoxes.CheckedIndices.Cast<int>().ToList();
            dp.Swines = checkedListBox1.CheckedIndices.Cast<int>().ToList();
            dp.Horses = checkedListBox2.CheckedIndices.Cast<int>().ToList();
            return dp;
        }

        unsafe void drawFrame(int time, IEntity selected)
        {
            int bmi = System.Threading.Interlocked.Increment(ref bmindex);
            var bm = bms[bmi & 1];
            var d = bm.LockBits(new Rectangle(0, 0, 400, 400),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            for (int i = 0; i < d.Height; i++)
            {
                int* ptr = (int*)(d.Scan0 + i * d.Stride);
                for (int j = 0; j < d.Width; j++)
                    *ptr++ = 0;
            }

            selected.Draw(getDrawParams(), d, time);

            bm.UnlockBits(d);

            pictureBox1.Image = bm;
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (listBox1.SelectedIndex)
            {
                case 0:
                    stackPanel1.SelectTab(1);
                    break;
                case 1:
                    stackPanel1.SelectTab(3);
                    break;
                case 2:
                    stackPanel1.SelectTab(4);
                    break;
                default:
                    break;
            }
        }

        static int gcd(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static int lcm(int a, int b)
        {
            return (a / gcd(a, b)) * b;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (selected == null)
                return;

            timer1.Stop();

            Bitmap bm = new Bitmap(400, 400);
            var d = bm.LockBits(new Rectangle(0, 0, 400, 400), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            selected.Draw(getDrawParams(), d, 0);
            bm.UnlockBits(d);

            int len = selected.lcmOfAnims;
            if (len > 1000)
            {
                if (MessageBox.Show("GIF would be too long (" + len + " frames), render it anyway?", "GIF too long", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var collection = new ImageMagick.MagickImageCollection())
                    {
                        for (int i = 0; i < len; i++)
                        {
                            bm = new Bitmap(400, 400);
                            d = bm.LockBits(new Rectangle(0, 0, 400, 400), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                            selected.Draw(getDrawParams(), d, i);
                            bm.UnlockBits(d);
                            collection.Add(new ImageMagick.MagickImage(bm));
                            collection[i].AnimationDelay = 10;
                        }

                        var qs = new ImageMagick.QuantizeSettings();
                        qs.Colors = 256;
                        qs.DitherMethod = ImageMagick.DitherMethod.No;
                        collection.Quantize(qs);

                        collection.Optimize();

                        collection.Write(saveFileDialog1.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            timer1.Start();
        }

        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (KAMDataFolder == null)
            {
                MessageBox.Show("No data loaded");
                return;
            }

            var s = new SaveChanges(entities, KAMDataFolder);
            s.ShowDialog();
        }

        private void ReplaceSpriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ReplaceSprite().Show();
        }
    }

    interface IEntity
    {
        void SwitchViewTo(Form1 form);
        string Name { get; }
        long RawOffset { get; set; }
        int RX { get; }
        int lcmOfAnims { get; }

        void Draw(DrawParams dp, BitmapData d, int time);
    }

    public class DrawParams
    {
        public bool Flags, Smoke, Stone;
        public int WorkIndex;
        public List<int> Fires, Res, Swines, Horses;
        public Form1 MainForm;

        public DrawParams()
        {
            Fires = new List<int>();
            Res = Fires;
            Swines = Fires;
            Horses = Fires;
        }
    }

    class Animal : IEntity, IEquatable<Animal>
    {
        public Point Offset { get; set; }
        [DisplayName("Animation (raw)")]
        public short[] Anim { get; set; }
        [DisplayName("Animation length (raw)")]
        public ushort AnimLength { get; set; }

        [TypeConverter(typeof(SpriteArrayConverter))]
        [ArrayLength(30)]
        public short[] Animation
        {
            get { return Anim; }
            set { Anim = value; }
        }

        public Animal(string name)
        {
            Name = name;
        }

        public string Name { get; }
        [ReadOnly(true)]
        public long RawOffset { get; set; }
        [Browsable(false)]
        public int RX => 2;
        [Browsable(false)]
        public int lcmOfAnims => AnimLength;

        public void Draw(DrawParams dp, BitmapData d, int time)
        {
            Form1.drawSprite(d, 2, Anim[time % AnimLength], 0, 0);
        }

        public bool Equals(Animal other)
        {
            return Offset == other.Offset && AnimLength == other.AnimLength &&
                Anim.SequenceEqual(other.Anim);
        }

        public void SwitchViewTo(Form1 form)
        {
            form.stackPanel1.SelectTab(0);
            form.stackPanel2.SelectTab(0);
        }
    }

    class Building : IEntity, IEquatable<Building>
    {
        [Category("Construction")]
        public short StoneTexture { get; set; }
        [Category("Construction")]
        public short WoodTexture { get; set; }
        [Category("Construction")]
        public short WoodenConstructionMaskTexture { get; set; }
        [Category("Construction")]
        public short StoneConstructionMaskTexture { get; set; }
        [Category("Working")]
        public WorkAnimationsList WorkAnimations { get; set; }
        [Category("Working")]
        public short[] SupplyResourcesIn { get; set; }
        [Category("Working")]
        public short[] SupplyResourcesOut { get; set; }
        [Category("Working")]
        public short[][] WorkAnim { get; set; }
        [Category("Working")]
        public ushort[] WorkAnimLength { get; set; }
        [Category("Working")]
        public int[] WorkAnimX { get; set; }
        [Category("Working")]
        public int[] WorkAnimY { get; set; }
        [Category("Construction")]
        public ushort WoodSteps { get; set; }
        [Category("Construction")]
        public ushort StoneSteps { get; set; }
        public ushort a1 { get; set; }
        public sbyte EntranceOffsetX { get; set; }
        public sbyte EntranceOffsetY { get; set; }
        public sbyte EntranceOffsetPxX { get; set; }
        public sbyte EntranceOffsetPxY { get; set; }
        [Category("Construction")]
        [TypeConverter(typeof(ByteArrayConverter))]
        public byte[] BuildArea { get; set; }
        [Category("Construction Cost")]
        public byte WoodCost { get; set; }
        [Category("Construction Cost")]
        public byte StoneCost { get; set; }
        [Category("Construction")]
        public Point[] BuildSupply { get; set; }
        public ushort a5 { get; set; }
        public ushort SizeArea { get; set; }
        public sbyte SizeX { get; set; }
        public sbyte SizeY { get; set; }
        public sbyte SizeX2 { get; set; }
        public sbyte SizeY2 { get; set; }
        public ushort WorkerWork { get; set; }
        public ushort WorkerRest { get; set; }
        [Category("Production")]
        [TypeConverter(typeof(ByteArrayConverter))]
        public byte[] ResInput { get; set; }
        [Category("Production")]
        [TypeConverter(typeof(ByteArrayConverter))]
        public byte[] ResOutput { get; set; }
        [Category("Production")]
        public sbyte ResProductionX { get; set; }
        [Category("Construction")]
        public ushort MaxHealth { get; set; }
        public ushort Sight { get; set; }
        public byte OwnerType { get; set; }
        [TypeConverter(typeof(ByteArrayConverter))]
        public byte[] Unknown { get; set; }

        public Building(string name)
        {
            Name = name;
            WorkAnim = new short[19][];
            WorkAnimLength = new ushort[19];
            WorkAnimX = new int[19];
            WorkAnimY = new int[19];
            WorkAnimations = new WorkAnimationsList(this);
        }

        public string Name { get; }
        [Browsable(false)]
        public bool Dirty { get; set; }
        [ReadOnly(true)]
        public long RawOffset { get; set; }

        [Browsable(false)]
        public int RX => 2;
        [Browsable(false)]
        public int lcmOfAnims { get; set; }

        public void Draw(DrawParams dp, BitmapData d, int time)
        {
            int renderMode = 0, buildStepsWood = 0, buildStepsStone = 0;
            if (dp.MainForm != null)
            {
                renderMode = dp.MainForm.listBox1.SelectedIndex;
                buildStepsWood = dp.MainForm.trackBar3.Value;
                buildStepsStone = dp.MainForm.trackBar6.Value;
            }

            lcmOfAnims = 1;

            switch (renderMode)
            {
                case 0:
                    if (dp.Stone)
                        Form1.drawSprite(d, 2, StoneTexture, 0, 0);

                    foreach (var item in dp.Res)
                    {
                        int index = item % 5 + 5 * (item / 10);
                        if (((item / 5) & 1) == 0)
                            Form1.drawSprite(d, 2, SupplyResourcesIn[index], 0, 0);
                        else
                            Form1.drawSprite(d, 2, SupplyResourcesOut[index], 0, 0);
                    }

                    if (Name == "Swine Farm")
                    {
                        foreach (var item in dp.Swines)
                        {
                            var a = Form1.animals[item];
                            int t = time % a.AnimLength;
                            Form1.drawSprite(d, 2, a.Anim[t], a.Offset.X, a.Offset.Y);
                            lcmOfAnims = Form1.lcm(lcmOfAnims, a.AnimLength);
                        }
                    }
                    else if (Name == "Stables")
                    {
                        foreach (var item in dp.Horses)
                        {
                            var a = Form1.animals[item + 15];
                            int t = time % a.AnimLength;
                            Form1.drawSprite(d, 2, a.Anim[t], a.Offset.X, a.Offset.Y);
                            lcmOfAnims = Form1.lcm(lcmOfAnims, a.AnimLength);
                        }
                    }

                    if (dp.Smoke)
                        drawWorkAnim(d, time, 5);

                    if (dp.Flags)
                    {
                        drawWorkAnim(d, time, 6);
                        drawWorkAnim(d, time, 8);
                        drawWorkAnim(d, time, 9);
                        drawWorkAnim(d, time, 10);
                    }

                    if (dp.WorkIndex == 0)
                        ; // nothing
                    else if (dp.WorkIndex == 1)
                        drawWorkAnim(d, time, 7);
                    else
                        drawWorkAnim(d, time, dp.WorkIndex - 2);

                    foreach (var item in dp.Fires)
                        drawWorkAnim(d, time, item + 11);
                    break;

                case 1:
                    // building frame
                    Form1.drawSpriteMasked(d, 2, WoodTexture, 0, 0, WoodenConstructionMaskTexture, buildStepsWood);
                    break;
                case 2:
                    // building stone
                    Form1.drawSprite(d, 2, WoodTexture, 0, 0);
                    Form1.drawSpriteMasked(d, 2, StoneTexture, 0, 0, StoneConstructionMaskTexture, buildStepsStone);
                    break;
            }
        }

        void drawWorkAnim(BitmapData d, int time, int index)
        {
            if (WorkAnimLength[index] == 0)
                return;
            Form1.drawSprite(d, 2, WorkAnim[index][time % WorkAnimLength[index]], WorkAnimX[index], WorkAnimY[index]);
            lcmOfAnims = Form1.lcm(lcmOfAnims, WorkAnimLength[index]);
        }

        public void SwitchViewTo(Form1 form)
        {
            form.listBox1.SelectedIndex = 0;
            form.stackPanel1.SelectTab(1);
            form.stackPanel2.SelectTab(1);
            form.stackPanel3.SelectTab(Name == "Swine Farm" ? 1 : (Name == "Stables" ? 2 : 0));
            form.propertyGrid1.SelectedObject = this;
            form.trackBar1.Value = 0;
            form.trackBar1.Maximum = WoodCost;
            form.trackBar2.Value = 0;
            form.trackBar2.Maximum = StoneCost;
            form.trackBar3.Maximum = WoodSteps;
            form.trackBar3.Value = WoodSteps;

            form.trackBar4.Value = 0;
            form.trackBar4.Maximum = WoodCost;
            form.trackBar5.Value = 0;
            form.trackBar5.Maximum = StoneCost;
            form.trackBar6.Maximum = StoneSteps;
            form.trackBar6.Value = StoneSteps;
        }

        public bool Equals(Building other)
        {
            return StoneTexture == other.StoneTexture &&
                WoodTexture == other.WoodTexture &&
                WoodenConstructionMaskTexture == other.WoodenConstructionMaskTexture &&
                StoneConstructionMaskTexture == other.StoneConstructionMaskTexture &&
                SupplyResourcesIn.SequenceEqual(other.SupplyResourcesIn) &&
                SupplyResourcesOut.SequenceEqual(other.SupplyResourcesOut) &&
                WorkAnim.Zip(other.WorkAnim, (a, b) => a.SequenceEqual(b)).All(a => a) &&
                WorkAnimLength.SequenceEqual(other.WorkAnimLength) &&
                WorkAnimX.SequenceEqual(other.WorkAnimX) &&
                WorkAnimY.SequenceEqual(other.WorkAnimY) &&
                WoodSteps == other.WoodSteps &&
                StoneSteps == other.StoneSteps &&
                a1 == other.a1 &&
                EntranceOffsetX == other.EntranceOffsetX &&
                EntranceOffsetY == other.EntranceOffsetY &&
                EntranceOffsetPxX == other.EntranceOffsetPxX &&
                EntranceOffsetPxY == other.EntranceOffsetPxY &&
                BuildArea.SequenceEqual(other.BuildArea) &&
                WoodCost == other.WoodCost &&
                StoneCost == other.StoneCost &&
                BuildSupply.SequenceEqual(other.BuildSupply) &&
                a5 == other.a5 &&
                SizeArea == other.SizeArea &&
                SizeX == other.SizeX &&
                SizeY == other.SizeY &&
                SizeX2 == other.SizeX2 &&
                SizeY2 == other.SizeY2 &&
                WorkerWork == other.WorkerWork &&
                WorkerRest == other.WorkerRest &&
                ResInput.SequenceEqual(other.ResInput) &&
                ResOutput.SequenceEqual(other.ResOutput) &&
                ResProductionX == other.ResProductionX &&
                MaxHealth == other.MaxHealth &&
                Sight == other.Sight &&
                OwnerType == other.OwnerType &&
                Unknown.SequenceEqual(other.Unknown);
        }
    }

    class Mapelem : IEntity
    {
        public bool Choppable { get; set; }
        public bool Walkable { get; set; }
        public bool BuildableOnEntireTile { get; set; }
        [Description("This field is usually 0, but set to 1 for grapes and wheat, and for one type of reeds.")]
        public int Unknown { get; set; }
        public bool Growable { get; set; }
        public bool KeepPlantingDistance { get; set; }
        public byte TreeTrunk { get; set; }
        public bool Buildable { get; set; }

        [DisplayName("Animation (raw)")]
        [Category("Animation")]
        public short[] Anim { get; set; }
        [DisplayName("Animation length (raw)")]
        [Category("Animation")]
        public ushort AnimLength { get; set; }

        [TypeConverter(typeof(SpriteArrayConverter))]
        [ArrayLength(30)]
        public short[] Animation
        {
            get { return Anim; }
            set { Anim = value; }
        }

        public Mapelem(string name)
        {
            Name = name;
        }

        public string Name { get; }
        [ReadOnly(true)]
        public long RawOffset { get; set; }
        [Browsable(false)]
        public int RX => 3;
        [Browsable(false)]
        public int lcmOfAnims => AnimLength;

        public void Draw(DrawParams dp, BitmapData d, int time)
        {
            Form1.drawSprite(d, 3, Anim[time % AnimLength], 0, 0);
        }

        public void SwitchViewTo(Form1 form)
        {
            form.stackPanel1.SelectTab(2);
            form.stackPanel2.SelectTab(2);
            form.propertyGridMapelem.SelectedObject = this;
        }
    }

    class ByteArrayConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
                return string.Join(", ", value as byte[]);
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                string txt = (string)value;
                try
                {
                    return (value as string).Split(',').Select(s => byte.Parse(s)).ToArray();
                }
                catch
                {
                    throw new InvalidCastException(
                        "Cannot convert the input '" +
                        value.ToString() + "' into a byte[]");
                }
            }
            else
                return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }
    }

    class ArrayLengthAttribute : Attribute
    {
        public readonly int Length;

        public ArrayLengthAttribute(int length)
        {
            Length = length;
        }
    }

    class SpriteArrayConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
                return string.Join(", ", value as short[]);
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                string txt = (string)value;
                var lengthAttr = context.PropertyDescriptor.Attributes[typeof(ArrayLengthAttribute)] as ArrayLengthAttribute;
                try
                {
                    return (value as string).Split(',')
                        .Select(s => short.Parse(s))
                        .Concat(Enumerable.Repeat((short)-1, int.MaxValue))
                        .Take(lengthAttr.Length).ToArray();
                }
                catch
                {
                    throw new InvalidCastException(
                        "Cannot convert the input '" +
                        value.ToString() + "' into a short[" + lengthAttr.Length + "]");
                }
            }
            else
                return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }
    }

    class WALEdit : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            WorkAnimationsList foo = value as WorkAnimationsList;
            if (svc != null && foo != null)
            {
                using (BuildingAnimEditor form = new BuildingAnimEditor(foo))
                {
                    svc.ShowDialog(form);
                }
            }
            return value;
        }
    }

    class WorkAnimation
    {
        [Browsable(false)]
        public string Name { get; private set; }
        public Point Offset
        {
            get
            {
                return new Point(owner.WorkAnimX[index], owner.WorkAnimY[index]);
            }
            set
            {
                owner.WorkAnimX[index] = value.X;
                owner.WorkAnimY[index] = value.Y;
            }
        }

        [DisplayName("Sprite list (raw)")]
        [TypeConverter(typeof(SpriteArrayConverter))]
        [ArrayLength(30)]
        public short[] SpriteList
        {
            get { return owner.WorkAnim[index]; }
            set { owner.WorkAnim[index] = value; }
        }

        [DisplayName("Sprite count (raw)")]
        public ushort SpriteCount
        {
            get { return owner.WorkAnimLength[index]; }
            set { owner.WorkAnimLength[index] = Math.Min(value, (ushort)30); }
        }

        Building owner;
        int index;

        [Browsable(false)]
        public int Index { get { return index; } }

        public WorkAnimation(string name, Building owner, int index)
        {
            Name = name;
            this.owner = owner;
            this.index = index;
        }
    }

    [Editor(typeof(WALEdit), typeof(UITypeEditor))]
    class WorkAnimationsList
    {
        internal List<WorkAnimation> list;
        internal Building owner;

        public WorkAnimationsList(Building owner)
        {
            this.owner = owner;
            list = new List<WorkAnimation>();
            list.Add(new WorkAnimation("Work 0", owner, 0));
            list.Add(new WorkAnimation("Work 1", owner, 1));
            list.Add(new WorkAnimation("Work 2", owner, 2));
            list.Add(new WorkAnimation("Work 3", owner, 3));
            list.Add(new WorkAnimation("Work 4", owner, 4));
            list.Add(new WorkAnimation("Smoke", owner, 5));
            list.Add(new WorkAnimation("Flagpole", owner, 6));
            list.Add(new WorkAnimation("Inhabitant idle", owner, 7));
            list.Add(new WorkAnimation("Flag 0", owner, 8));
            list.Add(new WorkAnimation("Flag 1", owner, 9));
            list.Add(new WorkAnimation("Flag 2", owner, 10));
            list.Add(new WorkAnimation("Fire 0", owner, 11));
            list.Add(new WorkAnimation("Fire 1", owner, 12));
            list.Add(new WorkAnimation("Fire 2", owner, 13));
            list.Add(new WorkAnimation("Fire 3", owner, 14));
            list.Add(new WorkAnimation("Fire 4", owner, 15));
            list.Add(new WorkAnimation("Fire 5", owner, 16));
            list.Add(new WorkAnimation("Fire 6", owner, 17));
            list.Add(new WorkAnimation("Fire 7", owner, 18));
        }

        public override string ToString()
        {
            return "Work animation list";
        }
    }

    static class Ext
    {
        public static short[] ReadInt16s(this BinaryReader r, int count)
        {
            var t = r.ReadBytes(count * 2);
            var res = new short[count];
            Buffer.BlockCopy(t, 0, res, 0, t.Length);
            return res;
        }

        public static void Write(this BinaryWriter w, short[] data, int length)
        {
            var buffer = new byte[length * 2];
            Buffer.BlockCopy(data, 0, buffer, 0, Math.Min(buffer.Length, data.Length * 2));
            for (int i = 2 * data.Length; i < buffer.Length; i++)
                buffer[i] = 0xFF;
            w.Write(buffer);
        }
    }
}
