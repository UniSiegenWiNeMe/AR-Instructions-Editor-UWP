using ARInstructionsEditor.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace ARInstructionsEditor.ViewModels
{
    public class MediaFileViewModel: INotifyPropertyChanged
    {
        private string _fileName;
        public string FileName { get { return _fileName; } set { Set(ref _fileName, value); } }

        private MediaType _type;
        public MediaType Type { get { return _type; } set { Set(ref _type, value); } }

        public MediaFileViewModel()
        {

        }
        public MediaFileViewModel(MediaFile from)
        {
            FileName = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "media", from.FileName);
            Type = from.Type;
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
    }
}
