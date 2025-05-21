#include <Arduino.h>
#include "WaterPump.h"

// put function declarations here:
int myFunction(int, int);

void setup() {
  // put your setup code here, to run once:
  WaterPumpSetup();
}

void loop() {
  // put your main code here, to run repeatedly:
  Serial.begin(9600);
  WaterPumpLoop();
}

// put function definitions here:
int myFunction(int x, int y) {
  return x + y;
}