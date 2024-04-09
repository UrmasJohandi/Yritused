namespace Yritused.Models
{
    public interface IYritusRepository
    {
        IQueryable<Yritus> Yritused { get; }
    }
}
