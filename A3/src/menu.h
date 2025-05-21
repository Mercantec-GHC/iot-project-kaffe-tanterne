#ifndef MENU_H
#define MENU_H

#include <Arduino_MKRIoTCarrier.h>

// MenuOption class to hold text and a void pointer to any object
class MenuOption {
  public:
    String text;
    void* data;
    MenuOption() : text(""), data(nullptr) {}
    MenuOption(const String& t, void* d = nullptr) : text(t), data(d) {}
};

void menuInit(MKRIoTCarrier& carrier);
void menuUpdate(MKRIoTCarrier& carrier);
void setMenuOptions(const MenuOption* options, int count);
int getSelectedIndex();
void setMenuSelectCallback(void (*callback)(int));
const MenuOption& getSelectedOption();

#endif // MENU_H
