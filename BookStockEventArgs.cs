namespace BookStoreApp.Domain;

public class BookStockEventArgs : EventArgs
{
    public Book Book { get; }

    public BookStockEventArgs(Book book)
    {
        Book = book;
    }
}
