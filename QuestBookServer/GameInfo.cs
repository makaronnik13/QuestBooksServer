
using SimpleJSON;

public class GameInfo
    {    
        public string name;
        public string description;
        public float popularity;
        public long old;
        public int price;
        public string author;
        public bool bought;

        public GameInfo(string name, string description, int price, float popularity, long old, string author)
        {
            this.name = name;
            this.description = description;
            this.popularity = popularity;
            this.old = old;
            this.price = price;
            this.author = author;
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
            return node;
        }
}
