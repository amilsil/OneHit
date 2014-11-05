
using System.Windows.Input;
using OneHit.Util;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
namespace OneHit.ViewModel
{
    public class ShortcutPropertiesViewModel : ViewModelBase
    {
        private ICommand _browsePath;
        public ICommand BrowsePathCommand
        {
            get
            {
                if (_browsePath == null)
                {
                    _browsePath = new RelayCommand(BrowsePath) { IsEnabled = true };
                }
                return _browsePath;
            }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;

                OnPropertyChanged("Path");
                OnPropertyChanged("OKEnabled");
            }
        }

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

		private string _params;
		public string Params
		{
			get { return _params; }
			set
			{
				_params = value;
				OnPropertyChanged("Params");
			}
		}

        public bool OKEnabled
        {
            get
            {
                // URL
                if (Path.StartsWith("http", false, System.Globalization.CultureInfo.CurrentCulture))
                {
                    return true;
                }

                if (Directory.Exists(Path)
                    && ((File.GetAttributes(Path) & FileAttributes.Directory) == FileAttributes.Directory))
                {
                    // Folder
                    return true;
                }
                else
                {
                    // File
                    return File.Exists(Path) && !string.IsNullOrEmpty(Label);
                }
            }
        }

        
        private void BrowsePath()
        {
            OpenFileDialog dlg = new OpenFileDialog();            

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Path = dlg.FileName;

				if (string.IsNullOrEmpty(Params))
				{
					Label = System.IO.Path.GetFileNameWithoutExtension(_path);
				}
				else
				{
					Label = Params;
				}
            }
        }        
    }
}
