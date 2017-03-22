using System;
using System.Text;
using System.IO;
using CoreGraphics;

namespace MuPdfNamespace
{
	public class PDFDocument : IDisposable
	{
		public string FilePath;

		public static IntPtr context;

		private static object staticLock = new object ();

		private PDFDocumentWrapper wrapper;

		private class PDFDocumentWrapper : IDisposable
		{
			public IntPtr Handle;
			public bool Disposed;

			public PDFDocumentWrapper (IntPtr handle)
			{
				Handle = handle;
			}

			public void Dispose ()
			{
				if (Handle != IntPtr.Zero) {
					lock (staticLock)
						MuPDFLib.CloseDocument (Handle);

					Handle = IntPtr.Zero;
					Disposed = true;
				}
			}
		}

		private IntPtr Handle {
			get {
				if (wrapper == null || wrapper.Disposed)
					CreateWrapper ();

				return wrapper.Handle;
			}
		}

		private void CreateWrapper ()
		{
			var handle = IntPtr.Zero;

			if (File.Exists (FilePath)) {
				// utf8 string with terminating zero
				lock (staticLock)
					handle = MuPDFLib.LoadDocument (Encoding.UTF8.GetBytes (FilePath + "\0"));
			}

			wrapper = new PDFDocumentWrapper (handle);
		}

		public bool LoadPDF (string filename)
		{
			if (!Inited)
				return false;

			FilePath = filename;

			CreateWrapper ();

			if (Handle == IntPtr.Zero)
				return false;

			return true;
		}

		public static void CloseContext (IntPtr ctx)
		{
			if (!Inited)
				return;

			MuPDFLib.CloseContext (ctx);
		}

		public int PageCount {
			get {
				if (Handle == IntPtr.Zero)
					return 0;

				lock (this)
					return MuPDFLib.GetPageCount (Handle);
			}
		}

		public CGSize GetPageSize (int pdfPage)
		{
			float width = 0, height = 0;
			lock (staticLock) {
				IntPtr page = MuPDFLib.LoadPage (context, Handle, pdfPage);
				try {
					MuPDFLib.GetPageSize (Handle, page, out width, out height);
					return new CGSize ((int)Math.Ceiling (width), (int)Math.Ceiling (height));
				} finally {
					MuPDFLib.ClosePage (context, Handle, page);
				}
			}
		}

		public void Dispose ()
		{
			if (wrapper != null) {
				wrapper.Dispose ();
				wrapper = null;
			}
		}

		public static bool Inited = false;

		public static void Init ()
		{
			if (!Inited) {
				try {
					
					// 128MB
					UInt32 storeSize = 128 << 20;

					MuPDFLib.Init (storeSize);
					context = MuPDFLib.GetContext ();
					Inited = true;
					Console.WriteLine ("MuPDFLib Store size: " + storeSize);
				} catch (DllNotFoundException) {
					Console.WriteLine ("dll not found!");
				} catch (EntryPointNotFoundException ex) {
					Console.WriteLine (ex);
				}
			}
		}

		public static void Deinit ()
		{
			if (Inited) {
				MuPDFLib.Deinit ();
				context = IntPtr.Zero;
				Inited = false;
			};
		}

		public NativeBitmap RenderToBitmap (IntPtr ctx, CGRect bbox, double K, int pdfPage, int angle, ref MuPDFLib.Cookie cookie)
		{
			lock (this) {
				
				NativeBitmap nbmp = null;

				IntPtr PDFPage = MuPDFLib.LoadPage (ctx, Handle, pdfPage);
				try {
					nbmp = new NativeBitmap ((int)bbox.Width, (int)bbox.Height, 32);
					MuPDFLib.RenderPageToBytes (ctx, Handle, PDFPage, nbmp.bits, (int)bbox.X, (int)bbox.Y, (int)bbox.Width, (int)bbox.Height, (float)K, angle / 90, ref cookie);

					if (cookie.abort != 0) {
						nbmp.Dispose ();
						nbmp = null;
					}
				} finally {
					MuPDFLib.ClosePage (ctx, Handle, PDFPage);
				}

				return nbmp;
			}
		}
	}
}