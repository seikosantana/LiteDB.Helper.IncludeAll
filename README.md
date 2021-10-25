# .IncludeAll() Extension for LiteDB v5
Original author: Chiko
This extension class allows you to .IncludeAll subdocuments referenced in the registered entities in LiteDB v5.

## Warning:
This package has not been heavily tested yet. Code may still needs to be polished. Issues and pull requests are welcome.

## How to Use
##### Setting up
Add the package in the project.
```dotnetcli
dotnet add package LiteDB.Helper.IncludeAll
```

Import using.
```csharp
using LiteDB.Helper.IncludeAll;
```

##### _Register_
Register the entities to get included on `.IncludeAll()`
Define which entity shall dereference which subdocument, and with `MemberExpression`, point out what property should be dereferenced. 
For example:
```csharp
LiteDBHelper.Register<Recipe, MaterialInRecipe>(r => r.Materials);
```
Provide its collection name if the subcollection is using custom collection name. For example:
```csharp
LiteDBHelper.Register<Recipe, MaterialInRecipe>(r => r.Materials, "materials");
```
_Note: The `Register` statement creates the `DbRef` for you in the global `BsonMapper`.  

##### To include all
The class `LiteDBHelper` defines an extension method for `ILiteCollection<T>`, so for any collection, you can, for example
```csharp
var result = someCollection.IncludeAll().FindAll().ToList();
```

#### A short and quick example
```csharp
using LiteDB;
using Types.TestTypes;

public class Test
{
    public void DoTest()
    {

        LiteDbHelper.Register<Recipe, MaterialInRecipe>(r => r.Materials);
        LiteDbHelper.Register<MaterialInRecipe, Material>(m => m.Material);
        LiteDbHelper.Register<Material, Cost>(m => m.Cost);

        var db = new LiteDatabase("test.db");

        Cost cost = new Cost
        {
            Amount = 20
        };
        var costs = db.GetCollection<Cost>("costs");
        cost.Id = costs.Insert(cost);

        Material material = new Material
        {
            Cost = cost,
            Name = "Mat 01"
        };
        var materials = db.GetCollection<Material>("materials");
        material.Id = materials.Insert(material);

        MaterialInRecipe mInRecipe = new MaterialInRecipe
        {
            Material = material,
            Weight = 500
        };
        var mInRecipes = db.GetCollection<MaterialInRecipe>("materialsInRecipe");
        mInRecipe.Id = mInRecipes.Insert(mInRecipe);

        Recipe recipe = new Recipe
        {
            Materials = new List<MaterialInRecipe> {
                mInRecipe
            },
            Name = "Recipe 01"
        };
        var recipes = db.GetCollection<Recipe>("recipes");
        recipes.Insert(recipe);

        var result = recipes.IncludeAll().FindAll().ToList();
    }
}
```

#### Some Tips
Inspect the database using `LiteDB.Studio` to make sure data stored as references.