using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	partial class PerformanceCollectionViewCell : UICollectionViewCell
	{
		public PerformanceInfo Performance { get; private set; }
		public PerformanceCollectionViewCell (IntPtr handle) : base (handle)
		{
		}

		public void UpdateCell(PerformanceInfo performance)
		{
			ContentView.Layer.BorderColor = UIColor.DarkGray.CGColor;
			ContentView.Layer.BorderWidth = 1.0f;
			ContentView.BackgroundColor = UIColor.White;
			ContentView.Layer.CornerRadius = 5f;
			ContentView.Layer.MasksToBounds = true;
			ContentView.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			ContentView.Layer.Opaque = true;

			this.Performance = performance;

			this.Time.Text = performance.TimeString;
			this.Type.Text = performance.Type;  

//			var tapGestureRecognizer = new UITapGestureRecognizer(() =>
//				{
//					UIMenuController uim = UIMenuController.SharedMenuController; 
//					uim.SetMenuVisible (true, true);
//				});
//			this.AddGestureRecognizer (tapGestureRecognizer);
		} 
	}
}
