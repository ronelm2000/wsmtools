namespace Montage.Card.API.Helpers;

public static class MultiAssert
{
    public static void AllAreTrue(IEnumerable<Action> assertions, Action<string>? finalAssertMethod = null, string message = "")
    {
        var exceptions = new List<Exception>();
        var list = new List<string>();
        foreach (var assertion in assertions)
        {
            try
            {
                assertion();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                list.Add(ex.Message);
            }
        }
        if (exceptions.Count > 0)
        {
            finalAssertMethod?.Invoke(message + ":" + Environment.NewLine + string.Join(Environment.NewLine, list));
            throw new AggregateException(message, exceptions);
        }
    }
}
