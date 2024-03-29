﻿using System.Diagnostics;
using common;
using static _17.Program;

namespace _17;

internal class Program
{

    const StringSplitOptions Tidy = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

    static readonly string xinput = @"
px{a<2006:qkq,m>2090:A,rfg}
pv{a>1716:R,A}
lnx{m>1548:A,A}
rfg{s<537:gd,x>2440:R,A}
qs{s>3448:A,lnx}
qkq{x<1416:A,crn}
crn{x>2662:A,R}
in{s<1351:px,qqz}
qqz{s>2770:qs,m<1801:hdj,R}
gd{a>3333:R,R}
hdj{m>838:A,pv}

{x=787,m=2655,a=1222,s=2876}
{x=1679,m=44,a=2067,s=496}
{x=2036,m=264,a=79,s=2244}
{x=2461,m=1339,a=466,s=291}
{x=2127,m=1623,a=2188,s=1013}";

    static async Task Main(string[] args)
    {
        var strings = input.Split("\r\n\r\n", Tidy);
        List<Item> items = strings[1].Split("\r\n", Tidy)
            .Select(l => l.Trim("{}".ToCharArray()).Split(",")
                .Select(e =>
                {
                    var prop = e.Split("=", Tidy);
                    return (name: prop[0], value: prop[1].ToLong() ?? 0);
                }).ToDictionary(t => t.name, t => t.value))
            .Select(d => new Item(d))
            .ToList();

        Dictionary<string, RuleSet> flows = strings[0].Split("\r\n", Tidy)
            .Select(l =>
            {
                var split = l.Split("{},".ToCharArray(), Tidy);
                var name = split[0];
                var ruleset = split.Skip(1).Select(r => new Rule(r)).ToList();

                return new RuleSet(name, ruleset);
            })
            .ToDictionary(x => x.Name, x => x);

        Part1(items, flows);
        await Part2Async(flows);

        async Task Part2Async(Dictionary<string, RuleSet> ruleSets)
        {
            Comparer<Range> rangeComparer =
                Comparer<Range>.Create((range, range1) => range.Start.Value.CompareTo(range1.Start.Value));

            Dictionary<string, SortedSet<Range>> ranges = new()
            {
                ["x"] = new(rangeComparer) { new(1, 4000) },
                ["m"] = new(rangeComparer) { new(1, 4000) },
                ["a"] = new(rangeComparer) { new(1, 4000) },
                ["s"] = new(rangeComparer) { new(1, 4000) }
            };

            foreach (var ruleSet in ruleSets)
            {
                foreach (var rule in ruleSet.Value.Rules)
                {
                    if (rule.Prop == "")
                        continue;
                    var sortedSet = ranges[rule.Prop];
                    var v = (Index)rule.Value;
                    int pos;
                    if (rule.Oper == ">")
                    {
                        pos = v.Value + 1;
                    }
                    else
                    {
                        pos = v.Value;
                    }
                    var currentRange = sortedSet.FirstOrDefault(ir => ir.Start.Value <= pos && pos <= ir.End.Value);
                    if ((currentRange.Start.Value == 0)
                        || (currentRange.Start.Value > pos - 1)
                        || (currentRange.End.Value < pos))
                        continue;
                    var newRanges = new List<Range>
                        {
                            new(currentRange.Start.Value, pos - 1),
                            new(pos, currentRange.End)
                        };
                    sortedSet.Remove(currentRange);
                    newRanges.ForEach(nr => sortedSet.Add(nr));
                }
            }

            var batchLength = ranges["x"].Count / 50;
            long accepted = 0;

            var threadNr = 0;
            List<List<List<Range>>>? topRanges = ranges["x"].ToBatches(batchLength + 1).ToBatches(15).ToList();
            var loops =  topRanges.Sum(r => r.Count);
            foreach (var xRanges in topRanges)
            {
                var innerRanges = xRanges.ToList();
                var tasks = innerRanges.Select(xRange =>
                {
                    var range = xRange;
                    var threadId = ++threadNr;
                    Debug.WriteLine("Starting "+(threadId*100/loops)+"%");
                    return Task.Run(() => Permutate(threadId, range, ranges, flows));
                });
                var results = await Task.WhenAll(tasks);
                accepted += results.Sum(a => a);
                results = null;
                tasks = null!;
            }

            Debug.WriteLine($"accepted part2: " + accepted);
            static List<Range> RangeSplit(Range currentRange, int pos)
            {
                var ranges = new List<Range>
                {
                    new(currentRange.Start.Value, pos - 1),
                    new(pos, currentRange.End)
                };
                return ranges;
            }
        }
    }

    private static long Permutate(int threadId, List<Range> xRange, Dictionary<string, SortedSet<Range>> ranges,
        Dictionary<string, RuleSet> flows)
    {
        var sw = new Stopwatch();
        sw.Start();
        long accepted = 0L;
        long subCount = ranges["m"].Count * ranges["a"].Count * (long)ranges["s"].Count;
        var count = xRange.Count * subCount;
        xRange.ForEach((x, xi) =>
        {
            var lngthX = (long)x.SimpleLength();
            var startValueX = x.Start.Value;
            ranges["m"].ForEach((m, mi) =>
            {
                var lngthM = lngthX * (long)m.SimpleLength();
                var startValueM = m.Start.Value;
                ranges["a"].ForEach((a, ai) =>
                {
                    var lngthA = lngthM * (long)a.SimpleLength();
                    var startValueA = a.Start.Value;
                    ranges["s"].ForEach((s, si) =>
                    {
                        var simpleLength = lngthA * s.SimpleLength();
                        var flow = EvalFlow(flows,
                            new Item(startValueX,
                                startValueM,
                                startValueA,
                                s.Start.Value, simpleLength));
                        if (flow == "A")
                        {
                            accepted += simpleLength;
                        }
                    });
                });
            });
            sw.Stop();
            Debug.WriteLine($"{DateTime.Now:T} T{threadId}: {xi}:{sw.Elapsed.TotalMinutes} ");
            sw.Start();

        });
        xRange.Clear();
        Debug.WriteLine($"T{threadId}: returning {accepted}");
        return accepted;
    }

