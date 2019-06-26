namespace Remoting
{
    /// <summary>
    /// Status of the response.
    /// </summary>
    public enum ResponseStatus : short
    {
        SUCCESS,
        ERROR,
        SERVER_EXCEPTION,
        UNKNOWN,
        SERVER_THREADPOOL_BUSY,
        ERROR_COMM,
        NO_PROCESSOR,
        TIMEOUT,
        CLIENT_SEND_ERROR,
        CODEC_EXCEPTION,
        CONNECTION_CLOSED,
        SERVER_SERIAL_EXCEPTION,
        SERVER_DESERIAL_EXCEPTION
    }
}