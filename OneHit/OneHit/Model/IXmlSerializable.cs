using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace OneHit.Model
{
	public interface IXmlSerializable
	{
		void WriteXml(XmlWriter writer);
	}
}
