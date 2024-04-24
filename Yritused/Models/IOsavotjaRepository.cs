namespace Yritused.Models
{
    public interface IOsavotjaRepository
    {
        IQueryable<Osavotja> Osavotjad { get; }
        Osavotja GetOsavotja(int id);
        void SaveOsavotja(Osavotja osavotja);
    }
}
