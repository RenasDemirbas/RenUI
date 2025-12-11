namespace RenUI.Core.Events;

public sealed class EventDispatcher<TEventArgs> where TEventArgs : UIEventArgs
{
    private readonly List<Action<TEventArgs>> _handlers = new();
    private readonly object _lock = new();

    public void Subscribe(Action<TEventArgs> handler)
    {
        lock (_lock)
        {
            if (!_handlers.Contains(handler))
            {
                _handlers.Add(handler);
            }
        }
    }

    public void Unsubscribe(Action<TEventArgs> handler)
    {
        lock (_lock)
        {
            _handlers.Remove(handler);
        }
    }

    public void Dispatch(TEventArgs args)
    {
        List<Action<TEventArgs>> handlersCopy;
        lock (_lock)
        {
            handlersCopy = new List<Action<TEventArgs>>(_handlers);
        }

        foreach (var handler in handlersCopy)
        {
            if (args.Handled) break;
            handler.Invoke(args);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _handlers.Clear();
        }
    }
}
