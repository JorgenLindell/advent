﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using common;

namespace _25
{
    internal static class LocalExtensions
    {
        public static IEnumerable<IEnumerable<T>> Permute<T>(this IEnumerable<T> sequence)
        {
            if (sequence == null)
            {
                yield break;
            }

            var list = sequence.ToList();

            if (!list.Any())
            {
                yield return Enumerable.Empty<T>();
            }
            else
            {
                var startingElementIndex = 0;

                foreach (var startingElement in list)
                {
                    var index = startingElementIndex;
                    var remainingItems = list.Where((e, i) => i != index);

                    foreach (var permutationOfRemainder in remainingItems.Permute())
                    {
                        yield return permutationOfRemainder.Prepend(startingElement);
                    }

                    startingElementIndex++;
                }
            }
        }
    }
    internal class Program
    {

        const StringSplitOptions Tidy = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

        static readonly string xinput = @"
jqt: rhn xhk nvd
rsh: frs pzl lsr
xhk: hfx
cmg: qnr nvd lhk bvb
rhn: xhk bvb hfx
bvb: xhk hfx
pzl: lsr hfx nvd
qnr: nvd
ntq: jqt hfx bvb xhk
nvd: lhk
lsr: lhk
rzs: qnr cmg lsr rsh
frs: qnr lhk lsr";


        static async Task Main(string[] args)
        {
            var strings = input.Split("\r\n", Tidy);
            var connections = strings.SelectMany(s =>
            {
                var all = s.Split(": ".ToCharArray(), Tidy);
                return Conn.Create(all);
            });
            var components = connections.GroupBy(x => x.From).ToDictionary(x => x.Key, x => x.Select(x => x.To).ToHashSet());

            var centrality = CalculateBetweennessCentrality(components).OrderByDescending(x => x.Value).ToList();

            var countOfDiscon = Disconnect3();

            var group1 = CountConnected(centrality[0].Key, components);
            var group2 = components.Count - group1;

            var result = group1 * (long)group2;
            Debug.WriteLine($"Result:  {result} ");

            return;


            string Disconnect3()
            {
                var dict = new Dictionary<string, HashSet<string>>();
                for (int i = 0; i < 6; i++)
                {
                    string name1 = centrality[i].Key;
                    Debug.Write($"{name1}: ");
                    dict[name1] = new HashSet<string>();
                    for (int j = 0; j < 6; j++)
                    {
                        var name2 = centrality[j].Key;
                        if (components[name1].Contains(name2))
                        {
                            Debug.Write($"{name2} ");
                            dict[name1].Add(name2);
                        }
                    }
                    Debug.WriteLine("");
                }
                var remove = dict.OrderBy(x => x.Value.Count).Select(x => x.Key).ToList();
                var permute = remove.Permute();


                var enumerable = permute
                    .Where(x =>
                    {
                        var pairs = x.ToBatches(2).Where(p => components[p[0]].Contains(p[1]));
                        return pairs.Count() == 3;
                    })
                    ;
                var combinations = enumerable
                    .Select(x => x.ToList())
                    .Where(x =>
                    {
                        var dict2 = dict.ToDictionary(x => x.Key, x => x.Value.ToHashSet());
                        var pairs = x.ToBatches(2).Where(p => dict2[p[0]].Contains(p[1]));
                        foreach (var pair in pairs)
                        {
                            var name1 = pair[0];
                            var name2 = pair[1];
                            if (!dict2.ContainsKey(name1) || !dict2[name1].Contains(name2))
                                return false;

                            dict2.ForEach((x, i) =>
                            {
                                x.Value.Remove(name2);
                                x.Value.Remove(name1);
                            });
                            dict2.ToList().ForEach((x, i) =>
                            {
                                if (x.Value.Count == 0) dict2.Remove(x.Key);
                            });
                        }
                        return true;
                    })
                    .ToList();

                foreach (var comb in combinations)
                {
                    var disconnected = 0;
                    var dict2 = dict.ToDictionary(x => x.Key, x => x.Value.ToHashSet());
                    foreach (var pair in comb.ToBatches(2))
                    {

                        var name1 = pair.First();
                        var name2 = pair.Skip(1).First();
                        components[name1].Remove(name2);
                        components[name2].Remove(name1);
                        disconnected++;
                        Debug.WriteLine($"{disconnected} Disconnect {name1}:{name2}");
                        dict.ForEach((x, i) =>
                        {
                            x.Value.Remove(name2);
                            x.Value.Remove(name1);
                        });
                        dict.ToList().ForEach((x, i) =>
                        {
                            if (x.Value.Count == 0) dict.Remove(x.Key);
                        });
                    }
                    break;
                }


                return centrality[0].Key;


            }
        }

        private static int CountConnected(string startNode, Dictionary<string, HashSet<string>> components)
        {
            var counted = new HashSet<string>();
            Queue<string> Q = new();
            Q.Enqueue(startNode);
            while (Q.Count > 0)
            {
                string v = Q.Dequeue();
                counted.Add(v);
                foreach (string w in components[v])
                {
                    if (!counted.Contains(w))
                        Q.Enqueue(w);
                }
            }

            return counted.Count;
        }

