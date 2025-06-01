#include <Arduino.h>
#include "Scales.h"
#include "ScalesCalibration.h"
#include "network.h"
#include "measurementapi.h"

// WiFi credentials and API info
const char* ssid = "MAGS-OLC";
const char* password = "Merc1234!";
const char* apiKey = "";
const char* host = "10.133.51.125";
const int apiPort = 8006;

Network network(ssid, password);
MeasurementApi measurementApi(network, host, apiKey, apiPort);

int waterId = -1;
int coffeeId = -1;
int waterIngredientId = -1;
int coffeeIngredientId = -1;

unsigned long lastApiCall = 0;
const unsigned long apiInterval = 10000; // 10 seconds;

int getWaterWeight();
int getCoffeeWeight();

void setup() {
    Serial.begin(9600);
    ScaleStart();
    //ScaleCalibrationStart();
    // Connect to WiFi
    while (network.isConnected() == false) {
        delay(500);
        Serial.print(".");
        network.connect();
    }
    Serial.println("WiFi connected");
}

void loop() {
    ScaleLoop();
    //ScaleCalibrationLoop();

    unsigned long now = millis();
    if (now - lastApiCall >= apiInterval) {
        lastApiCall = now;

        // 1. GET measurements
        char buffer[2048];
        int len = measurementApi.getMeasurements(buffer, sizeof(buffer));
        if (len > 0) {
            Serial.println("Measurements list:");
            Serial.println(buffer);

            // --- Use getIngredients to check for ingredients ---
            char ingrBuffer[1024];
            int ingrLen = measurementApi.getIngredients(ingrBuffer, sizeof(ingrBuffer));
            if (ingrLen > 0) {
                // Find or create Water ingredient
                waterIngredientId = measurementApi.findIngredientId(ingrBuffer, "Water");
                if (waterIngredientId == -1) {
                    Serial.println("Water ingredient not found, creating...");
                    waterIngredientId = measurementApi.createIngredient("Water");
                    Serial.print("Created Water ingredient ID: "); Serial.println(waterIngredientId);
                    delay(500);
                    measurementApi.getIngredients(ingrBuffer, sizeof(ingrBuffer));
                    waterIngredientId = measurementApi.findIngredientId(ingrBuffer, "Water");
                }
                // Always create a new Water measurement
                if (waterIngredientId != -1) {
                    int measId = measurementApi.createMeasurement(waterIngredientId, getWaterWeight());
                    Serial.print("Created Water measurement ID: "); Serial.println(measId);
                }

                // Find or create Coffee ingredient
                coffeeIngredientId = measurementApi.findIngredientId(ingrBuffer, "Instant Coffee");
                if (coffeeIngredientId == -1) {
                    Serial.println("Coffee ingredient not found, creating...");
                    coffeeIngredientId = measurementApi.createIngredient("Instant Coffee");
                    Serial.print("Created Coffee ingredient ID: "); Serial.println(coffeeIngredientId);
                    delay(500);
                    measurementApi.getIngredients(ingrBuffer, sizeof(ingrBuffer));
                    coffeeIngredientId = measurementApi.findIngredientId(ingrBuffer, "Instant Coffee");
                }
                // Always create a new Coffee measurement
                if (coffeeIngredientId != -1) {
                    int measId = measurementApi.createMeasurement(coffeeIngredientId, getCoffeeWeight());
                    Serial.print("Created Coffee measurement ID: "); Serial.println(measId);
                }
            }
        }
    }
}
int getWaterWeight() {
    return static_cast<int>(GetScale1Weight());
}
int getCoffeeWeight() {
    return static_cast<int>(GetScale2Weight());
}
