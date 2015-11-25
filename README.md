# Tunnel Wars AI Final Project

To get started:
1. Download Unity
2. Open Assets/Scene.unity

The main logic of the game is all crammed into Game.cs. The AI addition will add classes used by this main script.

#### Style conventions
* `camelCase` is used for normal variables, and variables that are private/protected to the class (called fields in C#)
* `PascalCase` for public class methods that can be called from anywhere
* Place `public` (can be used by any other class) methods towards the top of the file so they are easy to spot at first glance
* `// Capitalize comments`
* `private` is default more or less, so no need to write it explicitly 

#### Python to C# Overview
* To make a new instance of a class you need to prefix the constructor with `new` as in: `new MyClass(x, y, z)`
* There are no `#include`'s! Once you make something "public" then it can be accessed from any file.
* Classes are declared like this:
  `public class MyClass { ...`
  * or else:
  `public class DerivedClass : BaseClass { ...`
  * or else:
  `public class DerivedClass : BaseClass, Interface { ...`
* An interface is prefixed by the letter I and is declared as:
  `public interface IMyInterfaceName`
  * Interfaces are the same as in OCaml, and are basically a contract adopted by the class that states a minimum criteria for methods to implement. For example, many classes in Tunnel Wars implement the IAdvancing interface, which requires a `void Advance(List<WorldAction> actions) { ...` implementation that advances the class given a list of actions.
* Things that you put inside the class declaration can be prefixed with access modifiers. Use these:
  * `public`: Able to be accessed by an class or any code
  * `protected`: Able to be accessed by *derived* classes
  * `private`: Is the default. No access from outside of the class.
  * Don't make any member variables ("fields") public. Instead use a property (see ahead).
  * You can for some reason assign initial values to member variables within the class declaration itself, but *don't* do this. Instead assign them in the constructor function that is called when an instance is made.
* Constructors/initialization functions are declared as follows (within the class declaration):`public MyClass(int exampleArg1, float exampleArg2, double exampleArg3) {...`
  * As a general rule, DON'T call any of the class's own methods from within the constructor, including properties (see ahead). Instead, if you have to do this, make a new private `init` function instead. This will avoid headache if we need to make a subclass (C# has something called constructor chaining which is a nightmare to read and very limiting).
* If you want to make a class member variable accessible to the public, use a property instead:
  * Properties are declared like this: `public float MyProperty{ get; set; }`..
  * .. and then can be used like this `MyProperty = 2;` or `x = MyProperty;`
  * Using properties allows for you to implement side-effects when getting or setting the property value.
  * To do this, instead of putting semicolons after get and set, implement get and set functions as follows.
  * The initial shorthand of `{ get; set; }` is actually equivalent to this:
  ```
    // Declare a private field that is used behind the scenes for getter/setter methods
    float behindTheScenesVar;
    MyProperty {
      get {
        return behindTheScenesVar;
      }
      set {
        behindTheScenesVar = value; // value is a special keyword in C# that is meant exclusively for the passed-
                                    // in value to this set function
      }
    }
  ```
  * Properties are usually public so make sure you capitalize their names.
* See powerups/weapons for examples of how to do enums (equivalent of OCaml's OR'ed together types) in which the value is chosen from a finite set
