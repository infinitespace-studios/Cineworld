﻿using System;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using Foundation;

namespace CineworldiPhone
{
	public class PerformancesTableSource : UITableViewSource {
		string cellIdentifier = "PerformanceTableCell";

		List<List<PerformanceInfo>> PerformanceGroupedList = new List<List<PerformanceInfo>>();

		public PerformancesTableSource (List<PerformanceInfo> perfs)
		{
			Dictionary<DateTime, List<PerformanceInfo>> perfsByDate = new Dictionary<DateTime, List<PerformanceInfo>>();

			foreach (var perf in perfs) 
			{
				DateTime perfDate = perf.PerformanceTS.Date;
				List<PerformanceInfo> perfList = null;

				if (!perfsByDate.TryGetValue (perfDate, out perfList)) 
				{
					perfList = new List<PerformanceInfo> ();
					perfsByDate [perfDate] = perfList;
				}

				perfList.Add (perf);
			}

			PerformanceGroupedList = new List<List<PerformanceInfo>> (perfsByDate.Values);
		}

//		public override nfloat GetHeightForRow (UITableView tableView, Foundation.NSIndexPath indexPath)
//		{
//			return 135;
//		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return this.PerformanceGroupedList.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (cellIdentifier);

			if (cell != null) 
			{
				(cell as PerformanceTableCell).UpdateCell (PerformanceGroupedList [indexPath.Row]);
			}
			return cell;
		}
	}
}

