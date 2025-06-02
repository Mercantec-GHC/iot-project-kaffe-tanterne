#include <Arduino.h>
#include "network.h"
#include "powerplugapi.h"
#include "serveapi.h"
#include "WaterPump.h"
#include "CoffeeDispenser.h"

const char* ssid = "MAGS-OLC";
const char* password = "Merc1234!";
const char* apiKey = "";
const char* apiHost = "10.133.51.125";
const int apiPort = 8006;
const char* socketaddress = "192.168.1.151";

Network network(ssid, password);
ServeApi serveApi(network, apiHost, apiKey, apiPort);
PowerPlugApi powerPlugApi(network, socketaddress);

void setup() {
  Serial.begin(9600);

  while (network.isConnected() == false) {
        delay(500);
        Serial.print(".");
        network.connect();
    }
  Serial.println("WiFi connected");
  
  //CoffeeDispenserSetup();

  WaterPumpSetup();
}

void loop() {

  TemperatureRead();

  if (temp > 90.0) {
    powerPlugApi.toggleOff();
  }
    

  //CoffeeDispenserLoop();

  
  Order order;
  // Find the first order that is Handling (not Served yet)
  if (serveApi.getBusyOrder(&order) == 1) {
    Serial.print("Got handling order: ");
    Serial.print(order.id);
    Serial.print(" - ");
    Serial.println(order.name);
    WaterPumpLoop();
  } else {
    Serial.println(" ");
  }

  // Mark this order as served (if found)
  if (isDone == true) {
    if (order.id != 0) {
      int resp = serveApi.markAsServed(&order);
      Serial.print("Mark as served response: ");
      Serial.println(resp);
    }
  }
}