namespace Yritused.Models.Viewmodels
{
    public class OsavotjadListViewModel
    {
        public IEnumerable<Osavotja>? Osavotjad { get; set; }
        public PagingModel? PagingInfo { get; set; }
        public string? Path { get; set; }
        public string? orderBy { get; set; }
        public string? filterField { get; set; }
        public string? filterValue { get; set; }
    }
}
