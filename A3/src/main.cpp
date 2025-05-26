#include <Arduino.h>
#include "network.h"
#include "orderapi.h"
#include "powerplugapi.h"
#include "menu.h"

// WiFi credentials and API info
const char* ssid = "WiFimodem-C1F9";
const char* password = "bonkbonkbonk";
const char* apiKey = "";
const char* apiHost = "192.168.0.205";
const int apiPort = 8006;
const char* socketaddress = "192.168.1.151";
static unsigned long lastMenuOptionUpdate = 0;

MKRIoTCarrier carrier;
Network network(ssid, password);
OrderApi orderApi(network, apiHost, apiPort);
PowerPlugApi powerPlugApi(network, socketaddress);

// Use fixed-size arrays for orders and options
Order orders[5];
MenuOption options[5];
// Map menu index to order index
int orderMenuMap[5];

// Function to handle menu selection
void handleMenuSelection(int index);
// Function to update the menu options
void updateMenuOptions();

void setup() {
  Serial.begin(9600);
  carrier.noCase();
  carrier.begin();
  menuInit(carrier);
  network.connect();
  setMenuOptions(options, sizeof(options)/sizeof(options[0]));
  setMenuSelectCallback(handleMenuSelection);
}

void loop() {
  // Check if the WiFi is connected
  if (!network.isConnected()) {
    Serial.println("WiFi not connected, reconnecting...");
    network.connect();
  }

  // Check if the API is reachable
  if (!orderApi.checkApiConnection()) {
    Serial.println("API not reachable, reconnecting...");
    orderApi.checkApiConnection();
  }

  // Every 10 seconds, update the order list
  if (millis() - lastMenuOptionUpdate > 10000) {
    lastMenuOptionUpdate = millis();
    updateMenuOptions();
  }

  menuUpdate(carrier);
  delay(10);
}

// Function to handle menu selection
void handleMenuSelection(int index) {
    // Use orderMenuMap to get the correct order
    int orderIdx = orderMenuMap[index];
    if (orderIdx >= 0 && orderIdx < 5) {
        Serial.print("Selected order: ");
        Serial.print(orders[orderIdx].id);
        Serial.print(" - ");
        Serial.println(orders[orderIdx].name);
        // Add your logic here to use the selected order
        powerPlugApi.toggleOn();
    }
}

// Update the menu options
void updateMenuOptions() {
  Serial.println("Updating menu options...");

    // Get the order list from the API
    int orderCount = orderApi.getOrderList(orders, 5);
    Serial.print("Order count: ");
    Serial.println(orderCount);
    // For each order, create a menu option
    for (int i = 0; i < orderCount; ++i) {
        options[i] = MenuOption(orders[i].name, nullptr); // Do not store pointer to stack/global Order
        orderMenuMap[i] = i; // Map menu index to order index
        Serial.print("Order: ");
        Serial.println(orders[i].name);
    }
    setMenuOptions(options, orderCount);
    Serial.println("Menu options updated.");
}
