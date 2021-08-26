using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperationMetaData
{
    private Enums.OperationTypes operationType;
    public BlurMetaData blurData = new BlurMetaData();
    public StretchMetaData stretchData = new StretchMetaData();

    public Enums.OperationTypes OperationType
    {
        get => operationType;
        set => operationType = value;
    }
}
