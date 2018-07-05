# LewanSoul-xArm

## Objective

## USB Communications

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

#### (28) ServoOffsetRead  

Read the offset of one or more servos.

    Request: (byte)count { (byte)id }

**count** is the number of servos in id list. **id** is one or more servo ids.

    Response: (byte)count { (byte)id (byte)offset }


 

Uses https://github.com/mikeobrien/HidLibrary



        ServoMove =             3,  // (byte)count (ushort)time { (byte)id (ushort)position }
        GroupRunRepeat =        5,  // (byte)group[255=all] (byte)times 
        GroupRun =              6,  // (byte)group (ushort)count[0=continuous]
        GroupStop =             7,  // -none-
        GroupErase =            8,  // (byte)group[255=all]
        GroupSpeed =            11, // (byte)group (ushort)percentage
        xServoOffsetWrite =      12, 
        xServoOffsetRead =       13, 
        xServoOffsetAdjust =     14,
        GetBatteryVoltage =     15, // -none-; (ushort)millivolts
        ServoOff =              20, // (byte)count { (byte)id }
        ServoPositionRead =     21, // (byte)count { (byte)id }; (byte)count { (byte)id (byte)offset }
        ServoPositionWrite =    22, // (byte)count { (byte)id }
        ServoOffsetRead =       23, // (byte)count { (byte)id }; (byte)count { (byte)id (byte)offset }
        ServoOffsetWrite =      24, // (byte)id (ushort)value
        BusServoMoroCtrl =      26, // (byte)id (byte)??? (ushort)speed
        BusServoInfoWrite =     27, // (byte)id (ushort)pos_min (ushort)pos_max (ushort)volt_min (ushort)volt_max
                                    //         (ushort)temp_max (byte)led_status (byte)led_warning
        BusServoInfoRead =      28  // -none-; (byte)id (ushort)pos_min (ushort)pos_max (ushort)volt_min (ushort)volt_max 
                                    //         (ushort)temp_max (byte)led_status (byte)led_warning (byte)dev_offset
                                    //         (ushort)pos (byte)temp (ushort)volt
