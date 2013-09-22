using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Leap;

namespace WpfApplication1
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FloatingOSDWindow osd1 = new FloatingOSDWindow();

        [DllImport("user32.dll")]
        static extern int RedrawWindow(IntPtr hWnd, [In] ref RECT lprcUpdate, IntPtr hrgnUpdate, uint flags);

        
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }


        public MainWindow()
        {

            //System.Windows.Forms.Cursor.Current = new System.Windows.Forms.Cursor(new System.IO.MemoryStream(WpfApplication1.Properties.Resources.Myhand));
            //System.Windows.Forms.Cursor mycur = System.Windows.Forms.Cursors.Hand;
            
            //System.Windows.Forms.Cursor.Clip = 
            //this.Cursor = new System.Windows.Input.Cursor("C:\\Myhand.cur");


            InitializeComponent();

            // Create a sample listener and controller
            LeapListener listener = new LeapListener();
            Controller controller = new Controller();


            // Have the sample listener receive events from the controller
            controller.AddListener(listener);

            listener.OnFrame(controller);


            // Keep this process running until Enter is pressed
            Console.WriteLine("Press Enter to quit...");
            Console.ReadLine();

            // Remove the sample listener when done
            controller.RemoveListener(listener);
            controller.Dispose();

            System.Windows.Forms.Timer _timer = new Timer() { Interval = 1, Enabled = true };

            _timer.Tick += new EventHandler(Timer_Tick);

        }



        private void Timer_Tick(object sender, EventArgs e)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Hand;
                
                //g.Clear(System.Drawing.Color.Transparent);

                ///throw new NotImplementedException();
                System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;

                //RECT myrect = new RECT();
                //myrect.point = new System.Windows.Point(pt.X - 10, pt.Y - 10);
                //myrect.size = new System.Windows.Size(20, 20);

                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(new System.Drawing.Point(pt.X - 10, pt.Y - 10), new System.Drawing.Size(20, 20));

                //g.DrawEllipse(Pens.Black, pt.X - 10, pt.Y - 10, 20, 20);

                //g.DrawEllipse(Pens.Black, rect);

                //g.Dispose();

                RECT rc = new RECT(pt.X - 50, pt.Y - 50, pt.X + 50, pt.Y + 50);

                //RedrawWindow(IntPtr.Zero, ref rc, IntPtr.Zero, 0x0400/*RDW_FRAME*/ | 0x0100/*RDW_UPDATENOW*/| 0x0001/*RDW_INVALIDATE*/);

                osd1.Show(new System.Drawing.Point(pt.X - 25, pt.Y - 23), 155, System.Drawing.Color.Lime,
    new Font("Wingdings", 26f, System.Drawing.FontStyle.Regular),
                 500, FloatingWindow.AnimateMode.ExpandCollapse,
                 370, "l");
            }

        }
    }
}
    
    
