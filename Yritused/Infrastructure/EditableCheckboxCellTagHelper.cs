using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Yritused.Infrastructure
{
    [HtmlTargetElement("editablecheckboxcell")]
    public class EditableCheckboxCellTagHelper : TagHelper
    {
        public string? Class { get; set; }
        public string? Name { get; set; }
        public int Index { get; set; }
        public bool Checked { get; set; }
        public bool Hidden { get; set; }
        public int RowId { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            TagBuilder checkbox = new("input");
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

            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.AppendHtml(checkbox);
            output.Content.AppendHtml("&nbsp;");
        }
    }
}
