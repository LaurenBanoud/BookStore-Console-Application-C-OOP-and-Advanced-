# BookStore Console Application

A .NET 8 console app for managing a bookstore's catalog, customers, and
sales — companion project to the `bookstore.sql` database design (Task 1),
now built as an in-memory, object-oriented C# application instead of a
relational schema.

## How to run it

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.

> If you have a newer SDK only (e.g. .NET 10) and don't want to install .NET
> 8 separately, open `src/BookStoreApp/BookStoreApp.csproj` and change
> `<TargetFramework>net8.0</TargetFramework>` to match your installed SDK
> (e.g. `net10.0`). No other code changes are needed.

```bash
git clone <your-repo-url>
cd BookStoreApp
dotnet run --project src/BookStoreApp
```

On first run there's no saved data, so the app seeds a small sample
catalog (4 books) automatically. On every subsequent run it reloads
whatever you saved last time from `bookstore-data.json` (created next to
the project folder). The menu is numbered — type a number and press Enter;
any bad input is rejected with a message and re-prompted, it never crashes
the app.

## Project layout

```
src/BookStoreApp/
  Domain/         Book (abstract) + Paperback/Ebook/Audiobook, Customer,
                   Purchase, PurchaseItem, IEntity, custom exceptions
  Repositories/    IRepository<T> + InMemoryRepository<T> (generic, thread-safe)
  Services/        BookCatalogService, CustomerService, SalesService,
                   StockAlertCenter (event hub)
  Rules/           PricingRules (discount / increase / minimum price)
  Extensions/      StringExtensions, NumberExtensions, BookQueryExtensions
  Validation/      Validator, ValidationException
  Persistence/     BookStoreDataStore (async JSON save/load)
  UI/              ConsoleInput (crash-proof input readers)
  Program.cs       Menu wiring
```

## Design decisions & how each requirement is met

| # | Requirement | How it's handled |
|---|---|---|
| 1 | Domain model | `Book` is abstract; `Customer` and `Purchase`/`PurchaseItem` model the sales side. Everything implements `IEntity` so it can live in the generic repository. |
| 2 | Add/remove/search/list books | `BookCatalogService` + menu options 1–4. |
| 3 | Customers & multi-book purchases | `CustomerService.Register`; `SalesService.RecordPurchase` takes a `Dictionary<BookId, Quantity>` so one purchase can span any number of different books. |
| 4 | Validate all input, never crash | Every domain constructor validates through `Validator` and throws `ValidationException`. `ConsoleInput` loops on bad console input instead of throwing. `Program.SafeExecute` wraps every menu action in a try/catch that prints a friendly message. |
| 5 | Revenue / best seller / top customer | `SalesService.TotalRevenue`, `BestSellingBook`, `TopCustomer` — menu option 11. |
| 6 | Filter by category/author/price | `BookQueryExtensions.ByCategory/ByAuthor/InPriceRange`, wired to menu option 5. |
| 7 | New formats without touching existing code | `Book` is abstract; `Paperback`, `Ebook`, `Audiobook` extend it. The repository, services, filters, and reports only ever touch the `Book` base type, so adding e.g. `LargePrint : Book` needs zero changes to any of them (Open/Closed Principle). The only place aware of concrete types is the "add a book" menu, which prompts for format-specific fields — extending it means adding one new `case`, never editing an existing one. |
| 8 | Generic in-memory repository | `IRepository<T> where T : IEntity` + `InMemoryRepository<T>` backed by a `ConcurrentDictionary<Guid, T>`. The same class stores books, customers, and purchases. |
| 9 | Apply any rule to a list of books | `BookQueryExtensions.ApplyRule(this IEnumerable<Book>, Action<Book> rule)` runs any delegate against a list. `Rules/PricingRules.cs` ships discount/increase/minimum-price rules as examples; menu option 6 lets you build and apply one. |
| 10 | Notify on out-of-stock | `Book` raises its own `OutOfStock` event; `StockAlertCenter` is a static hub any part of the app can subscribe to without holding a reference to every book. `Program.cs` subscribes once to print a warning — a future email/logging module could subscribe the same way with no changes elsewhere. |
| 11 | Extension methods on built-in types | `string.IsValidEmail()`, `string.ToTitleCase()`, `string.Truncate()`, `decimal.ToCurrency()`, `decimal.ApplyPercentageDiscount()`, `decimal.IsBetween()`. |
| 12 (bonus) | Async save/load | `BookStoreDataStore.SaveAsync/LoadAsync` uses `JsonSerializer.SerializeAsync`/`DeserializeAsync` over a file stream. `Book`'s `[JsonPolymorphic]`/`[JsonDerivedType]` attributes let a single `List<Book>` round-trip mixed formats correctly. Data auto-saves on exit and can be saved anytime via menu option 12. |
| 13 (bonus) | Concurrent purchases stay consistent | Each `Book` guards its own `Stock` with an internal `lock`, so `DecreaseStock` is atomic per book — concurrent purchases of *different* books run in parallel, and concurrent purchases of the *same* book are serialized just long enough to keep the count correct and never negative. `SalesService.RecordPurchase` also rolls back any stock it already decremented if a later item in the same purchase fails. Menu option 13 fires several purchases at once with `Task.Run` so you can see this hold up live. |

## Sample interaction

```
==================== BookStore Menu ====================
 1. List all books
 2. Add a book
 3. Remove a book
 4. Search books (title/author)
 5. Filter books (category / author / price range)
 6. Apply a pricing rule to books
 7. Register a customer
 8. List customers
 9. Record a purchase
10. List purchases
11. Reports (revenue / best seller / top customer)
12. Save now
13. Demo: simulate concurrent purchases on one book
 0. Exit
==========================================================
Choose an option: 1

  [3f2a...] Clean Code — Robert C. Martin [Software] (Paperback) | $32.50 | Stock: 8 | 464 pages
  [9c1e...] Project Hail Mary — Andy Weir [Sci-Fi] (Ebook) | $14.99 | Stock: 25 | EPUB, 4.2 MB
  [7ab4...] Atomic Habits — James Clear [Self-Help] (Audiobook) | $19.99 | Stock: 5 | 330 min, narrated by James Clear
  [1d90...] 1984 — George Orwell [Fiction] (Paperback) | $9.99 | Stock: 12 | 328 pages
```

Trying to buy more copies than are in stock is rejected without crashing:

```
Choose an option: 9
...
Quantity of 'Atomic Habits': 999
  x Cannot sell 999 copies of 'Atomic Habits'. Only 5 left in stock.
```

## Screenshots

Startup (loading previously saved data) and listing all books:

![Startup and listing books](screenshots/01-startup-and-list-books.png)

Adding a new book (Paperback format):

![Adding a book](screenshots/02-add-book.png)

Filtering books by category:

![Filtering books by category](screenshots/03-filter-by-category.png)

Registering a customer:

![Registering a customer](screenshots/04-register-customer.png)

Recording a multi-book purchase for a customer:

![Recording a purchase, part 1](screenshots/05-record-purchase-1.png)
![Recording a purchase, part 2](screenshots/06-record-purchase-2.png)

Searching books by title or author:

![Searching books](screenshots/07-search-books.png)

> More screenshots (out-of-stock validation, reports, concurrency demo)
> can be added the same way — drop the PNG into `screenshots/` and add an
> `![...](screenshots/filename.png)` line above.

## Publishing this to GitHub

```bash
cd BookStoreApp
git init
git add .
git commit -m "BookStore console application"
git branch -M main
git remote add origin <your-empty-github-repo-url>
git push -u origin main
```
