#include <Arduino.h>
#include "network.h"
#include "powerplugapi.h"
#include "testapi.h"
#include "WaterPump.h"

const char* ssid = "MAGS-OLC";
const char* password = "Merc1234!";
const char* apiKey = "";
const char* apiHost = "10.133.51.125";
const int apiPort = 8006;
const char* socketaddress = "192.168.1.151";
static unsigned long lastMenuOptionUpdate = 0;

Network network(ssid, password);
TestApi testApi(network, apiHost, apiPort, apiKey);
PowerPlugApi powerPlugApi(network, socketaddress);

void setup() {
  Serial.begin(9600);
  WaterPumpSetup();
}

void loop() {
  if (!network.isConnected() == false) {
    Serial.println("WiFi not connected, reconnecting...");
    network.connect();
  }

  if (temp > 50.0) {
    powerPlugApi.toggleOff();
  }

  testApi.getBusyOrder(nullptr);

  WaterPumpLoop();

}