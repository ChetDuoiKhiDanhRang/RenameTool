﻿using System.Collections;
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
    public enum NameOrExtension
    {
        NameOnly,
        ExtensionOnly,
        NameAndExtension
    }

    public enum TextFormattings
    {
        None,
        LowerCase,
        UpperCase,
        TitleCase
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        int cou = 0;
        public int Cou { get => cou; set => cou = value; }



        private string searchPattern;
        public string SearchPattern
        {
            get { return searchPattern; }
            set
           {
                searchPattern = value;
                ValidateSearchPattern();
                OnPropertyChanged(nameof(SearchPattern));
            }
        }
        private void ValidateSearchPattern()
        {
            errors.Remove(nameof(SearchPattern));
            if (UseRegex)
            {
                try
                {
                    Regex reg = new Regex(SearchPattern);
                }
                catch (Exception e)
                {
                    errors[nameof(SearchPattern)] = new List<string> { $"error pattern: {e.Message}" };
                }
            }
            
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(SearchPattern)));
        }

        private string replaceWith;
        public string ReplaceWith
        {
            get { return replaceWith; }
            set { replaceWith = value; ValidateReplaceWith(); OnPropertyChanged(nameof(ReplaceWith)); }
        }
        private void ValidateReplaceWith()
        {
            errors.Remove(nameof(ReplaceWith));
            var illegalChar_name = System.IO.Path.GetInvalidFileNameChars();
            var illegalChar_path = System.IO.Path.GetInvalidPathChars();
            if (ReplaceWith.IndexOfAny(illegalChar_name) >= 0 || ReplaceWith.IndexOfAny(illegalChar_path) >= 0)
            {
                errors[nameof(ReplaceWith)] = new List<string> { $"illegal character!" };
            }
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(ReplaceWith)));
        }

        private bool useRegex;

        public bool UseRegex
        {
            get { return useRegex; }
            set { useRegex = value; ValidateSearchPattern(); OnPropertyChanged(nameof(UseRegex)); }
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

        private NameOrExtension targetPart;

        public NameOrExtension TargetPart
        {
            get { return targetPart; }
            set { targetPart = value; OnPropertyChanged(nameof(TargetPart)); }
        }

        private TextFormattings switchCases;

        public TextFormattings SwitchCases
        {
            get { return switchCases; }
            set { switchCases = value; OnPropertyChanged(nameof(SwitchCases)); }
        }

        private bool ignoreCase;

        public bool IgnoreCase
        {
            get { return ignoreCase; }
            set { ignoreCase = value; ValidateSearchPattern(); OnPropertyChanged(nameof(IgnoreCase)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public bool HasErrors => errors.Any();

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName) || (errors.Count == 0) || !errors.ContainsKey(propertyName))
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

        private void SaveSettings()
        {
            var x = Properties.Settings.Default;
            x.SearchPattern = SearchPattern;
            x.UseRegex = UseRegex;
            x.IgnoreCase = IgnoreCase;
            x.ReplaceWith = ReplaceWith;
            x.CaseSensitive = CaseSensitive;
            x.TargetPart = (int)TargetPart;
            x.SwitchCases = (int)SwitchCases;
            x.RemoveJunkSpace = RemoveJunkSpace;
            x.ToBaseASCII = ToBaseASCII;
            x.Save();
        }

        private void LoadSettings()
        {
            var x = Properties.Settings.Default;
            SearchPattern = x.SearchPattern;
            UseRegex = x.UseRegex;
            IgnoreCase = x.IgnoreCase;
            ReplaceWith = x.ReplaceWith;
            CaseSensitive = x.CaseSensitive;
            TargetPart = (NameOrExtension)x.TargetPart;
            RemoveJunkSpace = x.RemoveJunkSpace;
            ToBaseASCII = x.ToBaseASCII;
            SwitchCases = (TextFormattings)x.SwitchCases;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveSettings();
        }
    }
}