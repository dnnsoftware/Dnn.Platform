using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Threading;
using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
	public class ModInfoTransform : ImageTransform
	{
		/// <summary>
		/// Sets the TabID of the page containing the module
		/// </summary>
		public int TabID { get; set; }

		/// <summary>
		/// Sets the ModuleID of the module we want to generate information about
		/// </summary>
		public int ModuleID { get; set; }

		public override string UniqueString
		{
			get { return base.UniqueString + this.TabID.ToString() + "-" + this.ModuleID.ToString(); }
		}

        public ModInfoTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
		}

		public override Image ProcessImage(Image image)
		{
            PortalSettings portalSettings = Globals.GetPortalSettings();
            HostController hc = new HostController();
		    Dictionary<string, string> hostSettings = hc.GetSettingsDictionary();
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(portalSettings.CultureCode);
            
            ModuleController mc = new ModuleController();
		    ModuleInfo mi = mc.GetModule(ModuleID, TabID, true);

		    string cnt = mi.ContainerSrc;
		    string cntSource = "(Module)";
		    if (String.IsNullOrEmpty(cnt))
		    {
		        cnt = portalSettings.DefaultPortalContainer;
		        cntSource = "(Site)";
		        if (string.IsNullOrEmpty(cnt))
		        {
                    cnt = hostSettings["DefaultPortalContainer"];
		            cntSource = "(Host)";
		        }
		    }

		    string container = "";
		    if (cnt.StartsWith("[G]"))
		    {
		        container = "Host: ";
		        cnt = cnt.Replace("[G]Containers/", "");
		    }
            else if (cnt.StartsWith("[S]"))
            {
                container = "Site: ";
                cnt = cnt.Replace("[S]Containers/", "");
            }
		    container += cnt.Replace(".ascx", "").Replace("/", " - ");


		    int Width = 190;
		    int Height = 220;
            Bitmap bitmap = new Bitmap(Width,Height);
			Brush backColorBrush = new SolidBrush(Color.White);
			Brush colorBrush = new SolidBrush(Color.Black);
            Pen colorPen = new Pen(Color.Black, 2);
			

			using (Graphics objGraphics = Graphics.FromImage(bitmap))
			{
				// Initialize graphics
				objGraphics.Clear(Color.White);
				objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
				objGraphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

				// Fill bitmap with backcolor
				objGraphics.FillRectangle(backColorBrush,0,0, Width,Height);
				
				// Draw border
				objGraphics.DrawRectangle(new Pen(Color.FromArgb(255,204,204,204)), 0,0,Width,Height);

				// Draw module title
				// Use rectangle for text and align text to left of rectangle
				var font = new Font("Arial", 10, FontStyle.Bold);
                var medfontbold = new Font("Arial", 9, FontStyle.Bold);
                var medfont = new Font("Arial", 9, FontStyle.Regular);
                var smallfont = new Font("Arial", 7, FontStyle.Regular);
			    var dings = new Font("Webdings", 10, FontStyle.Regular);
				
                StringFormat leftFormat = new StringFormat() {Alignment = StringAlignment.Near,LineAlignment = StringAlignment.Center};
                StringFormat rightFormat = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };

				// Module name
                Rectangle rectangle = new Rectangle(6, 7, 150, 13);
                objGraphics.DrawString(mi.DesktopModule.FriendlyName, font, colorBrush, rectangle, leftFormat);

                // Description
                rectangle = new Rectangle(6,25,175,62);
                objGraphics.DrawString(mi.DesktopModule.Description, smallfont, colorBrush, rectangle, leftFormat);

                // Title
                objGraphics.DrawString("Title:", medfontbold, colorBrush, 6,92, leftFormat);
                objGraphics.DrawString(mi.ModuleTitle, smallfont, colorBrush, 6,104, leftFormat);

                // Container
                objGraphics.DrawString("Container:", medfontbold, colorBrush, 6,118, leftFormat);
                float offset = objGraphics.MeasureString("Container:", medfontbold).Width;
                objGraphics.DrawString(cntSource, medfont, new SolidBrush(Color.DarkGreen), 6 + offset, 118, leftFormat);
                objGraphics.DrawString(container, medfont, colorBrush, 6,132, leftFormat);

                // Expiration
			    if (mi.StartDate > new DateTime(1900, 1, 1) || mi.EndDate > new DateTime(1900, 1, 1))
			    {
                    var img = new Bitmap(Globals.ApplicationMapPath + @"\\images\calendar.png" );
                    
                    objGraphics.DrawImage(img, 6, 193);
                    
                    rectangle = new Rectangle(30, 200, 115, 12);
                    objGraphics.DrawString(mi.StartDate.ToShortDateString()+"-" + mi.EndDate.ToShortDateString(), smallfont, new SolidBrush(Color.Red), rectangle, leftFormat);
			    }
                
                // ModuleId
                rectangle = new Rectangle(120, 200, 64, 12);
                objGraphics.DrawString("ID:"+ ModuleID.ToString(), medfontbold, new SolidBrush(Color.DarkGreen), rectangle, rightFormat);

				// Save indicator to file
				objGraphics.Flush();
			}
			return (Image)bitmap;
		}
	}
}
