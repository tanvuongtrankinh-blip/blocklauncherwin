// Sample ModPE script: right-click (useItem) builds a small tower; chat "!heal" restores health.
var towers = 0;

function newLevel() {
  clientMessage(ChatColor.GREEN + "Sample script active in " + Level.getWorldName());
}

function useItem(x, y, z, itemId, blockId, side, itemDamage, blockDamage) {
  for (var i = 1; i <= 3; i++) Level.setTile(x, y + i, z, 4);
  towers++;
  ModPE.saveData("towers", String(towers));
  clientMessage("Built tower #" + towers);
  preventDefault();
}

function chatHook(str) {
  if (str == "!heal") {
    Player.setHealth(20);
    clientMessage(ChatColor.RED + "Healed!");
    preventDefault();
  }
}

function procCmd(cmd) {
  var p = cmd.split(" ");
  if (p[0] == "zombie") Level.spawnMob(Player.getX() + 2, Player.getY(), Player.getZ(), EntityType.ZOMBIE);
}
