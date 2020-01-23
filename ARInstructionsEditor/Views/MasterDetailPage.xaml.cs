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
        private Instruction _instruction;

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
            InstructionDataProvider.NameChanged += InstructionDataProvider_NameChanged;
        }

        private void InstructionDataProvider_NameChanged(object sender, EventArgs e)
        {
            InstructionName = InstructionDataProvider.Instruction.Name;
        }

        private void MasterDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            int currentSelectedIndex = -1;
            if (_selected != null)
            {
                currentSelectedIndex = _selected.StepNumber - 1;
            }

            StepItems.Clear();
            if (InstructionDataProvider.Instruction != null)
            {
                InstructionName = InstructionDataProvider.Instruction.Name;
            }
            else
            {
                InstructionDataProvider.Instruction = new Instruction();
                _instruction = InstructionDataProvider.Instruction;

                _instruction.Steps.Add(new Step() { StepNumber = 0, Items = new List<Item>() }); ;
            }

            StepViewModel.MaxSteps = InstructionDataProvider.Instruction.Steps.Count();
            foreach (var item in InstructionDataProvider.Instruction.Steps)
            {
                StepItems.Add(new StepViewModel(item));
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
            if(e.PropertyName == "NewItem")
            {
                var eventArgs = e as PropertyChangedExtendedEventArgs<StepViewModel>;

                if(eventArgs.OldValue.NextStepAvaiable)
                {
                    StepItems.Insert(eventArgs.OldValue.StepNumber, eventArgs.NewValue);
                    Selected = StepItems[eventArgs.OldValue.StepNumber];
                }
                else
                {
                    StepItems.Add(eventArgs.NewValue);
                    Selected = StepItems[StepItems.Count -1];
                }
                StepViewModel.MaxSteps = StepItems.Count;
                for (int i = 0; i < StepItems.Count(); i++)
                {
                    StepItems[i].StepNumber = i;
                }
                

            }
            if (e.PropertyName == "RemoveItem")
            {
                var tmp = sender as StepViewModel;
                
                StepItems.Remove(sender as StepViewModel);
                StepViewModel.MaxSteps = StepItems.Count;
                for (int i = 0; i < StepItems.Count(); i++)
                {
                    StepItems[i].StepNumber = i;
                }


                if (tmp.StepNumber == 1)
                {
                    Selected = StepItems[0];
                }
                else
                {
                    Selected = StepItems[tmp.StepNumber - 2];
                }
            }
            InstructionDataProvider.UpdateData(ConvertToListofSteps(StepItems));
        }

        private List<Step> ConvertToListofSteps(ObservableCollection<StepViewModel> stepItems)
        {
            var ret = new List<Step>();

            foreach (var stepVM in stepItems)
            {
                var tmpMediaFiles = new List<MediaFile>();
                foreach (var file in stepVM.Photos)
                {
                    tmpMediaFiles.Add(new MediaFile()
                    {
                        FileName = file.FileName.Split(@"\").Last(),
                        Type = file.Type
                    });
                }
                foreach (var file in stepVM.Videos)
                {
                    tmpMediaFiles.Add(new MediaFile()
                    {
                        FileName = file.FileName.Split(@"\").Last(),
                        Type = file.Type
                    });
                }

                ret.Add(new Step()
                {
                    StepNumber = stepVM.StepNumber - 1,
                    Items = stepVM.Items,
                    MediaFiles = tmpMediaFiles,
                    Text = stepVM.Text
                });
            }
            return ret;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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

            storage = value;

            if (storage != null)
            {
                storage.PropertyChanged += Selected_PropertyChanged;
            }
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// resets the page
        /// </summary>
        /// <param name="createNew">if true, a new instruction will be instanitated</param>
        internal void Reset(bool createNew = true)
        {
            StepItems.Clear();

            if (createNew)
            {
                InstructionDataProvider.Instruction = new Instruction();
                InstructionDataProvider.Instruction.Steps.Add(new Step() { StepNumber = 0, Items = new List<Item>() });
            }
            _instruction = InstructionDataProvider.Instruction;
            InstructionName = _instruction.Name;
            StepViewModel.MaxSteps = InstructionDataProvider.Instruction.Steps.Count();
            foreach (var item in InstructionDataProvider.Instruction.Steps)
            {
                StepItems.Add(new StepViewModel(item));
            }

            if (MasterDetailsViewControl.ViewState == MasterDetailsViewState.Both)
            {
                Selected = StepItems.FirstOrDefault();
            }
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
    }

    
}
