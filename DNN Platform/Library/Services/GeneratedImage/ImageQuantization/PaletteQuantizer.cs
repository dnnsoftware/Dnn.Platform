﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;

namespace DotNetNuke.Services.GeneratedImage.ImageQuantization
{
    /// <summary>
    /// Summary description for PaletteQuantizer.
    /// </summary>
    [CLSCompliant(false)]
    public  class PaletteQuantizer : Quantizer
	{
		/// <summary>
		/// Construct the palette quantizer
		/// </summary>
		/// <param name="palette">The color palette to quantize to</param>
		/// <remarks>
		/// Palette quantization only requires a single quantization step
		/// </remarks>
		public PaletteQuantizer ( ArrayList palette ) : base ( true )
		{
			this._colorMap = new Hashtable ( ) ;

			this._colors = new Color[palette.Count] ;
			palette.CopyTo ( this._colors ) ;
		}

		/// <summary>
		/// Override this to process the pixel in the second pass of the algorithm
		/// </summary>
		/// <param name="pixel">The pixel to quantize</param>
		/// <returns>The quantized value</returns>
		protected  override byte QuantizePixel ( Color32 pixel )
		{
			byte	colorIndex = 0 ;
			int		colorHash = pixel.ARGB ;	

			// Check if the color is in the lookup table
			if ( this._colorMap.ContainsKey ( colorHash ) )
				colorIndex = (byte)this._colorMap[colorHash] ;
			else
			{
				// Not found - loop through the palette and find the nearest match.
				// Firstly check the alpha value - if 0, lookup the transparent color
				if ( 0 == pixel.Alpha )
				{
					// Transparent. Lookup the first color with an alpha value of 0
					for ( int index = 0 ; index < this._colors.Length ; index++ )
					{
						if ( 0 == this._colors[index].A )
						{
							colorIndex = (byte)index ;
							break ;
						}
					}
				}
				else
				{
					// Not transparent...
					int	leastDistance = int.MaxValue ;
					int red = pixel.Red ;
					int green = pixel.Green;
					int blue = pixel.Blue;

					// Loop through the entire palette, looking for the closest color match
					for ( int index = 0 ; index < this._colors.Length ; index++ )
					{
						Color	paletteColor = this._colors[index];
						
						int	redDistance = paletteColor.R - red ;
						int	greenDistance = paletteColor.G - green ;
						int	blueDistance = paletteColor.B - blue ;

						int		distance = ( redDistance * redDistance ) + 
										   ( greenDistance * greenDistance ) + 
										   ( blueDistance * blueDistance ) ;

						if ( distance < leastDistance )
						{
							colorIndex = (byte)index ;
							leastDistance = distance ;

							// And if it's an exact match, exit the loop
							if ( 0 == distance )
								break ;
						}
					}
				}

				// Now I have the color, pop it into the hashtable for next time
				this._colorMap.Add ( colorHash , colorIndex ) ;
			}

			return colorIndex ;
		}

		/// <summary>
		/// Retrieve the palette for the quantized image
		/// </summary>
		/// <param name="palette">Any old palette, this is overrwritten</param>
		/// <returns>The new color palette</returns>
		protected override ColorPalette GetPalette ( ColorPalette palette )
		{
			for ( int index = 0 ; index < this._colors.Length ; index++ )
				palette.Entries[index] = this._colors[index] ;

			return palette ;
		}

		/// <summary>
		/// Lookup table for colors
		/// </summary>
		private readonly Hashtable _colorMap ;

		/// <summary>
		/// List of all colors in the palette
		/// </summary>
		protected Color[] _colors ;
	}
}
