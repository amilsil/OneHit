using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using OneHit.DataAccess;
using OneHit.Model;
using OneHit.Util;
using OneHit.View;
using System.Text.RegularExpressions;

namespace OneHit.ViewModel
{
	public class ShortcutViewModel : ViewModelBase, IDisposable
    {
        private CategoryViewModel _categoryViewModel;

		public Shortcut Shortcut
		{
			get;
			private set;
		}

		#region Constructor

        public ShortcutViewModel(Shortcut shortcut, CategoryViewModel categoryViewModel = null)
        {
            Shortcut = shortcut;
            _categoryViewModel = categoryViewModel;
        }

        #endregion                

        #region Filtering Members

        private string _filterText;
        public string FilterText
        {
            private get
            {
                return _filterText;
            }
            set
            {
                _filterText = value;
                OnPropertyChanged("FilteredHeight");
            }
        }

        /// <summary>
        /// FilterHeight changes the visibility of a shortcut
        /// </summary>
        public int FilteredHeight
        {
            get
            {
                if (this.IsSelected)
                    return 30;
                else
                    return 0;
            }
        }

        public int QuickLaunchNumber
        {
            get;
            set;
        }

        public bool IsViewQuickLaunchNumber
        {
            get;
            set;
        }

        public System.Windows.Media.Brush TxtNumberBg
        {
            get
            {
                if (IsHighlighted)
                {
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                }
                else
                {
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                }
            }
        }
        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                _isHighlighted = value;
                OnPropertyChanged("TxtNumberBg");
            }
        }
        /// <summary>
        /// If this is selected after filtering
        /// </summary>
        public bool IsSelected
        {
            get
            {
                if (string.IsNullOrEmpty(FilterText))
                    return true;

                if (FilterText.Contains(">"))
                {
                    //foldersearch > shortcut search
                    string folderSearch = FilterText.Split('>')[0].Trim();
                    string shortcutSearch = FilterText.Split('>')[1].Trim();

                    //search for folders 
                    if (_SearchForWords(folderSearch, this.ParentLabel))
                    {
                        //search for shortcuts now
                        string sSearch = string.Format("{0} {1}", this.Label, this.Path);
                        if (_SearchForWords(shortcutSearch, sSearch))
                            return true;
                    }
                    
                }

                string ss = string.Format("{0} {1} {2}", this.ParentLabel, this.Label, this.Path);
                return _SearchForWords(FilterText, ss);
            }
        }

        private bool _SearchForWords(string words, string source)
        {
            string strReg = "";
            foreach (string word in words.Split(' ', ',', '-', '.'))
            {
                //strReg += Regex.Escape(word) + @"[^\b\s\.\-,]*[\b\s\.\-,]*";
                strReg += Regex.Escape(word) + @"(.*)?";
            }

            Regex regSearch = new Regex(strReg, RegexOptions.IgnoreCase);

            return regSearch.IsMatch(source);            
        }

        /// <summary>
        /// Show the quick launch number along with the shortcut
        /// this number is the quick launch hit number
        /// </summary>
        /// <param name="index"></param>
        public void RenderQuickLaunchNumber(int index)
        {
            QuickLaunchNumber = index;
            IsViewQuickLaunchNumber = true;

            OnPropertyChanged("QuickLaunchNumber");
        }

        #endregion

        #region Bound Public Properties
        /// <summary>
        /// Need to show params if available.
        /// If not, path
        /// </summary>
        public string PathOrParams
        {
            get {
                if (string.IsNullOrEmpty(this.Params))
                    return this.Path;
                else
                    return this.Params;
            }
        }

        public string Label
        {
            get { return Shortcut.Label; }
            set 
            { 
                Shortcut.Label = value;
                OnPropertyChanged("Label");
            }
        }

        public string Path
        {
            get { return Shortcut.Path; }
            set 
            { 
                Shortcut.Path = value;
                OnPropertyChanged("Path");
                //OnPropertyChanged("IconSource");
            }
        }

        public string Params
        {
            get { return Shortcut.Params; }
            set
            {
                Shortcut.Params = value;
                OnPropertyChanged("Params");
            }
        }

        public string ParentLabel
        {
            get 
            {
                return Shortcut.CategoryLabel;
            }
        }

        private BitmapSource _bitmapSource;
        public BitmapSource IconSource
        {
            get
            {
				//return null;
                if (_bitmapSource == null)
                {

                    _bitmapSource = IconHelper.GetProxyBitmapSource(Path);
                }

                return _bitmapSource;              
            }
        }

        #endregion
        
		#region Bound Commands

        private ICommand _openShortcutCommand;
        public ICommand OpenShortcutCommand
        {
            get
            {
                if (_openShortcutCommand == null)
                {
                    _openShortcutCommand = new RelayCommand(OpenShortcut) { IsEnabled = true };
                }
                return _openShortcutCommand;
            }
        }
        private ICommand _deleteShortcutCommand;
        public ICommand DeleteShortcutCommand
        {
            get
            {
                if (_deleteShortcutCommand == null)
                {
                    _deleteShortcutCommand = new RelayCommand(DeleteShortcut) { IsEnabled = true };
                }
                return _deleteShortcutCommand;
            }
        }

        private ICommand _shortcutEditCommand;
        public ICommand ShortcutEditCommand
        {
            get
            {
                if (_shortcutEditCommand == null)
                {
                    _shortcutEditCommand = new RelayCommand(ShortcutEdit) { IsEnabled = true };
                }
                return _shortcutEditCommand;
            }
        }

        private ICommand _copyPathCommand;
        public ICommand CopyPathCommand
        {
            get
            {
                if (_copyPathCommand == null)
                {
                    _copyPathCommand = new RelayCommand(CopyPath) { IsEnabled = true };
                }
                return _copyPathCommand;
            }
        }

        #endregion

        #region Action Handlers

        private void CopyPath()
        {
            System.Windows.Clipboard.SetText(Path);
        }

        private void DeleteShortcut()
        {
            _categoryViewModel.DeleteShortcut(this);
        }

        internal void OpenShortcut()
        {
            try
            {
				ProcessStartInfo pInfo = new ProcessStartInfo(Path, Params);
				// Set working directory to executable location.
				pInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(Path);
				// Use windows default Open action. 
				pInfo.UseShellExecute = true;

                Process.Start(pInfo);				
            }
            catch (Win32Exception)
            {
                System.Windows.MessageBox.Show(string.Format("File {0} could not be opened!", Path), "Oops!");
            }
        }

        private void ShortcutEdit()
        {
            ShortcutPropertiesViewModel vm = new ShortcutPropertiesViewModel()
            {
                Label = this.Label,
                Path = this.Path,
                Params = this.Params
            };
            
            ShortcutPropertiesView propDialog = new ShortcutPropertiesView();
            propDialog.DataContext = vm;

            propDialog.Owner = Application.Current.MainWindow;
            propDialog.ShowDialog();

            if (propDialog.OkPressed)
            {
                this.Label = vm.Label;
                this.Path = vm.Path;
                this.Params = vm.Params;
            }

            ShortcutRepository.Instance.SaveToDataFile();
        }

        #endregion

		public void SetShortcut(Shortcut shortcut, CategoryViewModel categoryvm)
		{
			this.Shortcut = shortcut;
			this._categoryViewModel = categoryvm;

			if (shortcut != null)
			{
			    OnPropertyChanged("Label");
			    OnPropertyChanged("Path");
			    OnPropertyChanged("Params");
			    OnPropertyChanged("IconSource");
			}
		}

		public new void Dispose()
		{
			this.Shortcut = null;
			this._categoryViewModel = null;
            this._bitmapSource = null;
		}
	}
}
