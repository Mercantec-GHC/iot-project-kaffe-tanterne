#include <Arduino.h>
#include "network.h"
#include "powerplugapi.h"
#include "testapi.h"
#include "WaterPump.h"
#include "CoffeeDispenser.h"

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
  
  CoffeeDispenserSetup();

  WaterPumpSetup();
}

void loop() {


  if (temp > 30.0) {
    powerPlugApi.toggleOff();
  }

  CoffeeDispenserLoop();

  Order order;
  // Find the first order that is Handling (not Served yet)
  if (testApi.getBusyOrder(&order) == 1) {
    Serial.print("Got handling order: ");
    Serial.print(order.id);
    Serial.print(" - ");
    Serial.println(order.name);
  } else {
    Serial.println("No handling order found.");
  }

  WaterPumpLoop();

  // Mark this order as served (if found)
  if (order.id != 0) {
    int resp = testApi.markAsServed(&order);
    Serial.print("Mark as served response: ");
    Serial.println(resp);
  }
}