using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CalibrationData
{
    public float[,] calibrationTable;

    public CalibrationData (CalibrationController myController){
        // calibrationTable = myController.calibTable; // new float[5,6];

    }
    
}
