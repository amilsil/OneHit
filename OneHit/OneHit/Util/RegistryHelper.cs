using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OneHit.Util
{
	class RegistryHelper
	{
		private static RegistryKey _baseRegistryKey = Registry.CurrentUser;
		private static string _sSubKey = "SOFTWARE\\" + OneHit.Model.ApplicationContext.APPLICATION_NAME;

        #region Utility Methods

        private static string FormRegKey(string sSect)
		{
			return sSect;
		}

		private static RegistryKey _getSubKey()
		{
			return _baseRegistryKey.CreateSubKey(_sSubKey);
		}

		public static void SaveSetting(string Section, string Key, string Setting)
		{
			RegistryKey _subKey = _getSubKey();

			string text1 = FormRegKey(Section);
			RegistryKey key1 = _subKey.CreateSubKey(text1);
			if (key1 == null)
			{
				return;
			}
			try
			{
				key1.SetValue(Key, Setting);
			}
			catch (Exception)
			{
				return;
			}
			finally
			{
				key1.Close();
			}

		}

		public static string GetSetting(string Section, string Key, string Default)
		{
			if (Default == null)
			{
				Default = "";
			}

			RegistryKey _subKey = _getSubKey();

			string text2 = FormRegKey(Section);
			RegistryKey key1 = _subKey.OpenSubKey(text2);
			if (key1 != null)
			{
				object obj1 = key1.GetValue(Key, Default);
				key1.Close();
				if (obj1 != null)
				{
					if (!(obj1 is string))
					{
						return null;
					}
					return (string)obj1;
				}
				return null;
			}
			return Default;
		}

        #endregion

        #region Registry entry name constants

        private const string SETTINGS = "Settings";
        private const string RUN_AT_STARTUP = "RunAtStartup";
        private const string SAVE_CBLINKS_TO_CARD = "SaveToCard";
        private const string CARD_INDEX_FOR_CBLINKS = "DefaultCardCB";

        #endregion

        internal static bool IsSaveToCard
        {
            get 
            {
                int saveDefaultToCard;
                if (int.TryParse(GetSetting(SETTINGS, SAVE_CBLINKS_TO_CARD, "0"), out saveDefaultToCard))
                {
                }
                return (saveDefaultToCard == 1);
            }
            set
            {
                SaveSetting(SETTINGS, SAVE_CBLINKS_TO_CARD, (value? "1" : "0"));
            }
        }

        internal static int CardIndexToSaveTo
        {
            get
            {
                int cardIndex;
                if (int.TryParse(GetSetting(SETTINGS, CARD_INDEX_FOR_CBLINKS, "-1"), out cardIndex))
                {
                }

                return cardIndex;
            }
            set
            {
                SaveSetting(SETTINGS, CARD_INDEX_FOR_CBLINKS, value.ToString());
            }
        }

        internal static bool RunAtStartup
        {
            get
            {
                int runAtStartup;
                if (int.TryParse(GetSetting(SETTINGS, RUN_AT_STARTUP, "1"), out runAtStartup))
                {                    
                }
                return (runAtStartup == 1);
            }
            set
            {
                SaveSetting(SETTINGS, RUN_AT_STARTUP, (value? "1" : "0"));
            }
        }

        internal static void SaveSettings(ViewModel.SettingsViewModel vm)
        {
            IsSaveToCard = vm.IsSaveToCard;
            CardIndexToSaveTo = vm.SelectedCardIndex;
            RunAtStartup = vm.RunAtStartup;
        }

        internal static void InitializeSettings(ViewModel.SettingsViewModel vm)
        {
            vm.RunAtStartup = RunAtStartup;
            vm.SelectedCardIndex = CardIndexToSaveTo;
            vm.IsSaveToCard = IsSaveToCard;
        }
    }
}
