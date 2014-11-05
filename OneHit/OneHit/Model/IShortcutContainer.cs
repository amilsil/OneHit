using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneHit.Model
{
    /// <summary>
    /// For the purpose of distinguishing Category and Template
    /// </summary>
    public interface IShortcutContainer
    {
        string Label { get; set; }
    }
}
