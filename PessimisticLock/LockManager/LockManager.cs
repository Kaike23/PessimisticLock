using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace LockManager
{
    using Infrastructure.Session;
    using Session;

    //public class LockManager
    //{
    //    private static readonly LockManager manager = new LockManager();

    //    public static LockManager Manager { get { return manager; } }

    //    private Dictionary<Guid, List<LockItem>> locksSet;

    //    private LockManager()
    //    {
    //        this.locksSet = new Dictionary<Guid, List<LockItem>>();
    //    }

    //    public bool GetReadLock(Guid entityId)
    //    {
    //        List<LockItem> lockItems = new List<LockItem>();
    //        var hasLocks = VerifyLock(entityId, LockType.Write, out lockItems);
    //        PersistLock(entityId, lockItems, hasLocks, LockType.Read);
    //        return true;
    //    }

    //    public bool GetWriteLock(Guid entityId)
    //    {


    //        return true;
    //    }

    //    private bool VerifyLock(Guid entityId, LockType lockType, out List<LockItem> lockItems)
    //    {
    //        var hasLocks = locksSet.TryGetValue(entityId, out lockItems);
    //        if (hasLocks)
    //        {
    //            if (lockItems[0].Type == lockType)
    //            {
    //                var session = GetSession();
    //                Console.WriteLine(string.Format("{0} can't get read lock for entity {1}. {2} has {3} lock", session.Name, entityId, lockItems[0].Session.Name), lockType.ToString());
    //                return false;
    //            }
    //        }
    //        return true;
    //    }

    //    private void PersistLock(Guid entityId, List<LockItem> locks, bool hasLocks, LockType lockType)
    //    {
    //        var session = GetSession();
    //        using (var transaction = session.DbInfo.Connection.BeginTransaction(IsolationLevel.Serializable))
    //        {
    //            try
    //            {
    //                var query = string.Format("INSERT INTO Lock VALUES({0}, {1}, {2})", session.Id, entityId, lockType);
    //                var command = new SqlCommand(query, (SqlConnection)session.DbInfo.Connection, (SqlTransaction)transaction);
    //                command.ExecuteNonQuery();
    //                transaction.Commit();
    //                locks.Add(new LockItem(session, lockType));
    //                if (!hasLocks)
    //                    locksSet.Add(entityId, locks);
    //            }
    //            catch (SqlException ex)
    //            {
    //                transaction.Rollback();
    //                throw new Exception("Unexpected SqlException ocurred.", ex);
    //            }
    //        }
    //    }

    //    private ISession GetSession()
    //    {
    //        var sessionManager = SessionManager.Manager;
    //        return sessionManager.GetSession(sessionManager.Current);
    //    }
    //}

    public class LockManager
    {
        private Dictionary<Guid, LockType> locks;

        private LockManager()
        {
            this.locks = new Dictionary<Guid, LockType>();
        }

        public bool GetReadLock(Guid entityId)
        {
            List<LockItem> lockItems = new List<LockItem>();
            var hasLocks = VerifyLock(entityId, LockType.Write, out lockItems);
            PersistLock(entityId, lockItems, hasLocks, LockType.Read);
            return true;
        }

        public bool GetWriteLock(Guid entityId)
        {


            return true;
        }

        private bool VerifyLock(Guid entityId, LockType lockType, out List<LockItem> lockItems)
        {
            var hasLocks = locksSet.TryGetValue(entityId, out lockItems);
            if (hasLocks)
            {
                if (lockItems[0].Type == lockType)
                {
                    var session = GetSession();
                    Console.WriteLine(string.Format("{0} can't get read lock for entity {1}. {2} has {3} lock", session.Name, entityId, lockItems[0].Session.Name), lockType.ToString());
                    return false;
                }
            }
            return true;
        }

        private void PersistLock(Guid entityId, List<LockItem> locks, bool hasLocks, LockType lockType)
        {
            var session = GetSession();
            using (var transaction = session.DbInfo.Connection.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    var query = string.Format("INSERT INTO Lock VALUES({0}, {1}, {2})", session.Id, entityId, lockType);
                    var command = new SqlCommand(query, (SqlConnection)session.DbInfo.Connection, (SqlTransaction)transaction);
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    locks.Add(new LockItem(session, lockType));
                    if (!hasLocks)
                        locksSet.Add(entityId, locks);
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    throw new Exception("Unexpected SqlException ocurred.", ex);
                }
            }
        }

        private ISession GetSession()
        {
            var sessionManager = SessionManager.Manager;
            return sessionManager.GetSession(sessionManager.Current);
        }
    }
}
