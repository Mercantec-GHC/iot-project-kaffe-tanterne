#include "measurementapi.h"
#include <Arduino.h>
#include <ArduinoJson.h>

//Constructor
MeasurementApi::MeasurementApi(Network& network, const char* host, const char* apiKey, const int apiPort)
    : _network(network), _host(host), _apiKey(apiKey), _apiPort(apiPort) {}

//Helper to skip HTTP headers and read response body (robust, non-blocking, safe for Arduino)
static int readHttpBody(WiFiClient& client, char* buffer, int bufferSize) {
    unsigned long startTime = millis();
    while (!client.available()) {
        if (millis() - startTime > 5000) {
            Serial.println(">>> Client Timeout waiting for response!");
            client.stop();
            buffer[0] = '\0';
            return 0;
        }
        delay(1);
    }

    //Skip HTTP headers
    String line;
    while (client.connected() && client.available()) {
        line = client.readStringUntil('\n');
        if (line == "\r" || line.length() == 1) {
            break;
        }
        //Safety: timeout check
        if (millis() - startTime > 5000) {
            Serial.println(">>> Timeout while reading headers!");
            client.stop();
            buffer[0] = '\0';
            return 0;
        }
    }

    //Read the body directly into the buffer
    int idx = 0;
    bool foundJsonStart = false;
    while (client.connected() && (millis() - startTime < 7000)) {
        while (client.available()) {
            char c = client.read();
            // Find the start of JSON ('{' or '[')
            if (!foundJsonStart) {
                if (c == '{' || c == '[') {
                    foundJsonStart = true;
                } else {
                    continue;
                }
            }
            if (idx < bufferSize - 1) {
                buffer[idx++] = c;
            } else {
                break;
            }
        }
        if (!client.available()) delay(1);
        if (idx >= bufferSize - 1) break;
    }
    buffer[idx] = '\0';
    if (!foundJsonStart) {
        buffer[0] = '\0';
        return 0;
    }
    return idx;
}

int MeasurementApi::getMeasurements(char* buffer, int bufferSize) {
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, _apiPort)) {
        Serial.println("GET: Connection to API failed");
        return -1;
    }
    client.println("GET /api/Measurements/Index HTTP/1.1");
    client.print("Host: "); client.println(_host);
    client.println("Connection: close");
    client.println();

    Serial.println("GET Measurements request sent");
    int len = readHttpBody(client, buffer, bufferSize); 

    Serial.print("GET Measurements Response: "); Serial.println(buffer);
    client.stop();
    return len;
}

//GET all ingredients
int MeasurementApi::getIngredients(char* buffer, int bufferSize) {
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, _apiPort)) {
        Serial.println("GET: Connection to API (ingredients) failed");
        return -1;
    }
    client.println("GET /api/Ingredients/Index HTTP/1.1");
    client.print("Host: "); client.println(_host);
    client.println("Connection: close");
    client.println();

    int len = readHttpBody(client, buffer, bufferSize);
    client.stop();
    return len;
}

//Find ingredientId by name from ingredients buffer
int MeasurementApi::findIngredientId(const char* buffer, const char* ingredientName) {
    StaticJsonDocument<2048> doc;
    DeserializationError error = deserializeJson(doc, buffer);
    if (error) return -1;
    JsonArray arr;
    if (doc.is<JsonArray>()) arr = doc.as<JsonArray>();
    else if (doc.is<JsonObject>()) {
        for (JsonPair kv : doc.as<JsonObject>()) {
            if (kv.value().is<JsonArray>()) {
                arr = kv.value().as<JsonArray>();
                break;
            }
        }
    }
    if (!arr.isNull()) {
        for (JsonArray::iterator it = arr.begin(); it != arr.end(); ++it) {
            JsonObject ingr = (*it).as<JsonObject>();
            if (ingr.containsKey("name") && strcmp(ingr["name"].as<const char*>(), ingredientName) == 0) {
                return ingr["id"];
            }
        }
    }
    return -1;
}

