namespace Montage.Card.API.Helpers;

public static class MultiAssert
{
    public static void AllAreTrue(IEnumerable<Action> assertions, string message = "")
    {
        var exceptions = new List<Exception>();
        foreach (var assertion in assertions)
        {
            try
            {
                assertion();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        if (exceptions.Count > 0)
            throw new AggregateException(message, exceptions);
    }
}
