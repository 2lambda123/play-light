using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveCalibration(CalibrationController myController){

        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/calibrationData.kp";
        FileStream stream = new FileStream(path, FileMode.Create);
        CalibrationData data = new CalibrationData(myController);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static CalibrationData LoadCalibration (){

        string path = Application.persistentDataPath + "/calibrationData.kp";

        if(File.Exists(path)){

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            CalibrationData data = formatter.Deserialize(stream) as CalibrationData;
            stream.Close();
            return data;

        } else {
            Debug.LogError("CalibData file not found: " + path);
            return null;
        }
    }
}
