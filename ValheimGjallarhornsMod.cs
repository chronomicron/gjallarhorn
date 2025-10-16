using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using ItemManager; // Hypothetical ItemManager for item creation (e.g., Jötunn)

namespace ValheimGjallarhornsMod
{
    [BepInPlugin("com.chronomicron.valheimgjallarhorns", "Valheim Gjallarhorns", "1.0.2")]
    public class ValheimGjallarhornsMod : BaseUnityPlugin
    {
        private void Awake()
        {
            // Initialize Harmony for patching
            var harmony = new Harmony("com.chronomicron.valheimgjallarhorns");
            harmony.PatchAll();

            // Register the three horn items
            CreateSkoggjall();
            CreateVindblåst();
            CreateRagnarblæser();
        }

        private void CreateSkoggjall()
        {
            Item skoggjall = new Item("Skoggjall", "Skoggjall");
            skoggjall.Crafting.Add(CraftingTable.Workbench, 2);
            skoggjall.RequiredItems.Add("DeerHide", 2);
            skoggjall.RequiredItems.Add("TrollHide", 2);
            skoggjall.RequiredItems.Add("BoneFragments", 2);
            skoggjall.RequiredItems.Add("Bronze", 1);
            skoggjall.RequiredItems.Add("Copper", 1);
            skoggjall.CraftAmount = 1;

            skoggjall.Prefab.AddComponent<HornBehavior>().Setup(100f, new SkillBoost { Skill = Skills.SkillType.Run, Levels = 10f }, "Skoggjall", "sfx_skoggjall_blast", "skoggjall_icon.tex", "skoggjall.prefab");
            skoggjall.Item.m_itemData.m_shared.m_name = "$item_skoggjall";
            skoggjall.Item.m_itemData.m_shared.m_description = "A forest-carved horn that calls beasts from the shadows, boosting your stride.";
            skoggjall.Item.m_itemData.m_shared.m_maxStackSize = 1;
            skoggjall.Item.m_itemData.m_shared.m_weight = 2f;
            skoggjall.Item.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Utility;
            skoggjall.Item.m_itemData.m_shared.m_equipDuration = 0.5f;
        }

        private void CreateVindblåst()
        {
            Item vindblast = new Item("Vindblåst", "Vindblåst");
            vindblast.Crafting.Add(CraftingTable.Forge, 2);
            vindblast.RequiredItems.Add("TrollHide", 2);
            vindblast.RequiredItems.Add("WolfPelt", 2);
            vindblast.RequiredItems.Add("Root", 1);
            vindblast.RequiredItems.Add("Chain", 1);
            vindblast.RequiredItems.Add("FineWood", 2);
            vindblast.RequiredItems.Add("Iron", 2);
            vindblast.CraftAmount = 1;

            vindblast.Prefab.AddComponent<HornBehavior>().Setup(125f, new SkillBoost { Skill = Skills.SkillType.Bows, Levels = 10f }, "Vindblåst", "sfx_vindblast_blast", "vindblast_icon.tex", "vindblast.prefab");
            vindblast.Item.m_itemData.m_shared.m_name = "$item_vindblast";
            vindblast.Item.m_itemData.m_shared.m_description = "A wind-forged horn that stirs the hunt, sharpening your aim.";
            vindblast.Item.m_itemData.m_shared.m_maxStackSize = 1;
            vindblast.Item.m_itemData.m_shared.m_weight = 2.5f;
            vindblast.Item.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Utility;
            vindblast.Item.m_itemData.m_shared.m_equipDuration = 0.5f;
        }

        private void CreateRagnarblæser()
        {
            Item ragnarblaeser = new Item("Ragnarblæser", "Ragnarblæser");
            ragnarblaeser.Crafting.Add(CraftingTable.ArtisanTable, 1);
            ragnarblaeser.RequiredItems.Add("Bilebag", 1);
            ragnarblaeser.RequiredItems.Add("LoxPelt", 2);
            ragnarblaeser.RequiredItems.Add("ScaleHide", 4);
            ragnarblaeser.RequiredItems.Add("Tar", 1);
            ragnarblaeser.RequiredItems.Add("LinenThread", 2);
            ragnarblaeser.RequiredItems.Add("BlackMetal", 2);
            ragnarblaeser.CraftAmount = 1;

            ragnarblaeser.Prefab.AddComponent<HornBehavior>().Setup(150f, new SkillBoost { 
                Skill = Skills.SkillType.Swords | Skills.SkillType.Clubs | Skills.SkillType.Spears, 
                Levels = 10f 
            }, "Ragnarblæser", "sfx_ragnarblaeser_blast", "ragnarblaeser_icon.tex", "ragnarblaeser.prefab");
            ragnarblaeser.Item.m_itemData.m_shared.m_name = "$item_ragnarblaeser";
            ragnarblaeser.Item.m_itemData.m_shared.m_description = "A doom-herald horn that rallies the wild, empowering your blade and fury.";
            ragnarblaeser.Item.m_itemData.m_shared.m_maxStackSize = 1;
            ragnarblaeser.Item.m_itemData.m_shared.m_weight = 3f;
            ragnarblaeser.Item.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Utility;
            ragnarblaeser.Item.m_itemData.m_shared.m_equipDuration = 0.5f;
        }
    }

