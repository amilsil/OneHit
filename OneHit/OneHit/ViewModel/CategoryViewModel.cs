using System.Collections.ObjectModel;
using OneHit.Model;
using System.Windows.Input;
using OneHit.Util;
using OneHit.DataAccess;
using OneHit.View;
using System.Windows;
using System.IO;
using System.Windows.Media;
using System;
using System.Windows.Media.Imaging;

namespace OneHit.ViewModel
{
    public class CategoryViewModel : ViewModelBase
    {
        private MainWindowViewModel _mainWindowViewModel;
        private ObservableCollection<ShortcutViewModel> _shortcutViewModels;

        #region Constructor

        internal CategoryViewModel(Category category, MainWindowViewModel mainWindowViewModel)
        {
            Category = category;
            _mainWindowViewModel = mainWindowViewModel;

            _shortcutViewModels = new ObservableCollection<ShortcutViewModel>();
            foreach (Shortcut shortcut in Category.Shortcuts)
            {
                _shortcutViewModels.Add(new ShortcutViewModel(shortcut, this));
            }
        }

        #endregion

        #region Bound Properties

        public BitmapSource CollapseExpandImage
        {
            get
            {
                Uri uri;
                if (IsFolded)
                    uri = new Uri("pack://application:,,,/Resources/plus.png");                    
                else
                    uri = new Uri("pack://application:,,,/Resources/minus.png");

                BitmapImage bi = new BitmapImage(uri);

                return bi;
            }
        }

        // Extracted or Folded
        internal bool IsFolded
        {
            get { return Category.IsFolded; }
            set { 
                this.Category.IsFolded = value;
                OnPropertyChanged("CollapseExpandImage");
            }
        }

        public string Label
        {
            get { return Category.Label; }
            private set 
            { 
                Category.Label = value;
                OnPropertyChanged("Label");
            }
        }

        internal Category Category
        {
            get;
            private set;
        }

        /// <summary>
        /// Used to determine if the "Save as Template" menu should be enabled.
        /// </summary>
        public bool IsTemplateSavingEnabled
        {
            get
            {
                // if you have the priviledge to write to the remote share, that's enough.
                //return ShortcutRepository.Instance.HasSavePriviledgesForTemplates;
                return false;
            }
        }

        public ObservableCollection<ShortcutViewModel> ShortcutViewModels
        {
            get
            {
                return _shortcutViewModels;
            }
        }

        #endregion

        #region Bound Commands

        private ICommand _addShortcutCommand;
        public ICommand AddShortcutCommand
        {
            get
            {
                if (_addShortcutCommand == null)
                {
                    _addShortcutCommand = new RelayCommand(AddShortcut) { IsEnabled = true };
                }
                return _addShortcutCommand;
            }
        }

        private ICommand _deleteFolderCommand;
        public ICommand DeleteFolderCommand
        {
            get
            {
                if (_deleteFolderCommand == null)
                {
                    _deleteFolderCommand = new RelayCommand(DeleteFolder) { IsEnabled = true };
                }
                return _deleteFolderCommand;
            }
        }

        private ICommand _folderEditCommand;
        public ICommand FolderEditCommand
        {
            get
            {
                if (_folderEditCommand == null)
                {
                    _folderEditCommand = new RelayCommand(EditFolder) { IsEnabled = true };
                }
                return _folderEditCommand;
            }
        }

        private ICommand _saveAsTemplateCommand;
        public ICommand SaveAsTemplateCommand
        {
            get
            {
                if (_saveAsTemplateCommand == null)
                {
                    _saveAsTemplateCommand = new RelayCommand(SaveAsTemplate) { IsEnabled = true };
                }
                return _saveAsTemplateCommand;
            }
        }
        #endregion

        #region Action Handlers

