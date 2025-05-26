#ifndef ORDERAPI_H
#define ORDERAPI_H

#include "network.h"

class Order {
public:
    int id;
    String name;
    Order() : id(0), name("") {}
    Order(int id, const String& name) : id(id), name(name) {}
};

class OrderApi {
public:
    OrderApi(Network& network, const char* host, const int apiPort);
    bool checkApiConnection();
    int sendDataToApi(const char* endpoint, const char* data);
    int getOrderList(Order* orders, int maxOrders);
    // Future: parseOrderList, getOrderById, etc.
private:
    Network& _network;
    const char* _host;
    const int _apiPort;
};

#endif // ORDERAPI_H
