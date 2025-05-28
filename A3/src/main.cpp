#include <Arduino.h>
#include "network.h"
#include "orderapi.h"
#include "powerplugapi.h"
#include "menu.h"

// WiFi credentials and API info
const char* ssid = "MAGS-OLC";
const char* password = "Merc1234!";
const char* apiKey = "";
const char* apiHost = "10.133.51.125";
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

bool isMachineBusy(Network& network, const char* host, int port) {
    WiFiClient& client = network.getClient();
    if (!client.connect(host, port)) {
        Serial.println("Connection to API failed (MachineBusy)");
        return false;
    }
    client.println("GET /api/Orders/MachineBusy HTTP/1.1");
    client.println(String("Host: ") + host);
    client.println("Connection: close");
    client.println();

    unsigned long timeout = millis();
    while (!client.available()) {
        if (millis() - timeout > 5000) {
            Serial.println(">>> Client Timeout ! (MachineBusy)");
            client.stop();
            return false;
        }
    }
    // Skip HTTP headers
    String line;
    while (client.available()) {
        line = client.readStringUntil('\n');
        if (line == "\r" || line.length() == 1) {
            break;
        }
    }
    // Read the body
    String body = "";
    while (client.available()) {
        char c = client.read();
        body += c;
    }
    client.stop();
    // Parse for "IsBusy": true
    return body.indexOf("IsBusy\":true") != -1;
}

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

  // Check if the machine is busy
  if (isMachineBusy(network, apiHost, apiPort)) {
    // Show busy screen
    carrier.display.fillScreen(carrier.display.color565(0, 0, 0));
    carrier.display.setCursor(0, 0);
    carrier.display.setTextColor(carrier.display.color565(255, 0, 0));
    carrier.display.setTextSize(2);
    carrier.display.println("Machine is busy!");
    carrier.display.setTextSize(1);
    carrier.display.setTextColor(carrier.display.color565(255, 255, 255));
    carrier.display.println("");
    carrier.display.println("Please wait for");
    carrier.display.println("the current order");
    carrier.display.println("to finish.");
    delay(1000);
    return;
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

        // Mark the order as served via the edit endpoint
        int response = orderApi.editOrderSetServed(orders[orderIdx]);
        Serial.print("EditOrderSetServed response code: ");
        Serial.println(response);

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
    // Clear out any remaining menu options if the new order count is less than before
    for (int i = orderCount; i < 5; ++i) {
        options[i] = MenuOption("", nullptr);
        orderMenuMap[i] = -1;
    }
    setMenuOptions(options, orderCount);
    Serial.println("Menu options updated.");
}
