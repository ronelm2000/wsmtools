using System.Runtime.Serialization;

namespace Montage.Weiss.Tools.Impls.Services;

[Serializable]
public class ActivityLogExecutionException : Exception
{
    public ActivityLogExecutionException()
    {
    }

    public ActivityLogExecutionException(string message) : base(message)
    {
    }

    public ActivityLogExecutionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ActivityLogExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
