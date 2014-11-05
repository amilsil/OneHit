using System;
using System.Runtime.Serialization;
using System.Security;
using System.Xml;
using System.Xml.Schema;
using OneHit.Util;

namespace OneHit.Model
{
    /// <summary>
    /// Represents a shortcut
    /// </summary>
	public class Shortcut : OneHit.Model.IXmlSerializable, IEquatable<Shortcut>, IDisposable
    {
        public string Label { get; set; }
        public string Path { get; set; }
		public string Params { get; set; }

        // For Quick View
        private IShortcutContainer _ParentContainer;
        public string CategoryLabel 
        { 
            get 
            {
                if (_ParentContainer != null)
                {
                    return _ParentContainer.Label;
                }
                return null;
            } 
        }

        public static Shortcut CreateShortcut(IShortcutContainer shortcutContainer, string label, string path)
        {
            return new Shortcut() { _ParentContainer = shortcutContainer, Label = label, Path = path, Params=null };
        }

        public static Shortcut CreateShortcut(IShortcutContainer shortcutContainer, string label, string path, string _params)
		{
            return new Shortcut() { _ParentContainer = shortcutContainer, Label = label, Path = path, Params = _params };
		}

        public bool Equals(Shortcut other)
        {
            if ((this.Path.Equals(other.Path, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

		#region IXmlSerializable Memebers

		/// <summary>
		/// Writes the Shortcut to XmlDocument.
		/// </summary>
		/// <param name="writer"></param>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(XMLContext.XML_SHORTCUT);

			if (!string.IsNullOrEmpty(Params))
			{
				writer.WriteElementString(XMLContext.XML_PARAMS, Params);
			}
			writer.WriteElementString(XMLContext.XML_PATH, Path);
			writer.WriteElementString(XMLContext.XML_LABEL, Label);

			writer.WriteEndElement(); // End XML_SHORTCUT
		}

		#endregion

		#region IDisposable members

		public void Dispose()
		{
			Logger.WriteLine(string.Format("Disposing shortcut {0}", Label));
		}
		#endregion
	}
}
