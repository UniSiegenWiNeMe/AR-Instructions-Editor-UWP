using System;
using System.Linq;
using Microsoft.Toolkit.Uwp.UI.Animations;
using ARInstructionsEditor.Core.Models;
using ARInstructionsEditor.Services;
using ARInstructionsEditor.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using ARInstructionsEditor.Helpers;
using ARInstructionsEditor.Views;
using System.ComponentModel;
using System.IO;
using Windows.Media.Core;

namespace ARInstructionsEditor.Views
{
    public sealed partial class MasterDetailDetailControl : UserControl /*,INotifyPropertyChanged*/
    {
        public const string ImageGallerySelectedIdKey = "ImageGallerySelectedIdKey";
        public StepViewModel MasterMenuItem
        {
            get { return GetValue(MasterMenuItemProperty) as StepViewModel; }
            set { SetValue(MasterMenuItemProperty, value); }
        }

        public static readonly DependencyProperty MasterMenuItemProperty = DependencyProperty.Register("MasterMenuItem", typeof(StepViewModel), typeof(MasterDetailDetailControl), new PropertyMetadata(null, OnMasterMenuItemPropertyChanged));

        //public event PropertyChangedEventHandler PropertyChanged;

        public MasterDetailDetailControl()
        {
            InitializeComponent();
        }

        private static void OnMasterMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MasterDetailDetailControl;
            control.ForegroundElement.ChangeView(0, 0, 1);
        }

        private void ImagesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selected = e.ClickedItem as MediaFileViewModel;
            ImagesNavigationHelper.AddImageId(ImageGallerySelectedIdKey, selected.FileName);
            NavigationService.Frame.SetListDataItemForNextConnectedAnimation(selected);
            NavigationService.Navigate<ImageGalleryDetailPage>(new object[] { selected.FileName, MasterMenuItem.StepNumber - 1 });
        }

        private void ButtonVor_Click(object sender, RoutedEventArgs e)
        {
            MasterMenuItem.DecreaseStepNumber();
        }

        private void ButtonHinten_Click(object sender, RoutedEventArgs e)
        {
            MasterMenuItem.IncreaseStepNumber();
        }
        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            MasterMenuItem.AddNewStep();
        }
        private async void ButtonAddPhoto_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                if(!Directory.Exists(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "media")))
                {
                    Directory.CreateDirectory(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "media"));
                }

                // Application now has read/write access to the picked file
                if (!File.Exists(Path.Combine(ApplicationData.Current.TemporaryFolder.Path,"media", file.Name)))
                {
                    await file.CopyAsync(await StorageFolder.GetFolderFromPathAsync(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "media")));
                }

                MasterMenuItem.AddPhotoFile(new MediaFileViewModel() { FileName = Path.Combine(ApplicationData.Current.TemporaryFolder.Path,"media", file.Name) });
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Photos"));
            }
            else
            {
            }
        }

       

        private async void ButtonAddVideo_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".mp4");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                if (!Directory.Exists(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "media")))
                {
                    Directory.CreateDirectory(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "media"));
                }

                // Application now has read/write access to the picked file
                if (!File.Exists(Path.Combine(ApplicationData.Current.TemporaryFolder.Path,"media", file.Name)))
                {
                    await file.CopyAsync(await StorageFolder.GetFolderFromPathAsync(Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "media")));
                }

                MasterMenuItem.AddVideoFile(new MediaFileViewModel() { FileName = Path.Combine(ApplicationData.Current.TemporaryFolder.Path,"media", file.Name), Type = MediaType.Video });
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Videos"));
            }
            else
            {
            }
        }

        private MediaFileViewModel _lastRightClickedImage;
        private void GridViewImage_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            GridView gridView = (GridView)sender;
            PhotosMenuFlyout.ShowAt(gridView, e.GetPosition(gridView));
            _lastRightClickedImage = (MediaFileViewModel)((FrameworkElement)e.OriginalSource).DataContext;
        }

        private MediaFileViewModel _lastRightClickedVideo;
        private void GridViewVideo_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            GridView gridView = (GridView)sender;
            VideoMenuFlyout.ShowAt(gridView, e.GetPosition(gridView));
            _lastRightClickedVideo = (MediaFileViewModel)((FrameworkElement)e.OriginalSource).DataContext;
        }

        private async void RemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Delete photo?",
                Content = "Are you sure to delete this photo?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            // Delete the file if the user clicked the primary button.
            /// Otherwise, do nothing.
            if (result == ContentDialogResult.Primary)
            {
                // Delete the file.
                MasterMenuItem.RemovePhotoFile(_lastRightClickedImage);
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Photos"));
            }
            else
            {
                // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }

        }

        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (StepViewModel.MaxSteps > 1)
            {
                ContentDialog deleteFileDialog = new ContentDialog
                {
                    Title = "Delete step?",
                    Content = "Are you sure to delete this step?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel"
                };

                ContentDialogResult result = await deleteFileDialog.ShowAsync();

                // Delete the file if the user clicked the primary button.
                /// Otherwise, do nothing.
                if (result == ContentDialogResult.Primary)
                {
                    // Delete the file.
                    MasterMenuItem.RemoveStep();
                    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Photos"));
                }
                else
                {
                    // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                    // Do nothing.
                }
            }
            else
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "This step cannot be deleted",
                    Content = "This step cannot be deleted. An instructions must contain at least one step",
                    PrimaryButtonText = "Ok",
                   
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void RemoveVideo_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Delete Video?",
                Content = "Are you sure to delete this video?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            // Delete the file if the user clicked the primary button.
            /// Otherwise, do nothing.
            if (result == ContentDialogResult.Primary)
            {
                // Delete the file.
                MasterMenuItem.RemoveVideoFile(_lastRightClickedVideo);
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Photos"));
            }
            else
            {
                // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }
        }

        private void GridViewImages_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {

        }
    }

    public class UriToCachedImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var image = new BitmapImage();


            var fileName = (value as string).Split(@"\").Last();
            if (!string.IsNullOrEmpty(fileName))
            {

                StorageFolder storageFolder = ApplicationData.Current.RoamingFolder;
                StorageFile file = storageFolder.GetFileAsync(fileName).GetResults();

                SetImage(image, value as StorageFile);
                return image;
            }
            else
            {
                return null;
            }
        }


        async void SetImage(BitmapImage image, StorageFile file)
        {
            if (file == null) return;
            //var stream = await file.GetThumbnailAsync(ThumbnailMode.ListView);
            var stream = await file.OpenReadAsync();
            await image.SetSourceAsync(stream);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class UriToVideoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            var source = MediaSource.CreateFromUri(new Uri((value as string)));
            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
