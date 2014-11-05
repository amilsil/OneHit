using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;
using OneHit.Model;

namespace OneHit.DataAccess
{
    /// <summary>
    /// Writing and reading the xml data files
    /// A singleton
    /// </summary>
    public class ShortcutRepository
    {
        // Ultimate category list
        private List<Category> _categories;

        // Used to filter shortcuts on Quick View
        public List<Shortcut> ShortcutList = new List<Shortcut>();

        #region Constructors (static/default)

        static ShortcutRepository()
        {
        }

		/// <summary>
		/// Constructor
		/// </summary>
        public ShortcutRepository()
        {            
            if (_categories == null)
            {
                _categories = new List<Category>();
            }

			// Read data file shortcuts
			// Read all the template files
            ReadPrimaryDataFile();
            ReadLocalTemplateDirectory();
        }

        #endregion

        #region Add Remove Categories

		/// <summary>
		/// Create a new category, and add it to the category list
		/// </summary>
		/// <param name="label">Label for the new category</param>
		/// <returns>created category</returns>
        internal Category AddCategory(string label)
        {
            Category category = Category.CreateCategory(label);
            _categories.Add(category);
            return category;
        }

		/// <summary>
		/// Add a category to the category list
		/// </summary>
		/// <param name="category">Category to add</param>
        internal void AddCategory(Category category)
        {
            _categories.Add(category);
        }

		/// <summary>
		/// If the categories list have the passed in category, remove it.
		/// </summary>
		/// <param name="category">Category to remove</param>
        internal void RemoveCategory(Category category)
        {
            _categories.Remove(category);
        }

        #endregion

        #region File Systems Utilities        

        private void EnsureDirectoryStructure(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private bool CheckWritable(string filename)
        {
            FileStream stream = null;
            try
            {
                stream = File.OpenWrite(filename);
                stream.Close();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(string.Format("Authorization is not enough to write to {0}", filename));                
            }
            catch (IOException ioe)
            {
                MessageBox.Show(ioe.Message);                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);                
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return false;
        }

        #endregion 

        #region Internal interface

        internal List<Category> GetCategories()
        {
            return new List<Category>(_categories);
        }

        internal void SaveToDataFile()
        {
            SaveCategoriesToDatafile(_categories);
        }

        /// <summary>
        /// Save a category as a template in a new file.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filename"></param>
        /// <param name="label"></param>
        internal void SaveCategoryAsTemplate(Category category, string filename, string label)
        {
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            using (XmlWriter writer = XmlWriter.Create(Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION, ApplicationContext.TEMPLATE_DIRECTORY_NAME, filename), settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(XMLContext.XML_ROOT);
                writer.WriteStartElement(XMLContext.XML_TEMPLATE);
                
                writer.WriteElementString(XMLContext.XML_NAME, label);
                
                writer.WriteStartElement(XMLContext.XML_SHORTCUTS);

                category.WriteXmlAsTemplate(writer);

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement(); // End XML_ROOT
            }
        }

        internal bool HasSavePriviledgesForTemplates
        {
            get
            {
                return FileSystemUtility.IsWritable(Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION, ApplicationContext.TEMPLATE_DIRECTORY_NAME));
            }
        }
        #endregion

        #region Read/Write xml

		/// <summary>
		/// Invoked if file is not found, OR
		/// file format is wrong.
		/// </summary>
        private void CreateNewDatafile()
        {
			string remoteFile = Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION, ApplicationContext.PRIMARY_DATA_FILENAME);

			if (File.Exists(remoteFile))
			{
				File.Copy(remoteFile, ApplicationContext.LOCAL_PRIMARY_DATA_FILE);
			}
			else
			{
				// Create empty file
				// A dummy list of categories
				_categories = new List<Category>();
				Category category = Category.CreateCategory("Lucky folder");
				category.AddShortcut(Shortcut.CreateShortcut(category, "Notepad", @"C:\windows\notepad.exe"));
				_categories.Add(category);

				// Save them to the file
				SaveCategoriesToDatafile(_categories);
			}
        }

