#include "WaterPump.h"

DS18B20 ds(A1);
float temp;

int motor1pin1 = 2;
int motor1pin2 = 3;

unsigned long waterPumpStartTime = 0;
unsigned long waterPumpStopTime = 0;

bool waterPumpRunning = false;

const unsigned long waterPumpRunDuration = 20000;

void WaterPumpSetup() {
  pinMode(motor1pin1, OUTPUT);
  pinMode(motor1pin2, OUTPUT);
}

void WaterPumpLoop() {
  unsigned long currentMillis = millis();
  temp = ds.getTempC();
  Serial.print("Temperature: ");
  Serial.println(temp);

  if (temp > 50.0) {
    if (!waterPumpRunning) {
      digitalWrite(motor1pin1, HIGH);
      waterPumpStartTime = currentMillis;
      waterPumpRunning = true;
    } else {
      if (currentMillis - waterPumpStartTime >= waterPumpRunDuration) {
        digitalWrite(motor1pin1, LOW);
        waterPumpRunning = false;
      }
    }
  } else {
    digitalWrite(motor1pin1, LOW);
    waterPumpRunning = false;
  }
}
