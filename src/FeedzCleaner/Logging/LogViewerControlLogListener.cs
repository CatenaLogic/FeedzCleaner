namespace FeedzCleaner.Logging
{
    using Catel.Logging;

    public class LogViewerControlLogListener : LogListenerBase
    {
        public LogViewerControlLogListener()
        {
            IgnoreCatelLogging = true;
        }
    }
}
