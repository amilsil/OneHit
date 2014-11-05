using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using OneHit.Util;

namespace OneHit.Model
{
	internal class ShortcutCategoryFactory
	{
		public static Category CreateCategory(XmlNode node)
		{
			// If META is available, read it
			XmlNode xml_meta = node.SelectSingleNode(XMLContext.XML_META);

			bool isFolded = false;
			
            string sSource = default(string);

			string categoryLabel = node.Attributes[XMLContext.XML_LABEL].Value;			
				
			if (xml_meta != null && xml_meta.HasChildNodes)
			{
				string sFolded = (xml_meta[XMLContext.XML_FOLDED] != null) ?
					xml_meta[XMLContext.XML_FOLDED].InnerText : string.Empty;

				string sLive = (xml_meta[XMLContext.XML_LIVE] != null) ?
					xml_meta[XMLContext.XML_LIVE].InnerText : string.Empty;

				string sSourceType = (xml_meta[XMLContext.XML_SOURCE_TYPE] != null) ?
					xml_meta[XMLContext.XML_SOURCE_TYPE].InnerText : string.Empty;

                sSource = (xml_meta[XMLContext.XML_SOURCE] != null) ?
                    xml_meta[XMLContext.XML_SOURCE].InnerText : string.Empty;

				bool.TryParse(sFolded, out isFolded);				
			}

			// Create the category
			Category newCategory = default(Category);

			newCategory = Category.CreateCategory(categoryLabel);			

			// If SHORTCUTS node is available, every shortcut is inside of it
			XmlNode xml_shortcuts = (node.SelectSingleNode(XMLContext.XML_SHORTCUTS) != null) ?
				node.SelectSingleNode(XMLContext.XML_SHORTCUTS) : node;

			// Loop through shortcuts
			foreach (XmlNode xml_shortcut in xml_shortcuts.SelectNodes(XMLContext.XML_SHORTCUT))
			{
				newCategory.AddShortcut(CreateShortcut(newCategory, xml_shortcut));
			}

			newCategory.IsFolded = isFolded;

			return newCategory;
		}

		/// <summary>
		/// Reads the Shortcut from XmlDocument.
		/// </summary>
		/// <param name="reader"></param>
		public static Shortcut CreateShortcut(Category category, XmlNode node)
		{
			string linkLabel = (node[XMLContext.XML_LABEL] != null) ?
						node[XMLContext.XML_LABEL].InnerText : string.Empty;

			string linkPath = (node[XMLContext.XML_PATH] != null) ?
				node[XMLContext.XML_PATH].InnerText : string.Empty;

			string linkParams = (node[XMLContext.XML_PARAMS] != null) ?
				node[XMLContext.XML_PARAMS].InnerText : string.Empty;

			return Shortcut.CreateShortcut(category, linkLabel, linkPath, linkParams);
		}
	}
}
