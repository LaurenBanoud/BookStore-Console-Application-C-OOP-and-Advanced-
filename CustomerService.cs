using BookStoreApp.Domain;
using BookStoreApp.Repositories;

namespace BookStoreApp.Services;

public class CustomerService
{
    private readonly IRepository<Customer> _customers;

    public CustomerService(IRepository<Customer> customers)
    {
        _customers = customers;
    }

    public Customer Register(string name, string email)
    {
        var customer = new Customer(name, email);
        _customers.Add(customer);
        return customer;
    }

    public IReadOnlyList<Customer> GetAll() => _customers.GetAll();

    public Customer? GetById(Guid id) => _customers.GetById(id);

    public Customer? FindByEmail(string email) =>
        _customers.Find(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
}
