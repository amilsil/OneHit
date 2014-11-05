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
using OneHit.ViewModel;
using OneHit.Util;
using System.Windows.Interop;

namespace OneHit.View
{
    /// <summary>
    /// Interaction logic for QuickView.xaml
    /// </summary>
    public partial class QuickView : Window
    {
        public QuickView()
        {
            InitializeComponent();
        }

        private void txtFilter_KeyUp(object sender, KeyEventArgs e)
        {
            QuickViewViewModel qvm = this.DataContext as QuickViewViewModel;
            if (qvm != null)
            {
                qvm.FilterText = txtFilter.Text;
            }

            // Update if the number field is correct or not
            qvm.IsTxtNumberValid = false;
            int index = 0;
            if (Int32.TryParse(txtNumber.Text, out index))
            {
                if (qvm != null)
                {
                    if (qvm.SetHighlightedIndex(index))
                        qvm.IsTxtNumberValid = true;
                }
            }

            // if escape, Hide
            if (e.Key == Key.Escape)
            {
                this.Hide();
            }

            // if enter, enter quick launch with number mode
            if (e.Key == Key.Enter)
            {
                txtNumber.Focus();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            
            base.OnClosing(e);

            this.Hide();
        }

        private void OnShow(object sender, EventArgs e)
        {
            this.Show();
        }

        /// <summary>
        /// New Show for initializing
        /// </summary>
        public new void Show()
        {
            base.Show();
            
            // Bring the window to top, and allow it to sink
            Topmost = true;
            Topmost = false;

            this.txtFilter.Focus();
            this.txtFilter.SelectAll();
        }

        private void txtNumber_KeyUp(object sender, KeyEventArgs e)
        {
            //TODO:can only enter numbers
            // highlight the shortcut of this number

            QuickViewViewModel qvm = this.DataContext as QuickViewViewModel;

            qvm.IsTxtNumberValid = false;

            int index = 0;
            if (Int32.TryParse(txtNumber.Text, out index))
            {
                if (qvm != null)
                {
                    if (qvm.SetHighlightedIndex(index))
                        qvm.IsTxtNumberValid = true;
                }

                // if enter key pressed, open the selected link
                // hide the window
                if (e.Key == Key.Enter)
                {
                    qvm.OpenHighlightedIndex();

                    this.Close();
                }
            }
        }

        private void txtNumber_GotFocus(object sender, RoutedEventArgs e)
        {
            txtNumber.SelectAll();
        }

    }
}
