using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Session
{
    public interface ILockManager
    {
        bool GetLock(Guid entityId, LockType lockType);
        bool ReleaseLock(Guid entityId);
        bool ReleaseAllLocks();
    }
}
