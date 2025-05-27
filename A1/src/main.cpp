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

Network network(ssid, password);
TestApi testApi(network, apiHost, apiKey, apiPort);
PowerPlugApi powerPlugApi(network, socketaddress);

void setup() {
  Serial.begin(9600);

  while (network.isConnected() == false) {
        delay(500);
        Serial.print(".");
        network.connect();
    }
    Serial.println("WiFi connected");

  WaterPumpSetup();
}

void loop() {

  if (temp > 50.0) {
    powerPlugApi.toggleOff();
  }

  Order order;
  if (testApi.getBusyOrder(&order) == 1) {
    Serial.print("Got busy order: ");
    Serial.print(order.id);
    Serial.print(" - ");
    Serial.println(order.name);
  } else {
    Serial.println("No busy order found.");
  }

  WaterPumpLoop();

  testApi.markAsServed(&order);
}