using Microsoft.Toolkit.Uwp.UI.Animations;
using ARInstructionsEditor.Core.Services;
using ARInstructionsEditor.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace ARInstructionsEditor.Views
{
    public sealed partial class StartPage : Page, INotifyPropertyChanged
    {
        public bool _loading;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool loading
        {
            get { return _loading; }
            //set { _loading = value; }
            set { Set(ref _loading, value); }
        }
        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }
            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public StartPage()
        {
            this.InitializeComponent();
        }


        private async void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".zip");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                loading = true;
                using (Stream zipToOpen = await file.OpenStreamForReadAsync())
                {
                    await InstructionDataProvider.LoadDataFromZipAsync(zipToOpen, Path.Combine(ApplicationData.Current.TemporaryFolder.Path, file.DisplayName + ".save"));
                }
            }
            else
            {
            }

            NavigationService.Navigate<MasterDetailPage>();
        }
    }

    public class BooleanToVisiblity : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Boolean && !(bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
