#include <Arduino_MKRIoTCarrier.h>
#include "api.h"

MKRIoTCarrier carrier;

bool buttonPressed = false;


void setup() {
  Serial.begin(9600);
  carrier.noCase();
  carrier.begin();

  
  carrier.display.init(64, 64);
}

void loop() {
  // Check and reconnect to WiFi and API
  if (!checkAndReconnect()) {
    Serial.println("Failed to reconnect to WiFi or API");
    delay(10000); // Wait before retrying
    return;
  }

  carrier.display.fillScreen(carrier.display.color565(0, 0, 0));
  carrier.display.setCursor(16, 32);
  
  if (buttonPressed) {
    carrier.display.print("Button pressed");
  } else
  {
    carrier.display.print("None pressed");
  }

  carrier.Buttons.update();
  if (carrier.Buttons.getTouch(TOUCH4) && !buttonPressed) {
    buttonPressed = true;
    Serial.println("Button 4 pressed");
  } else if (!carrier.Buttons.getTouch(TOUCH4) && buttonPressed) {
    buttonPressed = false;
    Serial.println("Button released");
  }
  delay(10); // Wait before retrying
}