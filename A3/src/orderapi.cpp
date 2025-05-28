#include "orderapi.h"
#include <Arduino.h>
#include <ArduinoJson.h>

OrderApi::OrderApi(Network& network, const char* host, const int apiPort)
    : _network(network), _host(host), _apiPort(apiPort) {}

bool OrderApi::checkApiConnection() {
    //DEBUG
    return true;


    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, _apiPort)) {
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

int OrderApi::getOrderList(Order* orders, int maxOrders) {
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, _apiPort)) {
        Serial.println("Connection to API failed");
        return 0;
    }

    // Send GET request
    client.println("GET /api/Orders/Unserved HTTP/1.1");
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
        int userId = obj.containsKey("user") && obj["user"].containsKey("id") ? obj["user"]["id"] : 0;
        int recipeId = obj.containsKey("recipe") && obj["recipe"].containsKey("id") ? obj["recipe"]["id"] : 0;
        String name = "Order";
        if (obj.containsKey("recipe") && obj["recipe"].containsKey("name")) {
            name = obj["recipe"]["name"].as<String>();
        }
        orders[count] = Order(id, name, userId, recipeId);
        count++;
    }
    return count;
}

int OrderApi::markOrderAsServed(int orderId) {
    WiFiClient& client = _network.getClient();
    String endpoint = String("/api/Orders/MarkAsServed/") + String(orderId);
    if (!client.connect(_host, _apiPort)) {
        Serial.println("Connection to API failed");
        return 0;
    }
    // Send PUT request with no body
    client.println(String("PUT ") + endpoint + " HTTP/1.1");
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

int OrderApi::editOrderSetServed(const Order& order) {
    WiFiClient& client = _network.getClient();
    const char* endpoint = "/api/Orders/Update";
    if (!client.connect(_host, _apiPort)) {
        Serial.println("Connection to API failed");
        return 0;
    }
    // Prepare JSON body
    String json = "{";
    json += "\"Id\":" + String(order.id) + ",";
    json += "\"UserId\":" + String(order.userId) + ",";
    json += "\"RecipeId\":" + String(order.recipeId) + ",";
    json += "\"HasBeenServed\":1"; // 1 = Served
    json += "}";

    client.println(String("PUT ") + endpoint + " HTTP/1.1");
    client.println(String("Host: ") + _host);
    client.println("Content-Type: application/json");
    client.println("Connection: close");
    client.print("Content-Length: ");
    client.println(json.length());
    client.println();
    client.println(json);

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
