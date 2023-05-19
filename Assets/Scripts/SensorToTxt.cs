using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SensorToTxt : MonoBehaviour
{
    [SerializeField] private string fileName = "/data.txt";
    [SerializeField] private float samplingPeriod = 0.1f; // Save data every 1 second
    private float timer = 0f;
    private string filePath;
    private StreamWriter writer;
    private Sensor sensor;

    private void Start()
    {
        filePath = Application.dataPath + fileName;

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Delete the file
            File.Delete(filePath);
        }

        // Create the StreamWriter to write data into the file
        writer = new StreamWriter(filePath);
        sensor = GetComponent<Sensor>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Increment the timer
        timer += Time.deltaTime;

        // Check if it's time to save the data
        if (timer >= samplingPeriod)
        {
            // Get the current run time and the data you want to save
            float currentTime = Time.time;
            string data = sensor.GetOutput();// your data value;

            // Write the data into the file
            writer.WriteLine(currentTime + "\t" + data);

            // Reset the timer
            timer = 0f;
        }
    }

    private void OnApplicationQuit()
    {
        // Close the StreamWriter when the application is quitting
        writer.Close();
    }
}
