using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneHit.ViewModel
{
    class FolderPropertiesViewModel : ViewModelBase
    {
        private string _label;
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                OnPropertyChanged("Label");
                OnPropertyChanged("OKEnabled");
            }
        }

        public bool OKEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(Label);
            }
        }
    }
}
