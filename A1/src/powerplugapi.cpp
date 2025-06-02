#include "powerplugapi.h"
#include <Arduino.h>

PowerPlugApi::PowerPlugApi(Network& network, const char* socketAddress)
    : _network(network), _socketAddress(socketAddress) {}

bool PowerPlugApi::toggleOn() {
    WiFiClient& client = _network.getClient();
    Serial.print("[DEBUG] Connecting to: ");
    Serial.print(_socketAddress);
    Serial.println(":80");
    if (!client.connect(_socketAddress, 80)) {
        Serial.println("Failed to connect to power plug");
        return false;
    }
    Serial.println("[DEBUG] Connected!");

    // Prepare the body and correct content length
    String body = "{\"id\": 0, \"on\": true}";
    int contentLength = body.length();

    client.println("POST /rpc/Switch.Set HTTP/1.1");
    client.println(String("Host: ") + _socketAddress);
    client.println("Content-Type: application/json");
    client.println("User-Agent: Arduino/1.0");
    client.println("Connection: close");
    client.print("Content-Length: ");
    client.println(contentLength);
    client.println();
    client.print(body);

    // Wait for response and print it for debugging
    unsigned long timeout = millis();
    while (client.available() == 0) {
        if (millis() - timeout > 5000) {
            Serial.println("[DEBUG] Client Timeout!");
            client.stop();
            return false;
        }
    }
    Serial.println("[DEBUG] Response from power plug:");
    while (client.available()) {
        String line = client.readStringUntil('\n');
        Serial.println(line);
    }
    client.stop();
    Serial.println("Toggled power plug");
    return true;
}

bool PowerPlugApi::toggleOff() {
    WiFiClient& client = _network.getClient();
    Serial.print("[DEBUG] Connecting to: ");
    Serial.print(_socketAddress);
    Serial.println(":80");
    if (!client.connect(_socketAddress, 80)) {
        Serial.println("Failed to connect to power plug");
        return false;
    }
    Serial.println("[DEBUG] Connected!");

    // Prepare the body and correct content length
    String body = "{\"id\": 0, \"on\": false}";
    int contentLength = body.length();

    client.println("POST /rpc/Switch.Set HTTP/1.1");
    client.println(String("Host: ") + _socketAddress);
    client.println("Content-Type: application/json");
    client.println("User-Agent: Arduino/1.0");
    client.println("Connection: close");
    client.print("Content-Length: ");
    client.println(contentLength);
    client.println();
    client.print(body);

    // Wait for response and print it for debugging
    unsigned long timeout = millis();
    while (client.available() == 0) {
        if (millis() - timeout > 5000) {
            Serial.println("[DEBUG] Client Timeout!");
            client.stop();
            return false;
        }
    }
    Serial.println("[DEBUG] Response from power plug:");
    while (client.available()) {
        String line = client.readStringUntil('\n');
        Serial.println(line);
    }
    client.stop();
    Serial.println("Toggled power plug");
    return true;
}
