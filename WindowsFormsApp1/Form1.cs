using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using LitJson;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public static Point[] mouse = new Point[10];
        Color color = Color.Red;
        List<Point> UnscaledPoint = new List<Point>(20);
        public static string CurrentFolder, ConfigPath, cleanimg;
        public static List<Panorama> Panoramas = new List<Panorama>();
        public static Panorama SelectedPanorama;
        public static Buttons SelectedButton;



        public Form1()
        {
            InitializeComponent();
            //mouseList.AddRange(Enumerable.Repeat((Point)0, 20));
            textBox1.Enabled = false;
            ButtonLink.Enabled = false;
            CurrentFolder = @"C:\Users\p.m.kikin\Desktop\DEVELOP\SGUGIT\Assets\Materials";
        }

        public void button1_Click(object 
            sender, EventArgs e) //OpenFolder
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = CurrentFolder;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    Panoramas = new List<Panorama>();
                    Panoramas.Clear();
                    CurrentFolder = fbd.SelectedPath;
                    comboBox1.Items.Clear();
                    ConfigPath = System.IO.Path.Combine(CurrentFolder, "pano.cfg");
                    string ExistingConfig;
                    ExistingConfig = File.Exists(ConfigPath) ? File.ReadAllText(ConfigPath) : ""; //Read config file if Exists
                    if (ExistingConfig != "") { Panoramas = JsonMapper.ToObject<List<Panorama>>(ExistingConfig); } //Config Data to Object List if not empty
                    bool isNotEmpty = Panoramas.Any(); //is Panorama list not empty?

                    //MessageBox.Show(ConfigData[3].Name.ToString());
                    //MessageBox.Show(ConfigData.Exists(x => x.Name == "1.1").ToString());

                    //-----Fill The Objetc List-------->>>>>>>>>>>>>>>
                    foreach (string fileName in Directory.GetFiles(CurrentFolder, "*.jpg"))
                    {
                        if (isNotEmpty)
                        {
                            if (!Panoramas.Exists(x => x.Name == Path.GetFileNameWithoutExtension(fileName)))
                            {
                                Panoramas.Add(new Panorama
                                {
                                    Name = Path.GetFileNameWithoutExtension(fileName),
                                    Buttons = new List<Buttons> { }
                                });
                            }
                        }
                        else
                        {
                            Panoramas.Add(new Panorama
                            {
                                Name = Path.GetFileNameWithoutExtension(fileName),
                                Buttons = new List<Buttons> { }
                            });
                        }
                    }

                    for (int i = 0; i < Panoramas.Count; i++)
                    {
                        if (!Array.Exists(Directory.GetFiles(CurrentFolder, "*.jpg"), element => element.Contains(Panoramas[i].Name)))
                        {
                            Panoramas.RemoveAll(x => x.Name == Panoramas[i].Name);
                            i = i - 1;
                        }
                    }
                    //-----Fill The Objetc List--------<<<<<<<<<<<<<<<

                    //-----Fill The Combobox and Listbox-------->>>>>>>>>>>>>>>
                    foreach (Panorama pano in Panoramas)
                    {
                        UpdateComboBox(pano.Name);
                    }
                    //-----Fill The Combobox and Listbox--------<<<<<<<<<<<<<<<
                    ConfigWriter.ConfigWrite(Panoramas);
                }
            }
        }

        private void UpdateComboBox(string PanoName) //Fill the Combobox
        {
            comboBox1.Items.Add(PanoName);
        }

        private void buttonAdd_Click(object sender, EventArgs e) //Add
        {
            SelectedPanorama.Buttons.Add(new Buttons { Link = null, PositionX = 0, PositionY = 0});
            UpdateListBox(listBox1.Items.Count);
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void buttonDel_Click(object sender, EventArgs e) // Delete
        {
            for (int i = listBox1.SelectedIndices.Count - 1; i >= 0; i--)
            {
                SelectedPanorama.Buttons.RemoveAt(listBox1.SelectedIndex);
                UpdateListBox(listBox1.Items.Count-2);
                pictureBox1.Load(cleanimg);
                pictureBox1.Refresh();
                //Repaint(true);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) //Picture Choose
        {
            SelectedPanorama = Panoramas.Find(item => item.Name == comboBox1.SelectedItem.ToString());
            pictureBox1.Load(CurrentFolder+"/"+ SelectedPanorama.Name.ToString()+".jpg");
            cleanimg = pictureBox1.ImageLocation;
            listBox1.Items.Clear();
            UpdateListBox(0);
        }

        private void UpdateListBox(int CurrentIndex) //Fill listbox with buttons
        {
            if (SelectedPanorama.Buttons != null & SelectedPanorama.Buttons.Any())
            {
                listBox1.Items.Clear();
                foreach (Buttons button in SelectedPanorama.Buttons)
                {
                    listBox1.Items.Add("X: " + button.PositionX + " Y: " + button.PositionY);
                }
                listBox1.SelectedIndex = CurrentIndex;
            }
            else listBox1.Items.Clear();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) //ListBox
        {
            UpdateTextBox();
            Repaint(false);
        }

        private void UpdateTextBox() //Fill listbox with buttons
        {
            if (listBox1.Items.Count >= 1 & listBox1.SelectedIndex >= 0)
            {
                textBox1.Text = SelectedPanorama.Buttons[listBox1.SelectedIndex].Link;
                textBox1.Enabled = true;
                ButtonLink.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
                ButtonLink.Enabled = false;
            }
        }

        private void ButtonLink_Click(object sender, EventArgs e) //Link choose
        {
            GetButtonLink();
        }
        private void GetButtonLink() //Link choose
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "JPEG files (*.jpg)|*.jpg|All files (*.*)|*.*";
            dialog.InitialDirectory = CurrentFolder;
            dialog.Title = "Выберите изображение на которое ссылается кнопка";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (listBox1.Items.Count >= 1 & listBox1.SelectedIndex >= 0)
                {
                    System.IO.FileInfo fInfo = new System.IO.FileInfo(dialog.FileName);
                    SelectedPanorama.Buttons[listBox1.SelectedIndex].Link = Path.GetFileNameWithoutExtension(fInfo.Name.ToString());
                    UpdateTextBox();
                }
            }
        }
        private void pictureBox1_Click(object sender, EventArgs e) //Draw points
        {
            if (listBox1.Items.Count >= 1 & listBox1.SelectedIndex >= 0)
            {
                //Point p = pictureBox1.PointToClient(Cursor.Position);
                Point p = getTrueCoord();
                SelectedPanorama.Buttons[listBox1.SelectedIndex].PositionX = p.X;
                SelectedPanorama.Buttons[listBox1.SelectedIndex].PositionY = p.Y;
                UpdateListBox(listBox1.SelectedIndex);
                Repaint(true);
                if (textBox1.Text == "") GetButtonLink();
            }
        }
        private void Repaint(bool IfDelete) //Rpaint points
        {
            if (IfDelete) { pictureBox1.Load(cleanimg); pictureBox1.Refresh(); }
            Point truepoint = new Point();
            Color color1 = Color.Green;
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            Pen pen = new Pen(color, 4 / Scale());
            Pen pen1 = new Pen(color1, 4 / Scale());

            foreach (Buttons button in SelectedPanorama.Buttons)
            {
                truepoint.X = button.PositionX;
                truepoint.Y = button.PositionY;
                g.DrawRectangle(pen, truepoint.X, truepoint.Y, 4 / Scale(), 4 / Scale());
            }
            truepoint.X = SelectedPanorama.Buttons[listBox1.SelectedIndex].PositionX;
            truepoint.Y = SelectedPanorama.Buttons[listBox1.SelectedIndex].PositionY;
            g.DrawRectangle(pen1, truepoint.X, truepoint.Y, 4 / Scale(), 4 / Scale());
            pictureBox1.Image = pictureBox1.Image;
            pen.Dispose();
            pen1.Dispose();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            foreach (Panorama pano in Panoramas)
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(CurrentFolder + @"\" + pano.Name+".jpg");
                pano.Height = img.Height;
                pano.Width = img.Width;
                img.Dispose();
            }
            ConfigWriter.ConfigWrite(Panoramas);
        }

        private Point getTrueCoord()
        {
            Point p = pictureBox1.PointToClient(Cursor.Position);
            Point unscaled_p = new Point();
            int w_i = pictureBox1.Image.Width;
            int h_i = pictureBox1.Image.Height;
            int w_c = pictureBox1.Width;
            int h_c = pictureBox1.Height;
            float imageRatio = w_i / (float)h_i; // image W:H ratio
            float containerRatio = w_c / (float)h_c; // container W:H ratio
            if (imageRatio >= containerRatio)
            {
                // horizontal image
                float scaleFactor = w_c / (float)w_i;
                float scaledHeight = h_i * scaleFactor;
                // calculate gap between top of container and top of image
                float filler = Math.Abs(h_c - scaledHeight) / 2;
                unscaled_p.X = (int)(p.X / scaleFactor);
                unscaled_p.Y = (int)((p.Y - filler) / scaleFactor);
            }
            else
            {
                // vertical image
                float scaleFactor = h_c / (float)h_i;
                float scaledWidth = w_i * scaleFactor;
                float filler = Math.Abs(w_c - scaledWidth) / 2;
                unscaled_p.X = (int)((p.X - filler) / scaleFactor);
                unscaled_p.Y = (int)(p.Y / scaleFactor);
            }
            return unscaled_p;
        }
        private float Scale()
        {
            float scaleFactor;
            int w_i = pictureBox1.Image.Width;
            int h_i = pictureBox1.Image.Height;
            int w_c = pictureBox1.Width;
            int h_c = pictureBox1.Height;
            float imageRatio = w_i / (float)h_i;
            float containerRatio = w_c / (float)h_c;
            if (imageRatio >= containerRatio){scaleFactor = w_c / (float)w_i;}
            else{scaleFactor = h_c / (float)h_i;}
            return scaleFactor;
        }
    }

    public class Buttons
    {
        public string Link { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
    }

    public class Panorama
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Buttons> Buttons { get; set; }
        //public string[] Toys { get; set; }
    }

    public class ConfigWriter
    {
        public static void ConfigWrite(List<Panorama> pano)
        {
            JsonWriter writer = new JsonWriter();
            writer.PrettyPrint = true;
            writer.IndentValue = 2;
            JsonMapper.ToJson(pano, writer);
            Form1.ConfigPath = System.IO.Path.Combine(Form1.CurrentFolder, "pano.cfg");
            FileStream fs = new FileStream(Form1.ConfigPath, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.WriteLine(writer);
            sw.Dispose();
            fs.Dispose();
        }
    }

}

