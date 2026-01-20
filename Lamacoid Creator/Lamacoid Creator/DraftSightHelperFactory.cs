namespace Lamacoid_Creator
{
    /// <summary>
    /// Factory class for creating the appropriate DraftSight helper instance
    /// based on the test mode configuration.
    /// </summary>
    public static class DraftSightHelperFactory
    {
        /// <summary>
        /// Creates and returns the appropriate IDraftSightHelper implementation
        /// based on the Constants.UseTestMode setting.
        /// </summary>
        /// <returns>IDraftSightHelper instance (either real or mock)</returns>
        public static IDraftSightHelper Create()
        {
            if (Constants.UseTestMode)
            {
                Logger.Info("Test mode enabled - using MockDraftSightHelper");
                return new MockDraftSightHelper();
            }
            else
            {
                Logger.Info("Production mode - using real DraftSightHelper");
                return new DraftSightHelper();
            }
        }
    }
}
