namespace Yritused.Models
{
    public interface IYritusOsavotjaRepository
    {
        IQueryable <YritusOsavotja> YritusOsavotjad {  get; }
        YritusOsavotja GetYritusOsavotja(int Id);
        void SaveYritusOsavotja(YritusOsavotja yritusOsavotja);
    }
}
