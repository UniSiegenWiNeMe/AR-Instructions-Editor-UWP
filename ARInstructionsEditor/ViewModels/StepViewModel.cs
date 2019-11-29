using ARInstructionsEditor.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ARInstructionsEditor.ViewModels
{
    public class StepViewModel : INotifyPropertyChanged
    {
        public static int MaxSteps { get; set; }

        private int _stepNumber;
        public int StepNumber
        {
            get { return _stepNumber + 1; }
            set
            {
                Set(ref _stepNumber, value);
                SetButtonAvailabilities();
                NumberAndText = StepNumber + ": " + _text;
            }
        }

        private void SetButtonAvailabilities()
        {
            PreviousStepAvaiable = _stepNumber > 0;
            NextStepAvaiable = _stepNumber + 1 < MaxSteps;
        }

        public List<Item> Items { get; set; }
        public ObservableCollection<MediaFileViewModel> Photos { get; set; }
        public void AddPhotoFile(MediaFileViewModel file)
        {
            Photos.Add(file);
            OnPropertyChanged("Photos");
        }

        public ObservableCollection<MediaFileViewModel> Videos { get; set; }

        internal void AddVideoFile(MediaFileViewModel file)
        {
            Videos.Add(file);
            OnPropertyChanged("Videos");
        }
        
        private bool _previousStepAvaiable;
        public bool PreviousStepAvaiable
        {
            get
            {
                return _previousStepAvaiable;
            }
            set
            {
                Set(ref _previousStepAvaiable, value);
            }
        }

        private bool _nextStepAvaiable;
        public bool NextStepAvaiable
        {
            get
            {
                return _nextStepAvaiable;
            }
            set
            {
                Set(ref _nextStepAvaiable, value);
            }
        }

        private string _text;

        public string Text
        {
            get { return _text; }
            set { Set(ref _text, value);
                NumberAndText = StepNumber + ": " + _text;
            }
        }

        private String _numberAndText;


        public string NumberAndText
        { 
            get { return _numberAndText; }
            set { Set(ref _numberAndText, value);}
        }

        public StepViewModel(int stepNumber)
        {
            Photos = new ObservableCollection<MediaFileViewModel>();
            Videos = new ObservableCollection<MediaFileViewModel>();
            Items = new List<Item>();
            this._stepNumber = stepNumber;
        }
        public StepViewModel(Step from)
        {
            //MaxSteps = maxSteps;
            StepNumber = from.StepNumber;
            Items = from.Items;
            Photos = new ObservableCollection<MediaFileViewModel>();
            Videos = new ObservableCollection<MediaFileViewModel>();

            if (from.MediaFiles != null)
            {
                foreach (var mediaFile in from.MediaFiles.Where(x => x.Type == MediaType.Image))
                {
                    Photos.Add(new MediaFileViewModel(mediaFile));
                }
                foreach (var mediaFile in from.MediaFiles.Where(x => x.Type == MediaType.Video))
                {
                    Videos.Add(new MediaFileViewModel(mediaFile));
                }
            }
            //from.MediaFiles;            //new List<MediaFile>();

            //if (from.MediaFiles != null)
            //{
            //    foreach (var mediafile in from.MediaFiles)
            //    {
            //        MediaFiles.Add(new MediaFileViewModel(mediafile));
            //    }
            //}

            Text = from.Text;
        }

        public StepViewModel(StepViewModel from)
        {
            //MaxSteps = from.MaxSteps;
            StepNumber = from.StepNumber-1;
            Items = from.Items;
            Photos = from.Photos;
            Text = from.Text;
        }
        public void RemoveStep()
        {
            OnPropertyChanged("RemoveItem", new PropertyChangedExtendedEventArgs<StepViewModel>("RemoveItem", this, null));
        }
        public void AddNewStep()
        {
            int newStepNumber = this.StepNumber;
            OnPropertyChanged("NewItem", new PropertyChangedExtendedEventArgs<StepViewModel>("NewItem", this, new StepViewModel(newStepNumber)));
        }
        public void DecreaseStepNumber()
        {
            Set(ref _stepNumber, _stepNumber - 1, "StepNumber");
            SetButtonAvailabilities();
        }

        public void IncreaseStepNumber()
        {
            Set(ref _stepNumber, _stepNumber + 1, "StepNumber");
            SetButtonAvailabilities();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            if (propertyName == "StepNumber")
            {

                T temp = storage;
                storage = value;
                NotifyPropertyChanged(propertyName, temp, value);
            }
            else
            {
                storage = value;
                OnPropertyChanged(propertyName);
            }
        }

        protected void NotifyPropertyChanged<T>(string propertyName, T oldvalue, T newvalue)
        {
            OnPropertyChanged(propertyName, new PropertyChangedExtendedEventArgs<T>(propertyName, oldvalue, newvalue));
        }

      

        private void OnPropertyChanged(string propertyName, PropertyChangedEventArgs eventArgs = null)
        {
            if (eventArgs != null)
            {
                PropertyChanged?.Invoke(this, eventArgs);
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void RemovePhotoFile(MediaFileViewModel photo)
        {
            Photos.Remove(photo);
            OnPropertyChanged("Photos");
        }
    }
}
