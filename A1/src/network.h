#ifndef NETWORK_H
#define NETWORK_H

#include <BetterWiFiNINA.h>

class Network {
public:
    Network(const char* ssid, const char* password);
    void connect();
    bool isConnected();
    WiFiClient& getClient();
private:
    const char* _ssid;
    const char* _password;
    WiFiClient _client;
};

#endif // NETWORK_H
