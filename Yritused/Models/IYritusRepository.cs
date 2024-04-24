namespace Yritused.Models
{
    public interface IYritusRepository
    {
        IQueryable<Yritus> Yritused { get; }
        Yritus GetYritus(int id);
        void SaveYritus(Yritus yritus);
    }
}
