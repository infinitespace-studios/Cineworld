// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CineworldiPhone
{
	[Register ("FilmsViewController")]
	partial class FilmsViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITableView AllFilmsTable { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UISegmentedControl FilmSegments { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (AllFilmsTable != null) {
				AllFilmsTable.Dispose ();
				AllFilmsTable = null;
			}
			if (FilmSegments != null) {
				FilmSegments.Dispose ();
				FilmSegments = null;
			}
		}
	}
}
