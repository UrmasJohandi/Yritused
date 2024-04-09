namespace Yritused.Models
{
    public class Osavotja
    {
        public int id {  get; set; }
        public string? eesnimi { get; set; }
        public string? perenimi {  get; set; }
        public string? taisnimi { get; set;  }
        public string? liik {  get; set; }
        public string? isikukood {  get; set; }
        public string? makseviis { get; set; }
        public string? lisainfo { get; set; }
        public DateTime loodud {  get; set; }
        public DateTime? muudetud { get; set; }
        public DateTime? suletud {  get; set; }
    }
}
