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
            var houseroot = treeView1.Nodes.Add("Houses");
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
                    a.Anim = r.ReadUInt16s(30);
                    a.AnimLength = r.ReadUInt16();
                    a.Offset = new Point(r.ReadInt32(), r.ReadInt32());
                    if (!a.Equals(entities[i] as Animal))
                        houseroot.Nodes.Add(a.Name);
                }

                for (int i = 0; i < 29; i++)
                {
                    Building b = new Building(Form1.BuildingNames[i]);
                    oldEntities.Add(b);
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

                    if (!b.Equals(entities[i + 30] as Building))
                        houseroot.Nodes.Add(b.Name);
                }

                if (houseroot.Nodes.Count == 0)
                    treeView1.Nodes.Remove(houseroot);
            }

            treeView1.EndUpdate();
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (entityByName.ContainsKey(e.Node.Text))
            {
                propertyGridOld.SelectedObject = oldEntities.Find(x => x.Name == e.Node.Text);
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
                if (((TreeNode)root).Text == "Houses")
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
                            w.Write(a.Anim);
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
                            w.Write(b.SupplyResourcesIn);
                            w.Write(b.SupplyResourcesOut);

                            for (int j = 0; j < 19; j++)
                            {
                                w.Write(b.WorkAnim[j]);
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
                        string backupFile = Path.ChangeExtension(housedefines, ".dat.backup");
                        try { File.Delete(backupFile); } catch { }
                        File.Move(housedefines, backupFile);
                        File.Move(housedefines + "tmp", housedefines);
                    }
                }

            }


            Close();
        }
    }
}
