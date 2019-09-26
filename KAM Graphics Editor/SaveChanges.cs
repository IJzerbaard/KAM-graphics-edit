using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KAM_Graphics_Editor
{
    public partial class SaveChanges : Form
    {
        List<IEntity> entities;
        List<IEntity> oldEntities;
        Dictionary<string, IEntity> entityByName;
        string KAMDataFolder;

        internal SaveChanges(List<IEntity> entities, string KAMDataFolder)
        {
            this.entities = new List<IEntity>(entities);
            entityByName = new Dictionary<string, IEntity>();
            foreach (var item in entities)
                entityByName.Add(item.Name, item);
            this.KAMDataFolder = KAMDataFolder;
            InitializeComponent();
        }

        private void SaveChanges_Load(object sender, EventArgs e)
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            var houseroot = treeView1.Nodes.Add("houses.dat");
            string housedefines = Path.Combine(KAMDataFolder, "data", "defines", "houses.dat");
            oldEntities = new List<IEntity>();
            using (var fs = File.OpenRead(housedefines))
            {
                BinaryReader r = new BinaryReader(fs);

                for (int i = 0; i < 30; i++)
                {
                    Animal a = new Animal("Animal " + i);
                    oldEntities.Add(a);
                    a.RawOffset = r.BaseStream.Position;
                    a.Anim = r.ReadInt16s(30);
                    a.AnimLength = r.ReadUInt16();
                    a.Offset = new Point(r.ReadInt32(), r.ReadInt32());
                    if (!a.Equals(entities[i] as Animal))
                    {
                        var node = houseroot.Nodes.Add(a.Name);
                        node.Tag = a;
                    }
                }

                for (int i = 0; i < 29; i++)
                {
                    Building b = new Building(Form1.BuildingNames[i]);
                    oldEntities.Add(b);
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

                    if (!b.Equals(entities[i + 30] as Building))
                    {
                        var node = houseroot.Nodes.Add(b.Name);
                        node.Tag = b;
                    }
                }

                if (houseroot.Nodes.Count == 0)
                    treeView1.Nodes.Remove(houseroot);
            }

            var houserxroot = treeView1.Nodes.Add("houses.rx");
            for (int i = 0; i < Form1.allRX[2].Length; i++)
            {
                var sprite = Form1.allRX[2][i];
                if (sprite != null && sprite.OldVersion != null)
                {
                    var node = houserxroot.Nodes.Add("Sprite " + i);
                    node.Tag = sprite;
                }
            }
            if (houserxroot.Nodes.Count == 0)
                treeView1.Nodes.Remove(houserxroot);

            if (treeView1.Nodes.Count == 0)
                treeView1.Nodes.Add("No changes to save");

            treeView1.EndUpdate();
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is IEntity entity)
            {
                propertyGridOld.SelectedObject = entity;
                propertyGridNew.SelectedObject = entityByName[e.Node.Text];
            }
            else
            {
                propertyGridOld.SelectedObject = null;
                propertyGridNew.SelectedObject = null;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            foreach (var root in treeView1.Nodes)
            {
                string nodeText = ((TreeNode)root).Text;
                if (nodeText == "houses.dat")
                {
                    MemoryStream houses = new MemoryStream();
                    string housedefines = Path.Combine(KAMDataFolder, "data", "defines", "houses.dat");
                    using (var f = File.OpenRead(housedefines))
                        f.CopyTo(houses);

                    foreach (var item in ((TreeNode)root).Nodes)
                    {
                        var entity = entityByName[((TreeNode)item).Text];
                        houses.Position = entity.RawOffset;
                        var w = new BinaryWriter(houses);
                        if (entity is Animal a)
                        {
                            w.Write(a.Anim, 30);
                            w.Write(a.AnimLength);
                            w.Write(a.Offset.X);
                            w.Write(a.Offset.Y);
                        }
                        else if (entity is Building b)
                        {
                            w.Write(b.StoneTexture);
                            w.Write(b.WoodTexture);
                            w.Write(b.WoodenConstructionMaskTexture);
                            w.Write(b.StoneConstructionMaskTexture);
                            w.Write(b.SupplyResourcesIn, 20);
                            w.Write(b.SupplyResourcesOut, 20);

                            for (int j = 0; j < 19; j++)
                            {
                                w.Write(b.WorkAnim[j], 30);
                                w.Write(b.WorkAnimLength[j]);
                                w.Write(b.WorkAnimX[j]);
                                w.Write(b.WorkAnimY[j]);
                            }

                            w.Write(b.WoodSteps);
                            w.Write(b.StoneSteps);
                            w.Write(b.a1);
                            w.Write(b.EntranceOffsetX);
                            w.Write(b.EntranceOffsetY);
                            w.Write(b.EntranceOffsetPxX);
                            w.Write(b.EntranceOffsetPxY);
                            w.Write(b.BuildArea);
                            w.Write(b.WoodCost);
                            w.Write(b.StoneCost);

                            for (int j = 0; j < 12; j++)
                            {
                                w.Write(b.BuildSupply[j].X);
                                w.Write(b.BuildSupply[j].Y);
                            }

                            w.Write(b.a5);
                            w.Write(b.SizeArea);
                            w.Write(b.SizeX);
                            w.Write(b.SizeY);
                            w.Write(b.SizeX2);
                            w.Write(b.SizeY2);
                            w.Write(b.WorkerWork);
                            w.Write(b.WorkerRest);
                            w.Write(b.ResInput);
                            w.Write(b.ResOutput);
                            w.Write(b.ResProductionX);
                            w.Write(b.MaxHealth);
                            w.Write(b.Sight);
                            w.Write(b.OwnerType);
                            w.Write(b.Unknown);
                        }

                        w.Flush();
                    }

                    houses.Position = 0;
                    using (var f = File.OpenWrite(housedefines + "tmp"))
                    {
                        houses.CopyTo(f);
                        f.Close();
                        string backupFile = Path.ChangeExtension(housedefines, ".dat.bak");
                        try { File.Delete(backupFile); } catch { }
                        File.Move(housedefines, backupFile);
                        File.Move(housedefines + "tmp", housedefines);
                    }
                }
                else if (nodeText == "houses.rx")
                {
                    MemoryStream houses = new MemoryStream();
                    string housesrx = Path.Combine(KAMDataFolder, "data", "gfx", "res", "houses.rx");
                    using (var f = File.OpenRead(housesrx))
                    {
                        byte[] buffer = new byte[12];
                        int[] ibuffer = new int[3];
                        long lastPos = 0;
                        foreach (var item in from n in ((TreeNode)root).Nodes.Cast<TreeNode>()
                                             let sprite = (Form1.frame)n.Tag
                                             orderby sprite.OldVersion.RawOffset
                                             select sprite)
                        {
                            var sprite = item;
                            CopyStream(f, houses, (int)(sprite.OldVersion.RawOffset - lastPos));
                            f.Position += 12 + sprite.OldVersion.Raw.Length;
                            lastPos = sprite.OldVersion.RawOffset + 12 + sprite.OldVersion.Raw.Length;
                            ibuffer[0] = sprite.W | (sprite.H << 16);
                            ibuffer[1] = sprite.X;
                            ibuffer[2] = sprite.Y;
                            Buffer.BlockCopy(ibuffer, 0, buffer, 0, 12);
                            houses.Write(buffer, 0, 12);
                            houses.Write(sprite.Raw, 0, sprite.Raw.Length);
                        }
                        CopyStream(f, houses, int.MaxValue);
                    }

                    houses.Position = 0;
                    using (var f = File.OpenWrite(housesrx + "tmp"))
                    {
                        houses.CopyTo(f);
                        f.Close();
                        string backupFile = Path.ChangeExtension(housesrx, ".rx.bak");
                        try { File.Delete(backupFile); } catch { }
                        File.Move(housesrx, backupFile);
                        File.Move(housesrx + "tmp", housesrx);
                    }
                }
            }


            Close();
        }

        static void CopyStream(Stream input, Stream output, int bytes)
        {
            if (bytes <= 0)
                return;
            byte[] buffer = new byte[4096];
            int read;
            while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}