        private void AddShortcut()
        {
            ShortcutPropertiesViewModel vm = new ShortcutPropertiesViewModel();
            
            ShortcutPropertiesView propDialog = new ShortcutPropertiesView();
            propDialog.DataContext = vm;

            propDialog.Owner = Application.Current.MainWindow;
            propDialog.ShowDialog();

            if (propDialog.OkPressed)
            {
                Shortcut shortcut = Shortcut.CreateShortcut(this.Category, vm.Label, vm.Path, vm.Params);
                Category.AddShortcut(shortcut);
                _shortcutViewModels.Add(new ShortcutViewModel(shortcut, this));
                
                this.IsFolded = false;
                
                ShortcutRepository.Instance.SaveToDataFile();
            }            
        }

        public void DeleteShortcut(ShortcutViewModel shortcutViewModel)
        {
            _shortcutViewModels.Remove(shortcutViewModel);
            Category.RemoveShortcut(shortcutViewModel.Shortcut);

            ShortcutRepository.Instance.SaveToDataFile();
        }

        private void DeleteFolder()
        {
            _mainWindowViewModel.DeleteFolder(this);

            ShortcutRepository.Instance.SaveToDataFile();
        }

        internal void Drop(object sender, DragEventArgs e)
        {
            if (e.Data is DataObject && ((DataObject)e.Data).ContainsFileDropList())
            {
                foreach (string filePath in ((DataObject)e.Data).GetFileDropList())
                {
                    Shortcut shortcut = Shortcut.CreateShortcut(
                        Category,
                        label: Path.GetFileName(filePath),
                        path: filePath
                        );

                    Category.AddShortcut(shortcut);
                    _shortcutViewModels.Add(new ShortcutViewModel(shortcut, this));
                }

                ShortcutRepository.Instance.SaveToDataFile();
            }
        }

        internal void DropShortcut(CategoryViewModel parentCategoryVM, ShortcutViewModel shortcutViewModel)
        {
            DropShortcut(parentCategoryVM, shortcutViewModel, null);
        }

        internal void DropShortcut(
			CategoryViewModel parentCategoryVM, 
			ShortcutViewModel shortcutViewModel, 
			ShortcutViewModel droppedOnShortcutVM)
        {			
			parentCategoryVM.DeleteShortcut(shortcutViewModel);

			Shortcut shortcut = shortcutViewModel.Shortcut;		

			ShortcutViewModel newShortcutViewModel = new ShortcutViewModel(shortcut, this);
			if (droppedOnShortcutVM != null)
			{
				int index = _shortcutViewModels.IndexOf(droppedOnShortcutVM);
				_shortcutViewModels.Insert(index, newShortcutViewModel);
				Category.InsertShortcut(index, shortcut);
			}
			else
			{
				_shortcutViewModels.Add(newShortcutViewModel);
				Category.AddShortcut(shortcut); 
			}

            ShortcutRepository.Instance.SaveToDataFile();
        }

        private void EditFolder()
        {
            FolderPropertiesViewModel vm = new FolderPropertiesViewModel() { Label = this.Label };
            FolderPropertiesView proDialog = new FolderPropertiesView();
            proDialog.DataContext = vm;

            proDialog.Owner = Application.Current.MainWindow;
			proDialog.ShowDialog();

            if (proDialog.OkPressed)
            {
                this.Label = vm.Label;
                ShortcutRepository.Instance.SaveToDataFile();
            }
        }

        private void SaveAsTemplate()
        {
            // TODO: show an input box to enter the name and label of the template
            ShortcutRepository.Instance.SaveCategoryAsTemplate(this.Category, "test.tpl", "Test");
        }        

        /// <summary>
        /// Save the hyperlinks in the clipboard text to this card
        /// </summary>
        /// <param name="_hyperlinks"></param>
        public void AddShortcutsFromClipboard(string[] _hyperlinks)
        {
            foreach (string hyperlink in _hyperlinks)
            {
                Shortcut shortcut = Shortcut.CreateShortcut(this.Category, hyperlink, hyperlink, null);
                Category.AddShortcut(shortcut);
                _shortcutViewModels.Add(new ShortcutViewModel(shortcut, this));

                this.IsFolded = false;

                ShortcutRepository.Instance.SaveToDataFile();
            }
        }

        #endregion
    }
}
