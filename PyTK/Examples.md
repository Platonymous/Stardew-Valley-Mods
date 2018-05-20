# PyTK Examples

### Implemantation
```sh
using PyTK.Extensions;
using PyTK.Types;
using PyTK.CustomElementHandler;
using PyTK.CustomTV;
```

### Input
##### Keys

```sh
using PyTK.Extensions;

Keys.K.onPressed(() => Monitor.Log($"K pressed"));
Keys.J.onPressed(yourMethod);
```

##### Mouse
```sh
using PyTK.Extensions;
using PyTK.Types;

ButtonClick.UseToolButton.onTerrainClick<Grass>(o => Monitor.Log($"Number of Weeds: {o.numberOfWeeds}"));
ButtonClick.ActionButton.onObjectCLick<SObject>(yourMethod);
ButtonClick.ActionButton.onCLick(new Vector2(60,60), "Town".toLocation(), yourMethod);
```

### Items

##### Shop
```sh
using PyTK.Extensions;
using PyTK.Types;

new InventoryItem(new Chest(true), 100).addToNPCShop("Pierre");
new InventoryItem(yourItem, yourPrice).addToNPCShop(shopName);
```

##### Object Events
```sh
using PyTK.Extensions;
using PyTK.Types;

new ItemSelector<SObject>(o => o.name == "Chest").whenAddedToInventory(list => list.useAll(i => i.name = "Test"));
new TerrainSelector<TerrainFeature>(yourPredicate).whenAddedToLocation(yourMethod);
```

## Maps

##### Actions
```sh
using PyTK.Extensions;
using PyTK.Types;

new TileAction("myAction", myMethod).register();
```

##### Injection / Merging
```sh
using PyTK.Extensions;

myNewMap.inject(@"Maps/MyMap");
myMapReplacement.injectAs(@"Maps/Beach");
myMap.mergeInto("Town".toLocation().Map, position, sourceRectangle).injectAs(@"Maps/Town");
"Town".toLocation().clearArea(new Rectangle(60, 30, 20, 20));
```

## Textures

##### Injection / Manipulation
```sh
using PyTK.Extensions;
using PyTK.Types;

myNewTexture.inject($"Characters/MyTexture");
myTextureReplacement.injectAs($"Maps/MenuTiles");
myTileReplacement.injectTileInto($"Maps/springobjects", 74);
orgTexture.setSaturation(0).injectTileInto($"Maps/springobjects", new Range(129, 166), new Range(129, 166));
```

## Data

##### Injection / Manipulation
```sh
using PyTK.Extensions;
using PyTK.Types;

myData.injectInto(@"Data/ObjectInformation");
new Mail(id, text, attachements, AttachmentType.OBJECT).injectIntoMail();
```

## Utilities
##### Time
```sh
using PyTK.Types;

STime inTwoHours = STime.CURRENT + 120;
bool timeReached = STime.CURRENT > targetTime;
```

## Multiplayer
##### Messaging
```sh
using PyTK;
using PyTK.Types;

//send Messages
PyNet.sendMessage("MyMod.MyAddress","mydata");

//receive Messages
List<MPMessage> messages = PyNet.getNewMessages("MyMod.MyAddress").ToList();
		   
//use Messenger
PyMessenger<MyClass> messenger = new PyMessenger<MyClass>("MyMod.MyMessengerAddress");
MyClass envelope = new MyClass(params);
messenger.send(envelope, SerializationType.JSON);
List<MyClass> messages = messenger.receive().ToList();

// Responder
PyResponder<bool,long> pingResponder = new PyResponder<bool, long>("PytK.Ping", (s) =>
           {
               return true;
           }, 1);
		   
pingResponder.start();		   

public void callPingResponders()
{
	foreach (Farmer farmer in Game1.otherFarmers.Values)
	{
		long t = Game1.currentGameTime.TotalGameTime.Milliseconds;
		Task<bool> ping = PyNet.sendRequestToFarmer<bool>("PytK.Ping", t, farmer);
		ping.Wait();
		long r = Game1.currentGameTime.TotalGameTime.Milliseconds;
		if (ping.Result)
			Monitor.Log(farmer.Name + ": " + (r - t) + "ms", LogLevel.Info);
		else
			Monitor.Log(farmer.Name + ": No Answer", LogLevel.Error);
		}
	}
}

PyResponder<int,int> staminaResponder = new PyResponder<int, int>("PytK.StaminaRequest", (stamina) =>
            {
                if (stamina == -1)
                    return (int)Game1.player.Stamina;
                else
                {
                    Game1.player.Stamina = stamina;
                    return stamina;
                }

            }, 8);
			
staminaResponder.start();

public int getStamina(Famer farmer)
{
	Task<int> getStamina = PyNet.sendRequestToFarmer<int>("PytK.StaminaRequest", -1, farmer);
	getStamina.Wait();
	int stamina = getStamina.Result;
	Monitor.Log(farmer.Name + ": " + stamina, LogLevel.Info);
	return stamina;
}

public void setStamina(Farmer farmer, int stamina)
{
	Task<int> setStamina = PyNet.sendRequestToFarmer<int>("PytK.StaminaRequest", stamina, farmer);
	setStamina.Wait();
	Monitor.Log(farmer.Name + ": " + setStamina.Result, LogLevel.Info);
}
```

## Frameworks
##### Custom TV
```sh
using PyTK.CustomTV;

CustomTVMod.addChannel(id, name, action);
CustomTVMod.showProgram(sprite, text, nextAction);
```

##### Custom Element Handler
```sh
using PyTK.CustomElementHandler;

namespace MyNameSpace
{
    internal class MyTool : Tool, ISaveElement 
    {
        public object getReplacement()
        {
            return new Chest(true);
        }
        
        public Dictionary<string, string> getAdditionalSaveData() 
        {
            Dictionary<string, string> saveData = new Dictionary<string, string>();
            saveData.Add("something","myValue");
            returen saveData;
        }
    
        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            this.something = additionalSaveData["something"];
        }
    }
}
```
