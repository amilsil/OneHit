using System.Collections.Generic;

namespace OneHit.Model
{
    /// <summary>
    /// Represents a param of a Template
    /// </summary>
    internal class Param
    {
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public string Value { get; set; }
        internal string PlaceHolder
        {
            get
            {
                return "{" + Name + "}";
            }
        }
    }

    /// <summary>
    /// Represents a Template, that can be used to create folders.
    /// </summary>
    public class Template : IShortcutContainer
    {
        /// <summary>
        /// Template Name
        /// </summary>
        public string Label { get; set; }

        // Private properties
        private List<Param> _params = new List<Param>();
        private List<Shortcut> _shortcuts = new List<Shortcut>();

        /// <summary>
        /// Returns the list of Params,
        /// For the ViewModel
        /// </summary>
        internal List<Param> Params
        {
            get
            {
                return _params;
            }
        }        

        /// <summary>
        /// Creates a Template by the given name (as Label)
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>new Template</returns>
        internal static Template CreateTemplate(string name)
        {
            return new Template() { Label = name };
        }

        /// <summary>
        /// Adds a Param to the Template
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        internal void AddParam(string name, string description)
        {
            _params.Add(new Param() { Name = name, Description = description});
        }

        /// <summary>
        /// Adds an uncompiled Shortcut
        /// </summary>
        /// <param name="shortcut">uncompiled Shortcut</param>
        internal void AddShortcut(Shortcut shortcut)
        {
            _shortcuts.Add(shortcut);
        }

        /// <summary>
        /// Returns the compiled Category
        /// </summary>
        /// <returns>Category</returns>
        internal Category GetCompiledCategory()
        {
            Category category = Category.CreateCategory(Label);

            foreach (Shortcut shortcut in _shortcuts)
            {
                category.AddShortcut(
                    Shortcut.CreateShortcut(
                        category,
                        CompileValue(shortcut.Label),
                        CompileValue(shortcut.Path),
                        CompileValue(shortcut.Params)
                        )
                    );
            }

            return category;
        }

        /// <summary>
        /// Compiles a string by replacing Param PlaceHolders with Values.
        /// </summary>
        /// <param name="value">uncompiled string</param>
        /// <returns>compiled string</returns>
        private string CompileValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                foreach (Param param in _params)
                {
                    value = value.Replace(param.PlaceHolder, param.Value);
                }
            }

            return value;
        }
    }
}
