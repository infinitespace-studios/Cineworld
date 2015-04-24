// This file has been autogenerated from a class added in the UI designer.
using System;
using Foundation;
using UIKit;
using Xamarin.InAppPurchase;

namespace InAppPurchaseTest
{
	public partial class MainViewController : UITabBarController, IPurchaseViewController
	{
		#region Private Variables
		private PurchaseTableViewController _purchaseTable;
		private StoreTableViewController _storeTable;
		private FeaturesController _featuresController;
		private SettingsController _settingsController;
		private UIStoryboard _Storyboard;
		#endregion

		#region Computed Properties
		/// <summary>
		/// Gets or sets the purchase manager.
		/// </summary>
		/// <value>The purchase manager.</value>
		public InAppPurchaseManager PurchaseManager { get; set; }

		/// <summary>
		/// Gets the storyboard.
		/// </summary>
		/// <value>The storyboard.</value>
		public UIStoryboard Storyboard {
			get { return _Storyboard; }
		}

		/// <summary>
		/// Gets the purchase table.
		/// </summary>
		/// <value>The purchase table.</value>
		public PurchaseTableViewController purchaseTable {
			get { return _purchaseTable; }
		}

		/// <summary>
		/// Gets the store table.
		/// </summary>
		/// <value>The store table.</value>
		public StoreTableViewController storeTable {
			get { return _storeTable; }
		}
		#endregion

		#region Constructors
		public MainViewController (IntPtr handle) : base (handle)
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Attachs to purchase manager.
		/// </summary>
		/// <param name="purchaseManager">Purchase manager.</param>
		public void AttachToPurchaseManager(UIStoryboard Storyboard, InAppPurchaseManager purchaseManager) {

			// Save connection to purchase manager
			_Storyboard = Storyboard;
			PurchaseManager = purchaseManager;

			// Scan sub view controllers
			foreach (UIViewController controller in ChildViewControllers) {
				//Console.WriteLine (controller.ToString ());

				// Wireup sub views to the master purchase controller
				if (controller is PurchaseTableViewController) {
					// Found the previous purchase table, save and initialize
					_purchaseTable = (PurchaseTableViewController)controller;
					_purchaseTable.AttachToPurchaseManager (_Storyboard, purchaseManager);
				} else if (controller is StoreTableViewController) {
					// Found the available products for sale table, save and initialize
					_storeTable = (StoreTableViewController)controller;
					_storeTable.AttachToPurchaseManager (_Storyboard, purchaseManager);
				} else if (controller is FeaturesController) {
					// Found special features, save and initialize
					_featuresController = (FeaturesController)controller;
					_featuresController.AttachToPurchaseManager (_Storyboard, purchaseManager);
				} else if (controller is SettingsController) {
					// Found settings, save and initialize
					_settingsController = (SettingsController)controller;
					_settingsController.AttachToPurchaseManager (_Storyboard, purchaseManager);
				}
			}

		}
		#endregion
	}
}
