using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Yritused.Infrastructure
{
    public class PagingTagHelper(IConfiguration configuration, ILogger<PagingTagHelper> logger) : TagHelper
    {
        private IConfiguration Configuration { get; } = configuration;
        private readonly ILogger _logger = logger;

        #region Settings
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int MaxDisplayedPages { get; set; }
        public int GapSize { get; set; }
        public string SettingsJson { get; set; }

        #endregion

        #region Page size navigation
        public string PageSizeNavFormMethod { get; set; }
        public int PageSizeNavBlockSize { get; set; }
        public int PageSizeNavMaxItems { get; set; }
        public string PageSizeNavOnChange { get; set; }

        #endregion

        #region QueryString
        public string QueryStringKeyPageNo { get; set; }
        public string QueryStringKeyPageSize { get; set; }
        public string QueryStringValue { get; set; }

        #endregion

        #region Display settings
        public bool? ShowPageSizeNav { get; set; }
        public bool? ShowFirstLast { get; set; }
        public bool? ShowPrevNext { get; set; }
        public bool? ShowTotalPages { get; set; }
        public bool? ShowTotalRecords { get; set; }
        public bool? ShowLastNumberedPage { get; set; }
        public bool? ShowFirstNumberedPage { get; set; }

        #endregion

        #region Texts
        public string TextPageSize { get; set; }
        public string TextFirst { get; set; }
        public string TextLast { get; set; }
        public string TextNext { get; set; }
        public string TextPrevious { get; set; }
        public string TextTotalPages { get; set; }
        public string TextTotalRecords { get; set; }
        #endregion

        #region Screen Reader
        public string SrTextFirst { get; set; }
        public string SrTextLast { get; set; }
        public string SrTextNext { get; set; }
        public string SrTextPrevious { get; set; }

        #endregion

        #region Styling
        public string Class { get; set; }
        public string ClassPagingControlDiv { get; set; }
        public string ClassInfoDiv { get; set; }
        public string ClassPageSizeDiv { get; set; }
        public string ClassPagingControl { get; set; }
        public string ClassActivePage { get; set; }
        public string ClassDisabledJumpingButton { get; set; }
        public string ClassTotalRecords { get; set; }
        public string ClassTotalPages { get; set; }

        #endregion
        public string category { get; set; }
        private int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            SetDefaults();

            if (TotalPages > 0)
            {
                var pagingControl = new TagBuilder("ul");
                pagingControl.AddCssClass($"{ClassPagingControl}");

                if (ShowFirstLast != true && TotalPages > MaxDisplayedPages)
                {
                    ShowFirstLast = true;
                }

                if (ShowFirstLast == true)
                {
                    var first = CreatePagingLink(1, TextFirst, SrTextFirst, ClassDisabledJumpingButton);
                    pagingControl.InnerHtml.AppendHtml(first);
                }

                if (ShowPrevNext == true)
                {
                    var prevPage = PageNo - 1 <= 1 ? 1 : PageNo - 1;
                    var prev = CreatePagingLink(prevPage, TextPrevious, SrTextPrevious, ClassDisabledJumpingButton);
                    pagingControl.InnerHtml.AppendHtml(prev);
                }

                int start = 1;
                int end = MaxDisplayedPages;

                (start, end) = CalculateBoundaries(PageNo, TotalPages, MaxDisplayedPages);

                if (ShowFirstNumberedPage == true
                    && start > GapSize
                    && TotalPages > MaxDisplayedPages
                    && PageNo >= MaxDisplayedPages)
                {
                    var numTag = CreatePagingLink(1, null, null, ClassActivePage);
                    pagingControl.InnerHtml.AppendHtml(numTag);

                    var gap = new TagBuilder("li");
                    gap.AddCssClass("page-item border-0");
                    gap.InnerHtml.AppendHtml("&nbsp;...&nbsp;");
                    pagingControl.InnerHtml.AppendHtml(gap);
                }

                for (int i = start; i <= end; i++)
                {
                    var numTag = CreatePagingLink(i, null, null, ClassActivePage);
                    pagingControl.InnerHtml.AppendHtml(numTag);
                }

                if (ShowLastNumberedPage == true
                    && TotalPages - end >= GapSize
                    && PageNo - GapSize <= TotalPages - MaxDisplayedPages)
                {
                    var gap = new TagBuilder("li");
                    gap.AddCssClass("page-item border-0");
                    gap.InnerHtml.AppendHtml("&nbsp;...&nbsp;");
                    pagingControl.InnerHtml.AppendHtml(gap);

                    var numTag = CreatePagingLink(TotalPages, null, null, ClassActivePage);
                    pagingControl.InnerHtml.AppendHtml(numTag);
                }

                if (ShowPrevNext == true)
                {
                    var nextPage = PageNo + 1 > TotalPages ? TotalPages : PageNo + 1;
                    var next = CreatePagingLink(nextPage, TextNext, SrTextNext, ClassDisabledJumpingButton);
                    pagingControl.InnerHtml.AppendHtml(next);
                }

                if (ShowFirstLast == true)
                {
                    var last = CreatePagingLink(TotalPages, TextLast, SrTextLast, ClassDisabledJumpingButton);
                    pagingControl.InnerHtml.AppendHtml(last);
                }

                var pagingControlDiv = new TagBuilder("div");
                pagingControlDiv.AddCssClass($"{ClassPagingControlDiv}");
                pagingControlDiv.InnerHtml.AppendHtml(pagingControl);

                output.TagName = "div";
                output.Attributes.SetAttribute("class", $"{Class}");
                output.Content.AppendHtml(pagingControlDiv);

                if (ShowTotalPages == true || ShowTotalRecords == true)
                {
                    var infoDiv = new TagBuilder("div");
                    infoDiv.AddCssClass($"{ClassInfoDiv}");

                    if (ShowTotalPages == true)
                    {
                        var totalPagesInfo = AddDisplayInfo(TotalPages, TotalPages > 1 ? TextTotalPages : "page", ClassTotalPages);
                        infoDiv.InnerHtml.AppendHtml(totalPagesInfo);
                    }

                    if (ShowTotalRecords == true)
                    {
                        var totalRecordsInfo = AddDisplayInfo(TotalRecords, TotalRecords > 1 ? TextTotalRecords : "record", ClassTotalRecords);
                        infoDiv.InnerHtml.AppendHtml(totalRecordsInfo);
                    }

                    output.Content.AppendHtml(infoDiv);
                }

                if (ShowPageSizeNav == true)
                {
                    var psDropdown = CreatePageSizeControl();

                    var psDiv = new TagBuilder("div");
                    psDiv.AddCssClass($"{ClassPageSizeDiv}");
                    psDiv.InnerHtml.AppendHtml(psDropdown);

                    output.Content.AppendHtml(psDiv);
                }
            }
        }
        private void SetDefaults()
        {
            var _settingsJson = SettingsJson ?? "default";

            _logger.LogInformation($"----> PagingTagHelper SettingsJson: {SettingsJson} - {_settingsJson}");

            PageNo = PageNo > 1 ? PageNo :
                int.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:page-no"], out int _pn) ? _pn : 1;
            PageSize = PageSize > 0 ? PageSize :
                int.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:page-size"], out int _ps) ? _ps : 10;
            TotalRecords = TotalRecords > 0 ? TotalRecords :
                int.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:total-records"], out int _tr) ? _tr : 0;
            MaxDisplayedPages = MaxDisplayedPages > 0 ? MaxDisplayedPages :
                int.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:max-displayed-pages"], out int _dp) ? _dp : 10;
            GapSize = GapSize > 0 ? GapSize :
                int.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:gap-size"], out int _gap) ? _gap : 3;
            PageSizeNavFormMethod ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:page-size-nav-form-method"] ?? "get";
            PageSizeNavBlockSize = PageSizeNavBlockSize > 0 ? PageSizeNavBlockSize :
                int.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:page-size-nav-block-size"], out int _bs) ? _bs : 10;
            PageSizeNavMaxItems = PageSizeNavMaxItems > 0 ? PageSizeNavMaxItems :
                int.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:page-size-nav-max-items"], out int _mi) ? _mi : 3;
            PageSizeNavOnChange ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:page-size-nav-on-change"] ?? "this.form.submit();";
            QueryStringKeyPageNo ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:query-string-key-page-no"] ?? "p";
            QueryStringKeyPageSize ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:query-string-key-page-size"] ?? "s";
            QueryStringValue ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:query-string-value"] ?? "";
            ShowFirstLast = ShowFirstLast == null ?
                bool.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:show-first-last"], out bool _sfl) ? _sfl : false : ShowFirstLast;
            ShowPrevNext = ShowPrevNext == null ? bool.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:show-prev-next"], out bool _sprn) ? _sprn : false : ShowPrevNext;
            ShowPageSizeNav = ShowPageSizeNav == null ? bool.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:show-page-size-nav"], out bool _spsn) ? _spsn : false : ShowPageSizeNav;
            ShowTotalPages = ShowTotalPages == null ? bool.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:show-total-pages"], out bool _stp) ? _stp : false : ShowTotalPages;
            ShowTotalRecords = ShowTotalRecords == null ? bool.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:show-total-records"], out bool _str) ? _str : false : ShowTotalRecords;
            ShowFirstNumberedPage = ShowFirstNumberedPage == null ? bool.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:show-first-numbered-page"], out bool _sfp) ? _sfp : false : ShowFirstNumberedPage;
            ShowLastNumberedPage = ShowLastNumberedPage == null ? bool.TryParse(Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:show-last-numbered-page"], out bool _slp) ? _slp : false : ShowLastNumberedPage;
            TextPageSize ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:text-page-size"] ?? "Items per page";
            TextFirst ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:text-first"] ?? "&laquo;";
            TextLast ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:text-last"] ?? "&raquo;";
            TextPrevious ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:text-previous"] ?? "&lsaquo;";
            TextNext ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:text-next"] ?? "&rsaquo;";
            TextTotalPages ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:text-total-pages"] ?? "pages";
            TextTotalRecords ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:text-total-records"] ?? "records";
            SrTextFirst = SrTextFirst ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:sr-text-first"] ?? "First";
            SrTextLast = SrTextLast ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:sr-text-last"] ?? "Last";
            SrTextPrevious = SrTextPrevious ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:sr-text-previous"] ?? "Previous";
            SrTextNext = SrTextNext ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:sr-text-next"] ?? "Next";
            Class = Class ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:class"] ?? "row";
            ClassActivePage = ClassActivePage ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:class-active-page"] ?? "active";
            ClassDisabledJumpingButton = ClassDisabledJumpingButton ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:class-disabled-jumping-button"] ?? "disabled";
            ClassInfoDiv = ClassInfoDiv ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:class-info-div"] ?? "col";
            ClassPageSizeDiv = ClassPageSizeDiv ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:class-page-size-div"] ?? "col";
            ClassPagingControlDiv = ClassPagingControlDiv ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:class-paging-control-div"] ?? "col";
            ClassPagingControl = ClassPagingControl ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:class-paging-control"] ?? "pagination";
            ClassTotalPages = ClassTotalPages ?? Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:class-total-pages"] ?? "badge badge-secondary";
            ClassTotalRecords ??= Configuration[$"Paginator:PagingTagHelper:{_settingsJson}:class-total-records"] ?? "badge badge-info";

            _logger.LogInformation($"----> PagingTagHelper - " +
                $"{nameof(PageNo)}: {PageNo}, " +
                $"{nameof(PageSize)}: {PageSize}, " +
                $"{nameof(TotalRecords)}: {TotalRecords}, " +
                $"{nameof(TotalPages)}: {TotalPages}, " +
                $"{nameof(QueryStringKeyPageNo)}: {QueryStringKeyPageNo}, " +
                $"{nameof(QueryStringKeyPageSize)}: {QueryStringKeyPageSize}, " +
                $"{nameof(QueryStringValue)}: {QueryStringValue}" +
                $"");
        }
        private TagBuilder AddDisplayInfo(int count, string itemName, string cssClassName)
        {
            var span = new TagBuilder("span");
            span.AddCssClass($"{cssClassName}");
            span.InnerHtml.AppendHtml($"{count.ToString("N0")} {itemName}");

            return span;
        }
        private (int start, int end) CalculateBoundaries(int currentPageNo, int totalPages, int maxDisplayedPages)
        {
            var _start = 1;
            var _end = maxDisplayedPages;
            var _gap = (int)Math.Ceiling(maxDisplayedPages / 2.0);

            if (maxDisplayedPages > totalPages)
                maxDisplayedPages = totalPages;

            if (currentPageNo < maxDisplayedPages)
            {
                _start = 1;
                _end = maxDisplayedPages;
            }
            else if (currentPageNo + maxDisplayedPages > totalPages)
            {
                _start = totalPages - maxDisplayedPages > 0 ? totalPages - maxDisplayedPages : 1;
                _end = totalPages;
            }
            else
            {
                _start = currentPageNo - _gap > 0 ? currentPageNo - _gap : 1;
                _end = _start + maxDisplayedPages;
            }

            return (_start, _end);
        }
        private TagBuilder CreatePagingLink(int targetPageNo, string text, string textSr, string pClass)
        {
            var liTag = new TagBuilder("li");
            liTag.AddCssClass("page-item");

            var aTag = new TagBuilder("a");
            aTag.AddCssClass("page-link");
            aTag.Attributes.Add("href", CreateUrlTemplate(targetPageNo, PageSize, QueryStringValue));

            if (string.IsNullOrWhiteSpace(textSr))
            {
                aTag.InnerHtml.Append($"{targetPageNo}");
            }
            else
            {
                liTag.MergeAttribute("area-label", textSr);
                aTag.InnerHtml.AppendHtml($"<span area-hidden=\"true\">{text}</span>");
                aTag.InnerHtml.AppendHtml($"<span class=\"sr-only\">{textSr}</span>");
            }

            if (PageNo == targetPageNo)
            {
                liTag.AddCssClass($"{pClass}");
                aTag.Attributes.Add("tabindex", "-1");
                aTag.Attributes.Remove("href");
            }

            liTag.InnerHtml.AppendHtml(aTag);
            return liTag;
        }
        private TagBuilder CreatePageSizeControl()
        {
            var dropDown = new TagBuilder("select");
            dropDown.AddCssClass($"form-control");
            dropDown.Attributes.Add("name", QueryStringKeyPageSize);
            dropDown.Attributes.Add("onchange", $"{PageSizeNavOnChange}");

            for (int i = 1; i <= PageSizeNavMaxItems; i++)
            {
                var option = new TagBuilder("option");
                option.InnerHtml.AppendHtml($"{i * PageSizeNavBlockSize}");

                if ((i * PageSizeNavBlockSize) == PageSize)
                    option.Attributes.Add("selected", "selected");

                dropDown.InnerHtml.AppendHtml(option);
            }

            var fGroup = new TagBuilder("div");
            fGroup.AddCssClass("form-group");

            var label = new TagBuilder("label");
            label.Attributes.Add("for", "pageSizeControl");
            label.InnerHtml.AppendHtml($"{TextPageSize}&nbsp;");
            fGroup.InnerHtml.AppendHtml(label);
            fGroup.InnerHtml.AppendHtml(dropDown);

            var form = new TagBuilder("form");
            form.AddCssClass("form-inline");
            form.Attributes.Add("method", PageSizeNavFormMethod);
            form.InnerHtml.AppendHtml(fGroup);

            return form;
        }
        private string CreateUrlTemplate(int pageNo, int pageSize, string urlPath)
        {
            string p = $"{QueryStringKeyPageNo}={pageNo}";
            string s = $"{QueryStringKeyPageSize}={pageSize}";

            var urlTemplate = urlPath.TrimStart('?').Split('&').ToList();

            for (int i = 0; i < urlTemplate.Count; i++)
            {
                var q = urlTemplate[i];
                urlTemplate[i] =
                    q.StartsWith($"{QueryStringKeyPageNo}=") ? p :
                    q.StartsWith($"{QueryStringKeyPageSize}=") ? s :
                    q;
            }

            if (!urlTemplate.Any(x => x == p))
                urlTemplate.Add(p);

            if (!urlTemplate.Any(x => x == s))
                urlTemplate.Add(s);

            return "?" + string.Join('&', urlTemplate);
        }
    }
}
