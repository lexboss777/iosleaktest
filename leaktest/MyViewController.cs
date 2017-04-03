using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using MuPdfNamespace;
using ObjCRuntime;
using UIKit;

namespace leaktest
{
	public partial class MyViewController : UIViewController
	{
		UIButton btn, cgcollectBtn;

		public MyViewController() : base("MyViewController", null)
		{
			View.BackgroundColor = UIColor.Gray;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.

			btn = new UIButton(UIButtonType.System);
			btn.SetTitle("clickme", UIControlState.Normal);
			btn.TouchUpInside += Btn_TouchUpInside;

			View.AddSubview(btn);

			btn.Frame = new CGRect(200, 400, 60, 20);


			cgcollectBtn = new UIButton(UIButtonType.System);
			cgcollectBtn.SetTitle("gc collect", UIControlState.Normal);
			cgcollectBtn.TouchUpInside += (sender, e) =>
			{
				GC.Collect();
			}
			;
			View.AddSubview(cgcollectBtn);

			cgcollectBtn.Frame = new CGRect(200, 600, 60, 20);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		void RunTestWorker()
		{
			var somePdfFilePath = "gre_research_validity_data.pdf";
			TestMethod(somePdfFilePath);
			Console.WriteLine("Completed!");
		}

		async void Btn_TouchUpInside(object sender, EventArgs e)
		{
			Task t = null;
			try
			{
				t = new Task(RunTestWorker);
				t.Start();
				await t;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				if (t != null)
					t.Dispose();
				t = null;

				btn.TouchUpInside -= Btn_TouchUpInside;
				btn.RemoveFromSuperview();
				btn.Dispose();
				btn = null;
			}
		}

		public void TestMethod(string pdfFilePath)
		{
			try
			{
				if (File.Exists(pdfFilePath))
				{
					using (PDFDocument pdfDocument = new PDFDocument())
					{
						bool pdfLoaded = false;

						try
						{
							pdfLoaded = pdfDocument.LoadPDF(pdfFilePath);
						}
						catch (Exception ex)
						{
							Console.WriteLine("Could not load pdf: {0}", ex.Message);
						}

						if (pdfLoaded)
						{
							for (int Y = 0; Y < 5; Y++)
							{
								for (int i = 0; i < pdfDocument.PageCount; i++)
								{
									Console.WriteLine("Processing page: {0}", i);

									//TODO: [optional] you can specify thumbnailPath here if you do want to save thumbnails on disk
									string thumbnailPath = $"";

									SaveThumbnailGetSize(pdfDocument, i, thumbnailPath);
								}
							}
						}
					}

					MuPDFLib.Deinit();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		CGSize SaveThumbnailGetSize(PDFDocument pdfDocument, int pdfPage, string filename)
		{
			CGSize size = pdfDocument.GetPageSize(pdfPage);
			nfloat K = 750f / size.Width < 1 ? 750f / size.Width : 1;

			MuPDFLib.Cookie cookie = new MuPDFLib.Cookie();

			using (NativeBitmap img = pdfDocument.RenderToBitmap(PDFDocument.context, new CGRect(0, 0, (int)Math.Round(size.Width * K), (int)Math.Round(size.Height * K)), K, pdfPage, 0, ref cookie))
			{
				NSError err = null;
				using (var uimg = ((UIImage)img))
				{

					using (NSData data = uimg.AsJPEG(0.4f))
					{ // Memory leak occurs only with this line. If comment this, memory leak will no occur.
					
					}
				}

				if (err != null)
				{
					Console.WriteLine("Could not save thumbnail: {0}", err);
				}
			}

			return size;
		}
	}
}

