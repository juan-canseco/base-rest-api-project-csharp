namespace Application.Shared.Models
{
    public class PagedList<TEntity> where TEntity : class
    {
        public PagedList(IEnumerable<TEntity> items, long count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
            RowsCount = count;
        }

        // Required for mapping
        private PagedList() 
        {
            Items = default!;
        }

        public IEnumerable<TEntity> Items { get; private set; }
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public long RowsCount { get; private set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
