# Custom Farming Redux

### Json Format
```sh
id : a number that is unique within the pack
name : Name of the Machine
legacy : if your Machine is a port of a CF machine (CF1 had a different format), you likely won't need that.
description : The description
category : Name of the Category, when in doubt use "Crafting", "Mailbox" will behave like a mailbox, "Scarecrow" like a scarecrow, "Chest" like a chest or drawer
texture : the filename of the machine tilesheet(within the same folder)
tileindex : the tileindex of the machine on the tilesheet, if it's only one machine on the sheets it's 0
readyindex: the tileindex showing the machine in it's ready state (produce ready to harvest)

pulsate : wheter or not the machine should pulsate while working
frames : if the machine is animated while working, the number of frames (animation has to start one index removed from the standing index)
fps: speed of the animation
crafting : the crafting recipe "parentsheetindex number parentsheetindex number..."
starter: the material that is required to use the machine regardless of other materials (like coal in the furnace)
starter:index => parentsheetindex of the starter-material / alternatively starter => item : the name of the starter-material
starter:stack => the number of starter-materials required
showitem: should the used material be drawn onto the machine while working
itempos: where the shown item should be drawn relative to the machine
itemzoom: size of the item
water: can this machine be placed in water (experimental, untested)
forsale: is this machine sold in a shop
shop: whos shop is it sold in (ex. "Robin")
price: what is it sold for
condition: under what condition should it be sold (same syntax as eventconditions or Lua Conditons) *
asdisplay: is this a sign or display case and not actually a machine
workconditions: conditions under which the machine should work (checked when starting production or at the start of the day) *
conditionalanimation: true or false, should machine animation stop if workconditions aren't met 
* exp. "w rainy", "LC caller.location.IsOutdoors" (caller is the machine)
lightsource: false, or true if it has a lightsource
worklight: true or false, will only show the light when working (default: true)
lightcolor: [r,g,b,a] color of the light exp. [0,0,0,255], default is [ 0, 139, 139, 255 ] (like the furnace)
lightradius: radius of the light, default is 1.5

production: what the machine produces, can include multiple productions
production=>index : the parentsheetindex of the produce, alternatively you can leave this out and pick the produce by using:
production=>name : the name of the produce, if both index and name are set, it will use the index, but change the name of the procuce to that set as name
production=>item : same as name but can be used to set random produce ("random:Item1,Item2,Item3")
production=>prefix : if set true the name of the material used for production will be put in front of the name like "Grape Wine" if name = Wine and material is Grape
production=>suffix : same as prefix but the material name is put after the produce name
production=>insert : same as above but the material name is put in between the name (name has to be 2 words with this)
production=>insertpos: if the produce has more than two names this can be set to 1 to go after the second word or 2 after the third ..
production=>colored : if true the produce will be automaticly colored like the matrials used or like specified in color(auto coloring can be hit and miss)
production=>color : if a color is set [R,G,B,A] this color will be used instead of the autocoloring.
production=>time : the time it takes to produce
production=>stack : the number of items produced
production=>quality : the quality of the item produced
production=>exclude : [idx,idx..] parentsheetindexes that should not be used as material even if they fit the material condition
production=>include : [idx,idx..] parentsheetindexes that should be used as material even if they don't fit the material condition
production=>texture : the filename of the produce tilesheet(within the same folder) 
production=>tileindex : the tileindex of the produce on the tilesheet

production=>materials : the materials required for this production, can include multiple materials
production=>materials=>index : parentsheetindex of the material, can also be a category (negative number). if you want to pick one by name use:
production=>materials=>item: name of the material
production=>materials=>stack : required number this material
production=>material=>quality: required minimum quality of the material, or use:
production=>material=>exactquality: required quality of the material
```

