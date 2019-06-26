namespace Remoting
{
    /// <summary>
    /// Process connection events.
    /// </summary>
    public interface ConnectionEventProcessor
    {
        /// <summary>
        /// Process event.<br>
        /// </summary>
        /// <param name="remoteAddress"> remoting connection </param>
        /// <param name="connection"> Connection </param>
        void onEvent(string remoteAddress, Connection connection);
    }
}