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

		//public IntPtr bits;
		public byte[] bits;

		public NativeBitmap (int width, int height, int bitCount)
		{
			int bytesInPixel = bitCount / 8;

			colorSpace = CGColorSpace.CreateDeviceRGB ();
			int bitsLength = bytesInPixel * width * height;

			//bits = Marshal.AllocHGlobal (bitsLength);
			bits = new byte[bitsLength];
			Console.WriteLine($"allocating {bits.Length / 1024} KB");

			//cgdata = new CGDataProvider (bits, bitsLength);
			cgdata = new CGDataProvider(bits, 0, bitsLength);

			cgimage = new CGImage (width, height, 8, bitCount, bytesInPixel * width, colorSpace, CGImageAlphaInfo.None | (CGImageAlphaInfo)CGBitmapFlags.ByteOrder32Big, cgdata, null, false, CGColorRenderingIntent.Default);
			image = UIImage.FromImage (cgimage);
		}

		public static implicit operator UIImage (NativeBitmap gbmp)
		{
			return gbmp == null ? null : gbmp.image;
		}

		public void Dispose ()
		{
			if (cgdata != null)
			{
				cgdata.Dispose();
				cgdata = null;
			}

			if (cgimage != null)
			{
				cgimage.Dispose();
				cgimage = null;
			}

			if (image != null)
			{
				image.Dispose();
				image = null;
			}

			if (colorSpace != null)
			{
				colorSpace.Dispose();
				colorSpace = null;
			}

			//if (bits != IntPtr.Zero)
			//	Marshal.FreeHGlobal (bits);

			bits = null;

		}
	}
}

