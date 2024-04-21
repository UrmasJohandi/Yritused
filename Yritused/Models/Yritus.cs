namespace Yritused.Models
{
    public class Yritus
    {
        public int Id { get; set; }
        public string? YrituseNimi {  get; set; }
        public DateTime YrituseAeg { get; set; }
        public string? YrituseKoht { get; set; }
        public string? Lisainfo { get; set; }
        public int? Osavotjaid { get; set; }
        public DateTime Loodud {  get; set; }
        public DateTime? Muudetud { get; set; }
        public DateTime? Suletud { get; set; }
    }
}
