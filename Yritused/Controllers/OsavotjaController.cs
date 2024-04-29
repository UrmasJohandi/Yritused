using Microsoft.AspNetCore.Mvc;
using Yritused.Infrastructure;
using Yritused.Models;
using Yritused.Models.Viewmodels;

namespace Yritused.Controllers
{
    public class OsavotjaController(IHttpContextAccessor httpContextAcc, IOsavotjaRepository osavotjadRepo, ISeadistusRepository seadistusedRepo) : Controller
    {
        private const string module = "osavotjad";

        private readonly IOsavotjaRepository osavotjadRepository = osavotjadRepo;
        private readonly ISeadistusRepository seadistusedRepository = seadistusedRepo;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAcc;

        public IActionResult List(int p = 1, string? orderby = null, string? orderbybefore = null, int s = 0, string? filterField = null, string? filterValue = null)
        {
            return View("List", GetViewModel(p, orderby, orderbybefore, s, filterField, filterValue));
        }
        private OsavotjadListViewModel GetViewModel(int p, string? orderby, string? orderbybefore, int s, string? filterField, string? filterValue)
        {
            string orderByFieldBefore = "";
            string ascDescBefore = "";
            string sortField;
            Utilites.Order listOrder;

            if (orderby == null && orderbybefore == null)
            {
                sortField = "Id";
                listOrder = Utilites.Order.Desc;
            }
            else if (orderby != null && orderbybefore == null)
            {
                sortField = orderby.Split(" ")[0];
                listOrder = orderby.Split(" ")[1] == "asc" ? Utilites.Order.Asc : Utilites.Order.Desc;
            }
            else
            {
                if (orderbybefore != null)
                {
                    orderByFieldBefore = orderbybefore.Split(" ")[0];
                    ascDescBefore = orderbybefore.Split(" ")[1];
                }

                sortField = orderby ?? string.Empty;
                listOrder = orderby == orderByFieldBefore && ascDescBefore == "asc" ? Utilites.Order.Desc : orderby == orderByFieldBefore && ascDescBefore == "desc" ? Utilites.Order.Asc :
                    ascDescBefore == "asc" ? Utilites.Order.Asc : Utilites.Order.Desc;
            }

            int pageSize = GetPageSize(s);

            string[] filterFields = [];
            string[] filterValues = [];

            filterFields = string.IsNullOrEmpty(filterField) ? filterFields : filterField.Split(';');
            filterValues = string.IsNullOrEmpty(filterValue) ? filterValues : filterValue.Split(';');

            IEnumerable<Osavotja>? osavotjad = null;
            if (string.IsNullOrEmpty(filterField))
            {
                osavotjad = osavotjadRepository.Osavotjad.OrderByDynamic(sortField, listOrder);
            }
            else if (filterFields.Length == 1)
            {
                osavotjad = FilteredOsavotjad(filterField, filterValue, sortField, listOrder);
            }
            else
            {
                for (int j = 0; j < filterFields.Length; j++)
                {
                    osavotjad = j == 0 ? FilteredOsavotjad(filterFields[j], filterValues[j], sortField, listOrder) :
                        FilteredOsavotjadSecondRound(osavotjad, filterFields[j], filterValues[j]);
                }
            }

            OsavotjadListViewModel osavotjadListViewModel = new()
            {
                Osavotjad = (osavotjad ?? [])
                    .Skip((p - 1) * pageSize)
                    .Take(pageSize),
                PagingInfo = new PagingModel
                {
                    PageNo = p,
                    PageSize = pageSize,
                    TotalRecords = osavotjad == null ? 0 : osavotjad.Count()
                },
                Path = httpContextAccessor.HttpContext == null ? string.Empty : httpContextAccessor.HttpContext.Request.Path.Value,
                FilterField = filterField,
                FilterValue = filterValue,
                OrderBy = string.IsNullOrEmpty(filterField) ? sortField + " " + (listOrder == Utilites.Order.Asc ? "asc" : "desc") :
                    sortField + " " + (listOrder == Utilites.Order.Asc ? "asc" : "desc")
            };

            return osavotjadListViewModel;
        }
        public IActionResult GetOsavotja(int Id)
        {
            var osavotja = osavotjadRepository.Osavotjad.Where(o => o.Id == Id).SingleOrDefault();

            return Json(osavotja);
        }

