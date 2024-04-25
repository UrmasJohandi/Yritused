using Microsoft.AspNetCore.Mvc;
using Yritused.Infrastructure;
using Yritused.Models;
using Yritused.Models.Viewmodels;

namespace Yritused.Controllers
{
    public class YritusController(IHttpContextAccessor httpContextAcc, IYritusRepository yritusedRepo) : Controller
    {
        private readonly IYritusRepository yritusedRepository = yritusedRepo;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAcc;

        public IActionResult List(int p = 1, string? orderby = null, string? orderbybefore = null, int s = 0, string? filterField = null, string? filterValue = null)
        {
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

            int pageSize = Utilites.GetPageSize(s);

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
                Yritused = (yritused ?? new List<Yritus>())
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
        [HttpPost]
        public IActionResult SaveYritus([FromBody]Yritus yritus)
        {
            yritusedRepository.SaveYritus(yritus);

            return Json("OK");
        }
        private IEnumerable<Yritus> FilteredYritused(string? filterField, string? filterValue, string? sortField, Utilites.Order listOrder)
        {
            return yritusedRepository.Yritused;
        }
        private IEnumerable<Yritus> FilteredYritusedSecondRound(IEnumerable<Yritus>? yritused, string? filteredField, string? filterValue)
        {
            return yritusedRepository.Yritused;
        }
    }
}
