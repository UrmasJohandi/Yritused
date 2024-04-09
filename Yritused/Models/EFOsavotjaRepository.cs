namespace Yritused.Models
{
    public class EFOsavotjaRepository : IOsavotjaRepository
    {
        private ApplicationDbContext context;

        public EFOsavotjaRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IQueryable<Osavotja> Osavotjad => context.Osavotjad;
    }
}
