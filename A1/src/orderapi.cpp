#include "orderapi.h"
#include <Arduino.h>

OrderApi::OrderApi(Network& network, const char* host, const char* apiKey)
    : _network(network), _host(host), _apiKey(apiKey) {}

bool OrderApi::checkApiConnection() {
    //DEBUG
    return true;


    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, 80)) {
        Serial.println("Connection to API failed");
        return false;
    }
    return true;
}

int OrderApi::sendDataToApi(const char* endpoint, const char* data) {
    WiFiClient& client = _network.getClient();
    int responseCode = 0;
    if (client.connected()) {
        client.println(String("POST ") + endpoint + " HTTP/1.1");
        client.println(String("Host: ") + _host);
        client.println("Content-Type: application/json");
        client.println("Connection: close");
        client.print("Content-Length: ");
        client.println(strlen(data));
        client.println();
        client.println(data);
        delay(1000);
        while (client.available()) {
            String line = client.readStringUntil('\n');
            Serial.println(line);
            if (line.startsWith("HTTP/1.1")) {
                responseCode = line.substring(9, 12).toInt();
                Serial.print("Response code: ");
                Serial.println(responseCode);
            }
        }
    } else {
        Serial.println("Client not connected");
    }
    return responseCode;
}

// Helper: Dummy parse for now, just create some fake orders
int OrderApi::getOrderList(Order* orders, int maxOrders) {
    // Simulate reading 2 orders
    if (maxOrders < 2) return 0;
    orders[0] = Order(1, "Coffee");
    orders[1] = Order(2, "Latte");
    return 2;
}
