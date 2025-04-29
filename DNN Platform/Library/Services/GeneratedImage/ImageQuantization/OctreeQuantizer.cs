// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.GeneratedImage.ImageQuantization;

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

/// <summary>Quantize using an Octree.</summary>
public class OctreeQuantizer : Quantizer
{
    /// <summary>Stores the tree.</summary>
    private Octree octree;

    /// <summary>Maximum allowed color depth.</summary>
    private int maxColors;

    /// <summary>Initializes a new instance of the <see cref="OctreeQuantizer"/> class.</summary>
    /// <remarks>
    /// The Octree quantizer is a two pass algorithm. The initial pass sets up the octree,
    /// the second pass quantizes a color based on the nodes in the tree.
    /// </remarks>
    /// <param name="maxColors">The maximum number of colors to return.</param>
    /// <param name="maxColorBits">The number of significant bits.</param>
    public OctreeQuantizer(int maxColors, int maxColorBits)
        : base(false)
    {
        if (maxColors > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(maxColors), maxColors, "The number of colors should be less than 256");
        }

        if ((maxColorBits < 1) | (maxColorBits > 8))
        {
            throw new ArgumentOutOfRangeException(nameof(maxColorBits), maxColorBits, "This should be between 1 and 8");
        }

        // Construct the octree
        this.octree = new Octree(maxColorBits);
        this.maxColors = maxColors;
    }

    /// <inheritdoc />
    protected override void InitialQuantizePixel(Color32 pixel)
    {
        // Add the color to the octree
        this.octree.AddColor(pixel);
    }

    /// <inheritdoc />
    protected override byte QuantizePixel(Color32 pixel)
    {
        byte paletteIndex = (byte)this.maxColors;   // The color at [_maxColors] is set to transparent

        // Get the palette index if this non-transparent
        if (pixel.Alpha > 0)
        {
            paletteIndex = (byte)this.octree.GetPaletteIndex(pixel);
        }

        return paletteIndex;
    }

    /// <inheritdoc />
    protected override ColorPalette GetPalette(ColorPalette original)
    {
        // First off convert the octree to _maxColors colors
        ArrayList palette = this.octree.Palletize(this.maxColors - 1);

        // Then convert the palette based on those colors
        for (int index = 0; index < palette.Count; index++)
        {
            original.Entries[index] = (Color)palette[index];
        }

        // Add the transparent color
        original.Entries[this.maxColors] = Color.FromArgb(0, 0, 0, 0);

        return original;
    }

    /// <summary>Class which does the actual quantization.</summary>
    private class Octree
    {
        /// <summary>Mask used when getting the appropriate pixels for a given node.</summary>
        private static readonly int[] Mask = new int[8] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        /// <summary>The root of the octree.</summary>
        private OctreeNode root;

        /// <summary>Number of leaves in the tree.</summary>
        private int leafCount;

        /// <summary>Array of reducible nodes.</summary>
        private OctreeNode[] reducibleNodes;

        /// <summary>Maximum number of significant bits in the image.</summary>
        private int maxColorBits;

        /// <summary>Store the last node quantized.</summary>
        private OctreeNode previousNode;

        /// <summary>Cache the previous color quantized.</summary>
        private int previousColor;

        /// <summary>Initializes a new instance of the <see cref="Octree"/> class.</summary>
        /// <param name="maxColorBits">The maximum number of significant bits in the image.</param>
        public Octree(int maxColorBits)
        {
            this.maxColorBits = maxColorBits;
            this.leafCount = 0;
            this.reducibleNodes = new OctreeNode[9];
            this.root = new OctreeNode(0, this.maxColorBits, this);
            this.previousColor = 0;
            this.previousNode = null;
        }

        /// <summary>Gets or sets the number of leaves in the tree.</summary>
        public int Leaves
        {
            get { return this.leafCount; }
            set { this.leafCount = value; }
        }

        /// <summary>Gets the array of reducible nodes.</summary>
        protected OctreeNode[] ReducibleNodes
        {
            get { return this.reducibleNodes; }
        }

        /// <summary>Add a given color value to the octree.</summary>
        /// <param name="pixel">The pixel color.</param>
        public void AddColor(Color32 pixel)
        {
            // Check if this request is for the same color as the last
            if (this.previousColor == pixel.ARGB)
            {
                // If so, check if I have a previous node setup. This will only ocurr if the first color in the image
                // happens to be black, with an alpha component of zero.
                if (this.previousNode == null)
                {
                    this.previousColor = pixel.ARGB;
                    this.root.AddColor(pixel, this.maxColorBits, 0, this);
                }
                else
                {
                    // Just update the previous node
                    this.previousNode.Increment(pixel);
                }
            }
            else
            {
                this.previousColor = pixel.ARGB;
                this.root.AddColor(pixel, this.maxColorBits, 0, this);
            }
        }

        /// <summary>Reduce the depth of the tree.</summary>
        public void Reduce()
        {
            int index;

            // Find the deepest level containing at least one reducible node
            for (index = this.maxColorBits - 1; (index > 0) && (this.reducibleNodes[index] == null); index--)
            {
            }

            // Reduce the node most recently added to the list at level 'index'
            OctreeNode node = this.reducibleNodes[index];
            this.reducibleNodes[index] = node.NextReducible;

            // Decrement the leaf count after reducing the node
            this.leafCount -= node.Reduce();

            // And just in case I've reduced the last color to be added, and the next color to
            // be added is the same, invalidate the previousNode...
            this.previousNode = null;
        }

        /// <summary>Convert the nodes in the octree to a palette with a maximum of colorCount colors.</summary>
        /// <param name="colorCount">The maximum number of colors.</param>
        /// <returns>An arraylist with the palettized colors.</returns>
        public ArrayList Palletize(int colorCount)
        {
            while (this.Leaves > colorCount)
            {
                this.Reduce();
            }

            // Now palettize the nodes
            ArrayList palette = new ArrayList(this.Leaves);
            int paletteIndex = 0;
            this.root.ConstructPalette(palette, ref paletteIndex);

            // And return the palette
            return palette;
        }

        /// <summary>Get the palette index for the passed color.</summary>
        /// <param name="pixel">The pixel color.</param>
        /// <returns>The index.</returns>
        public int GetPaletteIndex(Color32 pixel)
        {
            return this.root.GetPaletteIndex(pixel, 0);
        }

        /// <summary>Keep track of the previous node that was quantized.</summary>
        /// <param name="node">The node last quantized.</param>
        protected void TrackPrevious(OctreeNode node)
        {
            this.previousNode = node;
        }

        /// <summary>Class which encapsulates each node in the tree.</summary>
        protected class OctreeNode
        {
            /// <summary>Flag indicating that this is a leaf node.</summary>
            private bool leaf;

            /// <summary>Number of pixels in this node.</summary>
            private int pixelCount;

            /// <summary>Red component.</summary>
            private int red;

            /// <summary>Green Component.</summary>
            private int green;

            /// <summary>Blue component.</summary>
            private int blue;

            /// <summary>Pointers to any child nodes.</summary>
            private OctreeNode[] children;

            /// <summary>Pointer to next reducible node.</summary>
            private OctreeNode nextReducible;

            /// <summary>The index of this node in the palette.</summary>
            private int paletteIndex;

            /// <summary>Initializes a new instance of the <see cref="OctreeNode"/> class.</summary>
            /// <param name="level">The level in the tree = 0 - 7.</param>
            /// <param name="colorBits">The number of significant color bits in the image.</param>
            /// <param name="octree">The tree to which this node belongs.</param>
            public OctreeNode(int level, int colorBits, Octree octree)
            {
                // Construct the new node
                this.leaf = level == colorBits;

                this.red = this.green = this.blue = 0;
                this.pixelCount = 0;

                // If a leaf, increment the leaf count
                if (this.leaf)
                {
                    octree.Leaves++;
                    this.nextReducible = null;
                    this.children = null;
                }
                else
                {
                    // Otherwise add this to the reducible nodes
                    this.nextReducible = octree.ReducibleNodes[level];
                    octree.ReducibleNodes[level] = this;
                    this.children = new OctreeNode[8];
                }
            }

            /// <summary>Gets return the child nodes.</summary>
            public OctreeNode[] Children
            {
                get { return this.children; }
            }

            /// <summary>Gets or sets the next reducible node.</summary>
            public OctreeNode NextReducible
            {
                get { return this.nextReducible; }
                set { this.nextReducible = value; }
            }

            /// <summary>Add a color into the tree.</summary>
            /// <param name="pixel">The color.</param>
            /// <param name="colorBits">The number of significant color bits.</param>
            /// <param name="level">The level in the tree.</param>
            /// <param name="octree">The tree to which this node belongs.</param>
            public void AddColor(Color32 pixel, int colorBits, int level, Octree octree)
            {
                // Update the color information if this is a leaf
                if (this.leaf)
                {
                    this.Increment(pixel);

                    // Setup the previous node
                    octree.TrackPrevious(this);
                }
                else
                {
                    // Go to the next level down in the tree
                    int shift = 7 - level;
                    int index = ((pixel.Red & Mask[level]) >> (shift - 2)) |
                                ((pixel.Green & Mask[level]) >> (shift - 1)) |
                                ((pixel.Blue & Mask[level]) >> shift);

                    OctreeNode child = this.children[index];

                    if (child == null)
                    {
                        // Create a new child node & store in the array
                        child = new OctreeNode(level + 1, colorBits, octree);
                        this.children[index] = child;
                    }

                    // Add the color to the child node
                    child.AddColor(pixel, colorBits, level + 1, octree);
                }
            }

            /// <summary>Reduce this node by removing all of its children.</summary>
            /// <returns>The number of leaves removed.</returns>
            public int Reduce()
            {
                this.red = this.green = this.blue = 0;
                int children = 0;

                // Loop through all children and add their information to this node
                for (int index = 0; index < 8; index++)
                {
                    if (this.children[index] != null)
                    {
                        this.red += this.children[index].red;
                        this.green += this.children[index].green;
                        this.blue += this.children[index].blue;
                        this.pixelCount += this.children[index].pixelCount;
                        ++children;
                        this.children[index] = null;
                    }
                }

                // Now change this to a leaf node
                this.leaf = true;

                // Return the number of nodes to decrement the leaf count by
                return children - 1;
            }

            /// <summary>Traverse the tree, building up the color palette.</summary>
            /// <param name="palette">The palette.</param>
            /// <param name="paletteIndex">The current palette index.</param>
            public void ConstructPalette(ArrayList palette, ref int paletteIndex)
            {
                if (this.leaf)
                {
                    // Consume the next palette index
                    this.paletteIndex = paletteIndex++;

                    // And set the color of the palette entry
                    palette.Add(Color.FromArgb(this.red / this.pixelCount, this.green / this.pixelCount, this.blue / this.pixelCount));
                }
                else
                {
                    // Loop through children looking for leaves
                    for (int index = 0; index < 8; index++)
                    {
                        if (this.children[index] != null)
                        {
                            this.children[index].ConstructPalette(palette, ref paletteIndex);
                        }
                    }
                }
            }

            /// <summary>Return the palette index for the passed color.</summary>
            public int GetPaletteIndex(Color32 pixel, int level)
            {
                int paletteIndex = this.paletteIndex;

                if (!this.leaf)
                {
                    int shift = 7 - level;
                    int index = ((pixel.Red & Mask[level]) >> (shift - 2)) |
                                ((pixel.Green & Mask[level]) >> (shift - 1)) |
                                ((pixel.Blue & Mask[level]) >> shift);

                    if (this.children[index] != null)
                    {
                        paletteIndex = this.children[index].GetPaletteIndex(pixel, level + 1);
                    }
                    else
                    {
                        throw new Exception("Didn't expect this!");
                    }
                }

                return paletteIndex;
            }

            /// <summary>Increment the pixel count and add to the color information.</summary>
            public void Increment(Color32 pixel)
            {
                this.pixelCount++;
                this.red += pixel.Red;
                this.green += pixel.Green;
                this.blue += pixel.Blue;
            }
        }
    }
}
