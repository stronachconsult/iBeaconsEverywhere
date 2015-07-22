using System;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;
using System.Linq;

namespace iBeaconsEverywhere.iOS
{
	public partial class MasterViewController : UITableViewController
	{
		private DataSource _dataSource;
        private CLLocationManager _locationManager;
	    private List<CLBeaconRegion> _beaconRegions; 

	    private static readonly List<string> _uuidList = new List<string>()
	    {
	        {"6dc7e358-6d0a-4da8-8f99-0a1efe58a81c"}, //arubas
	        {"b9407f30-f5f8-466e-aff9-25556b57fe6d"} //estimotes
	    };

		public MasterViewController () : base ("MasterViewController", null)
		{
            //Title = NSBundle.MainBundle.LocalizedString("Master", "iBeacons Everywhere");

            Near = UIImage.FromBundle("Images/square_near");
            Far = UIImage.FromBundle("Images/square_far");
            Immediate = UIImage.FromBundle("Images/square_immediate");
            Unknown = UIImage.FromBundle("Images/square_unknown");

            _locationManager = new CLLocationManager();
            _locationManager.RequestAlwaysAuthorization();

            _locationManager.DidRangeBeacons += (object sender, CLRegionBeaconsRangedEventArgs e) =>
            {
                _dataSource.Beacons = e.Beacons;
                TableView.ReloadData();
            };

            _beaconRegions = new List<CLBeaconRegion>();
		}

		public DetailViewController DetailViewController {
			get;
			set;
		}

		public UIImage Near, Far, Immediate, Unknown;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            Title = NSBundle.MainBundle.LocalizedString("Master", "iBeacons Everywhere");

            //Near = UIImage.FromBundle ("Images/square_near");
            //Far = UIImage.FromBundle ("Images/square_far");
            //Immediate = UIImage.FromBundle ("Images/square_immediate");
            //Unknown = UIImage.FromBundle ("Images/square_unknown");

            //_locationManager = new CLLocationManager();
            //_locationManager.RequestAlwaysAuthorization();

            //_beaconRegions = new List<CLBeaconRegion>();
		    foreach (var uid in _uuidList)
		    {
		        var beaconUuid = new NSUuid(uid);
		        var beaconRegion = new CLBeaconRegion(beaconUuid, uid);
		        beaconRegion.NotifyEntryStateOnDisplay = true;
		        beaconRegion.NotifyOnEntry = true;
		        beaconRegion.NotifyOnExit = true;

                _beaconRegions.Add(beaconRegion);
		    }


			_locationManager.RegionEntered += (object sender, CLRegionEventArgs e) => {

					var notification = new UILocalNotification ()
					{
					    AlertBody = string.Format("you have entered the region {0}", e.Region.Identifier)
					};
					UIApplication.SharedApplication.CancelAllLocalNotifications();
					UIApplication.SharedApplication.PresentLocationNotificationNow (notification);
			};


            //_locationManager.DidRangeBeacons += (object sender, CLRegionBeaconsRangedEventArgs e) => {
            //    _dataSource.Beacons = e.Beacons;
            //    TableView.ReloadData();
            //};

		    foreach (var clBeaconRegion in _beaconRegions)
		    {
                _locationManager.StartMonitoring(clBeaconRegion);
                _locationManager.StartRangingBeacons(clBeaconRegion);
		    }
			

			TableView.Source = _dataSource = new DataSource (this);

		}
	
		class DataSource : UITableViewSource
		{
			static readonly NSString CellIdentifier = new NSString ("Cell");
			CLBeacon[] beacons = new CLBeacon[]{};
			readonly MasterViewController controller;

			public DataSource (MasterViewController controller)
			{
				this.controller = controller;
			}

			public CLBeacon[] Beacons {
				get { return beacons; }
				set { beacons = value;}
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return beacons.Length;
			}

			// Customize the appearance of table view cells.
			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell (CellIdentifier);
				if (cell == null) {
					cell = new UITableViewCell (UITableViewCellStyle.Subtitle, CellIdentifier);
					cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				}

				var beacon = beacons [indexPath.Row];


				string message = string.Empty;

				switch (beacon.Proximity) {
				case CLProximity.Immediate:
					message = "Immediate";
					cell.ImageView.Image = controller.Immediate;
					break;
				case CLProximity.Near:
					message = "Near";
					cell.ImageView.Image = controller.Near;
					break;
				case CLProximity.Far:
					message = "Far";
					cell.ImageView.Image = controller.Far;
					break;
				case CLProximity.Unknown:
					message = "?";
					cell.ImageView.Image = controller.Unknown;
					break;
				}

				cell.TextLabel.Text = "M: " + beacon.Major + " m:" + beacon.Minor;

				cell.DetailTextLabel.Text = message + " " + beacon.ProximityUuid.AsString ();

				return cell;
			}

		
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (controller.DetailViewController == null)
					controller.DetailViewController = new DetailViewController ();

				controller.DetailViewController.Beacon =  (beacons [indexPath.Row]);

				// Pass the selected object to the new view controller.
				controller.NavigationController.PushViewController (controller.DetailViewController, true);
			}
		}

		#region Notified
		/*
			beaconRegion.NotifyEntryStateOnDisplay = true;
			beaconRegion.NotifyOnEntry = true;
			beaconRegion.NotifyOnExit = true;
			locationmanager.RegionEntered += (object sender, CLRegionEventArgs e) => {
				if (e.Region.Identifier == beaconId) {

					var notification = new UILocalNotification () { AlertBody = "The Xamarin beacon is close by!" };
					UIApplication.SharedApplication.CancelAllLocalNotifications();
					UIApplication.SharedApplication.PresentLocationNotificationNow (notification);
				}
			};
		locationmanager.StartMonitoring (beaconRegion);

		CLBeaconRegion beaconRegionNotifications;
		*/
		#endregion


		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			Unknown.Dispose ();
			Unknown = null;
			Near.Dispose ();
			Near = null;
			Far.Dispose ();
			Far = null;
			Immediate.Dispose ();
			Immediate = null;
		}
	}
}
