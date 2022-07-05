using iText.Layout.Element;
using System.Collections.Generic;

namespace eauction.Infrastructure
{
    public class GenericPagedList<T>
    {
        public IPager Pager { get; set; }
        public List<T> ListOfItems { get; set; }
    }
}
