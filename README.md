Visit us on Facebook: https://www.facebook.com/xarmenthusiast

# LewanSoul-xArm

The xArm has three means of communication.

1. HID-USB
2. TTL serial 9600 BAUD
3. Game Controller which appearently does not function even though the controller and adapter are provided.

## USB Communications

Uses https://github.com/mikeobrien/HidLibrary.

## xArm Controller USB Protocol

In general terms, a packet transmitted to the xArm will have the following format.

<table>
<thead><tr><th></th><th>id</th><th>header</th><th>length</th><th>command</th><th>parameters</th></tr></thead>
<tbody>
<tr><td><b>Bytes</b></td><td align="middle">1</td><td align="middle">2</td><td align="middle">1</td><td align="middle">1</td><td align="middle">0 or more</td></tr>
<tr><td><b>Comments</b></td><td>Any number.</td><td>Always 0x5555.</td><td>Here to end.</td><td>See <i>commands</i>.</td><td>See <i>commands</i>.</td></tr>
</tbody>
</table>

## Commands

Commands are essentially request packets embedded into USB HID reports.

USB HID reports generally have output and input data block lengths of 64 bytes. A USB HID report prepends an *id* byte so that messages spanning more than one block may be identified by their sequential block id. How how this is implemented is application specific. For purposes of controlling the xArm, a message never exceeds the maximum report length.

Requests and Reqponses are described in the following syntax:

* Each field is seperated by a space.
* Each field is described by the type in parentheses.
* Curly braces denote that their content may be repeated more than once.

#### ServoOffsetRead (28)  

Read the offset value of one or more servos.

Request: (byte)**count** { (byte)**id** }

Response: (byte)**count** { (byte)id (sbyte)**offset** }

Parameters: **count** is the number of servos in id list. **id** is one or more servo ids. **offset** is a signed byte with valid range of -128 to 128.
 
    ServoMove             3  (byte)count (ushort)time { (byte)id (ushort)position }
    GroupRunRepeat        5  (byte)group[255=all] (byte)times 
    GroupRun              6  (byte)group (ushort)count[0=continuous]
    GroupStop             7  -none-
    GroupErase            8  (byte)group[255=all]
    GroupSpeed           11  (byte)group (ushort)percentage
    xServoOffsetWrite    12  *** not sure
    xServoOffsetRead     13  *** not sure
    xServoOffsetAdjust   14  *** not sure
    GetBatteryVoltage    15  -none-; (ushort)millivolts
    ServoOff             20  (byte)count { (byte)id }
    ServoPositionRead    21  (byte)count { (byte)id }; (byte)count { (byte)id (ushort)position }
    ServoPositionWrite   22  (byte)count { (byte)id (ushort)position }
    ServoOffsetRead      23  (byte)count { (byte)id }; (byte)count { (byte)id (sbyte)offset }
    ServoOffsetWrite     24  (byte)id (sbyte)offset
    BusServoMoroCtrl     26  (byte)id (byte)??? (ushort)speed
    
    **These commands will affect all attached servos. Use these commands with only one servi attached.**
    BusServoInfoWrite    27  (byte)id (ushort)pos_min (ushort)pos_max (ushort)volt_min
                             (ushort)volt_max (ushort)temp_max (byte)led_status
                             (byte)led_warning
    BusServoInfoRead     28  -none-; (byte)id (ushort)pos_min (ushort)pos_max (ushort)volt_min
                             (ushort)volt_max (ushort)temp_max (byte)led_status
                             (byte)led_warning (byte)dev_offset (ushort)pos (byte)temp
                             (ushort)volt


#### BusServoInfoRead

BusServoInfoWrite affects all servos attached. Each servo must be programmed individually with the BusServoInfoWrite command. Reading info from the servos must also be performed individually.

Temperature range 50C to 100C in 1C increments. Default is 85C. The 16-bit field value is expressed in Centigrade.

Voltage range 4.5V to 12V in 0.1V increments. Default is 4.5V(min) and 12V(max). The 16-bit field value is expressed in millivolts, 4500mV(min) and 12000mV(max).

I have not tested it but I believe the ServoOff command will toggle the servo on and off and any servo movement command will turn it on if it was off.

# Ignore the following for now.

## Dispatch Timers

 `List<DispatcherTimer> dispatchTimers` is declared as a private property in MainWindow.cs only for possible future reference and is not used anywhere else.

 `InitializeDispatcherTimers()` is declared in `MainWindow()` and initializes 16-millisecond and 300-millisecond timers.
 
The *DispatcherTimer* will append up to one *Tick* to the *Interval* specified if that period exceeds one *Tick*'s period. A *Tick* seems to be about slightly faster than 1/60th of a second. Put another way, the *Tick* event will fire when the master *Tick* has occured, even if the specified period has been exceeded.

#### 16msTick

This timer is the game loop for the application and has a nominal frequency of 60Hz. It has two functions:

- Perform *scope*'s UI updates.
- Manage communication timing.

#### 300msTick
