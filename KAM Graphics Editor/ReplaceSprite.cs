using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KAM_Graphics_Editor
{
    public partial class ReplaceSprite : Form
    {
        public ReplaceSprite()
        {
            int[] colorTweak = new int[256];
            colorTweak[15] = 1;
            colorTweak[23] = 2;
            colorTweak[31] = 3;
            colorTweak[85] = 1;
            colorTweak[89] = 1;
            colorTweak[100] = 1;
            colorTweak[101] = 2;
            colorTweak[102] = 3;
            colorTweak[103] = 4;
            colorTweak[141] = 0x100;
            colorTweak[142] = 0x101;
            colorTweak[143] = 0x102;
            colorTweak[146] = 1;
            colorTweak[148] = 1;
            colorTweak[150] = 0x103;
            colorTweak[151] = 0x203;
            colorTweak[152] = 0x303;
            colorTweak[153] = 0x004;
            colorTweak[154] = 0x104;
            colorTweak[155] = 0x204;
            colorTweak[156] = 0x304;
            colorTweak[157] = 0x005;
            colorTweak[158] = 0x105;
            colorTweak[159] = 0x205;
            colorTweak[205] = 0x10000;
            colorTweak[207] = 0x404;
            colorTweak[220] = 0x10001;
            colorTweak[221] = 0x10002;
            colorTweak[222] = 0x20002;
            colorTweak[223] = 0x10101;
            pal = new int[256];
            for (int i = 0; i < pal.Length; i++)
                pal[i] = Form1.palette[i] ^ colorTweak[i];

            colorToIndex.Add(0, 0);
            colorToIndex.Add(unchecked((int)0xFFFF00FF), 0);
            colorToIndex.Add(0xFF00FF, 0);
            for (int i = 1; i < pal.Length; i++)
            {
                if (i >= 224 && i < 251 || i == 255)
                    continue;
                colorToIndex.Add(pal[i], (byte)i);
            }

            InitializeComponent();
        }

        int[] pal;
        Dictionary<int, byte> colorToIndex = new Dictionary<int, byte>();

        private void Button1_Click(object sender, EventArgs e)
        {
            int rx = comboBox1.SelectedIndex + 2;
            int index = (int)numericUpDown1.Value;
            if (index >= Form1.allRX[rx].Length ||
                Form1.allRX[rx][index] == null)
            {
                MessageBox.Show("Only valid sprites can be replaced.");
                return;
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Bitmap bm;
                using (var fs = openFileDialog1.OpenFile())
                {
                    try
                    {
                        bm = (Bitmap)Bitmap.FromStream(fs);
                    }
                    catch
                    {
                        MessageBox.Show("Sprite could not be imported.");
                        return;
                    }
                }
                replacementFrame = transcode(bm, Form1.allRX[rx][index]);
                if (replacementFrame == null)
                    return;
                Bitmap buffer = new Bitmap(400, 400);
                var d = buffer.LockBits(new Rectangle(0, 0, 400, 400), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                Form1.drawFrame(d, 0, 0, replacementFrame);
                buffer.UnlockBits(d);
                pictureBox2.Image = buffer;
                button3.Enabled = true;
            }
        }

        unsafe Form1.frame transcode(Bitmap b, Form1.frame oldframe)
        {
            Form1.frame f = new Form1.frame();
            f.OldVersion = oldframe;
            f.W = (ushort)b.Width;
            f.H = (ushort)b.Height;
            f.X = oldframe.X;
            f.Y = oldframe.Y;
            f.Raw = new byte[f.W * f.H];

            var d = b.LockBits(new Rectangle(0, 0, f.W, f.H), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < d.Height; y++)
                {
                    int* ptr = (int*)(d.Scan0 + d.Stride * y);
                    for (int x = 0; x < d.Width; x++)
                    {
                        int pixel = *ptr++;
                        if (colorToIndex.TryGetValue(pixel, out byte idx))
                            f.Raw[x + f.W * y] = idx;
                        else
                        {
                            if (MessageBox.Show(string.Format("Unknown color {0:X8} at ({1},{2})", pixel, x, y), "Unknown color", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                                return null;
                        }
                    }
                }
            }
            finally
            {
                b.UnlockBits(d);
            }
            return f;
        }

        Form1.frame replacementFrame;

        Bitmap buffer = new Bitmap(400, 400);

        unsafe void rerender(bool noReset = false)
        {
            if (!noReset)
            {
                spriteSavedLabel.Hide();
                replacementFrame = null;
                pictureBox2.Image = null;
                button3.Enabled = false;
            }
            int rx = comboBox1.SelectedIndex + 2;
            int index = (int)numericUpDown1.Value;
            if (index >= Form1.allRX[rx].Length ||
                Form1.allRX[rx][index] == null)
            {
                pictureBox1.Image = null;
                button1.Enabled = false;
                button2.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
                button2.Enabled = true;
                pictureBox1.Image = null;
                var d = buffer.LockBits(new Rectangle(0, 0, 400, 400), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                for (int i = 0; i < d.Height; i++)
                {
                    int* ptr = (int*)(d.Scan0 + i * d.Stride);
                    for (int j = 0; j < d.Width; j++)
                        *ptr++ = 0;
                }
                Form1.drawSprite(d, rx, (ushort)index, 0, 0);
                buffer.UnlockBits(d);
                pictureBox1.Image = buffer;
            }
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            rerender();
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            rerender();
        }

        private void ReplaceSprite_Load(object sender, EventArgs e)
        {
            rerender();
        }

        string lastFileLocation;

        private unsafe void Button2_Click(object sender, EventArgs e)
        {
            int rx = comboBox1.SelectedIndex + 2;
            int index = (int)numericUpDown1.Value;
            if (index >= Form1.allRX[rx].Length ||
                Form1.allRX[rx][index] == null)
                return;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try { File.Delete(saveFileDialog1.FileName); } catch { }
                var frame = Form1.allRX[rx][index];
                if (checkBoxIndexed.Checked)
                {
                    saveIndexedPNG(saveFileDialog1.OpenFile(), frame);
                }
                else
                {
                    using (Bitmap bm = new Bitmap(frame.W, frame.H))
                    {
                        var d = bm.LockBits(new Rectangle(0, 0, frame.W, frame.H), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                        for (int i = 0; i < d.Height; i++)
                        {
                            int* ptr = (int*)(d.Scan0 + i * d.Stride);
                            for (int j = 0; j < d.Width; j++)
                                *ptr++ = pal[frame.Raw[j + i * frame.W]];
                        }
                        bm.UnlockBits(d);
                        using (var fs = saveFileDialog1.OpenFile())
                            bm.Save(fs, ImageFormat.Png);
                    }
                }
                lastFileLocation = saveFileDialog1.FileName;
                spriteSavedLabel.Show();
            }
        }

        void saveIndexedPNG(Stream fs, Form1.frame sprite)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);
            w.Write(0x474E5089);
            w.Write(0x0A1A0A0D);

            // IHDR
            w.Write(toBE(13));
            long pos = ms.Position;
            w.Write(0x52444849);
            w.Write(toBE(sprite.W));
            w.Write(toBE(sprite.H));
            w.Write((byte)8);
            w.Write((byte)3);
            w.Write((byte)0);
            w.Write((byte)0);
            w.Write((byte)0);
            w.Write(toBE(CRC(ms, pos, ms.Position)));

            // PLTE
            w.Write(toBE(3 * 256));
            pos = ms.Position;
            w.Write(0x45544C50);
            w.Write((byte)0xff);
            w.Write((byte)0);
            w.Write((byte)0xff);
            for (int i = 1; i < pal.Length; i++)
            {
                w.Write((byte)(pal[i] >> 16));
                w.Write((byte)(pal[i] >> 8));
                w.Write((byte)(pal[i]));
            }
            w.Write(toBE(CRC(ms, pos, ms.Position)));

            // tRNS
            w.Write(toBE(1));
            pos = ms.Position;
            w.Write(0x534E5274);
            w.Write((byte)0);
            w.Write(toBE(CRC(ms, pos, ms.Position)));

            MemoryStream pixeldata = new MemoryStream();
            for (int y = 0; y < sprite.H; y++)
            {
                pixeldata.WriteByte(0);
                pixeldata.Write(sprite.Raw, y * sprite.W, sprite.W);
            }
            pixeldata.Position = 0;
            MemoryStream compressedPixels = new MemoryStream();
            compressedPixels.WriteByte(0x78);
            compressedPixels.WriteByte(0x5E);
            var c = new System.IO.Compression.DeflateStream(compressedPixels, System.IO.Compression.CompressionMode.Compress, true);
            pixeldata.CopyTo(c);
            c.Close();
            compressedPixels.Position = 0;

            // IDAT
            w.Write(toBE((int)compressedPixels.Length));
            pos = ms.Position;
            w.Write(0x54414449);
            compressedPixels.CopyTo(ms);
            w.Write(toBE(CRC(ms, pos, ms.Position)));

            // IEND
            w.Write(0);
            w.Write(0x444E4549);
            w.Write(0x826042AE);

            ms.Position = 0;
            ms.CopyTo(fs);
            fs.Close();
        }

        static int CRC(MemoryStream ms, long from, long to)
        {
            long oldPos = ms.Position;
            ms.Position = from;
            long length = to - from;
            byte[] buffer = new byte[length];
            ms.Read(buffer, 0, buffer.Length);
            int res = (int)Crc32(buffer, 0, (int)length, 0);
            ms.Position = oldPos;
            return res;
        }

        static uint[] crcTable;
        private static uint Crc32(byte[] stream, int offset, int length, uint crc)
        {
            uint c;
            if (crcTable == null)
            {
                crcTable = new uint[256];
                for (uint n = 0; n <= 255; n++)
                {
                    c = n;
                    for (var k = 0; k <= 7; k++)
                    {
                        if ((c & 1) == 1)
                            c = 0xEDB88320 ^ ((c >> 1) & 0x7FFFFFFF);
                        else
                            c = ((c >> 1) & 0x7FFFFFFF);
                    }
                    crcTable[n] = c;
                }
            }
            c = ~crc;
            var endOffset = offset + length;
            for (var i = offset; i < endOffset; i++)
            {
                c = crcTable[(c ^ stream[i]) & 0xff] ^ (c >> 8);
            }
            return ~c;
        }

        static int toBE(int x)
        {
            return (x << 24) | ((x >> 24) & 0xff) | ((x << 8) & 0xff0000) | ((x >> 8) & 0xff00);
        }

        private void SpriteSavedLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string argument = "/select, \"" + lastFileLocation + "\"";

            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            int rx = comboBox1.SelectedIndex + 2;
            int index = (int)numericUpDown1.Value;
            if (index >= Form1.allRX[rx].Length ||
                Form1.allRX[rx][index] == null)
                return;
            if (replacementFrame != null)
            {
                Form1.allRX[rx][index] = replacementFrame;
                rerender(true);
            }
        }
    }
}
