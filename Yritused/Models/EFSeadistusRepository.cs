namespace Yritused.Models
{
    public class EFSeadistusRepository(ApplicationDbContext ctx) : ISeadistusRepository
    {
        private readonly ApplicationDbContext context = ctx;
        public IQueryable<Seadistus> Seadistused => context.Seadistused;
        public Seadistus GetSeadistus(int Id) => context.Seadistused.Where(s => s.Id == Id).SingleOrDefault() ?? new Seadistus();
        public Seadistus GetSeadistusByMoodulAndLylitus(string Moodul, string Lylitus) => context.Seadistused.Where(s => s.Moodul == Moodul && s.Lylitus == Lylitus).SingleOrDefault() ?? new Seadistus();
        public void SaveSeadistus(Seadistus seadistus)
        {
            if (seadistus.Id == 0)
            {
                seadistus.Loodud = DateTime.Now;
                context.Seadistused.Add(seadistus);
            }
            else
            {
                Seadistus dbEntry = context.Seadistused.FirstOrDefault(s => s.Id == seadistus.Id) ?? new Seadistus();
                if (dbEntry != null)
                {
                    dbEntry.Moodul = seadistus.Moodul;
                    dbEntry.Lylitus = seadistus.Lylitus;
                    dbEntry.Vaartus = seadistus.Vaartus;
                    dbEntry.Muudetud = DateTime.Now;
                }
            }

            context.SaveChanges();
        }
    }
}
