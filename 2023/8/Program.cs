﻿using System.Diagnostics;
using common;


namespace _8
{
    internal class Program
    {
        const StringSplitOptions Tidy = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        static readonly string xinput = @"
LR

11A = (11B, XXX)
11B = (XXX, 11Z)
11Z = (11B, XXX)
22A = (22B, XXX)
22B = (22C, 22C)
22C = (22Z, 22Z)
22Z = (22B, 22B)
XXX = (XXX, XXX)";




        static void Main(string[] args)
        {
            List<string> matrix = input.Split("\r\n", Tidy).ToList();
            var moves = matrix[0];
            var links = matrix.Skip(1)
                .Select(x =>
                {
                    var y = x.Split(new[] { ' ', '=', '(', ')', ',' }, Tidy);
                    return (node: y[0], link: (l: y[1], r: y[2]));
                })
                .ToDictionary(x => x.node, x => x.link);

            Part2(moves, links);


        }

        private static void Part2(string moves, Dictionary<string, (string l, string r)> links)
        {
            var currents = links.Keys.Where(x => x[2] == 'A').ToList();
            var results = new List<double>();
            foreach (var current in currents)
            {
                var ix = currents.FindIndex(x => x == current);
                var single = currents.GetRange(ix, 1).ToList();
                var iter = moves.RepeatChars().GetEnumerator();
                var moves1 = ContinueLoop(links, single, iter);
                var moves2 = ContinueLoop(links, single, iter);
                long moves3 = ContinueLoop(links, single, iter);
                results.Add(moves3 / (double)moves.Length);
                Debug.WriteLine($"{current}: {moves1} {moves2} {moves3}    {moves3 / (double)moves.Length} ");
            }

            var tot = results.Aggregate((double)moves.Length, (x, y) => x * y);
            Debug.WriteLine($"Total= {tot}");
        }



        private static long ContinueLoop(Dictionary<string, (string l, string r)> links, List<string> currents, IEnumerator<char> iter)
        {
            var allEnd = false;
            long movesCnt = 0;

            while (!allEnd)
            {
                iter.MoveNext();
                var mov = iter.Current;
                allEnd = true;
                for (int i = 0; i < currents.Count; i++)
                {
                    if (mov == 'L')
                        currents[i] = links[currents[i]].l;
                    else
                        currents[i] = links[currents[i]].r;
                    if (currents[i][2] != 'Z')
                        allEnd = false;
                }

                ++movesCnt;
            }

            return movesCnt;
        }

        private static void Part1(string moves, Dictionary<string, (string l, string r)> links)
        {
            var current = "AAA";
            var movesCnt = 0;
            var iter = moves.RepeatChars().GetEnumerator();

            while (current != "ZZZ")
            {
                iter.MoveNext();
                var mov = iter.Current;
                if (mov == 'L')
                    current = links[current].l;
                else
                    current = links[current].r;
                ++movesCnt;
            }

            Debug.WriteLine(movesCnt);
        }


