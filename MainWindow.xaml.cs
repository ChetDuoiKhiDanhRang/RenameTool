using RenameTool.ViewModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

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
            DroppedItems = new ObservableCollection<ViewItem>();
            DroppedItems.CollectionChanged += DroppedItems_CollectionChanged;
            DeclareItems = new ObservableCollection<ViewItem>();
        }



        int cou = 0;
        public int Cou { get => cou; set => cou = value; }

        public List<string> ListTextFormattings
        {
            get
            {
                return new List<string> { "Keep original", "lower case", "UPPER CASE", "Title Case" };
            }
        }

        public List<string> ListTargetParts
        {
            get
            {
                return new List<string> { "Name", "Name + Extension", "Extension" };
            }
        }


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
                    errors[nameof(SearchPattern)] = new List<string> { $"{e.Message}" };
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

        private bool includeChildItems;

        public bool IncludeChildItems
        {
            get { return includeChildItems; }
            set { includeChildItems = value; OnPropertyChanged(nameof(IncludeChildItems)); }
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

        private ObservableCollection<ViewItem> droppedItems;
        public ObservableCollection<ViewItem> DroppedItems { get => droppedItems; set { droppedItems = value; OnPropertyChanged(nameof(DroppedItems)); } }

        private void DroppedItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(DroppedItems));
        }

        private ObservableCollection<ViewItem> declareItems;
        public ObservableCollection<ViewItem> DeclareItems
        {
            get { return declareItems; }
            set { declareItems = value; OnPropertyChanged(nameof(DeclareItems)); }
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
            if (propertyName == nameof(DroppedItems) || propertyName == nameof(DroppedItems) || propertyName == nameof(IncludeChildItems))
            {
                Cou++;
                DeclareItems?.Clear();
                foreach (var item in DroppedItems)
                {
                    DeclareItems.Add(new ViewItem(item.FullPath));
                    if (IncludeChildItems && !item.IsFile)
                    {
                        List<ViewItem> list = new List<ViewItem>();
                        var liF = Directory.EnumerateFiles(item.FullPath, "*", SearchOption.AllDirectories).ToList<string>();
                        liF.Sort();
                        var liD = Directory.EnumerateDirectories(item.FullPath, "*", SearchOption.AllDirectories).ToList();
                        liD.Sort();
                        foreach (var i in liF) { list.Add(new ViewItem(i) { RootLevel = item.Level + item.RootLevel }); }
                        foreach (var i in liD) { list.Add(new ViewItem(i) { RootLevel = item.Level + item.RootLevel }); }
                        list.Sort();
                        foreach (var i in list)
                        {
                            DeclareItems.Add((ViewItem)i);
                        }
                    }
                }
            }

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
            x.RemoveJunkSpace = RemoveJunkSpace;
            x.ToBaseASCII = ToBaseASCII;
            x.IncludeChildItems = IncludeChildItems;
            x.Save();
        }

        private void LoadSettings()
        {
            var x = Properties.Settings.Default;
            SearchPattern = x.SearchPattern;
            UseRegex = x.UseRegex;
            IgnoreCase = x.IgnoreCase;
            IncludeChildItems = x.IncludeChildItems;
            ReplaceWith = x.ReplaceWith;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var item = new ViewItem(@"D:\Z.ItemsTest");
            DroppedItems.Add(item);
        }

        private void Window_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            var x = e.Data.GetData(DataFormats.FileDrop);
            if (x != null)
            {
                DroppedItems.Clear();
                List<ViewItem> listFiles = new List<ViewItem>();
                List<ViewItem> listFolders = new List<ViewItem>();
                foreach (var item in x as IEnumerable<string>)
                {
                    ViewItem i = new ViewItem(item);
                    if (i.IsFile)
                    {
                        listFiles.Add(i);
                    }
                    else { listFolders.Add(i); }
                }
                listFiles.Sort();
                listFolders.Sort();
                foreach (var item in listFiles)
                {
                    DroppedItems.Add((ViewItem)item);
                }
                foreach (var item in listFolders)
                {
                    DroppedItems.Add((ViewItem)item);
                }
            }
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;

        }
    }
}