//
// System.Drawing.Imaging.ColorPalette.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging
{
	public sealed class ColorPalette {
		// 0x1: the color values in the array contain alpha information
		// 0x2: the color values are grayscale values.
		// 0x4: the colors in the array are halftone values.

		int flags;
		Color [] entries;

		//
		// There is no public constructor, this will be used somewhere in the
		// drawing code
		//
		internal ColorPalette ()
		{
			flags = 0;
			entries = new Color [0];
		}

		internal ColorPalette (int flags, Color[] colors) {
			this.flags = flags;
			entries = colors;
		}

		public Color [] Entries {
			get {
				return entries;
			}
		}

		public int Flags {
			get {
				return flags;
			}
		}
		
		/* Caller should call FreeHGlobal*/
		internal IntPtr getGDIPalette() 
		{
			GdiColorPalette palette = new GdiColorPalette ();			
			Color[] entries = Entries;
			int entry = 0;			
			int size = Marshal.SizeOf (palette) + (Marshal.SizeOf (entry) * entries.Length);			
			IntPtr lfBuffer = Marshal.AllocHGlobal(size);			
			
			palette.Flags = Flags;
			palette.Count = entries.Length;
			    			
			int[] values = new int[palette.Count];
			
			for (int i = 0; i < values.Length; i++) {
				values[i] = entries[i].ToArgb(); 
				//Console.Write("{0:X} ;", values[i]);			
			}   			
			
			//Console.WriteLine("pal size " + Marshal.SizeOf (palette) + " native " + NativeObject);			
			
			Marshal.StructureToPtr (palette, lfBuffer, false);	
			Marshal.Copy (values, 0, (IntPtr) (lfBuffer.ToInt32() + Marshal.SizeOf (palette)), values.Length);						    								
			
			return lfBuffer;
		}
		
		internal void setFromGDIPalette(IntPtr palette) 
		{
			
		}
	}
}
