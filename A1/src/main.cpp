#include <Arduino.h>
#include "network.h"
#include "powerplugapi.h"
#include "orderapi.h"
#include "WaterPump.h"

const char* ssid = "MAGS-OLC";
const char* password = "Merc1234!";
const char* apiKey = "";
const char* apiHost = "192.168.0.205";
const int apiPort = 8006;
const char* socketaddress = "192.168.1.151";
static unsigned long lastMenuOptionUpdate = 0;

Network network(ssid, password);
OrderApi orderApi(network, host, apiKey);
PowerPlugApi powerPlugApi(network, socketaddress);

void setup() {
  Serial.begin(9600);
  WaterPumpSetup();
}

void loop() {

  if (!network.isConnected()) {
    Serial.println("WiFi not connected, reconnecting...");
    network.connect();
  }

  WaterPumpLoop();

  if (temp > 50.0) {
    powerPlugApi.toggleOff();
  }
}