        public static Dictionary<string, double> CalculateBetweennessCentrality(Dictionary<string, HashSet<string>> components)
        {
            // Brandes algorithm
            Dictionary<string, double> betweenness = new();
            foreach (string node in components.Keys)
            {
                betweenness[node] = 0.0;
            }

            foreach (string s in components.Keys)
            {
                Stack<string> S = new();
                Dictionary<string, List<string>> P = new();
                foreach (string w in components.Keys)
                {
                    P[w] = new();
                }
                Dictionary<string, int> sigma = new();
                foreach (string t in components.Keys)
                {
                    sigma[t] = 0;
                }
                sigma[s] = 1;
                Dictionary<string, int> d = new();
                foreach (string t in components.Keys)
                {
                    d[t] = -1;
                }
                d[s] = 0;

                Queue<string> Q = new();
                Q.Enqueue(s);
                while (Q.Count > 0)
                {
                    string v = Q.Dequeue();
                    S.Push(v);
                    foreach (string w in components[v])
                    {
                        if (d[w] < 0)
                        {
                            Q.Enqueue(w);
                            d[w] = d[v] + 1;
                        }
                        if (d[w] == d[v] + 1)
                        {
                            sigma[w] += sigma[v];
                            P[w].Add(v);
                        }
                    }
                }
                Dictionary<string, double> delta = new();
                foreach (string t in components.Keys)
                {
                    delta[t] = 0;
                }
                while (S.Count > 0)
                {
                    string w = S.Pop();
                    foreach (string v in P[w])
                    {
                        delta[v] += (double)sigma[v] / sigma[w] * (1 + delta[w]);
                    }
                    if (w != s)
                    {
                        betweenness[w] += delta[w];
                    }
                }
            }
            return betweenness;
        }
        internal class Conn(string from, string to)
        {
            public string From { get; } = from;
            public string To { get; } = to;

