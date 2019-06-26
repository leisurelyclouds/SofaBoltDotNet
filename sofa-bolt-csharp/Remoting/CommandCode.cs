namespace Remoting
{
    /// <summary>
    /// Remoting command code stands for a specific remoting command, and every kind of command has its own code.
    /// </summary>
    public interface CommandCode
    {
        /// <summary>
        /// value 0 is occupied by heartbeat, don't use value 0 for other commands
        /// </summary>
        /// <returns> the short value of the code </returns>
        short value();
    }

    public static class CommandCode_Fields
    {
        public const short HEARTBEAT_VALUE = 0;
    }
}