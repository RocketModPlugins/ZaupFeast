# ZaupFeast
Feast Mod for Unturned requiring Rocket

Here is my take on the old Feast that was available for 2.2.5.  My version, you have a little bit more configuration options. I have included a folder with a default configuration file.  Please use it and modify it.  An example I used during testing is provided below as well. 

```xml
<?xml version="1.0" encoding="utf-8"?>
<FeastConfiguration xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Enabled>true</Enabled>
  <MinDropTime>600</MinDropTime>
  <MaxDropTime>1200</MaxDropTime>
  <DropRadius>20</DropRadius>
  <MinItemsDrop>5</MinItemsDrop>
  <MaxItemsDrop>25</MaxItemsDrop>
  <SkyDrop>false</SkyDrop>
  <PlaneEffectId>1001</PlaneEffectId>
  <SkydropEffectId>1006</SkydropEffectId>
  <MessageColor>red</MessageColor>
  <Items>
    <FeastItem>
      <Id>66</Id>
      <Name>Cloth</Name>
      <Chance>10</Chance>
      <Locations>
        <string>all</string>
      </Locations>
    </FeastItem>
    <FeastItem>
      <Id>43</Id>
      <Name>Military Ammunition Box</Name>
      <Chance>10</Chance>
      <Locations>
        <string>all</string>
      </Locations>
    </FeastItem>
    <FeastItem>
      <Id>44</Id>
      <Name>Civilian Ammunition Box</Name>
      <Chance>10</Chance>
      <Locations>
        <string>all</string>
      </Locations>
    </FeastItem>
    <FeastItem>
      <Id>13</Id>
      <Name>Canned Beans</Name>
      <Chance>10</Chance>
      <Locations>
        <string>all</string>
      </Locations>
    </FeastItem>
    <FeastItem>
      <Id>14</Id>
      <Name>Bottled Water</Name>
      <Chance>10</Chance>
      <Locations>
        <string>all</string>
      </Locations>
    </FeastItem>
    <FeastItem>
      <Id>10</Id>
      <Name>Police Vest</Name>
      <Chance>10</Chance>
      <Locations>
        <string>all</string>
      </Locations>
    </FeastItem>
    <FeastItem>
      <Id>251</Id>
      <Name>White Travelpack</Name>
      <Chance>10</Chance>
      <Locations>
        <string>all</string>
      </Locations>
    </FeastItem>
    <FeastItem>
      <Id>223</Id>
      <Name>Police Top</Name>
      <Chance>10</Chance>
      <Locations>
        <string>all</string>
      </Locations>
    </FeastItem>
    <FeastItem>
      <Id>224</Id>
      <Name>Police Bottom</Name>
      <Chance>10</Chance>
      <Locations>
        <string>all</string>
      </Locations>
    </FeastItem>
    <FeastItem>
      <Id>366</Id>
      <Name>Maple Crate</Name>
      <Chance>10</Chance>
      <Locations>
        <string>all</string>
      </Locations>
    </FeastItem>
  </Items>
</FeastConfiguration>
```

Most of these should be self explanatory.

Enabled: set to false if you really want to turn the feast off but not remove it from your server.

MinDropTime and MaxDropTime is the range of time between feast drops in seconds.  If you set both to the same number, it will be a set time.

DropRadius is how far from the location point they should randomly drop.  Default is 20 spaces.

MinItemsDrop and MaxItemsDrop is the range of number of items to be dropped each time.

SkyDrop:  If set to true, a series of effects will run and show an airdrop.  (Thank you Sven!!) Needs the RocketEffects Plane Bundle located at http://steamcommunity.com/sharedfiles/filedetails/?id=461458943.  Just subscribe to it and it will be downloaded to your client side.  Then copy the 461458943 folder located in you client workshop folder (on windows usually C:\Program Files (x86)\Steam\steamapps\workshop\content\304930) to the server workshop folder in Bundles\Workshop\Content.  The server will then send it to the players as needed.  Also requires Debris graphic setting to be on to be seen.  (Will not work if on Off.)

PlaneEffectId and SkydropEffectId are needed if SkyDrop is set true.  The planes are from 1001 to 1005.  SkydropEffect is only 1006 right now.

MessageColor: Set to a color by name or hex to give the feast messages sent to chat it's own color.

FeastItem is a required piece.  It gives all the info the feast needs to drop a certain type.  Name doesn't have to be exact (only for your reference), but id does.  Chance is how many times to add that id into the pot.  10 is default for 10 types.  (10 items, 10 times each, makes it 10% chance each item will be this specific item.)

Locations is the most configurable.  Just put all, and that item will drop at all locations, or you can specify 1 or more locations for that item to drop.  So you can have items drop in one location and not another.  The names come from the map nodes, and they do have to be exact to take.

All items are set to be removed when Unturned itself looks to sweep items that have been moved.  A message will be printed to the console that has the time and location of the next feast for logging purposes.

If you delete the configuration file, the system will recreate it on a restart but it will print a warning that it could not find it and disable the feast.  Just edit your config and restart the server.

Now includes a translation file for the two messages sent to chat.

```xml
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <TranslationEntry Id="coming_feast_msg" Value="The feast is beginning at {0} in {1} minutes!" />
  <TranslationEntry Id="now_feast_msg" Value="The feast is now at {0}!" />
</Translations>
```
