using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Yritused.Infrastructure
{
    [HtmlTargetElement("editablecheckboxcell2")]
    public class EditableCheckboxCell2TagHelper : TagHelper
    {
        public string? CellClass { get; set; }
        public string? Class { get; set; }
        public string? Name { get; set; }
        public int Index { get; set; }
        public bool Checked { get; set; }
        public bool Hidden { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            TagBuilder checkbox = new TagBuilder("input");
            checkbox.Attributes["type"] = "checkbox";
            checkbox.Attributes["id"] = $"checkbox_{Name}_{Index}";
            checkbox.Attributes["class"] = Class;

            if (Checked)
            {
                checkbox.Attributes["checked"] = "checked";
            }

            if (Hidden)
            {
                checkbox.Attributes["style"] = "display: none;";
            }

            TagBuilder span = new TagBuilder("span");
            span.Attributes["id"] = $"{Name}_{Index}";
            span.InnerHtml.AppendHtml(checkbox);
            span.InnerHtml.AppendHtml("&nbsp;");

            TagBuilder select = new TagBuilder("select");
            select.Attributes["class"] = $"{CellClass} form-control form-control-sm";
            select.Attributes["id"] = $"cmb_{Name}_{Index}";
            select.Attributes["autocomplete"] = "off";

            if (Checked)
            {
                select.InnerHtml.AppendHtml($"<option value='Yes' selected='selected'>Yes</option>");
                select.InnerHtml.AppendHtml($"<option value='No'>No</option>");
            }
            else
            {
                select.InnerHtml.AppendHtml($"<option value='Yes'>Yes</option>");
                select.InnerHtml.AppendHtml($"<option value='No' selected='selected'>No</option>");
            }

            TagBuilder hiddenSpan = new TagBuilder("span");
            hiddenSpan.Attributes["id"] = $"span_{Name}_{Index}";
            hiddenSpan.Attributes["style"] = "display: none;";
            hiddenSpan.InnerHtml.AppendHtml(select);

            output.TagName = "td";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", $"{CellClass}");
            output.Attributes.SetAttribute("id", $"{Name}_cell_{Index}");

            output.Content.AppendHtml(span);
            output.Content.AppendHtml(hiddenSpan);
        }
    }
}
