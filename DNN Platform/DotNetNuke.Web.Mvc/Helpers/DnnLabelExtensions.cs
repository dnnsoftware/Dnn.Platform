// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    public static class DnnLabelExtensions
    {
        public static MvcHtmlString Label<TModel>(this DnnHelper<TModel> dnnHelper, string expression, string labelText, string helpText)
        {
            return Label(dnnHelper, expression, labelText, helpText, null);
        }

        public static MvcHtmlString Label<TModel>(this DnnHelper<TModel> dnnHelper, string expression, string labelText, string helpText, object htmlAttributes)
        {
            return Label(dnnHelper, expression, labelText, helpText, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString Label<TModel>(this DnnHelper<TModel> dnnHelper, string expression, string labelText, string helpText, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = dnnHelper.HtmlHelper as HtmlHelper<TModel>;

            return LabelHelper(
                htmlHelper,
                ModelMetadata.FromStringExpression(expression, htmlHelper.ViewData),
                expression,
                labelText,
                helpText,
                htmlAttributes);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHelper<TModel> dnnHelper, Expression<Func<TModel, TValue>> expression)
        {
            return LabelFor(dnnHelper, expression, null, null, null);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHelper<TModel> dnnHelper, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return LabelFor(dnnHelper, expression, null, null, htmlAttributes);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHelper<TModel> dnnHelper, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes)
        {
            return LabelFor(dnnHelper, expression, null, null, htmlAttributes);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHelper<TModel> dnnHelper, Expression<Func<TModel, TValue>> expression, string helpText)
        {
            return LabelFor(dnnHelper, expression, null, helpText, null);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHelper<TModel> dnnHelper, Expression<Func<TModel, TValue>> expression, string helpText, object htmlAttributes)
        {
            return LabelFor(dnnHelper, expression, null, helpText, htmlAttributes);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHelper<TModel> dnnHelper, Expression<Func<TModel, TValue>> expression, string helpText, IDictionary<string, object> htmlAttributes)
        {
            return LabelFor(dnnHelper, expression, null, helpText, htmlAttributes);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHelper<TModel> dnnHelper, Expression<Func<TModel, TValue>> expression, string labelText, string helpText)
        {
            return LabelFor(dnnHelper, expression, labelText, helpText, null);
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHelper<TModel> dnnHelper, Expression<Func<TModel, TValue>> expression, string labelText, string helpText, object htmlAttributes)
        {
            return LabelFor(dnnHelper, expression, labelText, helpText, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString LabelFor<TModel, TValue>(this DnnHelper<TModel> dnnHelper, Expression<Func<TModel, TValue>> expression, string labelText, string helpText, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = dnnHelper.HtmlHelper as HtmlHelper<TModel>;

            return LabelHelper(
                htmlHelper,
                ModelMetadata.FromLambdaExpression(expression, dnnHelper.ViewData),
                ExpressionHelper.GetExpressionText(expression),
                labelText,
                helpText,
                htmlAttributes);
        }

        public static MvcHtmlString LabelHelper(HtmlHelper html, string htmlFieldName, string labelText, string helpText = null, IDictionary<string, object> htmlAttributes = null)
        {
            if (string.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }

            var divTag = new TagBuilder("div");
            divTag.Attributes.Add("class", "dnnLabel");
            divTag.Attributes.Add("style", "position:relative;");
            divTag.MergeAttributes(htmlAttributes, true);

            var labelTag = new TagBuilder("label");
            labelTag.Attributes.Add("for", htmlFieldName);

            var spanTag = new TagBuilder("span");
            spanTag.SetInnerText(labelText);

            labelTag.InnerHtml = spanTag.ToString();

            divTag.InnerHtml = labelTag.ToString();

            var aTag = new TagBuilder("a");
            aTag.Attributes.Add("class", "dnnFormHelp");
            aTag.Attributes.Add("href", "#");
            aTag.Attributes.Add("tabIndex", "-1");

            divTag.InnerHtml += aTag.ToString();

            var toolTipTag = new TagBuilder("div");
            toolTipTag.Attributes.Add("class", "dnnTooltip");

            var toolTipContentTag = new TagBuilder("div");
            toolTipContentTag.Attributes.Add("class", "dnnFormHelpContent dnnClear");

            spanTag = new TagBuilder("span");
            spanTag.Attributes.Add("class", "dnnHelpText");
            spanTag.SetInnerText(helpText);

            aTag = new TagBuilder("a");
            aTag.Attributes.Add("class", "pinHelp");
            aTag.Attributes.Add("href", "#");
            aTag.Attributes.Add("aria-label", "Pin");

            toolTipContentTag.InnerHtml = spanTag.ToString();
            toolTipContentTag.InnerHtml += aTag.ToString();

            toolTipTag.InnerHtml = toolTipContentTag.ToString();

            divTag.InnerHtml += toolTipTag.ToString();

            return new MvcHtmlString(divTag.ToString(TagRenderMode.Normal));
        }

        internal static MvcHtmlString LabelHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string labelText = null, string helpText = null, IDictionary<string, object> htmlAttributes = null)
        {
            string resolvedLabelText = labelText ?? metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            string resolvedHelpText = helpText ?? metadata.Description ?? metadata.Description ?? null;
            string resolvedId = TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName));

            return LabelHelper(html, resolvedId, resolvedLabelText, resolvedHelpText, htmlAttributes);
        }
    }
}
