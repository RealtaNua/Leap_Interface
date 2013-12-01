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
using System.Configuration;
using Leap;



namespace LeapTouchPoint
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FloatingOSDWindow osd1 = new FloatingOSDWindow();

            //Create a Leap listener and controller
            static LeapListener listener = new LeapListener();
            static Controller controller = new Controller();
            
            [DllImport("user32.dll")]
            static extern IntPtr LoadCursorFromFile(string lpFileName);

            [DllImport("user32.dll")]
            static extern IntPtr SetCursor(IntPtr hCursor);

            [DllImport("user32.dll")]
            static extern bool SetSystemCursor(IntPtr hcur, uint id);

            [DllImport("user32.dll")]
            static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr GetCursor();

            [DllImport("user32.dll", SetLastError = true)]
            static extern bool SystemParametersInfo(int uiAction, int uiParam, IntPtr pvParam, int fWinIni);

            
            private const uint OCR_NORMAL = 32512;
            public static int IDC_ARROW = 32512;
            private static Boolean cursor_is_blank = false;
            private ConfigFileSettings configFile = new ConfigFileSettings();
        
        public MainWindow()
        {


            InitializeComponent();

            //LeapTouchPointConfigWindow systraywindow = new LeapTouchPointConfigWindow();
            //systraywindow.Show();

            if (!isTrayIconStarted)
            {
                SysTrayApp();
            }

            loadConfigFileParameters();
            listener.setConfigFileParameters();
            
            System.Windows.Forms.Timer _timer = new Timer() { Interval = 1, Enabled = true };

            // Have the listener receive events from the controller
            controller.AddListener(listener);

            try
            {
                _timer.Tick += (sender, e) => Timer_Tick(sender, e);
            }
            finally
            {
                // Remove the listener when done
                //controller.RemoveListener(listener);
                //controller.Dispose();
            }

            // Subscribe to closing event (when X is pressed)
            this.Closing += ConfigWindow_OnClose;

            this.StateChanged += ConfigWindow_OnMinimize;

            applyAll.Click += applyAll_Click;

        }


        private static NotifyIcon trayIcon;
        private static System.Windows.Forms.ContextMenu trayMenu = new System.Windows.Forms.ContextMenu();
        private Boolean isTrayIconStarted = false;
        private static Boolean isStopped = false;
        private static Boolean isExit = false;
        private static Dictionary<string,string> configDic = new Dictionary<string,string>();
        //static LeapTouchPointConfigWindow config_window = new LeapTouchPointConfigWindow();

        private void loadConfigFileParameters()
        {
            configDic = configFile.getDefaultConfigParameters();
            fineSensitivitySlider.Value = double.Parse(configDic["fineSensitivitySlider"]);
            yAxisSlider.Value = double.Parse(configDic["yAxisSlider"]);
            secondsBeforeLocking.Text = configDic["secondsBeforeLocking"];
        }

        private void saveConfigFileParameters()
        {
            configDic["fineSensitivitySlider"] = fineSensitivitySlider.Value.ToString();
            configDic["yAxisSlider"] = yAxisSlider.Value.ToString();
            configDic["secondsBeforeLocking"] = secondsBeforeLocking.Text;
            configFile.setConfigParameters(configDic);
        }

        void SysTrayApp()
        {

            // Create a simple tray menu
            trayMenu.MenuItems.Add("Start", OnStart);
            trayMenu.MenuItems.Add("Stop", OnStop);
            trayMenu.MenuItems.Add("Exit", OnExit);


            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "TouchPoint";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            trayIcon.DoubleClick += new EventHandler(iconDoubleClick);
            /*
            trayIcon.BalloonTipClicked += new EventHandler(icon_BalloonTipClicked);
            trayIcon.DoubleClick += new EventHandler(icon_DoubleClick);
            trayIcon.BalloonTipClosed += new EventHandler(icon_BalloonTipClosed);
            trayIcon.MouseMove += new System.Windows.Forms.MouseEventHandler(icon_MouseMove);
            StateChanged += new EventHandler(MainWindow_StateChanged);
            */
            isTrayIconStarted = true;
        }

        void applyAll_Click(object sender, EventArgs e)
        {
            saveConfigFileParameters();
        }

        void iconDoubleClick(object sender, EventArgs e)
        {
            this.Show();

            this.Activate();
        }

        void ConfigWindow_OnMinimize(object sender, EventArgs e)
        {
            //Prevent window from minimizing
            this.WindowState = WindowState.Normal;

            //Hide window
            this.Hide();


        }

        void ConfigWindow_OnClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isExit)
            {
                // Prevent window from closing
                e.Cancel = true;

                // Hide window
                this.Hide();

                trayIcon.BalloonTipText = "TouchPoint is still running. Right-click to exit.";
                trayIcon.ShowBalloonTip(1);
            }
        }


        void OnStart(object sender, EventArgs e)
        {
            if (isStopped)
            {
                //Create a sample listener and controller
                listener = new LeapListener();
                controller = new Controller();

                // Have the sample listener receive events from the controller
                controller.AddListener(listener);

                isStopped = false;
            }
        }

        void OnStop(object sender, EventArgs e)
        {
            if (!isStopped)
            {
                // Remove the listener when done
                controller.RemoveListener(listener);
                controller.Dispose();
                listener.Dispose();

                isStopped = true;
            }
        }

        void OnExit(object sender, EventArgs e)
        {
            isExit = true;
            trayIcon.Dispose();
            trayMenu.Dispose();

            System.Windows.Application.Current.Shutdown();
        }




        private void Timer_Tick(object sender, EventArgs e)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {

                //Call lockOrUnlock function to lock or unlock screen based on preexisting logic
                Boolean isLocked = listener.lockOrUnlock();

                if (!isLocked)
                {
                    //Show text indicating screen is locked
                    osd1.Show(new System.Drawing.Point(500, 500), 155, System.Drawing.Color.DarkGreen,
                    new Font("Arial", 20f, System.Drawing.FontStyle.Regular),
                    500, FloatingWindow.AnimateMode.ExpandCollapse,
                    370, "UNLOCKED");

                    Leap.Vector leapVec = listener.trackLeapCursor(controller);
                    int LeapX = 0;
                    int LeapY = 0;

                    if (leapVec.x != 0 || leapVec.y != 0)
                    {

                        //System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.None;

                        LeapX = (int)leapVec.x;
                        LeapY = (int)leapVec.y;

                        string action = listener.get_action_type();

                        switch (action)
                        {
                            case "normal":
                                //Show normal cursor
                                /*
                                osd1.Show(new System.Drawing.Point(LeapX, LeapY), 155, System.Drawing.Color.Lime,
                                new Font("Wingdings", 26f, System.Drawing.FontStyle.Regular),
                                500, FloatingWindow.AnimateMode.ExpandCollapse,
                                370, "l");
                                */
                                //MouseInput.MoveMouse(new System.Drawing.Point(LeapX,LeapY));

                                listener.LeapMoveMouse(LeapX, LeapY);

                                break;

                            case "left_click":
                                //Show left click trigger cursor
                                /*
                                osd1.Show(new System.Drawing.Point(LeapX, LeapY), 155, System.Drawing.Color.DarkGreen,
                                new Font("Wingdings", 20f, System.Drawing.FontStyle.Regular),
                                500, FloatingWindow.AnimateMode.ExpandCollapse,
                                370, "l");
                                 */
                                //MouseInput.MoveMouse(new System.Drawing.Point(LeapX, LeapY));                            

                                listener.LeapMoveMouse(LeapX, LeapY);

                                break;

                            case "right_click":
                                /*
                                osd1.Show(new System.Drawing.Point(LeapX, LeapY), 155, System.Drawing.Color.Red,
                                new Font("Wingdings", 20f, System.Drawing.FontStyle.Regular),
                                500, FloatingWindow.AnimateMode.ExpandCollapse,
                                370, "l");
                                 */


                                break;


                        }

                        //string blank_cursor_location = @".\Resources\blank.cur";

                        if (cursor_is_blank == false)
                        {
                            try
                            {
                                //default_cursor = LoadCursor(IntPtr.Zero,IDC_ARROW);

                                //string blank_cursor_location = "C:\\Users\\AdminNUS\\Documents\\GitHub\\Leap_Interface\\LeapTouchPoint\\Resources\\blank.cur";
                                //System.IntPtr hide_cursor = LoadCursorFromFile(blank_cursor_location);

                                //SetSystemCursor(hide_cursor, OCR_NORMAL);
                            }
                            finally
                            {
                                //SetSystemCursor(default_cursor, OCR_NORMAL);
                            }
                        }


                    }
                    else
                    {
                        //string arrow_cursor_location = "C:\\Users\\AdminNUS\\Documents\\GitHub\\Leap_Interface\\LeapTouchPoint\\Resources\\aero_arrow.cur";
                        //default_cursor = LoadCursorFromFile(arrow_cursor_location);

                        //int SPI_SETCURSORS = 0x0057;
                        //SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, 0);

                        //SetCursor(LoadCursor(IntPtr.Zero,IDC_ARROW));
                        //SetSystemCursor(default_cursor, OCR_NORMAL);

                    }
                } //END isLocked = False
                else
                {
                    osd1.Show(new System.Drawing.Point(500, 500), 155, System.Drawing.Color.DarkGreen,
                                new Font("Arial", 20f, System.Drawing.FontStyle.Regular),
                                20, FloatingWindow.AnimateMode.ExpandCollapse,
                                10, "LOCKED");
                }
            }

        }
    }
}
    
    
