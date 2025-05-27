#include "testapi.h"
#include <Arduino.h>
#include <ArduinoJson.h>

TestApi::TestApi(Network &network, const char *host, int apiPort, const char *apiKey)
    : _network(network), _host(host), _apiPort(apiPort) {}

bool TestApi::checkApiConnection()
{
    //DEBUG
    return true;


    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, _apiPort)) {
        Serial.println("Connection to API failed");
        return false;
    }
    return true;
}

int TestApi::sendDataToApi(const char* endpoint, const char* data) {
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

int TestApi::getOrderList(Order* orders, int maxOrders) {
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, _apiPort)) {
        Serial.println("Connection to API failed");
        return 0;
    }

    // Send GET request
    client.println("GET /api/Orders/Index HTTP/1.1");
    client.println(String("Host: ") + _host);
    client.println("Connection: close");
    client.println();

    // Wait for response
    unsigned long timeout = millis();
    while (!client.available()) {
        if (millis() - timeout > 5000) {
            Serial.println(">>> Client Timeout !");
            client.stop();
            return 0;
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

    // Fix: Strip any characters before the first '['
    int jsonStart = body.indexOf('[');
    if (jsonStart > 0) {
        body = body.substring(jsonStart);
    }

    // Parse JSON
    const size_t capacity = 2048;
    DynamicJsonDocument doc(capacity);
    DeserializationError error = deserializeJson(doc, body);
    if (error) {
        Serial.print("deserializeJson() failed: ");
        Serial.println(error.c_str());
        return 0;
    }
    if (!doc.is<JsonArray>()) {
        Serial.println("Expected a JSON array, printing body for debug:");
        Serial.println(body);
        return 0;
    }
    JsonArray arr = doc.as<JsonArray>();
    int count = 0;
    for (JsonObject obj : arr) {
        if (count >= maxOrders) break;
        int id = obj["id"] | 0;
        String name = "Order";
        if (obj.containsKey("recipe") && obj["recipe"].containsKey("name")) {
            name = obj["recipe"]["name"].as<String>();
        }
        orders[count] = Order(id, name);
        count++;
    }
    return count;
}

int TestApi::getBusyOrder(Order* order) {
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, _apiPort)) {
        Serial.println("Connection to API failed");
        return 0;
    }

    // Send GET request
    client.println("GET /api/Orders/FirstOrderIfBusy HTTP/1.1");
    client.println(String("Host: ") + _host);
    client.println("Connection: close");
    client.println();

    // Wait for response
    unsigned long timeout = millis();
    while (!client.available()) {
        if (millis() - timeout > 5000) {
            Serial.println(">>> Client Timeout !");
            client.stop();
            return 0;
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

    // Parse JSON
    const size_t capacity = 512;
    DynamicJsonDocument doc(capacity);
    DeserializationError error = deserializeJson(doc, body);
    if (error) {
        Serial.print("deserializeJson() failed: ");
        Serial.println(error.c_str());
        return 0;
    }
    if (!doc.is<JsonObject>()) {
        Serial.println("Expected a JSON object, printing body for debug:");
        Serial.println(body);
        return 0;
    }
    int id = doc["id"] | 0;
    String name = "Order";
    if (doc.containsKey("recipe") && doc["recipe"].containsKey("name")) {
        name = doc["recipe"]["name"].as<String>();
    }
    *order = Order(id, name);
    return 1;
}

int TestApi::markAsServed(Order* order) {
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, _apiPort)) {
        Serial.println("Connection to API failed");
        return 0;
    }

    // Send PUT request with no body
    client.println("PUT /api/Orders/MarkAsServed HTTP/1.1");
    client.println(String("Host: ") + _host);
    client.println("Connection: close");
    client.println();

    // Wait for response
    unsigned long timeout = millis();
    while (!client.available()) {
        if (millis() - timeout > 5000) {
            Serial.println(">>> Client Timeout !");
            client.stop();
            return 0;
        }
    }

    // Read response code
    int responseCode = 0;
    while (client.available()) {
        String line = client.readStringUntil('\n');
        if (line.startsWith("HTTP/1.1")) {
            responseCode = line.substring(9, 12).toInt();
        }
    }
    client.stop();
    return responseCode;
}
