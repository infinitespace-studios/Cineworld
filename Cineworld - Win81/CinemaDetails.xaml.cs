﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Cineworld
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class CinemaDetails : Cineworld.Common.LayoutAwarePage
    {
        internal enum CinemaView
        {
            ShowByDate,
            Current,
            Upcoming,
        }

        CinemaView currentCinemaView = CinemaView.ShowByDate;

        CineworldService cws = new CineworldService();
        CinemaInfo SelectedCinema { get; set; }

        System.Collections.ObjectModel.ObservableCollection<GroupInfoList<object>> FilmsForSelectedDate = new System.Collections.ObjectModel.ObservableCollection<GroupInfoList<object>>();
        
        static SettingsCommand command = null;
        static SettingsCommand command2 = null;
        static SettingsCommand command3 = null;
        static SettingsCommand command4 = null;

        public CinemaDetails()
        {
            this.InitializeComponent();

            this.DataContext = this;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void SpinAndWait(bool bNewVal)
        {
            this.gBody.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
            this.gProgress.Visibility = bNewVal ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            this.prProgress.IsActive = bNewVal;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!Config.ShowCleanBackground)
            {
                this.LayoutRoot.Background = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(this.BaseUri, "/Assets/Cineworld_V2_846x468.png")),
                    Opacity = 0.2,
                    Stretch = Stretch.UniformToFill
                };
            }

            this.AllowSearch(false);

            DataTransferManager.GetForCurrentView().DataRequested += CinemaDetails_DataRequested;

            SpinAndWait(true);

            int iCin = (int)e.Parameter;

            bool bError = false;
            try
            {
                if (App.Cinemas == null || App.Cinemas.Count == 0)
                {
                    LocalStorageHelper lsh = new LocalStorageHelper();
                    await lsh.DownloadFiles(false);

                    await lsh.DeserialiseObjects();
                }

                SelectedCinema = App.Cinemas[iCin];
            }
            catch
            {
                bError = true;
            }

            if (bError)
            {
                await new MessageDialog("Error fetching cinemas details").ShowAsync();
                //return;
            }
            else
            {
                if (SelectedCinema != null)
                    LoadCinemaDetails();

                try
                {
                    if (!App.CinemaFilms.ContainsKey(iCin))
                    {
                        await new LocalStorageHelper().GetCinemaFilmListings(SelectedCinema.ID, false);
                    }

                    LoadFilmList(App.CinemaFilms[SelectedCinema.ID]);

                    string cinemastr = SelectedCinema.ID.ToString();
                    IReadOnlyList<SecondaryTile> tiles = await SecondaryTile.FindAllAsync();
                    SecondaryTile tile = tiles.FirstOrDefault(t => t.TileId == cinemastr);

                    if (tile == null)
                    {
                        this.btnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        this.btnUnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    else
                    {
                        this.btnUnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        this.btnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }

                    if (Config.FavCinemas.Contains(iCin))
                    {
                        this.btnFavourite.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        this.btnUnfavourite.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    else
                    {
                        this.btnFavourite.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        this.btnUnfavourite.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }

                }
                catch
                {
                    bError = true;
                }

                if (bError)
                {
                    await new MessageDialog("Error downloading showtime data").ShowAsync();
                }
            }

            SpinAndWait(false);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataTransferManager.GetForCurrentView().DataRequested -= CinemaDetails_DataRequested;

            base.OnNavigatedFrom(e);
        }

        PerformanceInfo perf = null;

        void CinemaDetails_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (perf != null)
            {
                args.Request.Data.Properties.Title = String.Format("\"{0}\" at Cineworld {1}", perf.FilmTitle, SelectedCinema.Name);
                args.Request.Data.Properties.Description = (String.Format("Shall we go and see it {0} at {1}? Book here {2}", DateTimeToStringConverter.ConvertData(perf.PerformanceTS), perf.PerformanceTS.ToString("HH:mm"), perf.BookUrl.ToString()));
                args.Request.Data.SetUri(perf.BookUrl);

                perf = null;
            }
            else
            {
                args.Request.Data.Properties.Title = "movie tonight?";
                args.Request.Data.SetText(String.Format("Shall we go and see something at Cineworld {0}?", SelectedCinema.Name));
            }
        }

        private void LoadCinemaDetails()
        {
            this.DataContext = SelectedCinema;
        }

        FilmData fdViewViewByDate = null;

        private void LoadFilmList(List<FilmInfo> films)
        {
            this.currentCinemaView = CinemaView.ShowByDate;

            fdViewViewByDate = new FilmData(films);

            cvsShowByDate.Source = FilmsForSelectedDate;
            
            this.SetFilmsForSelectedDate(DateTime.Today);

            //(semanticZoomShowByDate.ZoomedOutView as ListViewBase).ItemsSource = fdViewViewByDate.GetFilmHeaders(dataLetter);

            semanticZoomShowByDate.ViewChangeStarted -= semanticZoom_ViewChangeStarted;
            semanticZoomShowByDate.ViewChangeStarted += semanticZoom_ViewChangeStarted;
        }

        private void SetFilmsForSelectedDate(DateTime userSelection)
        {
            if (fdViewViewByDate == null)
                return;
            try
            {
                var dataLetter = fdViewViewByDate.GetGroupForDate(userSelection.Date);

                FilmsForSelectedDate.Clear();

                foreach (var entry in dataLetter)
                    FilmsForSelectedDate.Add(entry);

                (semanticZoomShowByDate.ZoomedOutView as ListViewBase).ItemsSource = fdViewViewByDate.GetFilmHeaders(dataLetter);

                (semanticZoomShowByDate.ZoomedOutView as ListViewBase).UpdateLayout();
                
                GroupInfoList<object> group = dataLetter.Find(g => g.Count > 0);

                if (group != null)
                    (this.semanticZoomShowByDate.ZoomedInView as ListViewBase).ScrollIntoView(group);
            }
            catch { }
        }

        private void SetCinemaView(CinemaView newCinemaView)
        {
            if (this.currentCinemaView == newCinemaView)
                return;

            this.currentCinemaView = newCinemaView;

            if(currentCinemaView == CinemaView.ShowByDate)
            {
                this.SetFilmsForSelectedDate((DateTime)this.dpShowing.Value);

                this.semanticZoomShowByDate.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.semanticZoomFilmList.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                return;
            }

            List<GroupInfoList<object>> filmDataSet = null;

            switch (this.currentCinemaView)
            {
                case CinemaView.Current:
                    filmDataSet = fdViewViewByDate.CurrentFilmGroups;
                    break;

                case CinemaView.Upcoming:
                    filmDataSet = fdViewViewByDate.UpcomingFilmGroups;
                    break;
            }

            this.semanticZoomShowByDate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.semanticZoomFilmList.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if (filmDataSet != null)
            {
                FilmsForSelectedDate.Clear();

                foreach (var entry in filmDataSet)
                    FilmsForSelectedDate.Add(entry);
                
                (this.semanticZoomFilmList.ZoomedOutView as ListViewBase).ItemsSource = fdViewViewByDate.GetFilmHeaders(filmDataSet);

                (this.semanticZoomFilmList.ZoomedOutView as ListViewBase).UpdateLayout();

                GroupInfoList<object> group = filmDataSet.Find(g => g.Count > 0);

                if (group != null)
                    (this.semanticZoomFilmList.ZoomedInView as ListViewBase).ScrollIntoView(group);
            }
        }


        void semanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.SourceItem == null)
                return;

            if (e.SourceItem.Item.GetType() == typeof(HeaderItem))
            {
                HeaderItem hi = (HeaderItem)e.SourceItem.Item;

                if (hi.Group != null)
                    e.DestinationItem = new SemanticZoomLocation() { Item = hi.Group };
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SemanticZoom sz = this.semanticZoomShowByDate;

            sz.IsZoomedInViewActive = false;
        }

        private void btnFilledViewOnly_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.ViewManagement.ApplicationView.TryUnsnap();
        }

        private async void btnPinToStartMenu_Click(object sender, RoutedEventArgs e)
        {
            SecondaryTile secondary = new SecondaryTile(SelectedCinema.ID.ToString(),
                SelectedCinema.Name, SelectedCinema.Name, Config.Region.ToString(),
                TileOptions.ShowNameOnLogo, new Uri("ms-appx:///Assets/Logo.png"));

            if (await secondary.RequestCreateAsync())
            {
                this.btnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.btnUnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }


        private async void btnUnPinToStartMenu_Click(object sender, RoutedEventArgs e)
        {
            string cinemastr = SelectedCinema.ID.ToString();
            IReadOnlyList<SecondaryTile> tiles = await SecondaryTile.FindAllAsync();
            SecondaryTile tile = tiles.FirstOrDefault(t => t.TileId == cinemastr);

            if (tile != null && await tile.RequestDeleteAsync())
            {
                this.btnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.btnUnPinToStartMenu.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            }
        }

        private void btnUnfavourite_Click(object sender, RoutedEventArgs e)
        {
            List<int> Cinemas = Config.FavCinemas;

            if (Cinemas.Contains(SelectedCinema.ID))
            {
                Cinemas.Remove(SelectedCinema.ID);
                Config.FavCinemas = Cinemas;
            }

            this.btnFavourite.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.btnUnfavourite.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void btnFavourite_Click(object sender, RoutedEventArgs e)
        {
            List<int> Cinemas = Config.FavCinemas;

            if (!Cinemas.Contains(SelectedCinema.ID))
            {
                Cinemas.Add(SelectedCinema.ID);
                Config.FavCinemas = Cinemas;
            }

            this.btnFavourite.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.btnUnfavourite.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private async void btnViewOnMap_Click(object sender, RoutedEventArgs e)
        {
            string BingMapsUri = String.Format("http://www.bing.com/maps/default.aspx?cp={0}~{1}&lvl=18&style=u", SelectedCinema.Latitude, SelectedCinema.Longitute);
            Uri uri = new Uri(BingMapsUri, UriKind.Absolute);
            await Launcher.LaunchUriAsync(uri);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            PerformanceInfo pi = (PerformanceInfo)(sender as Button).CommandParameter;
            
            PopupMenu menu = new PopupMenu();
            UICommand cmdBuyTickets = new UICommand("buy tickets", async (command) =>
            {
                if (Config.UseMobileWeb)
                {
                    InAppBrowser.NavigationUri = pi.BookUrl;
                    this.Frame.Navigate(typeof(InAppBrowser));
                }
                else
                {
                    await Launcher.LaunchUriAsync(pi.BookUrl);
                }
            });
            menu.Commands.Add(cmdBuyTickets);

            UICommand cmdShare = new UICommand("share details", (command) =>
            {
                perf = pi;
                DataTransferManager.ShowShareUI();
            });

            menu.Commands.Add(cmdShare);

            var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender)); 
            if (chosenCommand == null) // The command is null if no command was invoked. 
            {
                perf = null;
            } 
        }

        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        } 

        private void btnReviews_Click(object sender, RoutedEventArgs e)
        {
            Flyout flyOut = new Flyout();
            flyOut.Content = new ViewReviews();
            ViewReviews.ReviewTarget = Review.ReviewTargetDef.Cinema;
            ViewReviews.SelectedCinema = SelectedCinema;
            
            flyOut.Placement = FlyoutPlacementMode.Top;
            flyOut.ShowAt(sender as FrameworkElement);
        }

        private void btnRateCinema_Click(object sender, RoutedEventArgs e)
        {
            Flyout flyOut = new Flyout();
            flyOut.Content = new Review();
            Review.ReviewTarget = Review.ReviewTargetDef.Cinema;
            Review.SelectedCinema = SelectedCinema;

            flyOut.Placement = FlyoutPlacementMode.Top;
            flyOut.ShowAt(sender as FrameworkElement);
        }

        private void radShowByDate_Click(object sender, RoutedEventArgs e)
        {
            this.SetCinemaView(CinemaView.ShowByDate);
        }

        private void radCurrent_Click(object sender, RoutedEventArgs e)
        {
            this.SetCinemaView(CinemaView.Current);
        }

        private void radUpcoming_Click(object sender, RoutedEventArgs e)
        {
            this.SetCinemaView(CinemaView.Upcoming);
        }

        private void dpShowing_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            this.SetFilmsForSelectedDate((DateTime)e.NewValue);
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FilmInfo fi = (FilmInfo)(sender as Image).Tag;

            List<FilmInfo> films = App.CinemaFilms[SelectedCinema.ID];

            ShowPerformances.SelectedFilm = films.Find(f => f.EDI == fi.EDI);

            ShowPerformances.SelectedCinema = SelectedCinema;
            ShowPerformances.ShowFilmDetails = true;
            ShowPerformances.ShowCinemaDetails = false;

            this.Frame.Navigate(typeof(ShowPerformances));
        }

        private void SfRating_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (sender as RadRating).Value = this.SelectedCinema.AverageRating;

            Flyout flyOut = new Flyout();
            flyOut.Content = new ViewReviews();
            ViewReviews.ReviewTarget = Review.ReviewTargetDef.Cinema;
            ViewReviews.SelectedCinema = SelectedCinema;

            flyOut.Placement = FlyoutPlacementMode.Top;
            flyOut.ShowAt(sender as FrameworkElement);
        }
    }
        
}
