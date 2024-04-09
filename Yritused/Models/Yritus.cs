namespace Yritused.Models
{
    public class Yritus
    {
        public int id { get; set; }
        public string? yrituse_nimi__ {  get; set; }
        public DateTime? yrituse_aeg { get; set; }
        public string? yrituse_koht { get; set; }
        public string? lisainfo { get; set; }
        public DateTime loodud {  get; set; }
        public DateTime muudetud { get; set; }
        public DateTime suletud { get; set; }
    }
}
