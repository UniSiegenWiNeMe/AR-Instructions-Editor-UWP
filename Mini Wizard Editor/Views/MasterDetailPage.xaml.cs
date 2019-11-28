using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Toolkit.Uwp.UI.Controls;

using ARInstructionsEditor.Core.Models;
using ARInstructionsEditor.Core.Services;
using ARInstructionsEditor.ViewModels;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ARInstructionsEditor.Views
{
    public sealed partial class MasterDetailPage : Page, INotifyPropertyChanged
    {
        private string _instructionName;
        public string InstructionName
        {
            get { return _instructionName; }
            set
            {
                if (_instructionName != value)
                {
                    _instructionName = value;
                    OnPropertyChanged("InstructionName");
                }
            }
        }
        private StepViewModel _selected;
        public StepViewModel Selected
        {
            get { return _selected; }
            set
            {
                
                Set(ref _selected, value);
            }
        }

        public ObservableCollection<StepViewModel> StepItems { get; private set; } = new ObservableCollection<StepViewModel>();

        public MasterDetailPage()
        {
            InitializeComponent();
            Loaded += MasterDetailPage_Loaded;
        }

        private void MasterDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            int currentSelectedIndex = -1;
            if (_selected != null)
            {
                currentSelectedIndex = _selected.StepNumber - 1;
            }

            StepItems.Clear();
            var data = InstructionDataProvider.Instruction.Steps;
            InstructionName = InstructionDataProvider.Instruction.Name;
            foreach (var item in data)
            {
                StepItems.Add(new StepViewModel(item, data.Count()));
            }

            if (MasterDetailsViewControl.ViewState == MasterDetailsViewState.Both)
            {
                if (currentSelectedIndex == -1)
                {
                    Selected = StepItems.FirstOrDefault();
                }
                else
                {
                    Selected = StepItems[currentSelectedIndex];
                }
            }
        }

        private void Selected_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StepNumber")
            {
                var eventArgs = e as PropertyChangedExtendedEventArgs<int>;

                var tmp = new StepViewModel(StepItems[eventArgs.OldValue]);
                StepItems.RemoveAt(eventArgs.OldValue);
                StepItems.Insert(eventArgs.NewValue, tmp);

                for (int i = 0; i < StepItems.Count(); i++)
                {
                    StepItems[i].StepNumber = i;
                }

                Selected = StepItems[eventArgs.NewValue];
            }
            InstructionDataProvider.UpdateData(ConvertToListofSteps(StepItems));
        }

        private List<Step> ConvertToListofSteps(ObservableCollection<StepViewModel> stepItems)
        {
            var ret = new List<Step>();

            foreach (var step in stepItems)
            {
                var tmpMediaFiles = new List<MediaFile>();
                foreach (var file in step.Photos)
                {
                    tmpMediaFiles.Add(new MediaFile()
                    {
                        FileName = file.FileName.Split(@"\").Last(),
                        Type = file.Type
                    });
                }

                ret.Add(new Step()
                {
                    StepNumber = step.StepNumber - 1,
                    Items = step.Items,
                    MediaFiles = tmpMediaFiles,
                    Text = step.Text
                });
            }
            return ret;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Source.Clear();

            //// TODO WTS: Replace this with your actual data
            //var data = await InstructionDataProvider.GetImagesByStepNumber(currentStepID); /*SampleDataService.GetImageGalleryDataAsync("ms-appx:///Assets");*/

            //foreach (var item in data)
            //{
            //    Source.Add(mediaFileToMediaFileViewModel(item));
            //}


            //if (!string.IsNullOrEmpty(selectedImageID) && e.NavigationMode == NavigationMode.New)
            //{
            //    SelectedImage = Source.FirstOrDefault(i => i.FileName == selectedImageID);
            //}
            //else
            //{
            //    selectedImageID = ImagesNavigationHelper.GetImageId(MasterDetailDetailControl.ImageGallerySelectedIdKey);
            //    if (!string.IsNullOrEmpty(selectedImageID))
            //    {
            //        SelectedImage = Source.FirstOrDefault(i => i.FileName == selectedImageID);
            //    }
            //}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set(ref StepViewModel storage, StepViewModel value, [CallerMemberName]string propertyName = null)
        {
            
            if (Equals(storage, value))
            {
                return;
            }
            if (_selected != null)
            {
                storage.PropertyChanged -= Selected_PropertyChanged;
            }

            storage = value; /*!= null ? new StepViewModel(value) : null;*/

            if (storage != null)
            {
                storage.PropertyChanged += Selected_PropertyChanged;
            }
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
    }

    
}