        [HttpPost]
        public IActionResult SaveOsavotja([FromBody]Osavotja osavotja)
        {
            osavotjadRepository.SaveOsavotja(osavotja);

            return Json("OK");
        }
        public IActionResult DeleteOsavotja(int Id, int pageNr)
        {
            osavotjadRepository.DeleteOsavotja(Id);

            return RedirectToAction("List", new { p = pageNr});
        }
        public IActionResult GetAutocompleteByOsavotjaNimi(string taisnimi_fr)
        {
            List<string> osavotjad = [];

            foreach (Osavotja o in osavotjadRepository.Osavotjad.OrderBy(o => o.Taisnimi).ThenBy(o => o.Isikukood))
            {
                if (taisnimi_fr == null)
                {
                    osavotjad.Add(o.Taisnimi + " " + o.Isikukood);
                }
                else if ((o.Taisnimi + " " + o.Isikukood).ToLower().Contains(taisnimi_fr.ToLower(), StringComparison.CurrentCulture))
                {
                    osavotjad.Add(o.Taisnimi + " " + o.Isikukood);
                }
            }

            return Json(osavotjad);
        }
        public IActionResult GetOsavotjaByNimi(string taisnimi)
        {
            Osavotja osavotja = new();

            foreach (Osavotja o in osavotjadRepository.Osavotjad)
            {
                if ((o.Taisnimi + " " + o.Isikukood).ToLower().Contains(taisnimi.ToLower(), StringComparison.CurrentCulture))
                {
                    osavotja = o;
                    break;
                }
            }

            return Json(osavotja);
        }
        public IActionResult GetAutocompleteByIsikukood(string isikukood_fr)
        {
            List<string> osavotjad = [];

            foreach (Osavotja o in osavotjadRepository.Osavotjad.OrderBy(o => o.Isikukood).ThenBy(o => o.Taisnimi))
            {
                if (isikukood_fr == null)
                {
                    osavotjad.Add(o.Isikukood + " " + o.Taisnimi);
                }
                else if ((o.Isikukood + " " + o.Taisnimi).ToLower().Contains(isikukood_fr.ToLower(), StringComparison.CurrentCulture))
                {
                    osavotjad.Add(o.Isikukood + " " + o.Taisnimi);
                }
            }

            return Json(osavotjad);
        }
        public IActionResult GetOsavotjaByIsikukood(string isikukood)
        {
            Osavotja osavotja = new();

            foreach (Osavotja o in osavotjadRepository.Osavotjad)
            {
                if ((o.Isikukood + " " + o.Taisnimi).ToLower().Contains(isikukood.ToLower(), StringComparison.CurrentCulture))
                {
                    osavotja = o;
                    break;
                }
            }

            return Json(osavotja);
        }
        private int GetPageSize(int s)
        {
            var seadistus = seadistusedRepository.GetSeadistusByMoodulAndLylitus(module, "RiduLehel");
            var pageSize = s != 0 ? s : Convert.ToInt32(seadistus.Vaartus);

            if (pageSize != Convert.ToInt32(seadistus.Vaartus))
            {
                seadistus.Vaartus = Convert.ToString(pageSize);
                seadistusedRepository.SaveSeadistus(seadistus);
            }

            return pageSize;
        }
        private IEnumerable<Osavotja> FilteredOsavotjad(string? filterField, string? filterValue, string? sortField, Utilites.Order listOrder)
        {
            if (filterField != null && filterField.StartsWith("Id_"))
            {
                var Id = Convert.ToInt32(filterValue);

                return osavotjadRepository.Osavotjad.Where(o => o.Id == Id).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Eesnimi_"))
            {
                return osavotjadRepository.Osavotjad.Where(o => !string.IsNullOrEmpty(o.Eesnimi) && (o.Eesnimi ?? "").ToLower().Contains((filterValue ?? "").ToLower())).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Perenimi_"))
            {
                return osavotjadRepository.Osavotjad.Where(o => !string.IsNullOrEmpty(o.Perenimi) && (o.Perenimi ?? "").ToLower().Contains((filterValue ?? "").ToLower())).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Liik_"))
            {
                var liik = filterValue == "Füüsiline isik" ? "F" : filterValue == "Juriidiline isik" ? "J" : filterValue;

                return osavotjadRepository.Osavotjad.Where(o => !string.IsNullOrEmpty(o.Liik) && (o.Liik ?? "").ToLower().Contains((liik ?? "").ToLower())).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Isikukood_"))
            {
                return osavotjadRepository.Osavotjad.Where(o => !string.IsNullOrEmpty(o.Isikukood) && (o.Isikukood ?? "").ToLower().Contains((filterValue ?? "").ToLower())).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Makseviis_"))
            {
                var makseviis = filterValue == "Pangaülekandega" ? "P" : filterValue == "Sularahas" ? "S" : filterValue;

                return osavotjadRepository.Osavotjad.Where(o => !string.IsNullOrEmpty(o.Makseviis) && (o.Makseviis ?? "").ToLower().Contains((makseviis ?? "").ToLower())).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Lisainfo_"))
            {
                return osavotjadRepository.Osavotjad.Where(o => !string.IsNullOrEmpty(o.Lisainfo) && (o.Lisainfo ?? "").ToLower().Contains((filterValue ?? "").ToLower())).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Yritusi_") && (filterValue ?? "").StartsWith(">="))
            {
                var yritusi = Convert.ToInt32((filterValue ?? "").Replace(">=", "").Replace(" ", ""));

                return osavotjadRepository.Osavotjad.Where(o => o.Yritusi >= yritusi).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Yritusi_") && (filterValue ?? "").StartsWith(">"))
            {
                var yritusi = Convert.ToInt32((filterValue ?? "").Replace(">", "").Replace(" ", ""));

                return osavotjadRepository.Osavotjad.Where(o => o.Yritusi > yritusi).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Yritusi_") && (filterValue ?? "").StartsWith("<="))
            {
                var yritusi = Convert.ToInt32((filterValue ?? "").Replace("<=", "").Replace(" ", ""));

                return osavotjadRepository.Osavotjad.Where(y => y.Yritusi <= yritusi).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Yritusi_") && (filterValue ?? "").StartsWith("<"))
            {
                var yritusi = Convert.ToInt32((filterValue ?? "").Replace("<", "").Replace(" ", ""));

                return osavotjadRepository.Osavotjad.Where(o => o.Yritusi <= yritusi).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Yritusi_"))
            {
                var yritusi = Convert.ToInt32((filterValue ?? "").Replace("<", "").Replace(" ", ""));

                return osavotjadRepository.Osavotjad.Where(o => o.Yritusi == yritusi).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return osavotjadRepository.Osavotjad.Where(o => o.Loodud >= loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return osavotjadRepository.Osavotjad.Where(o => o.Loodud > loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return osavotjadRepository.Osavotjad.Where(o => o.Loodud <= loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return osavotjadRepository.Osavotjad.Where(o => o.Loodud < loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_"))
            {
                var loodud = Convert.ToDateTime(filterValue ?? "");

                return osavotjadRepository.Osavotjad.Where(o => o.Loodud == loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return osavotjadRepository.Osavotjad.Where(o => o.Muudetud >= muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return osavotjadRepository.Osavotjad.Where(o => o.Muudetud > muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return osavotjadRepository.Osavotjad.Where(o => o.Muudetud <= muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return osavotjadRepository.Osavotjad.Where(o => o.Muudetud < muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return osavotjadRepository.Osavotjad.Where(o => o.Muudetud == muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }

            return osavotjadRepository.Osavotjad;
        }

        private IEnumerable<Osavotja> FilteredOsavotjadSecondRound(IEnumerable<Osavotja>? osavotjad, string? filterField, string? filterValue)
        {
            if (filterField != null && filterField.StartsWith("Id_"))
            {
                var Id = Convert.ToInt32(filterValue);

                return (osavotjad ?? []).Where(o => o.Id == Id);
            }
            else if (filterField != null && filterField.StartsWith("Eesnimi_"))
            {
                return (osavotjad ?? []).Where(o => !string.IsNullOrEmpty(o.Eesnimi) && (o.Eesnimi ?? "").ToLower().Contains((filterValue ?? "").ToLower()));
            }
            else if (filterField != null && filterField.StartsWith("Perenimi_"))
            {
                return (osavotjad ?? []).Where(o => !string.IsNullOrEmpty(o.Perenimi) && (o.Perenimi ?? "").ToLower().Contains((filterValue ?? "").ToLower()));
            }
            else if (filterField != null && filterField.StartsWith("Liik_"))
            {
                var liik = filterValue == "Füüsiline isik" ? "F" : filterValue == "Juriidiline isik" ? "J" : filterValue;

                return (osavotjad ?? []).Where(o => !string.IsNullOrEmpty(o.Liik) && (o.Liik ?? "").ToLower().Contains((liik ?? "").ToLower()));
            }
            else if (filterField != null && filterField.StartsWith("Isikukood_"))
            {
                return (osavotjad ?? []).Where(o => !string.IsNullOrEmpty(o.Isikukood) && (o.Isikukood ?? "").ToLower().Contains((filterValue ?? "").ToLower()));
            }
            else if (filterField != null && filterField.StartsWith("Makseviis_"))
            {
                var makseviis = filterValue == "Pangaülekandega" ? "P" : filterValue == "Sularahas" ? "S" : filterValue;

                return (osavotjad ?? []).Where(o => !string.IsNullOrEmpty(o.Makseviis) && (o.Makseviis ?? "").ToLower().Contains((makseviis ?? "").ToLower()));
            }
            else if (filterField != null && filterField.StartsWith("Lisainfo_"))
            {
                return (osavotjad ?? []).Where(o => !string.IsNullOrEmpty(o.Lisainfo) && (o.Lisainfo ?? "").ToLower().Contains((filterValue ?? "").ToLower()));
            }
            else if (filterField != null && filterField.StartsWith("Yritusi_") && (filterValue ?? "").StartsWith(">="))
            {
                var yritusi = Convert.ToInt32((filterValue ?? "").Replace(">=", "").Replace(" ", ""));

                return (osavotjad ?? []).Where(o => o.Yritusi >= yritusi);
            }
            else if (filterField != null && filterField.StartsWith("Yritusi_") && (filterValue ?? "").StartsWith(">"))
            {
                var yritusi = Convert.ToInt32((filterValue ?? "").Replace(">", "").Replace(" ", ""));

                return (osavotjad ?? []).Where(o => o.Yritusi > yritusi);
            }
            else if (filterField != null && filterField.StartsWith("Yritusi_") && (filterValue ?? "").StartsWith("<="))
            {
                var yritusi = Convert.ToInt32((filterValue ?? "").Replace("<=", "").Replace(" ", ""));

                return (osavotjad ?? []).Where(y => y.Yritusi <= yritusi);
            }
            else if (filterField != null && filterField.StartsWith("Yritusi_") && (filterValue ?? "").StartsWith("<"))
            {
                var yritusi = Convert.ToInt32((filterValue ?? "").Replace("<", "").Replace(" ", ""));

                return (osavotjad ?? []).Where(o => o.Yritusi <= yritusi);
            }
            else if (filterField != null && filterField.StartsWith("Yritusi_"))
            {
                var yritusi = Convert.ToInt32((filterValue ?? "").Replace("<", "").Replace(" ", ""));

                return (osavotjad ?? []).Where(o => o.Yritusi == yritusi);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return (osavotjad ?? []).Where(o => o.Loodud >= loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return (osavotjad ?? []).Where(o => o.Loodud > loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return (osavotjad ?? []).Where(o => o.Loodud <= loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return (osavotjad ?? []).Where(o => o.Loodud < loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_"))
            {
                var loodud = Convert.ToDateTime(filterValue ?? "");

                return (osavotjad ?? []).Where(o => o.Loodud == loodud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return (osavotjad ?? []).Where(o => o.Muudetud >= muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return (osavotjad ?? []).Where(o => o.Muudetud > muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return (osavotjad ?? []).Where(o => o.Muudetud <= muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return (osavotjad ?? []).Where(o => o.Muudetud < muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return (osavotjad ?? []).Where(o => o.Muudetud == muudetud);
            }

            return osavotjadRepository.Osavotjad;
        }
    }
}
