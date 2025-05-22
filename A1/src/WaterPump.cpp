#include "WaterPump.h"

int motor1pin1 = 2;
int motor1pin2 = 3;

void WaterPumpSetup() {
  pinMode(motor1pin1, OUTPUT);
  pinMode(motor1pin2, OUTPUT);
}

void WaterPumpLoop() {
  temp = ds.getTempC();
  Serial.print("Temperature: ");
  Serial.println(temp);

  if (temp > 30.0) {
    digitalWrite(motor1pin1,   HIGH);
  } else {
    digitalWrite(motor1pin1, LOW);
  }
}
