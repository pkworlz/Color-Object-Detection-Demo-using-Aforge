using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO.Ports;

namespace AforgeAppByTut1
{
    public partial class Form1 : Form
    {
        private SerialPort mySp;
        private void InitArd()
        {
            try
            {
                mySp = new SerialPort();
                mySp.BaudRate = 9600;
                mySp.PortName = "COM3";
                mySp.Open();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message, "Error");
            }
        }
        public Form1()
        {
            InitializeComponent();
            numericUpDownR.Value = 75;
            numericUpDownG.Value = 255;
            numericUpDownB.Value = 255;
            trackBar3.Value = 111;
            blue = trackBar3.Value;
            InitArd();
            //Win32.POINT p = new Win32.POINT();
            //p.x = Convert.ToInt16(txtMouseX.Text);
            //p.y = Convert.ToInt16(txtMouseY.Text);

            //p.x = 50;
            //p.y = 50;

            
            //Win32.ClientToScreen(this.Handle, ref p);
            //Win32.SetCursorPos(500,500);
        }
        Graphics g;
        Bitmap video;
        int mode;
        int red, green, blue;
        bool OnOff=false;
        int TimTime=5;
        bool mouseActive=false;

        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;

        private void Form1_Load(object sender, EventArgs e)
        {
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevice)
            {
                comboBox1.Items.Add(Device.Name);
                
            }
            comboBox1.SelectedIndex = 0;
            FinalFrame = new VideoCaptureDevice();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
            FinalFrame.NewFrame += FinalFrame_NewFrame;
            FinalFrame.Start();
        }

        void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            video = (Bitmap)eventArgs.Frame.Clone();
            Bitmap video2 = (Bitmap)eventArgs.Frame.Clone();
            if (OnOff==true)
            {
                g = Graphics.FromImage(video2);
                g.DrawString(TimTime.ToString(), new Font("Arial", 20), new SolidBrush(Color.White), new PointF(2, 2));
                
            }
            if (mode == 1)
            {
                
                ColorFiltering colorFilter = new ColorFiltering();
                colorFilter.Red = new IntRange(red,(int)numericUpDownR.Value);
                colorFilter.Green = new IntRange(green, (int)numericUpDownG.Value);
                colorFilter.Blue = new IntRange(blue, (int)numericUpDownB.Value);
                colorFilter.ApplyInPlace(video2);
                //blob counter here..
                BlobCounter blobCounter = new BlobCounter();
                blobCounter.MinHeight = 20;
                blobCounter.MinWidth = 20;
                blobCounter.ObjectsOrder = ObjectsOrder.Size;
                blobCounter.ProcessImage(video2);
                Rectangle[] rect = blobCounter.GetObjectsRectangles();
                if (rect.Length>0)
                {
                    Rectangle objec = rect[0];
                    Rectangle objec2 = rect[1];
                    Graphics graphics = Graphics.FromImage(video2);
                    Graphics graphics2 = Graphics.FromImage(video2);
                    using (Pen pen = new Pen(Color.White,3))
                    {
                        //graphics.DrawRectangle(pen, objec);
                        //graphics2.DrawRectangle(pen, objec2);
                        if (objec.Height>20 & objec.Width>20)
                        {
                            graphics.DrawRectangle(pen, objec);
                            graphics.DrawString("1", new Font("Arial", 20), new SolidBrush(Color.White), new PointF(objec.X, objec.Y));    
                        }

                        if (objec2.Height > 20 & objec2.Width > 20)
                        {
                            graphics2.DrawRectangle(pen, objec2);
                            graphics2.DrawString("2", new Font("Arial", 20), new SolidBrush(Color.White), new PointF(objec2.X, objec2.Y));
                        }
                        
                        
                        PointF p1 = new PointF(objec.X, objec.Y);
                        PointF p2 = new PointF(objec2.X, objec2.Y);
                        graphics.DrawLine(pen,p1,p2);
                        
                        if (mouseActive==true)
                        {
                            Win32.SetCursorPos((int)p1.X, (int)p1.Y);
                        }

                        if (p2.X<30 && p2.Y<407 && mouseActive==true)
                        {
                             Win32.mouse_event(MouseEventType.LeftDown, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                             Win32.mouse_event(MouseEventType.LeftUp, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                             Thread.Sleep(20);
                        }
                        if (p1.X < 30 && p1.Y < 407 )
                        {
                            mySp.WriteLine("1");
                        }
                        else
                        {
                            mySp.WriteLine("0");
                        }
                        

                    }
                    Console.WriteLine(objec.X + "," + objec.Y + " | " + objec2.X + "," + objec2.Y);
                     //Console.WriteLine(objec2.X + "," + objec2.Y);
                    
                    graphics.Dispose();
                    graphics2.Dispose();
                   
                    
                }
                pictureBox2.Image = video2;
            }
            pictureBox1.Image = video;
            if (mode==1)
            {
                
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(FinalFrame.IsRunning==true)
            {
                FinalFrame.Stop();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = (Bitmap)pictureBox1.Image.Clone();
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimTime--;
            if (TimTime==0)
            {
                timer1.Enabled = false;
                OnOff = false;
                pictureBox2.Image = video;
                TimTime = 5;
            }
                                 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            OnOff = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            mode = 1; 
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            red = (int)trackBar1.Value;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            green = (int)trackBar2.Value;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            blue  = (int)trackBar3.Value;
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar=='a')
            {
                mouseActive = true;
            }
            if (e.KeyChar=='d')
            {
                mouseActive = false;
            }
            if (e.KeyChar=='c')
            {
                Win32.mouse_event(MouseEventType.LeftDown, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                Win32.mouse_event(MouseEventType.LeftUp, Cursor.Position.X, Cursor.Position.Y, 0, 0);
            }
            if (e.KeyChar == 'r')
            {
                Win32.mouse_event(MouseEventType.RightDown, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                Win32.mouse_event(MouseEventType.RightUp, Cursor.Position.X, Cursor.Position.Y, 0, 0);
            }
        }

        
    }

    public enum MouseEventType : int
    {
        LeftDown = 0x02,
        LeftUp = 0x04,
        RightDown = 0x08,
        RightUp = 0x10
    }
    public class Win32
    {
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("User32.Dll")]
        public static extern void mouse_event(MouseEventType dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        
        
        //[DllImport("User32.Dll")]
        //public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        //[StructLayout(LayoutKind.Sequential)]
        //public struct POINT
        //{
        //    public int x;
        //    public int y;
        //}
    }
}
