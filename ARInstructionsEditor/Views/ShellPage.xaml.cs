using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using ARInstructionsEditor.Core.Services;
using ARInstructionsEditor.Helpers;
using ARInstructionsEditor.Services;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ARInstructionsEditor.Views
{
    // TODO WTS: You can edit the text for the menu in String/en-US/Resources.resw
    // You can show pages in different ways (update main view, navigate, right pane, new windows or dialog) using MenuNavigationHelper class.
    // Read more about MenuBar project type here:
    // https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/projectTypes/menubar.md
    public sealed partial class ShellPage : Page, INotifyPropertyChanged
    {
        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);

        public ShellPage()
        {
            InitializeComponent();
            NavigationService.Frame = shellFrame;
            MenuNavigationHelper.Initialize(splitView, rightFrame);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            KeyboardAccelerators.Add(_altLeftKeyboardAccelerator);
            KeyboardAccelerators.Add(_backKeyboardAccelerator);
        }

        private void ShellMenuItemClick_Views_MasterDetail(object sender, RoutedEventArgs e)
        {
            MenuNavigationHelper.UpdateView(typeof(MasterDetailPage));
        }

        private void ShellMenuItemClick_File_Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var result = NavigationService.GoBack();
            args.Handled = result;
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

        private async void ShellMenuItemClick_File_Export(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("zip-File", new List<string>() { ".zip" });
            // Default file name if the user does not type one in or select a file to replace
            if (InstructionDataProvider.Instruction != null && !string.IsNullOrEmpty(InstructionDataProvider.Instruction.Name))
            {
                savePicker.SuggestedFileName = InstructionDataProvider.Instruction.Name;
            }

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file
                //var stream = await file.OpenStreamForWriteAsync();

                using (Stream zipFileToSave = await file.OpenStreamForWriteAsync())
                {
                    zipFileToSave.SetLength(0);
                    await InstructionDataProvider.ExportAsync(ApplicationData.Current.TemporaryFolder.Path, file.DisplayName , zipFileToSave);
                }

                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                }
                else
                {
                }
            }
            else
            {
            }
        }

        private async void ShellMenuItemClick_File_Create(object sender, RoutedEventArgs e)
        {
            if (NavigationService.Frame.Content is MasterDetailPage)
            {
                ContentDialog createNewFileDialog = new ContentDialog
                {
                    Title = "Create new instruction?",
                    Content = "Unsaved changes will be lost. Export your instruction before creating a new one.",
                    PrimaryButtonText = "Create new instruction",
                    CloseButtonText = "Cancel"
                };

                ContentDialogResult result = await createNewFileDialog.ShowAsync();
                
                if (result == ContentDialogResult.Primary)
                {
                    var page = NavigationService.Frame.Content as MasterDetailPage;
                    if (page != null)
                    {
                        page.Reset();
                    }
                }
            }
            else
            {
                NavigationService.Navigate<MasterDetailPage>();
            }
        }


        private async void ShellMenuItemClick_File_Load(object sender, RoutedEventArgs e)
        {
            if (NavigationService.Frame.Content is MasterDetailPage)
            {
                ContentDialog loadFileDialog = new ContentDialog
                {
                    Title = "Load instruction?",
                    Content = "Unsaved changes will be lost. Export your instruction before loading a new one.",
                    PrimaryButtonText = "Load instruction",
                    CloseButtonText = "Cancel"
                };

                ContentDialogResult result = await loadFileDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var page = NavigationService.Frame.Content as MasterDetailPage;
                    if (page != null)
                    {
                        var picker = new Windows.Storage.Pickers.FileOpenPicker();
                        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                        picker.FileTypeFilter.Add(".zip");

                        StorageFile file = await picker.PickSingleFileAsync();
                        if (file != null)
                        {
                            using (Stream zipToOpen = await file.OpenStreamForReadAsync())
                            {
                                await InstructionDataProvider.LoadDataFromZipAsync(zipToOpen, Path.Combine(ApplicationData.Current.TemporaryFolder.Path, file.DisplayName + ".save"));
                            }
                            page.Reset(false);
                        }
                        
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (NavigationService.Frame.Content is StartPage)
                {
                    var picker = new Windows.Storage.Pickers.FileOpenPicker();
                    picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                    picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                    picker.FileTypeFilter.Add(".zip");

                    StorageFile file = await picker.PickSingleFileAsync();
                    if (file != null)
                    {
                        var page = NavigationService.Frame.Content as StartPage;
                        page.loading = true;

                        using (Stream zipToOpen = await file.OpenStreamForReadAsync())
                        {
                            await InstructionDataProvider.LoadDataFromZipAsync(zipToOpen, Path.Combine(ApplicationData.Current.TemporaryFolder.Path, file.DisplayName + ".save"));
                        }
                        NavigationService.Navigate<MasterDetailPage>();
                    }
                }
            }
        }

        private void ShellMenuItemClick_File_Settings(object sender, RoutedEventArgs e)
        {
            MenuNavigationHelper.OpenInRightPane(typeof(SettingsPage));
        }
    }
}