### Example
```sh
{
  "name": "Examples",
  "author": "Platonymous",
  "version": "1.1.0",
  "machines": [
    {
      "id": 0,
      "name": "Alien Idol",
      "legacy": "Platonymous.More.AlienIdol.json",
      "description": "Produces Rare Seeds out of cosmic energy if placed outdoors",
      "category": "Crafting",
      "texture": "MoreMachines.png",
      "tileindex": 0,
      "pulsate": false,
      "lightsource": true,
      "worklight": false,
      "lightcolor": [ 0, 0, 0, 255 ],
      "lightradius": 2.0,
      "frames": 4,
      "workconditions": "LC caller.location.IsOutdoors",
      "conditionalanimation": true,
      "crafting": "336 20 768 20",
      "production": [
        {
          "item": "Rare Seed",
          "time": 1200
        }
      ]
    },
    {
      "id": 1,
      "name": "Lemonade Maker",
      "legacy": "Platonymous.LemonadeMaker.json",
      "description": "Turns fruits into a refreshing beverage",
      "category": "Crafting",
      "crafting": "338 10 388 20 62 1",
      "texture": "lemonade.png",
      "tileindex": 0,
      "pulsate": false,
      "frames": 8,
      "starter": {
        "index": 245,
        "stack": 3
      },
      "production": [
        {
          "name": "Lemonade",
          "decription": "A refreshing beverage",
          "prefix": true,
          "colored": true,
          "index": 612,
          "price": "original + (input * 2)",
          "time": 60,
          "texture": "producetilesheet.png",
          "tileindex": 0,
          "materials": [
            {
              "index": -79,
              "stack": 3
            }
          ]
        },
        {
          "name": "Stardew Cola",
          "decription": "A refreshing beverage",
          "index": 176,
          "colored": true,
          "color": [ 110, 80, 40, 255 ],
          "price": "original + (input * 2)",
          "time": 60,
          "texture": "producetilesheet.png",
          "tileindex": 0,
          "materials": [
            {
              "index": 433
            }
          ]
        },
        {
          "name": "Root Beer",
          "decription": "A refreshing beverage",
          "index": 176,
          "colored": true,
          "color": [ 70, 50, 40, 255 ],
          "price": "original + (input * 2)",
          "time": 60,
          "texture": "producetilesheet.png",
          "tileindex": 0,
          "materials": [
            {
              "index": 412
            }
          ]
        },
        {
          "name": "Malt Beer",
          "decription": "A refreshing beverage",
          "index": 176,
          "colored": true,
          "color": [ 190, 140, 60, 255 ],
          "price": "original + (input * 2)",
          "time": 60,
          "texture": "producetilesheet.png",
          "tileindex": 0,
          "materials": [
            {
              "index": 304
            }
          ]
        }
      ]
    },
    {
      "id": 2,
      "legacy": "Platonymous.More.SlimeRarecrow.json",
      "name": "Slime Scarecrow",
      "description": "A slimy Scarecrow",
      "category": "Scarecrow",
      "crafting": "766 50 388 100",
      "texture": "MoreMachines.png",
      "tileindex": 7,
      "pulsate": false,
      "frames": 6
    },
    {
      "id": 3,
      "name": "Sushi Barrel",
      "legacy": "Platonymous.SushiBarrel.json",
      "description": "Turns rice and vinegar into sushi",
      "category": "Crafting",
      "crafting": "709 20 335 5",
      "texture": "machines.png",
      "tileindex": 2,
      "starter": {
        "index": 419,
        "stack": 10
      },
      "production": [
        {
          "name": "Sushi",
          "description": "Fermented rice",
          "index": 228,
          "time": 1800,
          "price": "original + (input * 3)",
          "quality": 4,
          "stack": 5,
          "texture": "producetilesheet.png",
          "tileindex": 3,
          "materials": [
            {
              "index": 423,
              "stack": 10
            }
          ]
        }
      ]
    },
    {
      "id": 4,
      "name": "Oil Mill",
      "legacy": "Platonymous.OilMill.json",
      "description": "Turns vegetables, forage or fish into oil",
      "category": "Crafting",
      "crafting": "766 50 709 20 337 1",
      "texture": "machines.png",
      "tileindex": 0,
      "production": [
        {
          "index": 247,
          "time": 120,
          "quality": -1,
          "prefix": true,
          "exclude": [ 248 ],
          "include": [ -81, -80 ],
          "materials": [
            {
              "index": -75,
              "quality": -4
            }
          ]
        },
        {
          "name": "Deluxe Oil",
          "index": 247,
          "time": 120,
          "quality": -1,
          "insert": true,
          "exclude": [ 248 ],
          "include": [ -81, -80 ],
          "materials": [
            {
              "index": -75,
              "quality": 4
            }
          ]
        },
        {
          "index": 772,
          "time": 120,
          "quality": -1,
          "materials": [
            {
              "index": 248,
              "stack": 10,
              "quality": -4
            }
          ]
        },
        {
          "index": 772,
          "time": 120,
          "quality": -1,
          "materials": [
            {
              "index": 248,
              "stack": 5,
              "quality": 4
            }
          ]
        },
        {
          "name": "Oil of",
          "index": 247,
          "time": 120,
          "quality": -1,
          "custom": true,
          "suffix": true,
          "materials": [
            {
              "index": -4
            }
          ]
        },
        {
          "index": 247,
          "time": 120,
          "quality": -1,
          "materials": [
            {
              "index": -74,
            }
          ]
        }
      ]
    },
    {
      "id": 5,
      "name": "Grill",
      "legacy": "Platonymous.More.Grill.json",
      "description": "Grills vegetables and fish",
      "category": "Crafting",
      "crafting": "335 5 539 1",
      "lightsource": true,
      "lightcolor": [ 0, 139, 139, 255 ],
      "lightradius": 1.5,
      "worklight": true,
      "texture": "MoreMachines.png",
      "tileindex": 14,
      "frames": 5,
      "pulsate": false,
      "showitem": true,
      "itempos": [ 14, 32 ],
      "itemzoom": 0.6,
      "forsale": true,
      "shop": "Pierre",
      "price": "2000",
      "starter": {
        "index": 382
      },
      "production": [
        {
          "name": "Grilled",
          "description": "Grilled goodness",
          "index": 198,
          "time": 30,
          "suffix": true,
          "texture": "MoreProduce.png",
          "tileindex": 3,
          "quality": -1,
          "materials": [
            {
              "index": -4,
            }
          ]
        },
        {
          "name": "Grilled",
          "description": "Grilled goodness",
          "suffix": true,
          "index": 200,
          "time": 30,
          "texture": "MoreProduce.png",
          "tileindex": 4,
          "quality": -1,
          "materials": [
            {
              "index": -75,
            }
          ]
        },
        {
          "name": "Grilled Sandwich",
          "description": "Grilled goodness",
          "insert": true,
          "index": 200,
          "time": 30,
          "texture": "MoreProduce.png",
          "tileindex": 2,
          "quality": -1,
          "include": [ 426 ],
          "materials": [
            {
              "index": 424,
            }
          ]
        }
      ]
    }
  ]
}

```
