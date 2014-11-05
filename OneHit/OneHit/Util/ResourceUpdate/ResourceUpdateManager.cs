using System.Threading;
using System.Collections.Generic;
using System;

namespace OneHit.Util.ResourceUpdate
{
    public class ResourcesUpdatedEventArgs : EventArgs
    {
        public List<string> UpdatedFiles { get; set; }
    }

    /// <summary>
    /// Checks the server/client for new/changed versions of resource files.
    /// Copy them to the relevant local directories.
    /// </summary>
    class ResourceUpdateManager
    {
		public static event EventHandler UpdateCompletedEventHandler;

        public static EventWaitHandle _waitHandle = new AutoResetEvent(false);

        internal static List<UpdatableResource> _resourcesToUpdate = new List<UpdatableResource>();

        static ResourceUpdateManager()
        {
            _resourcesToUpdate.Add(new TemplateResource());            
        }

        private static void ResourceUpdateThreadProcedure()
        {
            while (true)
            {
				UpdateResources();

                if (_waitHandle.WaitOne(1000 * 60 * 15))
                {
                    // _waitHandle.Set called
                    break;
                }
            }
        }

		/// <summary>
		/// Update the resources (Templates)
		/// Notify the UI to update
		/// </summary>
        private static void UpdateResources()
        {
            List<string> _updatedFiles = new List<string>();

            bool _updated = false;
            foreach (UpdatableResource _resource in _resourcesToUpdate)
            {
				if (_resource.Update(_updatedFiles))
				{
            		_updated = true;
				}
            }

            if (_updated==true && UpdateCompletedEventHandler != null)
			{
                UpdateCompletedEventHandler(null, new ResourcesUpdatedEventArgs() { UpdatedFiles = _updatedFiles });
			}
        }

        #region Internal Methods

        internal static void StartResourceUpdateDaemon()
        {
            Thread thread = new Thread(ResourceUpdateThreadProcedure);
			thread.Start();
        }

        internal static void StopResourceUpdateDaemon()
        {
            _waitHandle.Set();
        }

        #endregion
    }
}
