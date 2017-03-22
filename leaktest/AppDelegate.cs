using System;
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using MuPdfNamespace;
using UIKit;

namespace leaktest
{
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window {
			get;
			set;
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			Window = new UIWindow (UIScreen.MainScreen.Bounds);

			MuPdfNamespace.PDFDocument.Init ();

			//TODO [required] Specify path to pdf
			RunTestWorker ("gre_research_validity_data.pdf");

			return true;
		}

		async Task RunTestWorker (string somePdfFilePath)
		{
			try {
				await Task.Run (() => {

					//run it several times to make memory leak more perceptible
					int iterations = 20;
					for (int i = 1; i < iterations; i++) {
						Console.WriteLine ($"Iteration {i} of {iterations}");
						TestMethod (somePdfFilePath);
					}

					Console.WriteLine ("Completed!");
				});
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}

		public void TestMethod (string pdfFilePath)
		{
			try {
				if (File.Exists (pdfFilePath)) {
					using (PDFDocument pdfDocument = new PDFDocument ()) {
						bool pdfLoaded = false;

						try {
							pdfLoaded = pdfDocument.LoadPDF (pdfFilePath);
						} catch (Exception ex) {
							Console.WriteLine ("Could not load pdf: {0}", ex.Message);
						}

						if (pdfLoaded) {
							for (int i = 0; i < pdfDocument.PageCount; i++) {
								Console.WriteLine ("Processing page: {0}", i);

								//TODO: [optional] you can specify thumbnailPath here if you do want to save thumbnails on disk
								string thumbnailPath = $"";

								SaveThumbnailGetSize (pdfDocument, i, thumbnailPath);
							}
						}
					}
				}
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}

		CGSize SaveThumbnailGetSize (PDFDocument pdfDocument, int pdfPage, string filename)
		{
			CGSize size = pdfDocument.GetPageSize (pdfPage);
			nfloat K = 750f / size.Width < 1 ? 750f / size.Width : 1;

			MuPDFLib.Cookie cookie = new MuPDFLib.Cookie ();

			using (NativeBitmap img = pdfDocument.RenderToBitmap (PDFDocument.context, new CGRect (0, 0, (int)Math.Round (size.Width * K), (int)Math.Round (size.Height * K)), K, pdfPage, 0, ref cookie)) {
				NSError err = null;
				using (var uimg = ((UIImage)img)) {

					using (NSData data = uimg.AsJPEG (0.4f)) { // Memory leak occurs only with this line. If comment this, memory leak will no occur.

						//[optional line]
						//data.Save (filename, true, out err);
					}
				}

				if (err != null) {
					Console.WriteLine ("Could not save thumbnail: {0}", err);
				}
			}

			return size;
		}
	}
}

