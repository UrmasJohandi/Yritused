namespace Yritused.Models
{
    public class YritusOsavotja
    {
        public int Id {  get; set; }
        public int Yritus_Id { get; set; }
        public int Osavotja_Id { get; set; }
        public DateTime Loodud {  get; set; }
        public DateTime? Muudetud { get; set; }
    }
}
