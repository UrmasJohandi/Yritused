namespace Yritused.Models
{
    public interface ISeadistusRepository
    {
        IQueryable<Seadistus> Seadistused { get; }
        Seadistus GetSeadistus(int Id);
        Seadistus GetSeadistusByMoodulAndLylitus(string Moodul, string Lylitus);
        void SaveSeadistus(Seadistus seadistus);
    }
}
