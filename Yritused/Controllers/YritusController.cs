using Microsoft.AspNetCore.Mvc;
using Yritused.Infrastructure;
using Yritused.Models;
using Yritused.Models.Viewmodels;

namespace Yritused.Controllers
{
    public class YritusController(IHttpContextAccessor httpContextAcc, IYritusRepository yritusedRepo, ISeadistusRepository seadistusedRepo) : Controller
    {
        private const string module = "yritused";

        private readonly IYritusRepository yritusedRepository = yritusedRepo;
        private readonly ISeadistusRepository seadistusedRepository = seadistusedRepo;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAcc;

        public IActionResult List(int p = 1, string? orderby = null, string? orderbybefore = null, int s = 0, string? filterField = null, string? filterValue = null)
        {
            var menu = httpContextAccessor.HttpContext == null ? "yritused" : httpContextAccessor.HttpContext.Request.Cookies["menu"];
            if (menu == "osavotjad")
            {
                return RedirectToAction("List", "Osavotja");
            }
            else if (menu == "yritusedosavotjad")
            {
                return RedirectToAction("List", "YritusOsavotja");
            }

            /* if (orderby == null && orderbybefore == null && filterField == null && filterValue == null)
            {
                filterField = "YrituseAeg_1";
                filterValue = ">" + Convert.ToString(DateTime.Now);
            } */

            return View("List", GetViewModel(p, orderby, orderbybefore, s, filterField, filterValue));
        }
        private YritusedListViewModel GetViewModel(int p, string? orderby, string? orderbybefore, int s, string? filterField, string? filterValue)
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

            IEnumerable<Yritus>? yritused = null;
            if (string.IsNullOrEmpty(filterField))
            {
                yritused = yritusedRepository.Yritused.OrderByDynamic(sortField, listOrder);
            }
            else if (filterFields.Length == 1)
            {
                yritused = FilteredYritused(filterField, filterValue, sortField, listOrder);
            }
            else
            {
                for (int j = 0; j < filterFields.Length; j++)
                {
                    yritused = j == 0 ? FilteredYritused(filterFields[j], filterValues[j], sortField, listOrder) :
                        FilteredYritusedSecondRound(yritused, filterFields[j], filterValues[j]);
                }
            }

            YritusedListViewModel yritusedListViewModel = new()
            {
                Yritused = (yritused ?? [])
                    .Skip((p - 1) * pageSize)
                    .Take(pageSize),
                PagingInfo = new PagingModel
                {
                    PageNo = p,
                    PageSize = pageSize,
                    TotalRecords = yritused == null ? 0 : yritused.Count()
                },
                Path = httpContextAccessor.HttpContext == null ? string.Empty : httpContextAccessor.HttpContext.Request.Path.Value,
                FilterField = filterField,
                FilterValue = filterValue,
                OrderBy = string.IsNullOrEmpty(filterField) ? sortField + " " + (listOrder == Utilites.Order.Asc ? "asc" : "desc") :
                    sortField + " " + (listOrder == Utilites.Order.Asc ? "asc" : "desc")
            };

