#ifndef ORDERAPI_H
#define ORDERAPI_H

#include "network.h"

class Order {
public:
    int id;
    String name;
    int userId;
    int recipeId;
    Order() : id(0), name(""), userId(0), recipeId(0) {}
    Order(int id, const String& name, int userId = 0, int recipeId = 0) : id(id), name(name), userId(userId), recipeId(recipeId) {}
};

class OrderApi {
public:
    OrderApi(Network& network, const char* host, const int apiPort);
    bool checkApiConnection();
    int sendDataToApi(const char* endpoint, const char* data);
    int getOrderList(Order* orders, int maxOrders);
    int markOrderAsServed(int orderId);
    int editOrderSetServed(const Order& order);
    // Future: parseOrderList, getOrderById, etc.
private:
    Network& _network;
    const char* _host;
    const int _apiPort;
};

#endif // ORDERAPI_H
