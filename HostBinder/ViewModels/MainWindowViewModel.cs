using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using HostBinder.Annotations;
using HostBinder.Commands;
using HostBinder.Helpers;
using HostBinder.Models;

namespace HostBinder.ViewModels
{
    public class MainWindowViewModel:INotifyPropertyChanged
    {
        #region Private Members
        private string _hostsFilePath;
        private string _hostsFileName;
        private string _hostsFileContents;

        private string _searchText;

        private ObservableCollection<HostItem> _hostItems;
        private ObservableCollection<HostItem> _filteredHostItems;
        private HostItem _selectedHostItem;

        private ObservableCollection<TargetAddress> _targets;
        private string[] _targetTags;
        private string[] _itemTags;
        private string _selectedItemTag;
        private string _selectedTargetTag;


        private System.IO.FileSystemWatcher _hostsWatcher;
        private BrowserInfo[] _browsers;

        #endregion

        #region Properties

        public string SearchText { get { return _searchText; } set { UpdateSearchText(value); OnPropertyChanged(); }}

        public string HostsFileContents { get { return _hostsFileContents; } set { _hostsFileContents = value; OnPropertyChanged(); } }

        public ObservableCollection<TargetAddress> Targets { get { return _targets; } set { _targets = value; OnPropertyChanged(); } }
        public ObservableCollection<HostItem> HostItems { get { return _hostItems; } set { _hostItems = value; OnPropertyChanged(); } }
        public ObservableCollection<HostItem> FilteredHostItems { get { return _filteredHostItems; } set { _filteredHostItems = value; OnPropertyChanged(); } }

        public HostItem SelectedHostItem { get { return _selectedHostItem; } set { _selectedHostItem = value; OnPropertyChanged(); } }

        public string[] TargetTags
        {
            get { return _targetTags; }
            set
            {
                if (Equals(value, _targetTags)) return;
                _targetTags = value;
                OnPropertyChanged();
            }
        }

        public string[] ItemTags
        {
            get { return _itemTags; }
            set
            {
                if (Equals(value, _itemTags)) return;
                _itemTags = value;
                OnPropertyChanged();
            }
        }

        public string SelectedTargetTag
        {
            get { return _selectedTargetTag; }
            set
            {
                if (value == _selectedTargetTag) return;
                _selectedTargetTag = value;
                OnPropertyChanged();
            }
        }

        public string SelectedItemTag
        {
            get { return _selectedItemTag; }
            set
            {
                if (value == _selectedItemTag) return;
                _selectedItemTag = value;
                OnPropertyChanged();
            }
        }

        public BrowserInfo[] Browsers
        {
            get { return _browsers; }
            set { 
                _browsers = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands
        
        public ICommand SelectedHostItemBindingChanged { get; private set; }
        public ICommand ClearSelectedTargetTag { get; private set; }
        public ICommand LaunchBrowser { get; private set; }
        
        #endregion

        #region Initialization

        public MainWindowViewModel()
        {
            _searchText = string.Empty;
            _hostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc");
            _hostsFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
            _hostItems = new ObservableCollection<HostItem>();
            _filteredHostItems = new ObservableCollection<HostItem>();
            _hostItems.CollectionChanged += _hostItems_CollectionChanged;
            _targets = new ObservableCollection<TargetAddress>();

            Browsers = BrowserHelper.GetInstalledBrowsers();


            SelectedHostItemBindingChanged = new SelectedHostItemBindingChangedCommand(this);
            ClearSelectedTargetTag = new ClearSelectedTargetTagCommand(this);
            LaunchBrowser = new LaunchBrowserCommand(this);

            LoadHostsFile();
            ParseHostFileContents();

            PropertyChanged += MainWindowViewModel_PropertyChanged;

            _hostsWatcher = new FileSystemWatcher();
            _hostsWatcher.Path = _hostsFilePath;
            _hostsWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _hostsWatcher.Changed += _hostsWatcher_Changed;
            _hostsWatcher.EnableRaisingEvents = true;
        }

        void _hostsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            System.Threading.Thread.Sleep(300);            
            App.Current.Dispatcher.Invoke(LoadHostsFile);
            //Dispatcher.CurrentDispatcher.Invoke(LoadHostsFile);
        }

        void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "HostsFileContents" : DoHostFileContentsChanged(); break;
                case "SelectedTargetTag": UpdateFilteredHostItems(); break;
            }

        }

