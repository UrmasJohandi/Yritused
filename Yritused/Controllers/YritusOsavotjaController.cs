using Microsoft.AspNetCore.Mvc;
using Yritused.Infrastructure;
using Yritused.Models;
using Yritused.Models.Viewmodels;

namespace Yritused.Controllers
{
    public class YritusOsavotjaController(IHttpContextAccessor httpContextAcc, IYritusOsavotjaRepository yritusOsavotjadRepo, IYritusRepository yritusedRepo, IOsavotjaRepository osavotjadRepo) : Controller
    {
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAcc;
        private readonly IYritusOsavotjaRepository yritusOsavotjadRepository = yritusOsavotjadRepo;
        private readonly IYritusRepository yritusedRepository = yritusedRepo;
        private readonly IOsavotjaRepository osavotjadRepository = osavotjadRepo;
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

            int pageSize = Utilites.GetPageSize(s);

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
        public IActionResult SaveYritusOsavotja([FromBody]YritusOsavotja yritusOsavotja)
        {
            yritusOsavotjadRepository.SaveYritusOsavotja(yritusOsavotja);

            return Json("OK");
        }
        private IEnumerable<YritusOsavotja> FilteredYritusOsavotjad(string? filterField, string? filterValue, string? sortField, Utilites.Order listOrder)
        {
            return yritusOsavotjadRepository.YritusOsavotjad;
        }
        private IEnumerable<YritusOsavotja> FilteredYritusOsavotjadSecondRound(IEnumerable<YritusOsavotja>? yritused, string? filteredField, string? filterValue)
        {
            return yritusOsavotjadRepository.YritusOsavotjad;
        }
    }
}
