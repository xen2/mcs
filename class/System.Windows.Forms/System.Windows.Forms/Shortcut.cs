//
// System.Windows.Forms.Shortcut.cs
//
// Author:
//   Dennis Hayes = dennish@raytek.com
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C), 2002 Ximian, Inc.  http://www.ximian.com
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

using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	public enum Shortcut
	{
		Alt0 = 262192,
		Alt1 = 262193,
		Alt2 = 262194,
		Alt3 = 262195,
		Alt4 = 262196,
		Alt5 = 262197,
		Alt6 = 262198,
		Alt7 = 262199,
		Alt8 = 262200,
		Alt9 = 262201,
		AltBksp = 262152,
		AltF1 = 262256,
		AltF2 = 262257,
		AltF3 = 262258,
		AltF4 = 262259,
		AltF5 = 262260,
		AltF6 = 262261,
		AltF7 = 262262,
		AltF8 = 262263,
		AltF9 = 262264,
		AltF10 = 262265,
		AltF11 = 262266,
		AltF12 = 262267,
		Ctrl0 = 131120,
		Ctrl1 = 131121,
		Ctrl2 = 131122,
		Ctrl3 = 131123,
		Ctrl4 = 131124,
		Ctrl5 = 131125,
		Ctrl6 = 131126,
		Ctrl7 = 131127,
		Ctrl8 = 131128,
		Ctrl9 = 131129,
		CtrlA = 131137,
		CtrlB = 131138,
		CtrlC = 131139,
		CtrlD = 131140,
		CtrlDel = 131118,
		CtrlE = 131141,
		CtrlF = 131142,
		CtrlF1 = 131184,
		CtrlF2 = 131185,
		CtrlF3 = 131186,
		CtrlF4 = 131187,
		CtrlF5 = 131188,
		CtrlF6 = 131189,
		CtrlF7 = 131190,
		CtrlF8 = 131191,
		CtrlF9 = 131192,
		CtrlF10 = 131193,
		CtrlF11 = 131194,
		CtrlF12 = 131195,
		CtrlG = 131143,
		CtrlH = 131144,
		CtrlI = 131145,
		CtrlIns = 131117,
		CtrlJ = 131146,
		CtrlK = 131147,
		CtrlL = 131148,
		CtrlM = 131149,
		CtrlN = 131150,
		CtrlO = 131151,
		CtrlP = 131152,
		CtrlQ = 131153,
		CtrlR = 131154,
		CtrlS = 131155,
		CtrlT = 131156,
		CtrlU = 131157,
		CtrlV = 131158,
		CtrlW = 131159,
		CtrlX = 131160,
		CtrlY = 131161,
		CtrlZ = 131162,
		CtrlShift0 = 196656,
		CtrlShift1 = 196657,
		CtrlShift2 = 196658,
		CtrlShift3 = 196659,
		CtrlShift4 = 196660,
		CtrlShift5 = 196661,
		CtrlShift6 = 196662,
		CtrlShift7 = 196663,
		CtrlShift8 = 196664,
		CtrlShift9 = 196665,
		CtrlShiftA = 196673,
		CtrlShiftB = 196674,
		CtrlShiftC = 196675,
		CtrlShiftD = 196676,
		CtrlShiftE = 196677,
		CtrlShiftF = 196678,
		CtrlShiftF1 = 196720,
		CtrlShiftF2 = 196721,
		CtrlShiftF3 = 196722,
		CtrlShiftF4 = 196723,
		CtrlShiftF5 = 196724,
		CtrlShiftF6 = 196725,
		CtrlShiftF7 = 196726,
		CtrlShiftF8 = 196727,
		CtrlShiftF9 = 196728,
		CtrlShiftF10 = 196729,
		CtrlShiftF11 = 196730,
		CtrlShiftF12 = 196731,
		CtrlShiftG = 196679,
		CtrlShiftH = 196680,
		CtrlShiftI = 196681,
		CtrlShiftJ = 196682,
		CtrlShiftK = 196683,
		CtrlShiftL = 196684,
		CtrlShiftM = 196685,
		CtrlShiftN = 196686,
		CtrlShiftO = 196687,
		CtrlShiftP = 196688,
		CtrlShiftQ = 196689,
		CtrlShiftR = 196690,
		CtrlShiftS = 196691,
		CtrlShiftT = 196692,
		CtrlShiftU = 196693,
		CtrlShiftV = 196694,
		CtrlShiftW = 196695,
		CtrlShiftX = 196696,
		CtrlShiftY = 196697,
		CtrlShiftZ = 196698,
		Del = 46,
		F1 = 112,
		F2 = 113,
		F3 = 114,
		F4 = 115,
		F5 = 116,
		F6 = 117,
		F7 = 118,
		F8 = 119,
		F9 = 120,
		F10 = 121,
		F11 = 122,
		F12 = 123,
		Ins = 45,
		None = 0,
		ShiftDel = 65582,
		ShiftF1 = 65648,
		ShiftF2 = 65649,
		ShiftF3 = 65650,
		ShiftF4 = 65651,
		ShiftF5 = 65652,
		ShiftF6 = 65653,
		ShiftF7 = 65654,
		ShiftF8 = 65655,
		ShiftF9 = 65656,
		ShiftF10 = 65657,
		ShiftF11 = 65658,
		ShiftF12 = 65659,
		ShiftIns = 65581
	}
}
