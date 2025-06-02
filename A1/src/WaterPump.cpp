#include "WaterPump.h"

DS18B20 ds(A1);
float temp;
bool isDone = false;

int motor1pin1 = 2;
int motor1pin2 = 3;

unsigned long waterPumpStartTime = 0;
unsigned long waterPumpStopTime = 0;

bool waterPumpRunning = false;

const unsigned long waterPumpRunDuration = 360000;

void WaterPumpSetup() {
  pinMode(motor1pin1, OUTPUT);
  pinMode(motor1pin2, OUTPUT);
}

void TemperatureRead() {
  temp = ds.getTempC();
  Serial.print("Temperature: ");
  Serial.println(temp);
}

void WaterPumpLoop() {
  unsigned long currentMillis = millis();

  if (temp > 70.0) {
    // If the temperature is above 80 degrees Celsius, start the water pump.
    if (!waterPumpRunning) {
      digitalWrite(motor1pin1, HIGH);
      waterPumpStartTime = currentMillis;
      waterPumpRunning = true;
      isDone = false; // Reset isDone only when starting a new cycle
    } else { // If the water pump is already running, check if it should stop.
      if (currentMillis - waterPumpStartTime >= waterPumpRunDuration) {
        digitalWrite(motor1pin1, LOW);
        waterPumpRunning = false;
        isDone = true; // Set isDone only after the full duration
      }
    }
  } else {
    // If the temperature is below or equal to 80 degrees Celsius, stop the water pump.
    digitalWrite(motor1pin1, LOW);
    waterPumpRunning = false;
  }
}
