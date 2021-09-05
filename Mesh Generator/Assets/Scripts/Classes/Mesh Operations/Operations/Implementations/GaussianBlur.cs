using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class GaussianBlur
{
    private const float NearBlack = 0.0000001f;
    public static float E = 2.718281828459045f;
    public static float[,] Blur(float[,] map, int dim, BlurMetaData metaData, float min, float max, CEvent_String checkpoint)
    {
        float[,] tempgrid = new float[dim, dim];
        float[] kernel = FlatKernel(metaData.KernelSize);

        float normal = 0;

        for (int i = 0; i < kernel.Length; ++i)
        {
            normal += kernel[i];
        }
        
        // keep track of the min/max as we convolve each col
        float mincon = float.MaxValue;
        float maxcon = float.MinValue;
        
        // first pass
        // vertical kernel
        for (int row = 0; row < dim; ++row)
        {
            for (int col = 0; col < dim; ++col)
            {
                float val = ConvolveVertical(map, dim, row, col, kernel, metaData, min, max);
                float vnormalized = val / normal;
                
                if (vnormalized < mincon) mincon = vnormalized;
                if (vnormalized > maxcon) maxcon = vnormalized;

                tempgrid[row, col] = vnormalized;
            }
        }
        
        Debug.Log("(Vertical pass) MAX = " + maxcon + " MIN = " + mincon);

        // second pass
        for (int row = 0; row < dim; ++row)
        {
            for (int col = 0; col < dim; ++col)
            {
                // we dont need to add the element back to the convolved value
                // float val = tempgrid[row, col] + ConvolveHorizonal(tempgrid, dim, row, col, kernel, metaData, min, max);
                
                float val = ConvolveHorizonal(tempgrid, dim, row, col, kernel, metaData, mincon, maxcon);
                
                float vnormalized = val / normal;

                if (vnormalized < mincon) mincon = vnormalized;
                if (vnormalized > maxcon) maxcon = vnormalized;

                tempgrid[row, col] = vnormalized;
                // tempgrid[row, col] = Putils.Remap(tempgrid[row, col], 0, normal, 0, 1);
            }
        }
        
        Debug.Log("(Horizontal pass) MAX = " + maxcon + " MIN = " + mincon);
        
        Debug.Log("(MIN) " + 
                  mincon + "->" + Putils.Remap(mincon, mincon, maxcon, 0, 1) +
                  "(MAX) " +
                  maxcon + "->" + Putils.Remap(maxcon, mincon, maxcon, 0, 1)
                  );
        
        // now go back and remap all of the values
        for (int row = 0; row < dim; ++row)
        {
            for (int col = 0; col < dim; ++col)
            {
                tempgrid[row, col] = Putils.Remap(tempgrid[row, col], mincon, maxcon, 0, 1);
            }
        }

        return tempgrid;
    }

    public static float[] FlatKernel(int ksize)
    {
        int kcenter = (ksize - 1) / 2;
        float[,] kernel2d = new float[ksize, ksize];
        float[] kernel1d = new float[ksize];
        float sum = 0;

        for (int i = 0; i < ksize; ++i)
        {
            float rowsum = 0;
            for (int j = 0; j < ksize; ++j)
            {
                int x = j - kcenter;
                int y = i - kcenter;
                float gaussian = Guassian2D(x, y, ksize);
                sum += gaussian;
                rowsum += gaussian;
                kernel2d[i, j] = gaussian;
            }

            kernel1d[i] = rowsum;
        }

        for (int i = 0; i < ksize; ++i)
        {
            kernel1d[i] /= sum;
        }
        
        return kernel1d;
        
        // float[] kernel = new float[ksize];
        // int center = (ksize - 1) / 2;
        // float sum = 0;
        //
        // for ( int i = 0; i < ((ksize - 1) / 2) + 1; ++i )
        // {
        //     float co = Guassian2D(center, center + i, ksize);
        //     kernel[center + i] = co;
        //     kernel[center - i] = co;
        //     sum += co;
        // }
        //
        // for (int i = 0; i < ksize; ++i)
        // {
        //     kernel[i] /= sum;
        // }
        //
        // return kernel;
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

    public static float ConvolveVertical(float[,] map, int dim, int row, int col, float[] kernel, BlurMetaData metaData, float min, float max)
    {
        int kcenter = (metaData.KernelSize - 1) / 2; // the number of elements on either side of the center
        
        float height = SampleAndRemap(map, row, col, min, max);

        float sum = height * kernel[kcenter];

        for (int k = 1; k < kcenter + 1; ++k)
        {
            // look down k rows
            float weight = kernel[kcenter - k];
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
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Mirror)
                {
                    if (row + k < dim)
                        sum += weight * SampleAndRemap(map, row + k, col, min, max);
                }
                
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
                    sum += weight * NearBlack;
                
                // mirror the values to the side of the kernel off the edge of the grid
                // look at the bottom half of the kernel
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Mirror)
                {
                    if ( row - k >= 0 )
                        sum += weight * SampleAndRemap(map, row - k, col, min, max);
                }
                
                // pad the kernel with the edge values
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Nearest)
                {
                    int knearest = k;
                    // move point down until it's within the grid
                    while (row + knearest >= dim) --knearest;
                    sum += weight * SampleAndRemap(map, row + knearest, col, min, max);
                }
            }
        }

        // sum = Mathf.Clamp01(sum);
        return sum;
    }
    
    public static float ConvolveHorizonal(float[,] map, int dim, int row, int col, float[] kernel, BlurMetaData metaData, float min, float max)
    {
        int kcenter = (metaData.KernelSize - 1) / 2; // the number of elements on either side of the center
        
        float height = SampleAndRemap(map, row, col, min, max);

        float sum = height * kernel[kcenter];
        float weight;
        
        for (int k = 1; k < kcenter + 1; ++k)
        {
            // look right k cols 
            weight = kernel[kcenter + k];
            if (col + k < dim) sum += weight * SampleAndRemap(map, row, col + k, min, max);
            else
            {
                // blend with white
                if (metaData.Mode == Enums.GaussianBlurBorderModes.BlendWhite) 
                    sum += weight * 1f;
                
                // blend with black
                else if ( metaData.Mode == Enums.GaussianBlurBorderModes.BlendBlack ) 
                    sum += weight * NearBlack;
                
                // mirror the values to the side of the kernel off the edge of the grid
                // look at the left side of the kernel
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Mirror)
                {
                    if ( col - k >= 0 )
                        sum += weight * SampleAndRemap(map, row, col - k, min, max);
                }

                // pad the kernel with the edge values
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Nearest)
                {
                    int knearest = k;
                    while (kcenter + knearest >= dim) --knearest;
                    sum += weight * SampleAndRemap(map, row, col + knearest, min, max);
                }
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Ignore)
                {
                    sum += 0;
                }
            }
            
            // look left k cols
            weight = kernel[kcenter - k];
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
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Mirror)
                {
                    if ( col + k < dim )
                        sum += weight * SampleAndRemap(map, row, col + k, min, max);
                }

                // pad the kernel with the edge values
                else if ( metaData.Mode == Enums.GaussianBlurBorderModes.Nearest )
                {
                    int knearest = k;
                    while (col - knearest < 0) ++knearest;
                    sum += weight * SampleAndRemap(map, row, col - knearest, min, max);
                }
                else if (metaData.Mode == Enums.GaussianBlurBorderModes.Ignore)
                {
                    sum += 0;
                }
            }
        }

        // sum = Mathf.Clamp01(sum);
        return sum;
    }

    private static float SampleAndRemap(float[,] map, int row, int col, float min, float max)
    {
        return Putils.Remap(map[row, col], min, max, 0, 1);
    }
}
