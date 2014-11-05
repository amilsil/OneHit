using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OneHit.ViewModel;

namespace OneHit.View
{
    /// <summary>
    /// Interaction logic for FilteredShortcutView.xaml
    /// </summary>
    public partial class FilteredShortcutView : UserControl
    {
        public FilteredShortcutView()
        {
            InitializeComponent();
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ShortcutViewModel svm = this.DataContext as ShortcutViewModel;
                if (svm != null)
                {
                    svm.OpenShortcut();
                }
            }
        }
    }
}
