using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneHit.ViewModel
{
    /// <summary>
    /// ViewModel for selecting target category to save clipboard hyperlinks to.
    /// </summary>
    public class CategoryForClipboardViewModel : ViewModelBase
    {
        #region Bound Properties

        /// <summary>
        /// cards list for combo box
        /// </summary>
        public List<OneHit.Model.Category> CategoryNames { 
            get {
                return OneHit.DataAccess.ShortcutRepository.Instance.GetCategories();
            } 
        }

        public bool IsDefaultCategoryForClipboard { get; set; }

        /// <summary>
        /// Selected index of the combo box.
        /// </summary>
        public int SelectedIndex { 
            get {
                return OneHit.Util.RegistryHelper.CardIndexToSaveTo;
            }
            set{
                OneHit.Util.RegistryHelper.CardIndexToSaveTo = value;
                OnPropertyChanged("OK_Enabled");
            } 
        }

        /// <summary>
        /// If OK button is enabled?
        /// </summary>
        public bool OK_Enabled
        {
            get
            {
                return SelectedIndex > -1;
            }
        }

        #endregion
    }
}
