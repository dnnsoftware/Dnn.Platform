using DotNetNuke.Entities.Portals.Internal;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DotNetNuke.Entities.Portals.Templates
{
    public class PortalTemplate : XmlDocument
    {
        public PortalTemplate(IPortalTemplateInfo templateToLoad)
        {
            var buffer = new StringBuilder(File.ReadAllText(templateToLoad.TemplateFilePath));

            if (!string.IsNullOrEmpty(templateToLoad.LanguageFilePath))
            {
                XDocument languageDoc;
                using (var reader = PortalTemplateIO.Instance.OpenTextReader(templateToLoad.LanguageFilePath))
                {
                    languageDoc = XDocument.Load(reader);
                }

                var localizedData = languageDoc.Descendants("data");

                foreach (var item in localizedData)
                {
                    var nameAttribute = item.Attribute("name");
                    if (nameAttribute != null)
                    {
                        string name = nameAttribute.Value;
                        var valueElement = item.Descendants("value").FirstOrDefault();
                        if (valueElement != null)
                        {
                            string value = valueElement.Value;

                            buffer = buffer.Replace(string.Format("[{0}]", name), value);
                        }
                    }
                }
            }

            this.LoadXml(buffer.ToString());
        }
    }
}
