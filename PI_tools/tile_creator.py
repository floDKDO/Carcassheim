from __future__ import annotations
from io import BufferedRandom
from typing import Any, Tuple, Union, List
from PIL import Image
import xml.etree.ElementTree as ET
from collections import deque
import os, sys

MAXWIDTH = 1000
MAXHEIGHT = 720

Number = Union[float, int]
ERR_OUT = sys.stdout

UNITY_PATH = "../"
UNITY_FILE = "Carcassheim_unity/"
UNITY_TILE_PATH = os.path.join(UNITY_FILE, "Assets/Affichage_InGame/Tile/")
UNITY_MASK_PATH = os.path.join(UNITY_TILE_PATH, "Mask/")


def save_front_xml(tl_file: TileFile):
    print("start save front")
    root = ET.ElementTree()
    root_el = ET.Element("carcasheim")
    meeple_id = 0
    zone_id = 0
    tile_id_conv = {}
    tile_slot_id_conv = {}
    meeple_id_conv = {}
    zone_id_conv = {}

    for tile_id, tile in enumerate(tl_file.getTiles()):
        tile_el = ET.Element("tuile")
        root_el.append(tile_el)

        id_el = ET.Element("id")
        id_el.text = f"{tile_id}"
        tile_el.append(id_el)
        tile_id_conv[tile.id] = tile_id
        tile_slot_id_conv[tile.id] = {}

        sprite_el = ET.Element("sprite")
        if tile.sprite is None:
            print(f"Error : Tile {tile.id} has no sprite", file=ERR_OUT)
            return
        try:
            path = os.path.relpath(
                tile.spriteName, os.path.join(UNITY_PATH, UNITY_FILE)
            )
            sprite_el.text = f"{path}"
        except:
            print("Error: trying to create path to sprite", file=ERR_OUT)
        tile_el.append(sprite_el)

        for slot_id, slot in enumerate(tile.lslots):
            slot_el = ET.Element("slot")
            tile_el.append(slot_el)

            id_el = ET.Element("id")
            id_el.text = f"{slot_id}"
            slot_el.append(id_el)

            x_el = ET.Element("x")
            x_el.text = f"{slot.x}"
            slot_el.append(x_el)
            y_el = ET.Element("y")
            y_el.text = f"{slot.y}"
            slot_el.append(y_el)
            
            tile_slot_id_conv[tile.id][slot.id] = slot_id

            sprite_el = ET.Element("sprite")
            if slot.mask_name is None:
                print(
                    f"Error: Slot {slot.id} in tile {tile.id} has no mask",
                    file=ERR_OUT,
                )
                return
            try:
                path = os.path.relpath(
                    slot.mask_name, os.path.join(UNITY_PATH, UNITY_FILE)
                )
                sprite_el.text = f"{path}"
            except:
                print("Error: trying to create path for slot", file=ERR_OUT)
            slot_el.append(sprite_el)

    root = ET.ElementTree(root_el)
    root.write("config_front.xml")


def save_back_xml(tl_file: TileFile):
    tl_file = tl_file.getActive()
    root_el = ET.Element("carcasheim")
    meeple_id = 0
    zone_id = 0
    tile_id_conv = {}
    tile_slot_id_conv = {}
    meeple_id_conv = {}
    zone_id_conv = {}
    slots_el = {}
    for zone_id, zone in enumerate(tl_file.getSlotType()):
        zone_el = ET.Element("terrain")

        id_el = ET.Element("id")  # zone id
        id_el.text = f"{zone_id}"
        zone_el.append(id_el)

        zone_id_conv[zone.id] = zone_id  # zone id normalization

        name_el = ET.Element("nom")
        if zone.name == "":
            print(f"Warning: Zone {zone.id} has no name", file=ERR_OUT)
        name_el.text = f"{zone.name}"
        zone_el.append(name_el)

        root_el.append(zone_el)

    for tile_id, tile in enumerate(tl_file.getTiles()):
        tile_el = ET.Element("tuile")
        tile_dir = {}
        for dir in DIR:
            tile_dir[dir] = False

        id_el = ET.Element("id")  # tile id
        id_el.text = f"{tile_id}"
        tile_el.append(id_el)
        tile_id_conv[tile.id] = tile_id  # tile id normalization
        tile_slot_id_conv[tile.id] = {}
        slots_el[tile.id] = {}

        for slot_id, slot in enumerate(tile.lslots):
            slot_el = ET.Element("slot")
            slots_el[tile.id][slot.id] = slot_el

            id_el = ET.Element("id")  # slot id
            id_el.text = f"{slot_id}"
            slot_el.append(id_el)
            tile_slot_id_conv[tile.id][slot.id] = slot_id  # slot id normalization

            zone_el = ET.Element("terrain")  # zone
            if slot.zone is None:
                print(
                    f"Error: Slot {slot.id} in tile {tile.id} has no type",
                    file=ERR_OUT,
                )
                return
            zone_el.text = f"{zone_id_conv[slot.zone.id]}"
            slot_el.append(zone_el)

            for dir in DIR:
                if slot.dir[dir]:
                    if tile_dir[dir]:
                        print(
                            f"Error: Position {dir} is linked more than once in tile {tile.id}",
                            file=ERR_OUT,
                        )
                        return
                    slot_el.append(ET.Element(dir))
                    tile_dir[dir] = True
            tile_el.append(slot_el)
        for dir in DIR:
            if not tile_dir[dir]:
                print(
                    f"Error: Position {dir} is linked to no slot in tile {tile.id}",
                    file=sys.stderr,
                )

        root_el.append(tile_el)

    for tile_id, tile in enumerate(tl_file.getTiles()):
        for slot_id, slot in enumerate(tile.lslots):
            for linked in slot.slot_links:
                link_el = ET.Element("link")
                link_el.text = f"{tile_slot_id_conv[tile.id][linked]}"
                slots_el[tile.id][slot.id].append(link_el)

    root = ET.ElementTree(root_el)
    root.write("config_back.xml")


EXPORT = {
    "front XML": save_front_xml,
    "back XML": save_back_xml,
}


def relpath(path=""):
    if len(path) == 0:
        return ""
    else:
        return os.path.relpath(path)


class SlotComptabilities:
    def __init__(self):
        self.comp = {}

    def add(self, id0: int, id1: int):
        # print(f"ADD {id0} {id1}")
        if id0 in self.comp:
            self.comp[id0].append(id1)
        else:
            self.comp[id0] = deque([id1])
        if id0 != id1:
            if id1 in self.comp:
                self.comp[id1].append(id0)
            else:
                self.comp[id1] = deque([id0])

    def reset(self):
        self.comp.clear()

    def remove(self, id0: int, id1: int):
        self.comp[id0].remove(id1)
        if id0 != id1:
            self.comp[id1].remove(id0)

    def remove_species(self, id: int):
        for idf in self.comp[id]:
            self.comp.remove(idf)
        self.comp.pop(id)

    def connectedTo(self, id0: int, id1: int):
        # print("RESULT", id0, id1, id0 in self.comp and id1 in self.comp[id0])
        return id0 in self.comp and id1 in self.comp[id0]

    def __str__(self):
        res = ""
        for id0 in self.comp:
            for id1 in self.comp[id0]:
                if id0 <= id1:
                    res += f"{id0},{id1};"
        return res + "\n"


class SlotType:
    NEXTID = 0

    def _nextId(self):
        id = SlotType.NEXTID
        SlotType.NEXTID += 1
        return id

    def __init__(self, tl_master: TileFile, name: str = "", spriteName: str = None):
        self.id = self._nextId()
        self.tl_master = tl_master
        self.name = name

        if spriteName is not None:
            try:
                self.sprite = Image.open(spriteName)
                self.spriteName = spriteName
            except:
                self.sprite = None
                self.spriteName = None
        else:
            self.spriteName = None
            self.sprite = None
        self.tl_master.addSlotType(self)

    def setId(self, id: int):
        self.id = id
        if id > SlotType.NEXTID:
            SlotType.NEXTID = id + 1

    def connectedTo(self, other: Union[SlotType, Meeples]):
        if type(other) == type(self):
            # print("SLOTTYPE", self.id, other.id)
            return self.tl_master.getComptabilities().connectedTo(self.id, other.id)
        else:
            return other.connectedTo(self)

    def addComptabilities(self, other: SlotType):
        self.tl_master.getComptabilities().add(self.id, other.id)

    def removeComptabilities(self, other: SlotType):
        self.tl_master.getComptabilities().remove(self.id, other.id)

    def destroy(self):
        self.tl_master.getComptabilities().remove_species(self.id)
        self.tl_master.getMeepleComptabilities().remove_species(self.id)
        self.tl_master.removeSlotType(self)

    def __str__(self):
        spriteName = self.spriteName if self.spriteName is not None else ""
        return f"{self.id},{spriteName},{self.name};"


DIR = [
    "n",
    "e",
    "w",
    "s",
    "nee",
    "nne",
    "nww",
    "nnw",
    "sse",
    "see",
    "ssw",
    "sww",
]
DIR_vect = {
    "n": (0, -1),
    "e": (1, 0),
    "w": (-1, 0),
    "s": (0, 1),
}


class Slot:
    def __init__(
        self,
        tile: Tile,
        coord: Tuple[float, float],
        zone: SlotType = None,
        mask_name: str = None,
        slot_links: list = None,
    ) -> None:
        self.x, self.y = coord
        self.zone = zone
        self.tile = tile
        self.setMask(mask_name)
        self.id = self.tile.addSlot(self)

        self.slot_links = slot_links if slot_links is not None else []
        self.dir = {}
        for dir in DIR:
            self.dir[dir] = False

    def setMask(self, name: str):
        self.mask_name = None if name == "" else name
        if self.mask_name is None:
            self.mask = None
            return
        try:
            self.mask = Image.open(self.mask_name).convert("L")
            if self.mask.size != self.tile.dim:
                self.mask = None
                self.mask_name = None
                return
            elif self.mask.size != self.tile.sprite.size:
                self.mask = self.mask.resize(self.tile.sprite.size)
        except:
            self.mask = None
            self.mask_name = None
            return
        img0 = Image.new("RGBA", self.mask.size, (0, 255, 0, 100))
        img1 = Image.new("RGBA", self.mask.size, (0, 0, 0, 0))
        self.mask = Image.composite(img0, img1, mask=self.mask)

    def setId(self, id: int):
        self.id = id
        self.tile.changedSlotId(id)

    def destroy(self):
        self.tile.removeSlot(self)

    def __str__(self):
        tile = self.tile.id
        zone = self.zone.id if self.zone is not None else -1
        mask_name = self.mask_name if self.mask_name is not None else ""
        pos = ""
        for dir in DIR:
            if self.dir[dir]:
                pos += f"pos,{tile},{self.id},{dir};"
        link = ""
        for lnk in self.slot_links:
            link += f"link,{self.id},{lnk};"
        return f"{tile},{self.id},{zone},{mask_name},{self.x},{self.y};" + pos + link


