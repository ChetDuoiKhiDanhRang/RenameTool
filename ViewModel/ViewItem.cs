using RenameTool.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace RenameTool.ViewModel
{
    internal class ViewItem : INotifyPropertyChanged, INotifyDataErrorInfo
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Item _Item { get; set; }
        public Item Parent { get => _Item.Parent; private set => _Item.Parent=value; }

        public string Name { get => _Item.Name; set { _Item.Name = value; OnPropertyChanged(nameof(Name)); } }    
        public string Extension { get => _Item.Extension; set { _Item.Extension = value; OnPropertyChanged(nameof(Extension)); } }    
        public string Location { get => _Item.Location; set { _Item.Location = value; OnPropertyChanged(nameof(Location)); } }    
        public string FullName { get => _Item.GetFullName(); } //set { OnPropertyChanged(nameof(FullName)); } }
        public string NewFullName { get => _Item.NewFullName; set { _Item.NewFullName = value; ValidateNewName(); OnPropertyChanged(nameof(FullName)); } }

        //Implement INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public bool HasErrors => _errors.Any();
        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_errors.TryGetValue(propertyName, out List<string>? _value)) return null;
            return _value;
        }
        //------------
        Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        private void ValidateNewName()
        {
            _errors.Remove(nameof(NewFullName));
            var illegalChars_filename = Path.GetInvalidFileNameChars();
            var illegalChars_path = Path.GetInvalidFileNameChars();
            if (NewFullName.IndexOfAny(illegalChars_filename)>=0|| NewFullName.IndexOfAny(illegalChars_path)>=0)
            {
                _errors[nameof(NewFullName)] = new List<string>() { $"Illegal characters: {illegalChars_filename}"};
            }
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(NewFullName)));
        }

    }
}
