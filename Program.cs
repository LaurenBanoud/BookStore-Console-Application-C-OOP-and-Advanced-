using BookStoreApp.Domain;
using BookStoreApp.Extensions;
using BookStoreApp.Persistence;
using BookStoreApp.Repositories;
using BookStoreApp.Rules;
using BookStoreApp.Services;
using BookStoreApp.UI;
using BookStoreApp.Validation;

var bookRepository = new InMemoryRepository<Book>();
var customerRepository = new InMemoryRepository<Customer>();
var purchaseRepository = new InMemoryRepository<Purchase>();

var catalog = new BookCatalogService(bookRepository);
var customers = new CustomerService(customerRepository);
var sales = new SalesService(purchaseRepository, bookRepository);

var dataFile = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "bookstore-data.json");
dataFile = Path.GetFullPath(dataFile);
var store = new BookStoreDataStore(dataFile);

// Requirement 10: subscribe to the central alert hub. Any number of other
// parts of the system could add their own subscription the same way.
StockAlertCenter.BookOutOfStock += (_, args) =>
    WriteWarning($"Heads up: '{args.Book.Title}' just sold out (0 left in stock).");

await LoadOrSeedAsync();

RunMenu();

await SaveAsync();
Console.WriteLine();
Console.WriteLine("Data saved. Goodbye!");

// ---------------------------------------------------------------------
// Startup / shutdown
// ---------------------------------------------------------------------

async Task LoadOrSeedAsync()
{
    var data = await store.LoadAsync();

    if (data.Books.Count == 0 && data.Customers.Count == 0 && data.Purchases.Count == 0)
    {
        SeedSampleData();
        Console.WriteLine("No saved data found - starting with a small sample catalog.");
        return;
    }

    bookRepository.ReplaceAll(data.Books);
    customerRepository.ReplaceAll(data.Customers);
    purchaseRepository.ReplaceAll(data.Purchases);
    catalog.RegisterAllForAlerts();

    Console.WriteLine($"Loaded {data.Books.Count} book(s), {data.Customers.Count} customer(s), " +
                       $"{data.Purchases.Count} purchase(s) from {Path.GetFileName(dataFile)}.");
}

async Task SaveAsync()
{
    await store.SaveAsync(bookRepository, customerRepository, purchaseRepository);
}

void SeedSampleData()
{
    catalog.AddBook(new Paperback("Clean Code", "Robert C. Martin", "Software", 32.50m, 8, 464));
    catalog.AddBook(new Ebook("Project Hail Mary", "Andy Weir", "Sci-Fi", 14.99m, 25, "EPUB", 4.2));
    catalog.AddBook(new Audiobook("Atomic Habits", "James Clear", "Self-Help", 19.99m, 5, 330, "James Clear"));
    catalog.AddBook(new Paperback("1984", "George Orwell", "Fiction", 9.99m, 12, 328));
}

// ---------------------------------------------------------------------
// Menu loop
// ---------------------------------------------------------------------

void RunMenu()
{
    var running = true;
    while (running)
    {
        Console.WriteLine();
        Console.WriteLine("==================== BookStore Menu ====================");
        Console.WriteLine(" 1. List all books");
        Console.WriteLine(" 2. Add a book");
        Console.WriteLine(" 3. Remove a book");
        Console.WriteLine(" 4. Search books (title/author)");
        Console.WriteLine(" 5. Filter books (category / author / price range)");
        Console.WriteLine(" 6. Apply a pricing rule to books");
        Console.WriteLine(" 7. Register a customer");
        Console.WriteLine(" 8. List customers");
        Console.WriteLine(" 9. Record a purchase");
        Console.WriteLine("10. List purchases");
        Console.WriteLine("11. Reports (revenue / best seller / top customer)");
        Console.WriteLine("12. Save now");
        Console.WriteLine("13. Demo: simulate concurrent purchases on one book");
        Console.WriteLine(" 0. Exit");
        Console.WriteLine("==========================================================");

        var choice = ConsoleInput.ReadMenuChoice("Choose an option: ", 0, 13);

        SafeExecute(() =>
        {
            switch (choice)
            {
                case 1: ListBooks(catalog.GetAll()); break;
                case 2: AddBookFlow(); break;
                case 3: RemoveBookFlow(); break;
                case 4: SearchBooksFlow(); break;
                case 5: FilterBooksFlow(); break;
                case 6: ApplyPricingRuleFlow(); break;
                case 7: RegisterCustomerFlow(); break;
                case 8: ListCustomers(); break;
                case 9: RecordPurchaseFlow(); break;
                case 10: ListPurchases(); break;
                case 11: ShowReports(); break;
                case 12: SaveAsync().GetAwaiter().GetResult(); Console.WriteLine("  Saved."); break;
                case 13: ConcurrentPurchaseDemo(); break;
                case 0: running = false; break;
            }
        });
    }
}

