#include "measurementapi.h"
#include <Arduino.h>
#include <ArduinoJson.h>

MeasurementApi::MeasurementApi(Network& network, const char* host, const char* apiKey, const int apiPort)
    : _network(network), _host(host), _apiKey(apiKey), _apiPort(apiPort) {}

//Helper to skip HTTP headers and read response body.
static int readHttpBody(WiFiClient& client, char* buffer, int bufferSize) {
    String line;
    while (client.connected()) {
        line = client.readStringUntil('\n');
        if (line == "\r" || line.length() == 1) {
            break;
        }
    }

    //Read the body into a String.
    String body = "";
    unsigned long timeout = millis();
    while (client.connected() && (millis() - timeout < 2000)) {
        while (client.available()) {
            char c = client.read();
            body += c;
            timeout = millis();
        }
        delay(1);
    }

    //Find the start of JSON. (Either '{' or '['.)
    int jsonStart = body.indexOf('{');
    int arrStart = body.indexOf('[');
    int start = -1;
    if (jsonStart == -1 && arrStart == -1) {
        buffer[0] = '\0';
        return 0;
    }
    if (jsonStart == -1) start = arrStart;
    else if (arrStart == -1) start = jsonStart;
    else start = (jsonStart < arrStart) ? jsonStart : arrStart;

    body = body.substring(start);

    //Copy the body to the buffer.
    int len = body.length();
    if (len >= bufferSize) len = bufferSize - 1;
    body.toCharArray(buffer, bufferSize);
    buffer[len] = '\0';
    return len;
}

//GET request to fetch already existing measurements.
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

    int len = readHttpBody(client, buffer, bufferSize); 
    client.stop();
    return len;
}

//GET request to fetch already existing ingredients.
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

//Find ingredientId by name from ingredients buffer.
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

//Post request to create a new measurement.
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

//PUT request to update data for measurements. (No longer in use.)
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

//PUT request to create a new ingredient. (This is for when the ingredient does not exist.)
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

    //Try to parse the new ingredient ID from the response.
    StaticJsonDocument<256> doc;
    DeserializationError error = deserializeJson(doc, buffer);
    if (!error && doc.is<JsonObject>() && doc.containsKey("id")) {
        return doc["id"];
    }
    return -1;
}

//Find measurementId by ingredient name from measurements buffer.
int MeasurementApi::findMeasurementId(const char* buffer, const char* ingredientName) {
    StaticJsonDocument<2048> doc;
    DeserializationError error = deserializeJson(doc, buffer);
    if (error) {
        Serial.print("JSON parse failed: ");
        Serial.println(error.c_str());
        return -1;
    }

    //Handle root array or object with array property.
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