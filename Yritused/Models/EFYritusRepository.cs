namespace Yritused.Models
{
    public class EFYritusRepository(ApplicationDbContext ctx) : IYritusRepository
    {
        private readonly ApplicationDbContext context = ctx;

        public IQueryable<Yritus> Yritused => context.Yritused;
        public Yritus GetYritus(int Id) => context.Yritused.Where(y => y.Id == Id).SingleOrDefault() ?? new Yritus();
        public void SaveYritus(Yritus yritus)
        {
            if (yritus.Id == 0)
            {
                yritus.Loodud = DateTime.Now;
                context.Yritused.Add(yritus);
            }
            else
            {
                Yritus dbEntry = context.Yritused.FirstOrDefault(y => y.Id == yritus.Id) ?? new Yritus();
                if (dbEntry != null)
                {
                    dbEntry.YrituseNimi = yritus.YrituseNimi;
                    dbEntry.YrituseAeg = yritus.YrituseAeg;
                    dbEntry.YrituseKoht = yritus.YrituseKoht;
                    dbEntry.Lisainfo = yritus.Lisainfo;
                    dbEntry.Muudetud = DateTime.Now;
                }
            }

            context.SaveChanges();
        }
        public void DeleteYritus(int Id)
        {
            Yritus dbEntry = context.Yritused.Where(y => y.Id == Id).FirstOrDefault() ?? new Yritus();
            if (dbEntry.Id != 0)
            {
                context.Yritused.Remove(dbEntry);
                context.SaveChanges();
            }
        }
    }
}
