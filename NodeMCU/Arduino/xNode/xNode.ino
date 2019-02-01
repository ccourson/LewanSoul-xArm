#include <SoftwareSerial.h>

#define BAUD 9600
#define rxPin 4
#define txPin 5

SoftwareSerial serial = SoftwareSerial(rxPin, txPin);

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(rxPin, INPUT);
  pinMode(txPin, OUTPUT);
  serial.begin(BAUD);
}

uint8_t buf[103];

// the loop function runs over and over again forever
void loop() {
  digitalWrite(LED_BUILTIN, LOW);
  delay(1000);

  buf[0] = 0x55;
  buf[1] = 0x55;
  buf[2] = 0x08; // len
  buf[3] = 0x03; // cmd
  buf[4] = 0x01; // num
  buf[5] = 0xf4; // time lsb
  buf[6] = 0x01; // time msb
  buf[7] = 0x01; // id
  buf[8] = 0x90; // pos lsb
  buf[9] = 0x01; // pos msb
  
  serial.write(buf, 10);
  
  digitalWrite(LED_BUILTIN, HIGH);
  delay(1000);

  buf[0] = 0x55;
  buf[1] = 0x55;
  buf[2] = 0x08; // len
  buf[3] = 0x03; // cmd
  buf[4] = 0x01; // num
  buf[5] = 0xf4; // time lsb
  buf[6] = 0x01; // time msb
  buf[7] = 0x01; // id
  buf[8] = 0x58; // pos lsb
  buf[9] = 0x02; // pos msb
  
  serial.write(buf, 10);
}
