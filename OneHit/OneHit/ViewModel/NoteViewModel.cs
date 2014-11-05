using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using OneHit.Util;
using System.Windows.Media;

namespace OneHit.ViewModel
{
    /// <summary>
    /// The data model of a NoteView.
    /// </summary>
    public class NoteViewModel : ViewModelBase
    {
        #region Instance Variables
        MainWindowViewModel _mainWindowViewModel;
        Color _lightYellow = Color.FromArgb(255, 250, 250, 160);
        Color _white = Color.FromArgb(255, 255, 255, 255);
        Color _lightBlue = Color.FromArgb(255, 200, 200, 255);
        #endregion

        /// <summary>
        /// Different types of notes.
        /// </summary>
        public enum NoteType
        {
            TemplatesUpdated,
            Hint
        }

        public NoteType TypeOfNote { get; set; }

        #region Bound Properties
        public string Header { get; internal set; }
        public string Text { get; internal set; }
        public string Time { get; internal set; }

        public Brush Background
        {
            get 
            {
                GradientBrush gbrush = new LinearGradientBrush(_white, _lightYellow, 90);
                
                switch (TypeOfNote)
                {
                    case NoteType.TemplatesUpdated: gbrush = new LinearGradientBrush(_white, _lightYellow, 90);
                        break;
                    case NoteType.Hint: gbrush = new LinearGradientBrush(_white, _lightBlue, 90);
                        break;
                }

                return gbrush;
            }
        }
        #endregion

        #region Bound Commands
        private ICommand _closeNoteCommand;
        public ICommand CloseNoteCommand
        {
            get
            {
                if (_closeNoteCommand == null)
                {
                    _closeNoteCommand = new RelayCommand(CloseNote) { IsEnabled = true };
                }
                return _closeNoteCommand;
            }
        }
        #endregion

        #region .Ctor
        public NoteViewModel(MainWindowViewModel mainWindowViewModel)
        {
            this._mainWindowViewModel = mainWindowViewModel;
        }

        #endregion

        #region Command Handlers
        private void CloseNote()
        {
            this._mainWindowViewModel.DeleteNote(this);
        }
        #endregion
    }
}
