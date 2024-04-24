using System.Text.RegularExpressions;
using System.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Yritused.Infrastructure
{
    [HtmlTargetElement("filtercelltooltip")]
    public class FilterCellTooltipTagHelper : TagHelper
    {
        public string? Class { get; set; }
        public string? Name { get; set; }
        public int Id { get; set; }
        public string? Value { get; set; }
        public string? Toggle { get; set; }
        public string? Placement { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string noHtmlValue = WebUtility.HtmlDecode(Regex.Replace(Value ?? string.Empty, @"<[^>]+>|&nbsp;", "").Trim());
            string noHtmlTitle = WebUtility.HtmlDecode(Regex.Replace(Title ?? string.Empty, @"<[^>]+>|&nbsp;", "").Trim());

            TagBuilder input = new("input");
            input.Attributes["class"] = $"{Class} form-control form-control-sm";
            input.Attributes["id"] = $"input_{Name}_{Id}";
            input.Attributes["value"] = $"{noHtmlTitle}";
            input.Attributes["spellcheck"] = "false";

            TagBuilder hiddenSpan = new("span");
            hiddenSpan.Attributes["id"] = $"span_{Name}_{Id}";
            hiddenSpan.Attributes["style"] = "display: none;";
            hiddenSpan.InnerHtml.AppendHtml(input);

            TagBuilder span = new("span");
            span.Attributes["id"] = $"{Name}_{Id}";
            span.InnerHtml.Append($"{noHtmlValue}");

            output.TagName = "td";
            output.TagMode = TagMode.StartTagAndEndTag;

            output.Attributes.SetAttribute("class", $"{Class}");
            output.Attributes.SetAttribute("id", $"{Name}_cell_{Id}");
            output.Attributes.SetAttribute("data-toggle", Toggle);
            output.Attributes.SetAttribute("data-placement", Placement);
            output.Attributes.SetAttribute("data-type", Type);
            output.Attributes.SetAttribute("title", Title);

            output.Content.AppendHtml(span);
            output.Content.AppendHtml(hiddenSpan);
        }
    }
}
