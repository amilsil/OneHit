using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneHit.Util;
using System.Windows.Input;

namespace OneHit.ViewModel
{
    /// <summary>
    /// Holds data for the settings view.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        #region Bound Properties

        public bool RunAtStartup { get; set; }
        public bool IsSaveToCard { get; set; }
        public int SelectedCardIndex { get; set; }

        public bool PromptEverytime {
            get
            {
                return !IsSaveToCard;
            }
            set
            {
                IsSaveToCard = !value;
                OnPropertyChanged("IsCardListEnabled");
            }
        }
        
        public bool IsCardListEnabled 
        {
            get
            {
                return IsSaveToCard;
            }
        }

        /// <summary>
        /// List to be viewed inside the combo box. To select the card to save to.
        /// </summary>
        public List<OneHit.Model.Category> CardsList
        {
            get 
            {
                return OneHit.DataAccess.ShortcutRepository.Instance.GetCategories();
            }
        }

        #endregion

        #region .Ctor

        internal SettingsViewModel()
        {
            // Initialize all the settings from the registry
            RegistryHelper.InitializeSettings(this);
        }

        #endregion

        #region Action handlers

        /// <summary>
        /// Saves all the settings to the registry.
        /// </summary>
        internal void Save()
        {
            WindowsShell.RegisterInStartup(RunAtStartup, Model.ApplicationContext.APPLICATION_NAME);

            RegistryHelper.SaveSettings(this);
        }

        #endregion
    }
}
