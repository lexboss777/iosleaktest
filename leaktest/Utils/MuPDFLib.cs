using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace MuPdfNamespace
{
	public class MuPDFLib
	{
#if WINDOWS
        const string dllName = "MuPDFLib.dll";
#else
		const string dllName = "__Internal";
#endif

		[DllImport (dllName)]
		public static extern void Init (UInt32 max_store);

		[DllImport (dllName)]
		public static extern void Deinit ();

		// UTF8
		[DllImport (dllName, CharSet = CharSet.Ansi)]
		public static extern IntPtr LoadDocument (byte [] filename);

		[DllImport (dllName)]
		public static extern void CloseDocument (IntPtr doc);

		[DllImport (dllName)]
		public static extern int GetPageCount (IntPtr doc);

		[DllImport (dllName)]
		public static extern IntPtr LoadPage (IntPtr ctx, IntPtr doc, int pagenumber);

		[DllImport (dllName)]
		public static extern void ClosePage (IntPtr ctx, IntPtr doc, IntPtr page);

		[DllImport (dllName)]
		public static extern void GetPageSize (IntPtr doc, IntPtr page, out float width, out float height);

		[DllImport (dllName)]
		public static extern void RenderPageToBytes (IntPtr ctx, IntPtr doc, IntPtr page, IntPtr bits, int x, int y, int width, int height, float scale, int iRotation, ref Cookie cookie);

		[DllImport (dllName)]
		public static extern void RenderPageToBytes (IntPtr ctx, IntPtr doc, IntPtr page, byte [] bits, int x, int y, int width, int height, float scale, int iRotation, ref Cookie cookie);

		[DllImport (dllName)]
		public static extern IntPtr GetPageList (IntPtr ctx, IntPtr doc, IntPtr page);

		[DllImport (dllName)]
		public static extern void ClosePageList (IntPtr ctx, IntPtr list);

		[DllImport (dllName)]
		public static extern void RenderPageListToBytes (IntPtr ctx, IntPtr list, IntPtr bits, int x, int y, int width, int height, float scale, int iRotation, ref Cookie cookie);

		[DllImport (dllName)]
		public static extern void RenderPageListToBytes (IntPtr ctx, IntPtr list, byte [] bits, int x, int y, int width, int height, float scale, int iRotation, ref Cookie cookie);

		[DllImport (dllName)]
		public static extern IntPtr CloneContext (IntPtr ctx);

		[DllImport (dllName)]
		public static extern void CloseContext (IntPtr cctx);

		[DllImport (dllName)]
		public static extern IntPtr GetContext ();

		[StructLayout (LayoutKind.Sequential)]
		public struct Cookie
		{
			public Int32 abort;
			public Int32 progress;
			public Int32 progress_max; /* -1 for unknown */
			public Int32 errors;
		};
	}
}
