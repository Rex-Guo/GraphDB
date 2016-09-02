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
using Microsoft.Windows.Controls.Ribbon;

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
        int intNodeIndex = -1;
        int intPointNodeIndex = -1;
        Node curModifyNode;
        Edge curModifyEdge;
        NodeInfo curSelectNode;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ErrorCode err = ErrorCode.NoError;
            AllReset();
            ChangeStyle("默认样式");
            StatusUpdateTimer_Init();
            gdb = new GraphDataBase();
            gdb.OpenDataBase("1.xml", ref err);
            FillNodeList();
            isDbAvailable = true;
        }

        private void FillNodeList()
        {
            string strItem;
            NodeListBox.Items.Clear();
            foreach (Node curNode in gdb.Nodes)
            {
                strItem = curNode.Number + " 名称:" + curNode.Name + " 类型:" + curNode.Type;
                this.NodeListBox.Items.Add(strItem);
            }
        }

        //完全重置
        private void AllReset()
        {
            gdb = null;
            curModifyNode = null;
            curModifyEdge = null;
            isDbAvailable = false;
            intNodeIndex = -1;
            intPointNodeIndex = -1;
            SetCurrentNodeInfo(-1);
            NodeListBox.Items.Clear();
            ClearArrows(drawingSurface);
            drawingSurface.ClearVisuals();
            ModifyEndName.Items.Clear();
            ModifyEndType.Items.Clear();
            RemoveEndName.Items.Clear();
            RemoveEndType.Items.Clear();
        }
        //节点更新
        private void GraphNodeUpdate()
        {
            curModifyNode = null;
            curModifyEdge = null;
            intNodeIndex = -1;
            SetCurrentNodeInfo(-1);
            NodeListBox.Items.Clear();
            ClearArrows(drawingSurface);
            drawingSurface.ClearVisuals();
            ModifyEndName.Items.Clear();
            ModifyEndType.Items.Clear();
            RemoveEndName.Items.Clear();
            RemoveEndType.Items.Clear();
            FillNodeList();
        }
        //连边更新
        private void GraphEdgeUpdate()
        {
            curModifyEdge = null;
            ClearArrows(drawingSurface);
            drawingSurface.ClearVisuals();
            SelectNodes(intNodeIndex);
            FindCustomNode(curNodeName, curNodeType);
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

        public void ShowStatus(string sStatus)
        {
            StatusLabel.Content = sStatus;
            StatusUpadteTimer.Start();
        }
        #endregion
        
        #region FileCommand
        //新建命令执行函数
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
                        ShowStatus("Save Failed");
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
                ShowStatus("Create Failed.");
                return;
            }
            ShowStatus("Create Success.");
            isDbAvailable = true;
        }
        //打开文件命令执行函数
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
                        ShowStatus("Save Failed.");
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
                ShowStatus("Open Failed.");
                return;
            }
            FillNodeList();
            ShowStatus("Open Success.");
            isDbAvailable = true;
        }
        //保存命令执行函数
        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ErrorCode err = ErrorCode.NoError;

            //保存网络
            SaveFile(ref err);
            if (err != ErrorCode.NoError)
            {
                ShowStatus("Save Failed.");
                return;
            }
            ShowStatus("Save Success.");
        }
        //保存文件
        private void SaveFile(ref ErrorCode err)
        {
            gdb.DataExport(ref err);
        }
        //另存为命令执行函数
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
                ShowStatus("Save As Failed.");
                return;
            }
            ShowStatus("Save As Success.");
        }
        //快速打印命令执行函数
        private void QuickPrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        //打印预览命令执行函数
        private void PrintPreviewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        //打印命令执行函数
        private void PrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        //关闭数据库执行函数
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
                        ShowStatus("Save Failed.");
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
                ShowStatus("Database Closed.");
                AllReset();
            }
        }
        //退出程序执行函数
        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        //关闭窗体前检查
        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult choice;
            ErrorCode err = ErrorCode.NoError;
            if (isDbAvailable == true)
            {
                choice = MessageBox.Show("Save current graph database to file？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (choice == MessageBoxResult.Yes)
                {
                    //保存网络
                    SaveFile(ref err);
                    if (err != ErrorCode.NoError)
                    {
                        ShowStatus("Save Failed.");
                        return;
                    }
                }
                else if (choice == MessageBoxResult.No)
                {
                }
                AllReset();
            }
        }
        //保存命令使能
        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }
        //另存为命令使能
        private void SaveAsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }
        //快速打印命令使能
        private void QuickPrintCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }
        //打印预览命令使能
        private void PrintPreviewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }
        //打印命令使能
        private void PrintCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }
        //关闭数据库命令使能
        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        
        #endregion

        #region Drawing
        private GraphDataBase SubGraph;
        private Brush drawingBrush = Brushes.AliceBlue;
        private Pen drawingPen = new Pen(Brushes.SteelBlue, 3);
        private Brush TextBrush = Brushes.Black;
        private Pen LinePen = new Pen(Brushes.Gray, 2);
        private Pen TextPen = new Pen(Brushes.White, 1);
        private List<Visual> visuals = new List<Visual>();
        private int radius = 20;
        private bool bolScrolltoCenter = false;

        // 渲染原型.
        private void DrawEllipse(DrawingVisual visual, string sName, Point center)
        {
            using (DrawingContext dc = visual.RenderOpen())
            {
                Brush brush = drawingBrush;

                dc.DrawEllipse(brush, drawingPen, center, radius, radius);
                FormattedText text = new FormattedText(sName, 
                                                        System.Globalization.CultureInfo.CurrentCulture, 
                                                        System.Windows.FlowDirection.LeftToRight, 
                                                        new Typeface("Times New Roman"),
                                                        12,
                                                        TextBrush
                                                        );
                int Height = Convert.ToInt32(text.Height);
                int Width = Convert.ToInt32(text.Width);
                dc.DrawText(text, new Point(center.X-Width/2, center.Y - Height/2));
            }
        }
        // 绘制连边文本
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
        //绘制连边形状
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
        //选择节点
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
            SetCurrentNodeInfo(index);
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
        //绘制节点图
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
                DrawEllipse(visual, node.Name, new Point(node.Position.X, node.Position.Y));
                visuals.Add(visual);
                drawingSurface.AddVisual(visual);
            }
        }
        //位置修正
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
        //清除连边形状
        private void ClearArrows(UIElement element)
        {
            BackCanvas.Children.Clear();
            BackCanvas.Children.Add(element);
        }
        //将节点图置于顶层
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
            if (SubGraph.NodeNum == 1)
            {
                return;
            }
            var maxZ = parent.Children.OfType<UIElement>()//linq语句，取Zindex的最大值
              .Where(x => x != element)
              .Select(x => Canvas.GetZIndex(x))
              .Max();
            Canvas.SetZIndex(element, maxZ + 1);
        }
        //获取visual索引
        private int GetVisualIndex(DrawingVisual visual)
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
        //构造新的ToolTip
        private ToolTip BuildNewTip(int index)
        {
            ToolTip NodeTip = new ToolTip();

            NodeTip.Content = SubGraph.Nodes[index].DataOutput();

            return NodeTip;
        }
        #endregion

        #region UICommand
        //查询按钮执行函数
        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorCode err = ErrorCode.NoError;
            string strResult, strCommand;

            if (isDbAvailable == false)
            {
                return;
            }
            //START node('*-国家') MATCH (Kingdom)-[:统治]->(District)<-[:连通 5..5]-(Neibhour) WHERE * RETURN Kingdom.Name, District.*
            strCommand = CommandBox.Text;
            strResult = gdb.DataQueryExecute(strCommand, ref err);//
            ResultBox.Text = strResult;
        }
        //节点列表框选中事件处理函数
        private void NodeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectNodes(NodeListBox.SelectedIndex);
            MainScroll.ScrollToBottom();
            MainScroll.ScrollToRightEnd();
            bolScrolltoCenter = true;
        }
        //主滚动框自动居中
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
        //画布鼠标移动事件-节点标签显示
        private void drawingSurface_MouseMove(object sender, MouseEventArgs e)
        {
            int visualindex = GetVisualIndex(drawingSurface.GetVisual(e.GetPosition(drawingSurface)));

            PointLabel.Content = visualindex.ToString();
            if (visualindex == -1 ||  visualindex >= SubGraph.NodeNum)
            {
                return;
            }
            ToolTip NodeTip = BuildNewTip(visualindex);
            intPointNodeIndex = visualindex;
            NodeTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
            drawingSurface.ToolTip = NodeTip;
        }
        //画布鼠标点击事件-切换选中节点并重新绘图
        private void drawingSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int visualindex = GetVisualIndex(drawingSurface.GetVisual(e.GetPosition(drawingSurface)));
            int intNode;
            string strName, strType;
            if (visualindex == -1 || visualindex >= SubGraph.NodeNum)
            {
                return;
            }
            else
            {
                strName = SubGraph.Nodes[visualindex].Name;
                strType = SubGraph.Nodes[visualindex].Type;
                intNode = gdb.GetIndexByNameAndType(strName, strType);
                if (intNode == -1)
                {
                    return;
                }
                NodeListBox.SelectedIndex = intNode;
            }
        }
        //清除命令框内容命令执行
        private void ClearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CommandBox.Text = "";
        }
        //清除命令框按钮使能
        private void ClearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (CommandBox == null)
            {
                return;
            }
            if (e.CanExecute == false && CommandBox.Text != "")
            {
                e.CanExecute = true;
                return;
            }
            e.CanExecute = false;
            return;
        }
        //清除结果框内容命令执行
        private void ClearResultCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ResultBox.Text = "";
        }
        //清除结果框按钮使能
        private void ClearResultCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ResultBox == null)
            {
                return;
            }
            if (e.CanExecute == false && ResultBox.Text != "")
            {
                e.CanExecute = true;
                return;
            }
            e.CanExecute = false;
            return;
        }
        //样式选择框
        private void NodeStyleSelection_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RibbonGalleryItem newItem;

            newItem = (RibbonGalleryItem)e.NewValue;
            ChangeStyle(newItem.ToolTipTitle);
            if (intNodeIndex == -1)
            {
                return;
            }
            DrawGarph();
        }
        //切换并读取样式
        void ChangeStyle(string strStyle)
        {
            Style curStyle;
            string strKey;
            Brush bBack = Brushes.AliceBlue, bStroke = Brushes.SteelBlue, bFore = Brushes.Black;
            double dubHeight = 40.0, dubWidth = 40.0, dubStrokeThickness = 3.0;

            switch (strStyle)
            {
                case "默认样式":
                    strKey = "DefaultNodeStyle";
                    break;
                case "深邃星空":
                    strKey = "PurpleNodeStyle";
                    break;
                case "底比斯之水":
                    strKey = "BlueNodeStyle";
                    break;
                case "千本樱":
                    strKey = "PinkNodeStyle";
                    break;
                default:
                    strKey = "DefaultNodeStyle";
                    break;
            }
            curStyle = (Style)TryFindResource(strKey);
            if (curStyle == null)
            {
                return;
            }
            foreach (Setter st in curStyle.Setters)
            {
                switch (st.Property.ToString())
                {
                    case "Fill":
                        bBack = (Brush)st.Value;
                        break;
                    case "Stroke":
                        bStroke = (Brush)st.Value;
                        break;
                    case "StrokeThickness":
                        dubStrokeThickness = (double)st.Value;
                        break;
                    case "Height":
                        dubHeight = (double)st.Value;
                        break;
                    case "Width":
                        dubWidth = (double)st.Value;
                        break;
                    case "Foreground":
                        bFore = (Brush)st.Value;
                        break;
                    default:
                        break;
                }
            }
            drawingBrush = bBack;
            drawingPen = new Pen(bStroke, dubStrokeThickness);
            TextBrush = bFore;
            radius = Convert.ToInt32(dubHeight / 2);
        }

        #endregion

        #region DATA
        string curNodeName = "";
        string curNodeType = "";
        //设置当前选中节点信息
        void SetCurrentNodeInfo(int index)
        {
            intNodeIndex = index;
            if (index < 0)
            {
                curSelectNode = new NodeInfo();
            }
            else
            {
                curSelectNode = new NodeInfo(gdb.GetNodeByIndex(intNodeIndex));
            }
            StatusNameBox.Text = curSelectNode.Name;
            StatusTypeBox.Text = curSelectNode.Type;
            UpdateProperties();
        }
        //更新属性列表
        void UpdateProperties()
        {
            ModifyPropertyComboBox.Items.Clear();
            ModifyPropertyTextBox.Text = "";
            foreach (NodeProperty np in curSelectNode.Properties)
            {
                ModifyPropertyComboBox.Items.Add(np.Key);
            }
        }
        //更新属性值
        private void ModifyPropertyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (intNodeIndex < 0 || ModifyPropertyComboBox.SelectedIndex <0)
            {
                return;
            }
            foreach (NodeProperty np in curSelectNode.Properties)
            {
                if (np.Key == ModifyPropertyComboBox.SelectedItem.ToString())
                {
                    ModifyPropertyTextBox.Text = np.Value;
                }
            }
        }
        //名称文本框值改变
        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            curNodeName = ((TextBox)sender).Text;
            if (curSelectNode == null )
            {
                return;
            }
            FindCustomNode(curNodeName, curNodeType);
        }
        //类型文本框值改变
        private void TypeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            curNodeType = ((TextBox)sender).Text;
            if (curSelectNode == null)
            {
                return;
            }
            FindCustomNode(curNodeName, curNodeType);
        }
        //查找用户指定节点
        private void FindCustomNode(string sName, string sType)
        {
            
            if(gdb == null)
            {
                return;
            }
            int index = gdb.GetIndexByNameAndType(sName, sType);
            if (index < 0)
            {
                return;
            }
            NodeListBox.SelectedIndex = index;
            curModifyNode = gdb.GetNodeByName(sName, sType);
            ModifyEndName.Items.Clear();
            RemoveEndName.Items.Clear();
            foreach (Edge edge in curModifyNode.OutBound)
            {
                if (ModifyEndName.Items.IndexOf(edge.End.Name) > 0)
                {
                    continue;
                }
                ModifyEndName.Items.Add(edge.End.Name);
                RemoveEndName.Items.Add(edge.End.Name);
            }
            ModifyEndName.SelectedIndex = 0;
            FillModifyEndType((string)ModifyEndName.SelectedItem);
            RemoveEndName.SelectedIndex = 0;
            FillRemoveEndType((string)RemoveEndName.SelectedItem);
            FindCustomEdge();
            return;
        }
        //修改节点名称改变
        private void ModifyNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).Text == "")
            {
                return;
            }
            if (e.AddedItems.Count <= 0)
            {
                return;
            }
            FillModifyEndType(e.AddedItems[0].ToString());
        }
        //填充修改类型列表内容
        private void FillModifyEndType(string sName)
        {
            ModifyEndType.Items.Clear();
            if (curModifyNode == null)
            {
                return;
            }
            foreach (Edge edge in curModifyNode.OutBound)
            {
                if (ModifyEndType.Items.IndexOf(edge.End.Name) > 0)
                {
                    continue;
                }
                if (sName != edge.End.Name)
                {
                    continue;
                }
                ModifyEndType.Items.Add(edge.End.Type);
            }
            ModifyEndType.SelectedIndex = 0;
        }
        //修改节点类型改变
        private void ModifyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).Text == "")
            {
                return;
            }
            FindCustomEdge();
        }
        //查找目标连边
        private void FindCustomEdge()
        {
            if (gdb == null)
            {
                return;
            }
            ModifyStartName.Text = StatusNameBox.Text;
            ModifyStartType.Text = StatusTypeBox.Text;
            if (ModifyStartName.Text == ""
                || ModifyStartType.Text == ""
                || ModifyEndName.Text == ""
                || ModifyEndType.Text == "")
            {
                return;
            }
            curModifyEdge = gdb.GetEdgeByNameAndType(ModifyStartName.Text, ModifyStartType.Text, ModifyEndName.Text, ModifyEndType.Text);
            if (curModifyEdge == null)
            {
                return;
            }
            EdgeKeyBox.Text = curModifyEdge.Type;
            EdgeValueBox.Text = curModifyEdge.Value;
        }
        //删除节点名称改变
        private void RemoveNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).Text == "")
            {
                return;
            }
            if (e.AddedItems.Count <= 0)
            {
                return;
            }
            FillRemoveEndType(e.AddedItems[0].ToString());
        }
        //填充删除节点两类型列表
        private void FillRemoveEndType(string sName)
        {
            RemoveEndType.Items.Clear();
            if (curModifyNode == null)
            {
                return;
            }
            foreach (Edge edge in curModifyNode.OutBound)
            {
                if (RemoveEndType.Items.IndexOf(edge.End.Name) > 0)
                {
                    continue;
                }
                if (sName != edge.End.Name)
                {
                    continue;
                }
                RemoveEndType.Items.Add(edge.End.Type);
            }
            RemoveEndType.SelectedIndex = 0;
        }
        //加入节点命令执行函数
        private void AddNodeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string strName, strType, strProperty;
            ErrorCode err = ErrorCode.NoError;
            if (IsContentLegal(out strName,out strType,out strProperty) == false)
            {
                ShowStatus("Name and Type are necessary.");
                return;
            }
            gdb.AddNodeData(strName, strType, ref err, strProperty);
            if (err != ErrorCode.NoError)
            {
                switch (err)
                {
                    case ErrorCode.NodeExists:
                        ShowStatus("Add Node failed, Node already exists.");
                        break;
                    case ErrorCode.CreateNodeFailed:
                        ShowStatus("Create Node failed.");
                        break;
                    default:
                        ShowStatus("Add Node failed, error code:" + err.ToString());
                        break;
                }
                return;
            }
            AddNodeProperties.Items.Clear();
            AddNodeKey.SelectedIndex = 0;
            AddNodeValue.Text = "";
            FillNodeList();
            ShowStatus("Add Node Success.");
            return;
        }
        //校验节点创建入参
        private bool IsContentLegal(out string sName, out string sType, out string sProperty)
        {
            string strName = null, strType = null, strProperty = "";

            foreach (string strItem in AddNodeProperties.Items)
            {
                switch (GetKeyFromItem(strItem))
                {
                    case "Name":
                        strName = GetValueFromItem(strItem);
                        break;
                    case "Type":
                        strType = GetValueFromItem(strItem);
                        break;
                    default:
                        strProperty += strItem + ",";
                        break;
                }
            }
            if (strName == null || strType == null)
            {
                sName = "";
                sType = "";
                sProperty = "";
                return false;
            }
            sName = strName;
            sType = strType;
            sProperty = strProperty;
            return true;
        }
        //加入属性命令执行函数
        private void AddPropertyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string strKey, strValue;

            strKey = AddNodeKey.Text;
            strValue = AddNodeValue.Text;
            if (strKey == "" || strValue == "")
            {
                ShowStatus("Key or Value can't be empty.");
                return;
            }
            //将键值对校验后加入列表
            AddPropertyIntoList(strKey ,strValue);
            SortList();
            AddNodeKey.Text = "";
            AddNodeValue.Text = "";
        }
        //在属性列表中加入新属性
        private void AddPropertyIntoList(string sKey, string sValue)
        {
            int index = 0;
            string strTar = null;

            foreach (string strItem in AddNodeProperties.Items)
            {
                if (GetKeyFromItem(strItem) == sKey)
                {
                    strTar= strItem;
                    break;
                }
                index++;
            }
            if (strTar != null)
            {
                AddNodeProperties.Items.Insert(index, sKey + ":" + sValue);
                AddNodeProperties.Items.Remove(strTar);
            }
            else
            {
                AddNodeProperties.Items.Add(sKey + ":" + sValue);
            }
        }
        //获取当前列表项中的key字段
        private string GetKeyFromItem(string sItem)
        {
            int index = sItem.IndexOf(':');
            string strResult;

            if (index < 0)
            {
                return sItem;
            }
            strResult = sItem.Substring(0, index);
            return strResult;
        }
        //获取当前列表项中的Value字段
        private string GetValueFromItem(string sItem)
        {
            int index = sItem.IndexOf(':');
            string strResult;

            if (index < 0)
            {
                return "";
            }
            strResult = sItem.Substring(index+1);
            return strResult;
        }
        //对当前属性列表进行排序
        private void SortList()
        {
            string strName = null, strType = null;
            int index = -1;

            foreach (string strItem in AddNodeProperties.Items)
            {
                if (GetKeyFromItem(strItem) == "Name")
                {
                    strName = strItem;
                    index++;
                    break;
                }
            }
            if (strName != null)
            {
                AddNodeProperties.Items.Remove(strName);
                AddNodeProperties.Items.Insert(index, strName);
            }
            foreach (string strItem in AddNodeProperties.Items)
            {
                if (GetKeyFromItem(strItem) == "Type")
                {
                    strType = strItem;
                    index++;
                    break;
                }
            }
            if (strType != null)
            {
                AddNodeProperties.Items.Remove(strType);
                AddNodeProperties.Items.Insert(index, strType);
            }
        }
        //属性列表选择项改变响应函数
        private void AddNodeProperties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string strItem;
            if (AddNodeProperties.SelectedIndex < 0)
            {
                return;
            }
            strItem = AddNodeProperties.SelectedItem.ToString();
            AddNodeKey.Text = GetKeyFromItem(strItem);
            AddNodeValue.Text = GetValueFromItem(strItem);
        }
        //移除属性命令执行函数
        private void RemovePropertyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (AddNodeProperties.SelectedIndex < 0)
            {
                return;
            }
            AddNodeProperties.Items.RemoveAt(AddNodeProperties.SelectedIndex);
        }
        //加入连边命令执行函数
        private void AddEdgeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ErrorCode err = ErrorCode.NoError;
            string strStartName, strStartType, strEndName, strEndType, strEdgeKey, strEdgeValue;

            if (AddStartName.Text == "" ||
                AddStartType.Text == "" ||
                AddEndName.Text == "" ||
                AddEndType.Text == "" ||
                AddEdgeKey.Text == "" ||
                AddEdgeValue.Text == "" )
            {
                ShowStatus("All fields of edge can't be empty.");
                return;
            }
            strStartName = AddStartName.Text;
            strStartType = AddStartType.Text;
            strEndName = AddEndName.Text;
            strEndType = AddEndType.Text;
            strEdgeKey = AddEdgeKey.Text;
            strEdgeValue = AddEdgeValue.Text;
            gdb.AddEdgeData(strStartName, strStartType, strEndName, strEndType, strEdgeKey, ref err, strEdgeValue);
            if (err != ErrorCode.NoError)
            {
                switch (err)
                {
                    case ErrorCode.NodeNotExists:
                        ShowStatus("Add Edge failed, End Node not exists.");
                        break;
                    case ErrorCode.EdgeExists:
                        ShowStatus("Add Edge failed, Edge already exists.");
                        break;
                    case ErrorCode.CreateEdgeFailed:
                        ShowStatus("Create Edge failed.");
                        break;
                    default:
                        ShowStatus("Add Edge failed, error code:" + err.ToString());
                        break;
                }
                return;
            }
            GraphEdgeUpdate();
            AddEndName.Text = "";
            AddEndType.Text = "";
            AddEdgeKey.Text = "";
            AddEdgeValue.Text = "";
            ShowStatus("Add Edge Success.");
            return;
        }
        //修改节点命令执行函数
        private void ModifyNodeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //获取值
            string strKey, strValue;
            NodeProperty npDel = null;

            strKey = ModifyPropertyComboBox.Text;
            strValue = ModifyPropertyTextBox.Text;
            if (strKey == "")
            {
                ShowStatus("Modify Property Failed, Key field can't be empty.");
                return;
            }
            if (curModifyNode == null)
            {
                ShowStatus("Modify Property Failed, no node be selected.");
                return;
            }
            //如果存在该key则修改
            foreach (NodeProperty np in curModifyNode.Properties)
            {
                if (strKey != np.Key)
                {
                    continue;
                }
                if (strValue == "")
                {
                    npDel = np;
                    break;
                }
                np.Value = strValue;
                ShowStatus("Modify Property Success.");
                return;
            }
            //如果value为空则删除该属性
            if (npDel != null)
            {
                curModifyNode.Properties.Remove(npDel);
                ShowStatus("Delete Property Success.");
                return;
            }
            if (strValue == "")
            {
                ShowStatus("Add Property Failed, Value field can't be empty.");
                return;
            }
            //如果不存在则插入新属性
            curModifyNode.Properties.Add(new NodeProperty(strKey, strValue));
            ShowStatus("Add Property Success.");
            return;
        }
        //修改连边命令执行函数
        private void ModifyEdgeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string strKey, strValue;

            strKey = EdgeKeyBox.Text;
            strValue = EdgeValueBox.Text;
            if (strKey == "" || strValue == "")
            {
                ShowStatus("Modify Edge Failed, Key or Value field can't be empty.");
                return;
            }
            if (curModifyEdge == null)
            {
                ShowStatus("Modify Edge Failed, no edge be selected.");
                return;
            }
            curModifyEdge.Type = strKey;
            curModifyEdge.Value = strValue;
            ShowStatus("Modify Edge Success.");
            return;
        }
        //移除节点命令执行函数
        private void RemoveNodeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ErrorCode err = ErrorCode.NoError;
            string strName, strType;

            strName = RemoveNodeName.Text;
            strType = RemoveNodeType.Text;
            if (strName == "" || strType == "")
            {
                ShowStatus("Name or Type of Node can't be empty.");
                return;
            }
            gdb.RemoveNodeData(strName, strType, ref err);
            if (err != ErrorCode.NoError)
            {
                ShowStatus("Remove Node failed, Node not exists.");
                return;
            }
            ShowStatus("Remove Node Success.");
            GraphNodeUpdate();
            return;
        }
        //移除连边命令执行函数
        private void RemoveEdgeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ErrorCode err = ErrorCode.NoError;
            string strStartName, strStartType, strEndName, strEndType;

            strStartName = RemoveStartName.Text;
            strStartType = RemoveStartType.Text;
            strEndName = RemoveEndName.Text;
            strEndType = RemoveEndType.Text;
            if (strStartName == "" ||
                strStartType == "" ||
                strEndName == "" ||
                strEndType == "")
            {
                ShowStatus("Name or Type of Nodes can't be empty.");
                return;
            }
            gdb.RemoveEdgeData(strStartName, strStartType, strEndName, strEndType, "", ref err);
            if (err != ErrorCode.NoError)
            {
                switch (err)
                {
                    case ErrorCode.NodeNotExists:
                        ShowStatus("Remove Edge failed, Start Node or End Node not exists.");
                        break;
                    case ErrorCode.EdgeNotExists:
                        ShowStatus("Remove Edge failed, Edge not exists.");
                        break;
                    default:
                        ShowStatus("Remove Edge failed, error code:" + err.ToString());
                        break;
                }
                return;
            }
            ShowStatus("Remove Edge Success.");
            GraphEdgeUpdate();
            return;
        }

        private void AddNodeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        private void AddPropertyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        private void RemovePropertyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (AddNodeProperties.SelectedIndex < 0)
            {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = true;
            return;
        }

        private void AddEdgeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        private void ModifyNodeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        private void ModifyEdgeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        private void RemoveNodeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        private void RemoveEdgeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = isDbAvailable;
        }

        #endregion

    }
}