    private static List<Item> Part1(List<Item> list, Dictionary<string, RuleSet> ruleSets)
    {
        var accepted = new List<Item>();
        foreach (var item in list)
        {
            var flow = EvalFlow(ruleSets, item);
            if (flow != "R")
                accepted.Add(item);
        }
        Debug.WriteLine($"accepted: " + accepted.Sum(e => e.Values.Sum()));
        return accepted;
    }

    private static string EvalFlow(Dictionary<string, RuleSet> ruleSets, Item item)
    {
        var flow = "in";
        while (!flow.In("R", "A"))
        {
            flow = ruleSets[flow].Eval(item);
        }

        return flow;
    }


    internal class RuleSet
    {
        public string Name { get; }
        public List<Rule> Rules { get; }

        public RuleSet(string name, List<Rule> rules)
        {
            Name = name;
            Rules = rules;
        }
        public string Eval(Item item)
        {
            var resp = Rules.First(r => r.Cond(item) != "");
            return resp.RetVal;

        }
    }

    internal class Rule
    {
        public Rule(string s)
        {
            var split = s.Split(":<>".ToCharArray(), Tidy);
            Prop = "";
            Value = 0;
            RetVal = "";
            Oper = "";
            if (split.Length == 1)
            {
                RetVal = split[0];
                Cond = (vd) => RetVal;
            }
            else
            {
                Prop = split[0];
                Value = split[1].ToInt() ?? 0;
                RetVal = split[2];
                if (s.Contains('>'))
                {
                    Oper = ">";
                    Cond = (vd) => vd[Prop] > Value ? RetVal : "";
                }
                else if (s.Contains('<'))
                {
                    Oper = "<";
                    Cond = (vd) => vd[Prop] < Value ? RetVal : "";
                }
            }
        }

        public string Oper { get; }

        public string RetVal { get; }

        public Func<Item, string> Cond { get; }
        public string Prop { get; }
        public int Value { get; }
    }
    internal class Item
    {
        public long XValue { get; set; }
        public long MValue { get; set; }
        public long AValue { get; set; }
        public long SValue { get; set; }
        public long Length { get; set; }
        public long[] Values => [XValue, MValue, AValue, SValue];

        public Item(long xValue, long mValue, long aValue, long sValue, long length = 1)
        {
            XValue = xValue;
            MValue = mValue;
            AValue = aValue;
            SValue = sValue;
            Length = length;
        }

        public Item(Dictionary<string, long> values)
        {
            values.ForEach((v, i) =>
            {
                switch (v.Key)
                {
                    case "x":
                        XValue = v.Value;
                        break;
                    case "m":
                        MValue = v.Value;
                        break;
                    case "a":
                        AValue = v.Value;
                        break;
                    case "s":
                        SValue = v.Value;
                        break;
                };

            });
        }

        public long this[string prop]
        {
            get
            {
                return prop switch
                {
                    "x" => XValue,
                    "m" => MValue,
                    "a" => AValue,
                    "s" => SValue,
                    _ => throw new ArgumentOutOfRangeException(nameof(prop), prop, null)
                };
            }
        }
    }

