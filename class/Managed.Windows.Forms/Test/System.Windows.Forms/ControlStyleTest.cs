//
// ControlStyleTest.cs (Auto-generated by GenerateControlStyleTest.cs).
//
// Author: 
//   Peter Dennis Bartok (pbartok@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms {
	[TestFixture]
	public class TestControlStyle {

		static Array style_values = Enum.GetValues(typeof(ControlStyles));
		static string[] style_names = Enum.GetNames(typeof(ControlStyles));

		public static string[] GetStyles(Control control) {
			string[] result;

			result = new string[style_names.Length];

			for (int i = 0; i < style_values.Length; i++) {
				result[i] = style_names[i] + "=" + control.GetType().GetMethod("GetStyle", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(control, new object[1] {(ControlStyles)style_values.GetValue(i)});
			}

			return result;
		}

		[Test]
		public void ControlStyleTest ()
		{
			string[] Control_want = {
				"ContainerControl=False",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new Control()), Control_want, "ControlStyles");
		}


		[Test]
		public void ButtonStyleTest ()
		{
			string[] Button_want = {
				"ContainerControl=False",
				"UserPaint=True",
				"Opaque=True",
				"ResizeRedraw=True",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=True",
				"SupportsTransparentBackColor=True",
				"StandardDoubleClick=False",
				"AllPaintingInWmPaint=True",
				"CacheText=True",
				"EnableNotifyMessage=False",
				"DoubleBuffer=True"
			};

			Assert.AreEqual(GetStyles(new Button()), Button_want, "ButtonStyles");
		}


		[Test]
		public void CheckBoxStyleTest ()
		{
			string[] CheckBox_want = {
				"ContainerControl=False",
				"UserPaint=True",
				"Opaque=True",
				"ResizeRedraw=True",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=True",
				"SupportsTransparentBackColor=True",
				"StandardDoubleClick=False",
				"AllPaintingInWmPaint=True",
				"CacheText=True",
				"EnableNotifyMessage=False",
				"DoubleBuffer=True"
			};

			Assert.AreEqual(GetStyles(new CheckBox()), CheckBox_want, "CheckBoxStyles");
		}


		[Test]
		public void RadioButtonStyleTest ()
		{
			string[] RadioButton_want = {
				"ContainerControl=False",
				"UserPaint=True",
				"Opaque=True",
				"ResizeRedraw=True",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=True",
				"SupportsTransparentBackColor=True",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=True",
				"EnableNotifyMessage=False",
				"DoubleBuffer=True"
			};

			Assert.AreEqual(GetStyles(new RadioButton()), RadioButton_want, "RadioButtonStyles");
		}


		[Test]
		public void DataGridStyleTest ()
		{
			string[] DataGrid_want = {
				"ContainerControl=False",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=True",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new DataGrid()), DataGrid_want, "DataGridStyles");
		}


		[Test]
		public void DateTimePickerStyleTest ()
		{
			string[] DateTimePicker_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=True",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new DateTimePicker()), DateTimePicker_want, "DateTimePickerStyles");
		}


		[Test]
		public void GroupBoxStyleTest ()
		{
			string[] GroupBox_want = {
				"ContainerControl=True",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=True",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=False",
				"UserMouse=False",
				"SupportsTransparentBackColor=True",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new GroupBox()), GroupBox_want, "GroupBoxStyles");
		}


		[Test]
		public void LabelStyleTest ()
		{
			string[] Label_want = {
				"ContainerControl=False",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=True",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=False",
				"UserMouse=False",
				"SupportsTransparentBackColor=True",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=True"
			};

			Assert.AreEqual(GetStyles(new Label()), Label_want, "LabelStyles");
		}


		[Test]
		public void LinkLabelStyleTest ()
		{
			string[] LinkLabel_want = {
				"ContainerControl=False",
				"UserPaint=True",
				"Opaque=True",
				"ResizeRedraw=True",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=False",
				"UserMouse=False",
				"SupportsTransparentBackColor=True",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=True"
			};

			Assert.AreEqual(GetStyles(new LinkLabel()), LinkLabel_want, "LinkLabelStyles");
		}


		[Test]
		public void ComboBoxStyleTest ()
		{
			string[] ComboBox_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new ComboBox()), ComboBox_want, "ComboBoxStyles");
		}


		[Test]
		public void ListBoxStyleTest ()
		{
			string[] ListBox_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new ListBox()), ListBox_want, "ListBoxStyles");
		}


		[Test]
		public void CheckedListBoxStyleTest ()
		{
			string[] CheckedListBox_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=True",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new CheckedListBox()), CheckedListBox_want, "CheckedListBoxStyles");
		}


		[Test]
		public void ListViewStyleTest ()
		{
			string[] ListView_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new ListView()), ListView_want, "ListViewStyles");
		}


		[Test]
		public void MdiClientStyleTest ()
		{
			string[] MdiClient_want = {
				"ContainerControl=False",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=False",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new MdiClient()), MdiClient_want, "MdiClientStyles");
		}


		[Test]
		public void MonthCalendarStyleTest ()
		{
			string[] MonthCalendar_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new MonthCalendar()), MonthCalendar_want, "MonthCalendarStyles");
		}


		[Test]
		public void PictureBoxStyleTest ()
		{
			string[] PictureBox_want = {
				"ContainerControl=False",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=False",
				"UserMouse=False",
				"SupportsTransparentBackColor=True",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=True"
			};

			Assert.AreEqual(GetStyles(new PictureBox()), PictureBox_want, "PictureBoxStyles");
		}


		[Test]
		public void ProgressBarStyleTest ()
		{
			string[] ProgressBar_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=False",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new ProgressBar()), ProgressBar_want, "ProgressBarStyles");
		}


		[Test]
		public void ScrollableControlStyleTest ()
		{
			string[] ScrollableControl_want = {
				"ContainerControl=True",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=False",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new ScrollableControl()), ScrollableControl_want, "ScrollableControlStyles");
		}


		[Test]
		public void ContainerControlStyleTest ()
		{
			string[] ContainerControl_want = {
				"ContainerControl=True",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=False",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new ContainerControl()), ContainerControl_want, "ContainerControlStyles");
		}


		[Test]
		public void FormStyleTest ()
		{
			string[] Form_want = {
				"ContainerControl=True",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=False",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new Form()), Form_want, "FormStyles");
		}


		[Test]
		public void PropertyGridStyleTest ()
		{
			string[] PropertyGrid_want = {
				"ContainerControl=True",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=False",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new PropertyGrid()), PropertyGrid_want, "PropertyGridStyles");
		}


		[Test]
		public void DomainUpDownStyleTest ()
		{
			string[] DomainUpDown_want = {
				"ContainerControl=True",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=True",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=False",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new DomainUpDown()), DomainUpDown_want, "DomainUpDownStyles");
		}


		[Test]
		public void NumericUpDownStyleTest ()
		{
			string[] NumericUpDown_want = {
				"ContainerControl=True",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=True",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=False",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new NumericUpDown()), NumericUpDown_want, "NumericUpDownStyles");
		}


		[Test]
		public void UserControlStyleTest ()
		{
			string[] UserControl_want = {
				"ContainerControl=True",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=False",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new UserControl()), UserControl_want, "UserControlStyles");
		}


		[Test]
		public void PanelStyleTest ()
		{
			string[] Panel_want = {
				"ContainerControl=True",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=False",
				"UserMouse=False",
				"SupportsTransparentBackColor=True",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=False",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new Panel()), Panel_want, "PanelStyles");
		}


		[Test]
		public void TabPageStyleTest ()
		{
			string[] TabPage_want = {
				"ContainerControl=True",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=False",
				"UserMouse=False",
				"SupportsTransparentBackColor=True",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=False",
				"CacheText=True",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new TabPage()), TabPage_want, "TabPageStyles");
		}


		[Test]
		public void HScrollBarStyleTest ()
		{
			string[] HScrollBar_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new HScrollBar()), HScrollBar_want, "HScrollBarStyles");
		}


		[Test]
		public void VScrollBarStyleTest ()
		{
			string[] VScrollBar_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new VScrollBar()), VScrollBar_want, "VScrollBarStyles");
		}


		[Test]
		public void SplitterStyleTest ()
		{
			string[] Splitter_want = {
				"ContainerControl=False",
				"UserPaint=True",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=False",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new Splitter()), Splitter_want, "SplitterStyles");
		}


		[Test]
		public void StatusBarStyleTest ()
		{
			string[] StatusBar_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=False",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new StatusBar()), StatusBar_want, "StatusBarStyles");
		}


		[Test]
		public void TabControlStyleTest ()
		{
			string[] TabControl_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new TabControl()), TabControl_want, "TabControlStyles");
		}


		[Test]
		public void RichTextBoxStyleTest ()
		{
			string[] RichTextBox_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new RichTextBox()), RichTextBox_want, "RichTextBoxStyles");
		}


		[Test]
		public void TextBoxStyleTest ()
		{
			string[] TextBox_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=True",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=False",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new TextBox()), TextBox_want, "TextBoxStyles");
		}


		[Test]
		public void DataGridTextBoxStyleTest ()
		{
			string[] DataGridTextBox_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=True",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=False",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new DataGridTextBox()), DataGridTextBox_want, "DataGridTextBoxStyles");
		}


		[Test]
		public void ToolBarStyleTest ()
		{
			string[] ToolBar_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=True",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new ToolBar()), ToolBar_want, "ToolBarStyles");
		}


		[Test]
		public void TrackBarStyleTest ()
		{
			string[] TrackBar_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=True",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new TrackBar()), TrackBar_want, "TrackBarStyles");
		}


		[Test]
		public void TreeViewStyleTest ()
		{
			string[] TreeView_want = {
				"ContainerControl=False",
				"UserPaint=False",
				"Opaque=False",
				"ResizeRedraw=False",
				"FixedWidth=False",
				"FixedHeight=False",
				"StandardClick=False",
				"Selectable=True",
				"UserMouse=False",
				"SupportsTransparentBackColor=False",
				"StandardDoubleClick=True",
				"AllPaintingInWmPaint=True",
				"CacheText=False",
				"EnableNotifyMessage=False",
				"DoubleBuffer=False"
			};

			Assert.AreEqual(GetStyles(new TreeView()), TreeView_want, "TreeViewStyles");
		}


	}
}