        private static readonly string input =
            @"
LRLRRRLRRLRLLRRRLRRLLRRRLLRLRRLLRRRLRLRRRLRRLRRLRRLLLLRRRLRRLRRRLRRRLRRRLRLRRRLRLLRRRLLRLRRRLRRRLRLRRLLRLLRRLRLLRRRLLRRLRLLLRLRLRLLRRRLRLRLRRLRRRLRRLRLRRRLRRRLRRRLLLLRLLRRLLRRRLRRLRRLLRRLRRRLLRRLLLRRRLRLRLLRRLRRRLRRLRRRLLRLRRRLRLLRLLRRRLRRLLRLRRRLRRLRRRLRRLRRLRRRLRRLRRRR

DQS = (CCN, PNF)
GST = (MBG, LFG)
SQF = (KCM, KCM)
NXD = (DCX, PKH)
LMN = (NTT, LJM)
MTQ = (DHL, PVN)
DDR = (TGF, RFT)
HHV = (KXC, CMQ)
JLX = (VSK, XPM)
MLD = (BNF, FGL)
RCQ = (TNS, MTQ)
LPT = (FFD, RVM)
GNV = (TQV, RBP)
DTG = (QKC, GBN)
QRM = (FTV, JJG)
PJN = (FQD, CMN)
FTV = (PCN, FHH)
LBN = (XHT, MNB)
LXF = (CBQ, DLB)
FRL = (DTF, SQD)
TMD = (HLR, MRQ)
PKH = (MSC, BLM)
FJB = (FRV, QPL)
JNS = (GHQ, MXC)
XMH = (NHH, DLT)
FRX = (SDF, THQ)
CVB = (BXQ, TDM)
BRV = (XDQ, GLR)
HRV = (SBB, QGT)
CFC = (XHT, MNB)
FKB = (PTG, NKC)
VJN = (PKQ, CKN)
NBB = (JVX, PBR)
JRR = (HMF, CRJ)
NKD = (HFG, JVD)
VXK = (VXS, RCQ)
CNG = (HJM, SSB)
DBP = (PPP, LVK)
MNH = (CNT, NMH)
NRX = (CLC, QNF)
GSS = (QKL, PJV)
VJM = (LHL, PTP)
PGN = (VXJ, DQK)
BDN = (PSJ, KQG)
VVF = (QPT, FKB)
PMN = (KDP, XRZ)
DFG = (GLF, XPS)
SGS = (CKL, GHG)
CMQ = (QFP, CVB)
TVF = (PBP, KQQ)
DGL = (BGP, NKD)
NPT = (RBK, NLH)
RBP = (PGQ, XBS)
QXR = (NXD, MKF)
TQV = (XBS, PGQ)
SDD = (CPM, JJJ)
RTL = (FBF, NSB)
GVT = (VGS, GVX)
LFM = (VRN, MJR)
FJT = (FSX, NJX)
XQG = (LTR, TPN)
MQQ = (BXN, CBD)
DSJ = (JDD, JDD)
FSR = (QCD, DFC)
TVS = (XBJ, XSC)
GTT = (NJQ, BVV)
DVV = (MQQ, QHQ)
JMC = (HRQ, TSJ)
DCX = (MSC, BLM)
KHB = (SJV, VLM)
VSK = (KRD, QFG)
BFT = (LTR, TPN)
XPS = (SGS, PRL)
RDK = (CTN, QGQ)
SHJ = (NKD, BGP)
VQT = (KQJ, FKK)
VXT = (CRK, NPK)
GXD = (RQN, FXK)
TQC = (CSV, FGS)
KGS = (GNP, TGK)
KQG = (XFG, KMG)
HCP = (HHD, KXB)
RSC = (SRQ, HBX)
QSK = (PMJ, TPH)
JSP = (NXD, MKF)
LTK = (JJC, STN)
XNJ = (LTK, TBQ)
LTH = (TTM, DQB)
LJM = (TVD, HHG)
SVN = (FQM, XVJ)
FSK = (DSM, NFJ)
TLX = (QXS, SDG)
LFB = (XSF, TCN)
QLB = (CFC, LBN)
KDP = (MGL, RXR)
XNR = (QCL, BVH)
BPL = (TLX, DQJ)
SPP = (HXF, CBC)
BBA = (TVC, KCX)
DXX = (PPP, LVK)
PPP = (XBM, JCN)
MTV = (GLL, KSM)
VXS = (MTQ, TNS)
TXQ = (GFF, HPD)
DQK = (TTQ, GSS)
MGL = (LNN, GTT)
XFG = (PNT, PMX)
HBD = (JQK, LBB)
TDP = (QMS, JNV)
TFB = (KSV, GJJ)
JTC = (MGG, TPC)
GHG = (NDH, HVG)
TVR = (QHX, GSC)
QDR = (LVM, SVK)
HDQ = (HBD, JPX)
HFG = (BMS, PFX)
FDD = (BFT, XQG)
JCN = (TDL, XBK)
SNS = (DMN, GVT)
HJX = (GDM, HCZ)
RSG = (NBV, LPR)
BDV = (RFB, HSN)
DVK = (LGR, MTV)
RGL = (LJM, NTT)
PNT = (NNP, BRR)
KLN = (JDD, QRZ)
TKQ = (SJS, JRR)
SRJ = (CXL, XMH)
TNB = (FBH, TML)
PKQ = (MCV, LDN)
MCV = (FSK, QJP)
DTD = (CXB, TMD)
HJM = (SHB, DSH)
GLL = (QCJ, NBB)
BBB = (GDQ, GGB)
RFB = (NXT, MXM)
CHX = (BFT, XQG)
JDR = (LGR, MTV)
PVJ = (HXK, LGG)
DRQ = (JHP, XGJ)
NLX = (SVK, LVM)
PQB = (RSC, GJG)
XNS = (LPM, QHR)
QJN = (PBP, KQQ)
HHG = (FJL, CMP)
NHH = (KNG, GND)
BCF = (NVH, FNK)
RVM = (QPV, TQC)
MMG = (TDP, MQM)
SLJ = (SCL, GST)
MGB = (CBC, HXF)
NVB = (QPT, FKB)
LKB = (LJP, SLZ)
JTH = (JPC, JPC)
VNV = (PCF, DTG)
LBB = (FJT, HSV)
CKR = (QKG, DCJ)
NLH = (VTL, RDK)
PPL = (RTL, JCX)
HTX = (FPR, VCQ)
JKJ = (NHG, QBX)
TPH = (MSJ, NMP)
PGQ = (HRV, FXB)
PVN = (HKL, XNV)
DRB = (LHJ, XHR)
TGF = (VVP, GSV)
BVV = (NKG, CTJ)
BRR = (HHS, MCH)
DGX = (RTL, JCX)
CNT = (NRF, SDD)
SLZ = (DNT, KVD)
XVJ = (LNJ, JXV)
XPK = (QHQ, MQQ)
XJR = (XPK, DVV)
JLN = (RXM, SQK)
XDQ = (NHM, XSH)
JVD = (BMS, PFX)
PMP = (FRX, XLJ)
CRT = (TMD, CXB)
XBJ = (BPL, HXB)
QCL = (JDR, DVK)
CXB = (MRQ, HLR)
NTR = (FLC, KGK)
JCP = (JKJ, QTL)
GNB = (THN, MXG)
KQJ = (FLJ, DJC)
KSM = (NBB, QCJ)
BLA = (KVD, DNT)
NPK = (TVF, QJN)
BMS = (TXQ, XND)
BNQ = (FQM, XVJ)
PPJ = (LTH, JBV)
FSD = (JKG, QLB)
CFB = (PCF, DTG)
BCR = (LMX, XJJ)
DMN = (VGS, GVX)
TDL = (JCP, CLX)
GSV = (VXK, HPC)
LRV = (CQG, LFB)
NBK = (DMN, GVT)
QQV = (GSC, QHX)
PMJ = (MSJ, NMP)
KJF = (CRQ, TCK)
TTQ = (PJV, QKL)
JDM = (KLK, TLS)
KVD = (NLX, QDR)
TRK = (FGL, BNF)
JQG = (CST, VLQ)
PNF = (XPG, DPP)
DLT = (KNG, GND)
LJP = (KVD, DNT)
CRJ = (SRJ, QBR)
MGG = (PNP, GVP)
VCD = (CHF, GDV)
TJD = (VXH, KDF)
QBP = (VXH, KDF)
TTM = (CHJ, VXT)
TVC = (VJS, HTX)
RFT = (VVP, GSV)
XDK = (CVS, QBC)
JMF = (NTS, VJN)
LRR = (PTP, LHL)
TVD = (FJL, CMP)
PSN = (NRX, NQC)
CKK = (TGF, RFT)
QGT = (TTH, JGB)
SRQ = (LHV, NJF)
QKG = (BDV, TLB)
MXM = (CVM, SMG)
LCR = (XPS, GLF)
JTT = (TSR, RDX)
QLS = (CHF, GDV)
TBQ = (JJC, STN)
LBQ = (TXB, VMF)
FMF = (SCL, GST)
QCD = (FRL, CRL)
TPN = (VQT, GQD)
LLT = (SJV, VLM)
VJC = (JXS, DFJ)
MXC = (NDJ, KJF)
FBF = (TKQ, BTB)
JTJ = (VQV, DDP)
JHP = (LPT, TBV)
BJS = (KMX, KMX)
FBJ = (KDP, KDP)
TNS = (DHL, PVN)
DQJ = (SDG, QXS)
FQD = (DBC, GNQ)
CKJ = (FBJ, FBJ)
SDF = (QRM, JHV)
KCS = (MRT, RSV)
FVH = (GSD, XHH)
CRK = (QJN, TVF)
PKC = (VSK, XPM)
DSH = (VNV, CFB)
FLT = (LMX, XJJ)
TCN = (TFB, JPV)
PSJ = (XFG, KMG)
NJF = (FPN, DTK)
SRP = (DSJ, DSJ)
FRV = (CFR, HVX)
QPV = (FGS, CSV)
TTH = (DXX, DBP)
VRN = (GGC, DRQ)
NKC = (RVL, PVJ)
MPT = (KKX, LFM)
DPT = (KKX, LFM)
LMX = (LRJ, PMP)
SHB = (CFB, VNV)
GJD = (LBQ, JLL)
QBC = (QSK, QSB)
NHL = (XPK, DVV)
TBJ = (KGX, FGG)
GHQ = (KJF, NDJ)
VHV = (LPM, QHR)
KXB = (BBQ, SSH)
DFC = (CRL, FRL)
BVC = (LJP, LJP)
JCX = (FBF, NSB)
CLC = (QMC, NBP)
NHG = (KGN, JSL)
SSK = (LTK, TBQ)
KRD = (PSN, SLF)
JHV = (JJG, FTV)
GGB = (LNM, SHL)
FGS = (FJB, LLB)
SMG = (KKQ, MSG)
JGB = (DXX, DBP)
CST = (RCB, LMP)
FMK = (KGP, KSJ)
KCL = (VLQ, CST)
MSG = (BNQ, SVN)
BNF = (QKN, KDG)
RFG = (GHQ, MXC)
DBC = (QPG, HDG)
CMN = (GNQ, DBC)
BTB = (SJS, JRR)
TGM = (BVC, BVC)
XQP = (RHG, BBB)
LTR = (VQT, GQD)
SST = (QXR, JSP)
NTS = (CKN, PKQ)
VSN = (TLS, KLK)
NHM = (CKJ, CKJ)
HKJ = (RFG, JNS)
FBH = (NCC, SGM)
LNN = (BVV, NJQ)
CRL = (SQD, DTF)
TDM = (LCF, JTJ)
GDQ = (LNM, SHL)
TCK = (TVR, QQV)
MCH = (JCD, BKG)
HTM = (KGP, KSJ)
TJQ = (SPP, MGB)
TFV = (VPQ, HVH)
MKF = (DCX, PKH)
LPR = (QLS, VCD)
DFV = (PGN, LGS)
NVH = (JRD, TGX)
BVH = (JDR, DVK)
KQQ = (DGH, LBJ)
KKX = (VRN, MJR)
PBV = (KGK, FLC)
MRM = (NVH, FNK)
CCN = (XPG, DPP)
MBG = (FPD, KBG)
DJJ = (CNG, NMX)
TSR = (FMR, JMX)
GDV = (MRM, BCF)
CBD = (KBX, PHJ)
BGP = (HFG, JVD)
NSB = (BTB, TKQ)
KBX = (CKR, CTM)
HVG = (LRR, VJM)
RMF = (CRT, DTD)
XHH = (VFR, CHB)
RSB = (XDQ, GLR)
SQK = (FVD, DFV)
LLR = (HLP, PQB)
RDX = (JMX, FMR)
GSD = (VFR, CHB)
HLP = (GJG, RSC)
SJS = (HMF, CRJ)
RXV = (BJS, FDS)
NBP = (VFH, KBN)
XPG = (XCJ, BKL)
GHC = (THN, MXG)
GNP = (DRR, HGV)
TJS = (KCM, HJX)
MQM = (JNV, QMS)
CSV = (LLB, FJB)
KXC = (QFP, CVB)
FSX = (RXN, KCS)
HQC = (SNS, NBK)
XBM = (XBK, TDL)
BXN = (KBX, PHJ)
HKL = (BMN, SST)
FDF = (JLL, LBQ)
XCJ = (PBM, MCG)
LPJ = (KDQ, KQC)
JQK = (FJT, HSV)
QTL = (NHG, QBX)
KBG = (SSK, XNJ)
PTP = (NMJ, LRV)
VPH = (BJS, FDS)
TSJ = (JGM, JMF)
MCG = (DMG, PPJ)
GDM = (LMN, RGL)
QPG = (SKM, XNR)
RTS = (JPX, HBD)
QNF = (NBP, QMC)
BKL = (PBM, MCG)
NRF = (JJJ, CPM)
JVX = (HTM, FMK)
FLC = (XVG, MNH)
DTK = (BCR, FLT)
VPQ = (RXV, VPH)
LBV = (FQD, CMN)
XRZ = (RXR, MGL)
HRQ = (JGM, JMF)
FGG = (SHJ, DGL)
XGJ = (LPT, TBV)
LLK = (JPC, FDZ)
FKK = (DJC, FLJ)
TJJ = (GNP, TGK)
SHL = (BFG, DQS)
TGK = (HGV, DRR)
CBQ = (CHR, RHK)
SQD = (VKX, DQR)
QPT = (PTG, NKC)
QKJ = (QCD, DFC)
PBP = (DGH, LBJ)
QKH = (DSJ, KLN)
HVH = (RXV, VPH)
KSJ = (TFV, HKM)
XNV = (SST, BMN)
VPT = (RHG, BBB)
CPM = (BDN, MMX)
RBK = (VTL, RDK)
JPC = (XTJ, TVS)
NDJ = (TCK, CRQ)
VFR = (KPK, DRB)
TBV = (FFD, RVM)
QGQ = (XTL, VJC)
PFX = (XND, TXQ)
PBM = (DMG, PPJ)
SSB = (SHB, DSH)
XBS = (HRV, FXB)
JBV = (TTM, DQB)
RVL = (HXK, LGG)
KBN = (TJQ, XBH)
FXK = (TNB, TSP)
MMT = (CRT, DTD)
KKQ = (BNQ, SVN)
HSV = (NJX, FSX)
GGC = (JHP, XGJ)
LVK = (JCN, XBM)
HCZ = (RGL, LMN)
GVF = (KXC, CMQ)
RLQ = (MPT, DPT)
TPX = (SQK, RXM)
TGX = (CVC, GXD)
VTL = (CTN, QGQ)
MMX = (PSJ, KQG)
STN = (DJJ, XSQ)
JKG = (CFC, LBN)
PRL = (GHG, CKL)
XDL = (PKC, JLX)
PHJ = (CKR, CTM)
XND = (HPD, GFF)
VXH = (TGM, TGM)
RQN = (TSP, TNB)
HBX = (LHV, NJF)
NKG = (CVL, FSD)
CVC = (FXK, RQN)
FPD = (XNJ, SSK)
JPX = (JQK, LBB)
VFH = (XBH, TJQ)
CTJ = (CVL, FSD)
FJL = (CHX, FDD)
MRQ = (TJJ, KGS)
HSN = (MXM, NXT)
TML = (SGM, NCC)
QHR = (HKQ, GPP)
VKX = (DGX, PPL)
AAA = (VHV, XNS)
CFR = (JMC, KBM)
PRT = (HXQ, JTT)
BKG = (GHC, GNB)
SCL = (MBG, LFG)
DPP = (BKL, XCJ)
XHS = (BVC, LKB)
CHB = (KPK, DRB)
TLF = (BVR, MMG)
QRZ = (KCX, TVC)
FVD = (PGN, LGS)
CTN = (XTL, VJC)
KGK = (MNH, XVG)
VLM = (LCR, DFG)
DSM = (TPX, JLN)
SGM = (FDF, GJD)
XQQ = (LDR, MHH)
SDG = (RMF, MMT)
HVX = (JMC, KBM)
JRD = (CVC, GXD)
GLR = (NHM, XSH)
PMX = (NNP, BRR)
XLJ = (THQ, SDF)
XBK = (JCP, CLX)
XLH = (XDL, XLV)
MTD = (CLB, ZZZ)
NFA = (MGL, RXR)
GJJ = (RSH, HCP)
CXL = (NHH, DLT)
GSC = (XDK, JTQ)
CRQ = (QQV, TVR)
LRJ = (FRX, XLJ)
CLB = (VHV, XNS)
CMM = (RLQ, RQQ)
DHL = (XNV, HKL)
XSQ = (CNG, NMX)
VVP = (VXK, HPC)
CHJ = (CRK, NPK)
QKN = (LXF, RBQ)
FQM = (LNJ, JXV)
JJG = (PCN, FHH)
DCL = (RQQ, RLQ)
BBQ = (NVB, VVF)
QMS = (XXP, RSG)
NFJ = (JLN, TPX)
RQQ = (DPT, MPT)
XSF = (TFB, JPV)
PNP = (XMQ, GNR)
QBX = (KGN, JSL)
JPV = (GJJ, KSV)
XCG = (LLR, GKQ)
BFG = (PNF, CCN)
GRJ = (TPC, MGG)
QCJ = (PBR, JVX)
LDR = (PBV, NTR)
ZZZ = (XNS, VHV)
GVX = (LPJ, TPB)
GBN = (XJR, NHL)
HXB = (TLX, DQJ)
JLL = (VMF, TXB)
KBM = (HRQ, TSJ)
XSH = (CKJ, MHM)
HXF = (JDM, VSN)
RXR = (LNN, GTT)
SLF = (NQC, NRX)
QPL = (HVX, CFR)
CVM = (KKQ, MSG)
CHR = (KCC, JBD)
FGL = (QKN, KDG)
GNQ = (HDG, QPG)
SBB = (TTH, JGB)
XMQ = (RSB, BRV)
SPD = (SRP, SRP)
JDD = (TVC, KCX)
FXB = (SBB, QGT)
RXM = (DFV, FVD)
FDS = (KMX, VKC)
CQG = (TCN, XSF)
CHF = (BCF, MRM)
QHX = (JTQ, XDK)
MJR = (DRQ, GGC)
DRA = (XTJ, TVS)
KCX = (VJS, HTX)
PCN = (HHV, GVF)
FFD = (TQC, QPV)
CBC = (VSN, JDM)
RPK = (MHH, LDR)
JTQ = (CVS, QBC)
SSH = (VVF, NVB)
KMG = (PMX, PNT)
JBD = (CQS, XCG)
DQB = (VXT, CHJ)
QSB = (PMJ, TPH)
KGX = (DGL, SHJ)
TSP = (TML, FBH)
LHV = (FPN, DTK)
LGR = (KSM, GLL)
CLX = (QTL, JKJ)
PSA = (LMN, RGL)
MXG = (KHB, LLT)
TLB = (HSN, RFB)
NQC = (CLC, QNF)
NMJ = (CQG, LFB)
NXT = (SMG, CVM)
LHL = (LRV, NMJ)
VQV = (FVH, RDN)
KDQ = (TJD, QBP)
DFJ = (TRK, MLD)
LBJ = (SQF, TJS)
CVS = (QSB, QSK)
JSL = (QKJ, FSR)
QKC = (NHL, XJR)
LCF = (VQV, DDP)
CKL = (HVG, NDH)
HKQ = (XLH, MDN)
DDP = (RDN, FVH)
RHG = (GDQ, GGB)
CKN = (LDN, MCV)
RXN = (MRT, RSV)
HHS = (JCD, BKG)
PJV = (GRJ, JTC)
HPC = (VXS, RCQ)
HGV = (LNS, NPT)
FMR = (QJD, TLF)
XTJ = (XSC, XBJ)
THN = (LLT, KHB)
TLS = (PRT, DBJ)
KGP = (HKM, TFV)
DNT = (QDR, NLX)
LLB = (QPL, FRV)
BLM = (VGC, TBJ)
GND = (XQP, VPT)
MSJ = (SPD, DRV)
LVM = (FMF, SLJ)
TPB = (KQC, KDQ)
RHK = (KCC, JBD)
LGG = (DDR, CKK)
LDN = (FSK, QJP)
JXV = (TRC, HQC)
GPP = (XLH, MDN)
TXB = (RTS, HDQ)
QFG = (SLF, PSN)
JMX = (QJD, TLF)
QFP = (TDM, BXQ)
XVG = (NMH, CNT)
KCC = (XCG, CQS)
DQR = (DGX, PPL)
FDZ = (TVS, XTJ)
XHR = (JTH, LLK)
XXP = (NBV, LPR)
JNV = (RSG, XXP)
HXQ = (TSR, RDX)
DBJ = (JTT, HXQ)
LFV = (CLB, CLB)
HKM = (HVH, VPQ)
NMH = (SDD, NRF)
FHH = (GVF, HHV)
QMC = (KBN, VFH)
FPN = (BCR, FLT)
DRV = (SRP, QKH)
NNP = (MCH, HHS)
NJQ = (CTJ, NKG)
MRT = (JQG, KCL)
KLK = (PRT, DBJ)
LHJ = (JTH, JTH)
VGS = (LPJ, TPB)
QBR = (CXL, XMH)
FXS = (JNS, RFG)
DTF = (VKX, DQR)
QHQ = (BXN, CBD)
BXQ = (LCF, JTJ)
GVP = (GNR, XMQ)
VJS = (VCQ, FPR)
HMF = (QBR, SRJ)
CMP = (FDD, CHX)
QJD = (BVR, MMG)
XLV = (PKC, JLX)
QKL = (JTC, GRJ)
PCF = (GBN, QKC)
LNM = (BFG, DQS)
SJV = (LCR, DFG)
KPK = (LHJ, LHJ)
GNR = (BRV, RSB)
NCC = (FDF, GJD)
RSH = (KXB, HHD)
MNB = (DCL, CMM)
XTL = (JXS, DFJ)
VKC = (LFV, MTD)
FLJ = (GNV, BPQ)
KCM = (GDM, GDM)
XBH = (SPP, MGB)
LMP = (PJN, LBV)
BVR = (TDP, MQM)
SKM = (BVH, QCL)
RBQ = (CBQ, DLB)
JCD = (GNB, GHC)
MHM = (FBJ, PMN)
HDG = (XNR, SKM)
KQC = (TJD, QBP)
NMX = (SSB, HJM)
HHD = (SSH, BBQ)
NDH = (LRR, VJM)
DJC = (BPQ, GNV)
GKQ = (HLP, PQB)
MHH = (PBV, NTR)
KDF = (TGM, XHS)
QJP = (DSM, NFJ)
VMF = (RTS, HDQ)
NBV = (QLS, VCD)
NJX = (KCS, RXN)
LGS = (VXJ, DQK)
PBR = (FMK, HTM)
GJG = (HBX, SRQ)
MDN = (XLV, XDL)
MSC = (VGC, TBJ)
NTT = (TVD, HHG)
FPR = (XQQ, RPK)
DGH = (SQF, SQF)
HLR = (KGS, TJJ)
LNS = (RBK, NLH)
SVK = (FMF, SLJ)
XSC = (HXB, BPL)
RSV = (JQG, KCL)
XJJ = (LRJ, PMP)
DLB = (RHK, CHR)
KNG = (XQP, VPT)
XHT = (DCL, CMM)
XPM = (QFG, KRD)
BPQ = (RBP, TQV)
QXS = (MMT, RMF)
CTM = (DCJ, QKG)
CQS = (GKQ, LLR)
PTG = (PVJ, RVL)
NMP = (SPD, DRV)
DCJ = (TLB, BDV)
RDN = (GSD, XHH)
KMX = (LFV, LFV)
KGN = (FSR, QKJ)
VCQ = (XQQ, RPK)
JJC = (DJJ, XSQ)
LPM = (HKQ, GPP)
GLF = (SGS, PRL)
TRC = (NBK, SNS)
HPD = (HKJ, FXS)
JGM = (NTS, VJN)
JXS = (TRK, MLD)
KDG = (RBQ, LXF)
VLQ = (RCB, LMP)
LFG = (KBG, FPD)
LNJ = (TRC, HQC)
CVL = (QLB, JKG)
BMN = (QXR, JSP)
KSV = (RSH, HCP)
RCB = (LBV, PJN)
FNK = (TGX, JRD)
HXK = (CKK, DDR)
GQD = (FKK, KQJ)
VXJ = (GSS, TTQ)
DMG = (JBV, LTH)
VGC = (FGG, KGX)
THQ = (QRM, JHV)
GFF = (FXS, HKJ)
JJJ = (BDN, MMX)
DRR = (NPT, LNS)
TPC = (PNP, GVP)
";
    }

}