using System;
using CoreGraphics;
using UIKit;
using System.Runtime.InteropServices;

namespace MuPdfNamespace
{
	public class NativeBitmap : IDisposable
	{
		CGDataProvider cgdata;
		CGImage cgimage;
		UIImage image;
		CGColorSpace colorSpace;
		public IntPtr bits;

#if PDFDebug
        static int refs = 0;
#endif

		public NativeBitmap (int width, int height, int bitCount)
		{
			int bytesInPixel = bitCount / 8;

			colorSpace = CGColorSpace.CreateDeviceRGB ();
			int bitsLength = bytesInPixel * width * height;
			bits = Marshal.AllocHGlobal (bitsLength);
			cgdata = new CGDataProvider (bits, bitsLength);
			cgimage = new CGImage (width, height, 8, bitCount, bytesInPixel * width, colorSpace, CGImageAlphaInfo.None | (CGImageAlphaInfo)CGBitmapFlags.ByteOrder32Big, cgdata, null, false, CGColorRenderingIntent.Default);
			image = UIImage.FromImage (cgimage);

#if PDFDebug
            Console.WriteLine(string.Format("NativeBitmap alloc {0}", ++refs));
#endif
		}

		public static implicit operator UIImage (NativeBitmap gbmp)
		{
			return gbmp == null ? null : gbmp.image;
		}

		public void Dispose ()
		{
			if (cgdata != null)
				cgdata.Dispose ();

			if (cgimage != null)
				cgimage.Dispose ();

			if (image != null)
				image.Dispose ();

			if (colorSpace != null)
				colorSpace.Dispose ();

			if (bits != IntPtr.Zero)
				Marshal.FreeHGlobal (bits);

#if PDFDebug
            Console.WriteLine(string.Format("NativeBitmap free {0}", --refs));
#endif
		}
	}
}

