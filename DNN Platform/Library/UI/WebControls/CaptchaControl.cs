// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    using Image = System.Web.UI.WebControls.Image;

    /// <summary>The CaptchaControl control provides a Captcha Challenge control.</summary>
    [ToolboxData("<{0}:CaptchaControl Runat=\"server\" CaptchaHeight=\"100px\" CaptchaWidth=\"300px\" />")]
    public class CaptchaControl : WebControl, INamingContainer, IPostBackDataHandler
    {
        internal const string KEY = "captcha";
        private const int EXPIRATIONDEFAULT = 120;
        private const int LENGTHDEFAULT = 6;
        private const string RENDERURLDEFAULT = "ImageChallenge.captcha.aspx";
        private const string CHARSDEFAULT = "abcdefghijklmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CaptchaControl));
        private static readonly string[] FontFamilies = { "Arial", "Comic Sans MS", "Courier New", "Georgia", "Lucida Console", "MS Sans Serif", "Tahoma", "Times New Roman", "Trebuchet MS", "Verdana" };

        private static readonly Random Rand = new Random();
        private static string separator = ":-:";
        private readonly Style errorStyle = new Style();
        private readonly Style textBoxStyle = new Style();
        private Color backGroundColor = Color.Transparent;
        private string backGroundImage = string.Empty;
        private string captchaChars = CHARSDEFAULT;
        private Unit captchaHeight = Unit.Pixel(100);
        private int captchaLength = LENGTHDEFAULT;
        private string captchaText;
        private Unit captchaWidth = Unit.Pixel(300);
        private int expiration;
        private bool isValid;
        private string renderUrl = RENDERURLDEFAULT;
        private string userText = string.Empty;
        private Image image;

        /// <summary>Initializes a new instance of the <see cref="CaptchaControl"/> class.</summary>
        public CaptchaControl()
        {
            this.ErrorMessage = Localization.GetString("InvalidCaptcha", Localization.SharedResourceFile);
            this.Text = Localization.GetString("CaptchaText.Text", Localization.SharedResourceFile);
            this.expiration = HostController.Instance.GetInteger("EXPIRATION_DEFAULT", EXPIRATIONDEFAULT);
        }

        public event ServerValidateEventHandler UserValidated;

        /// <summary>Gets and sets the BackGroundColor.</summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Error Message Control.")]
        public Style ErrorStyle
        {
            get
            {
                return this.errorStyle;
            }
        }

        /// <summary>Gets a value indicating whether the control is valid.</summary>
        [Category("Validation")]
        [Description("Returns True if the user was CAPTCHA validated after a postback.")]
        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        /// <summary>Gets the Style to use for the Text Box.</summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Text Box Control.")]
        public Style TextBoxStyle
        {
            get
            {
                return this.textBoxStyle;
            }
        }

        /// <summary>Gets or sets the BackGroundColor.</summary>
        [Category("Appearance")]
        [Description("The Background Color to use for the Captcha Image.")]
        public Color BackGroundColor
        {
            get
            {
                return this.backGroundColor;
            }

            set
            {
                this.backGroundColor = value;
            }
        }

        /// <summary>Gets or sets the BackGround Image.</summary>
        [Category("Appearance")]
        [Description("A Background Image to use for the Captcha Image.")]
        public string BackGroundImage
        {
            get
            {
                return this.backGroundImage;
            }

            set
            {
                this.backGroundImage = value;
            }
        }

        /// <summary>Gets or sets the list of characters.</summary>
        [Category("Behavior")]
        [DefaultValue(CHARSDEFAULT)]
        [Description("Characters used to render CAPTCHA text. A character will be picked randomly from the string.")]
        public string CaptchaChars
        {
            get
            {
                return this.captchaChars;
            }

            set
            {
                this.captchaChars = value;
            }
        }

        /// <summary>Gets or sets the height of the Captcha image.</summary>
        [Category("Appearance")]
        [Description("Height of Captcha Image.")]
        public Unit CaptchaHeight
        {
            get
            {
                return this.captchaHeight;
            }

            set
            {
                this.captchaHeight = value;
            }
        }

        /// <summary>Gets or sets the length of the Captcha string.</summary>
        [Category("Behavior")]
        [DefaultValue(LENGTHDEFAULT)]
        [Description("Number of CaptchaChars used in the CAPTCHA text")]
        public int CaptchaLength
        {
            get
            {
                return this.captchaLength;
            }

            set
            {
                this.captchaLength = value;
            }
        }

        /// <summary>Gets or sets the width of the Captcha image.</summary>
        [Category("Appearance")]
        [Description("Width of Captcha Image.")]
        public Unit CaptchaWidth
        {
            get
            {
                return this.captchaWidth;
            }

            set
            {
                this.captchaWidth = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the Viewstate is enabled.</summary>
        [Browsable(false)]
        public override bool EnableViewState
        {
            get
            {
                return base.EnableViewState;
            }

            set
            {
                base.EnableViewState = value;
            }
        }

        /// <summary>Gets or sets the ErrorMessage to display if the control is invalid.</summary>
        [Category("Behavior")]
        [Description("The Error Message to display if invalid.")]
        [DefaultValue("")]
        public string ErrorMessage { get; set; }

        /// <summary>Gets or sets the Expiration time in seconds.</summary>
        [Category("Behavior")]
        [Description("The duration of time (seconds) a user has before the challenge expires.")]
        [DefaultValue(EXPIRATIONDEFAULT)]
        public int Expiration
        {
            get
            {
                return this.expiration;
            }

            set
            {
                this.expiration = value;
            }
        }

        /// <summary>Gets or sets the Url to use to render the control.</summary>
        [Category("Behavior")]
        [Description("The URL used to render the image to the client.")]
        [DefaultValue(RENDERURLDEFAULT)]
        public string RenderUrl
        {
            get
            {
                return this.renderUrl;
            }

            set
            {
                this.renderUrl = value;
            }
        }

        /// <summary>Gets or sets the Help Text to use.</summary>
        [Category("Captcha")]
        [DefaultValue("Enter the code shown above:")]
        [Description("Instructional text displayed next to CAPTCHA image.")]
        public string Text { get; set; }

        private bool IsDesignMode
        {
            get
            {
                return HttpContext.Current == null;
            }
        }

        /// <summary>LoadPostData loads the Post Back Data and determines whether the value has change.</summary>
        /// <param name="postDataKey">A key to the PostBack Data to load.</param>
        /// <param name="postCollection">A name value collection of postback data.</param>
        /// <returns>Always <see langword="false"/>.</returns>
        public virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            this.userText = postCollection[postDataKey];
            this.Validate(this.userText);
            if (!this.isValid && !string.IsNullOrEmpty(this.userText))
            {
                this.captchaText = this.GetNextCaptcha();
            }

            return false;
        }

        /// <summary>RaisePostDataChangedEvent runs when the PostBackData has changed.</summary>
        public void RaisePostDataChangedEvent()
        {
        }

        /// <summary>Validates the posted back data.</summary>
        /// <param name="userData">The user entered data.</param>
        /// <returns><see langword="true"/> if the data is valid, otherwise <see langword="false"/>.</returns>
        public bool Validate(string userData)
        {
            var cacheKey = string.Format(DataCache.CaptchaCacheKey, userData);
            var cacheObj = DataCache.GetCache(cacheKey);

            if (cacheObj == null)
            {
                this.isValid = false;
            }
            else
            {
                this.isValid = true;
                DataCache.RemoveCache(cacheKey);
            }

            this.OnUserValidated(new ServerValidateEventArgs(this.captchaText, this.isValid));
            return this.isValid;
        }

        /// <summary>GenerateImage generates the Captcha Image.</summary>
        /// <param name="encryptedText">The Encrypted Text to display.</param>
        /// <returns>A <see cref="Bitmap"/> instance.</returns>
        internal static Bitmap GenerateImage(string encryptedText)
        {
            string encodedText = Decrypt(encryptedText);
            Bitmap bmp = null;
            string[] settings = Regex.Split(encodedText, separator);
            try
            {
                int width;
                int height;
                if (int.TryParse(settings[0], out width) && int.TryParse(settings[1], out height))
                {
                    string text = settings[2];
                    string backgroundImage = settings[3];

                    Graphics g;
                    Brush b = new SolidBrush(Color.LightGray);
                    Brush b1 = new SolidBrush(Color.Black);
                    if (string.IsNullOrEmpty(backgroundImage))
                    {
                        bmp = CreateImage(width, height);
                    }
                    else
                    {
                        bmp = (Bitmap)System.Drawing.Image.FromFile(HttpContext.Current.Request.MapPath(backgroundImage));
                    }

                    g = Graphics.FromImage(bmp);

                    // Create Text
                    GraphicsPath textPath = CreateText(text, width, height, g);
                    if (string.IsNullOrEmpty(backgroundImage))
                    {
                        g.FillPath(b, textPath);
                    }
                    else
                    {
                        g.FillPath(b1, textPath);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }

            return bmp;
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (this.CaptchaWidth.IsEmpty || this.CaptchaWidth.Type != UnitType.Pixel || this.CaptchaHeight.IsEmpty || this.CaptchaHeight.Type != UnitType.Pixel)
            {
                throw new InvalidOperationException("Must specify size of control in pixels.");
            }

            this.image = new Image { BorderColor = this.BorderColor, BorderStyle = this.BorderStyle, BorderWidth = this.BorderWidth, ToolTip = this.ToolTip, EnableViewState = false };
            this.Controls.Add(this.image);
        }

        /// <summary>Gets the next Captcha.</summary>
        /// <returns>The challenge string.</returns>
        protected virtual string GetNextCaptcha()
        {
            var sb = new StringBuilder();
            var rand = new Random();
            int n;
            var intMaxLength = this.CaptchaChars.Length;

            for (n = 0; n <= this.CaptchaLength - 1; n++)
            {
                sb.Append(this.CaptchaChars.Substring(rand.Next(intMaxLength), 1));
            }

            var challenge = sb.ToString();

            // NOTE: this could be a problem in a web farm using in-memory caching where
            // the request might go to another server in the farm. Also, in a system
            // with a single server or web-farm, the cache might be cleared
            // which will cause a problem in such case unless sticky sessions are used.
            var cacheKey = string.Format(DataCache.CaptchaCacheKey, challenge);
            DataCache.SetCache(
                cacheKey,
                challenge,
                (DNNCacheDependency)null,
                DateTime.Now.AddSeconds(this.expiration + 1),
                Cache.NoSlidingExpiration,
                CacheItemPriority.AboveNormal,
                null);
            return challenge;
        }

        /// <inheritdoc />
        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                // Load State from the array of objects that was saved at SaveViewState.
                var myState = (object[])savedState;

                // Load the ViewState of the Base Control
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }

                // Load the CAPTCHA Text from the ViewState
                if (myState[1] != null)
                {
                    this.captchaText = Convert.ToString(myState[1]);
                }
            }

            // var cacheKey = string.Format(DataCache.CaptchaCacheKey, masterPortalId);
            // _CaptchaText
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            // Generate Random Challenge Text
            this.captchaText = this.GetNextCaptcha();

            // Enable Viewstate Encryption
            this.Page.RegisterRequiresViewStateEncryption();

            // Call Base Class method
            base.OnPreRender(e);
        }

        protected virtual void OnUserValidated(ServerValidateEventArgs e)
        {
            ServerValidateEventHandler handler = this.UserValidated;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
            this.ControlStyle.AddAttributesToRender(writer);

            // Render outer <div> Tag
            writer.AddAttribute("class", "dnnLeft");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // Render image <img> Tag
            writer.AddAttribute(HtmlTextWriterAttribute.Src, this.GetUrl());
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            if (!string.IsNullOrEmpty(this.ToolTip))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, this.ToolTip);
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, Localization.GetString("CaptchaAlt.Text", Localization.SharedResourceFile));
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();

            // Render Help Text
            if (!string.IsNullOrEmpty(this.Text))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(this.Text);
                writer.RenderEndTag();
            }

            // Render text box <input> Tag
            this.TextBoxStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Style, "width:" + this.Width);
            writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, this.captchaText.Length.ToString());
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            if (!string.IsNullOrEmpty(this.AccessKey))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, this.AccessKey);
            }

            if (!this.Enabled)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }

            if (this.TabIndex > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, this.TabIndex.ToString());
            }

            if (this.userText == this.captchaText)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, this.userText);
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, string.Empty);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();

            // Render error message
            if (!this.IsValid && this.Page.IsPostBack && !string.IsNullOrEmpty(this.userText))
            {
                this.ErrorStyle.AddAttributesToRender(writer);
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.Write(this.ErrorMessage);
                writer.RenderEndTag();
            }

            // Render </div>
            writer.RenderEndTag();
        }

        /// <inheritdoc />
        protected override object SaveViewState()
        {
            var baseState = base.SaveViewState();
            var allStates = new object[2];
            allStates[0] = baseState;
            if (string.IsNullOrEmpty(this.captchaText))
            {
                this.captchaText = this.GetNextCaptcha();
            }

            allStates[1] = this.captchaText;

            return allStates;
        }

        /// <summary>Creates the Image.</summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        private static Bitmap CreateImage(int width, int height)
        {
            var bmp = new Bitmap(width, height);
            Graphics g;
            var rect = new Rectangle(0, 0, width, height);
            var rectF = new RectangleF(0, 0, width, height);

            g = Graphics.FromImage(bmp);

            Brush b = new LinearGradientBrush(
                rect,
                Color.FromArgb(Rand.Next(224), Rand.Next(224), Rand.Next(224)),
                Color.FromArgb(Rand.Next(224), Rand.Next(224), Rand.Next(224)),
                Convert.ToSingle(Rand.NextDouble()) * 360,
                false);
            g.FillRectangle(b, rectF);

            if (Rand.Next(2) == 1)
            {
                DistortImage(ref bmp, Rand.Next(5, 20));
            }
            else
            {
                DistortImage(ref bmp, -Rand.Next(5, 20));
            }

            return bmp;
        }

        /// <summary>Creates the Text.</summary>
        /// <param name="text">The text to display.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="g">Graphic draw context.</param>
        private static GraphicsPath CreateText(string text, int width, int height, Graphics g)
        {
            var textPath = new GraphicsPath();
            var ff = GetFont();
            var emSize = Convert.ToInt32(width * 2 / text.Length);
            Font f = null;
            try
            {
                var measured = new SizeF(0, 0);
                var workingSize = new SizeF(width, height);
                while (emSize > 2)
                {
                    f = new Font(ff, emSize);
                    measured = g.MeasureString(text, f);
                    if (!(measured.Width > workingSize.Width || measured.Height > workingSize.Height))
                    {
                        break;
                    }

                    f.Dispose();
                    emSize -= 2;
                }

                emSize += 8;
                f = new Font(ff, emSize);

                var fmt = new StringFormat();
                fmt.Alignment = StringAlignment.Center;
                fmt.LineAlignment = StringAlignment.Center;

                textPath.AddString(text, f.FontFamily, Convert.ToInt32(f.Style), f.Size, new RectangleF(0, 0, width, height), fmt);
                WarpText(ref textPath, new Rectangle(0, 0, width, height));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
            finally
            {
                f.Dispose();
            }

            return textPath;
        }

        /// <summary>Decrypts the CAPTCHA Text.</summary>
        /// <param name="encryptedContent">The encrypted text.</param>
        private static string Decrypt(string encryptedContent)
        {
            string decryptedText = string.Empty;
            try
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(encryptedContent);
                if (!ticket.Expired)
                {
                    decryptedText = ticket.UserData;
                }
            }
            catch (ArgumentException exc)
            {
                Logger.Debug(exc);
            }

            return decryptedText;
        }

        /// <summary>DistortImage distorts the captcha image.</summary>
        /// <param name="b">The Image to distort.</param>
        /// <param name="distortion">Distortion.</param>
        private static void DistortImage(ref Bitmap b, double distortion)
        {
            int width = b.Width;
            int height = b.Height;

            var copy = (Bitmap)b.Clone();
            for (int y = 0; y <= height - 1; y++)
            {
                for (int x = 0; x <= width - 1; x++)
                {
                    int newX = Convert.ToInt32(x + (distortion * Math.Sin(Math.PI * y / 64.0)));
                    int newY = Convert.ToInt32(y + (distortion * Math.Cos(Math.PI * x / 64.0)));
                    if (newX < 0 || newX >= width)
                    {
                        newX = 0;
                    }

                    if (newY < 0 || newY >= height)
                    {
                        newY = 0;
                    }

                    b.SetPixel(x, y, copy.GetPixel(newX, newY));
                }
            }
        }

        /// <summary>Encrypts the CAPTCHA Text.</summary>
        /// <param name="content">The text to encrypt.</param>
        /// <param name="expiration">The time the ticket expires.</param>
        private static string Encrypt(string content, DateTime expiration)
        {
            var ticket = new FormsAuthenticationTicket(1, HttpContext.Current.Request.UserHostAddress, DateTime.Now, expiration, false, content);
            return FormsAuthentication.Encrypt(ticket);
        }

        /// <summary>GetFont gets a random font to use for the Captcha Text.</summary>
        private static FontFamily GetFont()
        {
            FontFamily font = null;
            while (font == null)
            {
                try
                {
                    font = new FontFamily(FontFamilies[Rand.Next(FontFamilies.Length)]);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    font = null;
                }
            }

            return font;
        }

        /// <summary>Generates a random point.</summary>
        /// <param name="xmin">The minimum x value.</param>
        /// <param name="xmax">The maximum x value.</param>
        /// <param name="ymin">The minimum y value.</param>
        /// <param name="ymax">The maximum y value.</param>
        private static PointF RandomPoint(int xmin, int xmax, int ymin, int ymax)
        {
            return new PointF(Rand.Next(xmin, xmax), Rand.Next(ymin, ymax));
        }

        /// <summary>Warps the Text.</summary>
        /// <param name="textPath">The Graphics Path for the text.</param>
        /// <param name="rect">a rectangle which defines the image.</param>
        private static void WarpText(ref GraphicsPath textPath, Rectangle rect)
        {
            int intWarpDivisor;
            var rectF = new RectangleF(0, 0, rect.Width, rect.Height);

            intWarpDivisor = Rand.Next(4, 8);

            int intHrange = Convert.ToInt32(rect.Height / intWarpDivisor);
            int intWrange = Convert.ToInt32(rect.Width / intWarpDivisor);

            PointF p1 = RandomPoint(0, intWrange, 0, intHrange);
            PointF p2 = RandomPoint(rect.Width - (intWrange - Convert.ToInt32(p1.X)), rect.Width, 0, intHrange);
            PointF p3 = RandomPoint(0, intWrange, rect.Height - (intHrange - Convert.ToInt32(p1.Y)), rect.Height);
            PointF p4 = RandomPoint(rect.Width - (intWrange - Convert.ToInt32(p3.X)), rect.Width, rect.Height - (intHrange - Convert.ToInt32(p2.Y)), rect.Height);

            var points = new[] { p1, p2, p3, p4 };
            var m = new Matrix();
            m.Translate(0, 0);
            textPath.Warp(points, rectF, m, WarpMode.Perspective, 0);
        }

        /// <summary>Builds the URL for the Handler.</summary>
        private string GetUrl()
        {
            var url = this.ResolveUrl(this.RenderUrl);
            url += "?" + KEY + "=" + Encrypt(this.EncodeTicket(), DateTime.Now.AddSeconds(this.Expiration));

            // Append the Alias to the url so that it doesn't lose track of the alias it's currently on
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            url += "&alias=" + portalSettings.PortalAlias.HTTPAlias;
            return url;
        }

        /// <summary>Encodes the querystring to pass to the Handler.</summary>
        private string EncodeTicket()
        {
            var sb = new StringBuilder();

            sb.Append(this.CaptchaWidth.Value.ToString());
            sb.Append(separator + this.CaptchaHeight.Value);
            sb.Append(separator + this.captchaText);
            sb.Append(separator + this.BackGroundImage);

            return sb.ToString();
        }
    }
}
