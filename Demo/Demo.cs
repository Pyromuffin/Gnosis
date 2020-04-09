using System;
using System.Reflection;
using Gnosis;
using static Gnosis.Rule;
using static Gnosis.BuiltInActions;


namespace Demo
{
    class Demo
    {
        static void Main(string[] args)
        {
            Demo1 demo = new Demo1();
            demo.Start();

            // lets write some inform!
            var inform = new Inform();
            var output = inform.OutputInform(demo);
            var path = @"C:\Users\pyrom\Documents\GitHub\Inform Gen Test\Gen Test.materials\Extensions\Kelly MacNeill\Generated.I7X";

            System.IO.File.WriteAllText(path, output);
            Console.WriteLine(output);
        }
    }

    interface wet 
    { 
        bool wet { get; } 
        bool dry { get; }
    };

    /*
    public class Thing : Gnosis.Thing
    {
        public Thing(string name) : base(name){}
    }
    */

    public class Demo1
    {
        public bool lockdown = true;

        public void Start()
        {

            Room kitchen = new Room("The Kitchen");
            kitchen.description =
       @"I'm in the kitchen now. [if the lockdown is true] A pleasant yet urgent announcement rings '[b]Laserite Core criticality event detected: apartment lockdown in effect.[/b]' The doors leading to other parts of the apartment are shielded with comically elaborate folding door layers.[end if]
        [if unvisited]The refrigerator remains unmentioned.[end if]
        A [table] obnoxiously takes up most of the useful room in here.";

            Room yourRoom = new Room("Your Room");
            yourRoom.description =
       @"I'm in your room [if unvisited]now.[otherwise]again.[end if] What do you want me to do in here? You obviously know what's in your room, but just in case[one of] you have amnesia[or] you're standing on a rug that erases all memory of your existence outside the rug[or] a cougar ate your memory of being attacked by a cougar[or] i'm talking to a stranger who stole your phone[at random]: You've got your desk with wires growing out of it. There are some seltzer bottles strewn on the floor. It's kind of a mess in here tbh.";
            kitchen.connections.Add((Direction.West, yourRoom));


            Thing cellphone = new Thing("The cellphone");
            cellphone.description = "It's my fancy new iPhone LXVMMMMCDXX - and the object that i'm currently using to text you. In airplane mode, it hovers at a convenient distance to provide a truly hands-free experience.";
            cellphone.aliases.Add("phone");
            cellphone.aliases.Add("iphone LXVMMMMCDXX/--");
            kitchen.things.Add(cellphone);

            Supporter kitchenTable = new Supporter("The kitchen table");
            kitchen.things.Add(kitchenTable);
            kitchenTable.description = "An unlikely place to put anything useful.";

            Thing potato = new Thing("a potato");
            Thing lomato = new Thing("a lomato?");


            Thing chonkChart = new Thing("The chonk chart");
            chonkChart.description =
                @"A small holocard with a gaudy label that says '[b]Now That's What I Call Chonk! Vol. 1[/b]'
                I think I can use this with the Chonkédex app on my [cellphone].";

            kitchenTable.things.Add(chonkChart);


            Instead(taking).Condition((Thing noun) => (noun.edible || noun.portable) && (noun.lit || noun.scenery) ).Say("I lean over the [chart], but my[cellphone] zips in and absorbs the holocard before i can chow down.");
        }
    }

}
