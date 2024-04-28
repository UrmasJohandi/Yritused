namespace Yritused.Models
{
    public interface IOsavotjaRepository
    {
        IQueryable<Osavotja> Osavotjad { get; }
        Osavotja GetOsavotja(int Id);
        void SaveOsavotja(Osavotja osavotja);
        void DeleteOsavotja(int Id);
    }
}
