
namespace LockManager
{
    using Infrastructure.Session;

    public enum LockType
    {
        Read = 0,
        Write = 1
    }

    public class LockItem
    {
        public LockItem(ISession session, LockType type)
        {
            Session = session;
            Type = type;
        }

        public ISession Session { get; private set; }
        public LockType Type { get; private set; }
    }
}
