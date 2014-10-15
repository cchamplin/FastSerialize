High performance .NET JSON encoder/decoder

=========================== 

### Usage

```C#
  // Initialize the serializer
  var mySerializer = new Serializer(typeof(JsonSerializerString));
  
  var json = "[{\"Foo\"=\"Bar\"},{\"Foo\"=\"Bar\"}];
  
  var fooInstance = mySerializer.Deserialize<List<Foobar>>(json);
  
  ...
  public class Foobar { public string Foo { get; set; } }
```



### FAQ

**Q: I see the same snippets of code in many places, why don't you write clean code that can be reused?**

A: Occasionally you have to sacrifice abstraction and to an extent code maintainability to achieve greater performance. For a library as small as this serializer is I'm not too concerned.

**Q: Why doesn't it deserialize/serialize X class? Why isn't X feature supported? I found a bug!**

A: Create an issue and I will look into it.

**Q: If you do X you get can get a Y% performance increase!**

A: Not really a questions is it? Also great! Submit a pull request!