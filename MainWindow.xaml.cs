using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Threading;
using System.Configuration;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows.Controls;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Forms.MenuItem;
using TextBox = System.Windows.Controls.TextBox;


namespace clipboard
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        internal static NotifyIcon notifyIcon;

        public static ClipBoardManager clipBoardManager;
        public static ClipBoardMonitor clipMonitor;

        System.IntPtr handle = IntPtr.Zero;

        private static bool IsStart = Convert.ToBoolean(ConfigurationManager.AppSettings["IsStart"]);
        private static Int16 RecordCount = Convert.ToInt16(ConfigurationManager.AppSettings["Count"]);

        /// <summary> WPF窗口重写 </summary>
        protected override void OnSourceInitialized(EventArgs e)
        {

            base.OnSourceInitialized(e);

            this.win_SourceInitialized(this, e);

            // HTodo  ：添加剪贴板监视 
            handle = (new System.Windows.Interop.WindowInteropHelper(this)).Handle;

            ClipUnit.AddClipboardFormatListener(handle);

        }


        /// <summary> 添加监视消息 </summary>
        void win_SourceInitialized(object sender, EventArgs e)
        {

            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            hwndSource?.AddHook(new HwndSourceHook(clipMonitor.WndProc));

        }



        public MainWindow()
        {
            InitializeComponent();

            AutoStartConfig();

            clipMonitor = new ClipBoardMonitor(ClipHandle,WriteToFile);

            clipBoardManager = new ClipBoardManager();
            clipBoardManager.recordCount = RecordCount;

            HotKey hotKey = new HotKey(ModifierKeys.Control, Keys.Oem3, this);
            hotKey.HotKeyPressed += hotkey_Pressed;

            this.KeyDown += Get_KeyDown;
            ReadFromFile();
            ActiveWindow();
        }


        ~MainWindow()
        {
            notifyIcon.Visible = false;
            notifyIcon = null;
            ClipUnit.RemoveClipboardFormatListener(handle);
        }


        private void hotkey_Pressed(HotKey obj)
        {
            ActiveWindow(); //激活
            textBox.Text = "";
        }


        public void ShowActived()
        {
            try
            {
                Activate();
                Show();
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 失去焦点时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            CreateSystemTray();
        }

        private void Get_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)//Esc键  
            {
                CreateSystemTray();
            }
            else if (e.Key == Key.Enter)
            {
                DealEnter();
            }
        }

        private void DataGrid_OnGotFocus(object sender, RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            //here we just find the cell got focused ...
            //then we can use the cell key down or key up
            // iteratively traverse the visual tree
            while ((dep != null) && !(dep is System.Windows.Controls.DataGridCell) && !(dep is DataGridColumnHeader))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            if (dep is System.Windows.Controls.DataGridCell)
            {
                System.Windows.Controls.DataGridCell cell = dep as System.Windows.Controls.DataGridCell;
                //raise key down event of cell
                //                cell.IsSelected = true;
                cell.IsTabStop = false;
                cell.KeyDown += new System.Windows.Input.KeyEventHandler(cell_KeyDown);
            }
        }

        void cell_KeyDown(object sender, KeyEventArgs e)
        {
//            System.Windows.Controls.DataGridCell cell = sender as System.Windows.Controls.DataGridCell;
            if (e.Key == Key.Enter)
            {
                DealEnter();
                e.Handled = true;
            }

        }

        private void TextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                GetGridFocus(1);
            }
            else if (e.Key == Key.Down)
            {
                GetGridFocus(2);
            }

            textBox.Focus();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void SearchBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox text = sender as TextBox;
            string filter = text?.Text;

            dataGrid.ItemsSource = string.IsNullOrWhiteSpace(filter)?
                dataGrid.ItemsSource = clipBoardManager.ClipBoardSource :
                clipBoardManager.ClipBoardSource.Where( item=> item.Value.Contains(filter));

            dataGrid.SelectedIndex = 0;

            //            GetGridFocus();
        }


         /// <summary>
         /// 改变焦点
         /// </summary>
         /// <param name="changeType">0:获取默认，1:获取下一行，2:获取上一行</param>
        public void GetGridFocus(int changeType = 0)
        {
            if (changeType == 1)
            {
                if (dataGrid.SelectedIndex > 0)  
                    dataGrid.SelectedIndex -= 1;
            }
            else if (changeType == 2)
            {
                dataGrid.SelectedIndex += 1;
            }
            else
            {
                dataGrid.SelectedIndex = 0;
            }
            dataGrid.Focus();
            if (dataGrid.SelectedItem == null) { return; }
            System.Windows.Controls.DataGridCell cell = (dataGrid.Columns[0].GetCellContent(dataGrid.SelectedItem))?.Parent as System.Windows.Controls.DataGridCell; if (cell == null)
            {
                return;
            }
            cell.IsTabStop = false;
            cell.Focus();
        }

        /// <summary>
        /// 激活窗口
        /// </summary>
        public void ActiveWindow()
        {
            ClipUnit.POINT ptr = new ClipUnit.POINT();
            ClipUnit.GetCursorPos(out ptr);
            this.Top = ptr.Y + 10;
            this.Left = ptr.X + 10;
            if (!this.IsActive)
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    SystemCommands.RestoreWindow(this);
                }
                dataGrid.SelectedIndex = 0;

                ShowActived();

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(20);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        GetGridFocus();
                        textBox.Focus();
                    }));
                });
            }
        }

        /// <summary>
        /// 最小化到托盘
        /// </summary>
        public void CreateSystemTray()
        {
            //            if (notifyIcon != null) { return; }
            if (notifyIcon == null)
            {
                notifyIcon = new System.Windows.Forms.NotifyIcon
                {
                    Icon = new System.Drawing.Icon("clipboard.ico"),
                    Text = "ClipBoard",
                    ContextMenu = new System.Windows.Forms.ContextMenu()
                };
                notifyIcon.ContextMenu.MenuItems.Add("显示主窗口", OnSystemTray);

                notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("开机启动", OnSystemTray) { Checked = IsStart });

                notifyIcon.ContextMenu.MenuItems.Add("-");
                notifyIcon.ContextMenu.MenuItems.Add("退出", OnSystemTray);
                //                notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
            }
            notifyIcon.Visible = true;
            this.WindowState = WindowState.Minimized;
            this.Visibility = System.Windows.Visibility.Hidden;
            SystemCommands.MinimizeWindow(this);
            this.ShowInTaskbar = false;
        }

        private void OnSystemTray(object sender, EventArgs e)
        {
            var item = sender as System.Windows.Forms.MenuItem;
            var text = item?.Text;
            if (text == "显示主窗口" || Equals(sender, notifyIcon))
            {
                ActiveWindow();
                return;
            }

            if (text == "开机启动")
            {
                IsStart = item.Checked = !item.Checked;
                 
                SetAutoStartConfig(IsStart);
            }
            if (text == "退出")
            {
                notifyIcon.Visible = false;
                WriteToFile();
                Environment.Exit(0);
            }
        }

        private void SetAutoStartConfig(bool isStart)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["IsStart"].Value = isStart ? "true" : "false";
            // Save the changes in App.config file.
            config.Save(ConfigurationSaveMode.Modified);
            // Force a reload of a changed section.
            ConfigurationManager.RefreshSection("appSettings");
            AutoStartConfig();
        }

        private void AutoStartConfig()
        {
            string exeName = "pikaboard";
            AutoStart setAutoStart = new AutoStart(exeName);
            setAutoStart.SetAutoStart(IsStart);
        }

        public void ClipHandle()
        {
            clipBoardManager.ClipboardChanged();
            dataGrid.ItemsSource = clipBoardManager.ClipBoardSource;
        }

        private void DealEnter()
        {
            ClipBoardModel model = (ClipBoardModel)dataGrid.SelectedItem;
            clipBoardManager.CopyToBoard(model);

            CreateSystemTray();
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(200);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    IntPtr myPtr = ClipUnit.GetForegroundWindow();
                    ClipUnit.keybd_event(Keys.ControlKey, 0, 0, 0);
                    ClipUnit.keybd_event(Keys.V, 0, 0, 0);
                    ClipUnit.keybd_event(Keys.ControlKey, 0, 2, 0);
                }));
            });
        }

        private void WriteToFile()
        {
            string valueString = "";
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(System.Windows.Forms.Application.StartupPath, "BoardFile.txt")))
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                valueString += serializer.Serialize(clipBoardManager.ClipBoardSource);

                outputFile.WriteLine(valueString);

            }
        }

        private void ReadFromFile()
        {
            string line = "";
            using (StreamReader sr = new StreamReader(Path.Combine(System.Windows.Forms.Application.StartupPath, "BoardFile.txt")))
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                while ((line = sr.ReadLine()) != null)
                {
                    clipBoardManager.ClipBoardSource = serializer.Deserialize<ObservableCollection<ClipBoardModel>>(line);
                }

                dataGrid.ItemsSource = clipBoardManager.ClipBoardSource;
            }
        }
    }




}
