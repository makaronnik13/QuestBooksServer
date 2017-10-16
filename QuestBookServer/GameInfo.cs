
using SimpleJSON;

public class GameInfo
    {    
        public string name;
        public string description;
        public float popularity;
        public float old;
        public int price;
        public string author;
        public string imagePath;

        public GameInfo(string name, string description, float popularity, float old, int price, string author, string imagePath)
        {
            this.name = name;
            this.description = description;
            this.popularity = popularity;
            this.old = old;
            this.price = price;
            this.author = author;
            this.imagePath = imagePath;
        }

        public JSONNode SaveToJSON()
        {
            JSONNode node = new JSONObject();
            node["name"] = name;
            node["description"] = description;
            node["popularity"] = popularity;
            node["old"] = old;
            node["price"] = price;
            node["author"] = author;
            node["image"] = imagePath;
            return node;
        }
}
