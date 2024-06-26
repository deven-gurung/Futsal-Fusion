﻿using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FutsalFusion;

[HtmlTargetElement("a", Attributes = "s-text")]
[HtmlTargetElement("a", Attributes = "s-areas")]
[HtmlTargetElement("a", Attributes = "s-actionName")]
[HtmlTargetElement("a", Attributes = "s-controllerName")]
[HtmlTargetElement("a", Attributes = "s-rK1")]
[HtmlTargetElement("a", Attributes = "s-rV1")]
[HtmlTargetElement("a", Attributes = "s-rK2")]
[HtmlTargetElement("a", Attributes = "s-rV2")]
[HtmlTargetElement("a", Attributes = "s-rK3")]
[HtmlTargetElement("a", Attributes = "s-rV3")]
[HtmlTargetElement("a", Attributes = "s-rK4")]
[HtmlTargetElement("a", Attributes = "s-rV4")]
[HtmlTargetElement("a", Attributes = "s-rK5")]
[HtmlTargetElement("a", Attributes = "s-rV5")]
public class SecureLinkTagHelper : TagHelper
{
    #region Input Attributes
    [HtmlAttributeName("s-text")]
    public string text { get; set; }

    [HtmlAttributeName("s-areas")]
    public string areas { get; set; } = null;

    [HtmlAttributeName("s-actionName")]
    public string actionName { get; set; }

    [HtmlAttributeName("s-controllerName")]
    public string controllerName { get; set; }

    [HtmlAttributeName("s-rK1")]
    public string routekey1 { get; set; } = null;

    [HtmlAttributeName("s-rV1")]
    public string routeValues1 { get; set; } = null;

    [HtmlAttributeName("s-rK2")]
    public string routekey2 { get; set; } = null;

    [HtmlAttributeName("s-rV2")]
    public string routeValues2 { get; set; } = null;

    [HtmlAttributeName("s-rK3")]
    public string routekey3 { get; set; } = null;

    [HtmlAttributeName("s-rV3")]
    public string routeValues3 { get; set; } = null;

    [HtmlAttributeName("s-rK4")]
    public string routekey4 { get; set; } = null;

    [HtmlAttributeName("s-rV4")]
    public string routeValues4 { get; set; } = null;

    [HtmlAttributeName("s-rK5")]
    public string routekey5 { get; set; } = null;

    [HtmlAttributeName("s-rV5")]
    public string routeValues5 { get; set; } = null;
    #endregion

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var dataProtectionProvider = DataProtectionProvider.Create("WebQuery");
        var protector = dataProtectionProvider.CreateProtector("WebQuery.QueryStrings");

        var tag = new TagBuilder("a");
        var queryString = string.Empty;
        var mainString = string.Empty;
        var strings = new List<string>();

        if (routekey1 != null && routeValues1 != null)
        {
            strings.Add($"{routekey1}={routeValues1}");
        }

        if (routekey2 != null && routeValues2 != null)
        {
            strings.Add($"{routekey2}={routeValues2}");
        }

        if (routekey3 != null && routeValues3 != null)
        {
            strings.Add($"{routekey3}={routeValues3}");
        }

        if (routekey4 != null && routeValues4 != null)
        {
            strings.Add($"{routekey4}={routeValues4}");
        }

        if (routekey5 != null && routeValues5 != null)
        {
            strings.Add($"{routekey5}={routeValues5}");
        }

        if (routekey1 == null && routeValues1 == null && 
            routekey2 == null && routeValues2 == null && 
            routekey3 == null && routeValues3 == null && 
            routekey4 == null && routeValues4 == null && 
            routekey5 == null && routeValues5 == null)
        {
            mainString = String.IsNullOrEmpty(areas) ? $"/{controllerName}/{actionName}" : $"/{areas}/{controllerName}/{actionName}";
        }
        else
        {
            queryString += string.Join(",", strings);

            var format = new CultureInfo("en-GB");
            
            var random = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            
            var values = string.Join("[", random, queryString, DateTime.Now.ToString(format));

            mainString = string.IsNullOrEmpty(areas) ? $"/{controllerName}/{actionName}?q={protector.Protect(values)}" : $"/{areas}/{controllerName}/{actionName}?q={protector.Protect(values)}";
        }

        tag.Attributes["href"] = mainString;
        
        tag.InnerHtml.Append(string.IsNullOrEmpty(text) ? "____" : text);
        
        output.Content.SetHtmlContent(tag);
    }
}