    private static readonly string input = @"
lx{a>1404:kq,a>736:hmc,hbb}
db{a<2257:A,s>3446:R,R}
lhh{m<2267:zt,x>1979:A,vv}
vgt{m<834:R,a>1057:R,a>1009:R,R}
zfh{x<2428:gcf,s>3274:qxr,hm}
fvr{s<668:vgt,x>2691:A,vr}
tgr{a<459:A,a<656:R,x>2162:R,A}
lfp{x<2471:zm,fcx}
psk{s<2112:gtm,s>2147:A,x>2023:A,A}
qxd{m<2102:R,x<112:A,s<2655:R,A}
vd{a>3406:A,s>1912:R,x<2685:A,R}
llc{m<865:R,x>941:A,A}
vp{m<1653:R,vz}
zn{s<2063:A,m<3691:A,A}
vcz{a<979:R,s<3048:A,m>1098:vx,R}
tbc{a>3135:hsq,gkg}
mqb{s>1566:zp,a<3429:rtq,m<1267:jt,A}
ck{m>2005:xcs,s<2459:vmf,m<1263:chq,qtd}
jl{m<3119:R,x>2343:A,A}
zr{s<3108:pxz,s<3642:bj,zkf}
lb{s<2067:xzq,A}
zl{a<488:A,x>288:A,a>687:A,R}
rjm{s>3121:ndn,m<1642:sc,x>736:pzn,qdd}
lzj{s<580:R,a<1412:A,R}
gk{a<1352:R,R}
mr{a>1063:qzd,a<709:qhn,dg}
fj{x>1265:R,x>1101:A,vc}
dnv{m<1476:gqh,x<580:jv,m<2499:hx,hf}
sht{x>1264:A,A}
kz{a>298:A,A}
sd{x>661:A,x>649:R,A}
dp{x>1065:dk,gk}
tgf{x<2403:R,a>468:R,R}
hf{m>3405:ggj,A}
fdn{x>2281:qf,m<1336:gj,rx}
nxs{m>186:R,a>1564:R,m<158:A,A}
fg{a>1625:ql,xvz}
vdp{x>2318:A,s>2604:R,R}
vs{s<1883:st,x>491:kf,s<2827:hzd,ndz}
dlp{s<2936:A,x<609:R,m>1864:A,R}
prf{x<2197:A,m<3139:fkf,R}
dr{s<3034:A,x>907:A,m>1649:A,A}
ndn{x>747:R,s<3506:A,s>3762:R,A}
zx{x<236:R,s>1090:hv,x<339:vt,A}
kx{s<3240:R,s<3502:R,A}
zc{s>3327:A,m<627:A,A}
gfk{x<504:A,s<3472:A,s<3707:R,A}
qz{x>536:R,x<530:R,A}
mb{a<969:A,a<1250:A,A}
qf{m<2127:zmc,cms}
btm{s<2819:A,dt}
lj{m<2672:A,m>2781:R,R}
zd{m<1700:A,R}
jzc{m<1092:A,A}
jh{s<818:rxn,s>1161:kfd,m>3200:mv,jr}
xrt{a>1423:A,A}
thp{a>2225:R,a>2075:R,A}
hjq{s<357:rsj,x>2072:vhr,m>3475:R,R}
cmv{x<2737:R,A}
qt{x>2842:jd,s<1418:gg,a<2229:fnv,lfp}
zzm{s>1454:A,A}
nz{m<69:A,m>95:R,R}
jm{s>2476:kk,x>128:R,x<67:zj,jp}
qk{x<2107:A,s<1316:R,m>230:R,R}
fsh{s>3023:R,a>268:R,m>2009:A,R}
qdc{s<1204:R,a>2666:R,A}
zkf{x>1313:A,A}
xxn{a<3799:nmp,x>1219:A,A}
hnv{x<3439:cgz,s<2435:fz,kff}
mqd{s>2154:R,R}
kp{x>2602:A,R}
gj{m<541:lg,a>776:kbz,vfs}
gs{s<1631:A,a>1030:R,A}
lf{m<3374:A,R}
cp{a<1961:A,A}
tv{s<3138:R,A}
njr{s>2627:A,R}
hbb{m<2014:R,x>3569:rl,zzm}
qzd{a<1612:R,m<2664:cp,s<3066:R,zlv}
qcm{m>2532:R,R}
qqs{m>2638:A,R}
kg{x<165:jc,bc}
cb{a<1762:A,R}
td{a<417:R,gc}
kff{a>3189:dmh,bb}
rjf{m>2125:R,clz}
tmf{s>2692:sf,a>2779:ch,a<1989:qzc,glh}
ls{x<1643:qqp,s<1673:A,smx}
dmb{s<2658:A,A}
vnv{x>2066:qrj,dql}
dj{m>2068:R,m<1482:A,a<2599:thp,zd}
sq{s<2616:R,a<351:A,x<559:A,R}
ccn{a<281:R,m<874:A,x>2200:A,R}
sv{s>2080:kz,R}
th{s>555:A,a<3192:A,x<1944:A,R}
zz{x<192:R,A}
tq{a<1897:R,a<3111:R,R}
pl{x>960:A,s>2685:R,R}
mpc{x>1202:A,s>945:A,m>1712:A,A}
kbn{s>938:gcs,phb}
tkr{s>1688:cm,ncs}
fcq{x>54:R,a>2494:A,A}
xpg{s<216:A,s>253:R,x>2271:A,R}
dc{x>2221:dkx,s<584:vb,skp}
mv{a<3192:rfb,x<2665:R,s>950:R,fsn}
ldj{a>380:R,x<1203:A,a<210:R,R}
vjp{m<3124:gqd,zn}
gsl{a<3202:R,m<2591:A,R}
ld{m<2096:rqz,A}
krq{x<2338:A,m<1285:R,A}
gcj{m>3160:A,R}
qxr{m<2435:R,R}
vb{x<2054:A,R}
rn{m<1428:A,a<1451:A,A}
hx{a>556:dlp,R}
xbb{x>1368:vn,a<845:gxh,ss}
vm{s<738:R,A}
gg{a<1889:dm,m>2646:fv,s>859:xz,rrz}
phj{x<2054:bs,x<2138:R,gf}
nx{a<3697:R,x>3188:R,A}
pzn{m<2625:R,A}
vx{a<1795:R,A}
hl{m<995:A,R}
mrc{a<592:A,s<2377:R,A}
std{a<780:hzt,x<689:bgq,rjm}
dd{m>1212:A,x>3610:xk,s<2107:nc,pm}
xlf{x>739:A,R}
kl{s<2227:R,A}
xtb{x>2700:A,a<2769:R,A}
bdd{s<2805:A,dnj}
rjj{a>2955:vp,x<2194:lq,dj}
nr{a<1387:R,a<3004:R,xj}
rns{m<2217:jn,x>592:lf,x>552:A,R}
dmh{a<3609:R,s>3010:R,x>3726:R,R}
ct{m>3649:A,a>2647:A,m<3567:R,A}
rqz{s>2769:R,x>221:R,A}
gcf{s>3251:R,s<3234:A,x<2406:A,R}
xm{a>1015:mhv,s<2272:ktc,ck}
cd{s<1692:jj,m>1032:dr,x>910:pl,A}
st{x<487:mzm,a>1400:rjn,kbn}
dcn{x>3495:vdd,vss}
dn{a>1508:R,m>294:A,R}
hs{a>2342:A,a>1813:R,A}
cm{x>1293:vnx,s<2488:vjp,nf}
glh{a>2285:R,x>628:R,a<2175:mqd,chv}
vv{s>3330:A,x<1931:A,a>1265:A,R}
gq{a>2040:sht,s>1440:ns,A}
llg{s<2539:lv,m<1141:kx,pc}
prp{a>1740:A,x<2495:R,zpm}
rm{a>1055:A,m>445:A,x>3758:A,A}
clz{a>2865:R,x<2606:R,R}
zvl{s<2106:R,a>844:A,x>268:A,kh}
xk{s>1762:A,s<782:A,s<1152:R,A}
nmp{s>2995:A,a<3682:A,m<1075:A,A}
hzd{a>2639:xkn,a<1047:rg,gp}
bj{a<3194:R,A}
ppm{s<2880:R,x>2613:R,x<2563:R,R}
txc{x>2136:R,a<601:R,R}
dnj{x>2771:R,a>3718:A,a<3484:A,R}
df{a>187:R,x>2613:R,x>2523:R,zg}
zmc{s<3665:A,nmr}
pq{m<1475:R,x<2066:A,a<3122:A,R}
nsd{s>1209:A,R}
bf{a<3252:dzb,R}
ncd{s<287:A,A}
fgt{x<2144:A,a<2514:A,R}
jkj{s>3681:gr,s<3567:xns,m<3093:br,vbs}
ztc{m<2807:A,fb}
bs{a<503:A,R}
ff{s>2620:zpv,kl}
dcm{m>1036:R,a>3212:A,R}
qpj{s<368:A,x>2724:R,x<2604:R,R}
xns{m>2941:A,s>3516:A,R}
jgt{m>2953:R,A}
xzq{s<1279:R,m>3139:R,x>3898:A,R}
lqc{x<1524:R,A}
pld{m>570:A,m<354:R,m>450:R,A}
mqn{s<2459:tk,a<2771:bz,qc}
lfl{a>3534:xzc,s<2283:R,hnk}
fr{s>975:R,A}
bxj{a>2638:A,R}
nmr{s<3805:A,m>825:A,A}
phb{s<563:qcm,a<770:A,qgz}
xzc{a<3785:R,x>2817:A,a>3882:R,R}
fmf{m<1529:A,s>3440:A,R}
ncs{s<851:fj,m<3523:mnb,m>3830:gx,nr}
xqm{m<341:R,R}
gc{s<1991:A,s<2674:R,A}
pm{m<1128:A,A}
ggm{a<90:A,a<127:A,a<159:A,A}
kkb{s<2784:A,m<1652:R,A}
brc{m<832:R,m<1359:dcm,mnv}
ng{a<624:A,s>384:R,R}
csx{a>603:qz,a<325:R,mf}
nll{a<1393:A,A}
kh{m>1895:R,a<687:R,R}
jph{x>345:R,A}
dql{x>1966:A,m>1500:fmm,nn}
lqp{s>51:A,x>2315:A,A}
qhn{s<2935:dlz,s<3079:fsh,a<297:lcb,tv}
pdj{x<638:R,m<2547:bx,m>3174:tht,xlf}
xrb{a<3213:R,s<2172:R,A}
js{s<1184:A,m<365:qk,A}
mf{x<533:R,a<496:R,x>538:R,A}
gx{s<1294:A,R}
gjp{x<2225:lhh,x<2382:nh,s<3303:zfh,dvn}
qr{m<1507:A,x<608:R,R}
qlf{a>2578:R,s>3419:jsl,R}
nn{s>2984:A,R}
fd{s>2464:R,x>3006:R,a<3365:A,R}
bph{s>3672:A,nks}
dt{m<3237:R,s>3461:A,R}
dzb{s>195:A,A}
bk{x>243:sk,a>364:qxd,m<2388:ksg,R}
rq{m>2250:np,x>1380:R,bll}
hsq{a>3639:A,x<1969:R,a<3468:R,A}
qmc{s>2060:R,s>1064:A,s<367:R,vm}
lk{s<1735:R,x<3704:R,a>3402:A,A}
dvn{x<2416:xml,m>1970:A,R}
nrc{a<3519:A,x>2535:R,R}
chv{a<2235:R,a<2262:R,x<558:R,A}
vbg{a<3901:R,s>2950:A,a>3944:A,A}
jsh{x>2518:nrc,m>2653:nm,x>2494:A,mhs}
rtz{s<810:R,a<213:pp,ccn}
bgq{a>1077:A,dmb}
fh{x<323:A,R}
qlb{a>3125:A,x<2379:A,m>2986:R,R}
rk{a<1222:A,A}
bx{s<514:R,A}
hn{m>1531:A,A}
czf{s<3277:R,a>770:fxt,fbx}
bc{x<334:A,a<3044:A,x>392:A,A}
bfb{a>3735:vbg,x>2610:R,a>3551:R,hgd}
vmf{a>582:qlj,s>2377:R,A}
vdd{x>3763:lb,a<2201:qg,ghr}
jsl{a<1408:A,s<3621:R,R}
zg{x>2462:R,s>527:R,A}
lkx{s<3593:R,A}
rjn{s>843:rns,pdj}
tj{m>774:A,m>414:R,R}
vn{m<947:ls,a>674:qmc,td}
dls{s<1901:R,s>2055:A,a>265:A,A}
zf{m<1025:A,m>1354:R,A}
xcq{s<1879:A,x<2466:xv,a<1509:jgt,bvd}
rfb{a>2720:A,A}
nm{s<2617:R,s>3220:A,m>2855:R,R}
vhc{a<2549:A,x<231:A,a>3163:A,R}
mzm{m<1650:zx,hhm}
txd{m<3053:R,R}
qjv{m>1344:rn,m<1135:jzc,pfc}
vnx{m>3229:lqc,s<2587:cjx,R}
cgz{x<3157:fd,a>3139:R,kps}
fv{x>2503:jh,jk}
vh{x<2039:R,gsl}
ghr{a<3039:fkk,m<2935:jlb,R}
fcx{m<1727:llg,a<3251:ssc,x<2670:ks,fq}
fsn{m<3586:A,x<2764:A,A}
cms{x<2534:gcj,m>3172:R,x<2725:A,bt}
gvx{a>341:A,m>1762:kp,a>175:A,ggm}
txm{a>528:mpc,fr}
qlj{m>767:A,x<2446:A,x>2616:A,R}
fmm{a>804:R,m<2364:A,A}
hgd{x>2579:A,a>3397:A,R}
mg{s<1092:A,m<1987:R,m>2282:A,R}
xt{a<3754:A,A}
xrj{s>516:A,x>2541:A,R}
dqp{s>603:jl,a>823:kb,x<2201:phj,zql}
skv{x>3051:nx,m<1239:A,A}
pxz{m<1003:A,a<3142:R,x<1447:A,A}
vc{a>2144:A,m>3327:A,x<934:R,R}
bv{x>2618:R,a<553:R,R}
tzg{x>2469:mr,x<2180:vnv,kld}
hmc{x>3375:R,zvr}
xkg{s>344:gbt,x<2074:bf,xqt}
lcb{m>1499:A,s>3143:R,m>500:A,A}
vt{a<1669:R,m<617:R,m<1002:R,R}
ql{m>3355:R,A}
zj{m<2357:A,s>2123:R,m<3174:A,R}
vvz{a<1165:R,R}
jn{m<1160:R,R}
tht{x>726:R,m>3646:A,A}
jbz{s>2031:rm,m>379:rk,dn}
hc{m>1533:lx,m>1017:pj,a>2479:hnv,rr}
fkk{s<1826:A,R}
xj{x>1180:R,m>3697:A,R}
rhx{x>300:db,m<1063:nt,x>140:qlp,sz}
nt{m>595:tq,R}
bz{m>1444:clf,x>1456:qb,qn}
jp{a<2204:A,A}
zvr{m<1923:R,R}
hq{a>3276:xt,A}
zt{a<1305:A,s<3305:R,a<1726:A,R}
zfd{a>2110:R,m>796:A,R}
ppq{m>2699:A,s<345:A,m>2352:R,R}
xml{a<1012:A,s>3360:R,R}
rrz{x>2453:kv,xkg}
xb{s>2088:R,a<3145:A,s>2008:A,A}
qtn{x>2115:dls,m<1847:gpz,xhg}
dgj{x<2212:A,s<2993:R,a<545:R,R}
xqr{m>1282:hz,s>2660:R,A}
dm{m>1869:dqp,a>929:skf,x<2420:tp,kjs}
ktc{a>548:lcx,x>2408:gvx,qtn}
mrb{s>3436:R,m>3788:R,a<1445:A,ct}
ssc{s>2983:kn,m>2663:xtb,rjf}
rt{m>1702:ztc,s>3110:xhf,brc}
jbg{m<307:A,a>2460:R,A}
nc{x<3431:A,m<1098:A,A}
rsf{m>2206:R,m>2067:R,s>336:R,R}
nvc{s<645:sr,m>901:hn,jbg}
sf{x>614:R,x>559:A,x>519:R,gfk}
nks{s>3596:A,R}
xfz{x<2509:qts,A}
br{x<1947:R,m<2456:R,x<2005:R,A}
qg{m>2981:A,a>1017:R,cvh}
xhg{m>3051:R,s>1928:A,s<1595:R,A}
pp{s>1164:A,m>1167:R,A}
gp{x>241:pt,a>1721:jm,jpf}
cv{s>1481:R,A}
dk{s>1760:A,R}
stj{s<3357:czf,xf}
zbr{s<1138:R,x<988:A,R}
rx{x<2036:jkj,a>790:bph,mn}
fz{x<3726:xqm,x<3891:vrv,m>541:cv,R}
fzd{a>961:R,R}
hv{m>602:A,A}
gqd{s>2163:A,R}
rtq{x<1214:R,x>1474:R,x>1346:R,R}
dkx{s<802:A,a>1604:A,A}
tjk{x<2291:js,m<577:hq,s>1117:cgq,pxb}
jv{m>2425:A,a<773:sq,s<3062:ccv,A}
in{x<1853:qkm,qt}
xvz{x<3065:R,x>3211:R,A}
gf{m<2908:A,m<3533:R,x>2163:A,R}
jlb{x>3589:A,A}
kld{m>2008:drv,x<2280:vcz,clh}
qtd{s>2609:R,m<1650:A,s>2540:tgf,R}
dg{m>2295:R,m>1397:R,s>2971:pld,ppm}
bb{a>2721:A,a>2610:A,R}
sc{m>971:R,a<1018:A,s<2326:R,A}
vrv{m<520:R,s>1186:R,A}
qrj{x<2128:A,A}
kps{s<2243:R,a>2761:A,R}
tk{s<1211:nvc,s>1893:flt,a<2821:gq,mqb}
xf{a<980:R,m>2243:txd,x<2698:A,qs}
dq{s<655:A,x<2647:A,A}
drv{x>2342:R,x>2246:A,a<1276:dgj,A}
pc{s>3052:fmf,m<1526:R,kkb}
kb{a>1338:ppq,a>1052:R,m<2799:rsf,fzd}
mn{m>2276:R,tgr}
cs{x<1977:R,x>2102:R,R}
clh{x<2391:krq,s>2930:vvz,a>916:R,R}
vr{x<2612:A,x>2662:R,A}
hhm{x<305:R,R}
xgr{s<2578:btb,a<3527:A,zz}
bll{a>3087:R,a<2967:R,m>1959:A,A}
qgz{s>697:A,A}
kk{m<2351:A,A}
hp{m>117:nxs,s>2061:R,nz}
gcs{a>777:R,a<309:vq,a>561:R,rrl}
pt{a<1750:R,s<2214:R,jph}
sk{x<403:A,R}
hnk{x<2806:A,A}
ktp{m<2699:A,x<2246:A,R}
zp{x>1310:A,s>1712:A,R}
rrl{a>424:R,s<1317:A,A}
pfc{s<1706:A,R}
zfg{a<2412:A,x>259:R,a>3293:A,R}
vss{s<1538:nll,s>3080:qlf,fg}
sgm{x>2242:R,A}
jk{s<553:hjq,prf}
clf{x>1280:R,fkj}
qkm{x>798:pmz,vs}
kf{a>1463:tmf,x>624:std,x<547:cg,dnv}
fl{s<1516:lzj,m>883:R,a<1417:R,zfd}
vhb{s<859:A,R}
tfs{s<3566:A,m>676:A,R}
tqh{m<955:A,a<413:R,m>1545:R,A}
vq{a>204:R,m<1428:A,m<3072:R,A}
lq{a<2246:mg,x<2053:qdc,fgt}
qfl{a>3287:ncd,x<2589:bxj,hb}
cz{m>847:pn,s<939:A,txc}
qzc{a<1679:R,R}
gcr{s<2862:R,a>3629:cmv,vg}
xqt{s>143:A,a>2616:sgm,a<2239:qtl,lqp}
fxt{s<3325:A,A}
lg{x<2108:A,A}
blk{x>2620:A,R}
skp{s<917:R,R}
hm{m>2517:A,A}
gqh{s<2662:A,x<579:mb,R}
pn{m>1205:A,A}
ks{x>2544:bfb,m>3168:xfz,jsh}
lcx{x<2368:R,R}
mhv{m<2179:prp,xcq}
jd{m<2365:hc,dcn}
zxs{a>787:R,A}
cg{x<523:ff,csx}
mmm{x<289:R,a<1597:R,R}
mhs{a>3742:R,R}
gkh{s>54:R,x<2057:R,A}
kbz{m>923:A,A}
zrt{x<2282:R,R}
xkn{s<2422:hgg,a<3270:kg,s<2679:xgr,ld}
sdt{m>1282:A,s<647:R,R}
np{s<3268:R,A}
qb{x<1696:hs,R}
nh{s>3372:cj,R}
sr{m<930:A,s>268:A,R}
qlp{m>1813:A,vhc}
vg{s<3463:R,x>2743:A,s>3769:R,R}
xz{m>938:rjj,tjk}
gr{x>1938:R,s<3792:R,R}
zjb{a<828:R,m<3107:A,a<1261:R,R}
btb{m<2271:A,m<2947:A,x<200:R,A}
flt{a>3039:tz,x<1260:R,A}
qc{a>3552:xxn,m>1606:rq,zr}
nv{x>2304:qlb,s<2054:qqs,s>2251:A,ktp}
mnv{s<2840:R,a<3252:A,A}
qn{a<2099:cb,x<1109:llc,a>2465:zc,A}
qqp{a>648:R,a<320:A,R}
sxc{a<3276:R,s<2070:R,a<3734:A,A}
dxj{s<3288:R,s>3739:A,m<3507:R,A}
cjx{a>2388:A,m>2919:A,a>1208:A,A}
kq{m>2059:hr,x<3435:A,a<2422:R,lk}
chq{a>467:vdp,a>235:qnl,s<2605:A,A}
jgg{s<943:R,m<2776:A,a>2215:R,R}
rvb{a<2629:A,R}
cc{m>3405:A,x<2700:R,a>403:A,R}
gtm{m<984:R,a<3397:A,R}
cj{s>3423:A,s>3390:A,a>1164:A,R}
dlz{a<436:A,a>543:A,a>472:R,R}
rr{x<3371:ts,m>622:fl,m<242:hp,jbz}
ksg{s>2576:R,m<1302:A,R}
cvh{s>1463:R,A}
jt{a<3710:R,m>632:A,a<3880:R,R}
fb{m<3463:A,m>3691:A,A}
cgq{s<1278:nsd,a<3292:xn,R}
skf{a>1304:dc,x<2505:vhb,fvr}
gpz{a>362:A,R}
xcs{m>3291:zrt,x<2297:A,m>2861:A,R}
vz{a<3389:A,m<2205:R,A}
xv{a>1743:R,x<2115:A,A}
gxh{x<1008:cd,s<1487:txm,x<1170:vfr,khm}
cx{m>1746:nv,zf}
ccv{a>1227:R,x>563:R,A}
jj{a<424:A,A}
vql{m<2504:R,A}
ss{a>1248:dp,gs}
ch{s>2389:qr,R}
bt{s<3767:A,s<3895:A,m<2783:A,R}
qs{m<1193:A,m>1716:A,A}
zlv{m>3454:A,m>2965:A,m<2837:R,R}
hzt{x>720:njr,s>2998:A,sd}
xn{s>1331:A,x<2634:R,R}
gkg{m>1949:R,m<970:R,m>1481:A,A}
fnv{s<2779:xm,s>3451:fdn,s<3219:tzg,mss}
xhf{x>2141:tj,a>3223:tfs,R}
mnb{x>1160:A,zbr}
hz{m>1407:R,R}
ndz{m<2525:rhx,nmd}
rl{s>2207:R,s>1055:R,R}
nf{a>1760:dxj,a<994:R,m>3227:A,A}
ggj{m>3681:R,a<754:R,x<606:A,A}
vfs{s>3663:A,hl}
kn{x>2623:lkx,rvb}
gbt{x>2115:sdt,x<2021:th,s<565:R,pq}
fkf{s<882:A,a<3165:R,R}
jz{m>3221:jdt,mmm}
sz{x<84:fcq,R}
spv{s>410:A,s>363:ng,A}
rg{s>2362:bk,a<457:sv,zvl}
nmd{m>3436:mrb,m<2901:gm,s>3353:jgv,jz}
gm{a>1985:A,a<960:zl,lj}
fkj{x<1049:R,x<1135:R,a<1941:R,A}
kfd{s<1278:A,m>3243:A,R}
jdt{a<1600:R,a>2541:A,s<3132:A,A}
jr{a>2943:A,m>2953:R,a>2378:R,jgg}
fbx{m<1502:A,m<2978:R,R}
qnl{m>544:A,R}
fq{m<2835:bdd,x>2775:lfl,x>2714:gcr,btm}
pmz{m>2665:tkr,a>1489:mqn,xbb}
pj{a<2353:qjv,x>3310:dd,a>3313:skv,xqr}
ts{a>1058:R,R}
qtl{a<2007:A,A}
hgg{m<1358:fh,m>2737:sxc,x>279:xrb,xb}
lv{a<3305:R,a<3582:vd,blk}
jgv{m>3216:zfg,a<1636:zjb,s>3752:A,A}
zm{s>2390:rt,x<2179:jhl,cx}
ns{a<1816:A,A}
rsj{a<2875:A,R}
mss{x>2473:stj,gjp}
vfr{m>1649:R,a<535:A,a>692:zxs,mrc}
tp{a<357:rtz,s>500:cz,s>311:spv,tf}
hr{x>3243:R,R}
bvd{a<1882:R,m<3089:A,A}
qdd{x>706:R,x<699:A,s>2373:R,R}
zpv{a>667:A,m<2599:R,s<3261:A,R}
zql{s>359:xrj,x<2580:A,m>2798:cc,A}
vbs{m>3474:A,x>1947:R,R}
kjs{a>350:bv,df}
vhr{s<484:R,a>2875:R,A}
qts{s>2882:A,R}
kv{s<483:qfl,dq}
jpf{a<1278:vql,a<1526:xrt,R}
smx{s>2709:A,s>2326:R,A}
khm{s>2412:R,x>1252:R,s>1923:ldj,tqh}
jhl{s<1988:tbc,m>1736:vh,s>2176:cs,psk}
pxb{x<2637:R,A}
tf{s<146:gkh,x<2176:A,a<609:xpg,R}
jc{x>59:R,a<3004:R,R}
rxn{a>2738:qpj,R}
zpm{m<1395:R,R}
hb{s<192:A,R}
tz{s>2124:R,x<1437:A,s>2018:A,R}

{x=903,m=143,a=1348,s=25}
{x=2233,m=5,a=1257,s=509}
{x=375,m=272,a=3451,s=2803}
{x=151,m=1682,a=381,s=397}
{x=211,m=2629,a=88,s=113}
{x=1779,m=449,a=705,s=23}
{x=681,m=138,a=793,s=175}
{x=894,m=2089,a=234,s=502}
{x=2146,m=16,a=1129,s=1890}
{x=378,m=1036,a=2004,s=1529}
{x=111,m=38,a=537,s=1478}
{x=1488,m=338,a=1,s=1211}
{x=150,m=1630,a=1377,s=2200}
{x=8,m=369,a=134,s=2}
{x=1933,m=611,a=686,s=1951}
{x=828,m=662,a=233,s=1951}
{x=583,m=1722,a=1872,s=236}
{x=2299,m=95,a=331,s=651}
{x=2413,m=164,a=1186,s=490}
{x=302,m=3732,a=637,s=2302}
{x=3027,m=3305,a=103,s=953}
{x=527,m=9,a=282,s=219}
{x=1459,m=1654,a=330,s=333}
{x=2381,m=1229,a=2571,s=441}
{x=153,m=379,a=1452,s=1550}
{x=2696,m=1050,a=1829,s=2036}
{x=521,m=392,a=3003,s=567}
{x=1911,m=1937,a=2082,s=3478}
{x=207,m=980,a=3125,s=1305}
{x=2450,m=2665,a=1371,s=1116}
{x=32,m=973,a=2176,s=312}
{x=868,m=171,a=239,s=278}
{x=360,m=571,a=201,s=1129}
{x=1112,m=2313,a=138,s=263}
{x=335,m=426,a=56,s=476}
{x=1505,m=1239,a=864,s=192}
{x=929,m=62,a=971,s=1766}
{x=38,m=670,a=3889,s=208}
{x=448,m=1744,a=447,s=3365}
{x=274,m=1419,a=744,s=1039}
{x=356,m=970,a=1422,s=3159}
{x=1649,m=195,a=108,s=263}
{x=6,m=1033,a=3484,s=2578}
{x=243,m=676,a=1787,s=3214}
{x=3899,m=501,a=913,s=718}
{x=209,m=1520,a=323,s=1135}
{x=1361,m=166,a=760,s=132}
{x=1411,m=490,a=831,s=1643}
{x=1914,m=3518,a=289,s=573}
{x=320,m=505,a=16,s=9}
{x=2935,m=127,a=2681,s=971}
{x=368,m=294,a=861,s=536}
{x=59,m=901,a=411,s=182}
{x=3388,m=2984,a=3796,s=467}
{x=354,m=551,a=416,s=321}
{x=1821,m=1411,a=1965,s=1074}
{x=33,m=1370,a=492,s=639}
{x=94,m=1967,a=1626,s=346}
{x=692,m=88,a=1445,s=1210}
{x=404,m=2777,a=505,s=131}
{x=1672,m=3555,a=178,s=383}
{x=538,m=1389,a=1078,s=2653}
{x=2214,m=587,a=666,s=717}
{x=1298,m=1143,a=2221,s=1300}
{x=1289,m=2516,a=129,s=1271}
{x=165,m=515,a=275,s=264}
{x=790,m=713,a=854,s=121}
{x=2091,m=54,a=607,s=261}
{x=107,m=207,a=94,s=1280}
{x=725,m=1603,a=828,s=335}
{x=941,m=66,a=2301,s=1138}
{x=213,m=3088,a=614,s=56}
{x=51,m=471,a=1020,s=101}
{x=26,m=862,a=1846,s=406}
{x=33,m=2101,a=898,s=132}
{x=1082,m=319,a=2292,s=661}
{x=1395,m=1303,a=248,s=1196}
{x=435,m=1082,a=456,s=663}
{x=796,m=494,a=1167,s=1696}
{x=2400,m=544,a=1568,s=1100}
{x=55,m=1266,a=272,s=1794}
{x=1097,m=718,a=642,s=1656}
{x=1917,m=392,a=833,s=1455}
{x=546,m=1613,a=148,s=1539}
{x=1282,m=165,a=2653,s=1335}
{x=65,m=952,a=935,s=609}
{x=378,m=1654,a=2196,s=2554}
{x=2349,m=228,a=1221,s=925}
{x=489,m=506,a=1640,s=521}
{x=9,m=2416,a=1855,s=758}
{x=1170,m=1580,a=654,s=2060}
{x=139,m=1238,a=1074,s=274}
{x=3027,m=22,a=45,s=658}
{x=1562,m=1901,a=3043,s=21}
{x=1219,m=191,a=6,s=190}
{x=3618,m=288,a=443,s=198}
{x=1497,m=1866,a=1958,s=3505}
{x=1586,m=213,a=2333,s=969}
{x=1398,m=24,a=845,s=2130}
{x=936,m=2130,a=1794,s=119}
{x=1829,m=2394,a=104,s=918}
{x=47,m=1053,a=154,s=1847}
{x=10,m=982,a=2684,s=225}
{x=131,m=2301,a=588,s=59}
{x=2150,m=2878,a=47,s=3076}
{x=214,m=285,a=241,s=1600}
{x=1270,m=145,a=2252,s=693}
{x=75,m=2032,a=251,s=427}
{x=120,m=1086,a=274,s=1010}
{x=1189,m=1182,a=1184,s=1043}
{x=553,m=487,a=329,s=301}
{x=290,m=2284,a=127,s=1014}
{x=873,m=1330,a=377,s=2438}
{x=1369,m=172,a=413,s=2079}
{x=37,m=1179,a=100,s=435}
{x=1329,m=1523,a=305,s=114}
{x=69,m=758,a=225,s=79}
{x=35,m=27,a=1502,s=198}
{x=619,m=425,a=306,s=649}
{x=414,m=533,a=1428,s=3525}
{x=270,m=2415,a=32,s=1070}
{x=242,m=133,a=1264,s=54}
{x=39,m=571,a=108,s=908}
{x=1733,m=778,a=1968,s=2913}
{x=1498,m=904,a=734,s=331}
{x=2331,m=340,a=450,s=1833}
{x=1004,m=291,a=821,s=1012}
{x=1710,m=193,a=2013,s=50}
{x=31,m=87,a=1017,s=1464}
{x=621,m=391,a=1169,s=895}
{x=732,m=336,a=798,s=769}
{x=1052,m=62,a=1457,s=35}
{x=382,m=621,a=102,s=180}
{x=437,m=25,a=957,s=285}
{x=17,m=2915,a=148,s=122}
{x=1154,m=30,a=142,s=250}
{x=199,m=1270,a=3209,s=378}
{x=1929,m=832,a=649,s=1759}
{x=363,m=79,a=1015,s=766}
{x=359,m=120,a=134,s=997}
{x=171,m=1208,a=92,s=31}
{x=16,m=1603,a=373,s=2031}
{x=1051,m=1368,a=1700,s=1459}
{x=3353,m=2659,a=142,s=36}
{x=961,m=93,a=88,s=771}
{x=2166,m=302,a=3171,s=38}
{x=2812,m=876,a=681,s=53}
{x=333,m=242,a=1133,s=747}
{x=181,m=149,a=299,s=1232}
{x=500,m=103,a=2735,s=627}
{x=1065,m=258,a=724,s=9}
{x=197,m=1438,a=160,s=2493}
{x=926,m=1037,a=1498,s=1115}
{x=299,m=11,a=371,s=311}
{x=1025,m=7,a=865,s=773}
{x=2963,m=845,a=227,s=404}
{x=1741,m=66,a=1891,s=1028}
{x=301,m=18,a=159,s=750}
{x=459,m=1694,a=766,s=94}
{x=303,m=50,a=752,s=1762}
{x=1369,m=2296,a=1374,s=224}
{x=1036,m=514,a=609,s=51}
{x=199,m=1070,a=662,s=54}
{x=243,m=178,a=1703,s=1331}
{x=1081,m=1170,a=516,s=1650}
{x=21,m=27,a=775,s=1857}
{x=2821,m=3322,a=736,s=527}
{x=1858,m=1192,a=63,s=1519}
{x=1584,m=271,a=778,s=490}
{x=1738,m=2680,a=3451,s=594}
{x=40,m=976,a=1456,s=1596}
{x=311,m=2,a=448,s=78}
{x=766,m=3603,a=831,s=113}
{x=422,m=2060,a=2754,s=258}
{x=582,m=1526,a=859,s=1007}
{x=256,m=164,a=15,s=1}
{x=474,m=2830,a=361,s=2}
{x=308,m=1283,a=296,s=480}
{x=973,m=1037,a=1809,s=266}
{x=699,m=205,a=54,s=3111}
{x=2524,m=2458,a=69,s=2648}
{x=363,m=850,a=1402,s=178}
{x=440,m=2681,a=300,s=390}
{x=35,m=1148,a=713,s=904}
{x=1387,m=288,a=222,s=52}
{x=1627,m=2352,a=1502,s=888}
{x=2525,m=2810,a=1513,s=971}
{x=370,m=2436,a=1541,s=1049}
{x=3313,m=627,a=3203,s=3021}
{x=3874,m=1093,a=471,s=3030}
{x=841,m=981,a=1645,s=1219}
{x=125,m=14,a=1917,s=54}
{x=110,m=232,a=414,s=1781}
{x=1094,m=445,a=1585,s=3173}
{x=2043,m=294,a=1498,s=224}
{x=302,m=89,a=3154,s=1866}
{x=2814,m=1941,a=1563,s=1350}
{x=2116,m=528,a=1040,s=548}
{x=1221,m=579,a=871,s=1031}
{x=91,m=2424,a=57,s=1438}";
}


