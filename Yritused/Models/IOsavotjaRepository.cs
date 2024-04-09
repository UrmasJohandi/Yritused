namespace Yritused.Models
{
    public interface IOsavotjaRepository
    {
        IQueryable<Osavotja> Osavotjad { get; }
    }
}
