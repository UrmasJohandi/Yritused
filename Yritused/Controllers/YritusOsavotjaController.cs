using Microsoft.AspNetCore.Mvc;
using Yritused.Infrastructure;
using Yritused.Models;
using Yritused.Models.Viewmodels;

namespace Yritused.Controllers
{
    public class YritusOsavotjaController(IHttpContextAccessor httpContextAcc, IYritusOsavotjaRepository yritusOsavotjadRepo, IYritusRepository yritusedRepo, IOsavotjaRepository osavotjadRepo, 
        ISeadistusRepository seadistusedRepo) : Controller
    {
        private const string module = "yritusedosavotjad";

        private readonly IHttpContextAccessor httpContextAccessor = httpContextAcc;
        private readonly IYritusOsavotjaRepository yritusOsavotjadRepository = yritusOsavotjadRepo;
        private readonly IYritusRepository yritusedRepository = yritusedRepo;
        private readonly IOsavotjaRepository osavotjadRepository = osavotjadRepo;
        private readonly ISeadistusRepository seadistusedRepository = seadistusedRepo;
        public IActionResult List(int p = 1, string? orderby = null, string? orderbybefore = null, int s = 0, string? filterField = null, string? filterValue = null)
        {
            return View("List", GetViewModel(p, orderby, orderbybefore, s, filterField, filterValue));
        }
        private YritusOsavotjadListViewModel GetViewModel(int p, string? orderby, string? orderbybefore, int s, string? filterField, string? filterValue)
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

            IEnumerable<YritusOsavotja>? yritusOsavotjad = null;
            if (string.IsNullOrEmpty(filterField))
            {
                yritusOsavotjad = yritusOsavotjadRepository.YritusOsavotjad.OrderByDynamic(sortField, listOrder);
            }
            else if (filterFields.Length == 1)
            {
                yritusOsavotjad = FilteredYritusOsavotjad(filterField, filterValue, sortField, listOrder);
            }
            else
            {
                for (int j = 0; j < filterFields.Length; j++)
                {
                    yritusOsavotjad = j == 0 ? FilteredYritusOsavotjad(filterFields[j], filterValues[j], sortField, listOrder) :
                        FilteredYritusOsavotjadSecondRound(yritusOsavotjad, filterFields[j], filterValues[j]);
                }
            }

            YritusOsavotjadListViewModel yritusOsavotjadListViewModel = new()
            {
                YritusOsavotjad = (yritusOsavotjad ?? [])
                    .Skip((p - 1) * pageSize)
                    .Take(pageSize),
                PagingInfo = new PagingModel
                {
                    PageNo = p,
                    PageSize = pageSize,
                    TotalRecords = yritusOsavotjad == null ? 0 : yritusOsavotjad.Count()
                },
                Yritused = yritusedRepository.Yritused,
                Osavotjad = osavotjadRepository.Osavotjad,
                Path = httpContextAccessor.HttpContext == null ? string.Empty : httpContextAccessor.HttpContext.Request.Path.Value,
                FilterField = filterField,
                FilterValue = filterValue,
                OrderBy = string.IsNullOrEmpty(filterField) ? sortField + " " + (listOrder == Utilites.Order.Asc ? "asc" : "desc") :
                    sortField + " " + (listOrder == Utilites.Order.Asc ? "asc" : "desc")
            };

            return yritusOsavotjadListViewModel;
        }
        public IActionResult GetYritusOsavotja(int Id)
        {
            var yritusOsavotja = yritusOsavotjadRepository.YritusOsavotjad.Where(YritusOsavotja => YritusOsavotja.Id == Id).SingleOrDefault();

            return Json(yritusOsavotja);
        }
        public IActionResult GetYritusOsavotjaListViewModel(int Id)
        {
            var yritusOsavotja = yritusOsavotjadRepository.YritusOsavotjad.Where(YritusOsavotja => YritusOsavotja.Id == Id).SingleOrDefault() ?? new YritusOsavotja();

            YritusOsavotjaListViewModel yritusOsavotjaListViewModel = new()
            {
                YritusOsavotja = yritusOsavotja,
                Yritus = yritusedRepository.Yritused.Where(y => y.Id == yritusOsavotja.Yritus_Id).SingleOrDefault(),
                Osavotja = osavotjadRepository.Osavotjad.Where(o => o.Id == yritusOsavotja.Osavotja_Id).SingleOrDefault()
            };

            return Json(yritusOsavotjaListViewModel);
        }
        public IActionResult SaveYritusOsavotja([FromBody]YritusOsavotja yritusOsavotja)
        {
            yritusOsavotjadRepository.SaveYritusOsavotja(yritusOsavotja);

            var yritus = yritusedRepository.Yritused.Where(y => y.Id == yritusOsavotja.Yritus_Id).SingleOrDefault();
            if (yritus != null)
            {
                yritus.Osavotjaid = yritusOsavotjadRepository.GetYrituseOsavotjaid(yritus.Id);
                yritusedRepository.SaveYritus(yritus);
            }

            var osavotja = osavotjadRepository.Osavotjad.Where(o => o.Id == yritusOsavotja.Osavotja_Id).SingleOrDefault();
            if (osavotja != null)
            {
                osavotja.Yritusi = yritusOsavotjadRepository.GetOsavotjaYritusi(osavotja.Id);
                osavotjadRepository.SaveOsavotja(osavotja);
            }

            return Json("OK");
        }
        public IActionResult DeleteYritusOsavotja(int Id, int pageNr)
        {
            var yritusOsavotja = yritusOsavotjadRepository.GetYritusOsavotja(Id);

            var yritus = yritusedRepository.Yritused.Where(y => y.Id == yritusOsavotja.Yritus_Id).SingleOrDefault();
            if (yritus != null)
            {
                yritus.Osavotjaid = yritusOsavotjadRepository.GetYrituseOsavotjaid(yritus.Id);
                yritusedRepository.SaveYritus(yritus);
            }

            var osavotja = osavotjadRepository.Osavotjad.Where(o => o.Id == yritusOsavotja.Osavotja_Id).SingleOrDefault();
            if (osavotja != null)
            {
                osavotja.Yritusi = yritusOsavotjadRepository.GetOsavotjaYritusi(osavotja.Id);
                osavotjadRepository.SaveOsavotja(osavotja);
            }

            yritusOsavotjadRepository.DeleteYritusOsavotja(Id);

            return RedirectToAction("List", new { p = pageNr });
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
        private IEnumerable<YritusOsavotja> FilteredYritusOsavotjad(string? filterField, string? filterValue, string? sortField, Utilites.Order listOrder)
        {
            if (filterField != null && filterField.StartsWith("Id_"))
            {
                var Id = Convert.ToInt32(filterValue);

                return yritusOsavotjadRepository.YritusOsavotjad.Where(y => y.Id == Id).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseNimi_"))
            {
                var yritused = yritusedRepository.Yritused.Where(y => !string.IsNullOrEmpty(y.YrituseNimi) && (y.YrituseNimi ?? "").ToLower().Contains((filterValue ?? "").ToLower())).Select(y => y.Id);

                return yritusOsavotjadRepository.YritusOsavotjad.Where(yo => yritused.Contains(yo.Yritus_Id)).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseKoht_"))
            {
                var yritused = yritusedRepository.Yritused.Where(y => !string.IsNullOrEmpty(y.YrituseKoht) && (y.YrituseKoht ?? "").ToLower().Contains((filterValue ?? "").ToLower())).Select(y => y.Id);

                return yritusOsavotjadRepository.YritusOsavotjad.Where(yo => yritused.Contains(yo.Yritus_Id)).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith(">="))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                var yritused = yritusedRepository.Yritused.Where(y => y.YrituseAeg >= yrituseaeg).Select(y => y.Id);

                return yritusOsavotjadRepository.YritusOsavotjad.Where(yo => yritused.Contains(yo.Yritus_Id)).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith(">"))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                var yritused = yritusedRepository.Yritused.Where(y => y.YrituseAeg > yrituseaeg).Select(y => y.Id);

                return yritusOsavotjadRepository.YritusOsavotjad.Where(yo => yritused.Contains(yo.Yritus_Id)).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith("<="))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                var yritused = yritusedRepository.Yritused.Where(y => y.YrituseAeg <= yrituseaeg).Select(y => y.Id);

                return yritusOsavotjadRepository.YritusOsavotjad.Where(yo => yritused.Contains(yo.Yritus_Id)).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith("<"))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                var yritused = yritusedRepository.Yritused.Where(y => y.YrituseAeg < yrituseaeg).Select(y => y.Id);

                return yritusOsavotjadRepository.YritusOsavotjad.Where(yo => yritused.Contains(yo.Yritus_Id)).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_"))
            {
                var yrituseaeg = Convert.ToDateTime(filterValue ?? "");

                var yritused = yritusedRepository.Yritused.Where(y => y.YrituseAeg == yrituseaeg).Select(y => y.Id);

                return yritusOsavotjadRepository.YritusOsavotjad.Where(yo => yritused.Contains(yo.Yritus_Id)).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("OsavotjaTaisnimi_"))
            {
                var osavotjad = osavotjadRepository.Osavotjad.Where(o => !string.IsNullOrEmpty(o.Taisnimi) && (o.Taisnimi ?? "").ToLower().Contains((filterValue ?? "").ToLower())).Select(o => o.Id);

                return yritusOsavotjadRepository.YritusOsavotjad.Where(yo => osavotjad.Contains(yo.Osavotja_Id)).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("OsavotjaIsikukood_"))
            {
                var osavotjad = osavotjadRepository.Osavotjad.Where(o => !string.IsNullOrEmpty(o.Isikukood) && (o.Isikukood ?? "").ToLower().Contains((filterValue ?? "").ToLower())).Select(o => o.Id);

                return yritusOsavotjadRepository.YritusOsavotjad.Where(yo => osavotjad.Contains(yo.Osavotja_Id)).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return yritusOsavotjadRepository.YritusOsavotjad.Where(y => y.Loodud >= loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return yritusOsavotjadRepository.YritusOsavotjad.Where(y => y.Loodud > loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return yritusOsavotjadRepository.YritusOsavotjad.Where(y => y.Loodud <= loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return yritusOsavotjadRepository.YritusOsavotjad.Where(y => y.Loodud < loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_"))
            {
                var loodud = Convert.ToDateTime(filterValue ?? "");

                return yritusOsavotjadRepository.YritusOsavotjad.Where(y => y.Loodud == loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return yritusOsavotjadRepository.YritusOsavotjad.Where(y => y.Muudetud >= muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return yritusOsavotjadRepository.YritusOsavotjad.Where(y => y.Muudetud > muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return yritusOsavotjadRepository.YritusOsavotjad.Where(y => y.Muudetud <= muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return yritusOsavotjadRepository.YritusOsavotjad.Where(y => y.Muudetud < muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_"))
            {
                var muudetud = Convert.ToDateTime(filterValue ?? "");

                return yritusOsavotjadRepository.YritusOsavotjad.Where(y => y.Muudetud == muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }

            return yritusOsavotjadRepository.YritusOsavotjad;
        }
        private IEnumerable<YritusOsavotja> FilteredYritusOsavotjadSecondRound(IEnumerable<YritusOsavotja>? yritusosavotjad, string? filterField, string? filterValue)
        {
            if (filterField != null && filterField.StartsWith("Id_"))
            {
                var Id = Convert.ToInt32(filterValue);

                return (yritusosavotjad ?? []).Where(y => y.Id == Id);
            }
            else if (filterField != null && filterField.StartsWith("YrituseNimi_"))
            {
                var yritused = yritusedRepository.Yritused.Where(y => !string.IsNullOrEmpty(y.YrituseNimi) && (y.YrituseNimi ?? "").ToLower().Contains((filterValue ?? "").ToLower())).Select(y => y.Id);

                return (yritusosavotjad ?? []).Where(yo => yritused.Contains(yo.Yritus_Id));
            }
            else if (filterField != null && filterField.StartsWith("YrituseKoht_"))
            {
                var yritused = yritusedRepository.Yritused.Where(y => !string.IsNullOrEmpty(y.YrituseKoht) && (y.YrituseKoht ?? "").ToLower().Contains((filterValue ?? "").ToLower())).Select(y => y.Id);

                return (yritusosavotjad ?? []).Where(yo => yritused.Contains(yo.Yritus_Id));
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith(">="))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                var yritused = yritusedRepository.Yritused.Where(y => y.YrituseAeg >= yrituseaeg).Select(y => y.Id);

                return (yritusosavotjad ?? []).Where(yo => yritused.Contains(yo.Yritus_Id));
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith(">"))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                var yritused = yritusedRepository.Yritused.Where(y => y.YrituseAeg > yrituseaeg).Select(y => y.Id);

                return (yritusosavotjad ?? []).Where(yo => yritused.Contains(yo.Yritus_Id));
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith("<="))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                var yritused = yritusedRepository.Yritused.Where(y => y.YrituseAeg <= yrituseaeg).Select(y => y.Id);

                return (yritusosavotjad ?? []).Where(yo => yritused.Contains(yo.Yritus_Id));
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith("<"))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                var yritused = yritusedRepository.Yritused.Where(y => y.YrituseAeg < yrituseaeg).Select(y => y.Id);

                return (yritusosavotjad ?? []).Where(yo => yritused.Contains(yo.Yritus_Id));
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_"))
            {
                var yrituseaeg = Convert.ToDateTime(filterValue ?? "");

                var yritused = yritusedRepository.Yritused.Where(y => y.YrituseAeg == yrituseaeg).Select(y => y.Id);

                return (yritusosavotjad ?? []).Where(yo => yritused.Contains(yo.Yritus_Id));
            }
            else if (filterField != null && filterField.StartsWith("OsavotjaTaisnimi_"))
            {
                var osavotjad = osavotjadRepository.Osavotjad.Where(o => !string.IsNullOrEmpty(o.Taisnimi) && (o.Taisnimi ?? "").ToLower().Contains((filterValue ?? "").ToLower())).Select(o => o.Id);

                return (yritusosavotjad ?? []).Where(yo => osavotjad.Contains(yo.Osavotja_Id));
            }
            else if (filterField != null && filterField.StartsWith("OsavotjaIsikukood_"))
            {
                var osavotjad = osavotjadRepository.Osavotjad.Where(o => !string.IsNullOrEmpty(o.Isikukood) && (o.Isikukood ?? "").ToLower().Contains((filterValue ?? "").ToLower())).Select(o => o.Id);

                return (yritusosavotjad ?? []).Where(yo => osavotjad.Contains(yo.Osavotja_Id));
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return (yritusosavotjad ?? []).Where(y => y.Loodud >= loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return (yritusosavotjad ?? []).Where(y => y.Loodud > loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return (yritusosavotjad ?? []).Where(y => y.Loodud <= loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return (yritusosavotjad ?? []).Where(y => y.Loodud < loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_"))
            {
                var loodud = Convert.ToDateTime(filterValue ?? "");

                return (yritusosavotjad ?? []).Where(y => y.Loodud == loodud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return (yritusosavotjad ?? []).Where(y => y.Muudetud >= muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return (yritusosavotjad ?? []).Where(y => y.Muudetud > muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return (yritusosavotjad ?? []).Where(y => y.Muudetud <= muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return (yritusosavotjad ?? []).Where(y => y.Muudetud < muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_"))
            {
                var muudetud = Convert.ToDateTime(filterValue ?? "");

                return (yritusosavotjad ?? []).Where(y => y.Muudetud == muudetud);
            }

            return (yritusosavotjad ?? []);
        }
    }
}
