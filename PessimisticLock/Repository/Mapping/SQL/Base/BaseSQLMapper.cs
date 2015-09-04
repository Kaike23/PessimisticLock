using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Repository.Mapping.SQL.Base
{
    using Infrastructure.Domain;
    using Infrastructure.Mapping;
    using Infrastructure.Session;
    using Session;

    public abstract class BaseSQLMapper<T> : IDataMapper<T>
        where T : EntityBase
    {
        public BaseSQLMapper()
        { }

        protected Dictionary<Guid, T> loadedMap = new Dictionary<Guid, T>();
        protected abstract string FindStatement { get; }
        protected abstract string InsertStatement { get; }
        protected abstract string UpdateStatement { get; }
        protected abstract string DeleteStatement { get; }

        #region IDataMapper

        public T Find(Guid id)
        {
            T result;
            if (loadedMap.TryGetValue(id, out result))
                return result;

            try
            {
                using (var sqlCommand = new SqlCommand(FindStatement, DBConnection))
                {
                    sqlCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                    var reader = sqlCommand.ExecuteReader();
                    result = Load(reader);
                    reader.Close();
                }
                return result;
            }
            catch (SqlException e)
            {
                throw new ApplicationException(e.Message, e);
            }
        }
        public List<T> FindMany(IStatementSource source)
        {
            try
            {
                var sqlCommand = new SqlCommand(source.Query, DBConnection);
                sqlCommand.Parameters.AddRange(source.Parameters.ToArray());
                var reader = sqlCommand.ExecuteReader();
                var result = LoadAll(reader);
                reader.Close();
                return result;
            }
            catch (SqlException e)
            {
                throw new ApplicationException(e.Message, e);
            }
        }
        public Guid Insert(T entity)
        {
            try
            {
                var insertCommand = new SqlCommand(InsertStatement, DBConnection, DBTransaction);
                insertCommand.Parameters.AddWithValue("@Id", entity.Id);
                insertCommand.Parameters.AddWithValue("@Version", entity.Version);
                insertCommand.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy);
                insertCommand.Parameters.AddWithValue("@Created", entity.Created);
                insertCommand.Parameters.AddWithValue("@ModifiedBy", entity.ModifiedBy);
                insertCommand.Parameters.AddWithValue("@Modified", entity.Modified);
                DoInsert(entity, insertCommand);
                var affectedRows = insertCommand.ExecuteNonQuery();
                loadedMap.Add(entity.Id, entity);
                return entity.Id;
            }
            catch (SqlException e)
            {
                throw new ApplicationException(e.Message, e);
            }
        }
        public void Update(T entity)
        {
            var sessionName = GetSession().Name;
            var newVersion = entity.Version + 1;
            var modified = DateTime.Now;

            try
            {
                var updateCommand = new SqlCommand(UpdateStatement, DBConnection, DBTransaction);
                updateCommand.Parameters.AddWithValue("@Id", entity.Id);
                updateCommand.Parameters.AddWithValue("@Version", entity.Version);
                updateCommand.Parameters.AddWithValue("@ModifiedBy", sessionName);
                updateCommand.Parameters.AddWithValue("@Modified", modified);
                DoUpdate(entity, updateCommand);
                var rowCount = updateCommand.ExecuteNonQuery();
                if(rowCount == 0)
                {
                    throw new Exception(string.Format("Concurrency exception on {0}", entity.Id));
                }
                entity.SetSystemFields(newVersion, sessionName, modified);
            }
            catch (SqlException e)
            {
                throw new ApplicationException(e.Message, e);
            }
        }
        public void Delete(T entity)
        {
            try
            {
                var deleteCommand = new SqlCommand(DeleteStatement, DBConnection, DBTransaction);
                deleteCommand.Parameters.AddWithValue("@Id", entity.Id);
                deleteCommand.Parameters.AddWithValue("@Version", entity.Version);
                deleteCommand.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                throw new ApplicationException(e.Message, e);
            }
        }

        #endregion

        protected T Load(SqlDataReader reader)
        {
            if (!reader.HasRows) return default(T);
            reader.Read();
            var id = reader.GetGuid(0);
            if (loadedMap.ContainsKey(id)) return loadedMap[id];
            var resultEntity = DoLoad(id, reader);
            loadedMap.Add(id, resultEntity);
            return resultEntity;
        }
        protected List<T> LoadAll(SqlDataReader reader)
        {
            var resultEntities = new List<T>();
            if (reader.HasRows)
            {
                while (reader.Read())
                    resultEntities.Add(Load(reader));
            }
            return resultEntities;
        }

        protected abstract T DoLoad(Guid id, SqlDataReader reader);
        protected abstract void DoInsert(T entity, SqlCommand insertCommand);
        protected abstract void DoUpdate(T entity, SqlCommand updateCommand);

        private SqlConnection DBConnection { get { return (SqlConnection)GetSession().DbInfo.Connection; } }
        private SqlTransaction DBTransaction { get { return (SqlTransaction)GetSession().DbInfo.Transaction; } }

        private ISession GetSession()
        {
            var sessionManager = SessionManager.Manager;
            return sessionManager.GetSession(sessionManager.Current);
        }
    }
}
