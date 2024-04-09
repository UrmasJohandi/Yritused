namespace Yritused.Models
{
    public interface IYritusOsavotjaRepository
    {
        IQueryable <YritusOsavotja> YritusOsavotjad {  get; }
    }
}
