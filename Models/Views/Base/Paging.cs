namespace Models.Views.Base
{
    public class Paging
    {
        public int ItemsCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
