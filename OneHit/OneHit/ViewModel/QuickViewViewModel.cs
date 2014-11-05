using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneHit.ViewModel
{
    /// <summary>
    /// For Quick View
    /// </summary>
    internal class QuickViewViewModel : ViewModelBase
    {
        // Selected after filtering
        private List<ShortcutViewModel> _selectedShortcuts;
        // previousely hightlighted one
        private ShortcutViewModel prevHighlighted;

        private List<ShortcutViewModel> _shortcutVMs;

        public List<ViewModel.ShortcutViewModel> Shortcuts
        {
            get
            {
                if (_shortcutVMs == null)
                {
                    _shortcutVMs = new List<ShortcutViewModel>();
                    foreach (Model.Shortcut s in OneHit.DataAccess.ShortcutRepository.Instance.ShortcutList)
                    {
                        _shortcutVMs.Add(new ShortcutViewModel(s));
                    }
                }

                return _shortcutVMs;
            }
        }

        private string _filterText;
        public string FilterText
        {
            get 
            { 
                return _filterText; 
            }
            set
            {
                _filterText = value;
                FilterShortcuts(_filterText);
                OnPropertyChanged("TxtNumberBg");
            }
        }

        public System.Windows.Media.Brush TxtNumberBg
        {
            get
            {
                if (_isTxtNumberValid)
                {
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Silver);
                }
                else
                {
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                }
            }
        }

        private bool _isTxtNumberValid;
        public bool IsTxtNumberValid
        {
            set
            {
                _isTxtNumberValid = value;
                OnPropertyChanged("TxtNumberBg");
            }
        }

        /// <summary>
        /// Filter the shortcuts depending on the filtertext.
        /// </summary>
        /// <param name="filterText"></param>
        private void FilterShortcuts(string filterText)
        {
            if (_selectedShortcuts == null)
                _selectedShortcuts = new List<ShortcutViewModel>();
            else
                _selectedShortcuts.Clear();
            
            foreach (ShortcutViewModel svm in Shortcuts)
            {
                //updating filtertext will change shortcut visibility
                svm.FilterText = filterText;

                if (svm.IsSelected)
                {
                    //if this one is selected, 
                    //add this to the selected list
                    _selectedShortcuts.Add(svm);
                }
            }

            //view a yellow quick launch number with all selected shortcuts
            ShowQuickLaunchNumber();
        }

        /// <summary>
        /// Show a quick launch number with every shortcut
        /// </summary>
        private void ShowQuickLaunchNumber()
        {
            int index = 1;
            foreach(ShortcutViewModel svm in _selectedShortcuts)
            {
                svm.RenderQuickLaunchNumber(index);
                index++;
            }
        }

        // Set a shortcut selected as number entered after filtering
        public bool SetHighlightedIndex(int index)
        {
            // set previous one normal
            if (prevHighlighted != null)
            {
                prevHighlighted.IsHighlighted = false;
                prevHighlighted = null;
            }

            // select this one
            if (index > 0 && index < _selectedShortcuts.Count + 1)
            {
                _selectedShortcuts[index - 1].IsHighlighted = true;
                prevHighlighted = _selectedShortcuts[index - 1];

                return true;
            }

            return false;
        }

        public void OpenHighlightedIndex()
        {
            if(prevHighlighted!=null)
                prevHighlighted.OpenShortcut();
        }
    }
}
