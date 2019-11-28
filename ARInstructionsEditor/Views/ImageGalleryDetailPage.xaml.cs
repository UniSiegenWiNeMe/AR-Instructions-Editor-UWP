using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.Toolkit.Uwp.UI.Animations;

using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using ARInstructionsEditor.Core.Models;
using ARInstructionsEditor.Core.Services;
using ARInstructionsEditor.Helpers;
using ARInstructionsEditor.Services;
using Windows.UI.Xaml;
using ARInstructionsEditor.ViewModels;
using System.Windows.Input;

namespace ARInstructionsEditor.Views
{
    public sealed partial class ImageGalleryDetailPage : Page, INotifyPropertyChanged
    {
        private object _selectedImage;

        public object SelectedImage
        {
            get => _selectedImage;
            set
            {
                Set(ref _selectedImage, value);
                ImagesNavigationHelper.UpdateImageId(MasterDetailDetailControl.ImageGallerySelectedIdKey, ((MediaFileViewModel)SelectedImage).FileName);
            }
        }

        public ObservableCollection<MediaFileViewModel> Source { get; } = new ObservableCollection<MediaFileViewModel>();

        public ImageGalleryDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameters = e.Parameter as object[];
            var selectedImageID = parameters[0] as string;
            var currentStepID = (int)parameters[1];

            Source.Clear();

            var data = InstructionDataProvider.GetImagesByStepNumber(currentStepID); /*SampleDataService.GetImageGalleryDataAsync("ms-appx:///Assets");*/

            foreach (var item in data)
            {
                Source.Add(mediaFileToMediaFileViewModel(item));
            }


            if (!string.IsNullOrEmpty(selectedImageID) && e.NavigationMode == NavigationMode.New)
            {
                SelectedImage = Source.FirstOrDefault(i => i.FileName == selectedImageID);
            }
            else
            {
                selectedImageID = ImagesNavigationHelper.GetImageId(MasterDetailDetailControl.ImageGallerySelectedIdKey);
                if (!string.IsNullOrEmpty(selectedImageID))
                {
                    SelectedImage = Source.FirstOrDefault(i => i.FileName == selectedImageID);
                }
            }
        }

        private MediaFileViewModel mediaFileToMediaFileViewModel(MediaFile item)
        {
            return new MediaFileViewModel(item);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                NavigationService.Frame.SetListDataItemForNextConnectedAnimation(SelectedImage);
                ImagesNavigationHelper.RemoveImageId(MasterDetailDetailControl.ImageGallerySelectedIdKey);
            }
        }

        private void OnPageKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
                e.Handled = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
