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
myTileReplacement.injectTileInto($"Maps/springobjects", 74, 74);
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
