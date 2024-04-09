namespace Yritused.Models
{
    public class YritusOsavotja
    {
        public int id {  get; set; }
        public int yritus_id { get; set; }
        public int osavotja_id { get; set; }
        public DateTime loodud {  get; set; }
        public DateTime? muudetud { get; set; }
        public DateTime? suletud { get; set; }
    }
}
