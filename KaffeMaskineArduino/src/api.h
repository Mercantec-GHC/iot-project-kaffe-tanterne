// api.h - Declarations for API connection management
#ifndef API_H
#define API_H

#include <Arduino.h>

bool checkWiFiConnection();
void connectToWiFi();
bool checkApiConnection();
bool checkAndReconnect();
int sendDataToApi(const char* endpoint, const char* data);
char* getOrderList();

#endif // API_H
