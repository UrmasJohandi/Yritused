using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Yritused.Infrastructure
{
    [HtmlTargetElement("editableautocompletecell")]
    public class EditableAutoCompleteTagHelper : TagHelper
    {
        public string? Name { get; set; }
        public string? SpanName { get; set; }
        public int Index { get; set; }
        public string? Value { get; set; }
        public string? Class { get; set; }
        public string? ArticleValue { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            TagBuilder autocompleteinput = new("input");
            autocompleteinput.Attributes["class"] = $"form-control form-control-sm";
            autocompleteinput.Attributes["id"] = $"autocomplete_{Name}_{Index}";
            autocompleteinput.Attributes["autocomplete"] = "off";

            TagBuilder hiddenSpan = new("span");
            hiddenSpan.Attributes["id"] = $"span_{Name}_{Index}";
            hiddenSpan.Attributes["style"] = "display: none;";
            hiddenSpan.InnerHtml.AppendHtml(autocompleteinput);

            TagBuilder span = new("span");
            span.Attributes["id"] = $"{SpanName}_{Index}";
            span.InnerHtml.AppendHtml($"{Value}");

            TagBuilder input = new("input");
            input.Attributes["id"] = $"cmb_{Name}_selected_{Index}";
            input.Attributes["type"] = "hidden";
            input.Attributes["value"] = ArticleValue != "&nbsp;" ? $"{ArticleValue}" : string.Empty;

            output.TagName = "td";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", $"{Class}");
            output.Attributes.SetAttribute("id", Name == "mat_materialgroup" ? $"materialgroup_{Index}" : $"{Name}_{Index}");
            output.Content.AppendHtml(span);
            output.Content.AppendHtml(hiddenSpan);
            output.Content.AppendHtml(input);
        }
    }
}
