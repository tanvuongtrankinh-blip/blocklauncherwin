// Legacy global aliases that classic ModPE scripts rely on.
function getPlayerEnt() { return Player.getEntity(); }
function getPlayerX() { return Player.getX(); }
function getPlayerY() { return Player.getY(); }
function getPlayerZ() { return Player.getZ(); }
function getCarriedItem() { return Player.getCarriedItem(); }
function getTile(x, y, z) { return Level.getTile(x, y, z); }
function setTile(x, y, z, id, data) { Level.setTile(x, y, z, id, data || 0); }
function explode(x, y, z, r) { Level.explode(x, y, z, r); }
function spawnChicken(x, y, z, tex) { return Level.spawnChicken(x, y, z, tex || "mob/chicken.png"); }
function spawnCow(x, y, z, tex) { return Level.spawnCow(x, y, z, tex || "mob/cow.png"); }
function setNightMode(n) { Level.setNightMode(n); }
function bl_setMobSkin(e, t) { Entity.setMobSkin(e, t); }
function rideAnimal(a, b) { Entity.rideAnimal(a, b); }
function setPosition(e, x, y, z) { Entity.setPosition(e, x, y, z); }
function setVelX(e, v) { Entity.setVelX(e, v); }
function setVelY(e, v) { Entity.setVelY(e, v); }
function setVelZ(e, v) { Entity.setVelZ(e, v); }
function getPitch(e) { return Entity.getPitch(e || Player.getEntity()); }
function getYaw(e) { return Entity.getYaw(e || Player.getEntity()); }
function print(s) { ModPE.log(String(s)); }
