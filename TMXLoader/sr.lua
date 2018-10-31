lastNPC = nil
lastpos = nil
currentSpouse = nil
x1 = 0
y1 = 0

function entry(location,tile,layer)

	local size = (Game1.currentLocation.map.DisplayWidth / 64)
	local h = 9
	local w = 6
	x1 = 29
	y1 = 1
	
	if(size > 42) then x1 = 35 y1 = 10  end

	local y2 = y1 + h
	local x2 = x1 + w
	local pos = ":" .. x1 .. "-" .. x2 .. ":" .. y1 .. "-" .. y2;

	if(lastpos == nil) then
		local lastpos = pos
	end
	
	if currentSpouse ~= Game1.player.spouse or lastpos ~= pos then

		if lastNPC ~= nil then
			TMX.switchLayersAction("SwitchLayers AlwaysFront:AlwaysFront" .. lastNPC .. lastpos .. " Front:Front" .. lastNPC .. lastpos .. "  Buildings:Buildings" .. lastNPC .. lastpos .. "  Back:Back" .. lastNPC .. lastpos, location)
		end
		
		nextNPC = ""
		
		if Game1.player.spouse ~= nil then
			nextNPC =  Game1.player.spouse
			currentSpouse = Game1.player.spouse
		else
			currentSpouse = nil
		end

		if TMX.hasLayer(location.map, "Back" .. nextNPC) then
			TMX.switchLayersAction("SwitchLayers AlwaysFront:AlwaysFront" .. nextNPC .. pos .. " Front:Front" .. nextNPC .. pos .. "  Buildings:Buildings" .. nextNPC .. pos .. "  Back:Back" .. nextNPC .. pos, location)
		end

		lastNPC = nextNPC
		lastpos = pos
	end
end