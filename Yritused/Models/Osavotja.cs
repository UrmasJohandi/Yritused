namespace Yritused.Models
{
    public class Osavotja
    {
        public int id {  get; set; }
        public string? Eesnimi { get; set; }
        public string? Perenimi {  get; set; }
        public string? Taisnimi { get; set;  }
        public string? Liik {  get; set; }
        public string? Isikukood {  get; set; }
        public string? Makseviis { get; set; }
        public string? Lisainfo { get; set; }
        public DateTime Loodud {  get; set; }
        public DateTime? Muudetud { get; set; }
        public DateTime? Suletud {  get; set; }
    }
}
