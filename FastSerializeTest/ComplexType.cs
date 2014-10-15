using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSerializeTest
{
    public class ComplexType
    {
        public string _id;
        public int index;
        public Guid guid;
        public bool isActive;
        public string balance;
        public string picture;
        public int age;
        public string eyeColor;
        public string name;
        public string gender;
        public string company;
        public string email;
        public string phone;
        public string address;
        public string about;
        public string registered;
        public float latitude;
        public float longitude;
        public List<string> tags;
        public List<Named> friends;
        public string greeting;
        public string favoriteFruit;

    }
    public class Named
    {
        public int id;
        public string name;
    }
}
