#ifndef TESTAPI_H
#define TESTAPI_H

#include "network.h"

class Order {
public:
    int id;
    String name;
    Order() : id(0), name("") {}
    Order(int id, const String& name) : id(id), name(name) {}
};

class TestApi {
public:
    TestApi(Network &network, const char *host, int apiPort, const char *apiKey);
    bool checkApiConnection();
    int sendDataToApi(const char* endpoint, const char* data);
    int getOrderList(Order* orders, int maxOrders);
    int getBusyOrder(Order* orders);
    int markAsServed(Order* orders);
    // Future: parseOrderList, getOrderById, etc.
private:
    Network& _network;
    const char* _host;
    int _apiPort;
    const char* _apiKey;
};

#endif // TESTAPI_H
