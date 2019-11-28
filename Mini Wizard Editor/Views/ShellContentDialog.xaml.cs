﻿using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace ARInstructionsEditor.Views
{
    public sealed partial class ShellContentDialog : ContentDialog
    {
        private ShellContentDialog(Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            InitializeComponent();
            shellFrame.Navigate(pageType, parameter,  infoOverride);
            shellFrame.Width = Window.Current.Bounds.Width * 0.8;
            shellFrame.Height = Window.Current.Bounds.Height * 0.8;
        }

        public static ShellContentDialog Create(Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            return new ShellContentDialog(pageType, parameter, infoOverride);
        }
    }
}
