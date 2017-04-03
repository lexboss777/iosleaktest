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

			Window.RootViewController = new MyViewController();

			Window.MakeKeyAndVisible();

			return true;
		}
	}
}

