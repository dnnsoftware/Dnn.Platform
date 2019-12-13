#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnRatingItem : RadRatingItem
    {
        public DnnRatingItem()
        {
        }

        public DnnRatingItem(string imageUrl) : base(imageUrl)
        {
        }

        public DnnRatingItem(string imageUrl, string selectedImageUrl) : base(imageUrl, selectedImageUrl)
        {
        }

        public DnnRatingItem(string imageUrl, string selectedImageUrl, string hoveredImageUrl) : base(imageUrl, selectedImageUrl, hoveredImageUrl)
        {
        }

        public DnnRatingItem(string imageUrl, string selectedImageUrl, string hoveredImageUrl, string hoveredSelectedImageUrl)
            : base(imageUrl, selectedImageUrl, hoveredImageUrl, hoveredSelectedImageUrl)
        {
        }
    }
}
