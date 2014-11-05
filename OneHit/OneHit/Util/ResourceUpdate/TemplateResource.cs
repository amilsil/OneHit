using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneHit.Model;

namespace OneHit.Util.ResourceUpdate
{
	class TemplateResource : UpdatableResource
	{
		public TemplateResource() : base(ApplicationContext.TEMPLATE_DIRECTORY_NAME)
		{
			
		}
	}
}
