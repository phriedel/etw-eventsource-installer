namespace EventSourceInstallerLib
{
    public class StatusEventMessage
    {
        public StatusEventMessage(string manifestKey, string message)
        {
            ManifestKey = manifestKey;
            Message = message;
        }
        public string ManifestKey { get; private set; }
        public string Message { get; private set; }
    }
}
