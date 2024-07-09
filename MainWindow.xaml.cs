using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RenameTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string searchPattern;
        public string SearchPattern
        {
            get { return searchPattern; }
            set { searchPattern = value;
                ValidateSearchPattern();
                OnPropertyChanged(nameof(SearchPattern));
            }
        }
        private void ValidateSearchPattern()
        {
            errors.Remove(nameof(SearchPattern));
            try
            {
                Regex reg = new Regex(searchPattern);
            }
            catch (Exception e)
            {
                errors[nameof(SearchPattern)] = new List<string> { $"Error pattern: {e.Message}" };
            }
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(SearchPattern)));
        }

        private string replaceTo;
        public string ReplaceTo
        {
            get { return replaceTo; }
            set { replaceTo = value; ValidateReplaceTo(); OnPropertyChanged(nameof(ReplaceTo)); }
        }
        private void ValidateReplaceTo()
        {
            errors.Remove($"{nameof(ReplaceTo)}");
            var illegalChar_name = System.IO.Path.GetInvalidFileNameChars();
            var illegalChar_path = System.IO.Path.GetInvalidPathChars();
            if (ReplaceTo.IndexOfAny(illegalChar_name)>=0 || ReplaceTo.IndexOfAny(illegalChar_path) >=0)
            {
                errors[nameof(ReplaceTo)] = new List<string> { $"Illegel chars: {illegalChar_name}" };
            }
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(ReplaceTo)));
        }

        private bool caseSensitive;

        public bool CaseSensitive
        {
            get { return caseSensitive; }
            set { caseSensitive = value; OnPropertyChanged(nameof(CaseSensitive)); }
        }

        private bool removeJunkSpace;
        public bool RemoveJunkSpace
        {
            get { return removeJunkSpace; }
            set { removeJunkSpace = value; OnPropertyChanged(nameof(RemoveJunkSpace)); }
        }

        private bool toBaseASCII;

        public bool ToBaseASCII
        {
            get { return toBaseASCII; }
            set { toBaseASCII = value; OnPropertyChanged(nameof(ToBaseASCII)); }
        }



        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public bool HasErrors => errors.Any();

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }
            return errors[propertyName];
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Dictionary<string, List<string>> errors { get; set; } = new Dictionary<string, List<string>>();
    }
}