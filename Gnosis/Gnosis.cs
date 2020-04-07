using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Gnosis.Rule;

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



    public static class Globals
    {
        static public List<Room> rooms = new List<Room>();
        static public List<Action> actions = new List<Action>();
        static public Room location;
    }


    public class Gnobject
    {


    }

    public class Room : Gnobject
    {
        public bool lit = true;
        public bool visited = false;
        public string name;
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
        public bool lightProducing = false;
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
        public string name;

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

    public class Supporter : Thing
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
        public Expression<Predicate<Gnobject>> condition;
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

        public Rule Condition(Expression<Predicate<Gnobject>> expr )
        {
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

    public class Inform
    {
        string OutputRoom(Room room)
        {
            var s = room.name + " is a room. The description is \"" + room.description + "\"\n";
            foreach(var connection in room.connections)
            {
                s += connection.Item1.ToString() + " of " + room.name + " is " + connection.Item2.name + ".\n";
            }
            return s;
        }

        string OutputThing(Thing thing, Room location, Thing container)
        {
            var s = "";
            if(location != null)
                s = thing.name + " is a " + thing.GetType().Name + " in " + location.name + ". ";

            if(container != null)
                s = thing.name + " is a " + thing.GetType().Name + " on " + container.name + ". ";

            s += "The description is \"" + thing.description + "\"\n";


            foreach(var alias in thing.aliases)
            {
                s += "Understand \"" + alias + "\" as " + thing.name + ".\n";
            }


            if(thing is Supporter supporter)
            {
                foreach(var subthing in supporter.things)
                {
                    s += OutputThing(subthing, null, supporter);
                }
            }

            return s;
        }




        public string OutputInform()
        {
            var output = "Generated by Kelly MacNeill begins here.\n";

            foreach (var room in Globals.rooms)
            {
                output += OutputRoom(room);
    
                foreach (var thing in room.things)
                {
                    output += OutputThing(thing, room, null);
                }
            }


            output += "Generated ends here.\n";

            return output;
        }
    }
}
