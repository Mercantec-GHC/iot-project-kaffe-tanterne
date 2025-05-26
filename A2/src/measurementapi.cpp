#include "measurementapi.h"
#include <Arduino.h>
#include <ArduinoJson.h>

// Constructor
MeasurementApi::MeasurementApi(Network& network, const char* host, const char* apiKey)
    : _network(network), _host(host), _apiKey(apiKey) {}

// Helper to skip HTTP headers and read response body
static int readHttpBody(WiFiClient& client, char* buffer, int bufferSize) {
    bool headersEnded = false;
    int len = 0;
    while (client.connected() && len < bufferSize - 1) {
        if (!client.available()) delay(1);
        char c = client.read();
        if (!headersEnded) {
            static int crlfCount = 0;
            if (c == '\r' || c == '\n') ++crlfCount;
            else crlfCount = 0;
            if (crlfCount >= 2) headersEnded = true;
        } else {
            buffer[len++] = c;
        }
    }
    buffer[len] = '\0';
    return len;
}

int MeasurementApi::getMeasurements(char* buffer, int bufferSize) {
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, 80)) {
        Serial.println("GET: Connection to API failed");
        return -1;
    }
    client.println("GET /api/Measurements/Index HTTP/1.1");
    client.print("Host: "); client.println(_host);
    client.println("Connection: close");
    client.println();

    int len = readHttpBody(client, buffer, bufferSize);
    client.stop();
    return len;
}

int MeasurementApi::updateMeasurement(int id, int value) {
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, 80)) {
        Serial.println("PUT: Connection to API failed");
        return -1;
    }
    String json = "{\"id\":" + String(id) + ",\"value\":" + String(value) + "}";
    client.println("PUT /api/Measurements/Edit HTTP/1.1");
    client.print("Host: "); client.println(_host);
    client.println("Content-Type: application/json");
    client.print("Content-Length: "); client.println(json.length());
    client.println("Connection: close");
    client.println();
    client.println(json);

    int code = -1;
    while (client.connected()) {
        String line = client.readStringUntil('\n');
        if (line.startsWith("HTTP/1.1")) {
            code = line.substring(9, 12).toInt();
            break;
        }
    }
    client.stop();
    return code;
}

int MeasurementApi::createMeasurement(const char* ingredientName, int value) {
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, 80)) {
        Serial.println("POST: Connection to API failed");
        return -1;
    }
    String json = "{\"ingredient\":{\"name\":\"" + String(ingredientName) + "\"},\"value\":" + String(value) + "}";
    client.println("POST /api/Measurements/Create HTTP/1.1");
    client.print("Host: "); client.println(_host);
    client.println("Content-Type: application/json");
    client.print("Content-Length: "); client.println(json.length());
    client.println("Connection: close");
    client.println();
    client.println(json);

    // You can parse the response for the new ID if your API returns it
    client.stop();
    return -1; // Not implemented
}

int MeasurementApi::findMeasurementId(const char* buffer, const char* ingredientName) {
    StaticJsonDocument<2048> doc;
    DeserializationError error = deserializeJson(doc, buffer);
    if (error) {
        Serial.print("JSON parse failed: ");
        Serial.println(error.c_str());
        return -1;
    }

    // Handle root array or object with array property
    JsonArray arr;
    if (doc.is<JsonArray>()) {
        arr = doc.as<JsonArray>();
    } else if (doc.is<JsonObject>()) {
        for (JsonPair kv : doc.as<JsonObject>()) {
            if (kv.value().is<JsonArray>()) {
                arr = kv.value().as<JsonArray>();
                break;
            }
        }
    }
    if (!arr.isNull()) {
        for (JsonArray::iterator it = arr.begin(); it != arr.end(); ++it) {
            JsonObject meas = (*it).as<JsonObject>();
            if (meas.containsKey("ingredient")) {
                JsonObject ingr = meas["ingredient"];
                if (ingr.containsKey("name") && strcmp(ingr["name"].as<const char*>(), ingredientName) == 0) {
                    return meas["id"];
                }
            }
        }
    }
    return -1;
}