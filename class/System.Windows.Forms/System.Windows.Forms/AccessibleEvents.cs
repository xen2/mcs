//
// System.Windows.Forms.AccessibleEvents.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian = , Inc.  http://www.ximian.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace System.Windows.Forms {

	/// <summary>
	/// </summary>
	public enum AccessibleEvents{
		AcceleratorChange = 32786,
		Create = 32768,
		DefaultActionChange = 32765,
		DescriptionChange = 32781,
		Destroy = 32769,
		Focus = 32773,
		HelpChange = 32784,
		Hide = 32771,
		LocationChange = 32779,
		NameChange = 32780,
		ParentChange = 32783,
		Reorder = 32772,
		Selection = 32774,
		SelectionAdd = 32775,
		SelectionRemove = 32776,
		SelectionWithin = 32777,
		Show = 32770,
		StateChange = 32778,
		SystemAlert = 2,
		SystemCaptureEnd = 9,
		SystemCaptureStart = 8,
		SystemContextHelpEnd = 13,
		SystemContextHelpStart = 12,
		SystemDialogEnd = 17,
		SystemDialogStart = 16,
		SystemDragDropEnd = 15,
		SystemDragDropStart = 14,
		SystemForeground = 3,
		SystemMenuEnd = 5,
		SystemMenuPopupEnd = 7,
		SystemMenuPopupStart = 6,
		SystemMenuStart = 4,
		SystemMinimizeEnd = 23,
		SystemMinimizeStart = 22,
		SystemMoveSizeEnd = 11,
		SystemMoveSizeStart = 10,
		SystemScrollingEnd = 19,
		SystemScrollingStart = 18,
		SystemSound = 1,
		SystemSwitchEnd = 21,
		SystemSwitchStart = 20,
		ValueChange = 32782
	}
}
