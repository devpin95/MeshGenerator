using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GaussianBlur
{
    private const float NearBlack = 0.0000001f;
    public static float E = 2.718281828459045f;
    public static float[,] Blur(HeightMap map, BlurMetaData metaData, float min, float max, CEvent_String checkpoint)
    {
        int dim = map.WidthAndHeight();
        float[,] tempgrid = new float[dim, dim];
        float[] kernel = FlatKernel(metaData.KernelSize);

        // first pass
        // vertical kernel
        for (int row = 0; row < dim; ++row)
        {
            for (int col = 0; col < dim; ++col)
            {
                tempgrid[row, col] = Convolve(map, row, col, kernel, metaData, min, max);
            }
        }

        return tempgrid;
    }

    public static float[] FlatKernel(int ksize)
    {
        float[] kernel = new float[ksize];
        int center = (ksize - 1) / 2;
        float sum = 0;

        for ( int i = 0; i < ((ksize - 1) / 2) + 1; ++i )
        {
            float co = Guassian2D(center, center + i, ksize);
            kernel[center + i] = co;
            kernel[center - i] = co;
            sum += co;
        }

        for (int i = 0; i < ksize; ++i)
        {
            kernel[i] /= sum;
        }

        return kernel;
    }
    
    public static float CalculateGaussSigma(int ksize)
    {
        float radius = (ksize - 1) / 2f;
        return radius / 2f;
    }

    public static float Guassian2D(int x, int y, int ksize)
    {
        // https://en.wikipedia.org/wiki/Gaussian_blur
        float coeff = 0;
        float sigma = CalculateGaussSigma(ksize);

        float frac = 1f / (2 * Mathf.PI * sigma * sigma);
        float exp = -(x * x + y * y) / (2 * sigma * sigma);
        float eu = Mathf.Pow(E, exp);
        coeff = frac * eu;
        
        return coeff;
    }

    public static float Convolve(HeightMap map, int row, int col, float[] kernel, BlurMetaData metaData, float min, float max)
    {
        int dim = map.WidthAndHeight(); // the side of the map
        int kcenter = (metaData.KernelSize - 1) / 2; // the number of elements on either side of the center
        
        float height = SampleAndRemap(map, row, col, min, max);

        float sum = height * kernel[kcenter];
        float weight = 0;
        
        for (int k = 1; k < kcenter + 1; ++k)
        {
            // look down k rows
            weight = kernel[kcenter - k];
            
            if (row - k >= 0) sum += weight * SampleAndRemap(map, row - k, col, min, max);
            else
            {
                // blend with white
                if ( metaData.Mode == Enums.GaussianBlurBorderModes.BlendWhite ) 
                    sum += weight * 1f;
                
                // blend with black
                else if ( metaData.Mode == Enums.GaussianBlurBorderModes.BlendBlack ) 
                    sum += weight * NearBlack;
                
                // mirror the values to the side of the kernel off the edge of the grid
                // look at the top half of the kernel
                else if ( metaData.Mode == Enums.GaussianBlurBorderModes.Mirror && row + k < dim ) 
                    sum += weight * SampleAndRemap(map, row + k, col, min, max);
                
                // pad the kernel with the edge values
                else if ( metaData.Mode == Enums.GaussianBlurBorderModes.Nearest )
                {
                    int knearest = k;
                    while (row - knearest < 0) ++knearest;
                    sum += weight * SampleAndRemap(map, row - knearest, col, min, max);
                }
            }
            
            // look up k rows 
            weight = kernel[kcenter + k];
            if (row + k < dim) sum += weight * SampleAndRemap(map, row + k, col, min, max);
            else
            {
                // blend with white
                if (metaData.Mode == Enums.GaussianBlurBorderModes.BlendWhite) 
                    sum += weight * 1f;
                
                // blend with black
                else if ( metaData.Mode == Enums.GaussianBlurBorderModes.BlendBlack ) 
                    sum += kernel[kcenter + k] * NearBlack;
                
                // mirror the values to the side of the kernel off the edge of the grid
                // look at the bottom half of the kernel
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Mirror && row - k >= 0) 
                    sum += weight * SampleAndRemap(map, row + k, col, min, max);
                
                // pad the kernel with the edge values
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Nearest)
                {
                    int knearest = k;
                    // move point down until it's within the grid
                    while (row + knearest >= dim) --knearest;
                    sum += weight * SampleAndRemap(map, row + knearest, col, min, max);
                }
            }
            
            // look right k cols 
            weight = kernel[kcenter + k];
            if (col + k < dim) sum += weight * map.SampleMapAtXY(row, col + k);
            else
            {
                // blend with white
                if (metaData.Mode == Enums.GaussianBlurBorderModes.BlendWhite) 
                    sum += weight * 1f;
                
                // blend with black
                else if ( metaData.Mode == Enums.GaussianBlurBorderModes.BlendBlack ) 
                    sum += kernel[kcenter + k] * NearBlack;
                
                // mirror the values to the side of the kernel off the edge of the grid
                // look at the left side of the kernel
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Mirror && kcenter - k > 0) 
                    sum += weight * SampleAndRemap(map, row, col - k, min, max);
                
                // pad the kernel with the edge values
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Nearest)
                {
                    int knearest = k;
                    while (kcenter + knearest >= dim) --knearest;
                    sum += weight * SampleAndRemap(map, row, col + knearest, min, max);
                }
            }
            
            // look left k cols
            if (col - k >= 0) sum += weight * SampleAndRemap(map, row, col - k, min, max);
            else
            {
                // blend with white
                if ( metaData.Mode == Enums.GaussianBlurBorderModes.BlendWhite ) 
                    sum += weight * 1f;
                
                // blend with black
                else if ( metaData.Mode == Enums.GaussianBlurBorderModes.BlendBlack ) 
                    sum += weight * NearBlack;
                
                // mirror the values to the side of the kernel off the edge of the grid
                // look at the top half of the kernel
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Mirror && row + k < dim)
                    sum += weight * SampleAndRemap(map, row, col + k, min, max);
                
                // pad the kernel with the edge values
                else if ( metaData.Mode == Enums.GaussianBlurBorderModes.Nearest )
                {
                    int knearest = k;
                    while (row - knearest < 0) ++knearest;
                    sum += weight * SampleAndRemap(map, row, col - knearest, min, max);
                }
            }
        }

        return sum;
    }

    private static float SampleAndRemap(HeightMap map, int row, int col, float min, float max)
    {
        return Putils.Remap(map.SampleMapAtXY(row, col), min, max, 0, 1);
    }
}
