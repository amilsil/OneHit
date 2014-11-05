using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneHit.Model;

namespace OneHit.ViewModel
{
    class ParamViewModel : ViewModelBase
    {
        private Param _param;

        public ParamViewModel(Param param)
        {
            _param = param;
        }

        public string ParamName
        {
            get
            {
                return _param.Name;
            }
        }

        public string ParamDescription
        {
            get
            {
                return _param.Description;
            }
        }

        public string ParamValue
        {
            get
            {
                return _param.Value;
            }
            set
            {
                _param.Value = value;
            }
        }
    }

    class FolderFromTemplateViewModel : ViewModelBase
    {
        Template _template;

        public FolderFromTemplateViewModel(Template template)
        {
            _template = template;
        }

        public List<ParamViewModel> Params
        {
            get
            {
                List<ParamViewModel> paramViewModels = new List<ParamViewModel>();

                foreach (Param param in _template.Params)
                {
                    paramViewModels.Add(new ParamViewModel(param));
                }

                return paramViewModels;
            }
        }

        public string WindowTitle
        {
            get
            {
                return string.Format("Create folder from Template {0}", _template.Label);
            }
        }

        public String TemplateName
        {
            get
            {
                return _template.Label;
            }
            set
            {
                _template.Label = value;
            }
        }
    }
}