/// <summary>
/// Wraps every menu action so bad input or a broken business rule prints a
/// clear message instead of ever crashing the app (requirement 4).
/// </summary>
void SafeExecute(Action action)
{
    try
    {
        action();
    }
    catch (ValidationException ex)
    {
        WriteError($"Invalid input: {ex.Message}");
    }
    catch (InsufficientStockException ex)
    {
        WriteError(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        WriteError(ex.Message);
    }
    catch (Exception ex)
    {
        WriteError($"Something went wrong: {ex.Message}");
    }
}

// ---------------------------------------------------------------------
// Book flows
// ---------------------------------------------------------------------

void ListBooks(IEnumerable<Book> books)
{
    var list = books.ToList();
    Console.WriteLine();
    if (list.Count == 0)
    {
        Console.WriteLine("No books to show.");
        return;
    }

    foreach (var book in list)
        Console.WriteLine($"  [{book.Id}] {book}");
}

void AddBookFlow()
{
    Console.WriteLine();
    Console.WriteLine("Choose a format:");
    Console.WriteLine("  1) Paperback");
    Console.WriteLine("  2) Ebook");
    Console.WriteLine("  3) Audiobook");
    var format = ConsoleInput.ReadMenuChoice("Format: ", 1, 3);

    var title = ConsoleInput.ReadRequiredString("Title: ");
    var author = ConsoleInput.ReadRequiredString("Author: ");
    var category = ConsoleInput.ReadRequiredString("Category: ").ToTitleCase();
    var price = ConsoleInput.ReadPositiveDecimal("Price: ");
    var stockInt = ConsoleInput.ReadNonNegativeInt("Stock quantity: ");

    Book book = format switch
    {
        1 => new Paperback(title, author, category, price, stockInt,
                ConsoleInput.ReadPositiveInt("Page count: ")),
        2 => new Ebook(title, author, category, price, stockInt,
                ConsoleInput.ReadRequiredString("File format (e.g. EPUB, PDF): "),
                ConsoleInput.ReadPositiveDouble("File size (MB): ")),
        3 => new Audiobook(title, author, category, price, stockInt,
                ConsoleInput.ReadPositiveInt("Duration (minutes): "),
                ConsoleInput.ReadRequiredString("Narrator: ")),
        _ => throw new InvalidOperationException("Unsupported format."),
    };

    catalog.AddBook(book);
    Console.WriteLine();
    Console.WriteLine($"  Added: {book}");
    Console.WriteLine($"  ID: {book.Id}");
}

void RemoveBookFlow()
{
    ListBooks(catalog.GetAll());
    var id = ConsoleInput.ReadGuid("Enter the ID of the book to remove: ");
    if (catalog.RemoveBook(id))
        Console.WriteLine("  Book removed.");
    else
        Console.WriteLine("  No book found with that ID.");
}

void SearchBooksFlow()
{
    var term = ConsoleInput.ReadRequiredString("Search by title or author: ");
    ListBooks(catalog.Search(term));
}

void FilterBooksFlow()
{
    Console.WriteLine();
    Console.WriteLine("Filter by:");
    Console.WriteLine("  1) Category");
    Console.WriteLine("  2) Author");
    Console.WriteLine("  3) Price range");
    var choice = ConsoleInput.ReadMenuChoice("Choice: ", 1, 3);

    IEnumerable<Book> results = choice switch
    {
        1 => catalog.FilterByCategory(ConsoleInput.ReadRequiredString("Category: ")),
        2 => catalog.FilterByAuthor(ConsoleInput.ReadRequiredString("Author: ")),
        3 => catalog.FilterByPriceRange(
                ConsoleInput.ReadNonNegativeDecimal("Minimum price: "),
                ConsoleInput.ReadNonNegativeDecimal("Maximum price: ")),
        _ => Enumerable.Empty<Book>(),
    };

    ListBooks(results);
}

void ApplyPricingRuleFlow()
{
    Console.WriteLine();
    Console.WriteLine("Apply the rule to:");
    Console.WriteLine("  1) All books");
    Console.WriteLine("  2) One category");
    var scopeChoice = ConsoleInput.ReadMenuChoice("Choice: ", 1, 2);

    var targetBooks = scopeChoice == 2
        ? catalog.FilterByCategory(ConsoleInput.ReadRequiredString("Category: ")).ToList()
        : catalog.GetAll().ToList();

    if (targetBooks.Count == 0)
    {
        Console.WriteLine("  No books match that scope.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Choose a rule:");
    Console.WriteLine("  1) Percentage discount");
    Console.WriteLine("  2) Flat price increase");
    Console.WriteLine("  3) Set a minimum price");
    var ruleChoice = ConsoleInput.ReadMenuChoice("Choice: ", 1, 3);

    Action<Book> rule = ruleChoice switch
    {
        1 => PricingRules.PercentageDiscount(ConsoleInput.ReadPositiveDecimal("Discount percent (e.g. 10): ")),
        2 => PricingRules.FlatPriceIncrease(ConsoleInput.ReadPositiveDecimal("Increase amount: ")),
        3 => PricingRules.SetMinimumPrice(ConsoleInput.ReadPositiveDecimal("Minimum price: ")),
        _ => throw new InvalidOperationException("Unsupported rule."),
    };

    catalog.ApplyRuleToBooks(targetBooks, rule);
    Console.WriteLine($"  Rule applied to {targetBooks.Count} book(s).");
    ListBooks(targetBooks);
}

// ---------------------------------------------------------------------
// Customer flows
// ---------------------------------------------------------------------

void RegisterCustomerFlow()
{
    var name = ConsoleInput.ReadRequiredString("Customer name: ");
    var email = ConsoleInput.ReadEmail("Email: ");

    if (customers.FindByEmail(email) is not null)
    {
        Console.WriteLine("  A customer with that email is already registered.");
        return;
    }

    var customer = customers.Register(name, email);
    Console.WriteLine($"  Registered: {customer}");
    Console.WriteLine($"  ID: {customer.Id}");
}

void ListCustomers()
{
    var list = customers.GetAll();
    Console.WriteLine();
    if (list.Count == 0)
    {
        Console.WriteLine("No customers registered yet.");
        return;
    }

    foreach (var customer in list)
        Console.WriteLine($"  [{customer.Id}] {customer}");
}

// ---------------------------------------------------------------------
// Purchase flows
// ---------------------------------------------------------------------

void RecordPurchaseFlow()
{
    ListCustomers();
    Customer customer;
    if (customers.GetAll().Count == 0 || ConsoleInput.ReadYesNo("Register a new customer instead"))
    {
        var name = ConsoleInput.ReadRequiredString("Customer name: ");
        var email = ConsoleInput.ReadEmail("Email: ");
        customer = customers.FindByEmail(email) ?? customers.Register(name, email);
    }
    else
    {
        var id = ConsoleInput.ReadGuid("Enter the customer ID: ");
        customer = customers.GetById(id) ?? throw new ValidationException("No customer found with that ID.");
    }

    var basket = new Dictionary<Guid, int>();
    bool addingMore = true;
    while (addingMore)
    {
        ListBooks(catalog.GetAll().InStockOnly());
        var bookId = ConsoleInput.ReadGuid("Enter a book ID to add to the purchase: ");
        var book = catalog.GetById(bookId) ?? throw new ValidationException("No book found with that ID.");
        var quantity = ConsoleInput.ReadPositiveInt($"Quantity of '{book.Title}': ");

        basket[bookId] = basket.TryGetValue(bookId, out var existing) ? existing + quantity : quantity;
        Console.WriteLine($"  Added {quantity} x '{book.Title}' to the basket.");

        addingMore = ConsoleInput.ReadYesNo("Add another book to this purchase");
    }

    var purchase = sales.RecordPurchase(customer, basket);
    Console.WriteLine();
    Console.WriteLine($"  Purchase recorded for {customer.Name}. Total: {purchase.Total.ToCurrency()}");
    foreach (var item in purchase.Items)
        Console.WriteLine($"    - {item.Quantity} x {item.BookTitle} @ {item.UnitPriceAtSale.ToCurrency()} = {item.LineTotal.ToCurrency()}");
}

void ListPurchases()
{
    var list = sales.GetAll();
    Console.WriteLine();
    if (list.Count == 0)
    {
        Console.WriteLine("No purchases recorded yet.");
        return;
    }

    foreach (var purchase in list.OrderByDescending(p => p.PurchasedAtUtc))
    {
        Console.WriteLine($"  [{purchase.Id}] {purchase.CustomerName} - {purchase.PurchasedAtUtc:u} - Total: {purchase.Total.ToCurrency()}");
        foreach (var item in purchase.Items)
            Console.WriteLine($"      {item.Quantity} x {item.BookTitle} @ {item.UnitPriceAtSale.ToCurrency()}");
    }
}

void ShowReports()
{
    Console.WriteLine();
    Console.WriteLine($"Total revenue: {sales.TotalRevenue().ToCurrency()}");

    var best = sales.BestSellingBook();
    Console.WriteLine(best is null
        ? "Best-selling book: no sales yet."
        : $"Best-selling book: '{best.Value.Title}' ({best.Value.CopiesSold} copies sold)");

    var top = sales.TopCustomer();
    Console.WriteLine(top is null
        ? "Top customer: no sales yet."
        : $"Top customer: {top.Value.Name} ({top.Value.TotalSpent.ToCurrency()} spent)");
}

// ---------------------------------------------------------------------
// Bonus requirement 13 demo: concurrent purchases on the same book
// ---------------------------------------------------------------------

void ConcurrentPurchaseDemo()
{
    ListBooks(catalog.GetAll());
    var bookId = ConsoleInput.ReadGuid("Pick a book ID to hammer with concurrent purchases: ");
    var book = catalog.GetById(bookId) ?? throw new ValidationException("No book found with that ID.");
    var attempts = ConsoleInput.ReadPositiveInt("How many simultaneous 1-copy purchases to attempt: ");

    Console.WriteLine($"  Starting stock for '{book.Title}': {book.Stock}");
    var demoCustomer = customers.FindByEmail("demo@bookstore.local") ?? customers.Register("Concurrency Demo", "demo@bookstore.local");

    var tasks = Enumerable.Range(0, attempts).Select(_ => Task.Run(() =>
    {
        try
        {
            sales.RecordPurchase(demoCustomer, new Dictionary<Guid, int> { [bookId] = 1 });
            return "ok";
        }
        catch (InsufficientStockException)
        {
            return "out of stock";
        }
    })).ToArray();

    Task.WaitAll(tasks);

    var succeeded = tasks.Count(t => t.Result == "ok");
    Console.WriteLine($"  {succeeded} of {attempts} purchases succeeded.");
    Console.WriteLine($"  Final stock for '{book.Title}': {book.Stock} (never negative, even under concurrency).");
}

// ---------------------------------------------------------------------
// Console output helpers
// ---------------------------------------------------------------------

void WriteError(string message)
{
    var previous = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  x {message}");
    Console.ForegroundColor = previous;
}

void WriteWarning(string message)
{
    var previous = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"  ! {message}");
    Console.ForegroundColor = previous;
}
