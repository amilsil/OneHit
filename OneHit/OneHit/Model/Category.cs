using System.Collections.ObjectModel;
using System.Xml.Schema;
using System.Xml;
using OneHit.Util;
using OneHit.DataAccess;

namespace OneHit.Model
{
    // Data model of a Category
    public class Category : IXmlSerializable, IShortcutContainer
    {
        #region Public Properties

        public string Label { get; set; }
		public bool IsFolded { get; set; }
		public bool IsLive { get; set; }

        // Set of shortcuts of this category.
        private ObservableCollection<Shortcut> _shortcuts;
        public ObservableCollection<Shortcut> Shortcuts
        {
            get
            {
                if (_shortcuts == null)
                {
                    _shortcuts = new ObservableCollection<Shortcut>();
                }
                return _shortcuts;
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">Label for the category</param>
        public Category(string label)
        {
            Label = label;
        }
        #endregion

        #region Methods

        public void AddShortcut(Shortcut shortcut)
        {
            // Add the shortcut to the shortcuts list for Quick View
            // Error due to the property being called recursively. 
            // and not being initialized properly forever.

            // For the GUI adding of new shortcuts
            if(ShortcutRepository.IsInitialized)
                ShortcutRepository.Instance.ShortcutList.Add(shortcut);

            Shortcuts.Add(shortcut);
        }

		public void InsertShortcut(int index, Shortcut shortcut)
		{
			Shortcuts.Insert(index, shortcut);
		}

        public void RemoveShortcut(Shortcut shortcut)
        {
            // Remove shortcuts from the shortcuts list for Quick View            
             OneHit.DataAccess.ShortcutRepository.Instance.ShortcutList.Remove(shortcut);

            _shortcuts.Remove(shortcut);
        }

        public void WriteXmlAsTemplate(XmlWriter writer)
        {
            foreach (Shortcut shortcut in Shortcuts)
            {
                shortcut.WriteXml(writer);
            }
        }

        public static Category CreateCategory(string label)
        {
            return new Category(label);
        }

        public override string ToString()
        {
            return this.Label;
        }

        #endregion

		#region IXmlSerializable Members

		/// <summary>
		/// Serialize the category to Xml Document.
		/// Internally calls Shortcut.WriteXml() to write every shortcut in the category.
		/// </summary>
		/// <param name="writer">XmlWriter to write to</param>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(XMLContext.XML_CATEGORY);
			writer.WriteStartAttribute(XMLContext.XML_LABEL);
			writer.WriteString(Label);
			writer.WriteEndAttribute();

			// Write META
			writer.WriteStartElement(XMLContext.XML_META);
				writer.WriteStartElement(XMLContext.XML_FOLDED);
					writer.WriteString(IsFolded.ToString());
				writer.WriteEndElement(); // End XML_FOLDED

				writer.WriteStartElement(XMLContext.XML_LIVE);
					writer.WriteString(IsLive.ToString());
				writer.WriteEndElement(); // End XML_FOLDED
			writer.WriteEndElement(); // End XML_META

			// Write Shortcuts
			writer.WriteStartElement(XMLContext.XML_SHORTCUTS);

			foreach (Shortcut shortcut in Shortcuts)
			{
				shortcut.WriteXml(writer);
			}

			writer.WriteEndElement(); // End XML_SHORTCUTS

			writer.WriteEndElement(); // End XML_CATEGORY
		}

		#endregion
	}
}
