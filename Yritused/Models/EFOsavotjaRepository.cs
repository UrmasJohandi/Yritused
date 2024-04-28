namespace Yritused.Models
{
    public class EFOsavotjaRepository(ApplicationDbContext ctx) : IOsavotjaRepository
    {
        private readonly ApplicationDbContext context = ctx;

        public IQueryable<Osavotja> Osavotjad => context.Osavotjad;
        public Osavotja GetOsavotja(int Id) => context.Osavotjad.Where(o => o.Id == Id).SingleOrDefault() ?? new Osavotja();
        public void SaveOsavotja(Osavotja osavotja)
        {
            if (osavotja.Id == 0)
            {
                osavotja.Loodud = DateTime.Now;
                osavotja.Taisnimi = string.Format("{0} {1}", osavotja.Eesnimi, osavotja.Perenimi);
                context.Osavotjad.Add(osavotja);
            }
            else
            {
                Osavotja dbEntry = context.Osavotjad.FirstOrDefault(o => o.Id == osavotja.Id) ?? new Osavotja();
                if (dbEntry != null)
                {
                    dbEntry.Eesnimi = osavotja.Eesnimi;
                    dbEntry.Perenimi = osavotja.Perenimi;
                    dbEntry.Liik = osavotja.Liik;
                    dbEntry.Isikukood = osavotja.Isikukood;
                    dbEntry.Makseviis = osavotja.Makseviis;
                    dbEntry.Lisainfo = osavotja.Lisainfo;
                    dbEntry.Muudetud = DateTime.Now;
                }
            }

            context.SaveChanges();
        }
        public void DeleteOsavotja(int Id)
        {
            Osavotja dbEntry = context.Osavotjad.Where(o => o.Id == Id).FirstOrDefault() ?? new Osavotja();
            if (dbEntry.Id != 0)
            {
                context.Osavotjad.Remove(dbEntry);
                context.SaveChanges();
            }
        }
    }
}