            public static Conn[] Create(string[] all)
            {
                var src = all[0];
                return all.Skip(1).SelectMany(d => new List<Conn> { new(src, d), new(d, src) }).ToArray();
            }
        }
        private static readonly string input = @"
btz: vxx
cjk: jcc ftb zpq
jkg: hnl kzf jtb
pjn: sjh tmb zvg
qpn: lgx
zmm: bmj
vdl: csd sjh ghg nsv
thh: qmj bxt zgr krh
qzd: kvn
tcg: ljt pbc vnc
dqt: bdx
mhh: knx
lzz: mnj jqm htz
gxr: rdg
gdp: blx
bqg: zgx mtk ccm
shg: mfh dxt vkn djm
sft: gbd
drg: xfr zsv rrh
lcj: dxs jcc
mmc: csd qbn
kkq: kln mbd qnq
hnh: vpf htf sft ctm lth hcq ljt
gft: fhm zbv gkz bgs rmx
qvx: mpd qqv qch
sph: kkx tqh ddz
nlx: jll
mzf: qlk fjd
zfj: tmm
qfk: rpn
gqc: hvj mpj kkq gkg jts kcr
gjz: mnm
fkm: mnj ccx kxl cnz
nhj: kvn dmc rhb
tbd: vnz tmr
sbh: gmg
scm: zzc nrz
gmz: gtj rjq bpg zcm
nlt: vvp
zfc: cmh bsg kcz zcl
pdc: qfk
bsl: dng jbb slk zlg
kbr: vtx vdh hsj
bnp: zbh khg vgd
cnk: hfm zcl tzj crp jpx
qrl: vqf bbx ptq xcp
dtd: rvh mfv rms cnp
nsb: kkr
jrk: znq xzb gpv
qtj: vcb
jms: vgg zcm vtx
vxv: htf
sdx: djg nnh bsp mfv
dgz: xnt vtt
kcq: rdd qzv jhs
vfb: mdj svq ptl vvn
dbz: ttc zlq gsk
qrj: qzj
lkl: lqt xkz
qqd: dcn tmc
fkt: kgs zfd xnt
lhv: ktr vgb
lmp: vkj thx slg
lmg: kph jqm hcx
krt: zxb xzk fnk zxx sft
qzv: pqt lfv
htx: ngq lmj
gcn: xlb bzj xvl jmb
dhn: rtn vsf bkr hzc
knp: sqx jgc
fzb: gzf tmc bln
scs: rmn ppz tpb rpn
vzx: flf mbx tbd kkr gjv
xcc: sfh jjh zfc
nrc: ksr mcd lrb xtf pxz qzj
jkj: vmf bpm dmp jsj
ctp: bxb
jsl: gqh
xfx: kjx
fhs: gtc ppj rvx tkd slt
vgd: tmr xrv fsz
bxt: lgx
txb: xkn
hnq: kfb jqm
ths: hhh fzf pdt dst
phk: vgf jzh zxg lzp
cxh: phh mxr flg
tzh: zzv hbj bdx znr
vzf: qgl vlh dpr
xcm: ldt gth ljh
jvr: mlk rff jcc
bjt: hlz bgk tml mgp
scx: bvg nzv rcs flr zbq bnm
bjz: nmx cgs
bsb: hrn
gcg: prv lrd
qxm: bpz
hhv: ktr
mnc: lrn rff
hmt: bpm nqn rjv mgp
thx: bpg
nbd: lgz bqz psh djp mgh
cll: chf kbq rzn rsf
zzx: ccn qxl mqc
znq: hkq xdk
bdj: cjc
sdt: qtf sph lpv
hdv: xfl pfm bmn
kck: vjj xjf
lgg: jgk hhh znr djd
tlt: xgq tjd vvv bsp kjg
pgb: lmj
gvb: kfb
qpx: bxm knx xrv bdj
rsp: xsg
gnb: tvj rmn nfv xjt nvc
glt: skp hfh qvb
klv: ndh cvp mlk dqv
lmj: xph
qmj: jtb
jgt: tpr
vlq: qtj shb xsd
rgk: qvz fgj pfm
mbh: sbl
pqt: fcv zmm
hfs: rcg ljd rrv mbm
xsg: mkx
ckv: bsn sfh rms tzt
zbh: tqm
crr: kcz ldf
rhs: rvx jgc pxd bjz
hzm: rft fnd lqt
mqc: xgk ctc
zzq: rmb bfh jrk ndr rpn
tpb: bzk glv gxr
vrr: rrb spv
szl: kbq ggd kcn nrv dtt
xlr: bzk gpq
gzl: pzk
mbr: htv jkg
zpm: rlv
mbg: lph zvg ttd
mvs: zlg ghg kcn
ckc: drf hcq
jxd: ssf rmx nkt
ltd: mbh nlz
cfq: gjz bsh hlr
qjq: cmh lsq
jxf: chf tnf qqv
cgv: vpj zhd
hzk: rhr brs rsj lrn
btx: qxr
lcq: mdd xrg
kjx: cbj qmq lzd
qfq: tfg mjk
njv: trl xxl sst
vmg: ffb vzb hhd
zkn: zbq dkm psh vdh
gcp: dhn qmz rsj ptf
fnk: hqv qtz qqv
vvk: nrv tqh gjz
jzk: zsv jsj mnp
vjj: lzb tpl
stn: jxs dst lsv
clk: cxr
fpl: zgx lph zpq pvv
srs: fsc knb cpb zsg
xvs: ctp thj krh
qvz: zsv
lsl: lnm dxt gjv ztv
bcm: mlq
hhd: qsr
jnx: xvp
cvg: nhz qgp kst lqt
qbn: rft tlm jrc
dgh: xvs
rmn: vxv
gkg: pdc kgs vtm vjz
tfj: jvn tjc
fbg: mbm lfq sdz tvd vtm
fmv: tqg fkg
mrr: bmn gzf hpn pql cpc dlf
lzf: jgk slg lzt sgr
psp: rfm ccq
plz: cmd lnm ccb
cms: fxj gbp
vtt: hsp
zjf: vtx
kzs: jmv lqt slx ccb
mgh: gkg jgk
rlf: mjk flf mpm
kvr: njs klr fnj xjg
ccf: cxh jxs sbc
qgl: bkj
xpr: txs
mcd: zbk mrb tpl
qxh: jxr
mms: rqn xzf kbs vqg
qzj: ssm
dvr: rkm qmj zxg
ljq: bvg qvf
ctm: hgn fqt lzd
mbc: rdd ffs bxs
lsf: bpp mhj
mnj: qkc vgb
qbh: cpb
mtz: tjc ssb bxs
cks: hzc
dfv: ptl hnr zlq qkc
jts: qdv mkj
bbj: bnv
fgj: xkn psp dqv
bcg: xfr bfd mnr fll
xbn: gzg npf
kkh: hvm ndh stl hdv
tvt: mnr mvs hnr pvs
crq: bpz
dcm: sgl qhg pmv gqs
kvn: hzl
ggb: cpc rrh
ksg: qlv xrv pjp gzg
zmk: sgl
hkb: cfd pgz hpn xvp
lmx: clb jbb dgg cxm
qxr: zvm hdd
mfb: lcq hbj fjn jpx
lgl: tfx fmz jsv mrb
vqg: tjb xpx
fzf: kgt
vpd: bxm sgl
jkn: vjj tvd xjj sxb
zkk: stn hvj qtf
bzc: btj bsb mrv fnp
qbd: xsf drq
glv: smp
hrk: kln
vnc: bfl gzl
zjs: hvb rhb mln zlv gdp
bvh: ccb glp gzb
gvs: tqz nzp tld
crd: mrx
lnk: pbp mlq vln kgz vcb lrn
tzt: xzb
lss: dqt vxk
llz: lhr sjl jsh ngl
hkf: bsg xdk
qhg: jrc
tvd: pzk sfh
lvp: gfm pcp clk
ccn: mnj dph rvx
gth: jvn
qqg: ctd zhd
bcr: pbp bbj drq fcv
tmm: xpr
pdt: tdh zxb qbh gqj
tzq: gdx knx ntn slt
qvs: xvs qqv lgx kcz
fmh: gjf cfm zvt nvc
kzk: qfk qbh sdz
fcs: qpt vzb bzb rdl jjh prp
thj: hcq
gdq: hsp hvg
lpv: mnm jpc
dxt: tfg lff pds
zkh: sql pjq txr
pvs: mkx
nrv: ftj
prn: gjf kfh tst gks
mgc: nlb lss ppz
clq: ffb pdc tpr rbh lqh
qtq: pct fvt qvf qzp
mlq: ptl
hvm: xrj mmz ldz
lxl: mkf szp xzg mnc
bgk: jsl tqm ztl hsb ggb sjt
dxs: bvh hzl ksv
xfn: bcf qrj
bzj: mln xrj
ptf: jfq xkz nmp
hcf: vzb brc hhx ljt vjz
rnq: xsp fnj
mxc: slg txf hsj lfj dbq
knb: zzv
vkq: jck msq fvt lhr jml
hzc: lnf
qch: tpr
tff: rqj kjg
tnx: flg hhx sgh
vbx: ppz pnb qmj bcz
tqc: ssf lkl
hzb: bsx pnc tnx mcg
xln: sst gzf mkf ccm
hxf: jhs cvc prv lfp tqg pxt
lml: sjh vnd jsl
fsx: bjz
rxs: rtv qjg mkx lsf
kql: xvz gbd lgx
crp: dkm nlx gfp vtt rmj
rhd: hqz thm mkj
pjb: qpt cbj
znr: cbk mqz
bcz: dtt hxx
vdx: qzm
mtc: ljq hkq ptq
xdp: nkr hhl mjk pcp ntc
lcx: txp bcf hnf jxs
xsp: djm mhd tkd jmb mrx ktr
tvj: qfb trd
gpd: rzx fxn dcn tzl
lvq: qxp
ssb: pbh
ptq: mqk fxn rjp
knq: qkc
qkz: qpn glt ggl czz
fdm: vsb cgd ftq txb
fqb: lth hhd jvb qrj
xxl: pbh
dzh: nbv kcr qvf vdr
qkh: nxf mnp fzb dpc
brh: lkb glg qsr tdr
kkr: nmp
gbd: nzv
gpt: njs kpf zsd
qpt: bdx
qtg: qjf kvn xkp qjg kfx
cbv: skn xzb zfd pjb
lnm: ftq nmx
gzg: prd
lkk: vgd kcq
xnq: lff mqc
qjg: nsc
pjz: nlx bfl xgq
rks: dbz gzb qbd
kbz: dkb tbd rrr bht
vjx: gvb xhj prd
ghh: bbx jdz
frv: sxg psh djp
cjg: gfq xfm mlk lll mrt
nzn: mkx lrd
nbg: czp mfh hdh sqx
brs: vkn
gql: hpn gcg fmv vjx
qvf: xvz jgn
sgj: qrj fdr
nbv: jgn zjv
lgz: pdc csc
vkn: ssf
shl: rqj lfm hpl qrg lsq
trd: bfl
vbr: nlx qbh rrv mvb
tfh: pzb mrb
cdk: ngq lzp kfc tfh
pjr: kvg ttc
vnz: nkt lfv vcb
mhm: qbf kvg ssb sjn
sbq: ghg vxq mmq sxq
hth: pjb xpn cgv
zxg: jxr
nnz: ntc hhv
ggd: cjz
gks: bnm
xhm: xsg pbp zbh dcj
dgl: ftj
rpb: vvp
fmb: glr
nzj: bsp bsx kcr
xvh: crd fbd dgk
rkm: ffb jvb
ghg: dng
sgq: lkx vxx
vdr: qtf
xng: mdq txs lzn jzs
lxt: rcg tzh rst
fbf: sdt zjf lzb vjh
gjv: tgb
rrv: fqj qxp vlj nrz bnm
fhf: zbk bcp bbf
tnf: tzt qhx
jxk: mqz kkx gqj
qlg: csk gpv hfm sjk
zzl: xtf nvt dqj svz
bfh: tst vjh vbl
nhl: kjg bpg dgh mvf
ldz: hcg fsx mgp
tss: tqm qfq pjn fcv
flr: htx hcs
gzz: lnf
ttl: mms lnk pvs knx zfj gxq
kvl: rvh ngx czt dtt
qvd: xpx tfj frn
rtq: kpf mhj qqd
xpn: kck lkb
gfc: dgh jzr
nms: gcf fmv lcp
czt: qrg jtb
xzg: mdq
htl: qxj kgj
khj: dph bkt xsf fsz xpz jsl pvv
fmz: xfx gcj jgn
lrq: lfm vcl qqg trd cgr
crb: phh nzq qvb
tml: fkg bzd dmp
slx: gzz
lrs: gbr nnz qxm zsd cpc rzz
bmq: ljq xvz qdv sbc
pqd: vxx htd
qpc: rrr lrd tqg gbp
qls: gtc
kdg: qrn lfr kjg
nfv: glv mrb hrk
hhl: csd ldn
blg: vlk qxm xfm slx
sxp: hsb mnp cks sjn fdt
ngv: bcm ntn hsb pqp
xmc: tzj htv hqz
kps: xkp tqm lmg gqh ldz
xtm: ddz sfh
tdb: nfq jlt bsp zsg
lkd: nbj njs pgz
zxx: xzb zxg
mld: xkz tzl mkx fdn
dtg: jnx vfn hhl
dqn: zds hrq zrq gth pmv nkb
rqb: hhv drg knx nqn
vvj: xgv vkg ztv frn
mqr: ljh cxr
bjr: rcv fsd xjf znh
bpm: kfx
gpr: lhv ggb fsz
vkg: gjv plz cxr lzn
qlc: xjc drf gpq mvb
pxt: cjc lfp ffs
ntx: hbt thx
lsp: jvn bkr qhg
vjz: cbj sxg
qhd: qcd gfq jxd
qgn: fsc ffb msq
bvr: klb bqz hvh zxg
klt: ccl pbh xbn trl
xlb: tmr dft
jpx: tmf chf
jvn: bpz
ntc: pds glp
tjb: hhv
hvj: tzt
bgs: hcg jvf
gmg: nzv
vgr: jjh zts rcs
klk: flt gdp gbv vkn
nrt: vdx cgv
bzb: nbn qxh
cbj: ktt
ppr: tqh
prp: dzv
qrn: btz shz
rpf: jqq zzl kjj bxt
xjt: rms lnt
hld: bkt tmb zfj cmj jlk
xgq: fqt vxk
xgt: mgp rzx dpc zlv
vxq: hnq kgz
cjz: tld
tlq: xkn xkv
sbb: bcx shz nbn cqv kmm znh
glr: gcj
pxd: npf tzl dlf bzr
djs: xvh qjf jvf cgd
spv: knp qgp djm
dmc: gsk
gqk: llc rxf knh srk
fhp: fcv djm
cmb: mqk qrj
gqh: xvp
qgp: nkt lph
dpj: lck vjj
fkr: hgn qxh glr kck
bmz: lst pbp ttd trl
qhx: qzm
gmj: tpb tdr djp srs
cnp: klb mlz dgz
cpf: tpq vjr
zlg: bzr qjf rfc
qbs: rqb ttl zmz lcj
svv: dpj mtm qrn klc
xfr: vjk
lqt: ntc xjg
xhq: njv xjg vmx frn dph
lhg: xth nlp stl mbx
kfh: zjv mnm
zxt: gbr pxt kvn jpp
qcb: btx mrx xfr tjc
qht: mlz lrq qtn
fhm: gvb dsf qhd
thm: ctp gvq kjg
hqv: gbd jpc
csd: bfd
bkj: jld
kgp: nnh dzv dlg vkp
ftb: dcj gzf
jmv: vrr rjv kph
bst: vbl cmb qhx rdl lcq lcf
nsv: fzp lsf cmj ttd knq
cxm: kfx pkq
ghl: ctd pzb tst lfq zgr
mcb: chf dzv nzp lnt
hbx: ctc nbj dkl
dpn: dnf knq bzd bzr
gdx: vzn slk kfb
qrt: pzm knq cjk zlg hmq
hqs: hvb hzl dft tlq
rrb: xkn
dkl: jvf pqq jtp
crj: rvh dtt ljd tqz gdg
zds: tlq ztv
tkh: ppr bsx
lsc: ttd fdt tmm xnq kvr
szh: gxq prv jzs
xkz: bbj
ljr: vjr ngx hrk pjb
kqq: zcl jck
nvc: qrg dgl
hrl: nln qzd vkf
fvt: dgl
fcl: srd zkh fpv zfq
clb: mcj bqg xlb ttg
cnz: jqm zbh
sgf: nkt sqx kfb
ldq: bmd lfq
vdv: zfj rtn tfg mmc
msq: nrv
jlk: bnr
flt: nkt qmz
ppc: tfj jzs jcc jcb lkd
gxl: xzg
xvg: xjs pxz xzp jjh sbg zcd
hsj: pvm
zxj: vdr hbj zgh vjh
gnm: qxm
pcc: ggd crr dbq znq
jmm: rlv
khs: knp qxr pbh
bsx: pbj
lzt: gfp
kbq: hkq
bmk: zxs
lpj: vch zbs bhn vjt
slz: gnm mcx gbv gzb
fhh: nsc fhp bcm
czp: mfh plz
mfv: czt fmb
mnr: ztl hpn bpp
qlv: jgc
bml: dmc bln szh gxl
dtf: czp jgc jks
ggh: sxb xzb zzv
vqx: sql jxf ddz rcv
klr: xjg mtk rhr
zbv: rrb
kgt: llk
ktg: lnf dkl nbs rlf
xgk: fdn
xrv: tmb
vdh: sgh
mtk: trl
gpv: scm
prc: dft mhd dpc tmc
rqn: qvz mcx
bdn: bcz trr fss ngl
csk: rcg
jsv: zgr
gcq: ljh
sqp: czp bnv qrq pjp
nxr: hpx ldz tnv cms
csp: qxj bqd gjj lsq
qlk: nln rrb
zjv: nzv
hmq: pkd jvf xfl
dqj: kgt sjl xlr
lff: prv
cgr: tjd
fpv: kbr rst qlg pgb
kph: crd qbf zsd dqc
xhj: hnr vmf lcj rsj
kgv: qch pgb skn
rdl: zzz mlr
tbm: vvv cbk nlx
jbb: mln qhg
cjr: lrj qtz xgq cfq
nkr: kbv
cfm: mqz
sjl: smp tcg vnc bsh sbv xjc
xkq: mjk pjr vvq kxl
mjd: bgs lst kvn lml
qxl: tkd rff
pnc: hkt jxs
ldn: tjb fdt vcb
lfk: vkl ngx trr gjj rsf
gfp: ctp tmf
hpv: xjf ldq nlt shl
qcz: pfm gfm rqn
nht: hkj nlb
hrn: drf
trr: rzn
srk: zpq zmk fdn
pcd: gcq cxg
zhp: nrv tld lmj
kbv: xsf crq
lkb: xjj rpb
hsb: zct
mmt: lcf lnt nfr ngq
lrb: gbd llk
sqt: bnv pgp lcp lll
qxn: mbd mrn vgk sxg
vdn: gdg txr nrt prp
mvb: xfx mcb
hhh: lsq
zzz: hhx kfh hfh slg
rtv: mhh brs hhv kvr nzn
npz: txb bxm hdd xlb
dst: lks lzb
vmx: nln jcb
vfm: htf gvq
hlr: czt cfm klb
hkj: jck fsc hgn
nlh: tqc nqn fhl fsz
lll: zjb ffm nkr
gkz: cbq tmc hdd zbv
hzr: gxl clk vln qrq
kqc: tmc pcd gsl stl
gvq: xrg bkj nvc
tqh: lrb
hcx: zck gfq krx
mpk: qtf lbh bql glt
tks: ljq scm
ngs: lxt glt pqd zkf
rtz: drq vjs zds rfm tjq
gxq: rrh
njt: rlh lkk xkp gzz qmz
qbf: bpp
mlb: gfc kfh gks pct
ffp: ldt
kbl: bkj nnc srd
krv: fcv nsb tqm
srq: nlb rbg tck qqg pxz
lth: jgk
lld: phr kcn jfq lhv
pdq: trd txp
hqz: qmj hnf
szp: prd rsp
jzh: lzp xrg lth
pql: nsb mmq
bjs: shz djp
zgx: ksv gbr
mrs: thd kcq tfj xts pqq
mkq: mpx lss lfk flg
xpz: bpm gvb
gnf: vgb ccx mvs xsd
vch: dph xpr xsf xcm
qrg: ksr
hkt: fsc hvj
vsf: vpd jks rfm gqh
kvg: zpq
brc: pmc htl fsc
cjp: fbp pbj htf
kgj: rcg thj
txr: lsv nlz jjr
ppj: jrc
pds: pgp pkq xfm
lnp: dgk qlv slz gsl
msv: ctp nnh
cpc: jks
sbg: lks zpm
vjl: mnc tqc rxf ktf
qtn: hvg ksr
czd: nzv htx
qfx: vlh tpl knb fbt
ztl: rsj
mfh: gbp
rxf: rzz
vmf: zmm dng
ktf: btx lhv
dgp: cks szf vnd tvv rlf mvs
jxs: hbt xfn msv
dqs: mtk rsp
bpp: hhv
hsx: gxr nrv nlb znh
fnt: vjj tqh
gll: xcp lvq sbv tks
tzr: znh nnc
kkj: hcg kpf qpm gqs
zxq: hnr khs lkk zct vln bmj
ljd: hrn
rmj: zcl cjp pdq
zzg: gph mmc rnq xdp
tvp: htv pgc tjd kjj
ttg: dtg pvs bln
mxr: tdr kgs
qnb: fnk cbk bmd
lks: ngl
hjh: dlg hns gtq ssm
cxr: cxg
zts: jxr pnb
fhl: qjg mrp xjd pjr rsj
zfg: zvt bsx tzr fss htv
hzf: vzc xcp zvt phh
vhr: fsc kbq mqz vgr
dqc: ndz svq fsx
fqt: dzv
kcb: pnc nnh gkg bjs lzt
zfd: fdr
mdd: xhx
fmj: fzf htx ljd mfv
rtn: lfd dxr
hcg: hdh
nxp: bbj flx hpx vjs
rlh: fhh lvp tlm
gdn: sjk jml
hph: jld
hqp: vxv hkf sbh fnt
xtf: qbh
bgn: pvv fnd vvn lml mkf
bxg: mbr scs sdg jxf
vlk: xzg jtp
fhq: jck sft jjr zhd
lqz: vcx ccf ltd bjs
frn: mrd
qvq: gxv flj cbk pvm vkp
xtx: vdx fqj lgz mdh
rjr: hlz ctc mpm jkr
tql: rst gks frv ntx mrv
bsn: thj vlj
krn: prd xkr hrq ssf rzz
dkb: mcx qgp nqn
gbn: bkr xpz vjl prc nsc
fbd: hzl lzd ffm rbd
fqx: vjt kkr lrs
ljh: hzl
kmp: xsd pns tqc fqx ntn
ndh: ssb zvm
tdr: bfl
mpj: cgr zkf ngq
cvp: mrr lff zqx
bht: slt bmj ppj
bxm: pqq
zct: bpz
hls: qpn mtm skp qnq
rvh: kmm
qqh: dnf glp vmf cmd
tnd: fgx hxx znr
dmp: hzl zjr fhp
vvq: zbh
mhj: pqt
lfj: bsp xfx bvg
cqv: xht nlz ldf
hfh: cln
jqq: tff bql krq sgj kdg
rdg: jll qfk
sql: lsv jvb
nsl: zpq lkl zmm
pzk: fbp hbt
pbj: kbq lks
npt: hbj vtx sdc gqj
zrq: zlv jks qjg mhd
xnr: lkx nlz tmf
kns: kgj smm ljt btj
bbx: mqk gdn
lfp: gtc gsk
mvf: cpb xvz jpc
jmb: fnd
svp: rdd tjc csd
mdj: gxl ldt sjh
btj: tff dtt
vgg: zcd tqz fnk
zll: fgx sgh kfc hvg xpn
xcp: xvs rjq
bln: ctc
blb: mrd fhh
lck: rcg fdr
svq: fdt cnz
rzx: mbg
lbh: dgz jvb pqd llk
nlz: hsp
hvh: nrz rlv mrn
vzt: rtq rks dpc dgp tmb bnp cfd
vlj: sxg vvp
vzb: djd
fbr: ftj mxr rvf ncs
jcc: gjd fxj
zzc: jck jld
jmg: lfv cvc kvg qlv
zfq: bsn vkp zjv
ccb: pqq
zjr: gbv xth
bnb: mlq slk cxr shb
nbj: drq gtc
jcb: jkr
vsb: kbv fnd xvl
bhn: nbs hnq mqr zlg cmd
slk: gnm dcn
sgr: fdr hhh
vss: csc hrk hsj pjz
mpd: lzb knb rqj
xjj: ctp
lsz: rqn crd zsv
grc: kln qdz fss dpx dgz mbh sbl
nlp: mlk dtg
rvf: ckc
xcq: rcs qfb cbj vtt
mqz: hcs
lcp: ztl vjk
hhx: bcf
fpd: rbc nfr fvc zkk bfh
smp: vkp
knh: xbn rxf dmc
gjd: bmj clk fcv
flj: htf mdd
cxg: hvb blx
ssm: lkx
hvl: zvg hzl
pqp: lst mdq rzx
htd: sjk gjz
nth: sbh vbh zxs
mdh: fmb qrj qzm
vjb: qht pjx bql
vnd: jkr fjd
vbv: jml nrv dkm kqq
xzp: bzb bcf bqz dbq
nmx: mmq xzf
fgx: hkt bmk
sdc: dbx zkk qsr vrx
sxb: hxx
gzb: dgk
pns: lph cjc lvp
mrm: pnb xcc sxb ghh
dhf: zlq lrd jlk
ltf: qtj slz ppj dxr
cgm: ktt jkn kzf gdq
htz: jrc gqs vpd
mlz: hns fhf mdh djd
fjn: vfm hvh ftj hgn
nvt: xzp sgj tpb
kpv: trr jpc vxk ssm
fss: vbl
cmj: xzf lzz
cnn: zvh bnv gzb jqm
mkf: kvg
lfr: brh gnb zhp
tgb: flt gph ttc
mcg: tjd mdd nrv
lfd: ffp bdj hxc crq
tjq: nsc bgk fhm
fll: hnq svq khg
rjv: xsg
hnf: hrn
rsk: nrv jgt
pkd: vlk cks mrd qbd
vfn: gcq pgp
ggl: pdq sph dgh
cln: zfd
fxn: ptl ftq ccm
znj: rhs qcz qpm ghg xpr jkr
xdm: rmx hrq
kjj: prp nlz glg
pgp: nbs
pcp: gdp
phr: jkr vjs rjv
dbq: cln
cmh: ndr vvk tdm vkj
zxb: crt flg
kfc: gzl
fjd: pcp ktr xts
mrt: xbn gbr kxl
gsl: hvb rrr
zmz: tgb mbc xfm
hgn: hcq
vjh: vpj
flg: tfx
ptm: jmm vjb czd fqj
jkb: vpf hhh vdh jgt fqj prn mbd
fln: rfc lnf lfv mtz
bnr: jvf xth mcx
hxc: fkg kvg hpn
rst: tpq
rqd: hqv cpf nnc kkx
cmx: qpm dtf gph sgf
dsf: blb rnq qjf
xth: kfx xkv
ztx: tld lfm qmq
fvc: hkq
tpq: rsf glr lzp
qdz: zvt zxs lnt
jfq: cmd xpd
fdc: mzf rrr mcj bzj
pxc: gmg jxk tzj vzc znq
vgk: ktt tqh
ksv: cvc
tnc: sjk dgz gdg ppr
hns: txp
lkx: bxb
jtp: vkf cgs
zck: qls xdm
mkj: clh
nfx: mqr xkv khg xxl
hnl: vjz cpb sbg
rmc: mpd ppz ndr lmp
qcd: kfx
rqg: dxr nnz xvp vjk kst
cgz: gpv vjr zzc snv mgh xrg
znd: nlt vpj crb sbv krh
gtq: zkn qrj qxj tfx
pzb: vkj
kst: nbs djm
xph: dqt
svz: vqf
vjs: nln
nhg: nrt qnb mrn
czq: nnc xxd jgt pzk
jbg: xkv xzf ksv xxl hrl dbg
gfm: rfc drq
gpq: rlv lfm sgq
xgv: rzz qlk xgk
ndz: llc fdn vlq jhs mrd
mmb: vvv xjf djh tdh
ktc: mjv zbk nht pbc
pct: qpt mpn
gtj: lfq xtf bsh
hpx: kkr vjk jzs
rch: htv qnq gvs cgr
vpj: qrj
qfs: mkf gcg qls qcd hvl
pjq: kbl kgs qvb
kxn: rdg rvf dpj hhh
rpj: gzx rvg qfq svp
fzp: sgl hdh xkp
dpr: ztx hfh gzl vlh qzm frv vgf fnt
crs: htx glr jll
ftf: tvv mmq qzd xpx
czz: vzb vxv lkx
kdl: fnk xjc cpb glg
vbl: hkf
tmk: nsb bpm
lbc: xjf vzc rbh
mmz: fdt ffp btx
jpp: qvd vkf zck
vvn: ssf lfv ccq
ldf: jgn qdv
znh: jll
jjh: gjf
fql: lsf rrr sxp vsb
mpx: bsg
dpc: rfc vvq
zlx: xfm rzx hvl mnp tqc
nnc: thm
xjd: tqm xjg gzg
xrm: kst bcm bqg svp
pgc: crb drf qzj
cpk: rvg pgp nhj hpn
qvb: tst
thd: bmn
mcj: fsx cxm jsj rhb
szf: djm
vrx: zcd
vcx: glt zts
dxr: qzd
pqs: vsf jnx fgj jcc
bcp: nzp vjr qmj
rvg: llc
rdd: gcq
tnv: qcz vln qls nkr
ntn: bqg fnj
cts: fvt glg qtf msq
phx: cfm hxx zxg
xfl: jzk sqx
mxb: hlz vzx zlv fnj
tdh: vzc
stl: bdj
rft: tkd pkq
dlf: gvb xkp
lrj: mpx msq vtm
qcf: dqs slk mtk jcb
ndr: xph
tzl: ffp
kmm: llk
shz: rkm sbl
mjv: gqj mbd sgr
flq: znq rqj dgl mbh jsv
rhg: jpx vdr vnc vkl zxs
tdd: qbf npf zjb gzg
sdg: rbh zcm vcx htv
vjt: fkg jhs
mpm: bln cjc
tvv: rvx vzx
rmb: klb msr dkm
pkb: vmx ffm gpr ccq
sjt: mhh ftb zct svp
mpn: fbp bsg
hsv: jqm nqn szf txs
lhr: jsv hph
pgz: zjb jmb
nxf: xvh bzr zmk
vfq: tmf tfh qgn mbm
bkt: zbh ffs
vcl: thx bxb
hvc: zmk mzf fdm vcb
kln: tnf
kzb: mlz tnf dlg krh
spj: nqn xsp dcj jnx
mgg: jsh qjq mlr mqk
vlb: xmc pxc pjb cxh
lst: vjk
vlh: rhd
dqv: pvv kxl
klc: txp lpv nfr
skn: kql bbf rmn tvd qfk
bhd: glg bmk pct mkj vdx
jjj: tks pmc cjz tbm
hkm: kfc hvg xzb
srt: bnv rft xdm xpx
jdz: zhd mrn rpb vgk
tgl: xth zsd slz nkb
mrv: gzl
sdq: tnd ctd rbg kzk frr qfb
xpd: nzn
cgd: qbf dcj
qxp: pzb bzk
krq: zjf pdq ctp
gxn: nht zgh xtm nnh qtn
jsh: vrx
jxj: rsk zpm nth rqd
pjx: vqf tpr rms
lfq: flr
sdv: vpj glg fsd
sxk: flt vmx qtj ffs
pmh: scm cjz crt
pzm: trl tmk
hst: xhx jxf dqt hph
zkt: qhx szb qfb mrv
ngt: cgs npf hzc
xrj: ldt bfd qxl
jjr: qgl xph fvc rbg
rbc: xcp qpn lbc
xvl: ffs ffm
sst: nmp kcq
jxp: kkx ncs bxt mbr xjs
bcx: crs zgh lcf
jhx: rnq qzv pgz nms
zlv: zmk
qtz: xdk bpg
kzf: jtb lvq
sdz: jxf kqq hvg
pmv: zlg rff
hrq: sqx
mrp: dft hdh hbx
xts: hlz vvq
sbc: fsd qzp
nfr: sbh
xkr: dhf svq crq
tck: xzb sgq
cfd: mcx xpd trl
ctd: rms
hpl: fsd cgr mpx
ncs: vqf hsp zzq rbh
jlt: lzm pbj
srd: xjj zzv
flc: jvb flj mgg tkh
gzx: jlk gfq jvr gth
txf: qch ftj mpx
xhv: bsh tzr jzr qxh
cbq: ldt mhh
zvh: mdq qvz gbp
bxs: pcd khg pqq
hcs: fbp djd
zvm: ftq xvp
nkb: rsp
mks: qdv hvj cmb fvc
bbt: dqs hvm lsp gtc
ccx: vfn mnr
vgf: krh hvg
kgz: tlm
djg: rzn hkm phx
kfv: fzf psh czd bmk xlr
fmk: rhb nmp qcb gph
rsj: xkn
mbx: kvg
vvv: ktt
dlg: gdq
flf: tmm nnz jtt ktf
jhp: ngl ghh xdk gdn
xjc: ffb
fpr: tbm hhd lck rlp mmt
xfj: tdh jms qjq xtm
tlm: gzg stl rvg
zbs: rqn slx drq
nxz: kgz njs gkz kvg
xbv: smp fqt hfm rjq svz kbq
kpf: xgk
hzz: vxk rzn dbx xrg
vzn: qfq kbs dxs
djh: xgq ggh kgl ngl
rbd: szp tfg
lqm: jnp fmz hqp nzj
lzm: xhx bfl nlz
crf: vgd gsk mqc qkc
lzn: dng blx
tzx: srd tpl jml xnt
phh: smp
cxn: znq tdm btz ckc
bqd: nnc mbd csk
gjj: vxx zgr
ztv: mln
tdm: rjq mnm
vgb: pkq
phq: ntx sbv lbc sdv
sxq: txb brs mnr vxq
xqx: zpm ldq cpf hrk
lph: rhr
mtm: pbc lzd
bgz: zvg jtt pbh xsg
fnp: bbf ksr
krx: xpd gbv gnm drg
dpx: zpm lvq rpb
snv: tkh vkj xfn
pfh: vfn xjg nlp hzm
frr: bvg ssm fkt
zsg: gcj crr jld
ngx: bnm
lrn: jtt
gxm: sgf zzx mtk rrb
njs: tmk mbx
ddz: pnb
qmq: thx pvm
pbh: jsj
mrx: jtt
fsz: zds
bmn: rfm
pmc: jvb hkf
lqh: lkb pjb mpx
pbc: ngq htd csk
bkc: nkb mnc jjz vqg
nfq: gvq zvt vfm
kcn: mhj
mlr: htl zgh gxr clh
jmq: gzz xnq cxg btx
qjv: fcl pxz tck jtb
dbg: gqk hvb cmd
qxs: pzm tjb rrh fll pbh
nbn: dbx vrx
qnq: hbt
gcf: vsf qzd
rcv: tvd jmm
xzk: qzp sbl tpl
bkr: bfd
kbs: zbv fxj krv
fdr: zcd
dnf: xxl xsd
vkl: rsf qxj
mrc: nkt vlk ldz dft
ccl: zjb ksv lsz
kxq: hph brh fqj zkf
ctq: pvm rbg jlt zjf
csc: nzq tzj
kdv: fnk xgq czq qvx
sjn: zlq cgs
zhd: kmm hph
clh: fmb bdx
ccm: sqx
bzd: thd gqs mhd gnm kpf
nrb: qpm gcf qmz nsl qxl
rjp: ksr zcd gcj
xxd: fnp qzp bqz vvv
xjs: vmg qgl
nts: rsk xnr mlr snv
zcm: nbv dst
gdg: ppr
lsv: skp
zbq: nrz bsb
bnq: jsh pgb nnc nbn
dgg: xfm zjr dcn
sns: gxq rrh tmr ccq
pmp: tfx bbf msr gpv zxb
htq: mdd btz bxt hth
zqx: hdd kbv pql
kgl: vcl vxk jts
xht: bsb dvr ltd
nhz: vrr rhr cms mbg szf gzx
qrq: glp ngt lph qcd
rcs: qdv
vbh: lcf pbj mpd
hgd: zkf tpl hnf mgc kcz
zbk: klb kcr
vgz: xfm zgx psp ggb
nmf: qqd thd sjt xsg
szb: zzz tqz xnt
vpf: rpn znq
fxj: vcb
rlp: rzn bxb sxb
txs: slt bpz
smm: bql fvc jxr bzk
vtm: xhx gdg
tqg: rbd dgk
ljp: bzk hbt xjt glv
nzq: jmm mpn mbm
ttc: mmq blx
shb: knx rzz
nxq: pjb msv pmh jzr
gxv: qhx ggd ckc
dvj: mks gfc frr tvj
crt: nzp psh
drz: hvh gmg zxx gjf kgt qsr
jnp: nhg vzf vjr jzr
ttj: mrb mtc rvf cln rpb kgv
hfm: sgh
pjp: rgk gbr
hdf: sbl frr hns pct
bmd: xjf rms
zlq: vkf
kdb: msr cll hzz vvp
msr: nlt djd
fbt: qlc dbx lzt
rmx: ksv
flx: tmm qcd cbq
jjz: gpt llc nzn
jhv: ltf cvc pfm blb
rxp: skp svz llk kgs";

    }

}

