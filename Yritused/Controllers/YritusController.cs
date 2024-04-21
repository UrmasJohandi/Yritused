using Microsoft.AspNetCore.Mvc;
using Yritused.Infrastructure;
using Yritused.Models;
using Yritused.Models.Viewmodels;

namespace Yritused.Controllers
{
    public class YritusController : Controller
    {
        private readonly IYritusRepository yritusedRepository;
        private readonly IHttpContextAccessor httpContextAccessor;

        public YritusController(IHttpContextAccessor httpContextAcc, IYritusRepository yritusedRepo) 
        {
            httpContextAccessor = httpContextAcc;
            yritusedRepository = yritusedRepo;
        }

        public IActionResult List(int p = 1, string? orderby = null, string? orderbybefore = null, int s = 0, string? filterField = null, string? filterValue = null)
        {
            return View("List", getViewModel(p, orderby, orderbybefore, s, filterField, filterValue));
        }

        private YritusedListViewModel getViewModel(int p, string? orderby, string? orderbybefore, int s, string? filterField, string? filterValue)
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

            IEnumerable<Yritus>? yritused = null;
            if (string.IsNullOrEmpty(filterField))
            {
                yritused = yritusedRepository.Yritused.OrderByDynamic(sortField, listOrder);
            }
            else if (filterFields.Length == 1)
            {
                yritused = filteredYritused(filterField, filterValue, sortField, listOrder);
            }
            else
            {
                for (int j = 0; j < filterFields.Length; j++)
                {
                    if (j == 0)
                    {
                        yritused = filteredYritused(filterFields[j], filterValues[j], sortField, listOrder);
                    }
                    else
                    {
                        yritused = filteredYritusedSecondRound(yritused, filterFields[j], filterValues[j]);
                    }
                }
            }

            YritusedListViewModel yritusedListViewModel = new YritusedListViewModel
            {
                Yritused = (yritused == null ? new List<Yritus>() : yritused)
                    .Skip((p - 1) * pageSize)
                    .Take(pageSize),
                PagingInfo = new PagingModel
                {
                    PageNo = p,
                    PageSize = pageSize,
                    TotalRecords = yritused == null ? 0 : yritused.Count()
                },
                Path = httpContextAccessor.HttpContext == null ? string.Empty : httpContextAccessor.HttpContext.Request.Path.Value,
                filterField = filterField,
                filterValue = filterValue,
                orderBy = string.IsNullOrEmpty(filterField) ? sortField + " " + (listOrder == Utilites.Order.Asc ? "asc" : "desc") :
                    sortField + " " + (listOrder == Utilites.Order.Asc ? "asc" : "desc")
            };

            return yritusedListViewModel;
        }
        private IEnumerable<Yritus> filteredYritused(string? filterField, string? filterValue, string? sortField, Utilites.Order listOrder)
        {
            return yritusedRepository.Yritused;
        }
        private IEnumerable<Yritus> filteredYritusedSecondRound(IEnumerable<Yritus>? yritused, string? filteredField, string? filterValue)
        {
            return yritusedRepository.Yritused;
        }
    }
}
