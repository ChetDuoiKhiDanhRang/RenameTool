using RenameTool.ViewModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using BaseTools;
using System.Reflection;
using System.Security.Principal;
using System.Collections.Concurrent;

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
            DroppedItems = new List<ViewItem>();
            //DroppedItems.CollectionChanged += DroppedItems_CollectionChanged;
            DeclareItems = new List<ViewItem>();
            string info = "RenameTool ver" + Assembly.GetExecutingAssembly().GetName().Version;
            info += "; Framework: " + AppContext.TargetFrameworkName;
            info += "; Running as: " + Environment.UserName;
            lblInfo.Text = info;
        }

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

        private List<ViewItem> droppedItems;
        public List<ViewItem> DroppedItems { get => droppedItems; set { droppedItems = value; OnPropertyChanged(nameof(DroppedItems)); } }

        private void DroppedItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(DroppedItems));
        }

        private List<ViewItem> declareItems;
        public List<ViewItem> DeclareItems
        {
            get { return declareItems; }
            set
            {
                declareItems = value;
                ClickCount++;
                OnPropertyChanged(nameof(DeclareItems));
            }
        }

        public int ClickCount { get; set; } = 0;

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

        private ViewItem selectedItem;

        public ViewItem SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; OnPropertyChanged(nameof(SelectedItem)); }
        }


        private int maxLevel;
        public async void OnPropertyChanged(string propertyName)
        {
            if ((propertyName == nameof(DroppedItems) || propertyName == nameof(DeclareItems) || propertyName == nameof(IncludeChildItems)) && DeclareItems != null)
            {
                DeclareItems?.Clear();
                lsvItems.ItemsSource = null;
                maxLevel = 0;
                foreach (var item in DroppedItems)
                {
                    item.MyMainWindow = this;
                    DeclareItems.Add(item);
                    var tmpList = new ConcurrentBag<ViewItem>();
                    if (IncludeChildItems && !item.IsFile)
                    {
                        await TraversePath(item.FullPath, tmpList, item.Level + item.RootLevel, item.OrderString);
                        DeclareItems.AddRange(tmpList);
                    }
                }

                tbkFilesCount.Dispatcher.Invoke(() =>
                {
                    tbkFilesCount.Text = DeclareItems.Where(x => x.IsFile).Count().ToString();

                });

                tbkFoldersCount.Dispatcher.Invoke(() =>
                {
                    tbkFoldersCount.Text = DeclareItems.Where(x => !x.IsFile).Count().ToString();
                });
                Task.Run(() =>
                {
                    DeclareItems?.Sort();
                    lsvItems.Dispatcher.Invoke(() => { lsvItems.ItemsSource = DeclareItems; });
                });
            }

            if (!HasErrors)
            {
                GenerateNewName();
            }


            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void GenerateNewName()
        {
            if (DeclareItems == null || DeclareItems.Count == 0) { return; }
            Parallel.ForEach<ViewItem>(DeclareItems, (GenerateItemNewName));
        }

        internal void GenerateItemNewName(ViewItem item)
        {
            if (!item.WillBeApply) return;
            string target = "";

            item.NewName = item.Name;
            item.NewExtension = item.Extension;

            switch (TargetPart)
            {
                case NameOrExtension.NameOnly:
                    target = item.Name;
                    break;
                case NameOrExtension.ExtensionOnly:
                    target = item.Extension;
                    break;
                case NameOrExtension.NameAndExtension:
                    target = item.FullName;
                    break;
                default:
                    target = item.Name;
                    break;
            }
            switch (SwitchCases)
            {
                case TextFormattings.None:
                    break;
                case TextFormattings.LowerCase:
                    target = target.ToLower();
                    break;
                case TextFormattings.UpperCase:
                    target = target.ToUpper();
                    break;
                case TextFormattings.TitleCase:
                    target = StringHandler.Proper(target);
                    break;
                default:
                    break;
            }

            if (ToBaseASCII)
            {
                target = StringHandler.BaseASCII(target);
            }

            if (RemoveJunkSpace)
            {
                target = StringHandler.RemoveJunkSpaces(target);
            }

            if (SearchPattern != "" && !HasErrors)
            {
                if (UseRegex)
                {
                    if (SearchPattern == @"$")
                    {
                        target = target + ReplaceWith;
                    }
                    else if (SearchPattern == @"^")
                    {
                        target = ReplaceWith + target;
                    }
                    else
                    {
                        Regex reg = new Regex(SearchPattern, IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
                        foreach (Match match in reg.Matches(target))
                        {
                            if (match.Value != "") target = target.Replace(match.Value, ReplaceWith);
                        }
                    }
                }
                else { target = target.Replace(SearchPattern, ReplaceWith); }
            }

            switch (TargetPart)
            {
                case NameOrExtension.NameOnly:
                    item.NewName = target;
                    break;
                case NameOrExtension.ExtensionOnly:
                    if (item.IsFile)
                    {
                        item.NewExtension = target;
                    }
                    break;
                case NameOrExtension.NameAndExtension:
                    if (item.IsFile)
                    {
                        item.NewExtension = target.LastIndexOf(".") < 0 ? "" : target.Substring(target.LastIndexOf('.'));
                        item.NewName = (target.LastIndexOf(".") < 0 ? target : target.Substring(0, target.LastIndexOf(".")));
                    }
                    else
                    {
                        item.NewName = target;
                    }
                    break;
                default:
                    break;
            }

        }

        private async Task TraversePath(string fullPath, ConcurrentBag<ViewItem> declareItems, int rootLevel, string parentOrderString)
        {
            var files = Directory.EnumerateFiles(fullPath, "*", SearchOption.TopDirectoryOnly).ToList();
            var subfolders = Directory.EnumerateDirectories(fullPath, "*", SearchOption.TopDirectoryOnly).ToList();

            files.Sort();
            subfolders.Sort();

            int fi_count = 0;
            int fo_count = 0;

            foreach (var file in files)
            {
                fi_count++;
                declareItems.Add(new ViewItem(file) { RootLevel = rootLevel, MyMainWindow = this, OrderString = parentOrderString + ".fi" + fi_count });
            }

            var tasks = new List<Task>();
            foreach (var subfolder in subfolders)
            {
                fo_count++;
                tasks.Add(Task.Factory.StartNew(async () =>
                {
                    var folderItem = new ViewItem(subfolder) { RootLevel = rootLevel, MyMainWindow = this, OrderString = parentOrderString + ".fo" + fo_count };
                    maxLevel = folderItem.Level > maxLevel ? folderItem.Level : maxLevel;
                    declareItems.Add(folderItem);
                    await TraversePath(subfolder, declareItems, rootLevel, folderItem.OrderString);
                }));
            }
            Task.WaitAll(tasks.ToArray());
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
            SearchPattern = (x.SearchPattern == "\r\n                ") ? "" : x.SearchPattern;
            UseRegex = x.UseRegex;
            IgnoreCase = x.IgnoreCase;
            IncludeChildItems = x.IncludeChildItems;
            ReplaceWith = (x.ReplaceWith == "\r\n                ") ? "" : x.ReplaceWith;
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

        string logs = "";
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            logs = "";
            Parallel.ForEach<ViewItem>(DeclareItems.Where(x => x.IsFile && x.WillBeApply), new Action<ViewItem>(RenameFileItem));
            for (int i = maxLevel; i >= 0; i--)
            {
                var filterList = DeclareItems.Where(x => !x.IsFile && x.WillBeApply && x.Level == i).ToList();
                List<Task> tasks = new List<Task>();
                foreach (var folder in filterList)
                {
                    tasks.Add(Task.Factory.StartNew(new Action<object?>(RenameFolder), folder));
                }
                Task.WaitAll(tasks.ToArray());
            }
            OnPropertyChanged(nameof(DroppedItems));

            File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "ErrorLog.txt"), logs);
        }

        private void RenameFolder(object? item)
        {
            if (item == null) return;
            var folder = item as ViewItem; if (folder == null) return;
            try
            {
                if (folder.NewFullName != folder.FullName)
                {
                    Directory.Move(folder.FullPath, Path.Combine(folder.Location, folder.NewName));
                    folder.Name = folder.NewName;
                }
            }
            catch (Exception e)
            {
                logs += e.ToString();
            }
        }

        private void RenameFileItem(ViewItem item)
        {
            try
            {
                if (item.NewFullName != item.FullName)
                {
                    File.Move(item.FullPath, Path.Combine(item.Location, item.NewFullName));
                    item.Name = item.NewName;
                    item.Extension = item.NewExtension;
                }
            }
            catch (Exception e)
            {
                logs += e.Message;
            }
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
                    ViewItem i = new ViewItem(item) { };
                    if (i.IsFile)
                    {

                        listFiles.Add(i);
                    }
                    else
                    {
                        listFolders.Add(i);
                    }
                }
                listFiles.Sort();
                listFolders.Sort();
                int fi_count = 0;
                int fo_count = 0;
                foreach (var item in listFiles)
                {
                    fi_count++;
                    item.OrderString = "fi" + fi_count;
                    DroppedItems.Add(item);
                }
                foreach (var item in listFolders)
                {
                    fi_count++;
                    item.OrderString = "fo" + fi_count;
                    DroppedItems.Add(item);
                }
                OnPropertyChanged(nameof(DroppedItems));
            }
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;

        }



        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Parallel.ForEach<ViewItem>(DeclareItems, item => { item.WillBeApply = true; });
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Parallel.ForEach<ViewItem>(DeclareItems, item => { item.WillBeApply = false; });

        }

        private void ckbIntegrade2ContextMenu_Checked(object sender, RoutedEventArgs e)
        {
            string add2File = @"*\shell\Cxx-RenameTool";
            string add2Directory = @"Directory\shell\Cxx-RenameTool";
            string add2BackgroundContextMenu = @"Directory\background\shell\Cxx-RenameTool";

            string appPath = Path.Combine(AppContext.BaseDirectory, "RenameTool.exe");
        }
    }
}