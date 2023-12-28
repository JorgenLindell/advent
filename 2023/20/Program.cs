using System.Diagnostics;
using common;

namespace _20;

internal class Program
{

    const StringSplitOptions Tidy = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
    static readonly string xinput = @"
broadcaster -> a
%a -> inv, con
&inv -> b
%b -> con
&con -> output";

    static readonly string yinput = @"
broadcaster -> a, b, c
%a -> b
%b -> c
%c -> inv
&inv -> a";
    static int buttonClicks;

    static async Task Main(string[] args)
    {
        var strings = input.Split("\r\n", Tidy);
        Module.Modules = strings.Select(s => Module.CreateInstance(s))
            .ToDictionary(x => x.Name, x => x);
        Module.Modules["button"] = Module.CreateInstance("button -> broadcaster");
        Module.Setup();

        // Module.ListDeps("rx");
        // return;

        AnalyseDiscovered();
        return;

        Module.ReportedModules.Add("ln");
        Module.ReportedModules.Add("dr");
        Module.ReportedModules.Add("zx");
        Module.ReportedModules.Add("vn");

        var n = 100000000;
        var nn = n / 1000;
        var sw = new Stopwatch();
        sw.Start();
        var loops = 0;
        for (buttonClicks = 0; buttonClicks < n; buttonClicks++)
        {
            if (Module.Trace) Debug.WriteLine("-----------------");
            Module.Modules["button"].Receive(null);
            Module.ProcessUntilEmpty();
            if (buttonClicks % nn == 0)
            {
                Console.WriteLine($"{++loops} {sw.Elapsed:g} {(243221023462303 / ((double)loops * nn)) * sw.Elapsed:G}");
            }
        }


        Debug.WriteLine($"stats: Low:{Module.SentStats[Signal.SignalValue.Low]}  High:{Module.SentStats[Signal.SignalValue.High]} product={Module.SentStats[Signal.SignalValue.Low] * (long)Module.SentStats[Signal.SignalValue.High]}");

    }

    private static void AnalyseDiscovered()
    {
        // this periodicity is detected by tracing the changes for the relevant Conjunction nodes
        long drD = 3863;
        long vnD = 3943;
        long zxD = 3989;
        long lnD = 4003;
        var drS = drD - 1;
        var vnS = vnD - 1;
        var zxS = zxD - 1;
        var lnS = lnD - 1;
        long steps = drS;
        steps += drD * vnD * zxD * lnD - drD - 1;
        var drM = steps % drD;
        var vnM = steps % vnD;
        var zxM = steps % zxD;
        var lnM = steps % lnD;
        long best = 1000000;
        while (!(drM == drS
                       && vnM == vnS
                       && zxM == zxS
                       && lnM == lnS))
        {
            steps += 1;
            drM = steps % drD;
            vnM = steps % vnD;
            zxM = steps % zxD;
            lnM = steps % lnD;
            var totDiff = (drS - drM) + (vnS - vnM) + (zxS - zxM) + (lnS - lnM);
            Console.WriteLine($"{steps}:{totDiff}     {(drS - drM)} {(vnS - vnM)} {(zxS - zxM)} {(lnS - lnM)} ");
            if ((drM == drS
                 || vnM == vnS
                 || zxM == zxS
                 || lnM == lnS))
            {
                if (totDiff < best)
                {
                    best = totDiff;
                }
            }
        }
        // at steps 
        Console.WriteLine(steps + 1);
    }


    internal abstract class Module(string name, IEnumerable<string> receivers)
    {
        public static Dictionary<string, Module> Modules { get; set; }
        public static bool Trace = false;
        public static Queue<Signal> PulseQueue = new();

        public static Dictionary<Signal.SignalValue, long> SentStats = new()
        {
            { Signal.SignalValue.Low, 0 },
            { Signal.SignalValue.High, 0 }
        };
        public static Module CreateInstance(string line)
        {
            var split = line.Split(" ,".ToCharArray(), Tidy);
            if (split[0] == "broadcaster")
            {
                return new BroadCast("broadcaster", split.Skip(2));
            }
            if (split[0] == "button")
            {
                return new Button("button", "broadcaster");

            }

            if (split[0][0] == '%')
            {
                return new FlipFlop(split[0].Substring(1), split.Skip(2));

            }
            if (split[0][0] == '&')
            {
                return new Conjunction(split[0].Substring(1), split.Skip(2));

            }

            return new Untyped(split[0]);
        }

