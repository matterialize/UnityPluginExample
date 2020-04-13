package com.example.nativecalculations;

import android.app.Activity;
import android.util.Log;

import com.google.gson.Gson;
import com.unity3d.player.UnityPlayer;

class CalculationResults
{
    double diagonal;
    int perimeter;
    int area;
}

interface NativeCalculationsPluginInterface {
    String syncCalculation(int rectangleHeight, int rectangleWidth);
    void asyncCalculation(int rectangleHeight, int rectangleWidth);
}

public class NativeCalculationsPlugin implements NativeCalculationsPluginInterface {
    private static final String TAG = "NativeCalculations";
    private static final String GAME_OBJECT_NAME = "PluginBridge";
    private static Gson gson = new Gson();

    private Activity activity;

    public NativeCalculationsPlugin(Activity activity) {
        this.activity = activity;
        Log.d(TAG, "Initialized NativeCalculationsPlugin class");
    }

    public String syncCalculation(int rectangleHeight, int rectangleWidth) {
        try {
            Log.d(TAG, "syncCalculation for rectangleHeight: " + rectangleHeight + " and rectangleWidth: " + rectangleWidth);
            return performCalculations(rectangleHeight, rectangleWidth);
        } catch (Exception exception) {
            UnityPlayer.UnitySendMessage(GAME_OBJECT_NAME, "HandleException", exception.toString());
            return "";
        }
    }

    public void asyncCalculation(int rectangleHeight, int rectangleWidth) {
        try {
            Log.d(TAG, "asyncCalculation for rectangleHeight: " + rectangleHeight + " and rectangleWidth: " + rectangleWidth);
            // Assuming these calculations results required async methods
            String calculationResults = performCalculations(rectangleHeight, rectangleWidth);
            UnityPlayer.UnitySendMessage(GAME_OBJECT_NAME, "HandleAsyncCalculation", calculationResults);
        } catch (Exception exception) {
            UnityPlayer.UnitySendMessage(GAME_OBJECT_NAME, "HandleException", exception.toString());
        }
    }

    private String performCalculations(int rectangleHeight, int rectangleWidth) {
        CalculationResults calculationResults = new CalculationResults();

        calculationResults.diagonal = Math.sqrt(Math.pow(rectangleHeight, 2) + Math.pow(rectangleWidth, 2));
        calculationResults.perimeter = 2 * rectangleHeight + 2 * rectangleWidth;
        calculationResults.area = rectangleHeight * rectangleWidth;

        return gson.toJson(calculationResults);
    }
}
