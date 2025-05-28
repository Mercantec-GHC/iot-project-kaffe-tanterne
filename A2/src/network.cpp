#include "network.h"
#include <Arduino.h>

Network::Network(const char* ssid, const char* password)
    : _ssid(ssid), _password(password) {}

void Network::connect() {
    Serial.print("Connecting to ");
    Serial.println(_ssid);
    while (WiFi.status() != WL_CONNECTED) {
        WiFi.begin(_ssid, _password);
        delay(10000);
    }
    Serial.println("Connected to WiFi");
}

bool Network::isConnected() {
    return WiFi.status() == WL_CONNECTED;
}

WiFiClient& Network::getClient() {
    return _client;
}
