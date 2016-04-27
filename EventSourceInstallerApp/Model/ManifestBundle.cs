namespace EventSourceInstallerApp.Model
{
    public class ManifestBundle
    {
        #region Properties

        public string Manifest { get; set; }
        public string Dll { get; set; }
        public string SourcePath { get; set; }
        public string StatusMessage { get; set; }

        #endregion
    }
}
