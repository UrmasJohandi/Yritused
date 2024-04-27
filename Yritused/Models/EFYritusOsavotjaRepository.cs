using Yritused.Models.Viewmodels;

namespace Yritused.Models
{
    public class EFYritusOsavotjaRepository(ApplicationDbContext ctx) : IYritusOsavotjaRepository
    {
        private readonly ApplicationDbContext context = ctx;
        public IQueryable<YritusOsavotja> YritusOsavotjad => context.YritusOsavotjad;
        public YritusOsavotja GetYritusOsavotja(int Id) => context.YritusOsavotjad.Where(yo => yo.Id == Id).SingleOrDefault() ?? new YritusOsavotja();
        public void SaveYritusOsavotja(YritusOsavotja yritusOsavotja)
        {
            if (yritusOsavotja.Id == 0)
            {
                yritusOsavotja.Loodud = DateTime.Now;
                context.YritusOsavotjad.Add(yritusOsavotja);
            }
            else
            {
                YritusOsavotja dbEntry = context.YritusOsavotjad.FirstOrDefault(yo => yo.Id == yritusOsavotja.Id) ?? new YritusOsavotja();
                if (dbEntry != null)
                {
                    dbEntry.Yritus_Id = yritusOsavotja.Yritus_Id;
                    dbEntry.Osavotja_Id = yritusOsavotja.Osavotja_Id;
                    dbEntry.Muudetud = DateTime.Now;
                }
            }

            context.SaveChanges();
        }
        public int GetYrituseOsavotjaid(int YrituseId)
        {
            return context.YritusOsavotjad.Where(yo => yo.Yritus_Id == YrituseId).Count();
        }
        public int GetOsavotjaYritusi(int OsavotjaId)
        {
            return context.YritusOsavotjad.Where(yo => yo.Osavotja_Id == OsavotjaId).Count();
        }
    }
}