        /// <summary>
        /// Check if directory and file exists, If not create them
        /// Load the shortcuts to the list
        /// </summary>
        private void ReadPrimaryDataFile()
        {
            EnsureDirectoryStructure(ApplicationContext.USER_APPLICATION_DATA_PATH);

			if (!File.Exists(ApplicationContext.LOCAL_PRIMARY_DATA_FILE))
            {
                CreateNewDatafile();
            }

			if (File.Exists(ApplicationContext.LOCAL_PRIMARY_DATA_FILE))
            {
                _categories = LoadShortcutCategories();
            }
            else
            {
                _categories = new List<Category>();
            }
        }

        /// <summary>
        /// Loads the shortcut categories from the main data xml file.
        /// Called only from ReadPrimaryDataFile().
        /// Does not check if the file exists.
        /// </summary>
        /// <returns></returns>
        private List<Category> LoadShortcutCategories()
        {
            List<Category> _categories = new List<Category>();

			XmlDocument xmlDocument = new XmlDocument();

			bool fileReadSuccess = false;
			while (!fileReadSuccess)
			{
                XmlTextReader reader = new XmlTextReader(ApplicationContext.LOCAL_PRIMARY_DATA_FILE);
				try
				{
                    xmlDocument.Load(reader);
					fileReadSuccess = true;
				}
				catch (XmlException)
				{
					MessageBox.Show(string.Format("The format of the data file {0} is wrong. \n A new one will be created after backing up the current", ApplicationContext.PRIMARY_DATA_FILENAME));					
					reader.Close();

                    try
                    {
                        File.Move(ApplicationContext.LOCAL_PRIMARY_DATA_FILE,
                            ApplicationContext.USER_APPLICATION_DATA_PATH
                            + "backup_"
                            + DateTime.Now.ToFileTimeUtc()
                            + "_"
                            + ApplicationContext.PRIMARY_DATA_FILENAME);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                    finally
                    {
                        CreateNewDatafile();
                    }
				}
				finally
				{
					reader.Close();
				}
			}

            XmlElement root = xmlDocument.DocumentElement;

            XmlNodeList categories = root.SelectNodes(XMLContext.XML_CATEGORY);

            foreach (XmlNode xml_category in categories)
            {
                Category temp = ShortcutCategoryFactory.CreateCategory(xml_category);
				_categories.Add(temp);

                foreach (Shortcut s in temp.Shortcuts)
                {
                    ShortcutList.Add(s);
                }
            }

            return _categories;
        }

		/// <summary>
		/// Save the collection of categories to the data file.
		/// in the order the categories are in the collection.
		/// </summary>
		/// <param name="categories">A list of categories</param>
        private void SaveCategoriesToDatafile(List<Category> categories)
        {
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
			using (XmlWriter writer = XmlWriter.Create(ApplicationContext.LOCAL_PRIMARY_DATA_FILE, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(XMLContext.XML_ROOT);

                foreach (Category category in categories)
                {
					category.WriteXml(writer);
                }

				writer.WriteEndElement(); // End XML_ROOT
            }
        }

        #endregion

        #region Loading Templates
        
        List<Template> _templates = new List<Template>();

        public List<Template> GetTemplates()
        {
            _templates.Sort(new TemplateComparer());
            return new List<Template>(_templates);
        }

        /// <summary>
        /// Scans the template directory for *.tpl files.
        /// Calls ReadLocalTemplateFile() for each.
        /// </summary>
        public void ReadLocalTemplateDirectory()
        {
            if (!Directory.Exists(
				Path.Combine(
					ApplicationContext.USER_APPLICATION_DATA_PATH,
					ApplicationContext.TEMPLATE_DIRECTORY_NAME
					)
				))
                return;

			// Refill the templates
			_templates.Clear();

            // Template folder exists
            string[] templateFiles = Directory.GetFiles(Path.Combine(ApplicationContext.USER_APPLICATION_DATA_PATH, ApplicationContext.TEMPLATE_DIRECTORY_NAME), "*.tpl");

            foreach (string templateFile in templateFiles)
            {
                ReadLocalTemplateFile(templateFile);
            }
        }

        /// <summary>
        /// Reads a template file.
        /// A template file can have multiple templates.
        /// If format is wrong, skip the whole file.
        /// If format of a single template is wrong, jump to the next one.
        /// </summary>
        /// <param name="fileName"></param>
        private void ReadLocalTemplateFile(string fileName)
        {
            XmlTextReader reader = new XmlTextReader(fileName);
            XmlDocument xmlDocument = new XmlDocument();

            try
            {
                xmlDocument.Load(reader);                
            }
            catch (XmlException)
            {
                MessageBox.Show(string.Format("The format of the template file {0} is wrong.\n The file will be skipped.", fileName));
                reader.Close();
                return;
            }
            finally
            {
                reader.Close();
            }
            
            XmlElement xRoot = xmlDocument.DocumentElement;
            XmlNodeList xTemplates = xRoot.SelectNodes(XMLContext.XML_TEMPLATE);

            foreach (XmlNode xTemplate in xTemplates)
            {
                try
                {
                    // Template name = template name or file name
                    string templateName = (xTemplate[XMLContext.XML_NAME] != null) ?
                        xTemplate[XMLContext.XML_NAME].InnerText : fileName;

                    Template oTemplate = Template.CreateTemplate(templateName);

                    // we add template shortcuts to quickview.
                    // if params available, compiling needed. Thus, cannot add to quickview.
                    bool isCompilingNeeded = false;

					try
					{
						// Params
						XmlNode xParamsRoot = xTemplate.SelectSingleNode(XMLContext.XML_PARAMS);
						if (xParamsRoot != null)
						{
							XmlNodeList xParams = xParamsRoot.SelectNodes(XMLContext.XML_PARAM);

                            if (xParams.Count > 0)
                                isCompilingNeeded = true;

							foreach (XmlNode xParam in xParams)
							{
								string paramName = xParam[XMLContext.XML_NAME].InnerText;
								string paramDescription = xParam[XMLContext.XML_DESCRIPTION].InnerText;

								oTemplate.AddParam(paramName, paramDescription);
							}
						}
					}
					catch (Exception) 
					{ 
						// Don't worry if no parameters are defined.
					}

                    // Shortcuts
                    XmlNode xShortcutssRoot = xTemplate.SelectSingleNode(XMLContext.XML_SHORTCUTS);
                    XmlNodeList xShortcuts = xShortcutssRoot.SelectNodes(XMLContext.XML_SHORTCUT);

                    foreach (XmlNode xShortcut in xShortcuts)
                    {
                        string shortcutLabel = xShortcut[XMLContext.XML_LABEL].InnerText;
                        string shortcutPath = xShortcut[XMLContext.XML_PATH].InnerText;
                        string shortcutParams = (xShortcut[XMLContext.XML_PARAMS] != null) ?
                            xShortcut[XMLContext.XML_PARAMS].InnerText : string.Empty;

                        Shortcut shortcut = Shortcut.CreateShortcut(oTemplate, shortcutLabel, shortcutPath, shortcutParams);
                        oTemplate.AddShortcut(
                                shortcut
                            );

                        if(!isCompilingNeeded)
                            ShortcutList.Add(shortcut);
                    }

                    _templates.Add(oTemplate);
                }
                catch (Exception)
                {
                    // On any exception, jump to the next
                    continue;
                }                
            }
        }

        #endregion

        #region Singleton Property
        //when adding a shortcut through GUI, need to call category.addshortcut()
        //which we cannot call at the start. coz, constructor recurse can happen
        //read category.addshortcut comment
        public static bool IsInitialized = false;

        private static ShortcutRepository _instance;
        private static Object singleInstanceLockObject = new Object();
        public static ShortcutRepository Instance
        {
            get
            {
                lock (singleInstanceLockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new ShortcutRepository();
                        IsInitialized = true;
                    }
                    return _instance;
                }
            }
        }

        #endregion
    }

    public class TemplateComparer : IComparer<Template>
    {
        public int Compare(Template x, Template y)
        {
            return x.Label.CompareTo(y.Label);
        }
    }
}
