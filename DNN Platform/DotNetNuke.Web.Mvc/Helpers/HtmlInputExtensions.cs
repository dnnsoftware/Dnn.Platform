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
    /// Represents support for HTML input controls in an application.
    /// </summary>
    public static class HtmlInputExtensions
    {
        // CheckBox

        /// <summary>
        /// Returns a check box input element by using the specified HTML helper and the name of the form field.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "checkbox".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field.</param>
        public static MvcHtmlString CheckBox(this DnnHtmlHelper html, string name)
        {
            return html.HtmlHelper.CheckBox(name);
        }

        /// <summary>
        /// Returns a check box input element by using the specified HTML helper, the name of the form field, and a value to indicate whether the check box is selected.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "checkbox".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field.</param><param name="isChecked">true to select the check box; otherwise, false.</param>
        public static MvcHtmlString CheckBox(this DnnHtmlHelper html, string name, bool isChecked)
        {
            return html.HtmlHelper.CheckBox(name, isChecked);
        }

        /// <summary>
        /// Returns a check box input element by using the specified HTML helper, the name of the form field, a value that indicates whether the check box is selected, and the HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "checkbox".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field.</param><param name="isChecked">true to select the check box; otherwise, false.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString CheckBox(this DnnHtmlHelper html, string name, bool isChecked, object htmlAttributes)
        {
            return html.HtmlHelper.CheckBox(name, isChecked, htmlAttributes);
        }

        /// <summary>
        /// Returns a check box input element by using the specified HTML helper, the name of the form field, and the HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "checkbox".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString CheckBox(this DnnHtmlHelper html, string name, object htmlAttributes)
        {
            return html.HtmlHelper.CheckBox(name, htmlAttributes);
        }

        /// <summary>
        /// Returns a check box input element by using the specified HTML helper, the name of the form field, and the HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "checkbox".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString CheckBox(this DnnHtmlHelper html, string name, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.CheckBox(name, htmlAttributes);
        }

        /// <summary>
        /// Returns a check box input element by using the specified HTML helper, the name of the form field, a value to indicate whether the check box is selected, and the HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "checkbox".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field.</param><param name="isChecked">true to select the check box; otherwise, false.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString CheckBox(this DnnHtmlHelper html, string name, bool isChecked, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.CheckBox(name, isChecked, htmlAttributes);
        }

        /// <summary>
        /// Returns a check box input element for each property in the object that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element whose type attribute is set to "checkbox" for each property in the object that is represented by the specified expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><typeparam name="TModel">The type of the model.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString CheckBoxFor<TModel>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression)
        {
            return html.HtmlHelper.CheckBoxFor(expression);
        }

        /// <summary>
        /// Returns a check box input element for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element whose type attribute is set to "checkbox" for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString CheckBoxFor<TModel>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression, object htmlAttributes)
        {
            return html.HtmlHelper.CheckBoxFor(expression, htmlAttributes);
        }

        /// <summary>
        /// Returns a check box input element for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element whose type attribute is set to "checkbox" for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="htmlAttributes">A dictionary that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString CheckBoxFor<TModel>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.CheckBoxFor(expression, htmlAttributes);
        }

        // Hidden

        /// <summary>
        /// Returns a hidden input element by using the specified HTML helper and the name of the form field.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "hidden".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param>
        public static MvcHtmlString Hidden(this DnnHtmlHelper html, string name)
        {
            return html.HtmlHelper.Hidden(name);
        }

        /// <summary>
        /// Returns a hidden input element by using the specified HTML helper, the name of the form field, and the value.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "hidden".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">The value of the hidden input element. The value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object. If the element is not found in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/>, the value parameter is used.</param>
        public static MvcHtmlString Hidden(this DnnHtmlHelper html, string name, object value)
        {
            return html.HtmlHelper.Hidden(name, value);
        }

        /// <summary>
        /// Returns a hidden input element by using the specified HTML helper, the name of the form field, the value, and the HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "hidden".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">The value of the hidden input element The value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object. If the element is not found in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object, the value parameter is used.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString Hidden(this DnnHtmlHelper html, string name, object value, object htmlAttributes)
        {
            return html.HtmlHelper.Hidden(name, value, htmlAttributes);
        }

        /// <summary>
        /// Returns a hidden input element by using the specified HTML helper, the name of the form field, the value, and the HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "hidden".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">The value of the hidden input element. The value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object. If the element is not found in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object, the value parameter is used.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString Hidden(this DnnHtmlHelper html, string name, object value, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.Hidden(name, value, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML hidden input element for each property in the object that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "hidden" for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam>
        public static MvcHtmlString HiddenFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            return html.HtmlHelper.HiddenFor(expression);
        }

        /// <summary>
        /// Returns an HTML hidden input element for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "hidden" for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam>
        public static MvcHtmlString HiddenFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            return html.HtmlHelper.HiddenFor(expression, htmlAttributes);
        }

        /// <summary>
        /// Returns an HTML hidden input element for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "hidden" for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the property.</typeparam>
        public static MvcHtmlString HiddenFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.HiddenFor(expression, htmlAttributes);
        }

        // Password

        /// <summary>
        /// Returns a password input element by using the specified HTML helper and the name of the form field.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "password".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param>
        public static MvcHtmlString Password(this DnnHtmlHelper html, string name)
        {
            return html.HtmlHelper.Password(name);
        }

        /// <summary>
        /// Returns a password input element by using the specified HTML helper, the name of the form field, and the value.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "password".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">The value of the password input element. If this value is null, the value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.</param>
        public static MvcHtmlString Password(this DnnHtmlHelper html, string name, object value)
        {
            return html.HtmlHelper.Password(name, value);
        }

        /// <summary>
        /// Returns a password input element by using the specified HTML helper, the name of the form field, the value, and the HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "password".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">The value of the password input element. If this value is null, the value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString Password(this DnnHtmlHelper html, string name, object value, object htmlAttributes)
        {
            return html.HtmlHelper.Password(name, value, htmlAttributes);
        }

        /// <summary>
        /// Returns a password input element by using the specified HTML helper, the name of the form field, the value, and the HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "password".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">The value of the password input element. If this value is null, the value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString Password(this DnnHtmlHelper html, string name, object value, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.Password(name, value, htmlAttributes);
        }

        /// <summary>
        /// Returns a password input element for each property in the object that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element whose type attribute is set to "password" for each property in the object that is represented by the specified expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString PasswordFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            return html.HtmlHelper.PasswordFor(expression);
        }

        /// <summary>
        /// Returns a password input element for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element whose type attribute is set to "password" for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString PasswordFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            return html.HtmlHelper.PasswordFor(expression, htmlAttributes);
        }

        /// <summary>
        /// Returns a password input element for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element whose type attribute is set to "password" for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="htmlAttributes">A dictionary that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="expression"/> parameter is null.</exception>
        public static MvcHtmlString PasswordFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.PasswordFor(expression, htmlAttributes);
        }

        // RadioButton

        /// <summary>
        /// Returns a radio button input element that is used to present mutually exclusive options.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "radio".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">If this radio button is selected, the value of the radio button that is submitted when the form is posted. If the value of the selected radio button in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object matches this value, this radio button is selected.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception><exception cref="T:System.ArgumentNullException">The <paramref name="value"/> parameter is null.</exception>
        public static MvcHtmlString RadioButton(this DnnHtmlHelper html, string name, object value)
        {
            return html.HtmlHelper.RadioButton(name, value);
        }

        /// <summary>
        /// Returns a radio button input element that is used to present mutually exclusive options.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "radio".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">If this radio button is selected, the value of the radio button that is submitted when the form is posted. If the value of the selected radio button in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object matches this value, this radio button is selected.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception><exception cref="T:System.ArgumentNullException">The <paramref name="value"/> parameter is null.</exception>
        public static MvcHtmlString RadioButton(this DnnHtmlHelper html, string name, object value, object htmlAttributes)
        {
            return html.HtmlHelper.RadioButton(name, value, htmlAttributes);
        }

        /// <summary>
        /// Returns a radio button input element that is used to present mutually exclusive options.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "radio".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">If this radio button is selected, the value of the radio button that is submitted when the form is posted. If the value of the selected radio button in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object matches this value, this radio button is selected.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception><exception cref="T:System.ArgumentNullException">The <paramref name="value"/> parameter is null.</exception>
        public static MvcHtmlString RadioButton(this DnnHtmlHelper html, string name, object value, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.RadioButton(name, value, htmlAttributes);
        }

        /// <summary>
        /// Returns a radio button input element that is used to present mutually exclusive options.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "radio".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">If this radio button is selected, the value of the radio button that is submitted when the form is posted. If the value of the selected radio button in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object matches this value, this radio button is selected.</param><param name="isChecked">true to select the radio button; otherwise, false.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception><exception cref="T:System.ArgumentNullException">The <paramref name="value"/> parameter is null.</exception>
        public static MvcHtmlString RadioButton(this DnnHtmlHelper html, string name, object value, bool isChecked)
        {
            return html.HtmlHelper.RadioButton(name, value, isChecked);
        }

        /// <summary>
        /// Returns a radio button input element that is used to present mutually exclusive options.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "radio".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">If this radio button is selected, the value of the radio button that is submitted when the form is posted. If the value of the selected radio button in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object matches this value, this radio button is selected.</param><param name="isChecked">true to select the radio button; otherwise, false.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception><exception cref="T:System.ArgumentNullException">The <paramref name="value"/> parameter is null.</exception>
        public static MvcHtmlString RadioButton(this DnnHtmlHelper html, string name, object value, bool isChecked, object htmlAttributes)
        {
            return html.HtmlHelper.RadioButton(name, value, isChecked, htmlAttributes);
        }

        /// <summary>
        /// Returns a radio button input element that is used to present mutually exclusive options.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "radio".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">If this radio button is selected, the value of the radio button that is submitted when the form is posted. If the value of the selected radio button in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object matches this value, this radio button is selected.</param><param name="isChecked">true to select the radio button; otherwise, false.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><exception cref="T:System.ArgumentException">The <paramref name="name"/> parameter is null or empty.</exception><exception cref="T:System.ArgumentNullException">The <paramref name="value"/> parameter is null.</exception>
        public static MvcHtmlString RadioButton(this DnnHtmlHelper html, string name, object value, bool isChecked, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.RadioButton(name, value, isChecked, htmlAttributes);
        }

        /// <summary>
        /// Returns a radio button input element for each property in the object that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element whose type attribute is set to "radio" for each property in the object that is represented by the specified expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="value">If this radio button is selected, the value of the radio button that is submitted when the form is posted. If the value of the selected radio button in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object matches this value, this radio button is selected.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="value"/> parameter is null.</exception>
        public static MvcHtmlString RadioButtonFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object value)
        {
            return html.HtmlHelper.RadioButtonFor(expression, value);
        }

        /// <summary>
        /// Returns a radio button input element for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element whose type attribute is set to "radio" for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="value">If this radio button is selected, the value of the radio button that is submitted when the form is posted. If the value of the selected radio button in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object matches this value, this radio button is selected.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="value"/> parameter is null.</exception>
        public static MvcHtmlString RadioButtonFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object value, object htmlAttributes)
        {
            return html.HtmlHelper.RadioButtonFor(expression, value, htmlAttributes);
        }

        /// <summary>
        /// Returns a radio button input element for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element whose type attribute is set to "radio" for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="value">If this radio button is selected, the value of the radio button that is submitted when the form is posted. If the value of the selected radio button in the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> or the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object matches this value, this radio button is selected.</param><param name="htmlAttributes">A dictionary that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentNullException">The <paramref name="value"/> parameter is null.</exception>
        public static MvcHtmlString RadioButtonFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object value, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.RadioButtonFor(expression, value, htmlAttributes);
        }

        // TextBox

        /// <summary>
        /// Returns a text input element by using the specified HTML helper and the name of the form field.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "text".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param>
        public static MvcHtmlString TextBox(this DnnHtmlHelper html, string name)
        {
            return html.HtmlHelper.TextBox(name);
        }

        /// <summary>
        /// Returns a text input element by using the specified HTML helper, the name of the form field, and the value.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "text".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">The value of the text input element. If this value is null, the value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.</param>
        public static MvcHtmlString TextBox(this DnnHtmlHelper html, string name, object value)
        {
            return html.HtmlHelper.TextBox(name, value);
        }

        /// <summary>
        /// Returns a text input element.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "text".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field.</param><param name="value">The value of the text input element. If this value is null, the value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.</param><param name="format">A string that is used to format the input.</param>
        public static MvcHtmlString TextBox(this DnnHtmlHelper html, string name, object value, string format)
        {
            return html.HtmlHelper.TextBox(name, value, format);
        }

        /// <summary>
        /// Returns a text input element by using the specified HTML helper, the name of the form field, the value, and the HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "text".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">The value of the text input element. If this value is null, the value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString TextBox(this DnnHtmlHelper html, string name, object value, object htmlAttributes)
        {
            return html.HtmlHelper.TextBox(name, value, htmlAttributes);
        }

        /// <summary>
        /// Returns a text input element.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "text".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">The value of the text input element. If this value is null, the value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.</param><param name="format">A string that is used to format the input.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString TextBox(this DnnHtmlHelper html, string name, object value, string format, object htmlAttributes)
        {
            return html.HtmlHelper.TextBox(name, value, format, htmlAttributes);
        }

        /// <summary>
        /// Returns a text input element by using the specified HTML helper, the name of the form field, the value, and the HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "text".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">The value of the text input element. If this value is null, the value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString TextBox(this DnnHtmlHelper html, string name, object value, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.TextBox(name, value, htmlAttributes);
        }

        /// <summary>
        /// Returns a text input element.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "text".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="name">The name of the form field and the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> key that is used to look up the value.</param><param name="value">The value of the text input element. If this value is null, the value of the element is retrieved from the <see cref="T:System.Web.Mvc.ViewDataDictionary"/> object. If no value exists there, the value is retrieved from the <see cref="T:System.Web.Mvc.ModelStateDictionary"/> object.</param><param name="format">A string that is used to format the input.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param>
        public static MvcHtmlString TextBox(this DnnHtmlHelper html, string name, object value, string format, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.TextBox(name, value, format, htmlAttributes);
        }

        /// <summary>
        /// Returns a text input element for each property in the object that is represented by the specified expression.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element whose type attribute is set to "text" for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentException">The <paramref name="expression"/> parameter is null or empty.</exception>
        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            return html.HtmlHelper.TextBoxFor(expression);
        }

        /// <summary>
        /// Returns a text input element.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "text".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="format">A string that is used to format the input.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam>
        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string format)
        {
            return html.HtmlHelper.TextBoxFor(expression, format);
        }

        /// <summary>
        /// Returns a text input element for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element whose type attribute is set to "text" for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentException">The <paramref name="expression"/> parameter is null or empty.</exception>
        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            return html.HtmlHelper.TextBoxFor(expression, htmlAttributes);
        }

        /// <summary>
        /// Returns a text input element.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "text".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="format">A string that is used to format the input.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam>
        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string format, object htmlAttributes)
        {
            return html.HtmlHelper.TextBoxFor(expression, format, htmlAttributes);
        }

        /// <summary>
        /// Returns a text input element for each property in the object that is represented by the specified expression, using the specified HTML attributes.
        /// </summary>
        ///
        /// <returns>
        /// An HTML input element type attribute is set to "text" for each property in the object that is represented by the expression.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to render.</param><param name="htmlAttributes">A dictionary that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam><exception cref="T:System.ArgumentException">The <paramref name="expression"/> parameter is null or empty.</exception>
        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.TextBoxFor(expression, htmlAttributes);
        }

        /// <summary>
        /// Returns a text input element.
        /// </summary>
        ///
        /// <returns>
        /// An input element whose type attribute is set to "text".
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="expression">An expression that identifies the object that contains the properties to display.</param><param name="format">A string that is used to format the input.</param><param name="htmlAttributes">An object that contains the HTML attributes to set for the element.</param><typeparam name="TModel">The type of the model.</typeparam><typeparam name="TProperty">The type of the value.</typeparam>
        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, string format, IDictionary<string, object> htmlAttributes)
        {
            return html.HtmlHelper.TextBoxFor(expression, format, htmlAttributes);
        }
    }
}
