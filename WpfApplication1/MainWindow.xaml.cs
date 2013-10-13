using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using Leap;


namespace WpfApplication1
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FloatingOSDWindow osd1 = new FloatingOSDWindow();


            //Create a sample listener and controller
            LeapListener listener = new LeapListener();
            Controller controller = new Controller();



        public MainWindow()
        {


            InitializeComponent();


            // Keep this process running until Enter is pressed
           // Console.WriteLine("Press Enter to quit...");
            //Console.ReadLine();

            // Remove the sample listener when done
            //controller.RemoveListener(listener);
            //controller.Dispose();


            System.Windows.Forms.Timer _timer = new Timer() { Interval = 1, Enabled = true };


//            _timer.Tick += (sender,e) => Timer_Tick(sender,e,listener,controller);
            _timer.Tick += (sender, e) => Timer_Tick(sender, e);


        }


//        private void Timer_Tick(object sender, EventArgs e, LeapListener listener, Controller controller)
        private void Timer_Tick(object sender, EventArgs e)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {

                ///throw new NotImplementedException();
                System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;

                // Have the sample listener receive events from the controller
                controller.AddListener(listener);

                //X-position of leap cursor
                
                Leap.Vector leapVec = listener.trackLeapCursor(controller);
                int LeapX = 0;
                int LeapY = 0;

                if (leapVec.x != 0 || leapVec.y != 0)
                {

                    System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.None;

                    LeapX = (int)leapVec.x;
                    LeapY = (int)leapVec.y;

                    string action = listener.get_action_type();

                    switch (action)
                    {
                        case "normal":
                        //Show normal cursor
                        osd1.Show(new System.Drawing.Point(LeapX, LeapY), 155, System.Drawing.Color.Lime,
                        new Font("Wingdings", 26f, System.Drawing.FontStyle.Regular),
                        500, FloatingWindow.AnimateMode.ExpandCollapse,
                        370, "l");
                        break;

                        case "left_click":
                            //Show left click trigger cursor
                            osd1.Show(new System.Drawing.Point(LeapX, LeapY), 155, System.Drawing.Color.DarkGreen,
                            new Font("Wingdings", 20f, System.Drawing.FontStyle.Regular),
                            500, FloatingWindow.AnimateMode.ExpandCollapse,
                            370, "l");
                        break;

                        case "right_click":
                            osd1.Show(new System.Drawing.Point(LeapX, LeapY), 155, System.Drawing.Color.Red,
                            new Font("Wingdings", 20f, System.Drawing.FontStyle.Regular),
                            500, FloatingWindow.AnimateMode.ExpandCollapse,
                            370, "l");
                        break;

                    
                    }

                }

            }

        }
    }
}
    
    
