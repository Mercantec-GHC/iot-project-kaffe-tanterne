#include "CoffeeDispenser.h"
#include <Arduino.h>

int motor2pin1 = 4;
int motor2pin2 = 5;

unsigned long motorStartTime = 0;
unsigned long motorStopTime = 0;
bool motorRunning = false;

const unsigned long motorRunDuration = 200;
const unsigned long motorPauseDuration = 500;

void CoffeeDispenserSetup() {
  pinMode(motor2pin1, OUTPUT);
  pinMode(motor2pin2, OUTPUT);
}

void CoffeeDispenserLoop() {
    unsigned long currentMillis = millis();
    if (!motorRunning) {
        digitalWrite(motor2pin1, HIGH);
        digitalWrite(motor2pin2, LOW);
        motorStartTime = currentMillis;
        motorRunning = true;
    } else {
        if (currentMillis - motorStartTime >= motorRunDuration) {
            digitalWrite(motor2pin1, LOW);
            digitalWrite(motor2pin2, LOW);
            motorStopTime = currentMillis;
            motorRunning = false;
        }
    }
    if (!motorRunning && (currentMillis - motorStopTime >= motorPauseDuration)) {
        motorRunning = false;
    }
}
