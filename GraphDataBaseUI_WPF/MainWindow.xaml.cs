using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GraphDB;
using GraphDB.Core;
using GraphDB.Layout;
using Microsoft.Win32;

//&lt; < 小于号 
//&gt; > 大于号 
//&amp; & 和 
//&apos; ' 单引号 
//&quot; " 双引号 
//(&#x0020;)  空格 
//(&#x0009;) Tab 
//(&#x000D;) 回车 
//(&#x000A;) 换行 

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
            ClearArrows(drawingSurface);
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

        #region Drawing
        private GraphDataBase SubGraph;
        private Brush drawingBrush = Brushes.AliceBlue;
        private Brush selectedDrawingBrush = Brushes.LightGoldenrodYellow;
        private Pen drawingPen = new Pen(Brushes.SteelBlue, 3);
        private Pen LinePen = new Pen(Brushes.Gray, 2);
        private Pen TextPen = new Pen(Brushes.White, 1);
        private List<Visual> visuals = new List<Visual>();
        private int radius = 20;
        private int intNodeIndex = -1;
        private bool bolScrolltoCenter = false;

        // Rendering the Ellipse.
        private void DrawEllipse(DrawingVisual visual, string sName, Point center, bool isSelected)
        {
            using (DrawingContext dc = visual.RenderOpen())
            {
                Brush brush = drawingBrush;
                if (isSelected) brush = selectedDrawingBrush;
                dc.DrawEllipse(brush, drawingPen, center, radius, radius);
                FormattedText text = new FormattedText(sName, 
                                                        System.Globalization.CultureInfo.CurrentCulture, 
                                                        System.Windows.FlowDirection.LeftToRight, 
                                                        new Typeface("Times New Roman"),
                                                        12,
                                                        Brushes.Black
                                                        );
                int Height = Convert.ToInt32(text.Height);
                int Width = Convert.ToInt32(text.Width);
                dc.DrawText(text, new Point(center.X-Width/2, center.Y - Height/2));
            }
        }

        // Rendering the Line
        private void DrawText(DrawingVisual visual, string sType, Point start, Point end)
        {
            using (DrawingContext dc = visual.RenderOpen())
            {
                Brush brush = drawingBrush;
                FormattedText text = new FormattedText(sType,
                                                        System.Globalization.CultureInfo.CurrentCulture,
                                                        System.Windows.FlowDirection.LeftToRight,
                                                        new Typeface("Times New Roman"),
                                                        12,
                                                        Brushes.Black
                                                        );
                int Height = Convert.ToInt32(text.Height);
                int Width = Convert.ToInt32(text.Width);
                Point center = new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2);
                dc.DrawRectangle(Brushes.White, TextPen, new Rect(new Point(center.X - Width / 2, center.Y - Height / 2), new Point(center.X + Width / 2, center.Y + Height / 2)));
                dc.DrawText(text, new Point(center.X - Width / 2, center.Y - Height / 2));
            }
        }

        private void NodeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectNodes(NodeListBox.SelectedIndex);
            MainScroll.ScrollToBottom();
            MainScroll.ScrollToRightEnd();
            bolScrolltoCenter = true;
        }

        private void DrawLeaderLineArrow(Point startPt, Point endPt)
        {
            Arrow arrow = new Arrow();
            arrow.X1 = startPt.X;
            arrow.Y1 = startPt.Y;
            arrow.X2 = endPt.X;
            arrow.Y2 = endPt.Y;
            arrow.HeadWidth = 7;
            arrow.HeadHeight = 3;
            arrow.Stroke = Brushes.Gray;
            arrow.StrokeThickness = 2;
            BackCanvas.Children.Add(arrow);
        }

        private void SelectNodes(int index)
        {
            ErrorCode err = ErrorCode.NoError;
            Node curSelNode;
            //string strDrawNodes;
            List<Node> DrawNodes;
            List<Node> NeibourNodes;

            if (index < 0)
            {
                return;
            }
            curSelNode = gdb.GetNodeByIndex(index);
            if (curSelNode == null)
            {
                return;
            }
            DrawNodes = new List<Node>();
            NeibourNodes = new List<Node>();
            DrawNodes.Add(curSelNode);
            SubGraph = new GraphDataBase(curSelNode.OutBound.Count, radius);
            SubGraph.AddNodeData(curSelNode, ref err);
            //strDrawNodes = curSelNode.Number + "-" + curSelNode.Name + "\n";
            foreach (Edge edge in curSelNode.OutBound)
            {
                NeibourNodes.Add(edge.End);
                DrawNodes.Add(edge.End);
                SubGraph.AddNodeData(edge.End, ref err);
                SubGraph.AddEdgeData(curSelNode.Name, curSelNode.Type, edge.End.Name, edge.End.Type, edge.Type, ref err, edge.Value);
                //strDrawNodes += edge.Type + ":" + edge.End.Number + "-" + edge.End.Name + "\n";
            }
            foreach (Node node in NeibourNodes)
            {
                foreach (Edge edge in node.InBound)
                {
                    if (DrawNodes.IndexOf(edge.Start) < 0)
                    {
                        DrawNodes.Add(edge.Start);
                        SubGraph.AddNodeData(edge.Start, ref err);
                        //strDrawNodes += "被" + edge.Type + ":" + edge.Start.Number + "-" + edge.Start.Name + "\n";
                    }
                    if ((edge.Start.Name != curSelNode.Name || edge.Start.Type != curSelNode.Type)
                        && (NeibourNodes.IndexOf(edge.Start) < 0))
                    {
                        SubGraph.AddEdgeData(edge.Start.Name, edge.Start.Type, node.Name, node.Type, edge.Type, ref err, edge.Value);
                        //strDrawNodes += "被" + edge.Type + ":" + edge.Start.Number + "-" + edge.Start.Name + "\n";
                    }
                }
            }
            //ResultBox.Text = strDrawNodes;
            SubGraph.StartCicro();
            DrawGarph();
        }

        private void DrawGarph()
        {
            List<NodeDrawing> NodeDrawings;
            List<EdgeDrawing> EdgeDrawings;

            NodeDrawings = SubGraph.GetLayoutNodePoints();
            EdgeDrawings = SubGraph.GetLayoutEdgePoints();
            ClearArrows(drawingSurface);
            visuals.Clear();
            drawingSurface.ClearVisuals();
            drawingSurface.Width = SubGraph.Width;
            drawingSurface.Height = SubGraph.Height;
            foreach (EdgeDrawing edge in EdgeDrawings)
            {
                DrawingVisual visual = new DrawingVisual();
                Point Start = new Point(edge.StartPosition.X, edge.StartPosition.Y);
                Point End = new Point(edge.EndPosition.X, edge.EndPosition.Y);
                DrawText(visual, edge.Type, Start, End);
                End = ModifyPositiion(Start, End, radius);
                DrawLeaderLineArrow(Start, End);
                drawingSurface.AddVisual(visual);
            }
            BringToFront(drawingSurface);
            foreach (NodeDrawing node in NodeDrawings)
            {
                DrawingVisual visual = new DrawingVisual();
                DrawEllipse(visual, node.Name, new Point(node.Position.X, node.Position.Y), false);
                visuals.Add(visual);
                drawingSurface.AddVisual(visual);
            }
        }

        private Point ModifyPositiion(Point startPt, Point endPt, int iRadius)
        {
            int x, y;
            double deltaX, deltaY;
            double dubDistance;

            deltaX = startPt.X - endPt.X;
            deltaY = startPt.Y - endPt.Y;
            dubDistance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if(dubDistance == 0)
            {
                return endPt;
            }
            x = Convert.ToInt32((deltaX * iRadius) / dubDistance);
            y = Convert.ToInt32((deltaY * iRadius) / dubDistance);
            return new Point(endPt.X + x, endPt.Y + y);
        }

        private void ClearArrows(UIElement element)
        {
            BackCanvas.Children.Clear();
            BackCanvas.Children.Add(element);
        }

        private void BringToFront(DrawingCanvas element)//图片置于最顶层显示
        {
            if (element == null)
            {
                return;
            }
            Canvas parent = element.Parent as Canvas;
            if (parent == null)
            {
                return;
            }
            var maxZ = parent.Children.OfType<UIElement>()//linq语句，取Zindex的最大值
              .Where(x => x != element)
              .Select(x => Canvas.GetZIndex(x))
              .Max();
            Canvas.SetZIndex(element, maxZ + 1);
        }
       
        private void MainScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double dubBottom, dubRight;
            if (bolScrolltoCenter == true)
            {
                dubBottom = MainScroll.VerticalOffset;
                dubRight = MainScroll.HorizontalOffset;
                MainScroll.ScrollToVerticalOffset(dubBottom / 2);
                MainScroll.ScrollToHorizontalOffset(dubRight / 2);
                bolScrolltoCenter = false;
            }
        }
        #endregion

        private void drawingSurface_MouseMove(object sender, MouseEventArgs e)
        {
            int visualindex = GetVisualIndex(drawingSurface.GetVisual(e.GetPosition(drawingSurface)));

            if (visualindex == -1 || visualindex == intNodeIndex || visualindex >= SubGraph.NodeNum)
            {
                return;
            }
            else
            {
                ToolTip NodeTip = BuildNewTip(visualindex);
                NodeTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
                intNodeIndex = visualindex;
                drawingSurface.ToolTip = NodeTip;
            }
        }

        private  int GetVisualIndex(DrawingVisual visual)
        {
            int index = 0;
            if (visual == null)
            {
                return -1;
            }
            foreach (DrawingVisual localVisual in visuals)
            {
                if (localVisual.Equals(visual) == true)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        private ToolTip BuildNewTip(int index)
        {
            ToolTip NodeTip = new ToolTip();

            NodeTip.Content = SubGraph.Nodes[index].DataOutput();

            return NodeTip;
        }
    }
}
