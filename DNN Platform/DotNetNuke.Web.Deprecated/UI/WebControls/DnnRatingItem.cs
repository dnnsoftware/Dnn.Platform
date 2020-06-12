
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using Telerik.Web.UI;

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnRatingItem : RadRatingItem
    {
        public DnnRatingItem()
        {
        }

        public DnnRatingItem(string imageUrl)
            : base(imageUrl)
        {
        }

        public DnnRatingItem(string imageUrl, string selectedImageUrl)
            : base(imageUrl, selectedImageUrl)
        {
        }

        public DnnRatingItem(string imageUrl, string selectedImageUrl, string hoveredImageUrl)
            : base(imageUrl, selectedImageUrl, hoveredImageUrl)
        {
        }

        public DnnRatingItem(string imageUrl, string selectedImageUrl, string hoveredImageUrl, string hoveredSelectedImageUrl)
            : base(imageUrl, selectedImageUrl, hoveredImageUrl, hoveredSelectedImageUrl)
        {
        }
    }
}
