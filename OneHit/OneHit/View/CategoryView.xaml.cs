using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OneHit.Util;
using OneHit.ViewModel;

namespace OneHit.View
{
    /// <summary>
    /// Interaction logic for CategoryView.xaml
    /// </summary>
    public partial class CategoryView : UserControl
    {
        public CategoryView()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(_loadedEventHandler);
        }

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("SHORTCUT_VM") ||
                sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }  

        /// <summary>
        /// Drop an internal or external shortcut
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("SHORTCUT_VM"))
            {
                ShortcutViewModel shortcutViewModel = e.Data.GetData("SHORTCUT_VM") as ShortcutViewModel;
                CategoryViewModel parentCategoryVM = e.Data.GetData("PARENT_VM") as CategoryViewModel;

                ShortcutView droppedOnShortcutView = FindAnchestor<ShortcutView>((DependencyObject)e.OriginalSource);

				// Dropped on itself
				if (droppedOnShortcutView != null 
					&& (droppedOnShortcutView.DataContext as ShortcutViewModel).Equals(shortcutViewModel))
				{
					droppedOnShortcutView.BorderBrush = new SolidColorBrush(Colors.Transparent);
					droppedOnShortcutView.BorderThickness = new Thickness(0);
					return;
				}

				// Dropped on Header
                if (droppedOnShortcutView == null)
                {
                    (this.DataContext as CategoryViewModel).DropShortcut(parentCategoryVM, shortcutViewModel);
                }
                else
                {
					// Dropped on a shortcut                    
                    droppedOnShortcutView.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    droppedOnShortcutView.BorderThickness = new Thickness(0);

                    ShortcutViewModel droppedOnShortcutVM = null;
                    if (droppedOnShortcutView != null)
                    {
                        droppedOnShortcutVM = droppedOnShortcutView.DataContext as ShortcutViewModel;
                    }

                    (this.DataContext as CategoryViewModel).DropShortcut(
                        parentCategoryVM, shortcutViewModel, droppedOnShortcutVM);
                }
            }
            else
            {
                (this.DataContext as CategoryViewModel).Drop(sender, e);
            }

            e.Handled = true;
        }

		private void UserControl_DragLeave(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent("SHORTCUT_VM"))
			{
				ShortcutView dragOnShortcutView = FindAnchestor<ShortcutView>((DependencyObject)e.OriginalSource);
				//ShortcutViewModel dragOnShortcutVM = null;
				if (dragOnShortcutView != null)
				{
					//dragOnShortcutVM = dragOnShortcutView.DataContext as ShortcutViewModel;
					dragOnShortcutView.BorderBrush = new SolidColorBrush(Colors.Transparent);
					dragOnShortcutView.BorderThickness = new Thickness(0);
				}
			}
		}

        private void UserControl_DragOver(object sender, DragEventArgs e)
        {
            var dropPossible = e.Data != null && ((DataObject)e.Data).ContainsFileDropList();

            if (e.Data.GetDataPresent("SHORTCUT_VM"))
            {
                dropPossible = true;

				ShortcutView dragOnShortcutView = FindAnchestor<ShortcutView>((DependencyObject)e.OriginalSource);
				if (dragOnShortcutView != null)
				{
					dragOnShortcutView.BorderBrush = new SolidColorBrush(Colors.Orange);
					dragOnShortcutView.BorderThickness = new Thickness(0, 1, 0, 0);
				}
            }

            if (dropPossible)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        #region Collapse Expand

        private Color _collapsedTitleColor = Color.FromRgb(50,50,50);
        private Color _defaultTitleColor;

        /// <summary>
        /// Toggle zip/extract when click on the header.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.listBox1.Visibility == System.Windows.Visibility.Hidden)
            {
                _expand();
            }
            else
            {
                _collapse();
            }
        }

        /// <summary>
        /// Folder is loaded. Collapse the folder if folded according to the model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _loadedEventHandler(object sender, RoutedEventArgs e)
        {
            if (_collapsed)
            {
                _collapse();
            }
        }

        /// <summary>
        /// Zip the folder
        /// </summary>
        private void _collapse()
        {
            // Remember colour for restoring.
            _defaultTitleColor = ((SolidColorBrush)lblTitle.Background).Color;

            // Zip
            this.listBox1.Visibility = System.Windows.Visibility.Hidden;
            this.Height = this.ActualHeight - this.listBox1.ActualHeight;
            this.lblTitle.Background = new SolidColorBrush(_collapsedTitleColor);

            _collapsed = true;
        }

        /// <summary>
        /// Expand the folder
        /// </summary>
        private void _expand()
        {
            // Extract
            this.listBox1.Visibility = System.Windows.Visibility.Visible;
            this.lblTitle.Background = new SolidColorBrush(_defaultTitleColor);

            // Restore to content height
            this.Height = Double.NaN;

            _collapsed = false;
        }
        
        /// <summary>
        /// If the folder is collapsed or not according to the model
        /// </summary>
		private bool _collapsed 
		{
            get 
            {
                CategoryViewModel category_vm = this.DataContext as CategoryViewModel;
                if (category_vm != null)
                {
                    return category_vm.IsFolded;
                }

                return false;
            }
            set
            {
                CategoryViewModel category_vm = this.DataContext as CategoryViewModel;
                if (category_vm != null)
                {
                    if (category_vm.IsFolded != value)
                    {
                        category_vm.IsFolded = value;
                        OneHit.DataAccess.ShortcutRepository.Instance.SaveToDataFile();
                    }
                }
            }
		}

        #endregion

        #region Drag Stuff

        private Point startPoint;
        private ShortcutView draggedShortcutView;        
        private ShortcutViewModel draggedShortcutViewModel;
        private Object dragLock = new Object();
        private DataObject dragData;

        private void listBox1_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			// Store the mouse position
            startPoint = e.GetPosition(null);

			// Get the dragged ListViewItem
			ItemsControl itemsControl = sender as ItemsControl;
			draggedShortcutView = 	FindAnchestor<ShortcutView>((DependencyObject)e.OriginalSource);

			draggedShortcutViewModel
					= draggedShortcutView.DataContext as ShortcutViewModel;

			// Find the data behind the ListViewItem                
			dragData = new DataObject("SHORTCUT_VM", draggedShortcutViewModel);
			dragData.SetData("PARENT_VM", this.DataContext as CategoryViewModel);	

			try
			{
				Monitor.Exit(dragLock);
			}
			catch (Exception)
			{				
			}
        }

        private void listBox1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

		private void listBox1_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {	
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            // Avoid multiple drop for same drag
            if (!(e.OriginalSource is Button))
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > 10 ||
                Math.Abs(diff.Y) > 10))
            {
				if (Monitor.TryEnter(dragLock))
				{
                    try
                    {
                        DragDrop.DoDragDrop(draggedShortcutView, dragData, DragDropEffects.Move);
                    }
                    catch (System.IO.IOException)
                    {
                        // TODO: fix exception
                    }
				}
            }
        }

        // Helper to search up the VisualTree
        private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        #endregion
    }
}
