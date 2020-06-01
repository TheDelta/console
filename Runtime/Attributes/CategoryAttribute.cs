using System;
using static Popcron.Console.Parser;

namespace Popcron.Console
{
    [Serializable]
    public class CategoryAttribute : Attribute
    {
        public string name = "";
        public string id = "";

        public CategoryAttribute(string name, string id = "")
        {
            this.name = Sanitize(name);
            this.id = id;
        }
    }
}
