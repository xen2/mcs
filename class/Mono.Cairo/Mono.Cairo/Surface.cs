//
// Mono.Cairo.Surface.cs
//
// Authors:
//    Duncan Mak
//    Miguel de Icaza.
//
// (C) Ximian Inc, 2003.
// (C) Novell, Inc. 2003.
//
// This is an OO wrapper API for the Cairo API
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Collections;

namespace Cairo {

	public class Surface : IDisposable 
        {						
		protected static Hashtable surfaces = new Hashtable ();
                internal IntPtr surface = IntPtr.Zero;

		protected Surface()
		{
		}
		
                protected Surface (IntPtr ptr, bool owns)
                {
                        surface = ptr;
			lock (surfaces.SyncRoot){
				surfaces [ptr] = this;
			}
			if (!owns)
				CairoAPI.cairo_surface_reference (ptr);
                }

		static internal Surface LookupExternalSurface (IntPtr p)
		{
			lock (surfaces.SyncRoot){
				object o = surfaces [p];
				if (o == null){
					return new Surface (p, false);
				}
				return (Surface) o;
			}
		}		
		
		[Obsolete ("Use an ImageSurface constructor instead.")]
                public static Cairo.Surface CreateForImage (
                        ref byte[] data, Cairo.Format format, int width, int height, int stride)
                {
                        IntPtr p = CairoAPI.cairo_image_surface_create_for_data (
                                data, format, width, height, stride);
                        
                        return new Cairo.Surface (p, true);
                }

		[Obsolete ("Use an ImageSurface constructor instead.")]
                public static Cairo.Surface CreateForImage (
                        Cairo.Format format, int width, int height)
                {
                        IntPtr p = CairoAPI.cairo_image_surface_create (
                                format, width, height);

                        return new Cairo.Surface (p, true);
                }


                public Cairo.Surface CreateSimilar (
                        Cairo.Content content, int width, int height)
                {
                        IntPtr p = CairoAPI.cairo_surface_create_similar (
                                this.Handle, content, width, height);

                        return new Cairo.Surface (p, true);
                }

		~Surface ()
		{
			Dispose (false);
		}

		public void Show (Context gr, int width, int height) 
		{
			CairoAPI.cairo_set_source_surface (gr.Handle, surface, width, height);
			CairoAPI.cairo_paint (gr.Handle);
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (surface == (IntPtr) 0)
				return;
			
			lock (surfaces.SyncRoot)
				surfaces.Remove (surface);

			CairoAPI.cairo_surface_destroy (surface);
			surface = (IntPtr) 0;
		}
		
		public Status Finish ()
		{
			CairoAPI.cairo_surface_finish (surface);
			return Status;
		}
		
		public void Flush ()
		{
			CairoAPI.cairo_surface_flush (surface);
		}
		
		public void MarkDirty ()
		{
			CairoAPI.cairo_surface_mark_dirty (Handle);
		}
		
		public void MarkDirty (Rectangle rectangle)
		{
			CairoAPI.cairo_surface_mark_dirty_rectangle (Handle, (int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
		}
		
                public IntPtr Handle {
                        get {
				return surface;
			}
                }

		public PointD DeviceOffset {
			set {
				CairoAPI.cairo_surface_set_device_offset (surface, value.X, value.Y);
			}
		}
		
		public void Destroy()
		{
			Dispose (true);
		}

#if CAIRO_1_2
		public void SetFallbackResolution (double x, double y)
		{
			CairoAPI.cairo_surface_set_fallback_resolution (surface, x, y);
		}
#endif

		public void WriteToPng (string filename)
		{
			CairoAPI.cairo_surface_write_to_png (surface, filename);
		}
		
		[Obsolete ("Use Handle instead.")]
                public IntPtr Pointer {
                        get {
				return surface;
			}
                }
		
		public Status Status {
			get { return CairoAPI.cairo_surface_status (surface); }
		}

        }
}
