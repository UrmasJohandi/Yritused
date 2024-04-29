namespace Yritused.Models
{
    public class Seadistus
    {
        public int Id { get; set; }
        public string? Moodul { get; set; }
        public string? Lylitus { get; set; }
        public string? Vaartus { get; set; }
        public DateTime Loodud { get; set; }
        public DateTime? Muudetud { get; set; }
    }
}
