using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GwentPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;

namespace GwentPro
{
    public abstract class CardModerator : MonoBehaviour
    {
        // Method to deserialize the JSON data as a list of CardData objects
        static List<CardData> LoadCardData(string filePath)
        {
            string jsonData = File.ReadAllText(filePath);
            List<CardData> cardDataList = JsonConvert.DeserializeObject<List<CardData>>(jsonData);
            return cardDataList;
        }

        // Method to generate a prefab from a CardData object
        static GameObject PrefabGenerator(int counter, CardData card, string dataPath)
        {
#if UNITY_EDITOR
            GameObject cardobj = new GameObject(card.Name);
            cardobj.AddComponent(GetCardScript(card.CombatType) as Type);
            SetProperties(cardobj, card, dataPath);
            SetPrefabImage(cardobj, card);

            string prefabPath = "Assets/MyPrefabs/" + card.Faction + "Card" + counter + ".prefab";
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(cardobj, prefabPath);
            DestroyImmediate(cardobj);
            return saved;
#else
    GameObject cardobj = new GameObject(card.Name);
    cardobj.AddComponent(GetCardScript(card.CombatType) as Type);
    SetProperties(cardobj, card, dataPath);
    SetPrefabImage(cardobj, card);

    return cardobj;
#endif
        }
        // Method to set individual properties of the prefab
        static void SetProperties(GameObject cardobj, CardData card, string dataPath)
        {
            cardobj.GetComponent<CardClass>().cardname = card.Name;
            cardobj.GetComponent<CardClass>().cardpoint = card.Points;

            SetCombatTypeEnums(card.CombatType, cardobj.GetComponent<CardClass>().combatTypes);
            cardobj.GetComponent<CardClass>().crdtype = (CardClass.cardtype)Enum.Parse(typeof(CardClass.cardtype), card.CardType);

            cardobj.GetComponent<CardClass>().faction = new Faction(dataPath);
            cardobj.tag = dataPath;
        }

        //Sets the image of a card prefab and adds a BoxCollider2D component to the GameObject.
        static void SetPrefabImage(GameObject cardobj, CardData card)
        {
            SpriteRenderer sr = cardobj.AddComponent<SpriteRenderer>();
            Sprite cardimg = Resources.Load<Sprite>("CardImg" + "/" + card.Image);
            sr.sprite = cardimg;

            Image image = cardobj.AddComponent<Image>(); // Adding an Image component so the card works with the horizontal layout group.
            Sprite sprite = Resources.Load<Sprite>("CardImg" + "/" + card.Image);
            image.sprite = sprite; // Set the Sprite to the Image component
            image.enabled = false
            ;
            cardobj.AddComponent<BoxCollider2D>();

        }

        // Returns the appropriate CardScript based on the card's CombatType
        static Type GetCardScript(List<string> cmbType)
        {
            if (cmbType.Count > 0)
            {
                string firstType = cmbType[0];
                if (firstType == "Leader")
                {
                    return typeof(LeaderCard);
                }
                else if (firstType == "Special")
                {
                    return typeof(SpecialCard);
                }
                else
                {
                    return typeof(CardClass);
                }
            }
            else
            {
                // Manejar el caso en que CombatType esté vacío
                Debug.LogError("CombatType está vacío");
                return null;
            }
        }

        private static void SetCombatTypeEnums(List<string> combatType, List<CardClass.CombatTypeListItem> enumList)
        {
            Dictionary<string, CardClass.combatype> dict = new Dictionary<string, CardClass.combatype>(){
                {"Melee", CardClass.combatype.Melee},
                {"Range", CardClass.combatype.Range},
                {"Siege", CardClass.combatype.Siege},
                {"Leader", CardClass.combatype.Leader},
                {"Special", CardClass.combatype.Special}
            };
            foreach (var element in combatType)
            {
                if (dict.ContainsKey(element))
                {
                    enumList.Add(new CardClass.CombatTypeListItem(dict[element]));
                }

            }
        }

        // PrefabsList method to generate a list of prefabs
        public static List<GameObject> PrefabsList(string dataPath)
        {
            List<CardData> cardDataList = LoadCardData("Assets/GameScripts/" + dataPath + ".json");
            List<GameObject> prefabsList = new List<GameObject>();
            int counter = 0;
            foreach (CardData item in cardDataList)
            {
                prefabsList.Add(PrefabGenerator(counter, item, dataPath));
                counter++;
            }
            return prefabsList;
        }
    }

    [System.Serializable]
    public class CardData
    {
        public string Name;
        public int Points;
        public string CardType;
        public List<string> CombatType;
        public string Faction;

        public string Image;
        public string EffectName;

        public CardData(string name, int points, string cardType, List<string> combatType, string faction, string image, string effectName)
        {
            Name = name;
            Points = points;
            CardType = cardType;
            CombatType = combatType;
            Faction = faction;
            Image = image;
            EffectName = effectName;
        }
    }

}
