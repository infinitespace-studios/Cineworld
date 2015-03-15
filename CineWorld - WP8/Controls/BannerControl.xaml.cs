﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CineWorld;
using Windows.ApplicationModel.Store;

namespace Cineworld
{
    public partial class BannerControl : UserControl
    {
        public BannerControl()
        {
            InitializeComponent();

            this.btnHub.IsFrozen = !App.IsFree;

            if (App.IsFree)
            {
                this.btnHub.Tap -= btnHub_Tap;
                this.btnHub.Tap += btnHub_Tap;

                this.adControl.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.adControl.IsEnabled = false;
                this.adControl.Visibility = System.Windows.Visibility.Collapsed;
            }
        }   

        public static readonly DependencyProperty ViewTitleProperty = DependencyProperty.RegisterAttached("ViewTitle", typeof(String), typeof(BannerControl), new PropertyMetadata("", OnViewTitleChanged));
        private static void OnViewTitleChanged(DependencyObject theTarget, DependencyPropertyChangedEventArgs e)
        {
            BannerControl bc = theTarget as BannerControl;
            if (bc != null)
            {
                string value = e.NewValue as string;
                if (String.IsNullOrEmpty(value))
                {
                    bc.tbTitle.Text = String.Empty;
                    bc.tbTitle.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bc.tbTitle.Text = value;
                    bc.tbTitle.Visibility = Visibility.Visible;
                }
            }
        }
        public string ViewTitle
        {
            get { return GetValue(ViewTitleProperty).ToString(); }
            set { SetValue(ViewTitleProperty, value); }
        }

        private void adControl_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {

        }

        private async void btnRemoveAds_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                await CurrentApp.RequestProductPurchaseAsync(App.AdFreeIAP, false);
            }
            catch { }
        }

        private async void btnHub_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                await CurrentApp.RequestProductPurchaseAsync(App.AdFreeIAP, false);
            }
            catch { }
        }
    }
}
