using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Yritused.Infrastructure
{
    [HtmlTargetElement("filtercell")]
    public class FilterCellTagHelper : TagHelper
    {
        public string? Class { get; set; }
        public string? Name { get; set; }
        public int Id { get; set; }
        public string? Value { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            TagBuilder input = new TagBuilder("input");
            input.Attributes["class"] = $"{Class} form-control form-control-sm";
            input.Attributes["id"] = $"input_{Name}_{Id}";
            input.Attributes["value"] = $"{Value}";
            input.Attributes["spellcheck"] = "false";
            if ((Class ?? string.Empty).Contains("right-justified"))
            {
                input.Attributes["style"] = "float: right !important;";
            }

            TagBuilder hiddenSpan = new TagBuilder("span");
            hiddenSpan.Attributes["id"] = $"span_{Name}_{Id}";
            hiddenSpan.Attributes["style"] = "display: none;";
            hiddenSpan.InnerHtml.AppendHtml(input);

            TagBuilder span = new TagBuilder("span");
            span.Attributes["id"] = $"{Name}_{Id}";
            span.InnerHtml.Append($"{Value}");

            output.TagName = "td";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", $"{Class}");
            output.Attributes.SetAttribute("id", $"{Name}_cell_{Id}");
            output.Content.AppendHtml(span);
            output.Content.AppendHtml(hiddenSpan);
        }
    }
}
