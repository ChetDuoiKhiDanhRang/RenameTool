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
    public class ViewItem : INotifyPropertyChanged, INotifyDataErrorInfo, IComparable<ViewItem>
    {
        internal MainWindow MyMainWindow { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(WillBeApply))
            {
                if (WillBeApply)
                {
                    MyMainWindow?.GenerateItemNewName(this);
                }
                else
                {
                    _Item.NewExtension = _Item.Extension;
                    NewName = Name;
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Item _Item { get; set; }

        public bool IsFile { get => _Item.IsFile; set { _Item.IsFile = value; OnPropertyChanged(nameof(IsFile)); } }
        public int RootLevel { get => _Item.RootLevel; set { _Item.RootLevel = value; OnPropertyChanged(nameof(RootLevel)); } }
        public int Level { get => (_Item.Level - _Item.RootLevel); set { _Item.Level = value + _Item.RootLevel; OnPropertyChanged(nameof(Level)); } }
        public bool WillBeApply { get => _Item.WillBeApply; set { _Item.WillBeApply = value; OnPropertyChanged(nameof(WillBeApply)); } }


        public string Name { get => _Item.Name; set { _Item.Name = value; OnPropertyChanged(nameof(Name)); } }
        public string Extension { get => _Item.Extension; set { _Item.Extension = value; OnPropertyChanged(nameof(Extension)); } }
        public string Location { get => _Item.Location; set { _Item.Location = value; OnPropertyChanged(nameof(Location)); } }

        public string NewName
        {
            get => _Item.NewName; set
            {
                if (_Item.NewName != value)
                {
                    _Item.NewName = value; ValidateNewName(); OnPropertyChanged(nameof(NewFullName)); 
                } } }

        public string NewExtension
        {
            get => _Item.NewExtension; set
            {
                if (_Item.NewExtension != value)
                {
                    _Item.NewExtension = value; ValidateNewName(); 
                    OnPropertyChanged(nameof(NewFullName)); 
                } } }

        public string FullName { get => _Item.FullName; } //set { OnPropertyChanged(nameof(FullName)); } }
        public string NewFullName { get => _Item.NewFullName; }

        public string FullPath { get => _Item.GetFullPath(); }

        public ViewItem(string FullPath)
        {
            _Item = new Item(FullPath);
        }

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
            if (NewFullName.IndexOfAny(illegalChars_filename) >= 0 || NewFullName.IndexOfAny(illegalChars_path) >= 0)
            {
                _errors[nameof(NewFullName)] = new List<string>() { $"Illegal characters: {illegalChars_filename}" };
            }
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(NewFullName)));
        }

        public int CompareTo(ViewItem? other)
        {
            if (this.IsFile == other.IsFile)
            {
                return this.FullPath.CompareTo(other.FullPath);
            }
            else if (this.IsFile && !other.IsFile)
            {
                if (other.FullPath.Contains(this.Location))
                {
                    return 1;
                }
            }
            else if (!this.IsFile && other.IsFile)
            {
                if (this.FullPath.Contains(other.Location))
                {
                    return -1;
                }
            }

            return 0;
        }
    }
}
