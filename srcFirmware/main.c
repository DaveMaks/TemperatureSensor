#define F_CPU 16000000

#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/wdt.h>
#include <stdio.h>
#include <util/delay.h>
#include "onewire.h"
#include "ds18x20.h"

#define SET_B(x) |= (1<<x)
#define CLR_B(x) &=~(1<<x)
#define INV_B(x) ^=(1<<x)

#define W1_PORT PORTB
#define W1_DDR DDRB
#define W1_PIN PINB
#define W1_BIT 0

unsigned char	nDevices;	// количество сенсоров
unsigned char	owDevicesIDs[MAXDEVICES][10];	// Их ID

void USART_init();
void USART0_write(unsigned char data);
void print_address(unsigned char* address);
unsigned char search_ow_devices(void);
void PrintTemperature(void);
void PrintUid(void);
unsigned char getch_Uart(void);

FILE usart_str = FDEV_SETUP_STREAM(USART0_write, NULL, _FDEV_SETUP_WRITE); // для функции printf

int main(void){
	char GetCar;
	_delay_ms(1000);
	stdout = &usart_str; // указываем, куда будет выводить printf
	DDRB = 0b00000010; PORTB = 0b00000010;
	DDRC SET_B(0);
	USART_init(); // включаем uart
	nDevices = search_ow_devices(); // ищем все устройства
	while(1)
	{
		PORTC SET_B(0);
		GetCar=getch_Uart();
		switch (GetCar){
			case 'l':
				PrintUid();
				break;
			case 'q':
				PrintTemperature();
				break;
			case 'r':
				search_ow_devices();
				break;
		}
		PORTC CLR_B(0);
	}
}

void print_address(unsigned char* address) {
	printf("%.2X-%.2X-%.2X-%.2X-%.2X-%.2X-%.2X-%.2X", address[0],address[1],address[2],address[3],address[4],address[5],address[6],address[7]);
}

void USART_init(){
	// Set baud rate
	UBRRH = 0;
	UBRRL = 103;
	UCSRA = 0;
	// Enable receiver and transmitter
	UCSRB = (1<<TXEN) | (1<<RXEN);
	// Set frame format
	UCSRC = (1<<UCSZ1) | (1<<UCSZ0) | (1<<URSEL);
}

void USART0_write(unsigned char data){
	while ( !( UCSRA & (1<<UDRE)) ) ;
	UDR = data;
}

unsigned char search_ow_devices(void){ // поиск всех устройств на шине

	unsigned char	i;
	unsigned char	id[OW_ROMCODE_SIZE];
	unsigned char	diff, sensors_count;
	sensors_count = 0;
	for( diff = OW_SEARCH_FIRST; diff != OW_LAST_DEVICE && sensors_count < MAXDEVICES ; )
	{
		OW_FindROM( &diff, &id[0] );
		if( diff == OW_PRESENCE_ERR ) break;
		if( diff == OW_DATA_ERR )	break;
		for (i=0;i<OW_ROMCODE_SIZE;i++)
		owDevicesIDs[sensors_count][i] = id[i];
		sensors_count++;
	}
	return sensors_count;
}

void PrintTemperature(void){
	nDevices = search_ow_devices(); // ищем все устройства
	for (unsigned char i=0; i<nDevices; i++){
		print_address(owDevicesIDs[i]);
		unsigned char data[2]={0x00,0x00};// переменная для хранения старшего и младшего байта данных
		unsigned char themperature[3]={0x00,0x00,0x00}; // в этот массив будет записана температура
		DS18x20_StartMeasureAddressed(owDevicesIDs[i]); // запускаем измерение
		_delay_us(800); // ждем минимум 750 мс, пока конвентируется температура
		DS18x20_ReadData(owDevicesIDs[i], data); // считываем данные
		DS18x20_ConvertToThemperature(data, themperature); // преобразовываем температуру в человекопонятный вид
		printf(":%c%d.%d", themperature[0],themperature[1],themperature[2]);
		printf("\n\r");
	}
}

void PrintUid(void){
	for (unsigned char i=0; i<nDevices; i++){
		print_address(owDevicesIDs[i]);
		printf("\n\r");
	}
}

unsigned char getch_Uart(void){//	Получение байта
	while(!(UCSRA&(1<<RXC)))	//	Устанавливается, когда регистр свободен
	{}
	return UDR;
}





