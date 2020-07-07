// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    /// <summary>
    /// Represents support for making selections in a list.
    /// </summary>
    public static class HtmlSelectExtensions
    {
        // DropDownList

        /// <summary>
        /// Returns a single-selection select element using the specified HTML helper and the name of the form field.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString DropDownList(this DnnHtmlHelper html, string name)
        {
            return html.HtmlHelper.DropDownList(name);
        }

        /// <summary>
        /// Returns a single-selection select element using the specified HTML helper, the name of the form field, and an option label.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element with an option subelement for each item in the list.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><param name="optionLabel">The text for a default empty item. This parameter can be null.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString DropDownList(this DnnHtmlHelper html, string name, string optionLabel)
        {
            return html.HtmlHelper.DropDownList(name, optionLabel);
        }

        /// <summary>
        /// Returns a single-selection select element using the specified HTML helper, the name of the form field, and the specified list items.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element with an option subelement for each item in the list.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString DropDownList(this DnnHtmlHelper html, string name, IEnumerable<SelectListItem> selectList)
        {
            return html.HtmlHelper.DropDownList(name, selectList);
        }

        /// <summary>
        /// Returns a single-selection select element using the specified HTML helper, the name of the form field, the specified list items, and the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element with an option subelement for each item in the list.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString DropDownList(this DnnHtmlHelper html, string name, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            return html.HtmlHelper.DropDownList(name, selectList, htmlAttributes);
        }

        /// <summary>
        /// Returns a single-selection select element using the specified HTML helper, the name of the form field, the specified list items, and the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element with an option subelement for each item in the list.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString DropDownList(this DnnHtmlHelper html, string name, IEnumerable<SelectListItem> selectList, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.DropDownList(name, selectList, htmlAttributes);
        }

        /// <summary>
        /// Returns a single-selection select element using the specified HTML helper, the name of the form field, the specified list items, and an option label.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element with an option subelement for each item in the list.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="optionLabel">The text for a default empty item. This parameter can be null.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString DropDownList(this DnnHtmlHelper html, string name, IEnumerable<SelectListItem> selectList, string optionLabel)
        {
            return html.HtmlHelper.DropDownList(name, selectList, optionLabel);
        }

        /// <summary>
        /// Returns a single-selection select element using the specified HTML helper, the name of the form field, the specified list items, an option label, and the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element with an option subelement for each item in the list.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="optionLabel">The text for a default empty item. This parameter can be null.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString DropDownList(this DnnHtmlHelper html, string name, IEnumerable<SelectListItem> selectList, string optionLabel, object htmlAttributes)
        {
            return html.HtmlHelper.DropDownList(name, selectList, optionLabel, htmlAttributes);
        }

        /// <summary>
        /// Returns a single-selection select element using the specified HTML helper, the name of the form field, the specified list items, an option label, and the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element with an option subelement for each item in the list.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="optionLabel">The text for a default empty item. This parameter can be null.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString DropDownList(this DnnHtmlHelper html, string name, IEnumerable<SelectListItem> selectList, string optionLabel, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.DropDownList(name, selectList, optionLabel, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression using the specified list items.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString DropDownListFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList)
        {
            return html.HtmlHelper.DropDownListFor(expression, selectList);
        }

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression using the specified list items and HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString DropDownListFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            return html.HtmlHelper.DropDownListFor(expression, selectList, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression using the specified list items and HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString DropDownListFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.DropDownListFor(expression, selectList, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression using the specified list items and option label.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="optionLabel">The text for a default empty item. This parameter can be null.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString DropDownListFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, string optionLabel)
        {
            return html.HtmlHelper.DropDownListFor(expression, selectList, optionLabel);
        }

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression using the specified list items, option label, and HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="optionLabel">The text for a default empty item. This parameter can be null.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString DropDownListFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, string optionLabel, object htmlAttributes)
        {
            return html.HtmlHelper.DropDownListFor(expression, selectList, optionLabel, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression using the specified list items, option label, and HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="optionLabel">The text for a default empty item. This parameter can be null.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString DropDownListFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, string optionLabel, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.DropDownListFor(expression, selectList, optionLabel, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML select element for each value in the enumeration that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each value in the enumeration that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the values to display.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TEnum">The type of the value.</typeparam>
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TEnum>> expression)
        {
            return html.HtmlHelper.EnumDropDownListFor<TModel, TEnum>(expression);
        }

        /// <summary>
        /// Returns an HTML select element for each value in the enumeration that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each value in the enumeration that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the values to display.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TEnum">The type of the value.</typeparam>
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            return html.HtmlHelper.EnumDropDownListFor<TModel, TEnum>(expression, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML select element for each value in the enumeration that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each value in the enumeration that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the values to display.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TEnum">The type of the value.</typeparam>
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TEnum>> expression, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.EnumDropDownListFor<TModel, TEnum>(expression, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML select element for each value in the enumeration that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each value in the enumeration that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the values to display.</param><param name="optionLabel">The text for a default empty item. This parameter can be null.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TEnum">The type of the value.</typeparam>
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TEnum>> expression, string optionLabel)
        {
            return html.HtmlHelper.EnumDropDownListFor<TModel, TEnum>(expression, optionLabel);
        }

        /// <summary>
        /// Returns an HTML select element for each value in the enumeration that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each value in the enumeration that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the values to display.</param><param name="optionLabel">The text for a default empty item. This parameter can be null.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TEnum">The type of the value.</typeparam>
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TEnum>> expression, string optionLabel, object htmlAttributes)
        {
            return html.HtmlHelper.EnumDropDownListFor<TModel, TEnum>(expression, optionLabel, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML select element for each value in the enumeration that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each value in the enumeration that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the values to display.</param><param name="optionLabel">The text for a default empty item. This parameter can be null.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TEnum">The type of the value.</typeparam>
        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TEnum>> expression, string optionLabel, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.EnumDropDownListFor<TModel, TEnum>(expression, optionLabel, htmlAttributes);
        }

        // ListBox

        /// <summary>
        /// Returns a multi-select select element using the specified HTML helper and the name of the form field.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString ListBox(this DnnHtmlHelper html, string name)
        {
            return html.HtmlHelper.ListBox(name);
        }

        /// <summary>
        /// Returns a multi-select select element using the specified HTML helper, the name of the form field, and the specified list items.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element with an option subelement for each item in the list.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString ListBox(this DnnHtmlHelper html, string name, IEnumerable<SelectListItem> selectList)
        {
            return html.HtmlHelper.ListBox(name, selectList);
        }

        /// <summary>
        /// Returns a multi-select select element using the specified HTML helper, the name of the form field, and the specified list items.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element with an option subelement for each item in the list..
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString ListBox(this DnnHtmlHelper html, string name, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            return html.HtmlHelper.ListBox(name, selectList, htmlAttributes);
        }

        /// <summary>
        /// Returns a multi-select select element using the specified HTML helper, the name of the form field, the specified list items, and the specified HMTL attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element with an option subelement for each item in the list..
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field to return.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception>
        public static MvcHtmlString ListBox(this DnnHtmlHelper html, string name, IEnumerable<SelectListItem> selectList, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.ListBox(name, selectList, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression and using the specified list items.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString ListBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList)
        {
            return html.HtmlHelper.ListBoxFor(expression, selectList);
        }

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression using the specified list items and HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString ListBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            return html.HtmlHelper.ListBoxFor(expression, selectList, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression using the specified list items and HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML select element for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="selectList">A collection of <see cref="T:System.Web.Mvc.SelectListItem"/> objects that are used to populate the drop-down list.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString ListBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.ListBoxFor(expression, selectList, htmlAttributes);
        }
    }
}