//Create a new measurement
int MeasurementApi::createMeasurement(int ingredientId, int value) {
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, _apiPort)) {
        Serial.println("POST: Connection to API failed");
        return -1;
    }
    String json = "{\"ingredientId\":" + String(ingredientId) + ",\"value\":" + String(value) + "}";
    Serial.print("POST Measurement JSON: "); Serial.println(json);
    client.println("POST /api/Measurements/Create HTTP/1.1");
    client.print("Host: "); client.println(_host);
    client.println("Content-Type: application/json");
    client.print("Content-Length: "); client.println(json.length());
    client.println("Connection: close");
    client.println();
    client.println(json);

    char buffer[512];
    int len = readHttpBody(client, buffer, sizeof(buffer));
    Serial.print("POST Measurement Response: "); Serial.println(buffer);
    client.stop();

    StaticJsonDocument<512> doc;
    DeserializationError error = deserializeJson(doc, buffer);
    if (!error && doc.is<JsonObject>() && doc.containsKey("id")) {
        return doc["id"];
    }
    return -1;
}

//Update an existing measurement (this is no longer in use, but kept for reference)
int MeasurementApi::updateMeasurement(int id, int ingredientId, int value) {
    if (id == -1 || ingredientId == -1) {
        Serial.println("updateMeasurement: Invalid ID");
        return 400;
    }
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, _apiPort)) {
        Serial.println("PUT: Connection to API failed");
        return -1;
    }
    String json = "{\"id\":" + String(id) + ",\"ingredientId\":" + String(ingredientId) + ",\"value\":" + String(value) + "}";
    Serial.print("PUT JSON: "); Serial.println(json);
    client.println("PUT /api/Measurements/Edit HTTP/1.1");
    client.print("Host: "); client.println(_host);
    client.println("Content-Type: application/json");
    client.print("Content-Length: "); client.println(json.length());
    client.println("Connection: close");
    client.println();
    client.println(json);

    int code = -1;
    String response = "";
    bool headerEnded = false;
    while (client.connected()) {
        String line = client.readStringUntil('\n');
        response += line + "\n";
        if (line.startsWith("HTTP/1.1")) {
            code = line.substring(9, 12).toInt();
        }
        if (!headerEnded && (line == "\r" || line.length() == 1)) {
            headerEnded = true;
            break;
        }
    }
    while (client.available()) {
        response += client.readString();
    }
    Serial.print("PUT response: "); Serial.println(response);
    client.stop();
    return code;
}

//Create a new ingredient
int MeasurementApi::createIngredient(const char* ingredientName) {
    WiFiClient& client = _network.getClient();
    if (!client.connect(_host, _apiPort)) {
        Serial.println("POST: Connection to API failed (ingredient)");
        return -1;
    }
    String json = "{\"name\":\"" + String(ingredientName) + "\"}";
    Serial.print("POST Ingredient JSON: "); Serial.println(json);
    client.println("POST /api/Ingredients/Create HTTP/1.1");
    client.print("Host: "); client.println(_host);
    client.println("Content-Type: application/json");
    client.print("Content-Length: "); client.println(json.length());
    client.println("Connection: close");
    client.println();
    client.println(json);

    char buffer[256];
    int len = readHttpBody(client, buffer, sizeof(buffer));
    Serial.print("POST Ingredient Response: "); Serial.println(buffer);
    client.stop();

    // Try to parse the new ingredient ID from the response
    StaticJsonDocument<256> doc;
    DeserializationError error = deserializeJson(doc, buffer);
    if (!error && doc.is<JsonObject>() && doc.containsKey("id")) {
        return doc["id"];
    }
    return -1;
}

//Find measurement ID by ingredient name from measurements buffer
int MeasurementApi::findMeasurementId(const char* buffer, const char* ingredientName) {
    StaticJsonDocument<2048> doc;
    DeserializationError error = deserializeJson(doc, buffer);
    if (error) {
        Serial.print("JSON parse failed: ");
        Serial.println(error.c_str());
        return -1;
    }
    
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