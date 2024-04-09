namespace Yritused.Models
{
    public class EFYritusRepository : IYritusRepository
    {
        private ApplicationDbContext context;

        public EFYritusRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IQueryable<Yritus> Yritused => context.Yritused;
    }
}
