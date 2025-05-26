#include <Arduino.h>
#include "Scales.h"
#include "ScalesCalibration.h"
#include "network.h"
#include "measurementapi.h"

// WiFi credentials and API info
const char* ssid = "MAGS-OLC";
const char* password = "Merc1234!";
const char* apiKey = "your-API";
const char* host = "api.example.com";

Network network(ssid, password);
MeasurementApi measurementApi(network, host, apiKey);

int waterId = -1;
int coffeeId = -1;

unsigned long lastApiCall = 0;
const unsigned long apiInterval = 10000; // 10 seconds

void setup() {
    Serial.begin(9600);
    ScaleStart();
    //ScaleCalibrationStart();
    // Connect to WiFi
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
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

            // 2. Find or create water measurement
            waterId = measurementApi.findMeasurementId(buffer, "Water");
            if (waterId == -1) {
                Serial.println("Water measurement not found, creating...");
                measurementApi.createMeasurement("Water", getWaterWeight());
                // Re-fetch to get the new ID
                delay(1000);
                measurementApi.getMeasurements(buffer, sizeof(buffer));
                waterId = measurementApi.findMeasurementId(buffer, "Water");
            }

            // 3. Find or create coffee measurement
            coffeeId = measurementApi.findMeasurementId(buffer, "Coffee");
            if (coffeeId == -1) {
                Serial.println("Coffee measurement not found, creating...");
                measurementApi.createMeasurement("Coffee", getCoffeeWeight());
                // Re-fetch to get the new ID
                delay(1000);
                measurementApi.getMeasurements(buffer, sizeof(buffer));
                coffeeId = measurementApi.findMeasurementId(buffer, "Coffee");
            }

            // 4. PUT updates for water and coffee
            if (waterId != -1) {
                int resp1 = measurementApi.updateMeasurement(waterId, getWaterWeight());
                Serial.print("PUT water response: "); Serial.println(resp1);
            }
            if (coffeeId != -1) {
                int resp2 = measurementApi.updateMeasurement(coffeeId, getCoffeeWeight());
                Serial.print("PUT coffee response: "); Serial.println(resp2);
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
