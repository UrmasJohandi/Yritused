using Microsoft.AspNetCore.Mvc;
using Yritused.Infrastructure;
using Yritused.Models;
using Yritused.Models.Viewmodels;

namespace Yritused.Controllers
{
    public class OsavotjaController : Controller
    {
        private readonly IOsavotjaRepository osavotjadRepository;
        private readonly IHttpContextAccessor httpContextAccessor;

        public OsavotjaController(IHttpContextAccessor httpContextAcc, IOsavotjaRepository osavotjadRepo)
        {
            httpContextAccessor = httpContextAcc;
            osavotjadRepository = osavotjadRepo;
        }
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

            string[] filterFields = Array.Empty<string>();
            string[] filterValues = Array.Empty<string>();

            filterFields = string.IsNullOrEmpty(filterField) ? filterFields : filterField.Split(';');
            filterValues = string.IsNullOrEmpty(filterValue) ? filterValues : filterValue.Split(';');

            IEnumerable<Osavotja>? osavotjad = null;
            if (string.IsNullOrEmpty(filterField))
            {
                osavotjad = osavotjadRepository.Osavotjad.OrderByDynamic(sortField, listOrder);
            }
            else if (filterFields.Length == 1)
            {
                osavotjad = filteredOsavotjad(filterField, filterValue, sortField, listOrder);
            }
            else
            {
                for (int j = 0; j < filterFields.Length; j++)
                {
                    osavotjad = j == 0 ? filteredOsavotjad(filterFields[j], filterValues[j], sortField, listOrder) :
                        filteredOsavotjadSecondRound(osavotjad, filterFields[j], filterValues[j]);
                }
            }

            OsavotjadListViewModel osavotjadListViewModel = new OsavotjadListViewModel
            {
                Osavotjad = (osavotjad == null ? new List<Osavotja>() : osavotjad)
                    .Skip((p - 1) * pageSize)
                    .Take(pageSize),
                PagingInfo = new PagingModel
                {
                    PageNo = p,
                    PageSize = pageSize,
                    TotalRecords = osavotjad == null ? 0 : osavotjad.Count()
                },
                Path = httpContextAccessor.HttpContext == null ? string.Empty : httpContextAccessor.HttpContext.Request.Path.Value,
                filterField = filterField,
                filterValue = filterValue,
                orderBy = string.IsNullOrEmpty(filterField) ? sortField + " " + (listOrder == Utilites.Order.Asc ? "asc" : "desc") :
                    sortField + " " + (listOrder == Utilites.Order.Asc ? "asc" : "desc")
            };

            return osavotjadListViewModel;
        }
        private IEnumerable<Osavotja> filteredOsavotjad(string? filterField, string? filterValue, string? sortField, Utilites.Order listOrder)
        {
            return osavotjadRepository.Osavotjad;
        }
        private IEnumerable<Osavotja> filteredOsavotjadSecondRound(IEnumerable<Osavotja>? yritused, string? filteredField, string? filterValue)
        {
            return osavotjadRepository.Osavotjad;
        }
    }
}
