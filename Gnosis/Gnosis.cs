using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gnosis
{
    // http://cs.gettysburg.edu/~tneller/fys187-4/Inform7_CheatSheet.pdf


    public enum Direction
    {
        North,
        West,
        East,
        South,
        Up,
        Down,
        Inside,
        Outside
    }


    public static class BuiltInActions
    {
        public static UnaryAction taking = new UnaryAction("taking", "take");
        public static UnaryAction going = new UnaryAction("going", "go");
        public static UnaryAction examining = new UnaryAction("examining", "examine");
    }


    public static class Globals
    {
        static public List<Room> rooms = new List<Room>();
        static public List<Action> actions = new List<Action>();
        static public Room location;
        static public List<GlobalVariable> variables = new List<GlobalVariable>();
    }

    public abstract class GlobalVariable
    {
        public string name;
    }


    public class Global<T> : GlobalVariable
    {
        public T initialValue;

        public Global(string name, T initialValue)
        {
            this.name = name;
            this.initialValue = initialValue;
            Globals.variables.Add(this);
        }
    }


    

    public class Gnobject
    {
        public string name;
       
    }



    public partial class Room : Gnobject
    {
        public bool lit = true;
        public bool visited = false;
        public string description;
        public Gnobject region;
        public List<(Direction, Room)> connections = new List<(Direction, Room)>();
        public List<Thing> things = new List<Thing>();

        public Room(string name)
        {
            this.name = name;
            Globals.rooms.Add(this);
        }
    }




    public class Thing : Gnobject
    {
        public bool edible = false;
        public bool lit = false;
        public bool portable = true;
        public bool described = true;
        public bool markedForListing = false;
        public bool mentioned = true;

        public bool scenery = false;
        public bool wearable = false;
        public bool handled = false;
        public bool pushable = false;

        public string description;
        public string initialAppearance;

        public Gnobject matchingKey;
        public List<string> aliases = new List<string>();
        public List<System.Type> props = new List<System.Type>(); 

        public Thing(string name)
        {
            this.name = name;
        }

        public void Is<T>()
        {
            props.Add(typeof(T));
        }


    }

    public partial class Supporter : Thing
    {
        public Supporter(string name) : base(name)
        {
            portable = false;
            scenery = true;
        }

        public List<Thing> things = new List<Thing>();
    }

    public class Action {
        // actions ONLY apply to things, eg. a wet thing, a visible thing
        // OR kinds of value (enums), eg North, chonkedex app 


        public Action(string name, string verb)
        {
            this.name = name;
            verbs.Add(verb);
            Globals.actions.Add(this);
        }

        public List<string> verbs = new List<string>();
        public string name;
    };

    public class SelfAction : Action
    {
        public SelfAction(string name, string verb) : base(name, verb) { }
    }

    public class UnaryAction : Action
    {

        public UnaryAction(string name, string verb) : base(name, verb) { }
        public Expression<Predicate<Thing>> applyingTo = (Thing a) => a is Thing;
    }

    public class BinaryAction : Action
    {
        public BinaryAction(string name, string verb) : base(name, verb) { }
        public Expression<Predicate<Thing>> applyingToFirst = (Thing a) => a is Thing && a.portable;
        public Expression<Predicate<Thing>> applyingToSecond = (Thing a) => a is Thing;
    }


    public class OppositeAttribute : Attribute
    {
        string opposite;
        public OppositeAttribute(string s)
        {
            opposite = s;
        }
    }

    public class Rule
    {
        public enum Type
        {
            Before,
            Instead,
            Check,
            CarryOut,
            After,
            Report
        }

        public static List<Rule> rules = new List<Rule>();

        public Type type;

        public Action applyingTo;
        public List<Gnobject> targets = new List<Gnobject>();
        public List<Room> locations = new List<Room>();
        public Expression condition;
        public List<string> say = new List<string>();

        public Rule()
        {
            rules.Add(this);
        }


        public static Rule Instead(Action action)
        {
            var rule = new Rule();
            rule.type = Type.Instead;
            rule.applyingTo = action;

            return rule;
        }


        public Rule Target(Gnobject obj)
        {
            targets.Add(obj);
            return this;
        }

        public Rule Where(Room room)
        {
            locations.Add(room);
            return this;
        }

        public Rule Condition<T>(Expression<Predicate<T>> expr )
        {
            condition = expr;
            return this;
        }

        public Rule Condition<T>(T thing) where T : Thing
        {
            Expression<Predicate<Thing>> expr = (Thing a) => a == thing;
            condition = expr;
            return this;
        }
        public Rule Condition(Room room)
        {
            Expression<Predicate<Room>> expr = (Room a) => Globals.location == room;
            condition = expr;
            return this;
        }


        public Rule Do(Func<bool> result)
        {

            return this;
        }

        public Rule Say(string str)
        {
            say.Add(str);
            return this;
        }


    }
}
