namespace Yritused.Models.Viewmodels
{
    public class YritusedListViewModel
    {
        public IEnumerable<Yritus>? Yritused { get; set; }
        public PagingModel? PagingInfo { get; set; }
        public string? Path { get; set; }
        public string? orderBy { get; set; }
        public string? filterField { get; set; }
        public string? filterValue { get; set; }
    }
}
