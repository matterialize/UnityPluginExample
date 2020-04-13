using System;
using Plugins.NativeCalculations;
using UnityEngine;

public class CallingExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Debug.Log("CallingExample - started");

            var rectangleHeight = 3;
            var rectangleWidth = 4;

            var syncResults = NativeCalculations.PerformSyncCalculation(rectangleHeight, rectangleWidth);
            Debug.Log($"Sync results: diagonal = {syncResults.diagonal}, perimeter = {syncResults.perimeter}, area = {syncResults.area}");

            NativeCalculations.PerformAsyncCalculation(rectangleHeight, rectangleWidth, (asyncResults) =>
            {
                Debug.Log($"Async results: diagonal = {asyncResults.diagonal}, perimeter = {asyncResults.perimeter}, area = {asyncResults.area}");
            });
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
