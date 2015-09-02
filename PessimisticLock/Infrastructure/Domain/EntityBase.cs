using System;

namespace Infrastructure.Domain
{
    public abstract class EntityBase : IEntity
    {
        public EntityBase(Guid id)
        {
            Id = id;
        }

        public void SetSystemFields(long version, string modifiedBy, DateTime modified)
        {
            Version = version;
            Modified = modified;
            ModifiedBy = modifiedBy;
        }

        public Guid Id { get; private set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Modified { get; private set; }
        public string ModifiedBy { get; private set; }
        public long Version { get; private set; }
    }
}