        private void DoHostFileContentsChanged()
        {
            SaveHostsFile();
            ParseHostFileContents();
        }

        #endregion

        void _hostItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateFilteredHostItems();
        }

        private void SaveHostsFile()
        {
            _hostsWatcher.EnableRaisingEvents = false;
            File.WriteAllText(_hostsFileName, _hostsFileContents);
            _hostsWatcher.EnableRaisingEvents = true;
        }

        #region Parsing
        private void ParseHostFileContents()
        {
            ClearData();
            
            var lines = ParseTextHelper.GetTextLines(HostsFileContents);
            for (int i = 0; i < lines.Length; i++)
            {
                ParseHostsLine(i, lines[i]);
            }

            DiscoverTags();
        }

        private void DiscoverTags()
        {
            //get all the tags of the target
            TargetTags = Targets.SelectMany(t => t.Tags).Distinct().ToArray();
            ItemTags =
                HostItems.Join(Targets, item => item.Address, target => target.Address, (item, target) => target)
                         .SelectMany(t => t.Tags)
                         .Distinct()
                         .ToArray();

        }

        private void ParseHostsLine(int lineNumber, string line)
        {
            var match = Regex.Match(line, Constants.HostLineRegEx);
            if (match.Success)
            {
                var address = match.Groups[1].Value;
                var name = match.Groups[2].Value;
                var comment = match.Groups[3].Value.Trim();
                var favorite = (comment.StartsWith(Constants.FavoriteTag));
                if (favorite) comment=comment.Substring(Constants.FavoriteTag.Length);

                if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(name))
                { //just parse the comment

                    if (comment.StartsWith(Constants.TargetTag))
                    {
                        comment = comment.Substring(Constants.TargetTag.Length);
                        ParseTarget(lineNumber, comment);
                    }

                }
                else
                { //add this to the list
                    HostItems.Add(new HostItem() { Address = address, Name = name, Comment = comment, Favorite = favorite, LineNumber = lineNumber});
                }

            }

        }

        private void ParseTarget(int lineNumber, string targetText)
        {
            var targetComment = targetText.Split(Constants.CommentDelimiter);
            string comment = null;
            if (targetComment.Length > 1) comment = targetComment[1];
            var targetParams = targetComment[0].Split(Constants.CommandDelimiter);
            if (targetParams.Length >= 2)
            {
                var target = new TargetAddress() {Name = targetParams[0], Address = targetParams[1], Comment = comment, LineNumber = lineNumber};
                if (targetParams.Length >= 3) target.Tags = targetParams[2].Split(',');
                Targets.Add(target);
            }
        }

        private void ClearData()
        {
            HostItems.Clear();
            Targets.Clear();
        }
        #endregion

        private void LoadHostsFile()
        {
            if (File.Exists(_hostsFileName)) HostsFileContents = File.ReadAllText(_hostsFileName);
            else HostsFileContents = null;
        }

        #region Searching
        public void UpdateFilteredHostItems()
        {

            if (FilteredHostItems != null && HostItems != null)
            {
                //save the selected item, if possible
                var oldSelectedName = SelectedHostItem != null ? SelectedHostItem.Name : string.Empty;

                FilteredHostItems.Clear();

                //first, join ALL Host Items to ANY Matching targes (could result in duplicates)
                var list =
                    from item in HostItems
                    from target in Targets.Where(t => t.Address == item.Address).DefaultIfEmpty() //default if emtpy plus the where clause accomplishes a left join
                    select new {item, target};

                //now filter the list to just the items that match the search text
                var items = list.Where(o => o.item.Name.ToUpper().Contains(SearchText.ToUpper())).ToList();

                //now filter the list to just the items that intersect with the selected target tags list
                if (SelectedTargetTag!=null)
                    items = items.Where(o=>o.target!=null && o.target.Tags.Contains(SelectedTargetTag)).ToList();

                foreach (var o in items)
                {
                    if (!FilteredHostItems.Contains(o.item)) FilteredHostItems.Add(o.item);
                }

                //if the old selected item is still in the list, re-select it
                SelectedHostItem = FilteredHostItems.FirstOrDefault(i => i.Name == oldSelectedName);

                //if the selected item is null, try to find something to select
                //if (SelectedHostItem == null) SelectedHostItem = FilteredHostItems.FirstOrDefault();
            }
            
        }

        public void UpdateSearchText(string value)
        {
            _searchText = value;
            UpdateFilteredHostItems();
        }
        #endregion


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