            return yritusedListViewModel;
        }
        public IActionResult GetYritus(int Id)
        {
            var yritus = yritusedRepository.Yritused.Where(y => y.Id == Id).SingleOrDefault();

            return Json(yritus);
        }
        public IActionResult GetYritusByYrituseNimi(string yrituseNimi)
        {
            Yritus yritus = new();

            foreach (Yritus y in yritusedRepository.Yritused)
            {
                if ((y.YrituseNimi + " " + y.YrituseAeg.ToString("dd.MM.yyyy HH:mm") + " " + y.YrituseKoht).ToLower().Contains(yrituseNimi.ToLower(), StringComparison.CurrentCulture))
                {
                    yritus = y;
                    break;
                }
            }

            return Json(yritus);
        }
        [HttpPost]
        public IActionResult SaveYritus([FromBody]Yritus yritus)
        {
            var yritused_baasis = yritusedRepository.Yritused.Where(y => (y.YrituseNimi ?? string.Empty).ToLower() == (yritus.YrituseNimi ?? string.Empty)
                .ToLower() && y.YrituseAeg == yritus.YrituseAeg && (y.YrituseKoht ?? string.Empty).ToLower() == (yritus.YrituseKoht ?? string.Empty).ToLower() && y.Id != yritus.Id).ToList();

            if (yritused_baasis.Count() > 0)
            {
                return Json("Üritus juba eksisteerib!");
            }

            if (yritus.Id != 0)
            {
                var yritus_baasis = yritusedRepository.Yritused.Where(y => y.Id == yritus.Id).SingleOrDefault();

                if ((yritus_baasis ?? new Yritus()).YrituseAeg < DateTime.Now)
                {
                    return Json("Ürituse toimumise aeg on juba möödunud!");
                }
            }

            yritusedRepository.SaveYritus(yritus);

            return Json("OK");
        }
        public IActionResult DeleteYritus(int Id, int pageNr)
        {
            yritusedRepository.DeleteYritus(Id);

            return RedirectToAction("List", new { p = pageNr });
        }
        public IActionResult GetAutocompleteByYrituseNimi(string yrituseNimi_fr)
        {
            List<string> yritused = [];

            foreach (Yritus y in yritusedRepository.Yritused.OrderBy(o => o.YrituseNimi).ThenBy(o => o.YrituseAeg).ThenBy(o => o.YrituseKoht))
            {
                if (yrituseNimi_fr == null && y.YrituseAeg > DateTime.Now)
                {
                    yritused.Add(y.YrituseNimi + " " + y.YrituseAeg.ToString("dd.MM.yyyy HH:mm") + " " + y.YrituseKoht);
                }
                else if (y.YrituseAeg > DateTime.Now && (y.YrituseNimi + " " + y.YrituseAeg.ToString("dd.MM.yyyy HH:mm") + " " + y.YrituseKoht).ToLower()
                    .Contains((yrituseNimi_fr ?? string.Empty).ToLower(), StringComparison.CurrentCulture))
                {
                    yritused.Add(y.YrituseNimi + " " + y.YrituseAeg.ToString("dd.MM.yyyy HH:mm") + " " + y.YrituseKoht);
                }
            }

            return Json(yritused);
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
        private IEnumerable<Yritus> FilteredYritused(string? filterField, string? filterValue, string? sortField, Utilites.Order listOrder)
        {
            if (filterField != null && filterField.StartsWith("Id_"))
            {
                var Id = Convert.ToInt32(filterValue);

                return yritusedRepository.Yritused.Where(y => y.Id == Id).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseNimi_"))
            {
                return yritusedRepository.Yritused.Where(y => !string.IsNullOrEmpty(y.YrituseNimi) && (y.YrituseNimi ?? "").ToLower().Contains((filterValue ?? "").ToLower())).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseKoht_"))
            {
                return yritusedRepository.Yritused.Where(y => !string.IsNullOrEmpty(y.YrituseKoht) && (y.YrituseKoht ?? "").ToLower().Contains((filterValue ?? "").ToLower())).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Lisainfo_"))
            {
                return yritusedRepository.Yritused.Where(y => !string.IsNullOrEmpty(y.Lisainfo) && (y.Lisainfo ?? "").ToLower().Contains((filterValue ?? "").ToLower())).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Osavotjaid_") && (filterValue ?? "").StartsWith(">="))
            {
                var osavotjaid = Convert.ToInt32((filterValue ?? "").Replace(">=", "").Replace(" ", ""));

                return yritusedRepository.Yritused.Where(y => y.Osavotjaid >= osavotjaid).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Osavotjaid_") && (filterValue ?? "").StartsWith(">"))
            {
                var osavotjaid = Convert.ToInt32((filterValue ?? "").Replace(">", "").Replace(" ", ""));

                return yritusedRepository.Yritused.Where(y => y.Osavotjaid > osavotjaid).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Osavotjaid_") && (filterValue ?? "").StartsWith("<="))
            {
                var osavotjaid = Convert.ToInt32((filterValue ?? "").Replace("<=", "").Replace(" ", ""));

                return yritusedRepository.Yritused.Where(y => y.Osavotjaid <= osavotjaid).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Osavotjaid_") && (filterValue ?? "").StartsWith("<"))
            {
                var osavotjaid = Convert.ToInt32((filterValue ?? "").Replace("<", "").Replace(" ", ""));

                return yritusedRepository.Yritused.Where(y => y.Osavotjaid <= osavotjaid).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Osavotjaid_"))
            {
                var osavotjaid = Convert.ToInt32((filterValue ?? "").Replace("<", "").Replace(" ", ""));

                return yritusedRepository.Yritused.Where(y => y.Osavotjaid == osavotjaid).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith(">="))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.YrituseAeg >= yrituseaeg).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith(">"))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.YrituseAeg > yrituseaeg).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith("<="))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.YrituseAeg <= yrituseaeg).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith("<"))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.YrituseAeg < yrituseaeg).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_"))
            {
                var yrituseaeg = Convert.ToDateTime(filterValue ?? "");

                return yritusedRepository.Yritused.Where(y => y.YrituseAeg == yrituseaeg).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.Loodud >= loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.Loodud > loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.Loodud <= loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.Loodud < loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_"))
            {
                var loodud = Convert.ToDateTime(filterValue ?? "");

                return yritusedRepository.Yritused.Where(y => y.Loodud == loodud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.Muudetud >= muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.Muudetud > muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.Muudetud <= muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return yritusedRepository.Yritused.Where(y => y.Muudetud < muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_"))
            {
                var muudetud = Convert.ToDateTime(filterValue ?? "");

                return yritusedRepository.Yritused.Where(y => y.Muudetud == muudetud).OrderByDynamic(sortField ?? "Id", listOrder);
            }

            return yritusedRepository.Yritused.OrderByDynamic(sortField ?? "Id", listOrder);
        }
        private IEnumerable<Yritus> FilteredYritusedSecondRound(IEnumerable<Yritus>? yritused, string? filterField, string? filterValue)
        {
            if (filterField != null && filterField.StartsWith("Id_"))
            {
                var Id = Convert.ToInt32(filterValue);

                return (yritused ?? []).Where(y => y.Id == Id);
            }
            else if (filterField != null && filterField.StartsWith("YrituseNimi_"))
            {
                return (yritused ?? []).Where(y => !string.IsNullOrEmpty(y.YrituseNimi) && (y.YrituseNimi ?? "").ToLower().Contains((filterValue ?? "").ToLower()));
            }
            else if (filterField != null && filterField.StartsWith("YrituseKoht_"))
            {
                return (yritused ?? []).Where(y => !string.IsNullOrEmpty(y.YrituseKoht) && (y.YrituseKoht ?? "").ToLower().Contains((filterValue ?? "").ToLower()));
            }
            else if (filterField != null && filterField.StartsWith("Lisainfo_"))
            {
                return (yritused ?? []).Where(y => !string.IsNullOrEmpty(y.Lisainfo) && (y.Lisainfo ?? "").ToLower().Contains((filterValue ?? "").ToLower()));
            }
            else if (filterField != null && filterField.StartsWith("Osavotjaid_") && (filterValue ?? "").StartsWith(">="))
            {
                var osavotjaid = Convert.ToInt32((filterValue ?? "").Replace(">=", "").Replace(" ", ""));

                return (yritused ?? []).Where(y => y.Osavotjaid >= osavotjaid);
            }
            else if (filterField != null && filterField.StartsWith("Osavotjaid_") && (filterValue ?? "").StartsWith(">"))
            {
                var osavotjaid = Convert.ToInt32((filterValue ?? "").Replace(">", "").Replace(" ", ""));

                return (yritused ?? []).Where(y => y.Osavotjaid > osavotjaid);
            }
            else if (filterField != null && filterField.StartsWith("Osavotjaid_") && (filterValue ?? "").StartsWith("<="))
            {
                var osavotjaid = Convert.ToInt32((filterValue ?? "").Replace("<=", "").Replace(" ", ""));

                return (yritused ?? []).Where(y => y.Osavotjaid <= osavotjaid);
            }
            else if (filterField != null && filterField.StartsWith("Osavotjaid_") && (filterValue ?? "").StartsWith("<"))
            {
                var osavotjaid = Convert.ToInt32((filterValue ?? "").Replace("<", "").Replace(" ", ""));

                return (yritused ?? []).Where(y => y.Osavotjaid <= osavotjaid);
            }
            else if (filterField != null && filterField.StartsWith("Osavotjaid_"))
            {
                var osavotjaid = Convert.ToInt32((filterValue ?? "").Replace("<", "").Replace(" ", ""));

                return (yritused ?? []).Where(y => y.Osavotjaid == osavotjaid);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith(">="))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return (yritused ?? []).Where(y => y.YrituseAeg >= yrituseaeg);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith(">"))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return (yritused ?? []).Where(y => y.YrituseAeg > yrituseaeg);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith("<="))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return (yritused ?? []).Where(y => y.YrituseAeg <= yrituseaeg);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_") && (filterValue ?? "").StartsWith("<"))
            {
                var yrituseaeg = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return (yritused ?? []).Where(y => y.YrituseAeg < yrituseaeg);
            }
            else if (filterField != null && filterField.StartsWith("YrituseAeg_"))
            {
                var yrituseaeg = Convert.ToDateTime(filterValue ?? "");

                return (yritused ?? []).Where(y => y.YrituseAeg == yrituseaeg);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return (yritused ?? []).Where(y => y.Loodud >= loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith(">"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return (yritused ?? []).Where(y => y.Loodud > loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<="))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return (yritused ?? []).Where(y => y.Loodud <= loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_") && (filterValue ?? "").StartsWith("<"))
            {
                var loodud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return (yritused ?? []).Where(y => y.Loodud < loodud);
            }
            else if (filterField != null && filterField.StartsWith("Loodud_"))
            {
                var loodud = Convert.ToDateTime(filterValue ?? "");

                return (yritused ?? []).Where(y => y.Loodud == loodud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">=", "").Trim());

                return (yritused ?? []).Where(y => y.Muudetud >= muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith(">"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace(">", "").Trim());

                return (yritused ?? []).Where(y => y.Muudetud > muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<="))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<=", "").Trim());

                return (yritused ?? []).Where(y => y.Muudetud <= muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_") && (filterValue ?? "").StartsWith("<"))
            {
                var muudetud = Convert.ToDateTime((filterValue ?? "").Replace("<", "").Trim());

                return (yritused ?? []).Where(y => y.Muudetud < muudetud);
            }
            else if (filterField != null && filterField.StartsWith("Muudetud_"))
            {
                var muudetud = Convert.ToDateTime(filterValue ?? "");

                return (yritused ?? []).Where(y => y.Muudetud == muudetud);
            }

            return yritusedRepository.Yritused;
        }
    }
}