        public static void Setup()
        {
            var listOfMissing = new List<(Module module, string receiver)>();
            foreach (var module in Modules.Values)
            {
                foreach (var receiver in module.Receivers.ToList())
                {
                    if (Modules.TryGetValue(receiver, out var receiverModule))
                    {
                        receiverModule.SetupInput(module);
                        module.SetupOutput(receiverModule);
                    }
                    else
                    {
                        Debug.WriteLine("receiver " + receiver + " is not defined");
                        listOfMissing.Add((module, receiver));
                    }
                }
            }

            foreach (var v in listOfMissing)
            {
                if (!Modules.ContainsKey(v.receiver))
                    Modules[v.receiver] = Module.CreateInstance(v.receiver);

                if (Modules.TryGetValue(v.receiver, out var receiverModule))
                {
                    receiverModule.SetupInput(v.module);
                    v.module.SetupOutput(receiverModule);
                }
            }

            SentStats = new()
            {
                { Signal.SignalValue.Low, 0 },
                { Signal.SignalValue.High, 0 }
            };


        }

        public static HashSet<string> ReportedModules = new HashSet<string>();
        public static void Report(string s, string message)
        {
            if (ReportedModules.Contains(s))
            {
                Console.WriteLine(message);
            }
        }

        public static void ListDeps(string rx)
        {
            var pref = "";
            var module = Modules[rx];
            Debug.WriteLine(pref + module.ToString());
            List<Module> inputs = module.Inputs.Values.ToList();
            while (inputs.Count > 0)
            {
                var nextInputs = new List<Module>();
                foreach (var mod in inputs)
                {
                    Debug.WriteLine(pref + mod.ToString());
                    nextInputs.AddRange(mod.Inputs.Values);
                }
                inputs = nextInputs;
                pref += " ";
            }
        }

        public string Name { get; } = name;
        public List<string> Receivers { get; } = receivers.ToList();
        public Dictionary<string, Module> Inputs { get; } = new();
        public Dictionary<string, Module> Outputs { get; } = new();

        protected virtual void SetupOutput(Module receiverModule)
        {
            Outputs.Add(receiverModule.Name, receiverModule);
        }

        protected virtual void SetupInput(Module senderModule)
        {
            Inputs.Add(senderModule.Name, senderModule);
        }

        public abstract void Receive(Signal signal);

        public static int Process()
        {
            if (PulseQueue.TryDequeue(out Signal? signal))
            {
                if (Trace) Debug.WriteLine($" {signal.Src.Name} --{signal.Value}--> {signal.Dest.Name}");
                SentStats[signal.Value]++;
                signal.Dest.Receive(signal);
            }
            return PulseQueue.Count;
        }

        public static void ProcessUntilEmpty()
        {
            while (Process() > 0) { }
        }

        public void Send(Signal signal)
        {
            PulseQueue.Enqueue(signal);
        }
        public void Send(Signal.SignalValue value)
        {
            foreach (var receiver in Outputs.Values)
            {
                Send(new Signal(this, receiver, value));
            }
        }

        public override string ToString()
        {
            return $"{Name} {this.GetType().Name}  -> {this.Receivers.StringJoin(", ")}";
        }
    }

    internal class FlipFlop(string name, IEnumerable<string> receivers) : Module(name, receivers)
    {
        public enum FlipFlopState { On, Off }
        public FlipFlopState State { get; private set; } = FlipFlopState.Off;

        public override void Receive(Signal signal)
        {
            if (signal.Value == Signal.SignalValue.Low)
            {
                if (State == FlipFlopState.On)
                {
                    State = FlipFlopState.Off;
                    Send(Signal.SignalValue.Low);
                }
                else
                {
                    State = FlipFlopState.On;
                    Send(Signal.SignalValue.High);
                }
            }
        }
    }
    internal class Conjunction(string name, IEnumerable<string> receivers) : Module(name, receivers)
    {
        public Dictionary<string, Signal.SignalValue> ConjInputs { get; } = new();