class FailedCreation(Exception):
    def __init__(self, *args: object) -> None:
        super().__init__(*args)


class MeeplesSlotTypeComptabilities:
    def __init__(self):
        self.comp_M_to_ST = {}
        self.comp_ST_to_M = {}

    def reset(self):
        self.comp_M_to_ST.clear()
        self.comp_ST_to_M.clear()

    def add(self, id0, id1):
        if id0 in self.comp_M_to_ST:
            self.comp_M_to_ST[id0].append(id1)
        else:
            self.comp_M_to_ST[id0] = deque([id1])

        if id1 in self.comp_ST_to_M:
            self.comp_ST_to_M[id1].append(id0)
        else:
            self.comp_ST_to_M[id1] = deque([id0])

    def remove(self, id0, id1):
        self.comp_M_to_ST[id0].remove(id1)
        self.comp_ST_to_M[id1].remove(id0)

    def remove_meeple(self, id):
        for idf in self.comp_M_to_ST[id]:
            self.comp_ST_to_M.remove(idf)
        self.comp_M_to_ST.pop(id)

    def remove_species(self, id):
        for idf in self.comp_ST_to_M[id]:
            self.comp_M_to_ST.remove(idf)
        self.comp_ST_to_M.pop(id)

    def connectedTo(self, id0, id1):
        return id0 in self.comp_M_to_ST and id1 in self.comp_M_to_ST[id0]

    def __str__(self):
        res = ""
        for id0 in self.comp_M_to_ST:
            for id1 in self.comp_M_to_ST[id0]:
                res += f"{id0},{id1};"
        return res + "\n"


class Meeples:
    NEXT_ID = 0

    def _nextId():
        id = Meeples.NEXT_ID
        Meeples.NEXT_ID += 1
        return id

    def __init__(self, tl_master: TileFile, name: str = "", spriteName: str = None):
        self.tl_master = tl_master
        self.id = Meeples._nextId()

        self.name = name
        if spriteName is not None:
            try:
                self.sprite = Image.open(spriteName)
                self.spriteName = spriteName
            except:
                self.sprite = None
                self.spriteName = None
        else:
            self.spriteName = None
            self.sprite = None

        self.tl_master.addMeeple(self)

    def setId(self, id):
        self.id = id
        if id > Meeples.NEXT_ID:
            Meeples.NEXT_ID = id + 1

    def connectedTo(self, other: SlotType):
        return self.tl_master.getMeepleComptabilities().connectedTo(self.id, other.id)

    def addComptabilities(self, other: SlotType):
        self.tl_master.getMeepleComptabilities().add(self.id, other.id)

    def removeComptabilities(self, other: SlotType):
        self.tl_master.getMeepleComptabilities().remove(self.id, other.id)

    def destroy(self):
        self.tl_master.getMeepleComptabilities().remove_meeple(self.id)
        self.tl_master.removeMeeple(self)

    def __str__(self):
        spriteName = self.spriteName if self.spriteName is not None else ""
        return f"{self.id},{spriteName},{self.name};"


class Tile:
    NEXT_ID = 0
    lslots: List[Slot]

    def __init__(self, tl_master: TileFile, spriteName: str, exception_on=True) -> None:
        self.tl_master = tl_master
        self.spriteName = spriteName
        self.id = Tile.NEXT_ID
        Tile.NEXT_ID += 1

        self.lslots = []
        self.next_id_slot = 0
        self.tl_master.addTile(self)

        self.dim = None
        self.sprite = None
        self.spriteName = None
        self.setSprite(spriteName, exception_on)

    def __bool__(self):
        return self.sprite is not None

    def setSprite(self, spriteName: str, exception_on: bool = True):
        try:
            sprite = Image.open(spriteName)
            self.dim = sprite.size
            if sprite.width > MAXWIDTH or sprite.height > MAXHEIGHT:
                ratio = min(MAXWIDTH / sprite.width, MAXHEIGHT / sprite.height)
                sprite = sprite.resize(
                    (int(ratio * sprite.width), int(ratio * sprite.height))
                )
            self.sprite = sprite
            self.spriteName = spriteName
        except:
            if exception_on:
                raise FailedCreation()

    def changedSlotId(self, id: int):
        if id > self.next_id_slot:
            self.next_id_slot = id + 1

    def setId(self, id: int):
        self.id = id
        if self.id > Tile.NEXT_ID:
            Tile.NEXT_ID = self.id + 1

    def setNextId(next_id: int):
        Tile.NEXT_ID = next_id

    def removeSlot(self, slot: Slot):
        self.lslots.remove(slot)

    def addSlot(self, slot: Slot):
        self.lslots.append(slot)
        id = self.next_id_slot
        self.next_id_slot += 1
        return id

    def destroy(self):
        self.tl_master.removeTile(self)

    def __str__(self):
        spriteName = self.spriteName if self.spriteName is not None else ""
        return f"{self.id},{spriteName};"


class TileFile:
    VERSION = "1"
    tiles: List[Tile]
    slotTypes: List[SlotType]
    meepleTypes: List[Meeples]

    def getActive(self):
        if self.valid == 0:
            return self
        else:
            return self.nfile

    def __init__(self, other=None):
        self.Comptabilities = SlotComptabilities()
        self.tileMeepleComptabilities = MeeplesSlotTypeComptabilities()

        self.tiles = []
        self.slotTypes = []
        self.meepleTypes = []

        self.nfile = other
        self.valid = 0

    def reset(self):
        self.tiles.clear()
        self.slotTypes.clear()
        self.meepleTypes.clear()

        self.Comptabilities.reset()
        self.tileMeepleComptabilities.reset()

    def saveNextId(self):
        self.saved_ids = (
            Tile.NEXT_ID,
            SlotType.NEXTID,
            Meeples.NEXT_ID,
        )
        Tile.NEXT_ID = 0
        SlotType.NEXTID = 0
        Meeples.NEXT_ID = 0

    def restoreNextId(self):
        Tile.NEXT_ID,
        SlotType.NEXTID,
        Meeples.NEXT_ID = self.saved_ids

    def getTiles(self):
        if self.valid == 0:
            return self.tiles
        else:
            return self.nfile.tiles

    def getSlotType(self):
        if self.valid == 0:
            return self.slotTypes
        else:
            return self.nfile.slotTypes

    def getMeepleTypes(self):
        if self.valid == 0:
            return self.meepleTypes
        else:
            return self.nfile.meepleTypes

    def getComptabilities(self):
        if self.valid == 0:
            return self.Comptabilities
        else:
            return self.nfile.Comptabilities

    def getMeepleComptabilities(self):
        if self.valid == 0:
            return self.tileMeepleComptabilities
        else:
            return self.nfile.tileMeepleComptabilities

    def addTile(self, tile):
        if self.valid == 1:
            return self.nfile.addTile(tile)
        self.tiles.append(tile)

    def removeTile(self, tile):
        if self.valid == 1:
            return self.nfile.removeTile(tile)
        self.tiles.remove(tile)

    def addMeeple(self, meeple):
        if self.valid == 1:
            return self.nfile.addMeeple(meeple)
        self.meepleTypes.append(meeple)

    def removeMeeple(self, meeple):
        if self.valid == 1:
            return self.nfile.removeMeeple(meeple)
        self.meepleTypes.remove(meeple)

    def addSlotType(self, slt_type):
        if self.valid == 1:
            return self.nfile.addSlotType(slt_type)
        self.slotTypes.append(slt_type)

    def removeSlotType(self, slt_type):
        if self.valid == 1:
            return self.nfile.removeSlotType(slt_type)
        self.slotTypes.remove(slt_type)

    def save(self, filename):
        if self.valid == 1:
            return self.nfile.save(filename)
        if filename in "" or ".png" in filename or ".py" in filename:
            return
        if self.VERSION == "1":

            def join(cl):
                res = ""
                for c in cl:
                    res += c
                return res + "\n"

            with open(filename, "w") as file:
                file.write("#Version\n")
                file.writelines(self.VERSION + "\n")
                file.write("#Slot type\n")
                file.write(join([str(l) for l in self.slotTypes]))
                file.write("#Meeple type\n")
                file.write(join([str(l) for l in self.meepleTypes]))
                file.write("#Tiles\n")
                file.write(join([str(l) for l in self.tiles]))
                file.write("#Slot position\n")
                file.write(join([str(l) for c in self.tiles for l in c.lslots]))
                file.write("#Slot type comptability\n")
                file.write(str(self.Comptabilities))
                file.write("#Meeple comptability\n")
                file.write(str(self.tileMeepleComptabilities))

    def load(self, filename):
        res = False
        if self.nfile is None:
            self.nfile = TileFile(self)

        with open(filename, "r") as file:
            while True:
                line = file.readline()
                if line == "":
                    break
                elif line[0] == "#":
                    continue
                elif line[0] == "1":
                    if self.valid == 0:
                        nfile = self.nfile
                    else:
                        nfile = self
                    self.valid = (self.valid + 1) % 2
                    self.saveNextId()
                    e = self._loadV1(file, nfile)
                    if e is not True:
                        self.restoreNextId()
                        self.valid = (self.valid + 1) % 2
                        print(e.__repr__(), file=ERR_OUT)
                    else:
                        nfile.nfile.reset()
                        res = True
        return res

    def _loadV1(self, file: BufferedRandom, ntiles):
        phase = "SlotType"
        valid = True
        tile_map = {}
        slot_type_map = {}
        meeple_map = {}

        while True:
            l = file.readline()
            if l == "":
                return valid
            l = l[:-1]
            if len(l) > 0 and l[0] == "#":
                continue
            elif phase == "Tiles":
                data = l.split(";")
                for arg in data:
                    if len(arg) > 0:
                        try:
                            id, spriteName = arg.split(",")
                            id = int(id)
                        except Exception as e:
                            return phase + " : " + str(e) + " (" + arg + ")"
                        t = Tile(ntiles, spriteName, False)
                        t.setId(id)
                        tile_map[id] = t
                phase = "SlotPos"
            elif phase == "SlotType":
                data = l.split(";")
                for arg in data:
                    if len(arg) > 0:
                        try:
                            id, spriteName, name = arg.split(",")
                            id = int(id)
                        except Exception as e:
                            return phase + " : " + str(e) + " ( " + arg + ")"
                        t = SlotType(ntiles, name, spriteName)
                        t.setId(id)
                        slot_type_map[id] = t
                phase = "MeepleType"
            elif phase == "MeepleType":
                data = l.split(";")
                for arg in data:
                    if len(arg) > 0:
                        try:
                            id, spriteName, name = arg.split(",")
                            id = int(id)
                        except Exception as e:
                            return phase + " : " + str(e) + " ( " + arg + ")"
                        t = Meeples(ntiles, name, spriteName)
                        t.setId(id)
                        meeple_map[id] = t
                phase = "Tiles"
            elif phase == "SlotPos":
                data = l.split(";")
                index = 0
                while index < len(data):
                    arg = data[index]
                    if len(arg) > 0:
                        try:
                            id_tuile, id_slot1, zone, mask_name, x, y = arg.split(",")
                            id_tuile, id_slot1 = int(id_tuile), int(id_slot1)
                            zone = slot_type_map[int(zone)] if zone != "-1" else None
                            # print("ZONE", zone, "|", arg)
                            x, y = float(x), float(y)
                            slot = Slot(tile_map[id_tuile], (x, y), zone, mask_name)
                        except Exception as e:
                            raise e
                            return phase + " : " + str(e) + " ( " + arg + ")"
                        slot.setId(id_slot1)
                        index += 1
                        while index < len(data):
                            arg = data[index]
                            if len(arg) > 0:
                                arg = arg.split(",")
                                if len(arg) == 4 and arg[0] == "pos":
                                    _, id_tuile_bis, id_slot_bis, dir = arg
                                    id_tuile_bis = int(id_tuile_bis)
                                    id_slot_bis = int(id_slot_bis)
                                    if (
                                        id_tuile_bis != id_tuile
                                        or id_slot_bis != id_slot1
                                    ):
                                        return (
                                            str(arg)
                                            + f"({id_tuile_bis} == {id_tuile} ? {id_tuile_bis != id_tuile}) ({id_slot_bis} == {id_slot1} ? {id_slot_bis != id_slot1}) "
                                        )
                                    else:
                                        slot.dir[dir] = True
                                    index += 1
                                elif len(arg) == 3 and arg[0] == "link":
                                    _, id_a, id_b = arg
                                    id_a = int(id_a)
                                    id_b = int(id_b)
                                    if id_a != id_slot1:
                                        return f"Error: should be {id_slot1} in first id instead of {id_a}"
                                    else:
                                        slot.slot_links.append(id_b)
                                    index += 1
                                else:
                                    break
                            else:
                                break
                    else:
                        index += 1
                phase = "SlotComp"
            elif phase == "SlotComp":
                data = l.split(";")
                for arg in data:
                    if len(arg) > 0:
                        try:
                            id_slot0, id_slot1 = arg.split(",")
                            id_slot0, id_slot1 = int(id_slot0), int(id_slot1)
                            slot0, slot1 = (
                                slot_type_map[id_slot0],
                                slot_type_map[id_slot1],
                            )
                        except Exception as e:
                            return phase + " : " + str(e) + " ( " + arg + ")"
                        slot0.addComptabilities(slot1)
                phase = "MeepleComp"
            elif phase == "MeepleComp":
                data = l.split(";")
                for arg in data:
                    if len(arg) > 0:
                        try:
                            id_meeple, id_slot = arg.split(",")
                            id_meeple, id_slot = int(id_meeple), int(id_slot)
                            meeple, slot = meeple_map[id_meeple], slot_type_map[id_slot]
                        except Exception as e:
                            return phase + " : " + str(e) + " ( " + arg + ")"
                        meeple.addComptabilities(slot)
                phase = ""
                valid = True
            else:
                valid = False


