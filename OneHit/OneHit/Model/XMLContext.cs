
namespace OneHit.Model
{
	internal static class XMLContext
	{
		#region XML Node Keys

		internal static readonly string XML_CATEGORY = "Category";
		internal static readonly string XML_SHORTCUT = "Shortcut";
		internal static readonly string XML_SHORTCUTS = "Shortcuts";
		internal static readonly string XML_LABEL = "Label";
		internal static readonly string XML_PATH = "Path";
		internal static readonly string XML_ROOT = "OneHit";
		internal static readonly string XML_PARAMS = "Params";

		#endregion

		#region XML Template Keys

		internal static readonly string XML_TEMPLATE = "Template";
		internal static readonly string XML_PARAM = "Param";
		internal static readonly string XML_NAME = "Name";
		internal static readonly string XML_DESCRIPTION = "Description";		

		#endregion

		#region XML Meta Keys

		internal static readonly string XML_FOLDED = "Folded";
		internal static readonly string XML_LIVE = "Live"; 
		internal static readonly string XML_META = "Meta";
		internal static readonly string XML_SOURCE = "Source";
		internal static readonly string XML_SOURCE_TYPE = "Source_Type";

		#endregion
	}

	internal enum LiveItemSourceType : byte
	{
		Hudson = 1,

        Rss = 2
	}
}
