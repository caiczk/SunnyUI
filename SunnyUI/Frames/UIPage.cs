﻿/******************************************************************************
 * SunnyUI 开源控件库、工具类库、扩展类库、多页面开发框架。
 * CopyRight (C) 2012-2022 ShenYongHua(沈永华).
 * QQ群：56829229 QQ：17612584 EMail：SunnyUI@QQ.Com
 *
 * Blog:   https://www.cnblogs.com/yhuse
 * Gitee:  https://gitee.com/yhuse/SunnyUI
 * GitHub: https://github.com/yhuse/SunnyUI
 *
 * SunnyUI.dll can be used for free under the GPL-3.0 license.
 * If you use this code, please keep this note.
 * 如果您使用此代码，请保留此说明。
 ******************************************************************************
 * 文件名称: UIPage.cs
 * 文件说明: 页面基类，从Form继承，可放置于容器内
 * 当前版本: V3.0
 * 创建日期: 2020-01-01
 *
 * 2020-01-01: V2.2.0 增加文件说明
 * 2021-05-21: V3.0.4 更改了下页面切换重复执行的Init事件调用
 * 2021-06-20: V3.0.4 增加标题行，替代UITitlePage
 * 2021-07-18: V3.0.5 修复OnLoad在加载时重复加载两次的问题，增加Final函数，每次页面切换，退出页面都会执行
 * 2021-08-17: V3.0.6 增加TitleFont属性
 * 2021-08-24: V3.0.6 修复OnLoad在加载时重复加载两次的问题
 * 2021-12-01: V3.0.9 增加FeedBack和SetParam函数，用于多页面传值
 * 2021-12-30: V3.0.9 增加NeedReload，页面切换是否需要重载Load
******************************************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Sunny.UI
{
    [DefaultEvent("Initialize")]
    public partial class UIPage : Form, IStyleInterface, ISymbol
    {
        public readonly Guid Guid = Guid.NewGuid();
        private Color _rectColor = UIColor.Blue;

        private ToolStripStatusLabelBorderSides _rectSides = ToolStripStatusLabelBorderSides.None;

        protected UIStyle _style = UIStyle.Blue;

        [Browsable(false)]
        public IFrame Frame
        {
            get; set;
        }

        public UIPage()
        {
            InitializeComponent();

            base.BackColor = UIColor.LightBlue;
            TopLevel = false;
            if (this.Register()) SetStyle(UIStyles.Style);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            UpdateStyles();

            if (!IsDesignMode) base.Dock = DockStyle.Fill;

            Version = UIGlobal.Version;
            SetDPIScale();
        }

        [Browsable(false)]
        public bool IsScaled { get; private set; }

        public void SetDPIScale()
        {
            if (!IsScaled)
            {
                this.SetDPIScaleFont();
                if (!this.DPIScale().Equals(1))
                {
                    this.TitleFont = this.DPIScaleFont(this.TitleFont);
                }

                foreach (Control control in this.GetAllDPIScaleControls())
                {
                    control.SetDPIScaleFont();
                }

                IsScaled = true;
            }
        }

        public void Render()
        {
            SetStyle(UIStyles.Style);
        }

        private int _symbolSize = 24;

        [DefaultValue(24)]
        [Description("字体图标大小"), Category("SunnyUI")]
        public int SymbolSize
        {
            get => _symbolSize;
            set
            {
                _symbolSize = Math.Max(value, 16);
                _symbolSize = Math.Min(value, 128);
                SymbolChange();
                Invalidate();
            }
        }

        private int _symbol;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Editor(typeof(UIImagePropertyEditor), typeof(UITypeEditor))]
        [DefaultValue(0)]
        [Description("字体图标"), Category("SunnyUI")]
        public int Symbol
        {
            get => _symbol;
            set
            {
                _symbol = value;
                SymbolChange();
                Invalidate();
            }
        }

        private Point symbolOffset = new Point(0, 0);

        [DefaultValue(typeof(Point), "0, 0")]
        [Description("字体图标的偏移位置"), Category("SunnyUI")]
        public Point SymbolOffset
        {
            get => symbolOffset;
            set
            {
                symbolOffset = value;
                Invalidate();
            }
        }

        [DefaultValue(false), Description("在Frame框架中不被关闭"), Category("SunnyUI")]
        public bool AlwaysOpen
        {
            get; set;
        }

        protected virtual void SymbolChange()
        {
        }

        [Browsable(false)]
        public Point ParentLocation { get; set; } = new Point(0, 0);

        [DefaultValue(-1)]
        public int PageIndex { get; set; } = -1;

        [Browsable(false)]
        public Guid PageGuid { get; set; } = Guid.Empty;

        [Browsable(false), DefaultValue(null)]
        public TabPage TabPage { get; set; } = null;

        /// <summary>
        ///     边框颜色
        /// </summary>
        /// <value>The color of the border style.</value>
        [Description("边框颜色"), Category("SunnyUI")]
        public Color RectColor
        {
            get => _rectColor;
            set
            {
                _rectColor = value;
                AfterSetRectColor(value);
                _style = UIStyle.Custom;
                Invalidate();
            }
        }

        protected bool IsDesignMode
        {
            get
            {
                var ReturnFlag = false;
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                    ReturnFlag = true;
                else if (Process.GetCurrentProcess().ProcessName == "devenv")
                    ReturnFlag = true;

                return ReturnFlag;
            }
        }

        [DefaultValue(ToolStripStatusLabelBorderSides.None)]
        [Description("边框显示位置"), Category("SunnyUI")]
        public ToolStripStatusLabelBorderSides RectSides
        {
            get => _rectSides;
            set
            {
                _rectSides = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Tag字符串
        /// </summary>
        [DefaultValue(null)]
        [Description("获取或设置包含有关控件的数据的对象字符串"), Category("SunnyUI")]
        public string TagString
        {
            get; set;
        }

        public string Version
        {
            get;
        }

        /// <summary>
        /// 主题样式
        /// </summary>
        [DefaultValue(UIStyle.Blue), Description("主题样式"), Category("SunnyUI")]
        public UIStyle Style
        {
            get => _style;
            set => SetStyle(value);
        }

        /// <summary>
        /// 自定义主题风格
        /// </summary>
        [DefaultValue(false)]
        [Description("获取或设置可以自定义主题风格"), Category("SunnyUI")]
        public bool StyleCustomMode
        {
            get; set;
        }

        public event EventHandler Initialize;

        public event EventHandler Finalize;

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control is IStyleInterface ctrl)
            {
                if (!ctrl.StyleCustomMode) ctrl.Style = Style;
            }

            UIStyleHelper.SetRawControlStyle(e, Style);

            if (AllowShowTitle && !AllowAddControlOnTitle && e.Control.Top < TitleHeight)
            {
                e.Control.Top = Padding.Top;
            }
        }

        [DefaultValue(false)]
        [Description("允许在标题栏放置控件"), Category("SunnyUI")]
        public bool AllowAddControlOnTitle
        {
            get; set;
        }

        public virtual void Init()
        {
            Initialize?.Invoke(this, new EventArgs());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Init();
        }

        private bool IsShown;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            IsShown = true;
        }

        public void ReLoad()
        {
            if (IsShown)
            {
                if (NeedReload)
                    OnLoad(EventArgs.Empty);
                else
                    Init();
            }
        }


        /// <summary>
        /// 字体颜色
        /// </summary>
        [Description("页面切换是否需要重载Load"), Category("SunnyUI")]
        [DefaultValue(false)]
        public bool NeedReload { get; set; }

        // private void EventLoad()
        // {
        //     Type type = this.GetType().BaseType;
        //     while (type.Name != "Form")
        //     {
        //         type = type.BaseType;
        //     }
        //
        //     FieldInfo targetMethod = type.GetField("EVENT_LOAD", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        //     object obj = (object)targetMethod.GetValue(this);
        //
        //     EventHandler handler = (EventHandler)this.Events[obj];
        //     handler?.Invoke(this, EventArgs.Empty);
        // }

        public virtual void Final()
        {
            Finalize?.Invoke(this, new EventArgs());
        }

        public void SetStyle(UIStyle style)
        {
            this.SuspendLayout();
            UIStyleHelper.SetChildUIStyle(this, style);

            UIBaseStyle uiColor = UIStyles.GetStyleColor(style);
            if (!uiColor.IsCustom()) SetStyleColor(uiColor);
            _style = style;
            UIStyleChanged?.Invoke(this, new EventArgs());
            this.ResumeLayout();
        }

        public event EventHandler UIStyleChanged;

        public virtual void SetStyleColor(UIBaseStyle uiColor)
        {
            BackColor = uiColor.PlainColor;
            RectColor = uiColor.RectColor;
            ForeColor = UIFontColor.Primary;
            Invalidate();
        }

        protected virtual void AfterSetFillColor(Color color)
        {
        }

        protected virtual void AfterSetRectColor(Color color)
        {
        }

        protected virtual void AfterSetForeColor(Color color)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Width <= 0 || Height <= 0) return;

            if (AllowShowTitle)
            {
                e.Graphics.FillRectangle(TitleFillColor, 0, 0, Width, TitleHeight);
            }

            if (RectSides != ToolStripStatusLabelBorderSides.None)
            {
                if (RectSides.GetValue(ToolStripStatusLabelBorderSides.Left))
                    e.Graphics.DrawLine(RectColor, 0, 0, 0, Height - 1);
                if (RectSides.GetValue(ToolStripStatusLabelBorderSides.Top))
                    e.Graphics.DrawLine(RectColor, 0, 0, Width - 1, 0);
                if (RectSides.GetValue(ToolStripStatusLabelBorderSides.Right))
                    e.Graphics.DrawLine(RectColor, Width - 1, 0, Width - 1, Height - 1);
                if (RectSides.GetValue(ToolStripStatusLabelBorderSides.Bottom))
                    e.Graphics.DrawLine(RectColor, 0, Height - 1, Width - 1, Height - 1);
            }

            if (!AllowShowTitle) return;
            if (Symbol > 0)
            {
                e.Graphics.DrawFontImage(Symbol, SymbolSize, TitleForeColor, new Rectangle(ImageInterval, 0, SymbolSize, TitleHeight), SymbolOffset.X, SymbolOffset.Y);
            }

            SizeF sf = e.Graphics.MeasureString(Text, TitleFont);
            e.Graphics.DrawString(Text, TitleFont, TitleForeColor,
                Symbol > 0 ? ImageInterval * 2 + SymbolSize : ImageInterval, (TitleHeight - sf.Height) / 2);

            e.Graphics.SetHighQuality();
            if (ControlBox)
            {
                if (InControlBox)
                {
                    e.Graphics.FillRectangle(UIColor.Red, ControlBoxRect);
                }

                e.Graphics.DrawLine(Color.White,
                    ControlBoxRect.Left + ControlBoxRect.Width / 2 - 5,
                    ControlBoxRect.Top + ControlBoxRect.Height / 2 - 5,
                    ControlBoxRect.Left + ControlBoxRect.Width / 2 + 5,
                    ControlBoxRect.Top + ControlBoxRect.Height / 2 + 5);
                e.Graphics.DrawLine(Color.White,
                    ControlBoxRect.Left + ControlBoxRect.Width / 2 - 5,
                    ControlBoxRect.Top + ControlBoxRect.Height / 2 + 5,
                    ControlBoxRect.Left + ControlBoxRect.Width / 2 + 5,
                    ControlBoxRect.Top + ControlBoxRect.Height / 2 - 5);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (FormBorderStyle == FormBorderStyle.None && ShowTitle)
            {
                if (InControlBox)
                {
                    InControlBox = false;
                    Close();
                    AfterClose();
                }
            }
        }

        private void AfterClose()
        {
            Console.WriteLine("Close");
        }

        private Color titleFillColor = Color.FromArgb(76, 76, 76);

        /// <summary>
        /// 填充颜色，当值为背景色或透明色或空值则不填充
        /// </summary>
        [Description("标题颜色"), Category("SunnyUI")]
        [DefaultValue(typeof(Color), "76, 76, 76")]
        public Color TitleFillColor
        {
            get => titleFillColor;
            set
            {
                titleFillColor = value;
                Invalidate();
            }
        }

        private Color titleForeColor = Color.White;

        /// <summary>
        /// 字体颜色
        /// </summary>
        [Description("字体颜色"), Category("SunnyUI")]
        [DefaultValue(typeof(Color), "White")]
        public Color TitleForeColor
        {
            get => titleForeColor;
            set
            {
                titleForeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 标题字体
        /// </summary>
        private Font titleFont = UIFontColor.Font();

        /// <summary>
        /// 标题字体
        /// </summary>
        [Description("标题字体"), Category("SunnyUI")]
        [DefaultValue(typeof(Font), "微软雅黑, 12pt")]
        public Font TitleFont
        {
            get => titleFont;
            set
            {
                titleFont = value;
                Invalidate();
            }
        }

        private int imageInterval = 6;

        public int ImageInterval
        {
            get => imageInterval;
            set
            {
                imageInterval = Math.Max(2, value);
                Invalidate();
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            AfterSetFillColor(BackColor);
            _style = UIStyle.Custom;
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            AfterSetForeColor(ForeColor);
            _style = UIStyle.Custom;
        }

        private int titleHeight = 35;

        [Description("面板高度"), Category("SunnyUI")]
        [DefaultValue(35)]
        public int TitleHeight
        {
            get => titleHeight;
            set
            {
                titleHeight = Math.Max(value, 31);
                Padding = new Padding(Padding.Left, titleHeight, Padding.Right, Padding.Bottom);
                CalcSystemBoxPos();
                Invalidate();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            CalcSystemBoxPos();
        }

        private bool InControlBox;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (ShowTitle && ControlBox)
            {
                bool inControlBox = e.Location.InRect(ControlBoxRect);

                if (inControlBox != InControlBox)
                {
                    InControlBox = inControlBox;
                    Invalidate();
                }
            }
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);

            if (AllowShowTitle)
            {
                Padding = new Padding(Padding.Left, titleHeight, Padding.Right, Padding.Bottom);
            }
        }

        [Description("允许显示标题栏"), Category("SunnyUI"), DefaultValue(false)]
        public bool AllowShowTitle
        {
            get => ShowTitle;
            set => ShowTitle = value;
        }

        /// <summary>
        /// 是否显示窗体的标题栏
        /// </summary>
        private bool showTitle;

        /// <summary>
        /// 是否显示窗体的标题栏
        /// </summary>
        [Description("是否显示窗体的标题栏"), Category("WindowStyle"), DefaultValue(false)]
        public bool ShowTitle
        {
            get => showTitle;
            set
            {
                showTitle = value;
                Padding = new Padding(Padding.Left, value ? titleHeight : 0, Padding.Right, Padding.Bottom);
                Invalidate();
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        private void CalcSystemBoxPos()
        {
            ControlBoxRect = new Rectangle(Width - 6 - 28, titleHeight / 2 - 14, 28, 28);
        }

        private Rectangle ControlBoxRect;

        /// <summary>
        /// 是否显示窗体的控制按钮
        /// </summary>
        private bool controlBox;

        /// <summary>
        /// 是否显示窗体的控制按钮
        /// </summary>
        [Description("是否显示窗体的控制按钮"), Category("WindowStyle"), DefaultValue(false)]
        public new bool ControlBox
        {
            get => controlBox;
            set
            {
                controlBox = value;
                CalcSystemBoxPos();
                Invalidate();
            }
        }

        [Browsable(false)]
        public new bool MinimizeBox
        {
            get; set;
        }

        [Browsable(false)]
        public new bool MaximizeBox
        {
            get; set;
        }

        public void Feedback(object sender, params object[] objects)
        {
            Frame?.Feedback(this, PageIndex, objects);
        }

        public virtual void SetParam(int fromPageIndex, params object[] objects)
        {

        }

        #region 一些辅助窗口

        /// <summary>
        /// 显示进度提示窗
        /// </summary>
        /// <param name="desc">描述文字</param>
        /// <param name="maximum">最大进度值</param>
        /// <param name="decimalCount">显示进度条小数个数</param>
        public void ShowStatusForm(int maximum = 100, string desc = "系统正在处理中，请稍候...", int decimalCount = 1)
        {
            UIStatusFormService.ShowStatusForm(maximum, desc, decimalCount);
        }

        /// <summary>
        /// 隐藏进度提示窗
        /// </summary>
        public void HideStatusForm()
        {
            UIStatusFormService.HideStatusForm();
        }

        /// <summary>
        /// 设置进度提示窗步进值加1
        /// </summary>
        public void StatusFormStepIt()
        {
            UIStatusFormService.StepIt();
        }

        /// <summary>
        /// 设置进度提示窗描述文字
        /// </summary>
        /// <param name="desc">描述文字</param>
        public void SetStatusFormDescription(string desc)
        {
            UIStatusFormService.SetDescription(desc);
        }

        /// <summary>
        /// 显示等待提示窗
        /// </summary>
        /// <param name="desc">描述文字</param>
        public void ShowWaitForm(string desc = "系统正在处理中，请稍候...")
        {
            UIWaitFormService.ShowWaitForm(desc);
        }

        /// <summary>
        /// 隐藏等待提示窗
        /// </summary>
        public void HideWaitForm()
        {
            UIWaitFormService.HideWaitForm();
        }

        /// <summary>
        /// 设置等待提示窗描述文字
        /// </summary>
        /// <param name="desc">描述文字</param>
        public void SetWaitFormDescription(string desc)
        {
            UIWaitFormService.SetDescription(desc);
        }

        /// <summary>
        /// 正确信息提示框
        /// </summary>
        /// <param name="msg">信息</param>
        /// <param name="showMask">显示遮罩层</param>
        public void ShowSuccessDialog(string msg, bool showMask = true)
        {
            UIMessageDialog.ShowMessageDialog(msg, UILocalize.SuccessTitle, false, UIStyle.Green, showMask, Frame?.TopMost ?? false);
        }

        /// <summary>
        /// 信息提示框
        /// </summary>
        /// <param name="msg">信息</param>
        /// <param name="showMask">显示遮罩层</param>
        public void ShowInfoDialog(string msg, bool showMask = true)
        {
            UIMessageDialog.ShowMessageDialog(msg, UILocalize.InfoTitle, false, UIStyle.Gray, showMask, Frame?.TopMost ?? false);
        }

        /// <summary>
        /// 警告信息提示框
        /// </summary>
        /// <param name="msg">信息</param>
        /// <param name="showMask">显示遮罩层</param>
        public void ShowWarningDialog(string msg, bool showMask = true)
        {
            UIMessageDialog.ShowMessageDialog(msg, UILocalize.WarningTitle, false, UIStyle.Orange, showMask, Frame?.TopMost ?? false);
        }

        /// <summary>
        /// 错误信息提示框
        /// </summary>
        /// <param name="msg">信息</param>
        /// <param name="showMask">显示遮罩层</param>
        public void ShowErrorDialog(string msg, bool showMask = true)
        {
            UIMessageDialog.ShowMessageDialog(msg, UILocalize.ErrorTitle, false, UIStyle.Red, showMask, Frame?.TopMost ?? false);
        }

        /// <summary>
        /// 确认信息提示框
        /// </summary>
        /// <param name="msg">信息</param>
        /// <param name="showMask">显示遮罩层</param>
        /// <returns>结果</returns>
        public bool ShowAskDialog(string msg, bool showMask = true)
        {
            return UIMessageDialog.ShowMessageDialog(msg, UILocalize.AskTitle, true, UIStyle.Blue, showMask, Frame?.TopMost ?? false);
        }

        /// <summary>
        /// 正确信息提示框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="msg">信息</param>
        /// <param name="style">主题</param>
        /// <param name="showMask">显示遮罩层</param>
        public void ShowSuccessDialog(string title, string msg, UIStyle style = UIStyle.Green, bool showMask = true)
        {
            UIMessageDialog.ShowMessageDialog(msg, title, false, style, showMask, Frame?.TopMost ?? false);
        }

        /// <summary>
        /// 信息提示框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="msg">信息</param>
        /// <param name="style">主题</param>
        /// <param name="showMask">显示遮罩层</param>
        public void ShowInfoDialog(string title, string msg, UIStyle style = UIStyle.Gray, bool showMask = true)
        {
            UIMessageDialog.ShowMessageDialog(msg, title, false, style, showMask, Frame?.TopMost ?? false);
        }

        /// <summary>
        /// 警告信息提示框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="msg">信息</param>
        /// <param name="style">主题</param>
        /// <param name="showMask">显示遮罩层</param>
        public void ShowWarningDialog(string title, string msg, UIStyle style = UIStyle.Orange, bool showMask = true)
        {
            UIMessageDialog.ShowMessageDialog(msg, title, false, style, showMask, Frame?.TopMost ?? false);
        }

        /// <summary>
        /// 错误信息提示框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="msg">信息</param>
        /// <param name="style">主题</param>
        /// <param name="showMask">显示遮罩层</param>
        public void ShowErrorDialog(string title, string msg, UIStyle style = UIStyle.Red, bool showMask = true)
        {
            UIMessageDialog.ShowMessageDialog(msg, title, false, style, showMask, Frame?.TopMost ?? false);
        }

        /// <summary>
        /// 确认信息提示框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="msg">信息</param>
        /// <param name="style">主题</param>
        /// <param name="showMask">显示遮罩层</param>
        /// <returns>结果</returns>
        public bool ShowAskDialog(string title, string msg, UIStyle style = UIStyle.Blue, bool showMask = true)
        {
            return UIMessageDialog.ShowMessageDialog(msg, title, true, style, showMask, Frame?.TopMost ?? false);
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="text">消息文本</param>
        /// <param name="delay">消息停留时长(ms)。默认1秒</param>
        /// <param name="floating">是否漂浮</param>
        public void ShowInfoTip(string text, int delay = 1000, bool floating = true)
            => UIMessageTip.Show(text, null, delay, floating);

        /// <summary>
        /// 显示成功消息
        /// </summary>
        /// <param name="text">消息文本</param>
        /// <param name="delay">消息停留时长(ms)。默认1秒</param>
        /// <param name="floating">是否漂浮</param>
        public void ShowSuccessTip(string text, int delay = 1000, bool floating = true)
            => UIMessageTip.ShowOk(text, delay, floating);

        /// <summary>
        /// 显示警告消息
        /// </summary>
        /// <param name="text">消息文本</param>
        /// <param name="delay">消息停留时长(ms)。默认1秒</param>
        /// <param name="floating">是否漂浮</param>
        public void ShowWarningTip(string text, int delay = 1000, bool floating = true)
            => UIMessageTip.ShowWarning(text, delay, floating);

        /// <summary>
        /// 显示出错消息
        /// </summary>
        /// <param name="text">消息文本</param>
        /// <param name="delay">消息停留时长(ms)。默认1秒</param>
        /// <param name="floating">是否漂浮</param>
        public void ShowErrorTip(string text, int delay = 1000, bool floating = true)
            => UIMessageTip.ShowError(text, delay, floating);

        /// <summary>
        /// 在指定控件附近显示消息
        /// </summary>
        /// <param name="controlOrItem">控件或工具栏项</param>
        /// <param name="text">消息文本</param>
        /// <param name="delay">消息停留时长(ms)。默认1秒</param>
        /// <param name="floating">是否漂浮</param>
        public void ShowInfoTip(Component controlOrItem, string text, int delay = 1000, bool floating = true)
            => UIMessageTip.Show(controlOrItem, text, null, delay, floating);

        /// <summary>
        /// 在指定控件附近显示良好消息
        /// </summary>
        /// <param name="controlOrItem">控件或工具栏项</param>
        /// <param name="text">消息文本</param>
        /// <param name="delay">消息停留时长(ms)。默认1秒</param>
        /// <param name="floating">是否漂浮</param>
        public void ShowSuccessTip(Component controlOrItem, string text, int delay = 1000, bool floating = true)
            => UIMessageTip.ShowOk(controlOrItem, text, delay, floating);

        /// <summary>
        /// 在指定控件附近显示出错消息
        /// </summary>
        /// <param name="controlOrItem">控件或工具栏项</param>
        /// <param name="text">消息文本</param>
        /// <param name="delay">消息停留时长(ms)。默认1秒</param>
        /// <param name="floating">是否漂浮</param>
        public void ShowErrorTip(Component controlOrItem, string text, int delay = 1000, bool floating = true)
            => UIMessageTip.ShowError(controlOrItem, text, delay, floating);

        /// <summary>
        /// 在指定控件附近显示警告消息
        /// </summary>
        /// <param name="controlOrItem">控件或工具栏项</param>
        /// <param name="text">消息文本</param>
        /// <param name="delay">消息停留时长(ms)。默认1秒</param>
        /// <param name="floating">是否漂浮</param>
        public void ShowWarningTip(Component controlOrItem, string text, int delay = 1000, bool floating = true)
            => UIMessageTip.ShowWarning(controlOrItem, text, delay, floating, false);

        public void ShowInfoNotifier(string desc, bool isDialog = false, int timeout = 2000)
        {
            UINotifierHelper.ShowNotifier(desc, UINotifierType.INFO, UILocalize.InfoTitle, isDialog, timeout);
        }

        public void ShowSuccessNotifier(string desc, bool isDialog = false, int timeout = 2000)
        {
            UINotifierHelper.ShowNotifier(desc, UINotifierType.OK, UILocalize.SuccessTitle, isDialog, timeout);
        }

        public void ShowWarningNotifier(string desc, bool isDialog = false, int timeout = 2000)
        {
            UINotifierHelper.ShowNotifier(desc, UINotifierType.WARNING, UILocalize.WarningTitle, isDialog, timeout);
        }

        public void ShowErrorNotifier(string desc, bool isDialog = false, int timeout = 2000)
        {
            UINotifierHelper.ShowNotifier(desc, UINotifierType.ERROR, UILocalize.ErrorTitle, isDialog, timeout);
        }

        public void ShowInfoNotifier(string desc, EventHandler clickEvent, int timeout = 2000)
        {
            UINotifierHelper.ShowNotifier(desc, clickEvent, UINotifierType.INFO, UILocalize.InfoTitle, timeout);
        }

        public void ShowSuccessNotifier(string desc, EventHandler clickEvent, int timeout = 2000)
        {
            UINotifierHelper.ShowNotifier(desc, clickEvent, UINotifierType.OK, UILocalize.SuccessTitle, timeout);
        }

        public void ShowWarningNotifier(string desc, EventHandler clickEvent, int timeout = 2000)
        {
            UINotifierHelper.ShowNotifier(desc, clickEvent, UINotifierType.WARNING, UILocalize.WarningTitle, timeout);
        }

        public void ShowErrorNotifier(string desc, EventHandler clickEvent, int timeout = 2000)
        {
            UINotifierHelper.ShowNotifier(desc, clickEvent, UINotifierType.ERROR, UILocalize.ErrorTitle, timeout);
        }

        #endregion 一些辅助窗口
    }
}