using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurMetaData
{
    private int _kernelSize = 1;
    private float _standardDeviation = 1f;
    private Enums.GaussianBlurBorderModes mode = Enums.GaussianBlurBorderModes.BlendWhite;

    public int KernelSize
    {
        get => _kernelSize;
        set => _kernelSize = value;
    }

    public float StandardDeviation
    {
        get => _standardDeviation;
        set => _standardDeviation = value;
    }

    public Enums.GaussianBlurBorderModes Mode
    {
        get => mode;
        set => mode = value;
    }
}
