using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KAM_Graphics_Editor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

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
            return;
        }

        string[] BuildingNames;
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

        static int[] palette;

        void loadpalette()
        {
            var bm = Properties.Resources.pal;
            var d = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            palette = new int[256];
            System.Runtime.InteropServices.Marshal.Copy(d.Scan0 + 64 * d.Stride, palette, 0, 256);
            bm.UnlockBits(d);
        }


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
                    a.Anim = r.ReadUInt16s(30);
                    a.AnimLength = r.ReadUInt16();
                    a.Offset.X = r.ReadInt32();
                    a.Offset.Y = r.ReadInt32();
                }

                animals = new List<Animal>(entities.Cast<Animal>());

                for (int i = 0; i < 29; i++)
                {
                    Building b = new Building(BuildingNames[i]);
                    entities.Add(b);
                    b.RawOffset = r.BaseStream.Position;
                    b.StoneTexture = r.ReadUInt16();
                    b.WoodTexture = r.ReadUInt16();
                    b.WoodenConstructionMaskTexture = r.ReadUInt16();
                    b.StoneConstructionMaskTexture = r.ReadUInt16();
                    b.SupplyResourcesIn = r.ReadUInt16s(20);
                    b.SupplyResourcesOut = r.ReadUInt16s(20);

                    for (int j = 0; j < 19; j++)
                    {
                        b.WorkAnim[j] = r.ReadUInt16s(30);
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

            treeView1.Invoke(new Action(() =>
            {
                treeView1.BeginUpdate();
                try
                {
                    var houseroot = treeView1.Nodes.Add("Houses");
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
                    d.Anim = r.ReadUInt16s(30);
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

        struct frame
        {
            public ushort W, H;
            public int X, Y;
            public bool Dirty;
            public byte[] Raw;
        }

        static frame[][] allRX = new frame[5][];

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

        interface IEntity
        {
            void SwitchViewTo(Form1 form);
            string Name { get; }
            bool Dirty { get; set; }
            long RawOffset { get; set; }
            int RX { get; }
            int lcmOfAnims { get; }

            void Draw(Form1 form, BitmapData d, int time);
        }

        class Animal : IEntity
        {
            public Point Offset;
            public ushort[] Anim;
            public ushort AnimLength;

            public Animal(string name)
            {
                Name = name;
            }

            public string Name { get; }
            public bool Dirty { get; set; }
            public long RawOffset { get; set; }

            public int RX => 2;
            public int lcmOfAnims => AnimLength;

            public void Draw(Form1 form, BitmapData d, int time)
            {
                drawSprite(d, 2, Anim[time % AnimLength], 0, 0);
            }

            public void SwitchViewTo(Form1 form)
            {
                form.stackPanel1.SelectTab(0);
                form.stackPanel2.SelectTab(0);
            }
        }

        static List<Animal> animals;

        class Building : IEntity
        {
            [Category("Construction")]
            public ushort StoneTexture { get; set; }
            [Category("Construction")]
            public ushort WoodTexture { get; set; }
            [Category("Construction")]
            public ushort  WoodenConstructionMaskTexture { get; set; }
            [Category("Construction")]
            public ushort StoneConstructionMaskTexture { get; set; }

            [Category("Working")]
            public ushort[] SupplyResourcesIn { get; set; }
            [Category("Working")]
            public ushort[] SupplyResourcesOut { get; set; }
            [Category("Working")]
            public ushort[][] WorkAnim { get; set; }
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
            public byte[] BuildArea { get; set; }
            [Category("Construction")]
            public byte WoodCost { get; set; }
            [Category("Construction")]
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
            public byte[] ResInput { get; set; }
            public byte[] ResOutput { get; set; }
            public sbyte ResProductionX { get; set; }
            [Category("Construction")]
            public ushort MaxHealth { get; set; }
            public ushort Sight { get; set; }
            public byte OwnerType { get; set; }
            public byte[] Unknown { get; set; }

            public Building(string name)
            {
                Name = name;
                WorkAnim = new ushort[19][];
                WorkAnimLength = new ushort[19];
                WorkAnimX = new int[19];
                WorkAnimY = new int[19];
            }

            public string Name { get; }
            [Browsable(false)]
            public bool Dirty { get; set; }
            [ReadOnly(true)]
            public long RawOffset { get; set; }

            public int RX => 2;

            public int lcmOfAnims { get; set; }

            public void Draw(Form1 form, BitmapData d, int time)
            {
                bool drawFlags = (bool)form.flagCheckBox.Invoke(new Func<bool>(() => form.flagCheckBox.Checked));
                bool drawSmoke = (bool)form.smokeCheckBox.Invoke(new Func<bool>(() => form.smokeCheckBox.Checked));
                int selectedWorkIndex = (int)form.comboBox1.Invoke(new Func<int>(() => form.comboBox1.SelectedIndex));
                List<int> enabledFires = (List<int>)form.fireCheckBoxes.Invoke(new Func<List<int>>(() => {
                    var t = new List<int>();
                    foreach (var item in form.fireCheckBoxes.CheckedIndices)
                        t.Add((int)item);
                    return t;
                }));
                List<int> enabledRes = (List<int>)form.resourcesCheckBoxes.Invoke(new Func<List<int>>(() => {
                    var t = new List<int>();
                    foreach (var item in form.resourcesCheckBoxes.CheckedIndices)
                        t.Add((int)item);
                    return t;
                }));
                List<int> enabledSwines = (List<int>)form.checkedListBox1.Invoke(new Func<List<int>>(() => {
                    var t = new List<int>();
                    foreach (var item in form.checkedListBox1.CheckedIndices)
                        t.Add((int)item);
                    return t;
                }));
                List<int> enabledHorses = (List<int>)form.checkedListBox2.Invoke(new Func<List<int>>(() => {
                    var t = new List<int>();
                    foreach (var item in form.checkedListBox2.CheckedIndices)
                        t.Add((int)item);
                    return t;
                }));

                int renderMode = (int)form.listBox1.Invoke(new Func<int>(() => form.listBox1.SelectedIndex));
                int buildStepsWood = (int)form.trackBar3.Invoke(new Func<int>(() => form.trackBar3.Value));
                int buildStepsStone = (int)form.trackBar6.Invoke(new Func<int>(() => form.trackBar6.Value));

                lcmOfAnims = 1;

                switch (renderMode)
                {
                    case 0:
                        drawSprite(d, 2, StoneTexture, 0, 0);

                        foreach (var item in enabledRes)
                        {
                            int index = item % 5 + 5 * (item / 10);
                            if (((item / 5) & 1) == 0)
                                drawSprite(d, 2, SupplyResourcesIn[index], 0, 0);
                            else
                                drawSprite(d, 2, SupplyResourcesOut[index], 0, 0);
                        }

                        if (Name == "Swine Farm")
                        {
                            foreach (var item in enabledSwines)
                            {
                                var a = animals[item];
                                int t = time % a.AnimLength;
                                drawSprite(d, 2, a.Anim[t], a.Offset.X, a.Offset.Y);
                                lcmOfAnims = lcm(lcmOfAnims, a.AnimLength);
                            }
                        }
                        else if (Name == "Stables")
                        {
                            foreach (var item in enabledHorses)
                            {
                                var a = animals[item + 15];
                                int t = time % a.AnimLength;
                                drawSprite(d, 2, a.Anim[t], a.Offset.X, a.Offset.Y);
                                lcmOfAnims = lcm(lcmOfAnims, a.AnimLength);
                            }
                        }

                        if (drawSmoke)
                            drawWorkAnim(d, time, 5);

                        if (drawFlags)
                        {
                            drawWorkAnim(d, time, 6);
                            drawWorkAnim(d, time, 8);
                            drawWorkAnim(d, time, 9);
                            drawWorkAnim(d, time, 10);
                        }

                        if (selectedWorkIndex == 0)
                            ; // nothing
                        else if (selectedWorkIndex == 1)
                            drawWorkAnim(d, time, 7);
                        else
                            drawWorkAnim(d, time, selectedWorkIndex - 2);

                        foreach (var item in enabledFires)
                        {
                            drawWorkAnim(d, time, item + 10);
                        }
                        break;

                    case 1:
                        // building frame
                        drawSpriteMasked(d, 2, WoodTexture, 0, 0, WoodenConstructionMaskTexture, buildStepsWood);
                        break;
                    case 2:
                        // building stone
                        drawSprite(d, 2, WoodTexture, 0, 0);
                        drawSpriteMasked(d, 2, StoneTexture, 0, 0, StoneConstructionMaskTexture, buildStepsStone);
                        break;
                }
            }

            void drawWorkAnim(BitmapData d, int time, int index)
            {
                if (WorkAnimLength[index] == 0)
                    return;
                drawSprite(d, 2, WorkAnim[index][time % WorkAnimLength[index]], WorkAnimX[index], WorkAnimY[index]);
                lcmOfAnims = lcm(lcmOfAnims, WorkAnimLength[index]);
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
        }

        class Mapelem : IEntity
        {
            public ushort[] Anim;
            public ushort AnimLength;
            public bool Choppable;
            public bool Walkable;
            public bool BuildableOnEntireTile;
            public int Unknown;
            public bool Growable;
            public bool KeepPlantingDistance;
            public byte TreeTrunk;
            public bool Buildable;

            public Mapelem(string name)
            {
                Name = name;
            }

            public string Name { get; }
            public bool Dirty { get; set; }
            public long RawOffset { get; set; }

            public int RX => 3;
            public int lcmOfAnims => AnimLength;

            public void Draw(Form1 form, BitmapData d, int time)
            {
                drawSprite(d, 3, Anim[time % AnimLength], 0, 0);
            }

            public void SwitchViewTo(Form1 form)
            {
                form.stackPanel1.SelectTab(2);
                form.stackPanel2.SelectTab(2);
            }
        }

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

        static unsafe void drawSprite(System.Drawing.Imaging.BitmapData d, int rx, ushort sprIdx, int posx, int posy)
        {
            if (sprIdx == 0xffff)
                return;
            frame f = allRX[rx][sprIdx];
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

        static unsafe void drawSpriteMasked(System.Drawing.Imaging.BitmapData d, int rx, ushort sprIdx, int posx, int posy, ushort maskIdx, int steps)
        {
            if (sprIdx == 0xffff)
                return;
            frame f = allRX[rx][sprIdx];
            frame mask = allRX[rx][maskIdx];
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

        unsafe void drawFrame(int time, IEntity selected)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((object _params) =>
            {
                var p = (Tuple<int, IEntity>)_params;
                int bmi = System.Threading.Interlocked.Increment(ref bmindex);
                var bm = bms[bmi & 1];
                if (System.Threading.Monitor.TryEnter(bm))
                    System.Threading.Monitor.Exit(bm);
                else
                    return;
                lock (bm)
                {
                    var d = bm.LockBits(new Rectangle(0, 0, 400, 400),
                        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                    for (int i = 0; i < d.Height; i++)
                    {
                        int* ptr = (int*)(d.Scan0 + i * d.Stride);
                        for (int j = 0; j < d.Width; j++)
                            *ptr++ = 0;
                    }

                    p.Item2.Draw(this, d, p.Item1);

                    bm.UnlockBits(d);
                }
                pictureBox1.Invoke(new Action(() =>
                {
                    pictureBox1.Image = bm;
                }));
            }, new Tuple<int, IEntity>(time, selected));
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

        static int lcm(int a, int b)
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
            selected.Draw(this, d, 0);
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
                            selected.Draw(this, d, i);
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
    }

    static class Ext
    {
        public static ushort[] ReadUInt16s(this BinaryReader r, int count)
        {
            var t = r.ReadBytes(count * 2);
            var res = new ushort[count];
            Buffer.BlockCopy(t, 0, res, 0, t.Length);
            return res;
        }
    }
}
