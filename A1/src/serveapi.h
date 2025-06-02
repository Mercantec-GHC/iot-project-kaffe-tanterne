#ifndef SERVEAPI_H
#define SERVEAPI_H

#include "network.h"

class Order {
public:
    int id;
    String name;
    Order() : id(0), name("") {}
    Order(int id, const String& name) : id(id), name(name) {}
};

class ServeApi {
public:
    ServeApi(Network &network, const char *host, const char *apiKey, const int apiPort);
    bool checkApiConnection();
    int sendDataToApi(const char* endpoint, const char* data);
    int getOrderList(Order* orders, int maxOrders);
    int getBusyOrder(Order* orders);
    int markAsServed(Order* orders);
    // Future: parseOrderList, getOrderById, etc.
private:
    Network& _network;
    const char* _host;
    const char* _apiKey;
    const int _apiPort;
};

#endif // ServeAPI_H