if __name__ == "__main__":
    import tkinter as tk
    from tkinter import ttk
    from tkinter import filedialog
    from PIL import ImageTk, ImageDraw, ImageFilter

    DIR_COORD = {}

    class RadioButtonGroup(tk.Frame):
        def __init__(self, master):
            tk.Frame.__init__(self, master)

            self.selected = 0
            self.buttons = []

        def select_button(self, b, action=None):
            try:
                index_sel = self.buttons.index(b)
            except ValueError:
                raise Exception("Button not in radiobuttongroup tried to be selected")
            for i in range(len(self.buttons)):
                if i != index_sel:
                    self.buttons[i].config(bg=self.cget("bg"))
                else:
                    self.buttons[i].config(bg="#777777")
            if action is not None:
                action()

        def bind_accelerator(self, master, keys, button):
            master.bind(keys, lambda x: button.true_action())

        def addText(self, text, action):
            true_action = lambda: self.select_button(b, action)
            b = tk.Button(self, text=text, command=true_action)
            b.true_action = true_action
            b.pack(side=tk.TOP, expand=True, fill=tk.X)
            self.buttons.append(b)
            if len(self.buttons) == 1:
                self.select_button(b, None)
            return b

        def addImage(self, filename, action):
            true_action = lambda: self.select_button(b, action)
            icon = ImageTk.PhotoImage(Image.open(filename))
            b = tk.Button(self, image=icon, command=true_action)
            b.true_action = true_action
            b.icon = icon
            b.pack(side=tk.TOP, expand=True, fill=tk.X)
            self.buttons.append(b)
            if len(self.buttons) == 1:
                self.select_button(b, None)
            return b

    class ScrollableFrame(tk.Frame):
        def __init__(self, parent):

            tk.Frame.__init__(self, parent)
            self.canvas = tk.Canvas(self, borderwidth=0, background="#eeeeee")
            self.frame = tk.Frame(self.canvas, background="#eeeeee")
            self.vsb = tk.Scrollbar(self, orient="vertical", command=self.canvas.yview)
            self.canvas.configure(yscrollcommand=self.vsb.set)

            self.vsb.pack(side="right", fill="y")
            self.canvas.pack(side="left", fill="both", expand=True)
            self.windows_item = self.canvas.create_window(
                (4, 4), window=self.frame, anchor="nw", tags="self.frame"
            )

            self.canvas.bind("<Configure>", self.onFrameConfigure)

            self.lwidgets = []

        def onFrameConfigure(self, event):
            """Reset the scroll region to encompass the inner frame"""
            self.canvas.configure(scrollregion=self.canvas.bbox("all"))

            canvas_width = event.width
            self.canvas.itemconfig(self.windows_item, width=canvas_width)

        def add(self, widget):
            self.lwidgets.append(widget)

            widget.pack(side=tk.TOP, expand=True, fill=tk.X)

        def clear(self):
            for widget in self.lwidgets:
                widget.clear()
                widget.destroy()
            self.lwidgets.clear()
            self.canvas.configure(scrollregion=self.canvas.bbox("all"))

        def jump_to_widget(self, widget):
            if self.vsb.winfo_height() > self.canvas.winfo_height():
                pos = widget.winfo_rooty() - self.vsb.winfo_rooty()
                height = self.vsb.winfo_height()
                self.canvas.yview_moveto(pos / height)

    class Editor(tk.Frame):
        def __init__(self, master, tiles):
            tk.Frame.__init__(self, master)
            self.tiles = tiles

    class TableFrame(tk.Frame):
        LINKED = 0
        CUSTOM = 1

        def __init__(self, master, w=900, h=600, rect_size=40, mode=0):
            tk.Frame.__init__(self, master)
            self.w = w
            self.h = h
            self.rect_size = rect_size

            self.can_row = tk.Canvas(
                self, width=w, height=rect_size, bg="#EEEEFF", highlightthickness=0
            )
            self.can_row.grid(row=0, column=1)

            self.can_column = tk.Canvas(
                self, height=h, width=rect_size, bg="#eeeeff", highlightthickness=0
            )
            self.can_column.grid(row=1, column=0)

            self.can_full = tk.Canvas(
                self, height=h, width=w, bg="#EEEEEE", highlightthickness=0
            )
            self.can_full.grid(row=1, column=1)

            self.vsb = tk.Scrollbar(self, orient=tk.VERTICAL, command=self.displaceY)
            self.vsb.grid(row=1, column=2, sticky="ns")
            self.hsb = tk.Scrollbar(self, orient=tk.HORIZONTAL, command=self.displaceX)
            self.hsb.grid(row=2, column=1, sticky="ew")

            self.can_full.config(
                yscrollcommand=self.vsb.set, xscrollcommand=self.hsb.set
            )

            self.can_full.configure(scrollregion=self.can_full.bbox("all"))
            self.can_row.configure(scrollregion=self.can_row.bbox("all"))
            self.can_column.configure(scrollregion=self.can_column.bbox("all"))

            self.callback_row = []
            self.callback_column = []
            self.callback_full = []

            self.mode = mode

            self.nrow = 0
            self.ncolumn = 0

            self.coord_to_case = []
            self.coord_to_column = []
            self.coord_to_row = []

            self.table_size = 0

        def clear(self):
            self.can_full.delete("all")
            self.can_row.delete("all")
            self.can_column.delete("all")

            for l in self.coord_to_case:
                for o in l:
                    o.destroy()
            for o in self.coord_to_column:
                o.destroy()
            for o in self.coord_to_row:
                o.destroy()

            self.callback_column.clear()
            self.callback_full.clear()
            self.callback_row.clear()

            self.coord_to_case.clear()
            self.coord_to_row.clear()
            self.coord_to_column.clear()

            self.table_size = 0
            self.nrow = 0
            self.ncolumn = 0

            self.can_full.configure(scrollregion=self.can_full.bbox("all"))
            self.can_row.configure(scrollregion=self.can_row.bbox("all"))
            self.can_column.configure(scrollregion=self.can_column.bbox("all"))

        def getCanvas(self):
            return self.can_row, self.can_column, self.can_full

        def getCase(self, cx, cy):
            return self.coord_to_case[cx][cy]

        def bindCaseToClick(self, c, r, f):
            self.coord_to_case[c][r].bind("<ButtonPress-1>", f)

        def configCase(self, c, r, **kwargs):
            self.coord_to_case[c][r].config(**kwargs)

        def getColumn(self, cy):
            return self.coord_to_column[cy]

        def getRow(self, cx):
            return self.coord_to_row[cx]

        def displaceX(self, *args):
            self.can_full.xview(*args)
            self.can_row.xview(*args)

        def displaceY(self, *args):
            self.can_full.yview(*args)
            self.can_column.yview(*args)

        def _newWindowIn(self, can, line, x, y, bg=""):
            nf = tk.Frame(
                can,
                borderwidth=0,
                highlightthickness=0,
                bg=bg,
                width=self.rect_size - 1,
                height=self.rect_size - 1,
            )
            nf.pack_propagate(0)
            line.append(nf)
            nf.id_w = can.create_window(
                x + 1,
                y + 1,
                anchor="nw",
                window=nf,
                width=self.rect_size - 1,
                height=self.rect_size - 1,
            )
            return nf

        def addColumnAndRow(self, bg="red", bindings=None):
            if self.mode == TableFrame.LINKED:
                ic = self.table_size
                c = ic * self.rect_size
                self.coord_to_case.append([])
                for il in range(self.table_size):
                    l = il * self.rect_size
                    bgg = bindings() if bindings is not None else bg
                    a = self._newWindowIn(
                        self.can_full, self.coord_to_case[ic], c, l, bgg
                    )
                    b = self._newWindowIn(
                        self.can_full, self.coord_to_case[il], l, c, bgg
                    )
                    a.brother = b
                    b.brother = a
                bgg = bindings() if bindings is not None else bg
                a = self._newWindowIn(self.can_full, self.coord_to_case[ic], c, c, bgg)
                a.brother = a

                self.can_column.create_rectangle(
                    0, c, self.rect_size, c + self.rect_size
                )
                a = self._newWindowIn(self.can_column, self.coord_to_column, 0, c)

                self.can_row.create_rectangle(c, 0, c + self.rect_size, self.rect_size)
                b = self._newWindowIn(self.can_row, self.coord_to_row, c, 0)

                a.brother = b
                b.brother = a

                self.can_row.configure(scrollregion=self.can_row.bbox("all"))
                self.can_column.configure(scrollregion=self.can_column.bbox("all"))
                self.table_size += 1
                self.drawGrid()
                return a, b

        def addRow(self, bg="red", bindings=None):
            if self.mode == TableFrame.CUSTOM:
                ir = self.nrow
                c = ir * self.rect_size
                self.coord_to_case.append([])
                for it in range(self.ncolumn):
                    t = it * self.rect_size
                    bgg = bindings() if bindings is not None else bg
                    self._newWindowIn(self.can_full, self.coord_to_case[ir], t, c, bgg)
                self.can_column.create_rectangle(
                    0, c, self.rect_size, c + self.rect_size
                )
                a = self._newWindowIn(self.can_column, self.coord_to_row, 0, c)
                self.can_row.configure(scrollregion=self.can_row.bbox("all"))
                self.can_column.configure(scrollregion=self.can_column.bbox("all"))
                self.nrow += 1
                self.drawGrid()
                return a

        def addColumn(self, bg="red", bindings=None):
            if self.mode == TableFrame.CUSTOM:
                ic = self.ncolumn
                c = ic * self.rect_size
                for it in range(self.nrow):
                    t = it * self.rect_size
                    bgg = bindings() if bindings is not None else bg
                    self._newWindowIn(self.can_full, self.coord_to_case[it], c, t, bgg)
                self.can_row.create_rectangle(c, 0, c + self.rect_size, self.rect_size)
                b = self._newWindowIn(self.can_row, self.coord_to_column, c, 0)

                self.can_row.configure(scrollregion=self.can_row.bbox("all"))
                self.can_column.configure(scrollregion=self.can_column.bbox("all"))
                self.ncolumn += 1
                self.drawGrid()
                return b

        def drawGrid(self):
            self.can_full.delete("grid")
            if self.mode == TableFrame.LINKED:
                mc = self.table_size * self.rect_size
                for i in range(self.table_size):
                    lc = self.rect_size * i
                    self.can_full.create_line(0, lc, mc, lc, tag="grid")
                    self.can_full.create_line(lc, 0, lc, mc, tag="grid")
            self.can_full.configure(scrollregion=self.can_full.bbox("all"))

        def clickRow(self, ev):
            c = ev.x // self.rect_size
            if (
                c < 0
                or (c >= self.table_size and self.mode == self.LINKED)
                or (c >= self.nrow and self.mode == self.CUSTOM)
            ):
                c = None
            for f in self.callback_row:
                f(c)

        def clickColumn(self, ev):
            c = ev.y // self.rect_size
            if (
                c < 0
                or (c >= self.table_size and self.mode == self.LINKED)
                or (c >= self.ncolumn and self.mode == self.CUSTOM)
            ):
                c = None
            for f in self.callback_column:
                f(c)

        def clickFull(self, ev):
            x, y = ev.x, ev.y
            cx = ev.x // self.rect_size
            cy = ev.y // self.rect_size
            if (
                cx < 0
                or (cx >= self.table_size and self.mode == self.LINKED)
                or (cx >= self.ncolumn and self.mode == self.CUSTOM)
                or cy < 0
                or (cy >= self.table_size and self.mode == self.LINKED)
                or (cy >= self.ncolumn and self.mode == self.CUSTOM)
            ):
                cx = None
                cy = None
            for f in self.callback_full:
                f(cx, cy)

    class ComptabilityEditor(Editor):
        def __init__(self, master, tiles, table_type):
            Editor.__init__(self, master, tiles)
            self.config(bg="white")

            self.selected_type = None
            self.id_var = tk.StringVar()
            self.name_var = tk.StringVar()
            self.name_var.trace_add("write", self.traceNameChange)

            param_frame = tk.Frame(self)

            self.id_lab = tk.Label(param_frame, textvariable=self.id_var)
            self.id_lab.grid(row=0, column=1)

            self.name = tk.Entry(
                param_frame, textvariable=self.name_var, state=tk.DISABLED
            )
            self.name.grid(row=0, column=2, sticky="ew")
            param_frame.grid_columnconfigure(2, weight=1)

            icon_frame = tk.Frame(
                param_frame, width=40, height=40, relief=tk.SUNKEN, borderwidth=4
            )
            self.icon = tk.Label(
                icon_frame,
                bg="#999999",
            )
            self.icon.bind("<ButtonPress-1>", self.chooseIcon)
            icon_frame.pack_propagate(0)
            self.icon.pack(expand=True, fill=tk.BOTH)
            icon_frame.grid(row=0, column=3)

            self.icon_remove = tk.Button(
                param_frame, text="X", state=tk.DISABLED, command=self.removeIcon
            )
            self.icon_remove.grid(row=0, column=4, sticky="w")

            self.add_bt = tk.Button(param_frame, text="+")
            self.add_bt.grid(row=0, column=0)

            param_frame.pack(side=tk.TOP, expand=True, fill=tk.X)

            self.tab_frame = TableFrame(self, mode=table_type)
            self.tab_frame.pack(side=tk.BOTTOM)
            self.columns_to_slot = []

            self.iter_index = 0
            self.iter_creation = None

        def initIter(self, iter_creation=None):
            self.iter_index = 0
            self.iter_creation = iter_creation

        def nextIter(self):
            if (
                self.iter_index == len(self.columns_to_slot)
                and self.iter_creation is not None
            ):
                return self.iter_creation
            item = self.columns_to_slot[self.iter_index].pointed_object
            self.iter_index += 1
            return item

        def clear(self):
            for o in self.columns_to_slot:
                o.clear()
            self.columns_to_slot.clear()
            self.tab_frame.clear()
            self.selectByColumn(None)

        def removeIcon(self):
            if self.selected_type is not None:
                self.selected_type.setSprite(self.icon, self.icon_remove)

        def chooseIcon(self, *args):
            if self.selected_type is not None:
                self.selected_type.setSprite(
                    self.icon,
                    self.icon_remove,
                    relpath(filedialog.askopenfilename()),
                )

        def selectByColumn(self, sti):
            if sti is not None:
                self.selected_type = sti
                self.selected_type.selected(
                    self.id_var, self.name_var, self.icon, self.icon_remove
                )
                self.name.config(state=tk.NORMAL)
            else:
                self.selected_type = None
                self.id_var.set("")
                self.name_var.set("")
                self.icon.config(image="")
                self.name.config(state=tk.DISABLED)
                self.icon_remove.config(state=tk.DISABLED)

        def traceNameChange(self, *args):
            if self.selected_type is not None:
                self.selected_type.setName(self.name_var.get())

        def toggleState(e0, e1):
            if e0.connectedTo(e1):
                e0.removeComptabilities(e1)
                return "red"

            else:
                e0.addComptabilities(e1)
                return "green"

    class SlotTypeEditor(ComptabilityEditor):
        def __init__(self, master, tiles):
            ComptabilityEditor.__init__(self, master, tiles, TableFrame.LINKED)
            self.meeple_editor = None
            self.tile_editor = None
            self.add_bt.config(command=self.newSlotType)

        def setTileEditor(self, te):
            self.tile_editor = te

        def setMeepleEditor(self, meeple_editor):
            self.meeple_editor = meeple_editor

        def addSlotType(self, ns):
            self.initIter(ns)
            frames = list(
                self.tab_frame.addColumnAndRow(
                    bindings=(
                        lambda: "green" if ns.connectedTo(self.nextIter()) else "red"
                    )
                )
            )
            self.meeple_editor.initIter()
            frames.append(
                self.meeple_editor.tab_frame.addColumn(
                    bindings=(
                        lambda: "green"
                        if ns.connectedTo(self.meeple_editor.nextIter())
                        else "red"
                    )
                )
            )
            sti = SlotTypeIndicator(frames, self, ns)
            self.tile_editor.addSlotType(sti)
            self.columns_to_slot.append(sti)
            self.selectByColumn(sti)

            c = self.tab_frame.table_size - 1
            for l, ps in enumerate(self.columns_to_slot):
                ps = ps.pointed_object

                def callback(event, e0=ns, e1=ps):
                    bg = SlotTypeEditor.toggleState(e0, e1)
                    event.widget.config(bg=bg)
                    event.widget.brother.config(bg=bg)

                self.tab_frame.bindCaseToClick(c, l, callback)
                if l != c:
                    self.tab_frame.bindCaseToClick(l, c, callback)
            for l, ps in enumerate(self.meeple_editor.columns_to_slot):
                ps = ps.pointed_object

                def callback(event, e0=ps, e1=ns):
                    bg = SlotTypeEditor.toggleState(e0, e1)
                    event.widget.config(bg=bg)

                self.meeple_editor.tab_frame.bindCaseToClick(l, c, callback)
                if ps.name == "Meeple" and not ps.connectedTo(ns):
                    self.meeple_editor.tab_frame.getCase(l, c).config(
                        bg=SlotTypeEditor.toggleState(ps, ns)
                    )

        def newSlotType(self):
            ns = SlotType(self.tiles)
            ns.addComptabilities(ns)
            self.addSlotType(ns)

    class TypeIndicator:
        def __init__(
            self,
            frames,
            master: ComptabilityEditor,
            pointed_object,
        ):
            self.master = master
            self.pointed_object = pointed_object

            self.icon_ptimg = None
            self.icons = []
            callback = lambda x: master.selectByColumn(self)
            for frame in frames:
                icon = tk.Label(
                    frame,
                )
                icon.bind("<Button-1>", callback)
                icon.pack(expand=True, fill=tk.BOTH)
                self.icons.append(icon)
            self.updateImg()

        def addFollower(self, label):
            self.icons.append(label)
            if self.icon_ptimg is not None:
                label.config(image=self.icon_ptimg)

        def clear(self):
            for o in self.icons:
                o.destroy()

        def selected(
            self,
            idStr: tk.StringVar,
            nameStr: tk.StringVar,
            icon: tk.Label,
            icon_bt: tk.Button,
        ):
            idStr.set(f"Id : {self.pointed_object.id}")
            nameStr.set(self.pointed_object.name)

            sprt = self.pointed_object.sprite
            if sprt is None:
                icon.config(image="")
                icon_bt.config(state=tk.DISABLED)
            else:
                icon.config(image=self.icon_ptimg)
                icon_bt.config(state=tk.NORMAL)

        def setName(self, name):
            self.pointed_object.name = name

        def setSprite(self, icon_lbl, icon_bt, spriteName=None):
            if spriteName is None:
                self.pointed_object.spriteName = None
                self.pointed_object.sprite = None
                self.updateImg()
                icon_bt.config(state=tk.DISABLED)
                icon_lbl.config(image="")
            else:
                try:
                    img = Image.open(spriteName)
                    self.pointed_object.sprite = img
                    self.pointed_object.spriteName = spriteName
                except:
                    return
                self.updateImg()
                icon_bt.config(state=tk.NORMAL)
                icon_lbl.config(image=self.icon_ptimg)

        def updateImg(self):
            sprt = self.pointed_object.sprite
            if sprt is not None:
                s = self.master.tab_frame.rect_size - 2
                sprt = ImageTk.PhotoImage(sprt.resize((s, s)))
                self.icon_ptimg = sprt
                for icon in self.icons:
                    icon.config(image=sprt)
            else:
                for icon in self.icons:
                    icon.config(image="")
                self.icon_ptimg = None

    class MeeplesEditor(ComptabilityEditor):
        def __init__(self, master, tiles):
            ComptabilityEditor.__init__(self, master, tiles, TableFrame.CUSTOM)
            self.slt_editor = None
            self.add_bt.config(command=self.newMeeple)

        def setSlotTypeEditor(self, slt_editor):
            self.slt_editor = slt_editor

        def newMeeple(self):
            mt = Meeples(self.tiles)
            self.addMeeple(mt)

        def addMeeple(self, mt):
            c = self.tab_frame.nrow
            self.slt_editor.initIter()
            frames = [
                self.tab_frame.addRow(
                    bindings=(
                        lambda: "green"
                        if mt.connectedTo(self.slt_editor.nextIter())
                        else "red"
                    )
                )
            ]
            mti = MeeplelIndicator(frames, self, mt)
            self.columns_to_slot.append(mti)
            self.selectByColumn(mti)

            for l, pc in enumerate(self.slt_editor.columns_to_slot):
                pc = pc.pointed_object

                def callback(event, e0=mt, e1=pc):
                    bg = ComptabilityEditor.toggleState(e0, e1)
                    event.widget.config(bg=bg)

                self.tab_frame.bindCaseToClick(c, l, callback)

    class MeeplelIndicator(TypeIndicator):
        def __init__(self, frames, master: SlotTypeEditor, meeple_type: Meeples):
            super().__init__(frames, master, meeple_type)

    class SlotTypeIndicator(TypeIndicator):
        def __init__(self, frames, master: SlotTypeEditor, slot_type: SlotType):
            super().__init__(frames, master, slot_type)

    class TileIndicator(tk.Label):
        def __init__(self, fen, master, tile: Tile):
            self.icon = None
            self.tile = tile
            self.masterT = master
            tk.Label.__init__(
                self,
                fen,
                text=f"Tile {tile.id}",
                compound=tk.LEFT,
            )
            self.bind("<ButtonPress-1>", self.select)
            self._updateSprite()

            self.islots = []
            self.selected = False
            slot_maps = {}
            for slot in tile.lslots:
                si = SlotIndicator(self.masterT, slot)
                slot_maps[slot.id] = si
                self.islots.append(si)
                si.destroy()
            SlotIndicator.treatLink(slot_maps)

        def __bool__(self):
            return bool(self.tile)

        def setSpriteName(self, spriteName):
            try:
                self.tile.setSprite(spriteName)
                self._updateSprite()
            except FailedCreation:
                pass

        def _updateSprite(self):
            if self.tile.sprite is not None:
                self.icon = ImageTk.PhotoImage(self.tile.sprite.resize((50, 50)))
                self.config(image=self.icon)
                self.masterT.drawTile()

        def clear(self):
            for o in self.islots:
                o.clear()
            self.islots.clear()

        def addSlot(self, slot):
            self.islots.append(SlotIndicator(self.masterT, slot))
            if not self.selected:
                self.islots[-1].destroy()
            return self.islots[-1]

        def select(self, event=None):
            self.masterT.selectTile(self)

        def enableSel(self):
            for isl in self.islots:
                isl.moveTo()
                isl.draw()
            self.config(bg="#9999ff")
            self.selected = True

        def disableSel(self):
            for isl in self.islots:
                isl.destroy()
            self.config(bg="#ffffff")
            self.selected = False

    class SlotIndicator:
        class SlotLink:
            def __init__(self, master, a, b):
                self.master = master
                if a.slot.id >= b.slot.id:
                    a, b = b, a
                self.a = a
                self.b = b
                self.repre = None

            def draw(self):
                if self.repre is None:
                    self.repre = self.master.drawLine(
                        self.a.x, self.a.y, self.b.x, self.b.y
                    )
                    self.master.iconfig(self.repre, fill="#FF5C20")

            def move(self, target=None, x=None, y=None):
                if self.repre is not None:
                    if target == self.a:
                        c = (x, y) + self.b.center()
                    elif target == self.b:
                        c = (x, y) + self.a.center()
                    else:
                        c = self.a.center() + self.b.center()
                    self.master.imove(self.repre, *c)

            def destroy(self):
                if self.repre is not None:
                    self.master.idelete(self.repre)
                    self.repre = None

            def isEdge(self, a, b):
                if a < b:
                    return self.a.slot.id == a and self.b.slot.id == b
                else:
                    return self.a.slot.id == b and self.b.slot.id == a

        SLOT_SIZE = 30
        ENABLE_COL = "#1144ff"
        DISABLE_COL = "#000000"
        MASK = None
        DEFAULT_IMAGE = None
        ZONE_IMAGE = {}

        def createMask():
            mask = Image.new("L", (SlotIndicator.SLOT_SIZE, SlotIndicator.SLOT_SIZE), 0)
            draw = ImageDraw.Draw(mask)
            draw.ellipse(
                (2, 2, SlotIndicator.SLOT_SIZE - 2, SlotIndicator.SLOT_SIZE - 2),
                fill=255,
            )
            SlotIndicator.MASK = mask.filter(ImageFilter.GaussianBlur(2))

        def reset_zone():
            SlotIndicator.ZONE_IMAGE.clear()

        def defaultImage():
            if SlotIndicator.DEFAULT_IMAGE is None:
                img0 = Image.new(
                    "RGBA",
                    (SlotIndicator.SLOT_SIZE, SlotIndicator.SLOT_SIZE),
                    (255, 255, 255, 255),
                )
                img1 = Image.new(
                    "RGBA",
                    (SlotIndicator.SLOT_SIZE, SlotIndicator.SLOT_SIZE),
                    (255, 255, 255, 0),
                )
                SlotIndicator.DEFAULT_IMAGE = ImageTk.PhotoImage(
                    Image.composite(img0, img1, mask=SlotIndicator.MASK)
                )
            return SlotIndicator.DEFAULT_IMAGE

        def _getImageForZone(zone):
            if zone is None:
                return SlotIndicator.defaultImage()
            if zone.id not in SlotIndicator.ZONE_IMAGE:
                if zone.sprite is None:
                    return SlotIndicator.defaultImage()
                img1 = Image.new(
                    "RGBA",
                    (SlotIndicator.SLOT_SIZE, SlotIndicator.SLOT_SIZE),
                    (255, 255, 255, 0),
                )
                img0 = zone.sprite.resize(
                    (SlotIndicator.SLOT_SIZE, SlotIndicator.SLOT_SIZE)
                )
                SlotIndicator.ZONE_IMAGE[zone.id] = ImageTk.PhotoImage(
                    Image.composite(img0, img1, mask=SlotIndicator.MASK)
                )
            return SlotIndicator.ZONE_IMAGE[zone.id]

        def __init__(self, master, slot: Slot) -> None:
            if SlotIndicator.MASK is None:
                SlotIndicator.createMask()
            if slot.zone is not None:
                self.mask = SlotIndicator._getImageForZone(slot.zone)
            else:
                self.mask = SlotIndicator.defaultImage()

            self.selected = False
            self.master = master
            self.slot = slot

            tile = slot.tile
            if not tile:
                self.x, self.y = 0, 0
            else:
                self.x, self.y = (
                    slot.x * tile.sprite.width,
                    slot.y * tile.sprite.height,
                )
            self.node = GraphLinker.Node((0, 0, 0, 0), "Slot", self)
            self.node.updateZone_center(self.x, self.y, self.SLOT_SIZE)
            self.lines = {}
            for dir in DIR:
                self.lines[dir] = None
            self.slot_links = []
            self.drawned = False
            self.draw()

        def treatLink(slots_ind):
            for id_a in slots_ind:
                slt = slots_ind[id_a]
                for id_b in slt.slot.slot_links:
                    if id_a < id_b:
                        l = SlotIndicator.SlotLink(slt.master, slt, slots_ind[id_b])
                        slt.slot_links.append(l)
                        slots_ind[id_b].slot_links.append(l)

        def changeSlotType(self, slt_type: SlotType):
            self.slot.zone = slt_type
            self.mask = SlotIndicator._getImageForZone(slt_type)
            if self.repre_img is not None:
                self.master.iconfig(self.repre_img, image=self.mask)

        def draw(self):
            if self.slot.tile and not self.drawned:
                col = self.ENABLE_COL if self.selected else self.DISABLE_COL
                self.repre_img = self.master.drawImage(self.x, self.y, image=self.mask)
                self.repre_outline = self.master.drawCircle(
                    self.x, self.y, self.SLOT_SIZE // 2
                )
                self.master.iconfig(self.repre_outline, width=2, outline=col)

                for dir in DIR:
                    if self.slot.dir[dir]:
                        l = self.master.drawLine(
                            self.x, self.y, *DIR_COORD[dir].center()
                        )
                        self.master.iconfig(l, width=2, tag="graph", fill=col)
                        self.lines[dir] = l

                for l in self.slot_links:
                    l.draw()

                self.node.enable()
                self.drawned = True

        def destroy(self):
            if self.slot.tile and self.drawned:
                self.master.idelete(self.repre_img)
                self.master.idelete(self.repre_outline)

                for dir in DIR:
                    l = self.lines[dir]
                    if l is not None:
                        self.master.idelete(l)
                    self.lines[dir] = None

                for l in self.slot_links:
                    l.destroy()

                self.repre_img = None
                self.repre_outline = None
                self.node.disable()
                self.drawned = False

        def clear(self):
            self.destroy()

        def moveRepreTo(self, x, y):
            self.master.imove(self.repre_img, x, y)
            self.master.imove(self.repre_outline, x, y, self.SLOT_SIZE // 2)
            for dir in DIR:
                l = self.lines[dir]
                if l is not None:
                    c = DIR_COORD[dir].center()
                    self.master.imove(l, x, y, *c)
            for l in self.slot_links:
                l.move(self, x, y)

        def moveTo(self, x=None, y=None):
            tile = self.slot.tile
            if tile:
                if (
                    x is not None
                    and y is not None
                    and 0 <= x < tile.sprite.width
                    and 0 <= y < tile.sprite.height
                ):
                    self.slot.x, self.slot.y = (
                        x / tile.sprite.width,
                        y / tile.sprite.height,
                    )
                else:
                    x, y = (
                        self.slot.x * tile.sprite.width,
                        self.slot.y * tile.sprite.height,
                    )
            else:
                x, y = 0, 0
            self.x, self.y = x, y
            self.node.updateZone_center(x, y, self.SLOT_SIZE // 2)
            if self.drawned:
                for dir in DIR:
                    l = self.lines[dir]
                    if l is not None:
                        c = DIR_COORD[dir].center()
                        self.master.imove(l, x, y, *c)
                self.master.imove(self.repre_img, x, y)
                self.master.imove(self.repre_outline, x, y, self.SLOT_SIZE // 2)
            for l in self.slot_links:
                l.move()

        def center(self):
            return self.node.center()

        def enableSel(self):
            self.master.iconfig(self.repre_outline, outline=SlotIndicator.ENABLE_COL)
            for dir in DIR:
                l = self.lines[dir]
                if l is not None:
                    self.master.iconfig(l, fill=SlotIndicator.ENABLE_COL)
            self.selected = True

        def disableSel(self):
            self.master.iconfig(self.repre_outline, outline=SlotIndicator.DISABLE_COL)
            for dir in DIR:
                l = self.lines[dir]
                if l is not None:
                    self.master.iconfig(l, fill=SlotIndicator.DISABLE_COL)
            self.selected = False

        def distSqr(self, x, y):
            return (self.x - x) ** 2 + (self.y - y) ** 2

        def closeTo(self, x, y):
            return self.distSqr(x, y) <= (SlotIndicator.SLOT_SIZE) ** 2 // 2

        def connectTo(self, node, line):
            if node.spec == "Pos":
                dir = node.object
                if self.slot.dir[dir]:
                    self.slot.dir[dir] = False
                    self.master.idelete(self.lines[dir])
                    self.lines[dir] = None
                    return True
                else:
                    self.slot.dir[dir] = True
                    self.lines[dir] = line
                    col = (
                        SlotIndicator.ENABLE_COL
                        if self.selected
                        else SlotIndicator.DISABLE_COL
                    )
                    self.master.iconfig(line, fill=col)
                    return False
            elif node.spec == "Slot":
                source = self
                target = node.object
                source_slt = source.slot
                target_slt = target.slot
                source_id = source_slt.id
                target_id = target_slt.id
                # print(source_id, target_id)
                if target_id == source_id:
                    return True

                if target_id in source_slt.slot_links:
                    target_slt.slot_links.remove(source_id)
                    source_slt.slot_links.remove(target_id)

                    for l in (source.slot_links, target.slot_links):
                        for i in range(len(l) - 1, -1, -1):
                            if l[i].isEdge(source_id, target_id):
                                l.pop(i).destroy()
                else:
                    target_slt.slot_links.append(source_id)
                    source_slt.slot_links.append(target_id)

                    l = SlotIndicator.SlotLink(self.master, source, target)
                    source.slot_links.append(l)
                    target.slot_links.append(l)
                    l.draw()
                # print(source_slt.slot_links, target_slt.slot_links)
                return True

    class TileParameters(Editor):
        def __init__(self, master, tiles, tl_master, byrow=3, size_rect=40):
            Editor.__init__(self, master, tiles)
            self.tl_master = tl_master

            self.byrow = byrow
            self.rect_size = size_rect
            self.lrow = []
            self.index_insert = 0
            self.index_row = 0

            tile_parameters = tk.LabelFrame(self, text="Tile", bg="#CCCCCC")
            tile_parameters.grid_columnconfigure(0, weight=1)
            tile_parameters.pack(side=tk.TOP, fill=tk.X)

            self.tile_name = tk.Label(
                tile_parameters, text="Pas de selection", justify=tk.LEFT, bg="#DDDDDD"
            )
            self.tile_name.grid(row=0, column=0, sticky="ew")
            self.bt_changesprite = tk.Button(
                tile_parameters,
                text="...",
                state=tk.DISABLED,
                command=self.changeSprite,
            )
            self.bt_changesprite.grid(row=0, column=1)

            slot_parameters = tk.LabelFrame(self, text="Slot", bg="#CCCCCC")
            slot_parameters.grid_columnconfigure(1, weight=1)
            slot_parameters.grid_rowconfigure(3, weight=1)
            slot_parameters.pack(side=tk.TOP, expand=True, fill=tk.BOTH)

            self.slot_type_indicator = tk.Label(
                slot_parameters, justify=tk.LEFT, bg="#CCCCCC"
            )
            self.slot_type_indicator.grid(row=0, column=0)

            self.slot_type_name = tk.Label(
                slot_parameters, text="Pas de selection", bg="#DDDDDD", justify=tk.LEFT
            )
            self.slot_type_name.grid(row=0, column=1, sticky="ew")

            fr = tk.Frame(slot_parameters, bg="#CCCCCC")
            fr.grid(column=0, row=1, columnspan=2, sticky="ew")

            slot_mask_show = tk.Button(fr, text="Show", state=tk.DISABLED)
            slot_mask_show.pack(side=tk.LEFT)
            slot_mask_show.bind("<Enter>", lambda x: tl_master.showMask())
            slot_mask_show.bind("<Leave>", lambda x: tl_master.hideMask())

            self.slot_mask = tk.Label(fr, text="", justify=tk.LEFT, bg="#DDDDDD")
            self.slot_mask.pack(side=tk.LEFT, fill=tk.X, expand=True)

            self.slot_mask_bt = tk.Button(
                fr, state=tk.DISABLED, text="...", command=self.changeMask
            )
            self.slot_mask_bt.pack(side=tk.LEFT)

            self.bt_up = tk.Button(
                slot_parameters, text="▲", command=self.upSel, state=tk.DISABLED
            )
            self.bt_up.grid(row=2, column=0, columnspan=2, sticky="ew")

            self.table_types = tk.Frame(slot_parameters)
            self.table_types.grid(row=3, column=0, columnspan=2, sticky="nwes")
            self.table_types.grid_propagate(False)

            self.bt_down = tk.Button(
                slot_parameters, text="▼", command=self.downSel, state=tk.DISABLED
            )
            self.bt_down.grid(row=4, column=0, columnspan=2, sticky="ew")

            self.reset()

        def changeSprite(self):
            if self.tl_master.selected_tile is not None:
                self.tl_master.selected_tile.setSpriteName(
                    relpath(filedialog.askopenfilename(filetypes=[("image", "*.png")]))
                )

        def changeMask(self):
            if self.tl_master.selected_slot is not None:
                self.tl_master.selected_slot.slot.setMask(
                    relpath(filedialog.askopenfilename(filetypes=[("image", "*.png")]))
                )
                self.maskSelected()

        def tileSelected(self):
            if self.tl_master.selected_tile is None:
                self.tile_name.config(text="Pas de selection")
                self.bt_changesprite.config(state=tk.DISABLED)
            else:
                tile = self.tl_master.selected_tile.tile
                self.tile_name.config(text=f"{tile.id}  :  {tile.spriteName}")
                self.bt_changesprite.config(state=tk.ACTIVE)

        def slotSelected(self):
            if self.tl_master.selected_slot is None:
                self.slot_type_name.config(text="Pas de selection")
                self.slot_type_indicator.config(image="")
            else:
                slt_type = self.tl_master.selected_slot.slot.zone
                if slt_type is None:
                    self.slot_type_name.config(text="Zone non renseignée")
                else:
                    self.slot_type_name.config(text=f"Zone: {slt_type.name}")

            self.maskSelected()

        def maskSelected(self):
            if self.tl_master.selected_slot is None:
                self.slot_mask.config(text="")
                self.slot_mask_bt.config(state=tk.DISABLED)
            else:
                msk = self.tl_master.selected_slot.slot.mask_name
                if msk is None:
                    self.slot_mask.config(text="Pas de masque")
                else:
                    self.slot_mask.config(text=f"Mask: {msk}")
                self.slot_mask_bt.config(state=tk.ACTIVE)

        def addSlotType(self, slt_indicator):
            fr = tk.Frame(self.table_types, width=self.rect_size, height=self.rect_size)
            lab = tk.Label(fr, bg="#AAAAAA")
            lab.pack(fill=tk.BOTH, expand=True)
            lab.bind(
                "<ButtonPress-1>", lambda x: self.select(slt_indicator.pointed_object)
            )
            slt_indicator.addFollower(lab)
            c, r = self.index_insert % self.byrow, self.index_insert // self.byrow
            if r == len(self.lrow):
                self.lrow.append([])
            self.lrow[-1].append(fr)
            if self.index_row <= r:
                fr.grid(row=r, column=c)
            self.index_insert += 1

        def select(self, slt_type=None):
            if self.tl_master.selected_slot is not None:
                self.tl_master.selected_slot.changeSlotType(slt_type)

        def upSel(self):
            self.index_row -= 1
            if self.index_row <= 0:
                self.index_row = 0
                self.bt_up.config(state=tk.DISABLED)
            if self.index_row < len(self.row) - 1:
                self.bt_down.config(state=tk.ACTIVE)
            for i, st in enumerate(self.lrow[self.index_row]):
                st.grid(row=self.index_row, column=i)

        def downSel(self):
            self.index_row += 1
            if self.index_row >= len(self.lrow) - 1:
                self.index_row = len(self.lrow) - 1
                self.bt_down.config(state=tk.DISABLED)
            if self.index_row > 0:
                self.bt_up.config(state=tk.ACTIVE)
            for st in self.lrow[self.index_row - 1]:
                st.grid_forget()

        def reset(self):
            for l in self.lrow:
                for e in l:
                    e.grid_forget()
            self.lrow.clear()
            none_indic = tk.Frame(
                self.table_types,
                bg="#999999",
                width=self.rect_size,
                height=self.rect_size,
            )
            self.lrow.append([none_indic])
            none_indic.grid(row=0, column=0)
            self.index_row = 0
            self.index_insert = 1
            self.bt_up.config(state=tk.DISABLED)
            none_indic.bind("<ButtonPress-1>", lambda x: self.select(None))

    class GraphLinker:
        class Node:
            def __init__(self, zone, spec, object) -> None:
                self.zone = zone
                self.spec = spec
                self.object = object
                self.enabled = True

            def connectTo(self, other, line):
                return self.object.connectTo(other, line)

            def enable(self):
                self.enabled = True

            def disable(self):
                self.enabled = False

            def inzone(self, x, y):
                return self.enabled and (
                    self.zone[0] <= x < self.zone[2]
                    and self.zone[1] <= y < self.zone[3]
                )

            def updateZone_ws(self, x, y, s):
                self.zone = [x, y, x + s, y + s]

            def updateZone_center(self, x, y, s):
                s //= 2
                self.zone = [x - s, y - s, x + s, y + s]

            def updateZone(self, x0, y0, x1, y1):
                self.zone = [x0, y0, x1, y1]

            def center(self):
                return (self.zone[0] + self.zone[2]) / 2, (
                    self.zone[1] + self.zone[3]
                ) / 2

        def __init__(self, master, root) -> None:
            self.species = {}
            self.elems = []

            self.master = master
            self.root = root
            self.line = None

            self.node_selected = None

        def addSpecies(self, name):
            self.species[name] = set()

        def addComptabilities(self, spec0, spec1):
            self.species[spec0].add(spec1)
            self.species[spec1].add(spec0)

        def addElem(self, zone, spec, object):
            if spec not in self.species:
                raise Exception("Tried to add an object to an unspecified species")
            node = GraphLinker.Node(zone, spec, object)
            self.elems.append(node)
            return node

        def addNode(self, node):
            if node.spec not in self.species:
                raise Exception("Tried to add a node to an unspecified species")
            self.elems.append(node)

        def _findElem(self, x, y):
            for elem in self.elems:
                if elem.inzone(x, y):
                    return elem

        def selectStart(self, x, y):
            sel = self._findElem(x, y)
            self.node_selected = sel
            if sel is not None:
                x0, y0 = sel.center()
                self.line = self.master.drawLine(x0, y0, x, y)
                self.master.iconfig(self.line, fill="black", width=2, tag="graph")
                if self.root is not None:
                    self.master.tile_zone.tag_raise(self.line, self.root)

        def selectTemp(self, x, y):
            if self.node_selected is not None:
                sel = self._findElem(x, y)
                if sel is not None:
                    if sel.spec in self.species[self.node_selected.spec]:
                        color = "#00ff00"
                    else:
                        color = "#ff0000"
                else:
                    color = "#000000"

                x0, y0 = self.node_selected.center()
                self.master.imove(self.line, x0, y0, x, y)
                self.master.iconfig(self.line, fill=color)

        def selectEnd(self, x, y):
            if self.node_selected is not None:
                sel = self._findElem(x, y)
                if sel is not None:
                    self.master.imove(
                        self.line, *(sel.center() + self.node_selected.center())
                    )
                if sel is None or self.node_selected.connectTo(sel, self.line):
                    self.master.idelete(self.line)
                self.node_selected = None
                self.line = None

        def clear(self):
            self.reset()
            self.node_selected = None
            self.elems.clear()

        def reset(self):
            if self.line is not None:
                self.master.idelete(self.line)

    class TileEditor(Editor):
        class NodePos(GraphLinker.Node):
            def __init__(self, master, dir) -> None:
                super().__init__([0, 0, 0, 0], "Pos", dir)
                self.arrows = []
                # for d in dir:
                #     self.arrows.append(master.drawLine(0, 0, 0, 0))
                #     master.iconfig(self.arrows[-1], width=2)
                self.repre = master.drawRectangle(0, 0, 0, 0)
                master.iconfig(self.repre, fill="white", width=2)
                DIR_COORD[dir] = self
                self.dir = dir

            def updateZone_center(self, master, x, y, s):
                super().updateZone_center(x, y, s)
                if self.repre is not None:
                    x, y = self.center()
                    master.imove(self.repre, *self.zone)
                #     for i, d in enumerate(self.dir):
                #         vx, vy = DIR_vect[d]
                #         master.imove(self.arrows[i], x, y, x + vx * 30, y + vy * 30)

            def connectTo(self, node, line):
                if self.spec == node.spec:
                    return True
                return node.connectTo(self, line)

        def _consRadio(self):
            radio = RadioButtonGroup(self)
            self.radios = radio
            s = radio.addImage("select.png", (lambda: self.selectMode("Select")))
            radio.addImage("add.png", (lambda: self.selectMode("Add")))
            radio.addImage("move.png", (lambda: self.selectMode("Move")))
            radio.addImage("link.png", (lambda: self.selectMode("Link")))
            radio.grid(column=0, row=0, rowspan=2)

        def _addPosNode(self):
            self.graphe.addNode(self.pos_n)
            self.graphe.addNode(self.pos_nne)
            self.graphe.addNode(self.pos_nee)
            self.graphe.addNode(self.pos_nnw)
            self.graphe.addNode(self.pos_nww)
            self.graphe.addNode(self.pos_s)
            self.graphe.addNode(self.pos_sww)
            self.graphe.addNode(self.pos_ssw)
            self.graphe.addNode(self.pos_sse)
            self.graphe.addNode(self.pos_see)
            self.graphe.addNode(self.pos_e)
            self.graphe.addNode(self.pos_w)

        def selectMode(self, mode):
            if mode in ("Select", "Move", "Link", "Add"):
                self.GrapheMode = mode
            else:
                raise Exception("Selected mode unrecognized")

        def __init__(self, master, tiles):
            Editor.__init__(self, master, tiles)
            self.grid_columnconfigure(1, weight=4)
            self.grid_columnconfigure(2, weight=1)
            self.grid_columnconfigure(3, weight=1)
            self.grid_rowconfigure(0, weight=1)

            no = ttk.Notebook(self)
            no.grid(column=3, row=0, sticky="news")

            self.tile_add_bt = tk.Button(self, text="+", borderwidth=5)
            self.tile_add_bt.grid(column=3, row=1, sticky="ew")

            self.tile_list = ScrollableFrame(no)
            self.tile_parameters = TileParameters(no, tiles, self)

            no.add(child=self.tile_list, text="Liste tuiles")
            no.add(child=self.tile_parameters, text="Paramètre slot")

            self.tile_zone = tk.Canvas(self)
            self.tile_zone.grid(column=1, row=0, rowspan=2)

            self._consRadio()

            self.pos_size = 24
            self.drawned_tile = self.tile_zone.create_image(
                self.pos_size, self.pos_size, anchor="nw"
            )
            self.drawned_mask = self.tile_zone.create_image(
                self.pos_size, self.pos_size, anchor="nw"
            )

            self.outline = self.tile_zone.create_rectangle(0, 0, 0, 0, tag="root")
            self.tile_zone.tag_raise("graph", "root")
            self.graphe = GraphLinker(self, self.outline)
            self.graphe.addSpecies("Pos")
            self.graphe.addSpecies("Slot")
            self.graphe.addComptabilities("Pos", "Slot")
            self.graphe.addComptabilities("Slot", "Slot")

            self.pos_nnw = TileEditor.NodePos(self, "nnw")
            self.pos_nww = TileEditor.NodePos(self, "nww")
            self.pos_nne = TileEditor.NodePos(self, "nne")
            self.pos_nee = TileEditor.NodePos(self, "nee")
            self.pos_sww = TileEditor.NodePos(self, "sww")
            self.pos_ssw = TileEditor.NodePos(self, "ssw")
            self.pos_sse = TileEditor.NodePos(self, "sse")
            self.pos_see = TileEditor.NodePos(self, "see")
            self.pos_n = TileEditor.NodePos(self, "n")
            self.pos_e = TileEditor.NodePos(self, "e")
            self.pos_w = TileEditor.NodePos(self, "w")
            self.pos_s = TileEditor.NodePos(self, "s")

            self._addPosNode()

            self.adaptSize(100, 100)

            self.selected_tile = None
            self.selected_slot = None

            self.tile_add_bt.configure(command=self.newTile)
            self.pressid = self.tile_zone.bind("<ButtonPress-1>", self.tileZoneClick)
            self.tile_zone.bind("<Motion>", self.tileZoneMotion)
            self.tile_zone.bind("<ButtonRelease-1>", self.tileZoneRelease)

            self.MouseDown = False
            self.GrapheMode = "Select"

        def clear(self):
            self.selectTile(None)
            self.tile_list.clear()
            self.graphe.clear()
            self.tile_zone.itemconfig(self.drawned_tile, image="")
            self.tile_parameters.reset()
            self._addPosNode()
            self.adaptSize(100, 100)

        def addSlotType(self, slt_type):
            self.tile_parameters.addSlotType(slt_type)

        def tileZoneClick(self, event):
            if (
                self.selected_tile is not None
                and self.selected_tile
                and not self.MouseDown
            ):
                x, y = event.x, event.y
                x -= self.pos_size
                y -= self.pos_size
                if self.GrapheMode != "Link":
                    if (
                        x < 0
                        or y < 0
                        or x >= self.selected_tile.tile.sprite.width
                        or y >= self.selected_tile.tile.sprite.height
                    ):
                        # print("Hors zone")
                        self.selectSlot(None)
                    else:
                        self.MouseDown = True
                        for s in self.selected_tile.islots:
                            if s.closeTo(x, y):
                                # print("Found")
                                self.selectSlot(s)
                                return
                        # print("Not Found")
                        if self.GrapheMode == "Add":
                            # print("Adding")
                            self.newSlot(x, y)
                        else:
                            self.selectSlot(None)
                else:
                    self.graphe.selectStart(x, y)
                    self.MouseDown = True

        def tileZoneMotion(self, event):
            if self.selected_tile is not None and self.MouseDown:
                x, y = event.x, event.y
                x -= self.pos_size
                y -= self.pos_size
                if self.selected_slot is not None and self.GrapheMode == "Move":
                    self.selected_slot.moveRepreTo(x, y)
                elif self.GrapheMode == "Link":
                    self.graphe.selectTemp(x, y)

        def tileZoneRelease(self, event):
            if self.selected_tile is not None and self.MouseDown:
                x, y = event.x, event.y
                x -= self.pos_size
                y -= self.pos_size
                if self.selected_slot is not None and self.GrapheMode == "Move":
                    self.selected_slot.moveTo(x, y)
                elif self.GrapheMode == "Link":
                    self.graphe.selectEnd(x, y)
            if self.MouseDown:
                self.MouseDown = False

        def newTile(self):
            self.tile_zone.unbind("<ButtonPress-1>", self.pressid)
            try:
                tile = Tile(self.tiles, relpath(filedialog.askopenfilename(filetypes=[("image", "*.png")])))
            except FailedCreation:
                print("Failed creation of tile", file=ERR_OUT)
                self.pressid = self.tile_zone.bind(
                    "<ButtonPress-1>", self.tileZoneClick
                )
                return
            ti = TileIndicator(self.tile_list.frame, self, tile)

            self.tile_list.add(ti)

            self.selectTile(ti)
            self.pressid = self.tile_zone.bind("<ButtonPress-1>", self.tileZoneClick)

        def addTile(self, tile: Tile):
            ti = TileIndicator(self.tile_list.frame, self, tile)
            self.tile_list.add(ti)
            for slt in ti.islots:
                self.graphe.addNode(slt.node)

        def newSlot(self, x, y):
            coord = (
                x / self.selected_tile.tile.sprite.width,
                y / self.selected_tile.tile.sprite.height,
            )
            slot = Slot(self.selected_tile.tile, coord)
            si = self.selected_tile.addSlot(slot)
            self.graphe.addNode(si.node)
            self.selectSlot(si)

        def selectTile(self, tileIndicator):
            self.tile_list.jump_to_widget(tileIndicator)

            if self.selected_tile is not None:
                self.selected_tile.disableSel()

            self.selectSlot(None)

            self.selected_tile = tileIndicator
            self.drawTile()

            self.tile_parameters.tileSelected()

        def selectSlot(self, slot):
            if self.selected_slot is not None:
                self.selected_slot.disableSel()
            if slot is None:
                self.selected_slot = None
            else:
                self.selected_slot = slot
                self.selected_slot.enableSel()
            self.tile_parameters.slotSelected()

        def adaptSize(self, width, height):
            self.tile_zone.configure(
                width=width + 2 * self.pos_size, height=height + 2 * self.pos_size
            )
            size = self.pos_size
            can = self

            self.tile_zone.coords(
                self.outline,
                self.pos_size,
                self.pos_size,
                self.pos_size + width,
                self.pos_size + height,
            )

            self.pos_nnw.updateZone_center(can, size, 0, size)
            self.pos_nww.updateZone_center(can, 0, size, size)
            self.pos_nne.updateZone_center(can, width - size, 0, size)
            self.pos_nee.updateZone_center(can, width, size, size)
            self.pos_ssw.updateZone_center(can, size, height, size)
            self.pos_sww.updateZone_center(can, 0, height - size, size)
            self.pos_sse.updateZone_center(can, width - size, height, size)
            self.pos_see.updateZone_center(can, width, height - size, size)

            self.pos_n.updateZone_center(can, width // 2, 0, size)
            self.pos_e.updateZone_center(can, width, height // 2, size)
            self.pos_s.updateZone_center(can, width // 2, height, size)
            self.pos_w.updateZone_center(can, 0, height // 2, size)

        def showMask(self):
            if self.selected_slot is not None:
                tile = self.selected_tile.tile
                slt = self.selected_slot.slot
                if slt.mask is not None and tile.sprite is not None:
                    self.mask = ImageTk.PhotoImage(slt.mask)
                    self.tile_zone.itemconfig(self.drawned_mask, image=self.mask)

        def hideMask(self):
            self.tile_zone.itemconfig(self.drawned_mask, image="")

        def drawTile(self):
            if self.selected_tile:
                self.adaptSize(
                    self.selected_tile.tile.sprite.width,
                    self.selected_tile.tile.sprite.height,
                )
                self.full_sprite = ImageTk.PhotoImage(self.selected_tile.tile.sprite)
                self.tile_zone.itemconfig(self.drawned_tile, image=self.full_sprite)
                self.selected_tile.enableSel()
                self.tile_zone.tag_lower("graph", "root")
            else:
                self.tile_zone.itemconfig(self.drawned_tile, image="")
                self.adaptSize(100, 100)

        def drawImage(self, x, y, image=None):
            return self.tile_zone.create_image(
                self.pos_size + x, self.pos_size + y, image=image
            )

        def drawRectangle(self, x0, y0, x1, y1):
            return self.tile_zone.create_rectangle(
                x0 + self.pos_size,
                y0 + self.pos_size,
                x1 + self.pos_size,
                y1 + self.pos_size,
            )

        def drawLine(self, x0, y0, x1, y1):
            return self.tile_zone.create_line(
                x0 + self.pos_size,
                y0 + self.pos_size,
                x1 + self.pos_size,
                y1 + self.pos_size,
            )

        def drawCircle(self, x, y, r):
            x += self.pos_size
            y += self.pos_size
            return self.tile_zone.create_oval(x - r, y - r, x + r, y + r)

        def idelete(self, obj):
            self.tile_zone.delete(obj)

        def imove(self, item, x, y, r=None, s=None):
            x += self.pos_size
            y += self.pos_size
            if r is None:
                self.tile_zone.coords(item, x, y)
            elif s is None:
                self.tile_zone.coords(item, x - r, y - r, x + r, y + r)
            else:
                self.tile_zone.coords(item, x, y, r + self.pos_size, s + self.pos_size)

        def iconfig(self, item, **kwargs):
            self.tile_zone.itemconfig(item, **kwargs)

    fen = tk.Tk()
    fen.title("PI Tile Tool")

    men = tk.Menu(fen)

    tiles = TileFile()

    save_file = ""

    def save(redo=False):
        global save_file
        # print("soive")
        if redo or save_file == "":
            name = relpath(filedialog.asksaveasfilename())
            if name == "" or type(name) == tuple or ".py" in name or ".png" in name:
                print("Wrong filename", file=ERR_OUT)
                return
            else:
                save_file = name
        tiles.save(save_file)

    def load():
        global save_file
        name = relpath(filedialog.askopenfilename())
        if name != "" and tiles.load(name):
            save_file = name
            me.clear()
            te.clear()
            ste.clear()
            for t in tiles.getTiles():
                te.addTile(t)
            for st in tiles.getSlotType():
                ste.addSlotType(st)
            for mt in tiles.getMeepleTypes():
                me.addMeeple(mt)

    file_menu = tk.Menu(men, tearoff=False)

    if len(EXPORT) > 0:
        export_menu = tk.Menu(file_menu, tearoff=False)
        for name in EXPORT:

            def f(name):
                return lambda: EXPORT[name](tiles)

            export_menu.add_command(label=name, command=f(name))
        file_menu.add_cascade(menu=export_menu, label="Export")

    file_menu.add_command(
        label="Sauvegarder",
        accelerator="Ctrl + S",
        command=lambda: save(),
    )
    file_menu.add_command(
        label="Sauvegarder sous",
        accelerator="Ctrl + Maj + S",
        command=lambda: save(True),
    )
    file_menu.add_command(
        label="Ouvrir", accelerator="Ctrl + O", command=lambda: load()
    )

    men.add_cascade(menu=file_menu, label="Fichier")
    fen.config(menu=men)
    onglets = ttk.Notebook(fen)
    onglets.pack(fill=tk.BOTH, expand=True)

    te = TileEditor(onglets, tiles)
    onglets.add(child=te, text="Tile")
    me = MeeplesEditor(onglets, tiles)
    onglets.add(child=me, text="Meeples")
    ste = SlotTypeEditor(onglets, tiles)
    onglets.add(child=ste, text="Slots type")

    me.setSlotTypeEditor(ste)
    ste.setMeepleEditor(me)
    ste.setTileEditor(te)

    fen.bind_all("<Control - s>", lambda x: save())
    fen.bind_all("<Control-Shift-S>", lambda x: save(True))
    fen.bind_all("<Control - o>", lambda x: load())
    for (k, b) in zip(("a", "z", "e", "r"), te.radios.buttons):
        te.radios.bind_accelerator(fen, k, b)

    fen.mainloop()
