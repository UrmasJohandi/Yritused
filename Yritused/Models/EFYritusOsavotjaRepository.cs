namespace Yritused.Models
{
    public class EFYritusOsavotjaRepository(ApplicationDbContext ctx) : IYritusOsavotjaRepository
    {
        private readonly ApplicationDbContext context = ctx;

        public IQueryable<YritusOsavotja> YritusOsavotjad => context.YritusOsavotjad;
    }
}
