namespace Yritused.Models
{
    public class EFYritusOsavotjaRepository : IYritusOsavotjaRepository
    {
        private ApplicationDbContext context;

        public EFYritusOsavotjaRepository(ApplicationDbContext ctx) 
        {
            context = ctx;
        }
        public IQueryable<YritusOsavotja> YritusOsavotjad => context.YritusOsavotjad;
    }
}
