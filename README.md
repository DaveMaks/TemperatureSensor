# Описание
Аппаратно программный комплекс для мониторинга температуры в серверной комнате. Идея - сделать устройство для сбора данных с цифровых температурных датчиков Dallas DS18B20, на базе микроконтроллера AVR Atmega8 и последующим мониторингом и хранение данных с них.

# Схемотехника
<div>
<img src="/pcb/Scheme.jpg?raw=true" alt="" height="200px">
<img src="/pcb/PCB.JPG?raw=true" alt="" height="200px">
</div>

# Клиент
Общается с устройством по UART, выполняя запрос на получение устройств и значения на линии 1-write.
<div>
<img src="/winUI.png?raw=true" alt="" height="200px">
<img src="/dbrows.png?raw=true" alt="" height="200px">
</div>

# Настройки клиента
Настройка клиента производится в Properties
```xml
<Settings>
    <Setting Name="ComPort" Type="System.String" Scope="User">
      <Value Profile="(Default)">COM4</Value>
    </Setting>
    <Setting Name="DBHost" Type="System.String" Scope="User">
      <Value Profile="(Default)">10.110.0.0</Value>
    </Setting>
    <Setting Name="DBLogin" Type="System.String" Scope="User">
      <Value Profile="(Default)">DBLOGIN</Value>
    </Setting>
    <Setting Name="DBPwd" Type="System.String" Scope="User">
      <Value Profile="(Default)">DBPWD</Value>
    </Setting>
    <Setting Name="DBBaseName" Type="System.String" Scope="User">
      <Value Profile="(Default)">DBNAME</Value>
    </Setting>
    <Setting Name="TimeOut" Type="System.Int32" Scope="User">
      <Value Profile="(Default)">10</Value>
    </Setting>
    <Setting Name="IsStartDefault" Type="System.Boolean" Scope="User">
      <Value Profile="(Default)">False</Value>
    </Setting>
    <Setting Name="ContineUID" Type="System.String" Scope="User">
      <Value Profile="(Default)">28-C0-17-5D-05-00-00-DC,28-C0-17-5D-05-00-00-DC</Value>
    </Setting>
    <Setting Name="MaxTempAlarm" Type="System.Int32" Scope="User">
      <Value Profile="(Default)">21</Value>
    </Setting>
    <Setting Name="EmailFrom" Type="System.String" Scope="User">
      <Value Profile="(Default)">example@hostname.kz</Value>
    </Setting>
    <Setting Name="EmailTo" Type="System.String" Scope="User">
      <Value Profile="(Default)">exampleusername@hostname.kz</Value>
    </Setting>
    <Setting Name="EmailServer" Type="System.String" Scope="User">
      <Value Profile="(Default)">mail.hostname.kz</Value>
    </Setting>
    <Setting Name="EmailLogin" Type="System.String" Scope="User">
      <Value Profile="(Default)">exampleusername</Value>
    </Setting>
    <Setting Name="EmailPwd" Type="System.String" Scope="User">
      <Value Profile="(Default)">exampleusernamepass</Value>
    </Setting>
  </Settings>
```

License
----
MIT

