namespace Yritused.Models
{
    public class Osavotja
    {
        public int Id {  get; set; }
        public string? Eesnimi { get; set; }
        public string? Perenimi {  get; set; }
        public string? Taisnimi { get; set;  }
        public string? Liik {  get; set; }
        public string? Isikukood {  get; set; }
        public string? Makseviis { get; set; }
        public string? Lisainfo { get; set; }
        public int? Yritusi { get; set; }
        public DateTime Loodud {  get; set; }
        public DateTime? Muudetud { get; set; }
    }
}
