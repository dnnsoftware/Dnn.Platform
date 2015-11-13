using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Threading;
using DotNetNuke.Collections.Internal;
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


		    const int width = 190;
		    const int height = 220;
            Bitmap bitmap = new Bitmap(width,height);
			Brush backColorBrush = new SolidBrush(Color.White);
			Brush colorBrush = new SolidBrush(Color.Black);
			

			using (Graphics objGraphics = Graphics.FromImage(bitmap))
			{
			    int x = 6;
			    int y = 7;
                
                // Initialize graphics
				objGraphics.Clear(Color.White);
				objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
				objGraphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

				// Fill bitmap with backcolor
				objGraphics.FillRectangle(backColorBrush,0,0, width,height);
				
				var font = new Font("Arial", 10, FontStyle.Bold);
                var medfontbold = new Font("Arial", 9, FontStyle.Bold);
                var smallfont = new Font("Arial", 7, FontStyle.Regular);
				
                StringFormat leftFormat = new StringFormat() {Alignment = StringAlignment.Near,LineAlignment = StringAlignment.Center};
                StringFormat rightFormat = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };

				// Module name
                Rectangle rectangle = new Rectangle(x, y, 150, 13);
                objGraphics.DrawString(mi.ModuleDefinition.FriendlyName, font, colorBrush, rectangle, leftFormat);

                // Draw flag if culture code not empty
                if (!String.IsNullOrEmpty(mi.CultureCode))
                {
                    string flagFile = Globals.ApplicationMapPath + @"\images\flags\" + mi.CultureCode + ".gif";
                    if (File.Exists(flagFile))
                    {
                        objGraphics.DrawImage(new Bitmap(flagFile), new Rectangle(167, y, 17, 10));
                    }
                }
                
                // Description
                int rows = Convert.ToInt32(Math.Min(mi.DesktopModule.Description.Length, 224) / 37);
                int descHeight = rows * 10 + 2;
			    y = y + 17;
                rectangle = new Rectangle(x,y,175,descHeight);
                objGraphics.DrawString(mi.DesktopModule.Description, smallfont, colorBrush, rectangle, leftFormat);

                // Title
			    y = y + descHeight + 10;
                objGraphics.DrawString("Title:", medfontbold, colorBrush, x, y, leftFormat);
			    y = y + 14;
                objGraphics.DrawString(mi.ModuleTitle, smallfont, colorBrush, x,y, leftFormat);

                // Container
			    y = y + 17;
                objGraphics.DrawString("Container:", medfontbold, colorBrush, x,y, leftFormat);
                float offset = objGraphics.MeasureString("Container:", medfontbold).Width;
                objGraphics.DrawString(cntSource, medfontbold, new SolidBrush(Color.DarkGreen), x + offset, y, leftFormat);

                y = y + 14;
                objGraphics.DrawString(container, smallfont, colorBrush, x,y, leftFormat);

                // Pane
                y = y + 17;
                objGraphics.DrawString("Pane:", medfontbold, colorBrush, x, y, leftFormat);
                y = y + 14;
                objGraphics.DrawString(mi.PaneName, smallfont, colorBrush, x, y, leftFormat);

                // Expiration
			    if (mi.StartDate > new DateTime(1900, 1, 1) || mi.EndDate > new DateTime(1900, 1, 1))
			    {
                    string calFile = Globals.ApplicationMapPath + @"\images\calendar.png";
                    if (File.Exists(calFile))
                    {
                        objGraphics.DrawImage(new Bitmap(calFile), new Rectangle(x, 200, 11, 11));
                    }
                    
                    rectangle = new Rectangle(x + 15, 200, 115, 12);
                    objGraphics.DrawString(mi.StartDate.ToShortDateString()+" - " + mi.EndDate.ToShortDateString(), smallfont, new SolidBrush(Color.Red), rectangle, leftFormat);
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
