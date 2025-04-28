#include <Arduino.h>

const int buttonPin = 2;
bool buttonState = false;

void setup() {
  pinMode(buttonPin, INPUT_PULLUP);
  Serial.begin(9600);
}

void loop() {
  int reading = digitalRead(buttonPin);
  if (reading == LOW) {
    Serial.println("Pressed");
    delay(500);
  }
}