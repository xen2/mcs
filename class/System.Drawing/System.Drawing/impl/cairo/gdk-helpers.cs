using System;
using System.Runtime.InteropServices;

namespace Gdk {

        internal class Pixbuf
        {
                const string libgdk_pixbuf = "libgdk_pixbuf-2.0.0.dll";
                
		[DllImport (libgdk_pixbuf, EntryPoint="gdk_pixbuf_new")]
		internal static extern IntPtr New (
                        int colorspace, bool has_alpha, int bits_per_sample,
                        int width, int height);

		[DllImport(libgdk_pixbuf, EntryPoint = "gdk_pixbuf_new_from_data")]
		internal static extern IntPtr NewFromData (
                        IntPtr data, Gdk.Colorspace colorspace, bool has_alpha, int bits_per_sample,
                        int width, int height, int rowstride,
                        IntPtr destroy_fn, IntPtr destroy_fn_data);

		[DllImport(libgdk_pixbuf, EntryPoint = "gdk_pixbuf_finalize")]
                internal static extern void Finalize (IntPtr pixbuf);

                [DllImport (libgdk_pixbuf, EntryPoint = "gdk_pixbuf_get_pixels")]
                internal static extern IntPtr GetPixels (IntPtr pixbuf);

                [DllImport (libgdk_pixbuf, EntryPoint = "gdk_pixbuf_get_rowstride")]
                internal static extern int GetRowstride (IntPtr pixbuf);
        }

        internal enum Colorspace {
                Rgb = 0
        }
}
