# Robot Commands

## USB HID Report

Commands are sent to the xArm in one or more USB HID reports. HID reports consist of 1 Report ID byte followed by 64 data bytes. Robot commands are always sent as one command in one report.

At a high level, command reports have the following format:

| Report ID | SIG | LEN | CMD | DATA |
|---|---|---|---|---|
| byte | ushort | byte | byte | *up to 60 bytes. see individual commands.* |

**Report ID:** This byte can be any number. Typically it is used to identify a report by sequence number when data is sent using more than one reccord.

**SIG:** The Signature is an unsigned 16-bit word and is always 0x5555. I believe this is used only to verify the identity of the report.

**LEN:** This byte of all of the following bytes including this byte.

**CMD:** A single byte.

**DATA:** After Report ID, SIG, LEN and CMD, the report contains room for 60 bytes of data. Robot command lengths are well within this limit. DATA is command specific.

The xArm PC application creates a 50-byte buffer and fills that buffer with the DATA content. It then calls a method that assembles the report parts and DATA. The report is transmitted and if the command is expected to respond, it waits for this response for 300 milliseconds.

## Attribute Notation

**(type)name:** The type and name of a parameter is declared by placing the type in parenthesis and immediately following it without whitespace by the name of the paramter. Therefore **(byte)group** represents a **byte** value named **group**. Mutiple parameters are seperated by spaces.

**\{ }:** Paramters that may be repeated are represented by curly braces.

**CAPS:** Paramters in capital letters define 

## ServoMove

| Command | Dec | Hex | Tx/Rx | DATA |
| :-: | :-: | :-: | :-: |
| ServoMove | 3 | 03 | Tx | (byte)count (ushort)milliseconds { (byte)servo (ushort)position[FF00=null] } |
| | | | Rx | -none- |

Positions 1 to 6 servos over a duration of milliseconds. ServoMove does not return a reply.

**(byte)count:** 8 bits, unsigned. The number of servos in servo position array. Valid numbers in range 1 to 6.

**(ushort)milliseconds:** 16 bits, unsigned. Duration of servo movements. The robot will take this long to complete the movement. Valid range is 0 to 3,000.





                       Dec Hex Tx; Rx
    ------------------+---+---+--------------------------------------------------------
    ServoMove            3  03 (byte)count (ushort)milliseconds { (byte)servo (ushort)position[FF00=null] }

                            
    GroupDownload        5  05 (byte)cmd (byte)group [parameters]
                               - cmd=1: parameters={ (byte)count[mod(255)] }[extends by byte as needed]; (byte)cmd 00
                               - cmd=2: parameters=00 (byte)count 00                                   ; (byte)cmd 00
                               - cmd=3: parameters=00 00          [255 actions]                        ; (byte)cmd 00
                               - cmd=3: parameters=00 00 01       [512 actions]                        ; (byte)cmd 00
                               - cmd=3: parameters=00 00 01 02    [765 actions]                        ; (byte)cmd 00
                               - cmd=3: parameters=00 00 01 02 03 [1,020 actions]                      ; (byte)cmd 00
                               - cmd=4: parameters=(byte)block (byte)index 0B (ushort)milliseconds { (byte)servo (ushort)position } 
                                                                                                       ; (byte)cmd 00
                               - cmd=5: parameters= -none-                                             ; (byte)cmd 00
                               * A group may contain 4 blocks of 255 actions or 1,020 actions.        
                            
    GroupRun =           6  06 (byte)group (ushort)count[0=continuous]
    GroupStop            7  07 -none-
    GroupErase           8  08 (byte)group[255=all]; [empty response]
                            
    GroupSpeed          11  0b (byte)group[255=all] (ushort)percentage
                            
    GetBatteryVoltage   15  0f -none-; (ushort)millivolts
                            
    ServoOff            20  14 (byte)count { (byte)servo }
    ServoPositionRead   21  15 (byte)count { (byte)servo }; (byte)count { (byte)servo (ushort)position }
    ServoOffsetWrite    22  16 (byte)count { (byte)servo }
    ServoOffsetRead     23  17 (byte)count { (byte)servo }; (byte)count { (byte)servo (sbyte)offset }
    ServoOffsetAdjust   24  18 (byte)servo (short)offset
                            
    ServoSpeed          26  1a (byte)servo (byte)mode[0=servo,1=motor] (ushort)milliseconds
    BusServoInfoWrite   27  1b (byte)servo (ushort)position_min (ushort)position_max (ushort)millivolts_min (ushort)millivolts_max (ushort)temp_max (byte)led_status[0=enable,1=disable] (byte)led_warning[bits:0=overheat,1=overvoltage,2=overposition]
    BusServoInfoRead    28  1c -none-; (byte)id (ushort)position_min (ushort)position_max (ushort)millivolts_min (ushort)millivolts_max (ushort)temp_max (byte)led_status[0=enable,1=disable] (byte)led_warning[bits:0=overheat,1=overvoltage,2=overposition] (byte)offset (ushort)position (byte)temp (ushort)millivolts
