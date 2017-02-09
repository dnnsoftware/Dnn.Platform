using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace DotNetNuke.Web.UI.WebControls.Internal
{
    public class DnnDropDownCheckBoxes : CheckBoxList, IPostBackEventHandler
    {
        public DnnDropDownCheckBoxes()
        {
            AddJQueryReference = true;
            UseButtons = false;
            UseSelectAllNode = true;
            ReseteParentProperties();
        }

        private void ReseteParentProperties()
        {
            base.RepeatDirection = RepeatDirection.Vertical;
            base.RepeatLayout = RepeatLayout.Flow;
            base.AutoPostBack = false;
        }

        #region Properties

        private DropDownStyle _style;

        /// <summary>
        /// CSS classes of HTML elements building the control, implicit width and height properties
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public new DropDownStyle Style
        {
            get
            {
                if (_style == null)
                {
                    _style = new DropDownStyle();

                    if (IsTrackingViewState)
                    {
                        ((IStateManager)_style).TrackViewState();
                    }
                }

                return _style;
            }
        }

        /// <summary>
        /// !!! Use in .aspx in Visual Studio 2008 instead of &lt;Style/&gt; to avoid 'Ambiguous match found' parsing error !!!
        /// CSS classes of HTML elements building the control, implicit width and height properties
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public DropDownStyle Style2
        {
            get
            {
                if (_style == null)
                {
                    _style = new DropDownStyle();

                    if (IsTrackingViewState)
                    {
                        ((IStateManager)_style).TrackViewState();
                    }
                }

                return _style;
            }
        }

        /// <summary>
        /// Same as Style.SelectBoxCssClass
        /// </summary>
        public new string CssClass
        {
            get
            {
                return Style.SelectBoxCssClass;
            }
            set
            {
                Style.SelectBoxCssClass = value;
            }
        }

        private DropDownTexts _texts;

        /// <summary>
        /// Texts for control elements
        /// </summary>
        /// <remarks>
        /// Use this complex property to specify Texts for specific control elements, e.g. button captions, 'select all' node, select box caption
        /// </remarks>
        [Localizable(true)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public DropDownTexts Texts
        {
            get
            {
                if (_texts == null)
                {
                    _texts = new DropDownTexts();

                    if (IsTrackingViewState)
                    {
                        ((IStateManager)_texts).TrackViewState();
                    }
                }

                return _texts;
            }
        }

        /// <summary>
        /// Whether to include reference to JQuery using embeded resources
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set this property to 'true 'if you don't have reference to jQuery lib on your page/master page. 
        /// Set it to 'false' otherwise (or you'll get not working control)
        /// </para>
        /// <para>
        /// If you use ScriptManager or ScriptManagerProxy controls to add script references (via ScriptReference tag) the control is capable of detecting
        /// jQuery reference among others and in this case you may always have this property set to 'true'
        /// </para>
        /// </remarks>
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool AddJQueryReference
        {
            get
            {
                return (bool)(ViewState["AddJQueryReference"] ?? true);
            }
            set
            {
                {
                    ViewState["AddJQueryReference"] = value;
                }
            }
        }

        /// <summary>
        /// Whether to use 'OK' and 'Cancel' for initiating postbacks
        /// </summary>
        /// <remarks>
        /// If 'true' - corresponding buttons will be displayed within drop down box. Clicking 'OK' causes postback, 
        /// the value of the AutoPostBack property is irrelivant in this case.
        /// </remarks>
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool UseButtons
        {
            get
            {
                return (bool)(ViewState["UseButtons"] ?? true);
            }
            set
            {
                {
                    ViewState["UseButtons"] = value;
                }
            }
        }

        /// <summary>
        /// Whether to initiate postback whenever drop down box is hidden
        /// </summary>
        /// <remarks>
        /// If 'true' - clicking on any area outside the control will trigger postback to server. When UseButtons = 'true' this property
        /// has no effect
        /// </remarks>
        [PersistenceMode(PersistenceMode.Attribute)]
        public new bool AutoPostBack
        {
            get
            {
                return (bool)(ViewState["AutoPostBack2"] ?? false);
            }
            set
            {
                ViewState["AutoPostBack2"] = value;
            }
        }

        /// <summary>
        /// Whther to show 'Select all' node in the drop down
        /// </summary>
        /// <remarks>
        /// </remarks>
        public bool UseSelectAllNode
        {
            get
            {
                return (bool)(ViewState["UseSelectAllNode"] ?? true);
            }
            set
            {
                {
                    ViewState["UseSelectAllNode"] = value;
                }
            }
        }

        private bool AreScriptsInitialized
        {
            get
            {
                return (bool)(ViewState["AreScriptsInitialized"] ?? false);
            }
            set
            {
                ViewState["AreScriptsInitialized"] = value;
            }
        }

        private bool PropertiesAreDirty
        {
            get
            {
                return
                    ViewState.IsItemDirty("UseSelectAllNode") ||
                    ViewState.IsItemDirty("AutoPostBack2") ||
                    ViewState.IsItemDirty("UseButtons") ||
                    ViewState.IsItemDirty("AddJQueryReference");
            }
        }

        #endregion Properties

        #region Parent methods' overides

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);

            var scriptManager = ScriptManager.GetCurrent(Page);
            var asyncEnabled = scriptManager != null;

            if (AddJQueryReference)
                IncludeJqueryScript(scriptManager, asyncEnabled);

#if DEBUG
            var scriptResource = checkBoxScriptResource;
#else
            var scriptResource = checkBoxMinScriptResource;
#endif

            if (asyncEnabled)
                scriptManager.Scripts.Add(new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), scriptResource)));
            else Page.ClientScript.RegisterClientScriptInclude("dd_script", Page.ClientScript.GetWebResourceUrl(this.GetType(), scriptResource));

            var initializeScript = string.Format(
                initScript,
                ClientID,
                divPstfx,
                selectPstfx,
                "dd_chk_" + ClientID,
                UseButtons.ToString().ToLower(),
                AutoPostBack.ToString().ToLower(),
                UseSelectAllNode.ToString().ToLower());

            var cssRef = string.Format("<link rel='stylesheet' type='text/css' href='{0}' />", Page.ClientScript.GetWebResourceUrl(this.GetType(), defaultCssResource));

            var postbackScript = string.Format("function dd_chk_{0}(){{ {1} }}", ClientID, Page.ClientScript.GetPostBackEventReference(this, null));

            if (asyncEnabled && !AreScriptsInitialized && scriptManager.IsInAsyncPostBack)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), ClientID, initializeScript, true);
                ScriptManager.RegisterClientScriptBlock(this, GetType(), "post_" + ClientID, postbackScript, true);
                AreScriptsInitialized = true;
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), ClientID, initializeScript, true);
                Page.ClientScript.RegisterClientScriptBlock(GetType(), "post_" + ClientID, postbackScript, true);
                AreScriptsInitialized = true;
            }

            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "dd_chk_styles", cssRef, false);

            if (Page.IsPostBack && asyncEnabled && (PropertiesAreDirty || Texts.IsDirty || Style.IsDirty))
            {
                var script = string.Format(
                    updateScript,
                    ClientID,
                    divPstfx,
                    selectPstfx,
                    "dd_chk_" + ClientID,
                    UseButtons.ToString().ToLower(),
                    AutoPostBack.ToString().ToLower(),
                    UseSelectAllNode.ToString().ToLower());

                ScriptManager.RegisterStartupScript(this, GetType(), ClientID + "_upd", script, true);
            }
        }

        private void IncludeJqueryScript(ScriptManager scriptManager, bool asyncEnabled)
        {
            if (!asyncEnabled)
                Page.ClientScript.RegisterClientScriptInclude("dd_chk_jquery", Page.ClientScript.GetWebResourceUrl(this.GetType(), jqeuryResource));
            else
            {
                var jqureyRegistered = scriptManager.Scripts.All(script1 => !script1.Path.Contains("jquery-1.")); // search for jQuery registrations
                if (jqureyRegistered)
                    scriptManager.Scripts.Add(
                        new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), jqeuryResource)));
            }

        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            //ReseteParentProperties();

            // Add wrapper div
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "relative");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + selectPstfx);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, !string.IsNullOrEmpty(Style.SelectBoxCssClass) ? Style.SelectBoxCssClass + " " + selectCssClass : selectCssClass);
            if (Style.SelectBoxWidth.Value > 0) writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Style.SelectBoxWidth.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // Add caption
            if (!string.IsNullOrEmpty(Texts.SelectBoxCaption))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, "caption");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.WriteEncodedText(Texts.SelectBoxCaption);
                writer.RenderEndTag();
            }

            // Add dropdown div markup
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + divPstfx);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, !string.IsNullOrEmpty(Style.DropDownBoxCssClass) ? Style.DropDownBoxCssClass + " " + dropDownCssClass : dropDownCssClass);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
            if (Style.DropDownBoxBoxWidth.Value > 0) writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Style.DropDownBoxBoxWidth.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // Add div with check boxes
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "checks");
            if (Style.DropDownBoxBoxHeight.Value > 0) writer.AddStyleAttribute(HtmlTextWriterStyle.Height, Style.DropDownBoxBoxHeight.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // Add span for 'Select all' node
            if (UseSelectAllNode)
            {

                var selectAllHtml = @"<input type='checkbox' name='{0}'><label for='{0}'>{1}</label>";

                if (!string.IsNullOrEmpty(Texts.SelectAllNode))
                    selectAllHtml = string.Format(selectAllHtml, ClientID + selectAllPstfx, Texts.SelectAllNode);

                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "block");
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.Write(selectAllHtml);
                writer.RenderEndTag();
            }

            // Render legacy markup within wrapping markup
            base.Render(writer);

            // Close div with check boxes
            writer.RenderEndTag();

            // Add div with action buttons
            if (UseButtons)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, "buttons");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                // Close buttons div
                writer.RenderEndTag();
            }

            // Close dropdown div
            writer.RenderEndTag();

            // Close wrapper div
            writer.RenderEndTag();
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            if (CausesValidation)
            {
                Page.Validate(ValidationGroup);
            }

            OnSelectedIndexChanged(EventArgs.Empty);
        }

        #endregion

        #region State management overrides

        protected override void LoadViewState(object savedState)
        {
            var p = savedState as Pair;
            if (p != null)
            {
                base.LoadViewState(p.First);
                var propertiesState = p.Second as object[];

                if (propertiesState != null)
                {
                    if (propertiesState.Length > 0 && propertiesState[0] != null)
                        ((IStateManager)Texts).LoadViewState(propertiesState[0]);
                    if (propertiesState.Length > 1 && propertiesState[1] != null)
                        ((IStateManager)Style).LoadViewState(propertiesState[1]);
                }
                return;
            }
            base.LoadViewState(savedState);
        }

        protected override object SaveViewState()
        {
            object baseState = base.SaveViewState();
            var thisState = new object[2];

            if (_texts != null)
                thisState[0] = ((IStateManager)_texts).SaveViewState();
            if (_style != null)
                thisState[1] = ((IStateManager)_style).SaveViewState();

            return new Pair(baseState, thisState);
        }

        protected override void TrackViewState()
        {
            if (_texts != null)
                ((IStateManager)_texts).TrackViewState();
            if (_style != null)
                ((IStateManager)_style).TrackViewState();

            base.TrackViewState();
        }
        #endregion

        #region Constants

        private const string inputTag = @"<input type='button' value='{0}'></input>";
        private const string initScript = @"window.{0} = new DropDownScript('{0}','{1}','{2}',{3}, {4}, {5}, {6}); window.{0}.init();";
        private const string updateScript = @"window.{0}.update('{1}','{2}',{3}, {4}, {5}, {6});";
        private const string divPstfx = "_dv";
        private const string selectPstfx = "_sl";
        private const string selectAllPstfx = "_sll";
        private const string defaultCssResource = "Saplin.Controls.EmbeddedResources.DefaultStyles.css";
        private const string jqeuryResource = "Saplin.Controls.EmbeddedResources.jquery-1.6.1.min.js";
        private const string checkBoxScriptResource = "Saplin.Controls.EmbeddedResources.DropDownScript.js";
        private const string checkBoxMinScriptResource = "Saplin.Controls.EmbeddedResources.DropDownScript.min.js";
        private const string selectCssClass = "dd_chk_select";
        private const string dropDownCssClass = "dd_chk_drop";

        #endregion

        #region Not implemented members

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public new Unit Width
        {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public new Unit Height
        {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public new RepeatLayout RepeatLayout
        {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public new RepeatDirection RepeatDirection
        {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public new Color BackColor
        {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public new Color BorderColor
        {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public new bool BorderStyle
        {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public new Unit BorderWidth
        {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public new Unit CellPadding
        {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public new Unit CellSpacing
        {
            get;
            set;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public new bool EnableTheming
        {
            get;
            set;
        }

        #endregion
    }

    public class DropDownTexts : StateManagedComplexProperty
    {
        private StateBag viewState;

        public DropDownTexts()
        {
            SelectBoxCaption = "Select";
            SelectAllNode = "Select all";
        }

        /// <summary>
        /// Caption of the control
        /// </summary>
        [Localizable(true)]
        [DefaultValue("Select")]
        [NotifyParentProperty(true)]
        public string SelectBoxCaption
        {
            get
            {
                return ViewState["SelectBoxCaption"] as string;
            }
            set
            {
                ViewState["SelectBoxCaption"] = value;
            }
        }

        /// <summary>
        /// 'Select all' node (check box) text
        /// </summary>
        [Localizable(true)]
        [DefaultValue("Select All")]
        [NotifyParentProperty(true)]
        public string SelectAllNode
        {
            get
            {
                return ViewState["SelectAllNode"] as string;
            }
            set
            {
                ViewState["SelectAllNode"] = value;
            }
        }
    }

    public class DropDownStyle : StateManagedComplexProperty
    {
        public string SelectBoxCssClass
        {
            get
            {
                return ViewState["SelectBoxCssClass"] as string;
            }
            set
            {
                ViewState["SelectBoxCssClass"] = value;
            }
        }

        public string DropDownBoxCssClass
        {
            get
            {
                return ViewState["DropDownBoxCssClass"] as string;
            }
            set
            {
                ViewState["DropDownBoxCssClass"] = value;
            }
        }

        public Unit SelectBoxWidth
        {
            get
            {
                return (Unit)(ViewState["SelectBoxWidth"] ?? new Unit());
            }
            set
            {
                ViewState["SelectBoxWidth"] = value;
            }
        }

        public Unit DropDownBoxBoxWidth
        {
            get
            {
                return (Unit)(ViewState["DropDownBoxBoxWidth"] ?? new Unit());
            }
            set
            {
                ViewState["DropDownBoxBoxWidth"] = value;
            }
        }

        public Unit DropDownBoxBoxHeight
        {
            get
            {
                return (Unit)(ViewState["DropDownBoxBoxHeight"] ?? new Unit());
            }
            set
            {
                ViewState["DropDownBoxBoxHeight"] = value;
            }
        }
    }

    public abstract class StateManagedComplexProperty : IStateManager
    {
        private bool isTrackingViewState;
        private StateBag viewState;

        protected virtual StateBag ViewState
        {
            get
            {
                if (viewState == null)
                {
                    viewState = new StateBag(false);

                    if (isTrackingViewState)
                    {
                        ((IStateManager)viewState).TrackViewState();
                    }
                }
                return viewState;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return isTrackingViewState;
            }
        }

        void IStateManager.LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                ((IStateManager)ViewState).LoadViewState(savedState);
            }
        }

        object IStateManager.SaveViewState()
        {
            object savedState = null;

            if (viewState != null)
            {
                savedState =
                   ((IStateManager)viewState).SaveViewState();
            }
            return savedState;
        }

        void IStateManager.TrackViewState()
        {
            isTrackingViewState = true;

            if (viewState != null)
            {
                ((IStateManager)viewState).TrackViewState();
            }
        }

        public bool IsDirty
        {
            get
            {
                return ViewState.IsDirty();
            }
        }
    }

    public static class StateBagExtensions
    {
        public static bool IsDirty(this StateBag stateBag)
        {
            foreach (string key in stateBag.Keys)
            {
                if (stateBag.IsItemDirty(key)) return true;
            }

            return false;
        }
    }
}