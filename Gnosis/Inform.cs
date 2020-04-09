﻿using System.Reflection;
using System.Linq.Expressions;
using System;

namespace Gnosis
{
    public partial class Inform
    {



        string OutputGlobalVariable(GlobalVariable variable)
        {
            (string, string) GetTypeString(GlobalVariable var)
            {
                if (var is Global<bool> b)
                    return ("truth state", b.initialValue.ToString());
                if (var is Global<string> str)
                    return ("text", str.initialValue);
                if (var is Global<int> i)
                    return ("number", i.initialValue.ToString());

                return ("something", "");
            }

            var type = GetTypeString(variable);

            string s = variable.name + " is a " + type.Item1 + " that varies. " + variable.name + " is initially " + type.Item2 + ".\n";
            return s;
        }

        string OutputField(FieldInfo field, object gameClass)
        {
            string GetTypeString(System.Type t)
            {
                if (t == typeof(bool))
                    return "truth state";
                if (t == typeof(string))
                    return "text";
                if (t == typeof(int))
                    return "number";
                if (t == typeof(Thing))
                    return "thing";

                throw new System.Exception("invalid global field");
            }

            var typeStr = GetTypeString(field.FieldType);
            var objvalue = field.GetValue(gameClass);

            string s = field.Name + " is a " + typeStr + " that varies. " + field.Name + " is initially " + objvalue.ToString() + ".\n";

            return s;
        }

        string OutputGlobalFields(object gameClass)
        {
            var s = "";
            var type = gameClass.GetType();
            var fields = type.GetFields();
            foreach(var field in fields)
            {
                s += OutputField(field, gameClass);
            }
            
            return s;
        }


        string OutputRule(Rule rule)
        {
            var s = rule.type.ToString() + " " + rule.applyingTo.name;
            if(rule.condition != null)
            {
                s += " when ";

                var expr = rule.condition;
                if (expr is Expression<Predicate<Thing>> thingExpr)
                {
                    var visitor = new ThingVisitor();
                    visitor.Visit(expr);
                    return s + visitor.s + ":";
                }
            }

            return s;
        }


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




        public string OutputInform(object game)
        {
            var output = "Generated by Kelly MacNeill begins here.\n";

            output += OutputRule(Rule.rules[0]);

            output += OutputGlobalFields(game);

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