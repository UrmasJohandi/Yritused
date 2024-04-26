namespace Yritused.Models.Viewmodels
{
    public class YritusOsavotjadListViewModel
    {
        public IEnumerable<YritusOsavotja>? YritusOsavotjad { get; set; }
        public IEnumerable<Yritus>? Yritused {  get; set; }
        public IEnumerable<Osavotja>? Osavotjad { get; set; }
        public PagingModel? PagingInfo { get; set; }
        public string? Path { get; set; }
        public string? OrderBy { get; set; }
        public string? FilterField { get; set; }
        public string? FilterValue { get; set; }
    }
}
