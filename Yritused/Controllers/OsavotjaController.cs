using Microsoft.AspNetCore.Mvc;
using Yritused.Infrastructure;
using Yritused.Models;
using Yritused.Models.Viewmodels;

namespace Yritused.Controllers
{
    public class OsavotjaController(IHttpContextAccessor httpContextAcc, IOsavotjaRepository osavotjadRepo) : Controller
    {
        private readonly IOsavotjaRepository osavotjadRepository = osavotjadRepo;
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

            int pageSize = Utilites.GetPageSize(s);

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
        private IEnumerable<Osavotja> FilteredOsavotjad(string? filterField, string? filterValue, string? sortField, Utilites.Order listOrder)
        {
            return osavotjadRepository.Osavotjad;
        }
        private IEnumerable<Osavotja> FilteredOsavotjadSecondRound(IEnumerable<Osavotja>? yritused, string? filteredField, string? filterValue)
        {
            return osavotjadRepository.Osavotjad;
        }
    }
}
