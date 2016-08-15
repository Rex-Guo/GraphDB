using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GraphDB;
using GraphDB.Core;
using Microsoft.Win32;

//&lt; < 小于号 
//&gt; > 大于号 
//&amp; & 和 
//&apos; ' 单引号 
//&quot; " 双引号 

namespace GraphDataBaseUI_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        GraphDataBase gdb;
        bool isDbAvailable = false;
        DispatcherTimer StatusUpadteTimer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ErrorCode err = ErrorCode.NoError;
            StatusUpdateTimer_Init();

            gdb = new GraphDataBase();
            gdb.OpenDataBase("1.xml", ref err);
            FillNodeList();
            isDbAvailable = true;
        }

        private void FillNodeList()
        {
            string strItem;
            foreach (Node curNode in gdb.Nodes)
            {
                strItem = curNode.Number + " 名称:" + curNode.Name + " 类型:" + curNode.Type;
                this.NodeListBox.Items.Add(strItem);
            }
        }

        #region StatusTimer
        private void StatusUpdateTimer_Init()
        {
            StatusUpadteTimer = new DispatcherTimer();
            StatusUpadteTimer.Interval = new TimeSpan(0, 0, 3);
            StatusUpadteTimer.Tick += new EventHandler(StatusUpdateTimer_Tick);
            StatusUpadteTimer.IsEnabled = false;
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            StatusLabel.Content = "Ready";
            StatusUpadteTimer.IsEnabled = false;
        }
        #endregion

        private void AllReset()
        {
            gdb = null;
            isDbAvailable = false;
            NodeListBox.Items.Clear();
            drawingSurface.ClearVisuals();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorCode err = ErrorCode.NoError;
            string strResult, strCommand;

            //START node('*-国家') MATCH (Kingdom)-[:统治]->(District)<-[:连通 5..5]-(Neibhour) WHERE * RETURN Kingdom.Name, District.*
            strCommand = CommandBox.Text;
            strResult = gdb.DataQueryExecute(strCommand, ref err);//
            ResultBox.Text = strResult;
        }
        
        #region FileCommand
        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult choice;
            ErrorCode err = ErrorCode.NoError;
            SaveFileDialog savedialog;
            string strPath;

            if (isDbAvailable == true)
            {
                choice = MessageBox.Show("Save current graph database to file？", "警告", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                if (choice == MessageBoxResult.Yes)
                {
                    //保存网络
                    SaveFile(ref err);
                    if (err != ErrorCode.NoError)
                    {
                        StatusLabel.Content = "Save Failed";
                        StatusUpadteTimer.Start();
                        return;
                    }
                }
                else if (choice == MessageBoxResult.No)
                {
                }
                else if (choice == MessageBoxResult.Cancel)
                {
                    return;
                }
                AllReset();
            }
            gdb = new GraphDataBase();
            //初始化对话框，文件类型，过滤器，初始路径等设置
            savedialog = new SaveFileDialog();
            savedialog.Filter = "XML files (*.xml)|*.xml";
            savedialog.FilterIndex = 0;
            savedialog.RestoreDirectory = true;
            //成功选取文件后，根据文件类型执行读取函数
            if (savedialog.ShowDialog() != true)
            {
                return;
            }
            Cursor = Cursors.Wait;
            strPath = savedialog.FileName;
            gdb = new GraphDataBase();
            gdb.CreateDataBase(strPath, ref err);
            Cursor = Cursors.Arrow;
            if (err != ErrorCode.NoError)
            {
                MessageBox.Show("Can not open file.", "警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                StatusLabel.Content = "Create Failed";
                StatusUpadteTimer.Start();
                return;
            }
            StatusLabel.Content = "Create Success";
            StatusUpadteTimer.Start();
            isDbAvailable = true;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult choice;
            OpenFileDialog opendialog;
            ErrorCode err = ErrorCode.NoError;
            string strPath;

            if (isDbAvailable == true)
            {
                choice = MessageBox.Show("Save current graph database to file？", "警告", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                if (choice == MessageBoxResult.Yes)
                {
                    //保存网络
                    SaveFile(ref err);
                    if (err != ErrorCode.NoError)
                    {
                        StatusLabel.Content = "Save Failed";
                        StatusUpadteTimer.Start();
                        return;
                    }
                }
                else if (choice == MessageBoxResult.No)
                {
                }
                else if (choice == MessageBoxResult.Cancel)
                {
                    return;
                }
                AllReset();
            }
            //初始化对话框，文件类型，过滤器，初始路径等设置
            opendialog = new OpenFileDialog();
            opendialog.Filter = "All files (*.*)|*.*|XML files (*.xml)|*.xml";
            opendialog.FilterIndex = 0;
            opendialog.RestoreDirectory = true;
            //成功选取文件后，根据文件类型执行读取函数
            if (opendialog.ShowDialog() != true)
            {
                return;
            }
            Cursor = Cursors.Wait;
            strPath = opendialog.FileName;
            gdb = new GraphDataBase();
            gdb.OpenDataBase(strPath, ref err);
            Cursor = Cursors.Arrow;
            if (err != ErrorCode.NoError)
            {
                MessageBox.Show("Can not open file.", "警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                StatusLabel.Content = "Open Failed";
                StatusUpadteTimer.Start();
                return;
            }
            FillNodeList();
            StatusLabel.Content = "Open Success";
            StatusUpadteTimer.Start();
            isDbAvailable = true;
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ErrorCode err = ErrorCode.NoError;

            //保存网络
            SaveFile(ref err);
            if (err != ErrorCode.NoError)
            {
                StatusLabel.Content = "Save Failed";
                StatusUpadteTimer.Start();
                return;
            }
            StatusLabel.Content = "Save Success";
            StatusUpadteTimer.Start();
        }

        private void SaveFile(ref ErrorCode err)
        {
            gdb.DataExport(ref err);
        }

        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog savedialog;
            ErrorCode err = ErrorCode.NoError;
            string strPath;

            //调出另存为对话框
            //初始化对话框，文件类型，过滤器，初始路径等设置
            savedialog = new SaveFileDialog();
            savedialog.Filter = "XML files (*.xml)|*.xml";
            savedialog.FilterIndex = 0;
            savedialog.RestoreDirectory = true;
            //成功选取文件后，根据文件类型执行读取函数
            if (savedialog.ShowDialog() != true)
            {
                return;
            }
            Cursor = Cursors.Wait;
            strPath = savedialog.FileName;
            //切换IO句柄中的目标地址,并保存
            gdb.DataBaseSaveAs(strPath, ref err);
            Cursor = Cursors.Arrow;
            if (err != ErrorCode.NoError)
            {
                MessageBox.Show("Can not save file.", "警告", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                StatusLabel.Content = "Save As Failed";
                StatusUpadteTimer.Start();
                return;
            }
            StatusLabel.Content = "Save As Success";
            StatusUpadteTimer.Start();
        }

        private void QuickPrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void PrintPreviewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void PrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult choice;
            ErrorCode err = ErrorCode.NoError;
            if (isDbAvailable == true)
            {
                choice = MessageBox.Show("Save current graph database to file？", "警告", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                if (choice == MessageBoxResult.Yes)
                {
                    //保存网络
                    SaveFile(ref err);
                    if (err != ErrorCode.NoError)
                    {
                        StatusLabel.Content = "Save Failed";
                        StatusUpadteTimer.Start();
                        return;
                    }
                }
                else if (choice == MessageBoxResult.No)
                {
                }
                else if (choice == MessageBoxResult.Cancel)
                {
                    return;
                }
                StatusLabel.Content = "Database Closed";
                StatusUpadteTimer.Start();
                AllReset();
            }
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult choice;
            ErrorCode err = ErrorCode.NoError;
            if (isDbAvailable == true)
            {
                choice = MessageBox.Show("Save current graph database to file？", "警告", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                if (choice == MessageBoxResult.Yes)
                {
                    //保存网络
                    SaveFile(ref err);
                    if (err != ErrorCode.NoError)
                    {
                        StatusLabel.Content = "Save Failed";
                        StatusUpadteTimer.Start();
                        return;
                    }
                }
                else if (choice == MessageBoxResult.No)
                {
                }
                else if (choice == MessageBoxResult.Cancel)
                {
                    return;
                }
                AllReset();
            }
            
            this.Close();
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        private void SaveAsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        private void QuickPrintCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        private void PrintPreviewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        private void PrintCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }
        #endregion


        private Brush drawingBrush = Brushes.AliceBlue;
        private Brush selectedDrawingBrush = Brushes.LightGoldenrodYellow;
        private Pen drawingPen = new Pen(Brushes.SteelBlue, 3);
        private Size squareSize = new Size(30, 30);

        private void drawingSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pointClicked = e.GetPosition(drawingSurface);

            
            DrawingVisual visual = new DrawingVisual();
            DrawEllipse(visual, pointClicked, false);
            drawingSurface.AddVisual(visual);
        }

        // Rendering the square.
        private void DrawEllipse(DrawingVisual visual, Point center, bool isSelected)
        {
            using (DrawingContext dc = visual.RenderOpen())
            {
                Brush brush = drawingBrush;
                if (isSelected) brush = selectedDrawingBrush;
                dc.DrawEllipse(brush, drawingPen, center, 20, 20);
            }
        }

    }
}
