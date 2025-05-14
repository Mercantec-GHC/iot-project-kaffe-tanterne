// API connection management
#include <BetterWiFiNINA.h>

const char* ssid = "MAGS-OLC"; // Network SSID (name)
const char* password = "Merc1234!"; // Network password
const char* api = "your-API"; // Api key
const char* host = "api.example.com"; // Api host

WiFiClient client;

// Function to check if the device is connected to wifi
bool checkWiFiConnection() {
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("WiFi not connected");
    return false;
  }
  return true;
}

// Function to connect to the wifi network
void connectToWiFi() {
  Serial.print("Connecting to ");
  Serial.println(ssid);

  // Attempt to connect to WiFi network
  while (WiFi.status() != WL_CONNECTED) {
    WiFi.begin(ssid, password);
    delay(10000);
  }

  Serial.println("Connected to WiFi");
}

// Function to check if the API is reachable
bool checkApiConnection() {
  // Check if the API is reachable
  if (!client.connect(host, 80)) {
    Serial.println("Connection to API failed");
    return false;
  }
  return true;
}

// Function to check and reconnect to WiFi and API
bool checkAndReconnect() {
  if (!checkWiFiConnection()) {
    connectToWiFi();
  }

  if (!checkApiConnection()) {
    Serial.println("Reconnecting to API...");
    if (client.connect(host, 80)) {
      Serial.println("Reconnected to API");
      return true;
    } else {
      Serial.println("Failed to reconnect to API");
      return false;
    }
  }
  return true;
}

// Function to send data to the API
int sendDataToApi(const char* endpoint, const char* data) {
    int responseCode = 0;

    if (client.connected()) {
        client.println("POST " + String(endpoint) + " HTTP/1.1");
        client.println("Host: " + String(host));
        client.println("Content-Type: application/json");
        client.println("Connection: close");
        client.print("Content-Length: ");
        client.println(strlen(data));
        client.println();
        client.println(data);
        delay(1000);
        // Read the response from the server
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

char* getOrderList() {
    // Check if the API is reachable
    if (!checkApiConnection()) {
        Serial.println("API not reachable");
        return nullptr;
    }

    // Send a GET request to the API
    client.println("GET /orders HTTP/1.1");
    client.println("Host: " + String(host));
    client.println("Connection: close");
    client.println();

    // Read the response from the server
    String response = "";
    while (client.connected() || client.available()) {
        if (client.available()) {
            String line = client.readStringUntil('\n');
            response += line + "\n";
        }
    }

    // Print the response
    Serial.println(response);
    
    // Return the response as a char array
    char* responseCharArray = new char[response.length() + 1];
    strcpy(responseCharArray, response.c_str());
    
    return responseCharArray;
}