
using SimpleJSON;

public class GameInfo
    {    
        public string name;
        public string description;
        public float popularity;
        public float old;
        public int price;
        public string author;
        public byte[] imageBytes;

        public GameInfo(string name, string description, float popularity, float old, int price, string author)
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
            node["image"] = System.Text.Encoding.UTF8.GetString(imageBytes);
            return node;
        }
}