    // Struct to hold skill boost data
    public struct SkillBoost
    {
        public Skills.SkillType Skill;
        public float Levels;
    }

    // Custom component for horn behavior
    public class HornBehavior : MonoBehaviour, IEquipment
    {
        private float noiseLevel;
        private SkillBoost skillBoost;
        private string hornName;
        private string soundAsset;
        private string iconAsset;
        private string modelAsset;
        private float cooldown = 30f;
        private float lastUsed = -30f;

        public void Setup(float noise, SkillBoost boost, string name, string sound, string icon, string model)
        {
            noiseLevel = noise;
            skillBoost = boost;
            hornName = name;
            soundAsset = sound;
            iconAsset = icon;
            modelAsset = model;
        }

        public void Use(Player player)
        {
            if (Time.time - lastUsed < cooldown)
            {
                player.Message(MessageHud.MessageType.Center, $"{hornName} is on cooldown!");
                return;
            }

            lastUsed = Time.time;

            // Generate noise to attract and wake animals from afar
            var noiseMaker = player.GetComponent<NoiseMaker>();
            if (noiseMaker != null)
            {
                noiseMaker.GenerateNoise(noiseLevel, player.transform.position);
            }

            // Ping location on minimap with tier-specific icon (placeholder)
            Minimap.PinData pin = Minimap.instance.AddPin(player.transform.position, Minimap.PinType.Icon3, $"{hornName} Call", true, false);
            pin.m_saveToGlobalMap = true; // Persist on global map

            // Apply skill boost
            StatusEffect skillEffect = ScriptableObject.CreateInstance<StatusEffect>();
            skillEffect.m_name = $"{hornName} Boost";
            skillEffect.m_ttl = 30f; // Lasts 30 seconds
            skillEffect.m_skillLevel = skillBoost.Skill;
            skillEffect.m_skillLevelIncrease = skillBoost.Levels;
            player.GetSEMan().AddStatusEffect(skillEffect);

            // Play tier-specific sound effect (loads placeholder asset)
            if (!string.IsNullOrEmpty(soundAsset))
            {
                // Assume asset bundle loaded; in practice, use Resources.Load<AudioClip>(soundAsset)
                AudioClip clip = Resources.Load<AudioClip>(soundAsset);
                if (clip != null)
                {
                    ZSFX.PlayAt(clip, player.transform.position);
                }
                else
                {
                    ZSFX.PlayAt("sfx_horn_generic", player.transform.position); // Fallback
                }
            }

            player.Message(MessageHud.MessageType.Center, $"The {hornName} awakens the beasts!");
        }
    }

    // Harmony patch to integrate horn usage
    [HarmonyPatch(typeof(Player), "UseHotbarItem")]
    public class Player_UseHotbarItem_Patch
    {
        static void Postfix(Player __instance, int index)
        {
            ItemDrop.ItemData item = __instance.GetInventory().GetItemAt(index, 0);
            if (item != null && (item.m_shared.m_name == "$item_skoggjall" || 
                                 item.m_shared.m_name == "$item_vindblast" || 
                                 item.m_shared.m_name == "$item_ragnarblaeser"))
            {
                HornBehavior horn = item.m_dropPrefab.GetComponent<HornBehavior>();
                if (horn != null)
                {
                    horn.Use(__instance);
                }
            }
        }
    }

    // Localization for item names and descriptions
    [HarmonyPatch(typeof(Localization), "SetupLanguage")]
    public class Localization_Patch
    {
        static void Postfix(Localization __instance)
        {
            __instance.AddWord("item_skoggjall", "Skoggjall");
            __instance.AddWord("item_skoggjall_desc", "A forest-carved horn that calls beasts from the shadows, boosting your stride.");
            __instance.AddWord("item_vindblast", "Vindblåst");
            __instance.AddWord("item_vindblast_desc", "A wind-forged horn that stirs the hunt, sharpening your aim.");
            __instance.AddWord("item_ragnarblaeser", "Ragnarblæser");
            __instance.AddWord("item_ragnarblaeser_desc", "A doom-herald horn that rallies the wild, empowering your blade and fury.");
        }
    }
}