        private List<long> LastSent = new List<long>();
        private long FirstSentLow = 0;
        public override void Receive(Signal signal)
        {
            ConjInputs[signal.Src.Name] = signal.Value;
            Signal.SignalValue signalValue;
            if (ConjInputs.Values
                .All(x => x == Signal.SignalValue.High))
            {
                signalValue = Signal.SignalValue.Low;
                if (FirstSentLow == 0)
                {
                    FirstSentLow = buttonClicks;
                    Console.WriteLine(Name + " sends FIRST low at click " + buttonClicks);
                }
            }
            else
            {
                signalValue = Signal.SignalValue.High;
                LastSent.Add(buttonClicks);
                LastSent = LastSent.Take(25).ToList();
                var last = LastSent.TakeLast(6).ToList();
                var diffs = last.Skip(1).Take(5).Select((v, i) => (v - last[i]).ToString()).StringJoin(",");

                var kj = "" + ((Conjunction)Modules["kj"]).ConjInputs.Select(pair => $" {pair.Key}:{pair.Value}")
                    .StringJoin();


                Module.Report(Name, $"{Name} sends {signalValue} at click {buttonClicks} : {diffs} | {kj}");

            }

            Send(signalValue);
        }


        protected override void SetupInput(Module senderModule)
        {
            base.SetupInput(senderModule);
            ConjInputs[senderModule.Name] = Signal.SignalValue.Low;
        }
    }
    internal class BroadCast(string name, IEnumerable<string> receivers) : Module(name, receivers)
    {
        public string Name { get; }

        public override void Receive(Signal signal)
        {
            Send(signal.Value);
        }
    }
    internal class Button(string name, string receivers) : Module(name, [receivers])
    {

        public override void Receive(Signal signal)
        {
            Send(Signal.SignalValue.Low);
        }
    }
    internal class Untyped(string name) : Module(name, Array.Empty<string>())
    {

        public override void Receive(Signal signal)
        {
            if (signal.Value == Signal.SignalValue.Low)
                Console.WriteLine("\r\n" + Name + ":" + signal.Value + " clicks " + buttonClicks);
        }
    }
    internal class Signal(Module src, Module dest, Signal.SignalValue value)
    {
        public enum SignalValue { Low, High }
        public Signal(Module src, string dest, SignalValue value)
            : this(src, Module.Modules[dest], value) { }

        public Module Src { get; } = src;
        public Module Dest { get; } = dest;
        public SignalValue Value { get; } = value;
    }


    private static readonly string input = @"
&pr -> pd, vx, vn, cl, hm
%hm -> qb
%nm -> dh, jv
%lv -> jv, tg
%dg -> tm, jm
%mt -> jv, zp
&ln -> kj
&kj -> rx
&dr -> kj
%dx -> ts
&qs -> kf, dr, sc, rg, gl, dx
%dh -> jv, mc
%rg -> qs, vq
%kt -> jv, mt
%lh -> qs, dl
%tp -> pf, jm
%bf -> vx, pr
%mv -> qs, gl
%ts -> ng, qs
%kf -> dx
%gv -> jm, km
%dl -> qs
%nd -> dg
%km -> jm
%ns -> pr, pn
%gl -> kf
%pd -> pr, jp
%xv -> nd, jm
%hf -> nm
%vx -> ns
%vq -> bs, qs
%sc -> mv
&jv -> hj, rc, kt, ln, zp, hf
%rc -> hj
%jp -> mx, pr
%mf -> gv, jm
&zx -> kj
%tg -> jv
%bs -> sc, qs
%ng -> qs, lh
%tk -> pr
%qb -> bf, pr
%pn -> pr, cb
%cl -> hm
%pb -> tp
broadcaster -> kt, pd, xv, rg
&jm -> pb, tm, zx, mk, xv, nd
%vc -> jv, hf
%mc -> jv, lv
%mk -> pb
%tm -> mh
%cb -> pr, tk
%hj -> vc
%zp -> rc
%mh -> mk, jm
%pf -> mf, jm
%mx -> cl, pr
&vn -> kj";

}