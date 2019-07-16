
function doMagic (playedToday)
    maxDist = 9
	if (playedToday) then maxDist = 5 end

	isRaining = Game1.isRaining
	isOutdoors = Game1.currentLocation.isOutdoors
            
	if isRaining == true or isOutdoors == false then 
		return false 
	end
			
	Luau.setGameValue("isRaining", true, 500)
	Luau.setGameValue("isRaining", false, 2000)
	terrain = Game1.currentLocation.terrainFeatures.FieldDict
            
	for key,val in pairs(terrain) do
		if Luau.getObjectType(terrain[key].Value) == "StardewValley.TerrainFeatures.HoeDirt" then 
			if terrain[key].Value.state.Value < 1 then 
				water(key, terrain[key].Value) 
			end
		end
	end
    
	return true
end
        
function getDistance(v1,v2)
	distX = math.abs(v2.X - v1.X)
	distY = math.abs(v2.Y - v1.Y)
	return (math.sqrt((distX * distX) + (distY * distY)))
end

function water(v, hd)
    if getDistance(Game1.player.getTileLocation(),v) < maxDist then 
		hd.state.Value = 1 
	end
end