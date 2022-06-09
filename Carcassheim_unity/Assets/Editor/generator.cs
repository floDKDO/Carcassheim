using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;
using UnityEngine;
using UnityEditor;

public class generator
{
    static string generated_path = "Assets/Affichage_InGame/Tuiles/generated/";

    [MenuItem("Tools/Create tiles prefab")]
    static public void generateTilePrefab()
    {
        using (XmlReader reader = XmlReader.Create("Assets/Affichage_InGame/Tuiles/config_front.xml"))
        {
            int state = 0;
            while (reader.Read())
            {
                if (state < 0)
                {
                    Debug.Log("Reentering when file should be finished");
                    break;
                }

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "carcasheim" && state == 0)
                            state = 1;
                        else if (reader.Name == "tuile")
                        {
                            if (state == 1)
                            {
                                readingTile(reader);
                            }
                            else
                            {
                                Debug.Log("Reading tuile before entering carcasheim");
                            }
                        }
                        else
                            Debug.Log("Unkown state " + reader.Name);
                        break;
                    case XmlNodeType.Text:
                        Debug.Log("No text should be read when not in state " + reader.Value);
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "carcasheim")
                            state = -1;
                        else
                        {
                            Debug.Log("Only carcahseim should be ended.");
                        }
                        break;
                }
            }
        }
    }

    static public TuileRepre readingTile(XmlReader reader)
    {
        Debug.Log("READ TILE");
        bool tile_finished = false;
        int id = -1;
        string sprite = "";
        bool all_slot = true;
        List<SlotIndic> slots = new List<SlotIndic>();

        TuileRepre tile = null;
        int state = -1;
        while (!tile_finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "id":
                            state = 0;
                            break;
                        case "sprite":
                            state = 1;
                            break;
                        case "slot":
                            slots.Add(readingSlot(reader));
                            break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    switch (state)
                    {
                        case 0:
                            if (!int.TryParse(reader.Value, out id)) id = -1;
                            break;
                        case 1:
                            sprite = reader.Value;
                            break;
                        default:
                            Debug.Log("Tried to read value when in no state : " + reader.Value);
                            break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    switch (reader.Name)
                    {
                        case "tuile":
                            tile_finished = true;
                            break;
                        case "id":
                        case "sprite":
                            state = -1;
                            break;
                        default:
                            Debug.Log("Tried to end element other than tile : " + reader.Name);
                            break;
                    }
                    break;
            }
        }
        all_slot = true;
        foreach (SlotIndic slot in slots)
        {
            all_slot = slot != null;
        }

        if (id >= 0 && all_slot && sprite.Length > 0)
        {
            Debug.Log("Tried to create tile " + id.ToString() + " of sprite " + sprite);
            GameObject obj = PrefabUtility.LoadPrefabContents("Assets/Affichage_InGame/Tuiles/tile_default.prefab");

            tile = obj.GetComponent<TuileRepre>();
            // PrefabUtility.UnpackPrefabInstance(tile.model, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            Renderer red = tile.model.GetComponent<Renderer>();
            Material mat = new Material(red.sharedMaterials[2]);
            Texture text = AssetDatabase.LoadAssetAtPath<Texture>(sprite);
            mat.mainTexture = text;
            Debug.Log(mat.mainTexture);
            Material[] materials = red.sharedMaterials;
            string mat_name = generated_path + "mat_tile_" + id.ToString() + ".mat";
            AssetDatabase.CreateAsset(mat, mat_name);
            materials[2] = AssetDatabase.LoadAssetAtPath<Material>(mat_name);
            red.sharedMaterials = materials;
            Debug.Log(red.sharedMaterials[2].mainTexture);


            foreach (SlotIndic slot in slots)
            {
                obj = PrefabUtility.LoadPrefabContents("Assets/Affichage_InGame/Tuiles/Face.prefab");

                TileFace face = obj.GetComponent<TileFace>();
                mat = new Material(face.GetComponent<Renderer>().sharedMaterial);
                mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture>(slot.Maskname);
                mat_name = generated_path + "mask_" + id.ToString() + "_" + slot.Id.ToString() + ".mat";
                AssetDatabase.CreateAsset(mat, mat_name);
                face.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(mat_name);

                slot.setFace(face);
                tile.addSlot(slot);
            }
            PrefabUtility.SaveAsPrefabAsset(tile.gameObject, generated_path + "Resources/tile" + id.ToString() + ".prefab");
            PrefabUtility.UnloadPrefabContents(tile.gameObject);
        }
        foreach (SlotIndic slot in slots)
        {
            if (slot != null)
            {
                PrefabUtility.UnloadPrefabContents(slot.front.gameObject);
                PrefabUtility.UnloadPrefabContents(slot.gameObject);
            }
        }
        Debug.Log("END TILE");
        return tile;
    }

    static public SlotIndic readingSlot(XmlReader reader)
    {
        int id = -1;
        float x = 0, y = 0;
        bool x_valid = false, y_valid = false;
        bool slot_finished = false;
        string mask = "";
        int state = -1;

        SlotIndic slot = null;

        Debug.Log("READING SLOT");
        while (!slot_finished && reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    switch (reader.Name)
                    {
                        case "id":
                            state = 0;
                            break;
                        case "x":
                            state = 1;
                            break;
                        case "y":
                            state = 2;
                            break;
                        case "sprite":
                            state = 3;
                            break;
                        default:
                            Debug.Log("Tried to enter unknown state : " + reader.Name);
                            break;
                    }
                    break;
                case XmlNodeType.Text:
                    switch (state)
                    {
                        case 0:
                            if (!int.TryParse(reader.Value, out id)) id = -1;
                            break;
                        case 1:
                            x_valid = float.TryParse(reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out x);
                            break;
                        case 2:
                            y_valid = float.TryParse(reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                            break;
                        case 3:
                            mask = reader.Value;
                            break;
                        default:
                            Debug.Log("Tried to read value when in no state : " + reader.Value);
                            break;
                    }
                    state = -1;
                    break;
                case XmlNodeType.EndElement:
                    switch (reader.Name)
                    {
                        case "slot":
                            slot_finished = true;
                            break;
                        case "id":
                        case "x":
                        case "y":
                        case "sprite":
                            state = -1;
                            break;
                        default:
                            Debug.Log("Tried to end element other than slot : " + reader.Name);
                            break;
                    }
                    break;
            }
        }
        if (id >= 0 && x_valid && y_valid && mask.Length > 0)
        {
            Debug.Log("Tried to create slot of id " + id.ToString() + " of coordinates (" + x.ToString() + ", " + y.ToString() + ") of mask " + mask);
            GameObject obj = PrefabUtility.LoadPrefabContents("Assets/Affichage_InGame/Tuiles/slot_indic.prefab");
            slot = obj.GetComponent<SlotIndic>();
            slot.Xf = x;
            slot.Yf = y;
            slot.Id = id;
            slot.Maskname = mask;
        }
        Debug.Log("END SLOT");
        return slot;
    }
}
