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
using System.Windows.Shapes;

namespace OneHit.View
{
    /// <summary>
    /// Interaction logic for FolderFromTemplateView.xaml
    /// </summary>
    public partial class FolderFromTemplateView : Window
    {
        public bool OkPressed { get; set; }

        public FolderFromTemplateView()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            OkPressed = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
