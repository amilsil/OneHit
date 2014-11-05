using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OneHit.DataAccess;
using OneHit.Model;
using OneHit.Util;
using OneHit.Util.ResourceUpdate;
using OneHit.View;
using System.Text;
using System;

namespace OneHit.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        List<Template> _templates;
		
        #region Constructors

        /// <summary>
        /// Constructor default
        /// </summary>
        public MainWindowViewModel()
        {			
            _cards = new ObservableCollection<ViewModelBase>();
             _templates = ShortcutRepository.Instance.GetTemplates();

            // Make the ViewModels for Categories
			// Add them to the collection, which is used for rendering
            foreach (Category category in ShortcutRepository.Instance.GetCategories())
            {
                _cards.Add(new CategoryViewModel(category, this));
            }

			// UpdateComplete of Resources
			ResourceUpdateManager.UpdateCompletedEventHandler += 
				new System.EventHandler(ResourceUpdateManager_UpdateCompletedEventHandler);
        }

        #endregion

		#region Event Handlers

		/// <summary>
		/// UpdateComplete of Resources
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ResourceUpdateManager_UpdateCompletedEventHandler(object sender, System.EventArgs e)
		{  
			ShortcutRepository.Instance.ReadLocalTemplateDirectory();
			OnPropertyChanged("TemplateMenuItems");

            ResourcesUpdatedEventArgs re = e as ResourcesUpdatedEventArgs;
            if (re != null)
            {
                if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        (Action)delegate()
                        {
                            // Your Action Code
                            ShowMessageResourcesUpdated(re.UpdatedFiles);
                        });
                }
                OnPropertyChanged("Cards");
            }
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Get new templates and update the menu items accordingly.
		/// </summary>
		private void _UpdateTemplateMenuItems()
		{
			_templates = ShortcutRepository.Instance.GetTemplates();
			if (_templateMenuItems == null)
			{
				_templateMenuItems = new ObservableCollection<MenuItem>();
			}
			else
			{
				_templateMenuItems.Clear();
			}

			foreach (Template template in _templates)
			{
				_templateMenuItems.Add(
					new MenuItem()
					{
						Header = template.Label,
						Command = CreateFolderFromTemplateCommand,
						CommandParameter = template
					}
				 );
			}
		}

        /// <summary>
        /// Add new shortcuts from the hyperlinks copied to clipboard
        /// </summary>
        /// <param name="hyperlinks"></param>
        internal void AddShortcutsFromClipboard(Queue<string> hyperlinks)
        {
            int index = -1;
            
            if (RegistryHelper.IsSaveToCard && RegistryHelper.CardIndexToSaveTo != -1)
            {
                // If default is to sliently save the hyperlink to a predefined card.
                ((CategoryViewModel)Cards[RegistryHelper.CardIndexToSaveTo]).AddShortcutsFromClipboard(hyperlinks.ToArray());
                ((MainWindow)Application.Current.MainWindow).ShowBalloon(
                    "OneHit: Shortcut added!",
                    string.Format("Shortcut was added to card [{0}]", ((CategoryViewModel)Cards[RegistryHelper.CardIndexToSaveTo]).Label), 
                    System.Windows.Forms.ToolTipIcon.Info, 
                    5);
            }

            else
            {
                // View the dialog to select the card, to which the clipboard links to be saved.
                CategoryForClipboardViewModel vm = new CategoryForClipboardViewModel();
                CategoryForClipboardView selectCategoryDialog = new CategoryForClipboardView();
                selectCategoryDialog.DataContext = vm;
                selectCategoryDialog.Owner = Application.Current.MainWindow;
                selectCategoryDialog.ShowDialog();

                if (selectCategoryDialog.OkPressed)
                {
                    index = selectCategoryDialog.cbCards.SelectedIndex;

                    RegistryHelper.IsSaveToCard = vm.IsDefaultCategoryForClipboard;
                    RegistryHelper.CardIndexToSaveTo = index;

                    ((CategoryViewModel)Cards[index]).AddShortcutsFromClipboard(hyperlinks.ToArray());
                }
            }
            
        }

		#endregion

		#region Bound Properties & Commands

		ObservableCollection<ViewModelBase> _cards;
        /// <summary>
        /// ViewModels viewed as cards in the main window
        /// </summary> 
        public ObservableCollection<ViewModelBase> Cards
        {
            get
            {
                return _cards;
            }
        }

        private ICommand _addFolderCommand;
        
        public ICommand AddFolderCommand
        {
            get
            {
                if (_addFolderCommand == null)
                {
                    _addFolderCommand = new RelayCommand(AddFolder) { IsEnabled = true };
                }
                return _addFolderCommand;
            }
        }

        public ICommand CreateFolderFromTemplateCommand
        {
            get
            {
                return new ParamRelayCommand<Template>(CreateFolderFromTemplate) { IsEnabled = true };
            }
        }

        private ICommand _viewSettingsCommand;
        public ICommand ViewSettingsCommand
        {
            get
            {
                if (_viewSettingsCommand == null)
                {
                    _viewSettingsCommand = new RelayCommand(ViewSettings) { IsEnabled = true };
                }
                return _viewSettingsCommand;
            }
        }

		private ObservableCollection<MenuItem> _templateMenuItems;
        /// <summary>
        /// Items of the Template submenu
        /// </summary>
		public ObservableCollection<MenuItem> TemplateMenuItems
		{
			get
			{
				_UpdateTemplateMenuItems();
				return _templateMenuItems;
			}
		}

        #endregion

        #region Action Handlers
        /// <summary>
        /// Adds a folder by user entered Label
        /// </summary>
        private void AddFolder()
        {
            FolderPropertiesViewModel vm = new FolderPropertiesViewModel();
            FolderPropertiesView propDialog = new FolderPropertiesView();
            propDialog.DataContext = vm;
            propDialog.Owner = Application.Current.MainWindow;
            propDialog.ShowDialog();

            if (propDialog.OkPressed)
            {
                Category category = ShortcutRepository.Instance.AddCategory(vm.Label);
                Cards.Add(new CategoryViewModel(category, this));

                ShortcutRepository.Instance.SaveToDataFile();
            }
        }

        /// <summary>
        /// Create a folder using a selected Template.
        /// Param values are entered by the user.
        /// </summary>
        /// <param name="template"></param>
        private void CreateFolderFromTemplate(Template template)
        {
            FolderFromTemplateView folderFromTemplateView = new FolderFromTemplateView();
            FolderFromTemplateViewModel vm = new FolderFromTemplateViewModel(template);
            folderFromTemplateView.DataContext = vm;

            folderFromTemplateView.Owner = Application.Current.MainWindow;
            folderFromTemplateView.ShowDialog();

            if (folderFromTemplateView.OkPressed)
            {
                Category newCategory = template.GetCompiledCategory();
                _cards.Add(new CategoryViewModel(newCategory, this));
                ShortcutRepository.Instance.AddCategory(newCategory);

                ShortcutRepository.Instance.SaveToDataFile();
            }
        }

        /// <summary>
        /// Delete a folder        
        /// </summary>
        /// <param name="categoryViewModel"></param>
        public void DeleteFolder(CategoryViewModel categoryViewModel)
        {
            // Called from CategoryViewModel.cs
			ShortcutRepository.Instance.RemoveCategory(categoryViewModel.Category);
            _cards.Remove(categoryViewModel);
        }

        /// <summary>
        /// Delete the note
        /// </summary>
        /// <param name="noteViewModel"></param>
        public void DeleteNote(NoteViewModel noteViewModel)
        {
            _cards.Remove(noteViewModel);
        }

        /// <summary>
        /// View the Settings View
        /// </summary>
        public void ViewSettings()
        {
            SettingsViewModel vm = new SettingsViewModel();
            SettingsView settingsWindow = new SettingsView();
            settingsWindow.DataContext = vm;

            settingsWindow.Owner = Application.Current.MainWindow;
            settingsWindow.ShowDialog();
        }

        #endregion

        private void ShowMessageResourcesUpdated(List<string> updatedFiles)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("New templates were added successfully.\n");
            sb.Append("Select [Add Folder From Template] from the context menu to use them.\n");

            foreach (string filename in updatedFiles)
            {
                sb.Append("\n" + filename);
            }

            Cards.Insert(1, new NoteViewModel(this) { Header = "New Templates!", Text = sb.ToString(), Time = DateTime.Now.ToShortTimeString(), TypeOfNote = NoteViewModel.NoteType.TemplatesUpdated });
        }
    }
}
