using System;

namespace Model.Customers
{
    using Infrastructure.Domain;
    using Session;

    public class Customer : EntityBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName { get { return string.Format("{0} {1}", FirstName, LastName); } }

        public Customer(Guid id, string firstName, string lastName)
            : base(id)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public static Customer Create(string firstName, string lastName)
        {
            var sessionManager = SessionManager.Manager;
            var session = sessionManager.GetSession(sessionManager.Current);

            var customer = new Customer(Guid.NewGuid(), firstName, lastName);
            customer.CreatedBy = session.Name;
            customer.Created = DateTime.Now;
            customer.SetSystemFields(0, customer.CreatedBy, customer.Created);
            return customer;
        }
    }
}