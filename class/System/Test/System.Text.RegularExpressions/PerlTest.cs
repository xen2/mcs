//
// assembly:	System_test
// namespace:	MonoTests.System.Text.RegularExpressions
// file:	PerlTest.cs
//
// Authors:
//   Dan Lewis (dlewis@gmx.co.uk)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Gonzalo Paniagua Javier
//
// (c) 2002 Dan Lewis
// (c) 2003 Martin Willemoes Hansen
// (c) 2005 Novell, Inc.
//

using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions {
[TestFixture]
public class PerlTest {
    [Test]
    public void Trials ()
    {
        Console.WriteLine ("{0} trials", PerlTrials.trials.Length);
    }

    // We choose to run the trials in separate tests rather than in a loop, since we want all trials to be run.
    // A failing assertion in a test aborts the rest of the trials in that test.  So, we put one trial in each test.
    [Test] public void Trial0000 () {
        PerlTrials.trials [0].Execute ();
    }
    [Test] public void Trial0001 () {
        PerlTrials.trials [1].Execute ();
    }
    [Test] public void Trial0002 () {
        PerlTrials.trials [2].Execute ();
    }
    [Test] public void Trial0003 () {
        PerlTrials.trials [3].Execute ();
    }
    [Test] public void Trial0004 () {
        PerlTrials.trials [4].Execute ();
    }
    [Test] public void Trial0005 () {
        PerlTrials.trials [5].Execute ();
    }
    [Test] public void Trial0006 () {
        PerlTrials.trials [6].Execute ();
    }
    [Test] public void Trial0007 () {
        PerlTrials.trials [7].Execute ();
    }
    [Test] public void Trial0008 () {
        PerlTrials.trials [8].Execute ();
    }
    [Test] public void Trial0009 () {
        PerlTrials.trials [9].Execute ();
    }
    [Test] public void Trial0010 () {
        PerlTrials.trials [10].Execute ();
    }
    [Test] public void Trial0011 () {
        PerlTrials.trials [11].Execute ();
    }
    [Test] public void Trial0012 () {
        PerlTrials.trials [12].Execute ();
    }
    [Test] public void Trial0013 () {
        PerlTrials.trials [13].Execute ();
    }
    [Test] public void Trial0014 () {
        PerlTrials.trials [14].Execute ();
    }
    [Test] public void Trial0015 () {
        PerlTrials.trials [15].Execute ();
    }
    [Test] public void Trial0016 () {
        PerlTrials.trials [16].Execute ();
    }
    [Test] public void Trial0017 () {
        PerlTrials.trials [17].Execute ();
    }
    [Test] public void Trial0018 () {
        PerlTrials.trials [18].Execute ();
    }
    [Test] public void Trial0019 () {
        PerlTrials.trials [19].Execute ();
    }
    [Test] public void Trial0020 () {
        PerlTrials.trials [20].Execute ();
    }
    [Test] public void Trial0021 () {
        PerlTrials.trials [21].Execute ();
    }
    [Test] public void Trial0022 () {
        PerlTrials.trials [22].Execute ();
    }
    [Test] public void Trial0023 () {
        PerlTrials.trials [23].Execute ();
    }
    [Test] public void Trial0024 () {
        PerlTrials.trials [24].Execute ();
    }
    [Test] public void Trial0025 () {
        PerlTrials.trials [25].Execute ();
    }
    [Test] public void Trial0026 () {
        PerlTrials.trials [26].Execute ();
    }
    [Test] public void Trial0027 () {
        PerlTrials.trials [27].Execute ();
    }
    [Test] public void Trial0028 () {
        PerlTrials.trials [28].Execute ();
    }
    [Test] public void Trial0029 () {
        PerlTrials.trials [29].Execute ();
    }
    [Test] public void Trial0030 () {
        PerlTrials.trials [30].Execute ();
    }
    [Test] public void Trial0031 () {
        PerlTrials.trials [31].Execute ();
    }
    [Test] public void Trial0032 () {
        PerlTrials.trials [32].Execute ();
    }
    [Test] public void Trial0033 () {
        PerlTrials.trials [33].Execute ();
    }
    [Test] public void Trial0034 () {
        PerlTrials.trials [34].Execute ();
    }
    [Test] public void Trial0035 () {
        PerlTrials.trials [35].Execute ();
    }
    [Test] public void Trial0036 () {
        PerlTrials.trials [36].Execute ();
    }
    [Test] public void Trial0037 () {
        PerlTrials.trials [37].Execute ();
    }
    [Test] public void Trial0038 () {
        PerlTrials.trials [38].Execute ();
    }
    [Test] public void Trial0039 () {
        PerlTrials.trials [39].Execute ();
    }
    [Test] public void Trial0040 () {
        PerlTrials.trials [40].Execute ();
    }
    [Test] public void Trial0041 () {
        PerlTrials.trials [41].Execute ();
    }
    [Test] public void Trial0042 () {
        PerlTrials.trials [42].Execute ();
    }
    [Test] public void Trial0043 () {
        PerlTrials.trials [43].Execute ();
    }
    [Test] public void Trial0044 () {
        PerlTrials.trials [44].Execute ();
    }
    [Test] public void Trial0045 () {
        PerlTrials.trials [45].Execute ();
    }
    [Test] public void Trial0046 () {
        PerlTrials.trials [46].Execute ();
    }
    [Test] public void Trial0047 () {
        PerlTrials.trials [47].Execute ();
    }
    [Test] public void Trial0048 () {
        PerlTrials.trials [48].Execute ();
    }
    [Test] public void Trial0049 () {
        PerlTrials.trials [49].Execute ();
    }
    [Test] public void Trial0050 () {
        PerlTrials.trials [50].Execute ();
    }
    [Test] public void Trial0051 () {
        PerlTrials.trials [51].Execute ();
    }
    [Test] public void Trial0052 () {
        PerlTrials.trials [52].Execute ();
    }
    [Test] public void Trial0053 () {
        PerlTrials.trials [53].Execute ();
    }
    [Test] public void Trial0054 () {
        PerlTrials.trials [54].Execute ();
    }
    [Test] public void Trial0055 () {
        PerlTrials.trials [55].Execute ();
    }
    [Test] public void Trial0056 () {
        PerlTrials.trials [56].Execute ();
    }
    [Test] public void Trial0057 () {
        PerlTrials.trials [57].Execute ();
    }
    [Test] public void Trial0058 () {
        PerlTrials.trials [58].Execute ();
    }
    [Test] public void Trial0059 () {
        PerlTrials.trials [59].Execute ();
    }
    [Test] public void Trial0060 () {
        PerlTrials.trials [60].Execute ();
    }
    [Test] public void Trial0061 () {
        PerlTrials.trials [61].Execute ();
    }
    [Test] public void Trial0062 () {
        PerlTrials.trials [62].Execute ();
    }
    [Test] public void Trial0063 () {
        PerlTrials.trials [63].Execute ();
    }
    [Test] public void Trial0064 () {
        PerlTrials.trials [64].Execute ();
    }
    [Test] public void Trial0065 () {
        PerlTrials.trials [65].Execute ();
    }
    [Test] public void Trial0066 () {
        PerlTrials.trials [66].Execute ();
    }
    [Test] public void Trial0067 () {
        PerlTrials.trials [67].Execute ();
    }
    [Test] public void Trial0068 () {
        PerlTrials.trials [68].Execute ();
    }
    [Test] public void Trial0069 () {
        PerlTrials.trials [69].Execute ();
    }
    [Test] public void Trial0070 () {
        PerlTrials.trials [70].Execute ();
    }
    [Test] public void Trial0071 () {
        PerlTrials.trials [71].Execute ();
    }
    [Test] public void Trial0072 () {
        PerlTrials.trials [72].Execute ();
    }
    [Test] public void Trial0073 () {
        PerlTrials.trials [73].Execute ();
    }
    [Test] public void Trial0074 () {
        PerlTrials.trials [74].Execute ();
    }
    [Test] public void Trial0075 () {
        PerlTrials.trials [75].Execute ();
    }
    [Test] public void Trial0076 () {
        PerlTrials.trials [76].Execute ();
    }
    [Test] public void Trial0077 () {
        PerlTrials.trials [77].Execute ();
    }
    [Test] public void Trial0078 () {
        PerlTrials.trials [78].Execute ();
    }
    [Test] public void Trial0079 () {
        PerlTrials.trials [79].Execute ();
    }
    [Test] public void Trial0080 () {
        PerlTrials.trials [80].Execute ();
    }
    [Test] public void Trial0081 () {
        PerlTrials.trials [81].Execute ();
    }
    [Test] public void Trial0082 () {
        PerlTrials.trials [82].Execute ();
    }
    [Test] public void Trial0083 () {
        PerlTrials.trials [83].Execute ();
    }
    [Test] public void Trial0084 () {
        PerlTrials.trials [84].Execute ();
    }
    [Test] public void Trial0085 () {
        PerlTrials.trials [85].Execute ();
    }
    [Test] public void Trial0086 () {
        PerlTrials.trials [86].Execute ();
    }
    [Test] public void Trial0087 () {
        PerlTrials.trials [87].Execute ();
    }
    [Test] public void Trial0088 () {
        PerlTrials.trials [88].Execute ();
    }
    [Test] public void Trial0089 () {
        PerlTrials.trials [89].Execute ();
    }
    [Test] public void Trial0090 () {
        PerlTrials.trials [90].Execute ();
    }
    [Test] public void Trial0091 () {
        PerlTrials.trials [91].Execute ();
    }
    [Test] public void Trial0092 () {
        PerlTrials.trials [92].Execute ();
    }
    [Test] public void Trial0093 () {
        PerlTrials.trials [93].Execute ();
    }
    [Test] public void Trial0094 () {
        PerlTrials.trials [94].Execute ();
    }
    [Test] public void Trial0095 () {
        PerlTrials.trials [95].Execute ();
    }
    [Test] public void Trial0096 () {
        PerlTrials.trials [96].Execute ();
    }
    [Test] public void Trial0097 () {
        PerlTrials.trials [97].Execute ();
    }
    [Test] public void Trial0098 () {
        PerlTrials.trials [98].Execute ();
    }
    [Test] public void Trial0099 () {
        PerlTrials.trials [99].Execute ();
    }
    [Test] public void Trial0100 () {
        PerlTrials.trials [100].Execute ();
    }
    [Test] public void Trial0101 () {
        PerlTrials.trials [101].Execute ();
    }
    [Test] public void Trial0102 () {
        PerlTrials.trials [102].Execute ();
    }
    [Test] public void Trial0103 () {
        PerlTrials.trials [103].Execute ();
    }
    [Test] public void Trial0104 () {
        PerlTrials.trials [104].Execute ();
    }
    [Test] public void Trial0105 () {
        PerlTrials.trials [105].Execute ();
    }
    [Test] public void Trial0106 () {
        PerlTrials.trials [106].Execute ();
    }
    [Test] public void Trial0107 () {
        PerlTrials.trials [107].Execute ();
    }
    [Test] public void Trial0108 () {
        PerlTrials.trials [108].Execute ();
    }
    [Test] public void Trial0109 () {
        PerlTrials.trials [109].Execute ();
    }
    [Test] public void Trial0110 () {
        PerlTrials.trials [110].Execute ();
    }
    [Test] public void Trial0111 () {
        PerlTrials.trials [111].Execute ();
    }
    [Test] public void Trial0112 () {
        PerlTrials.trials [112].Execute ();
    }
    [Test] public void Trial0113 () {
        PerlTrials.trials [113].Execute ();
    }
    [Test] public void Trial0114 () {
        PerlTrials.trials [114].Execute ();
    }
    [Test] public void Trial0115 () {
        PerlTrials.trials [115].Execute ();
    }
    [Test] public void Trial0116 () {
        PerlTrials.trials [116].Execute ();
    }
    [Test] public void Trial0117 () {
        PerlTrials.trials [117].Execute ();
    }
    [Test] public void Trial0118 () {
        PerlTrials.trials [118].Execute ();
    }
    [Test] public void Trial0119 () {
        PerlTrials.trials [119].Execute ();
    }
    [Test] public void Trial0120 () {
        PerlTrials.trials [120].Execute ();
    }
    [Test] public void Trial0121 () {
        PerlTrials.trials [121].Execute ();
    }
    [Test] public void Trial0122 () {
        PerlTrials.trials [122].Execute ();
    }
    [Test] public void Trial0123 () {
        PerlTrials.trials [123].Execute ();
    }
    [Test] public void Trial0124 () {
        PerlTrials.trials [124].Execute ();
    }
    [Test] public void Trial0125 () {
        PerlTrials.trials [125].Execute ();
    }
    [Test] public void Trial0126 () {
        PerlTrials.trials [126].Execute ();
    }
    [Test] public void Trial0127 () {
        PerlTrials.trials [127].Execute ();
    }
    [Test] public void Trial0128 () {
        PerlTrials.trials [128].Execute ();
    }
    [Test] public void Trial0129 () {
        PerlTrials.trials [129].Execute ();
    }
    [Test] public void Trial0130 () {
        PerlTrials.trials [130].Execute ();
    }
    [Test] public void Trial0131 () {
        PerlTrials.trials [131].Execute ();
    }
    [Test] public void Trial0132 () {
        PerlTrials.trials [132].Execute ();
    }
    [Test] public void Trial0133 () {
        PerlTrials.trials [133].Execute ();
    }
    [Test] public void Trial0134 () {
        PerlTrials.trials [134].Execute ();
    }
    [Test] public void Trial0135 () {
        PerlTrials.trials [135].Execute ();
    }
    [Test] public void Trial0136 () {
        PerlTrials.trials [136].Execute ();
    }
    [Test] public void Trial0137 () {
        PerlTrials.trials [137].Execute ();
    }
    [Test] public void Trial0138 () {
        PerlTrials.trials [138].Execute ();
    }
    [Test] public void Trial0139 () {
        PerlTrials.trials [139].Execute ();
    }
    [Test] public void Trial0140 () {
        PerlTrials.trials [140].Execute ();
    }
    [Test] public void Trial0141 () {
        PerlTrials.trials [141].Execute ();
    }
    [Test] public void Trial0142 () {
        PerlTrials.trials [142].Execute ();
    }
    [Test] public void Trial0143 () {
        PerlTrials.trials [143].Execute ();
    }
    [Test] public void Trial0144 () {
        PerlTrials.trials [144].Execute ();
    }
    [Test] public void Trial0145 () {
        PerlTrials.trials [145].Execute ();
    }
    [Test] public void Trial0146 () {
        PerlTrials.trials [146].Execute ();
    }
    [Test] public void Trial0147 () {
        PerlTrials.trials [147].Execute ();
    }
    [Test] public void Trial0148 () {
        PerlTrials.trials [148].Execute ();
    }
    [Test] public void Trial0149 () {
        PerlTrials.trials [149].Execute ();
    }
    [Test] public void Trial0150 () {
        PerlTrials.trials [150].Execute ();
    }
    [Test] public void Trial0151 () {
        PerlTrials.trials [151].Execute ();
    }
    [Test] public void Trial0152 () {
        PerlTrials.trials [152].Execute ();
    }
    [Test] public void Trial0153 () {
        PerlTrials.trials [153].Execute ();
    }
    [Test] public void Trial0154 () {
        PerlTrials.trials [154].Execute ();
    }
    [Test] public void Trial0155 () {
        PerlTrials.trials [155].Execute ();
    }
    [Test] public void Trial0156 () {
        PerlTrials.trials [156].Execute ();
    }
    [Test] public void Trial0157 () {
        PerlTrials.trials [157].Execute ();
    }
    [Test] public void Trial0158 () {
        PerlTrials.trials [158].Execute ();
    }
    [Test] public void Trial0159 () {
        PerlTrials.trials [159].Execute ();
    }
    [Test] public void Trial0160 () {
        PerlTrials.trials [160].Execute ();
    }
    [Test] public void Trial0161 () {
        PerlTrials.trials [161].Execute ();
    }
    [Test] public void Trial0162 () {
        PerlTrials.trials [162].Execute ();
    }
    [Test] public void Trial0163 () {
        PerlTrials.trials [163].Execute ();
    }
    [Test] public void Trial0164 () {
        PerlTrials.trials [164].Execute ();
    }
    [Test] public void Trial0165 () {
        PerlTrials.trials [165].Execute ();
    }
    [Test] public void Trial0166 () {
        PerlTrials.trials [166].Execute ();
    }
    [Test] public void Trial0167 () {
        PerlTrials.trials [167].Execute ();
    }
    [Test] public void Trial0168 () {
        PerlTrials.trials [168].Execute ();
    }
    [Test] public void Trial0169 () {
        PerlTrials.trials [169].Execute ();
    }
    [Test] public void Trial0170 () {
        PerlTrials.trials [170].Execute ();
    }
    [Test] public void Trial0171 () {
        PerlTrials.trials [171].Execute ();
    }
    [Test] public void Trial0172 () {
        PerlTrials.trials [172].Execute ();
    }
    [Test] public void Trial0173 () {
        PerlTrials.trials [173].Execute ();
    }
    [Test] public void Trial0174 () {
        PerlTrials.trials [174].Execute ();
    }
    [Test] public void Trial0175 () {
        PerlTrials.trials [175].Execute ();
    }
    [Test] public void Trial0176 () {
        PerlTrials.trials [176].Execute ();
    }
    [Test] public void Trial0177 () {
        PerlTrials.trials [177].Execute ();
    }
    [Test] public void Trial0178 () {
        PerlTrials.trials [178].Execute ();
    }
    [Test] public void Trial0179 () {
        PerlTrials.trials [179].Execute ();
    }
    [Test] public void Trial0180 () {
        PerlTrials.trials [180].Execute ();
    }
    [Test] public void Trial0181 () {
        PerlTrials.trials [181].Execute ();
    }
    [Test] public void Trial0182 () {
        PerlTrials.trials [182].Execute ();
    }
    [Test] public void Trial0183 () {
        PerlTrials.trials [183].Execute ();
    }
    [Test] public void Trial0184 () {
        PerlTrials.trials [184].Execute ();
    }
    [Test] public void Trial0185 () {
        PerlTrials.trials [185].Execute ();
    }
    [Test] public void Trial0186 () {
        PerlTrials.trials [186].Execute ();
    }
    [Test] public void Trial0187 () {
        PerlTrials.trials [187].Execute ();
    }
    [Test] public void Trial0188 () {
        PerlTrials.trials [188].Execute ();
    }
    [Test] public void Trial0189 () {
        PerlTrials.trials [189].Execute ();
    }
    [Test] public void Trial0190 () {
        PerlTrials.trials [190].Execute ();
    }
    [Test] public void Trial0191 () {
        PerlTrials.trials [191].Execute ();
    }
    [Test] public void Trial0192 () {
        PerlTrials.trials [192].Execute ();
    }
    [Test] public void Trial0193 () {
        PerlTrials.trials [193].Execute ();
    }
    [Test] public void Trial0194 () {
        PerlTrials.trials [194].Execute ();
    }
    [Test] public void Trial0195 () {
        PerlTrials.trials [195].Execute ();
    }
    [Test] public void Trial0196 () {
        PerlTrials.trials [196].Execute ();
    }
    [Test] public void Trial0197 () {
        PerlTrials.trials [197].Execute ();
    }
    [Test] public void Trial0198 () {
        PerlTrials.trials [198].Execute ();
    }
    [Test] public void Trial0199 () {
        PerlTrials.trials [199].Execute ();
    }
    [Test] public void Trial0200 () {
        PerlTrials.trials [200].Execute ();
    }
    [Test] public void Trial0201 () {
        PerlTrials.trials [201].Execute ();
    }
    [Test] public void Trial0202 () {
        PerlTrials.trials [202].Execute ();
    }
    [Test] public void Trial0203 () {
        PerlTrials.trials [203].Execute ();
    }
    [Test] public void Trial0204 () {
        PerlTrials.trials [204].Execute ();
    }
    [Test] public void Trial0205 () {
        PerlTrials.trials [205].Execute ();
    }
    [Test] public void Trial0206 () {
        PerlTrials.trials [206].Execute ();
    }
    [Test] public void Trial0207 () {
        PerlTrials.trials [207].Execute ();
    }
    [Test] public void Trial0208 () {
        PerlTrials.trials [208].Execute ();
    }
    [Test] public void Trial0209 () {
        PerlTrials.trials [209].Execute ();
    }
    [Test] public void Trial0210 () {
        PerlTrials.trials [210].Execute ();
    }
    [Test] public void Trial0211 () {
        PerlTrials.trials [211].Execute ();
    }
    [Test] public void Trial0212 () {
        PerlTrials.trials [212].Execute ();
    }
    [Test] public void Trial0213 () {
        PerlTrials.trials [213].Execute ();
    }
    [Test] public void Trial0214 () {
        PerlTrials.trials [214].Execute ();
    }
    [Test] public void Trial0215 () {
        PerlTrials.trials [215].Execute ();
    }
    [Test] public void Trial0216 () {
        PerlTrials.trials [216].Execute ();
    }
    [Test] public void Trial0217 () {
        PerlTrials.trials [217].Execute ();
    }
    [Test] public void Trial0218 () {
        PerlTrials.trials [218].Execute ();
    }
    [Test] public void Trial0219 () {
        PerlTrials.trials [219].Execute ();
    }
    [Test] public void Trial0220 () {
        PerlTrials.trials [220].Execute ();
    }
    [Test] public void Trial0221 () {
        PerlTrials.trials [221].Execute ();
    }
    [Test] public void Trial0222 () {
        PerlTrials.trials [222].Execute ();
    }
    [Test] public void Trial0223 () {
        PerlTrials.trials [223].Execute ();
    }
    [Test] public void Trial0224 () {
        PerlTrials.trials [224].Execute ();
    }
    [Test] public void Trial0225 () {
        PerlTrials.trials [225].Execute ();
    }
    [Test] public void Trial0226 () {
        PerlTrials.trials [226].Execute ();
    }
    [Test] public void Trial0227 () {
        PerlTrials.trials [227].Execute ();
    }
    [Test] public void Trial0228 () {
        PerlTrials.trials [228].Execute ();
    }
    [Test] public void Trial0229 () {
        PerlTrials.trials [229].Execute ();
    }
    [Test] public void Trial0230 () {
        PerlTrials.trials [230].Execute ();
    }
    [Test] public void Trial0231 () {
        PerlTrials.trials [231].Execute ();
    }
    [Test] public void Trial0232 () {
        PerlTrials.trials [232].Execute ();
    }
    [Test] public void Trial0233 () {
        PerlTrials.trials [233].Execute ();
    }
    [Test] public void Trial0234 () {
        PerlTrials.trials [234].Execute ();
    }
    [Test] public void Trial0235 () {
        PerlTrials.trials [235].Execute ();
    }
    [Test] public void Trial0236 () {
        PerlTrials.trials [236].Execute ();
    }
    [Test] public void Trial0237 () {
        PerlTrials.trials [237].Execute ();
    }
    [Test] public void Trial0238 () {
        PerlTrials.trials [238].Execute ();
    }
    [Test] public void Trial0239 () {
        PerlTrials.trials [239].Execute ();
    }
    [Test] public void Trial0240 () {
        PerlTrials.trials [240].Execute ();
    }
    [Test] public void Trial0241 () {
        PerlTrials.trials [241].Execute ();
    }
    [Test] public void Trial0242 () {
        PerlTrials.trials [242].Execute ();
    }
    [Test] public void Trial0243 () {
        PerlTrials.trials [243].Execute ();
    }
    [Test] public void Trial0244 () {
        PerlTrials.trials [244].Execute ();
    }
    [Test] public void Trial0245 () {
        PerlTrials.trials [245].Execute ();
    }
    [Test] public void Trial0246 () {
        PerlTrials.trials [246].Execute ();
    }
    [Test] public void Trial0247 () {
        PerlTrials.trials [247].Execute ();
    }
    [Test] public void Trial0248 () {
        PerlTrials.trials [248].Execute ();
    }
    [Test] public void Trial0249 () {
        PerlTrials.trials [249].Execute ();
    }
    [Test] public void Trial0250 () {
        PerlTrials.trials [250].Execute ();
    }
    [Test] public void Trial0251 () {
        PerlTrials.trials [251].Execute ();
    }
    [Test] public void Trial0252 () {
        PerlTrials.trials [252].Execute ();
    }
    [Test] public void Trial0253 () {
        PerlTrials.trials [253].Execute ();
    }
    [Test] public void Trial0254 () {
        PerlTrials.trials [254].Execute ();
    }
    [Test] public void Trial0255 () {
        PerlTrials.trials [255].Execute ();
    }
    [Test] public void Trial0256 () {
        PerlTrials.trials [256].Execute ();
    }
    [Test] public void Trial0257 () {
        PerlTrials.trials [257].Execute ();
    }
    [Test] public void Trial0258 () {
        PerlTrials.trials [258].Execute ();
    }
    [Test] public void Trial0259 () {
        PerlTrials.trials [259].Execute ();
    }
    [Test] public void Trial0260 () {
        PerlTrials.trials [260].Execute ();
    }
    [Test] public void Trial0261 () {
        PerlTrials.trials [261].Execute ();
    }
    [Test] public void Trial0262 () {
        PerlTrials.trials [262].Execute ();
    }
    [Test] public void Trial0263 () {
        PerlTrials.trials [263].Execute ();
    }
    [Test] public void Trial0264 () {
        PerlTrials.trials [264].Execute ();
    }
    [Test] public void Trial0265 () {
        PerlTrials.trials [265].Execute ();
    }
    [Test] public void Trial0266 () {
        PerlTrials.trials [266].Execute ();
    }
    [Test] public void Trial0267 () {
        PerlTrials.trials [267].Execute ();
    }
    [Test] public void Trial0268 () {
        PerlTrials.trials [268].Execute ();
    }
    [Test] public void Trial0269 () {
        PerlTrials.trials [269].Execute ();
    }
    [Test] public void Trial0270 () {
        PerlTrials.trials [270].Execute ();
    }
    [Test] public void Trial0271 () {
        PerlTrials.trials [271].Execute ();
    }
    [Test] public void Trial0272 () {
        PerlTrials.trials [272].Execute ();
    }
    [Test] public void Trial0273 () {
        PerlTrials.trials [273].Execute ();
    }
    [Test] public void Trial0274 () {
        PerlTrials.trials [274].Execute ();
    }
    [Test] public void Trial0275 () {
        PerlTrials.trials [275].Execute ();
    }
    [Test] public void Trial0276 () {
        PerlTrials.trials [276].Execute ();
    }
    [Test] public void Trial0277 () {
        PerlTrials.trials [277].Execute ();
    }
    [Test] public void Trial0278 () {
        PerlTrials.trials [278].Execute ();
    }
    [Test] public void Trial0279 () {
        PerlTrials.trials [279].Execute ();
    }
    [Test] public void Trial0280 () {
        PerlTrials.trials [280].Execute ();
    }
    [Test] public void Trial0281 () {
        PerlTrials.trials [281].Execute ();
    }
    [Test] public void Trial0282 () {
        PerlTrials.trials [282].Execute ();
    }
    [Test] public void Trial0283 () {
        PerlTrials.trials [283].Execute ();
    }
    [Test] public void Trial0284 () {
        PerlTrials.trials [284].Execute ();
    }
    [Test] public void Trial0285 () {
        PerlTrials.trials [285].Execute ();
    }
    [Test] public void Trial0286 () {
        PerlTrials.trials [286].Execute ();
    }
    [Test] public void Trial0287 () {
        PerlTrials.trials [287].Execute ();
    }
    [Test] public void Trial0288 () {
        PerlTrials.trials [288].Execute ();
    }
    [Test] public void Trial0289 () {
        PerlTrials.trials [289].Execute ();
    }
    [Test] public void Trial0290 () {
        PerlTrials.trials [290].Execute ();
    }
    [Test] public void Trial0291 () {
        PerlTrials.trials [291].Execute ();
    }
    [Test] public void Trial0292 () {
        PerlTrials.trials [292].Execute ();
    }
    [Test] public void Trial0293 () {
        PerlTrials.trials [293].Execute ();
    }
    [Test] public void Trial0294 () {
        PerlTrials.trials [294].Execute ();
    }
    [Test] public void Trial0295 () {
        PerlTrials.trials [295].Execute ();
    }
    [Test] public void Trial0296 () {
        PerlTrials.trials [296].Execute ();
    }
    [Test] public void Trial0297 () {
        PerlTrials.trials [297].Execute ();
    }
    [Test] public void Trial0298 () {
        PerlTrials.trials [298].Execute ();
    }
    [Test] public void Trial0299 () {
        PerlTrials.trials [299].Execute ();
    }
    [Test] public void Trial0300 () {
        PerlTrials.trials [300].Execute ();
    }
    [Test] public void Trial0301 () {
        PerlTrials.trials [301].Execute ();
    }
    [Test] public void Trial0302 () {
        PerlTrials.trials [302].Execute ();
    }
    [Test] public void Trial0303 () {
        PerlTrials.trials [303].Execute ();
    }
    [Test] public void Trial0304 () {
        PerlTrials.trials [304].Execute ();
    }
    [Test] public void Trial0305 () {
        PerlTrials.trials [305].Execute ();
    }
    [Test] public void Trial0306 () {
        PerlTrials.trials [306].Execute ();
    }
    [Test] public void Trial0307 () {
        PerlTrials.trials [307].Execute ();
    }
    [Test] public void Trial0308 () {
        PerlTrials.trials [308].Execute ();
    }
    [Test] public void Trial0309 () {
        PerlTrials.trials [309].Execute ();
    }
    [Test] public void Trial0310 () {
        PerlTrials.trials [310].Execute ();
    }
    [Test] public void Trial0311 () {
        PerlTrials.trials [311].Execute ();
    }
    [Test] public void Trial0312 () {
        PerlTrials.trials [312].Execute ();
    }
    [Test] public void Trial0313 () {
        PerlTrials.trials [313].Execute ();
    }
    [Test] public void Trial0314 () {
        PerlTrials.trials [314].Execute ();
    }
    [Test] public void Trial0315 () {
        PerlTrials.trials [315].Execute ();
    }
    [Test] public void Trial0316 () {
        PerlTrials.trials [316].Execute ();
    }
    [Test] public void Trial0317 () {
        PerlTrials.trials [317].Execute ();
    }
    [Test] public void Trial0318 () {
        PerlTrials.trials [318].Execute ();
    }
    [Test] public void Trial0319 () {
        PerlTrials.trials [319].Execute ();
    }
    [Test] public void Trial0320 () {
        PerlTrials.trials [320].Execute ();
    }
    [Test] public void Trial0321 () {
        PerlTrials.trials [321].Execute ();
    }
    [Test] public void Trial0322 () {
        PerlTrials.trials [322].Execute ();
    }
    [Test] public void Trial0323 () {
        PerlTrials.trials [323].Execute ();
    }
    [Test] public void Trial0324 () {
        PerlTrials.trials [324].Execute ();
    }
    [Test] public void Trial0325 () {
        PerlTrials.trials [325].Execute ();
    }
    [Test] public void Trial0326 () {
        PerlTrials.trials [326].Execute ();
    }
    [Test] public void Trial0327 () {
        PerlTrials.trials [327].Execute ();
    }
    [Test] public void Trial0328 () {
        PerlTrials.trials [328].Execute ();
    }
    [Test] public void Trial0329 () {
        PerlTrials.trials [329].Execute ();
    }
    [Test] public void Trial0330 () {
        PerlTrials.trials [330].Execute ();
    }
    [Test] public void Trial0331 () {
        PerlTrials.trials [331].Execute ();
    }
    [Test] public void Trial0332 () {
        PerlTrials.trials [332].Execute ();
    }
    [Test] public void Trial0333 () {
        PerlTrials.trials [333].Execute ();
    }
    [Test] public void Trial0334 () {
        PerlTrials.trials [334].Execute ();
    }
    [Test] public void Trial0335 () {
        PerlTrials.trials [335].Execute ();
    }
    [Test] public void Trial0336 () {
        PerlTrials.trials [336].Execute ();
    }
    [Test] public void Trial0337 () {
        PerlTrials.trials [337].Execute ();
    }
    [Test] public void Trial0338 () {
        PerlTrials.trials [338].Execute ();
    }
    [Test] public void Trial0339 () {
        PerlTrials.trials [339].Execute ();
    }
    [Test] public void Trial0340 () {
        PerlTrials.trials [340].Execute ();
    }
    [Test] public void Trial0341 () {
        PerlTrials.trials [341].Execute ();
    }
    [Test] public void Trial0342 () {
        PerlTrials.trials [342].Execute ();
    }
    [Test] public void Trial0343 () {
        PerlTrials.trials [343].Execute ();
    }
    [Test] public void Trial0344 () {
        PerlTrials.trials [344].Execute ();
    }
    [Test] public void Trial0345 () {
        PerlTrials.trials [345].Execute ();
    }
    [Test] public void Trial0346 () {
        PerlTrials.trials [346].Execute ();
    }
    [Test] public void Trial0347 () {
        PerlTrials.trials [347].Execute ();
    }
    [Test] public void Trial0348 () {
        PerlTrials.trials [348].Execute ();
    }
    [Test] public void Trial0349 () {
        PerlTrials.trials [349].Execute ();
    }
    [Test] public void Trial0350 () {
        PerlTrials.trials [350].Execute ();
    }
    [Test] public void Trial0351 () {
        PerlTrials.trials [351].Execute ();
    }
    [Test] public void Trial0352 () {
        PerlTrials.trials [352].Execute ();
    }
    [Test] public void Trial0353 () {
        PerlTrials.trials [353].Execute ();
    }
    [Test] public void Trial0354 () {
        PerlTrials.trials [354].Execute ();
    }
    [Test] public void Trial0355 () {
        PerlTrials.trials [355].Execute ();
    }
    [Test] public void Trial0356 () {
        PerlTrials.trials [356].Execute ();
    }
    [Test] public void Trial0357 () {
        PerlTrials.trials [357].Execute ();
    }
    [Test] public void Trial0358 () {
        PerlTrials.trials [358].Execute ();
    }
    [Test] public void Trial0359 () {
        PerlTrials.trials [359].Execute ();
    }
    [Test] public void Trial0360 () {
        PerlTrials.trials [360].Execute ();
    }
    [Test] public void Trial0361 () {
        PerlTrials.trials [361].Execute ();
    }
    [Test] public void Trial0362 () {
        PerlTrials.trials [362].Execute ();
    }
    [Test] public void Trial0363 () {
        PerlTrials.trials [363].Execute ();
    }
    [Test] public void Trial0364 () {
        PerlTrials.trials [364].Execute ();
    }
    [Test] public void Trial0365 () {
        PerlTrials.trials [365].Execute ();
    }
    [Test] public void Trial0366 () {
        PerlTrials.trials [366].Execute ();
    }
    [Test] public void Trial0367 () {
        PerlTrials.trials [367].Execute ();
    }
    [Test] public void Trial0368 () {
        PerlTrials.trials [368].Execute ();
    }
    [Test] public void Trial0369 () {
        PerlTrials.trials [369].Execute ();
    }
    [Test] public void Trial0370 () {
        PerlTrials.trials [370].Execute ();
    }
    [Test] public void Trial0371 () {
        PerlTrials.trials [371].Execute ();
    }
    [Test] public void Trial0372 () {
        PerlTrials.trials [372].Execute ();
    }
    [Test] public void Trial0373 () {
        PerlTrials.trials [373].Execute ();
    }
    [Test] public void Trial0374 () {
        PerlTrials.trials [374].Execute ();
    }
    [Test] public void Trial0375 () {
        PerlTrials.trials [375].Execute ();
    }
    [Test] public void Trial0376 () {
        PerlTrials.trials [376].Execute ();
    }
    [Test] public void Trial0377 () {
        PerlTrials.trials [377].Execute ();
    }
    [Test] public void Trial0378 () {
        PerlTrials.trials [378].Execute ();
    }
    [Test] public void Trial0379 () {
        PerlTrials.trials [379].Execute ();
    }
    [Test] public void Trial0380 () {
        PerlTrials.trials [380].Execute ();
    }
    [Test] public void Trial0381 () {
        PerlTrials.trials [381].Execute ();
    }
    [Test] public void Trial0382 () {
        PerlTrials.trials [382].Execute ();
    }
    [Test] public void Trial0383 () {
        PerlTrials.trials [383].Execute ();
    }
    [Test] public void Trial0384 () {
        PerlTrials.trials [384].Execute ();
    }
    [Test] public void Trial0385 () {
        PerlTrials.trials [385].Execute ();
    }
    [Test] public void Trial0386 () {
        PerlTrials.trials [386].Execute ();
    }
    [Test] public void Trial0387 () {
        PerlTrials.trials [387].Execute ();
    }
    [Test] public void Trial0388 () {
        PerlTrials.trials [388].Execute ();
    }
    [Test] public void Trial0389 () {
        PerlTrials.trials [389].Execute ();
    }
    [Test] public void Trial0390 () {
        PerlTrials.trials [390].Execute ();
    }
    [Test] public void Trial0391 () {
        PerlTrials.trials [391].Execute ();
    }
    [Test] public void Trial0392 () {
        PerlTrials.trials [392].Execute ();
    }
    [Test] public void Trial0393 () {
        PerlTrials.trials [393].Execute ();
    }
    [Test] public void Trial0394 () {
        PerlTrials.trials [394].Execute ();
    }
    [Test] public void Trial0395 () {
        PerlTrials.trials [395].Execute ();
    }
    [Test] public void Trial0396 () {
        PerlTrials.trials [396].Execute ();
    }
    [Test] public void Trial0397 () {
        PerlTrials.trials [397].Execute ();
    }
    [Test] public void Trial0398 () {
        PerlTrials.trials [398].Execute ();
    }
    [Test] public void Trial0399 () {
        PerlTrials.trials [399].Execute ();
    }
    [Test] public void Trial0400 () {
        PerlTrials.trials [400].Execute ();
    }
    [Test] public void Trial0401 () {
        PerlTrials.trials [401].Execute ();
    }
    [Test] public void Trial0402 () {
        PerlTrials.trials [402].Execute ();
    }
    [Test] public void Trial0403 () {
        PerlTrials.trials [403].Execute ();
    }
    [Test] public void Trial0404 () {
        PerlTrials.trials [404].Execute ();
    }
    [Test] public void Trial0405 () {
        PerlTrials.trials [405].Execute ();
    }
    [Test] public void Trial0406 () {
        PerlTrials.trials [406].Execute ();
    }
    [Test] public void Trial0407 () {
        PerlTrials.trials [407].Execute ();
    }
    [Test] public void Trial0408 () {
        PerlTrials.trials [408].Execute ();
    }
    [Test] public void Trial0409 () {
        PerlTrials.trials [409].Execute ();
    }
    [Test] public void Trial0410 () {
        PerlTrials.trials [410].Execute ();
    }
    [Test] public void Trial0411 () {
        PerlTrials.trials [411].Execute ();
    }
    [Test] public void Trial0412 () {
        PerlTrials.trials [412].Execute ();
    }
    [Test] public void Trial0413 () {
        PerlTrials.trials [413].Execute ();
    }
    [Test] public void Trial0414 () {
        PerlTrials.trials [414].Execute ();
    }
    [Test] public void Trial0415 () {
        PerlTrials.trials [415].Execute ();
    }
    [Test] public void Trial0416 () {
        PerlTrials.trials [416].Execute ();
    }
    [Test] public void Trial0417 () {
        PerlTrials.trials [417].Execute ();
    }
    [Test] public void Trial0418 () {
        PerlTrials.trials [418].Execute ();
    }
    [Test] public void Trial0419 () {
        PerlTrials.trials [419].Execute ();
    }
    [Test] public void Trial0420 () {
        PerlTrials.trials [420].Execute ();
    }
    [Test] public void Trial0421 () {
        PerlTrials.trials [421].Execute ();
    }
    [Test] public void Trial0422 () {
        PerlTrials.trials [422].Execute ();
    }
    [Test] public void Trial0423 () {
        PerlTrials.trials [423].Execute ();
    }
    [Test] public void Trial0424 () {
        PerlTrials.trials [424].Execute ();
    }
    [Test] public void Trial0425 () {
        PerlTrials.trials [425].Execute ();
    }
    [Test] public void Trial0426 () {
        PerlTrials.trials [426].Execute ();
    }
    [Test] public void Trial0427 () {
        PerlTrials.trials [427].Execute ();
    }
    [Test] public void Trial0428 () {
        PerlTrials.trials [428].Execute ();
    }
    [Test] public void Trial0429 () {
        PerlTrials.trials [429].Execute ();
    }
    [Test] public void Trial0430 () {
        PerlTrials.trials [430].Execute ();
    }
    [Test] public void Trial0431 () {
        PerlTrials.trials [431].Execute ();
    }
    [Test] public void Trial0432 () {
        PerlTrials.trials [432].Execute ();
    }
    [Test] public void Trial0433 () {
        PerlTrials.trials [433].Execute ();
    }
    [Test] public void Trial0434 () {
        PerlTrials.trials [434].Execute ();
    }
    [Test] public void Trial0435 () {
        PerlTrials.trials [435].Execute ();
    }
    [Test] public void Trial0436 () {
        PerlTrials.trials [436].Execute ();
    }
    [Test] public void Trial0437 () {
        PerlTrials.trials [437].Execute ();
    }
    [Test] public void Trial0438 () {
        PerlTrials.trials [438].Execute ();
    }
    [Test] public void Trial0439 () {
        PerlTrials.trials [439].Execute ();
    }
    [Test] public void Trial0440 () {
        PerlTrials.trials [440].Execute ();
    }
    [Test] public void Trial0441 () {
        PerlTrials.trials [441].Execute ();
    }
    [Test] public void Trial0442 () {
        PerlTrials.trials [442].Execute ();
    }
    [Test] public void Trial0443 () {
        PerlTrials.trials [443].Execute ();
    }
    [Test] public void Trial0444 () {
        PerlTrials.trials [444].Execute ();
    }
    [Test] public void Trial0445 () {
        PerlTrials.trials [445].Execute ();
    }
    [Test] public void Trial0446 () {
        PerlTrials.trials [446].Execute ();
    }
    [Test] public void Trial0447 () {
        PerlTrials.trials [447].Execute ();
    }
    [Test] public void Trial0448 () {
        PerlTrials.trials [448].Execute ();
    }
    [Test] public void Trial0449 () {
        PerlTrials.trials [449].Execute ();
    }
    [Test] public void Trial0450 () {
        PerlTrials.trials [450].Execute ();
    }
    [Test] public void Trial0451 () {
        PerlTrials.trials [451].Execute ();
    }
    [Test] public void Trial0452 () {
        PerlTrials.trials [452].Execute ();
    }
    [Test] public void Trial0453 () {
        PerlTrials.trials [453].Execute ();
    }
    [Test] public void Trial0454 () {
        PerlTrials.trials [454].Execute ();
    }
    [Test] public void Trial0455 () {
        PerlTrials.trials [455].Execute ();
    }
    [Test] public void Trial0456 () {
        PerlTrials.trials [456].Execute ();
    }
    [Test] public void Trial0457 () {
        PerlTrials.trials [457].Execute ();
    }
    [Test] public void Trial0458 () {
        PerlTrials.trials [458].Execute ();
    }
    [Test] public void Trial0459 () {
        PerlTrials.trials [459].Execute ();
    }
    [Test] public void Trial0460 () {
        PerlTrials.trials [460].Execute ();
    }
    [Test] public void Trial0461 () {
        PerlTrials.trials [461].Execute ();
    }
    [Test] public void Trial0462 () {
        PerlTrials.trials [462].Execute ();
    }
    [Test] public void Trial0463 () {
        PerlTrials.trials [463].Execute ();
    }
    [Test] public void Trial0464 () {
        PerlTrials.trials [464].Execute ();
    }
    [Test] public void Trial0465 () {
        PerlTrials.trials [465].Execute ();
    }
    [Test] public void Trial0466 () {
        PerlTrials.trials [466].Execute ();
    }
    [Test] public void Trial0467 () {
        PerlTrials.trials [467].Execute ();
    }
    [Test] public void Trial0468 () {
        PerlTrials.trials [468].Execute ();
    }
    [Test] public void Trial0469 () {
        PerlTrials.trials [469].Execute ();
    }
    [Test] public void Trial0470 () {
        PerlTrials.trials [470].Execute ();
    }
    [Test] public void Trial0471 () {
        PerlTrials.trials [471].Execute ();
    }
    [Test] public void Trial0472 () {
        PerlTrials.trials [472].Execute ();
    }
    [Test] public void Trial0473 () {
        PerlTrials.trials [473].Execute ();
    }
    [Test] public void Trial0474 () {
        PerlTrials.trials [474].Execute ();
    }
    [Test] public void Trial0475 () {
        PerlTrials.trials [475].Execute ();
    }
    [Test] public void Trial0476 () {
        PerlTrials.trials [476].Execute ();
    }
    [Test] public void Trial0477 () {
        PerlTrials.trials [477].Execute ();
    }
    [Test] public void Trial0478 () {
        PerlTrials.trials [478].Execute ();
    }
    [Test] public void Trial0479 () {
        PerlTrials.trials [479].Execute ();
    }
    [Test] public void Trial0480 () {
        PerlTrials.trials [480].Execute ();
    }
    [Test] public void Trial0481 () {
        PerlTrials.trials [481].Execute ();
    }
    [Test] public void Trial0482 () {
        PerlTrials.trials [482].Execute ();
    }
    [Test] public void Trial0483 () {
        PerlTrials.trials [483].Execute ();
    }
    [Test] public void Trial0484 () {
        PerlTrials.trials [484].Execute ();
    }
    [Test] public void Trial0485 () {
        PerlTrials.trials [485].Execute ();
    }
    [Test] public void Trial0486 () {
        PerlTrials.trials [486].Execute ();
    }
    [Test] public void Trial0487 () {
        PerlTrials.trials [487].Execute ();
    }
    [Test] public void Trial0488 () {
        PerlTrials.trials [488].Execute ();
    }
    [Test] public void Trial0489 () {
        PerlTrials.trials [489].Execute ();
    }
    [Test] public void Trial0490 () {
        PerlTrials.trials [490].Execute ();
    }
    [Test] public void Trial0491 () {
        PerlTrials.trials [491].Execute ();
    }
    [Test] public void Trial0492 () {
        PerlTrials.trials [492].Execute ();
    }
    [Test] public void Trial0493 () {
        PerlTrials.trials [493].Execute ();
    }
    [Test] public void Trial0494 () {
        PerlTrials.trials [494].Execute ();
    }
    [Test] public void Trial0495 () {
        PerlTrials.trials [495].Execute ();
    }
    [Test] public void Trial0496 () {
        PerlTrials.trials [496].Execute ();
    }
    [Test] public void Trial0497 () {
        PerlTrials.trials [497].Execute ();
    }
    [Test] public void Trial0498 () {
        PerlTrials.trials [498].Execute ();
    }
    [Test] public void Trial0499 () {
        PerlTrials.trials [499].Execute ();
    }
    [Test] public void Trial0500 () {
        PerlTrials.trials [500].Execute ();
    }
    [Test] public void Trial0501 () {
        PerlTrials.trials [501].Execute ();
    }
    [Test] public void Trial0502 () {
        PerlTrials.trials [502].Execute ();
    }
    [Test] public void Trial0503 () {
        PerlTrials.trials [503].Execute ();
    }
    [Test] public void Trial0504 () {
        PerlTrials.trials [504].Execute ();
    }
    [Test] public void Trial0505 () {
        PerlTrials.trials [505].Execute ();
    }
    [Test] public void Trial0506 () {
        PerlTrials.trials [506].Execute ();
    }
    [Test] public void Trial0507 () {
        PerlTrials.trials [507].Execute ();
    }
    [Test] public void Trial0508 () {
        PerlTrials.trials [508].Execute ();
    }
    [Test] public void Trial0509 () {
        PerlTrials.trials [509].Execute ();
    }
    [Test] public void Trial0510 () {
        PerlTrials.trials [510].Execute ();
    }
    [Test] public void Trial0511 () {
        PerlTrials.trials [511].Execute ();
    }
    [Test] public void Trial0512 () {
        PerlTrials.trials [512].Execute ();
    }
    [Test] public void Trial0513 () {
        PerlTrials.trials [513].Execute ();
    }
    [Test] public void Trial0514 () {
        PerlTrials.trials [514].Execute ();
    }
    [Test] public void Trial0515 () {
        PerlTrials.trials [515].Execute ();
    }
    [Test] public void Trial0516 () {
        PerlTrials.trials [516].Execute ();
    }
    [Test] public void Trial0517 () {
        PerlTrials.trials [517].Execute ();
    }
    [Test] public void Trial0518 () {
        PerlTrials.trials [518].Execute ();
    }
    [Test] public void Trial0519 () {
        PerlTrials.trials [519].Execute ();
    }
    [Test] public void Trial0520 () {
        PerlTrials.trials [520].Execute ();
    }
    [Test] public void Trial0521 () {
        PerlTrials.trials [521].Execute ();
    }
    [Test] public void Trial0522 () {
        PerlTrials.trials [522].Execute ();
    }
    [Test] public void Trial0523 () {
        PerlTrials.trials [523].Execute ();
    }
    [Test] public void Trial0524 () {
        PerlTrials.trials [524].Execute ();
    }
    [Test] public void Trial0525 () {
        PerlTrials.trials [525].Execute ();
    }
    [Test] public void Trial0526 () {
        PerlTrials.trials [526].Execute ();
    }
    [Test] public void Trial0527 () {
        PerlTrials.trials [527].Execute ();
    }
    [Test] public void Trial0528 () {
        PerlTrials.trials [528].Execute ();
    }
    [Test] public void Trial0529 () {
        PerlTrials.trials [529].Execute ();
    }
    [Test] public void Trial0530 () {
        PerlTrials.trials [530].Execute ();
    }
    [Test] public void Trial0531 () {
        PerlTrials.trials [531].Execute ();
    }
    [Test] public void Trial0532 () {
        PerlTrials.trials [532].Execute ();
    }
    [Test] public void Trial0533 () {
        PerlTrials.trials [533].Execute ();
    }
    [Test] public void Trial0534 () {
        PerlTrials.trials [534].Execute ();
    }
    [Test] public void Trial0535 () {
        PerlTrials.trials [535].Execute ();
    }
    [Test] public void Trial0536 () {
        PerlTrials.trials [536].Execute ();
    }
    [Test] public void Trial0537 () {
        PerlTrials.trials [537].Execute ();
    }
    [Test] public void Trial0538 () {
        PerlTrials.trials [538].Execute ();
    }
    [Test] public void Trial0539 () {
        PerlTrials.trials [539].Execute ();
    }
    [Test] public void Trial0540 () {
        PerlTrials.trials [540].Execute ();
    }
    [Test] public void Trial0541 () {
        PerlTrials.trials [541].Execute ();
    }
    [Test] public void Trial0542 () {
        PerlTrials.trials [542].Execute ();
    }
    [Test] public void Trial0543 () {
        PerlTrials.trials [543].Execute ();
    }
    [Test] public void Trial0544 () {
        PerlTrials.trials [544].Execute ();
    }
    [Test] public void Trial0545 () {
        PerlTrials.trials [545].Execute ();
    }
    [Test] public void Trial0546 () {
        PerlTrials.trials [546].Execute ();
    }
    [Test] public void Trial0547 () {
        PerlTrials.trials [547].Execute ();
    }
    [Test] public void Trial0548 () {
        PerlTrials.trials [548].Execute ();
    }
    [Test] public void Trial0549 () {
        PerlTrials.trials [549].Execute ();
    }
    [Test] public void Trial0550 () {
        PerlTrials.trials [550].Execute ();
    }
    [Test] public void Trial0551 () {
        PerlTrials.trials [551].Execute ();
    }
    [Test] public void Trial0552 () {
        PerlTrials.trials [552].Execute ();
    }
    [Test] public void Trial0553 () {
        PerlTrials.trials [553].Execute ();
    }
    [Test] public void Trial0554 () {
        PerlTrials.trials [554].Execute ();
    }
    [Test] public void Trial0555 () {
        PerlTrials.trials [555].Execute ();
    }
    [Test] public void Trial0556 () {
        PerlTrials.trials [556].Execute ();
    }
    [Test] public void Trial0557 () {
        PerlTrials.trials [557].Execute ();
    }
    [Test] public void Trial0558 () {
        PerlTrials.trials [558].Execute ();
    }
    [Test] public void Trial0559 () {
        PerlTrials.trials [559].Execute ();
    }
    [Test] public void Trial0560 () {
        PerlTrials.trials [560].Execute ();
    }
    [Test] public void Trial0561 () {
        PerlTrials.trials [561].Execute ();
    }
    [Test] public void Trial0562 () {
        PerlTrials.trials [562].Execute ();
    }
    [Test] public void Trial0563 () {
        PerlTrials.trials [563].Execute ();
    }
    [Test] public void Trial0564 () {
        PerlTrials.trials [564].Execute ();
    }
    [Test] public void Trial0565 () {
        PerlTrials.trials [565].Execute ();
    }
    [Test] public void Trial0566 () {
        PerlTrials.trials [566].Execute ();
    }
    [Test] public void Trial0567 () {
        PerlTrials.trials [567].Execute ();
    }
    [Test] public void Trial0568 () {
        PerlTrials.trials [568].Execute ();
    }
    [Test] public void Trial0569 () {
        PerlTrials.trials [569].Execute ();
    }
    [Test] public void Trial0570 () {
        PerlTrials.trials [570].Execute ();
    }
    [Test] public void Trial0571 () {
        PerlTrials.trials [571].Execute ();
    }
    [Test] public void Trial0572 () {
        PerlTrials.trials [572].Execute ();
    }
    [Test] public void Trial0573 () {
        PerlTrials.trials [573].Execute ();
    }
    [Test] public void Trial0574 () {
        PerlTrials.trials [574].Execute ();
    }
    [Test] public void Trial0575 () {
        PerlTrials.trials [575].Execute ();
    }
    [Test] public void Trial0576 () {
        PerlTrials.trials [576].Execute ();
    }
    [Test] public void Trial0577 () {
        PerlTrials.trials [577].Execute ();
    }
    [Test] public void Trial0578 () {
        PerlTrials.trials [578].Execute ();
    }
    [Test] public void Trial0579 () {
        PerlTrials.trials [579].Execute ();
    }
    [Test] public void Trial0580 () {
        PerlTrials.trials [580].Execute ();
    }
    [Test] public void Trial0581 () {
        PerlTrials.trials [581].Execute ();
    }
    [Test] public void Trial0582 () {
        PerlTrials.trials [582].Execute ();
    }
    [Test] public void Trial0583 () {
        PerlTrials.trials [583].Execute ();
    }
    [Test] public void Trial0584 () {
        PerlTrials.trials [584].Execute ();
    }
    [Test] public void Trial0585 () {
        PerlTrials.trials [585].Execute ();
    }
    [Test] public void Trial0586 () {
        PerlTrials.trials [586].Execute ();
    }
    [Test] public void Trial0587 () {
        PerlTrials.trials [587].Execute ();
    }
    [Test] public void Trial0588 () {
        PerlTrials.trials [588].Execute ();
    }
    [Test] public void Trial0589 () {
        PerlTrials.trials [589].Execute ();
    }
    [Test] public void Trial0590 () {
        PerlTrials.trials [590].Execute ();
    }
    [Test] public void Trial0591 () {
        PerlTrials.trials [591].Execute ();
    }
    [Test] public void Trial0592 () {
        PerlTrials.trials [592].Execute ();
    }
    [Test] public void Trial0593 () {
        PerlTrials.trials [593].Execute ();
    }
    [Test] public void Trial0594 () {
        PerlTrials.trials [594].Execute ();
    }
    [Test] public void Trial0595 () {
        PerlTrials.trials [595].Execute ();
    }
    [Test] public void Trial0596 () {
        PerlTrials.trials [596].Execute ();
    }
    [Test] public void Trial0597 () {
        PerlTrials.trials [597].Execute ();
    }
    [Test] public void Trial0598 () {
        PerlTrials.trials [598].Execute ();
    }
    [Test] public void Trial0599 () {
        PerlTrials.trials [599].Execute ();
    }
    [Test] public void Trial0600 () {
        PerlTrials.trials [600].Execute ();
    }
    [Test] public void Trial0601 () {
        PerlTrials.trials [601].Execute ();
    }
    [Test] public void Trial0602 () {
        PerlTrials.trials [602].Execute ();
    }
    [Test] public void Trial0603 () {
        PerlTrials.trials [603].Execute ();
    }
    [Test] public void Trial0604 () {
        PerlTrials.trials [604].Execute ();
    }
    [Test] public void Trial0605 () {
        PerlTrials.trials [605].Execute ();
    }
    [Test] public void Trial0606 () {
        PerlTrials.trials [606].Execute ();
    }
    [Test] public void Trial0607 () {
        PerlTrials.trials [607].Execute ();
    }
    [Test] public void Trial0608 () {
        PerlTrials.trials [608].Execute ();
    }
    [Test] public void Trial0609 () {
        PerlTrials.trials [609].Execute ();
    }
    [Test] public void Trial0610 () {
        PerlTrials.trials [610].Execute ();
    }
    [Test] public void Trial0611 () {
        PerlTrials.trials [611].Execute ();
    }
    [Test] public void Trial0612 () {
        PerlTrials.trials [612].Execute ();
    }
    [Test] public void Trial0613 () {
        PerlTrials.trials [613].Execute ();
    }
    [Test] public void Trial0614 () {
        PerlTrials.trials [614].Execute ();
    }
    [Test] public void Trial0615 () {
        PerlTrials.trials [615].Execute ();
    }
    [Test] public void Trial0616 () {
        PerlTrials.trials [616].Execute ();
    }
    [Test] public void Trial0617 () {
        PerlTrials.trials [617].Execute ();
    }
    [Test] public void Trial0618 () {
        PerlTrials.trials [618].Execute ();
    }
    [Test] public void Trial0619 () {
        PerlTrials.trials [619].Execute ();
    }
    [Test] public void Trial0620 () {
        PerlTrials.trials [620].Execute ();
    }
    [Test] public void Trial0621 () {
        PerlTrials.trials [621].Execute ();
    }
    [Test] public void Trial0622 () {
        PerlTrials.trials [622].Execute ();
    }
    [Test] public void Trial0623 () {
        PerlTrials.trials [623].Execute ();
    }
    [Test] public void Trial0624 () {
        PerlTrials.trials [624].Execute ();
    }
    [Test] public void Trial0625 () {
        PerlTrials.trials [625].Execute ();
    }
    [Test] public void Trial0626 () {
        PerlTrials.trials [626].Execute ();
    }
    [Test] public void Trial0627 () {
        PerlTrials.trials [627].Execute ();
    }
    [Test] public void Trial0628 () {
        PerlTrials.trials [628].Execute ();
    }
    [Test] public void Trial0629 () {
        PerlTrials.trials [629].Execute ();
    }
    [Test] public void Trial0630 () {
        PerlTrials.trials [630].Execute ();
    }
    [Test] public void Trial0631 () {
        PerlTrials.trials [631].Execute ();
    }
    [Test] public void Trial0632 () {
        PerlTrials.trials [632].Execute ();
    }
    [Test] public void Trial0633 () {
        PerlTrials.trials [633].Execute ();
    }
    [Test] public void Trial0634 () {
        PerlTrials.trials [634].Execute ();
    }
    [Test] public void Trial0635 () {
        PerlTrials.trials [635].Execute ();
    }
    [Test] public void Trial0636 () {
        PerlTrials.trials [636].Execute ();
    }
    [Test] public void Trial0637 () {
        PerlTrials.trials [637].Execute ();
    }
    [Test] public void Trial0638 () {
        PerlTrials.trials [638].Execute ();
    }
    [Test] public void Trial0639 () {
        PerlTrials.trials [639].Execute ();
    }
    [Test] public void Trial0640 () {
        PerlTrials.trials [640].Execute ();
    }
    [Test] public void Trial0641 () {
        PerlTrials.trials [641].Execute ();
    }
    [Test] public void Trial0642 () {
        PerlTrials.trials [642].Execute ();
    }
    [Test] public void Trial0643 () {
        PerlTrials.trials [643].Execute ();
    }
    [Test] public void Trial0644 () {
        PerlTrials.trials [644].Execute ();
    }
    [Test] public void Trial0645 () {
        PerlTrials.trials [645].Execute ();
    }
    [Test] public void Trial0646 () {
        PerlTrials.trials [646].Execute ();
    }
    [Test] public void Trial0647 () {
        PerlTrials.trials [647].Execute ();
    }
    [Test] public void Trial0648 () {
        PerlTrials.trials [648].Execute ();
    }
    [Test] public void Trial0649 () {
        PerlTrials.trials [649].Execute ();
    }
    [Test] public void Trial0650 () {
        PerlTrials.trials [650].Execute ();
    }
    [Test] public void Trial0651 () {
        PerlTrials.trials [651].Execute ();
    }
    [Test] public void Trial0652 () {
        PerlTrials.trials [652].Execute ();
    }
    [Test] public void Trial0653 () {
        PerlTrials.trials [653].Execute ();
    }
    [Test] public void Trial0654 () {
        PerlTrials.trials [654].Execute ();
    }
    [Test] public void Trial0655 () {
        PerlTrials.trials [655].Execute ();
    }
    [Test] public void Trial0656 () {
        PerlTrials.trials [656].Execute ();
    }
    [Test] public void Trial0657 () {
        PerlTrials.trials [657].Execute ();
    }
    [Test] public void Trial0658 () {
        PerlTrials.trials [658].Execute ();
    }
    [Test] public void Trial0659 () {
        PerlTrials.trials [659].Execute ();
    }
    [Test] public void Trial0660 () {
        PerlTrials.trials [660].Execute ();
    }
    [Test] public void Trial0661 () {
        PerlTrials.trials [661].Execute ();
    }
    [Test] public void Trial0662 () {
        PerlTrials.trials [662].Execute ();
    }
    [Test] public void Trial0663 () {
        PerlTrials.trials [663].Execute ();
    }
    [Test] public void Trial0664 () {
        PerlTrials.trials [664].Execute ();
    }
    [Test] public void Trial0665 () {
        PerlTrials.trials [665].Execute ();
    }
    [Test] public void Trial0666 () {
        PerlTrials.trials [666].Execute ();
    }
    [Test] public void Trial0667 () {
        PerlTrials.trials [667].Execute ();
    }
    [Test] public void Trial0668 () {
        PerlTrials.trials [668].Execute ();
    }
    [Test] public void Trial0669 () {
        PerlTrials.trials [669].Execute ();
    }
    [Test] public void Trial0670 () {
        PerlTrials.trials [670].Execute ();
    }
    [Test] public void Trial0671 () {
        PerlTrials.trials [671].Execute ();
    }
    [Test] public void Trial0672 () {
        PerlTrials.trials [672].Execute ();
    }
    [Test] public void Trial0673 () {
        PerlTrials.trials [673].Execute ();
    }
    [Test] public void Trial0674 () {
        PerlTrials.trials [674].Execute ();
    }
    [Test] public void Trial0675 () {
        PerlTrials.trials [675].Execute ();
    }
    [Test] public void Trial0676 () {
        PerlTrials.trials [676].Execute ();
    }
    [Test] public void Trial0677 () {
        PerlTrials.trials [677].Execute ();
    }
    [Test] public void Trial0678 () {
        PerlTrials.trials [678].Execute ();
    }
    [Test] public void Trial0679 () {
        PerlTrials.trials [679].Execute ();
    }
    [Test] public void Trial0680 () {
        PerlTrials.trials [680].Execute ();
    }
    [Test] public void Trial0681 () {
        PerlTrials.trials [681].Execute ();
    }
    [Test] public void Trial0682 () {
        PerlTrials.trials [682].Execute ();
    }
    [Test] public void Trial0683 () {
        PerlTrials.trials [683].Execute ();
    }
    [Test] public void Trial0684 () {
        PerlTrials.trials [684].Execute ();
    }
    [Test] public void Trial0685 () {
        PerlTrials.trials [685].Execute ();
    }
    [Test] public void Trial0686 () {
        PerlTrials.trials [686].Execute ();
    }
    [Test] public void Trial0687 () {
        PerlTrials.trials [687].Execute ();
    }
    [Test] public void Trial0688 () {
        PerlTrials.trials [688].Execute ();
    }
    [Test] public void Trial0689 () {
        PerlTrials.trials [689].Execute ();
    }
    [Test] public void Trial0690 () {
        PerlTrials.trials [690].Execute ();
    }
    [Test] public void Trial0691 () {
        PerlTrials.trials [691].Execute ();
    }
    [Test] public void Trial0692 () {
        PerlTrials.trials [692].Execute ();
    }
    [Test] public void Trial0693 () {
        PerlTrials.trials [693].Execute ();
    }
    [Test] public void Trial0694 () {
        PerlTrials.trials [694].Execute ();
    }
    [Test] public void Trial0695 () {
        PerlTrials.trials [695].Execute ();
    }
    [Test] public void Trial0696 () {
        PerlTrials.trials [696].Execute ();
    }
    [Test] public void Trial0697 () {
        PerlTrials.trials [697].Execute ();
    }
    [Test] public void Trial0698 () {
        PerlTrials.trials [698].Execute ();
    }
    [Test] public void Trial0699 () {
        PerlTrials.trials [699].Execute ();
    }
    [Test] public void Trial0700 () {
        PerlTrials.trials [700].Execute ();
    }
    [Test] public void Trial0701 () {
        PerlTrials.trials [701].Execute ();
    }
    [Test] public void Trial0702 () {
        PerlTrials.trials [702].Execute ();
    }
    [Test] public void Trial0703 () {
        PerlTrials.trials [703].Execute ();
    }
    [Test] public void Trial0704 () {
        PerlTrials.trials [704].Execute ();
    }
    [Test] public void Trial0705 () {
        PerlTrials.trials [705].Execute ();
    }
    [Test] public void Trial0706 () {
        PerlTrials.trials [706].Execute ();
    }
    [Test] public void Trial0707 () {
        PerlTrials.trials [707].Execute ();
    }
    [Test] public void Trial0708 () {
        PerlTrials.trials [708].Execute ();
    }
    [Test] public void Trial0709 () {
        PerlTrials.trials [709].Execute ();
    }
    [Test] public void Trial0710 () {
        PerlTrials.trials [710].Execute ();
    }
    [Test] public void Trial0711 () {
        PerlTrials.trials [711].Execute ();
    }
    [Test] public void Trial0712 () {
        PerlTrials.trials [712].Execute ();
    }
    [Test] public void Trial0713 () {
        PerlTrials.trials [713].Execute ();
    }
    [Test] public void Trial0714 () {
        PerlTrials.trials [714].Execute ();
    }
    [Test] public void Trial0715 () {
        PerlTrials.trials [715].Execute ();
    }
    [Test] public void Trial0716 () {
        PerlTrials.trials [716].Execute ();
    }
    [Test] public void Trial0717 () {
        PerlTrials.trials [717].Execute ();
    }
    [Test] public void Trial0718 () {
        PerlTrials.trials [718].Execute ();
    }
    [Test] public void Trial0719 () {
        PerlTrials.trials [719].Execute ();
    }
    [Test] public void Trial0720 () {
        PerlTrials.trials [720].Execute ();
    }
    [Test] public void Trial0721 () {
        PerlTrials.trials [721].Execute ();
    }
    [Test] public void Trial0722 () {
        PerlTrials.trials [722].Execute ();
    }
    [Test] public void Trial0723 () {
        PerlTrials.trials [723].Execute ();
    }
    [Test] public void Trial0724 () {
        PerlTrials.trials [724].Execute ();
    }
    [Test] public void Trial0725 () {
        PerlTrials.trials [725].Execute ();
    }
    [Test] public void Trial0726 () {
        PerlTrials.trials [726].Execute ();
    }
    [Test] public void Trial0727 () {
        PerlTrials.trials [727].Execute ();
    }
    [Test] public void Trial0728 () {
        PerlTrials.trials [728].Execute ();
    }
    [Test] public void Trial0729 () {
        PerlTrials.trials [729].Execute ();
    }
    [Test] public void Trial0730 () {
        PerlTrials.trials [730].Execute ();
    }
    [Test] public void Trial0731 () {
        PerlTrials.trials [731].Execute ();
    }
    [Test] public void Trial0732 () {
        PerlTrials.trials [732].Execute ();
    }
    [Test] public void Trial0733 () {
        PerlTrials.trials [733].Execute ();
    }
    [Test] public void Trial0734 () {
        PerlTrials.trials [734].Execute ();
    }
    [Test] public void Trial0735 () {
        PerlTrials.trials [735].Execute ();
    }
    [Test] public void Trial0736 () {
        PerlTrials.trials [736].Execute ();
    }
    [Test] public void Trial0737 () {
        PerlTrials.trials [737].Execute ();
    }
    [Test] public void Trial0738 () {
        PerlTrials.trials [738].Execute ();
    }
    [Test] public void Trial0739 () {
        PerlTrials.trials [739].Execute ();
    }
    [Test] public void Trial0740 () {
        PerlTrials.trials [740].Execute ();
    }
    [Test] public void Trial0741 () {
        PerlTrials.trials [741].Execute ();
    }
    [Test] public void Trial0742 () {
        PerlTrials.trials [742].Execute ();
    }
    [Test] public void Trial0743 () {
        PerlTrials.trials [743].Execute ();
    }
    [Test] public void Trial0744 () {
        PerlTrials.trials [744].Execute ();
    }
    [Test] public void Trial0745 () {
        PerlTrials.trials [745].Execute ();
    }
    [Test] public void Trial0746 () {
        PerlTrials.trials [746].Execute ();
    }
    [Test] public void Trial0747 () {
        PerlTrials.trials [747].Execute ();
    }
    [Test] public void Trial0748 () {
        PerlTrials.trials [748].Execute ();
    }
    [Test] public void Trial0749 () {
        PerlTrials.trials [749].Execute ();
    }
    [Test] public void Trial0750 () {
        PerlTrials.trials [750].Execute ();
    }
    [Test] public void Trial0751 () {
        PerlTrials.trials [751].Execute ();
    }
    [Test] public void Trial0752 () {
        PerlTrials.trials [752].Execute ();
    }
    [Test] public void Trial0753 () {
        PerlTrials.trials [753].Execute ();
    }
    [Test] public void Trial0754 () {
        PerlTrials.trials [754].Execute ();
    }
    [Test] public void Trial0755 () {
        PerlTrials.trials [755].Execute ();
    }
    [Test] public void Trial0756 () {
        PerlTrials.trials [756].Execute ();
    }
    [Test] public void Trial0757 () {
        PerlTrials.trials [757].Execute ();
    }
    [Test] public void Trial0758 () {
        PerlTrials.trials [758].Execute ();
    }
    [Test] public void Trial0759 () {
        PerlTrials.trials [759].Execute ();
    }
    [Test] public void Trial0760 () {
        PerlTrials.trials [760].Execute ();
    }
    [Test] public void Trial0761 () {
        PerlTrials.trials [761].Execute ();
    }
    [Test] public void Trial0762 () {
        PerlTrials.trials [762].Execute ();
    }
    [Test] public void Trial0763 () {
        PerlTrials.trials [763].Execute ();
    }
    [Test] public void Trial0764 () {
        PerlTrials.trials [764].Execute ();
    }
    [Test] public void Trial0765 () {
        PerlTrials.trials [765].Execute ();
    }
    [Test] public void Trial0766 () {
        PerlTrials.trials [766].Execute ();
    }
    [Test] public void Trial0767 () {
        PerlTrials.trials [767].Execute ();
    }
    [Test] public void Trial0768 () {
        PerlTrials.trials [768].Execute ();
    }
    [Test] public void Trial0769 () {
        PerlTrials.trials [769].Execute ();
    }
    [Test] public void Trial0770 () {
        PerlTrials.trials [770].Execute ();
    }
    [Test] public void Trial0771 () {
        PerlTrials.trials [771].Execute ();
    }
    [Test] public void Trial0772 () {
        PerlTrials.trials [772].Execute ();
    }
    [Test] public void Trial0773 () {
        PerlTrials.trials [773].Execute ();
    }
    [Test] public void Trial0774 () {
        PerlTrials.trials [774].Execute ();
    }
    [Test] public void Trial0775 () {
        PerlTrials.trials [775].Execute ();
    }
    [Test] public void Trial0776 () {
        PerlTrials.trials [776].Execute ();
    }
    [Test] public void Trial0777 () {
        PerlTrials.trials [777].Execute ();
    }
    [Test] public void Trial0778 () {
        PerlTrials.trials [778].Execute ();
    }
    [Test] public void Trial0779 () {
        PerlTrials.trials [779].Execute ();
    }
    [Test] public void Trial0780 () {
        PerlTrials.trials [780].Execute ();
    }
    [Test] public void Trial0781 () {
        PerlTrials.trials [781].Execute ();
    }
    [Test] public void Trial0782 () {
        PerlTrials.trials [782].Execute ();
    }
    [Test] public void Trial0783 () {
        PerlTrials.trials [783].Execute ();
    }
    [Test] public void Trial0784 () {
        PerlTrials.trials [784].Execute ();
    }
    [Test] public void Trial0785 () {
        PerlTrials.trials [785].Execute ();
    }
    [Test] public void Trial0786 () {
        PerlTrials.trials [786].Execute ();
    }
    [Test] public void Trial0787 () {
        PerlTrials.trials [787].Execute ();
    }
    [Test] public void Trial0788 () {
        PerlTrials.trials [788].Execute ();
    }
    [Test] public void Trial0789 () {
        PerlTrials.trials [789].Execute ();
    }
    [Test] public void Trial0790 () {
        PerlTrials.trials [790].Execute ();
    }
    [Test] public void Trial0791 () {
        PerlTrials.trials [791].Execute ();
    }
    [Test] public void Trial0792 () {
        PerlTrials.trials [792].Execute ();
    }
    [Test] public void Trial0793 () {
        PerlTrials.trials [793].Execute ();
    }
    [Test] public void Trial0794 () {
        PerlTrials.trials [794].Execute ();
    }
    [Test] public void Trial0795 () {
        PerlTrials.trials [795].Execute ();
    }
    [Test] public void Trial0796 () {
        PerlTrials.trials [796].Execute ();
    }
    [Test] public void Trial0797 () {
        PerlTrials.trials [797].Execute ();
    }
    [Test] public void Trial0798 () {
        PerlTrials.trials [798].Execute ();
    }
    [Test] public void Trial0799 () {
        PerlTrials.trials [799].Execute ();
    }
    [Test] public void Trial0800 () {
        PerlTrials.trials [800].Execute ();
    }
    [Test] public void Trial0801 () {
        PerlTrials.trials [801].Execute ();
    }
    [Test] public void Trial0802 () {
        PerlTrials.trials [802].Execute ();
    }
    [Test] public void Trial0803 () {
        PerlTrials.trials [803].Execute ();
    }
    [Test] public void Trial0804 () {
        PerlTrials.trials [804].Execute ();
    }
    [Test] public void Trial0805 () {
        PerlTrials.trials [805].Execute ();
    }
    [Test] public void Trial0806 () {
        PerlTrials.trials [806].Execute ();
    }
    [Test] public void Trial0807 () {
        PerlTrials.trials [807].Execute ();
    }
    [Test] public void Trial0808 () {
        PerlTrials.trials [808].Execute ();
    }
    [Test] public void Trial0809 () {
        PerlTrials.trials [809].Execute ();
    }
    [Test] public void Trial0810 () {
        PerlTrials.trials [810].Execute ();
    }
    [Test] public void Trial0811 () {
        PerlTrials.trials [811].Execute ();
    }
    [Test] public void Trial0812 () {
        PerlTrials.trials [812].Execute ();
    }
    [Test] public void Trial0813 () {
        PerlTrials.trials [813].Execute ();
    }
    [Test] public void Trial0814 () {
        PerlTrials.trials [814].Execute ();
    }
    [Test] public void Trial0815 () {
        PerlTrials.trials [815].Execute ();
    }
    [Test] public void Trial0816 () {
        PerlTrials.trials [816].Execute ();
    }
    [Test] public void Trial0817 () {
        PerlTrials.trials [817].Execute ();
    }
    [Test] public void Trial0818 () {
        PerlTrials.trials [818].Execute ();
    }
    [Test] public void Trial0819 () {
        PerlTrials.trials [819].Execute ();
    }
    [Test] public void Trial0820 () {
        PerlTrials.trials [820].Execute ();
    }
    [Test] public void Trial0821 () {
        PerlTrials.trials [821].Execute ();
    }
    [Test] public void Trial0822 () {
        PerlTrials.trials [822].Execute ();
    }
    [Test] public void Trial0823 () {
        PerlTrials.trials [823].Execute ();
    }
    [Test] public void Trial0824 () {
        PerlTrials.trials [824].Execute ();
    }
    [Test] public void Trial0825 () {
        PerlTrials.trials [825].Execute ();
    }
    [Test] public void Trial0826 () {
        PerlTrials.trials [826].Execute ();
    }
    [Test] public void Trial0827 () {
        PerlTrials.trials [827].Execute ();
    }
    [Test] public void Trial0828 () {
        PerlTrials.trials [828].Execute ();
    }
    [Test] public void Trial0829 () {
        PerlTrials.trials [829].Execute ();
    }
    [Test] public void Trial0830 () {
        PerlTrials.trials [830].Execute ();
    }
    [Test] public void Trial0831 () {
        PerlTrials.trials [831].Execute ();
    }
    [Test] public void Trial0832 () {
        PerlTrials.trials [832].Execute ();
    }
    [Test] public void Trial0833 () {
        PerlTrials.trials [833].Execute ();
    }
    [Test] public void Trial0834 () {
        PerlTrials.trials [834].Execute ();
    }
    [Test] public void Trial0835 () {
        PerlTrials.trials [835].Execute ();
    }
    [Test] public void Trial0836 () {
        PerlTrials.trials [836].Execute ();
    }
    [Test] public void Trial0837 () {
        PerlTrials.trials [837].Execute ();
    }
    [Test] public void Trial0838 () {
        PerlTrials.trials [838].Execute ();
    }
    [Test] public void Trial0839 () {
        PerlTrials.trials [839].Execute ();
    }
    [Test] public void Trial0840 () {
        PerlTrials.trials [840].Execute ();
    }
    [Test] public void Trial0841 () {
        PerlTrials.trials [841].Execute ();
    }
    [Test] public void Trial0842 () {
        PerlTrials.trials [842].Execute ();
    }
    [Test] public void Trial0843 () {
        PerlTrials.trials [843].Execute ();
    }
    [Test] public void Trial0844 () {
        PerlTrials.trials [844].Execute ();
    }
    [Test] public void Trial0845 () {
        PerlTrials.trials [845].Execute ();
    }
    [Test] public void Trial0846 () {
        PerlTrials.trials [846].Execute ();
    }
    [Test] public void Trial0847 () {
        PerlTrials.trials [847].Execute ();
    }
    [Test] public void Trial0848 () {
        PerlTrials.trials [848].Execute ();
    }
    [Test] public void Trial0849 () {
        PerlTrials.trials [849].Execute ();
    }
    [Test] public void Trial0850 () {
        PerlTrials.trials [850].Execute ();
    }
    [Test] public void Trial0851 () {
        PerlTrials.trials [851].Execute ();
    }
    [Test] public void Trial0852 () {
        PerlTrials.trials [852].Execute ();
    }
    [Test] public void Trial0853 () {
        PerlTrials.trials [853].Execute ();
    }
    [Test] public void Trial0854 () {
        PerlTrials.trials [854].Execute ();
    }
    [Test] public void Trial0855 () {
        PerlTrials.trials [855].Execute ();
    }
    [Test] public void Trial0856 () {
        PerlTrials.trials [856].Execute ();
    }
    [Test] public void Trial0857 () {
        PerlTrials.trials [857].Execute ();
    }
    [Test] public void Trial0858 () {
        PerlTrials.trials [858].Execute ();
    }
    [Test] public void Trial0859 () {
        PerlTrials.trials [859].Execute ();
    }
    [Test] public void Trial0860 () {
        PerlTrials.trials [860].Execute ();
    }
    [Test] public void Trial0861 () {
        PerlTrials.trials [861].Execute ();
    }
    [Test] public void Trial0862 () {
        PerlTrials.trials [862].Execute ();
    }
    [Test] public void Trial0863 () {
        PerlTrials.trials [863].Execute ();
    }
    [Test] public void Trial0864 () {
        PerlTrials.trials [864].Execute ();
    }
    [Test] public void Trial0865 () {
        PerlTrials.trials [865].Execute ();
    }
    [Test] public void Trial0866 () {
        PerlTrials.trials [866].Execute ();
    }
    [Test] public void Trial0867 () {
        PerlTrials.trials [867].Execute ();
    }
    [Test] public void Trial0868 () {
        PerlTrials.trials [868].Execute ();
    }
    [Test] public void Trial0869 () {
        PerlTrials.trials [869].Execute ();
    }
    [Test] public void Trial0870 () {
        PerlTrials.trials [870].Execute ();
    }
    [Test] public void Trial0871 () {
        PerlTrials.trials [871].Execute ();
    }
    [Test] public void Trial0872 () {
        PerlTrials.trials [872].Execute ();
    }
    [Test] public void Trial0873 () {
        PerlTrials.trials [873].Execute ();
    }
    [Test] public void Trial0874 () {
        PerlTrials.trials [874].Execute ();
    }
    [Test] public void Trial0875 () {
        PerlTrials.trials [875].Execute ();
    }
    [Test] public void Trial0876 () {
        PerlTrials.trials [876].Execute ();
    }
    [Test] public void Trial0877 () {
        PerlTrials.trials [877].Execute ();
    }
    [Test] public void Trial0878 () {
        PerlTrials.trials [878].Execute ();
    }
    [Test] public void Trial0879 () {
        PerlTrials.trials [879].Execute ();
    }
    [Test] public void Trial0880 () {
        PerlTrials.trials [880].Execute ();
    }
    [Test] public void Trial0881 () {
        PerlTrials.trials [881].Execute ();
    }
    [Test] public void Trial0882 () {
        PerlTrials.trials [882].Execute ();
    }
    [Test] public void Trial0883 () {
        PerlTrials.trials [883].Execute ();
    }
    [Test] public void Trial0884 () {
        PerlTrials.trials [884].Execute ();
    }
    [Test] public void Trial0885 () {
        PerlTrials.trials [885].Execute ();
    }
    [Test] public void Trial0886 () {
        PerlTrials.trials [886].Execute ();
    }
    [Test] public void Trial0887 () {
        PerlTrials.trials [887].Execute ();
    }
    [Test] public void Trial0888 () {
        PerlTrials.trials [888].Execute ();
    }
    [Test] public void Trial0889 () {
        PerlTrials.trials [889].Execute ();
    }
    [Test] public void Trial0890 () {
        PerlTrials.trials [890].Execute ();
    }
    [Test] public void Trial0891 () {
        PerlTrials.trials [891].Execute ();
    }
    [Test] public void Trial0892 () {
        PerlTrials.trials [892].Execute ();
    }
    [Test] public void Trial0893 () {
        PerlTrials.trials [893].Execute ();
    }
    [Test] public void Trial0894 () {
        PerlTrials.trials [894].Execute ();
    }
    [Test] public void Trial0895 () {
        PerlTrials.trials [895].Execute ();
    }
    [Test] public void Trial0896 () {
        PerlTrials.trials [896].Execute ();
    }
    [Test] public void Trial0897 () {
        PerlTrials.trials [897].Execute ();
    }
    [Test] public void Trial0898 () {
        PerlTrials.trials [898].Execute ();
    }
    [Test] public void Trial0899 () {
        PerlTrials.trials [899].Execute ();
    }
    [Test] public void Trial0900 () {
        PerlTrials.trials [900].Execute ();
    }
    [Test] public void Trial0901 () {
        PerlTrials.trials [901].Execute ();
    }
    [Test] public void Trial0902 () {
        PerlTrials.trials [902].Execute ();
    }
    [Test] public void Trial0903 () {
        PerlTrials.trials [903].Execute ();
    }
    [Test] public void Trial0904 () {
        PerlTrials.trials [904].Execute ();
    }
    [Test] public void Trial0905 () {
        PerlTrials.trials [905].Execute ();
    }
    [Test] public void Trial0906 () {
        PerlTrials.trials [906].Execute ();
    }
    [Test] public void Trial0907 () {
        PerlTrials.trials [907].Execute ();
    }
    [Test] public void Trial0908 () {
        PerlTrials.trials [908].Execute ();
    }
    [Test] public void Trial0909 () {
        PerlTrials.trials [909].Execute ();
    }
    [Test] public void Trial0910 () {
        PerlTrials.trials [910].Execute ();
    }
    [Test] public void Trial0911 () {
        PerlTrials.trials [911].Execute ();
    }
    [Test] public void Trial0912 () {
        PerlTrials.trials [912].Execute ();
    }
    [Test] public void Trial0913 () {
        PerlTrials.trials [913].Execute ();
    }
    [Test] public void Trial0914 () {
        PerlTrials.trials [914].Execute ();
    }
    [Test] public void Trial0915 () {
        PerlTrials.trials [915].Execute ();
    }
    [Test] public void Trial0916 () {
        PerlTrials.trials [916].Execute ();
    }
    [Test] public void Trial0917 () {
        PerlTrials.trials [917].Execute ();
    }
    [Test] public void Trial0918 () {
        PerlTrials.trials [918].Execute ();
    }
    [Test] public void Trial0919 () {
        PerlTrials.trials [919].Execute ();
    }
    [Test] public void Trial0920 () {
        PerlTrials.trials [920].Execute ();
    }
    [Test] public void Trial0921 () {
        PerlTrials.trials [921].Execute ();
    }
    [Test] public void Trial0922 () {
        PerlTrials.trials [922].Execute ();
    }
    [Test] public void Trial0923 () {
        PerlTrials.trials [923].Execute ();
    }
    [Test] public void Trial0924 () {
        PerlTrials.trials [924].Execute ();
    }
    [Test] public void Trial0925 () {
        PerlTrials.trials [925].Execute ();
    }
    [Test] public void Trial0926 () {
        PerlTrials.trials [926].Execute ();
    }
    [Test] public void Trial0927 () {
        PerlTrials.trials [927].Execute ();
    }
    [Test] public void Trial0928 () {
        PerlTrials.trials [928].Execute ();
    }
    [Test] public void Trial0929 () {
        PerlTrials.trials [929].Execute ();
    }
    [Test] public void Trial0930 () {
        PerlTrials.trials [930].Execute ();
    }
    [Test] public void Trial0931 () {
        PerlTrials.trials [931].Execute ();
    }
    [Test] public void Trial0932 () {
        PerlTrials.trials [932].Execute ();
    }
    [Test] public void Trial0933 () {
        PerlTrials.trials [933].Execute ();
    }
    [Test] public void Trial0934 () {
        PerlTrials.trials [934].Execute ();
    }
    [Test] public void Trial0935 () {
        PerlTrials.trials [935].Execute ();
    }
    [Test] public void Trial0936 () {
        PerlTrials.trials [936].Execute ();
    }
    [Test] public void Trial0937 () {
        PerlTrials.trials [937].Execute ();
    }
    [Test] public void Trial0938 () {
        PerlTrials.trials [938].Execute ();
    }
    [Test] public void Trial0939 () {
        PerlTrials.trials [939].Execute ();
    }
    [Test] public void Trial0940 () {
        PerlTrials.trials [940].Execute ();
    }
    [Test] public void Trial0941 () {
        PerlTrials.trials [941].Execute ();
    }
    [Test] public void Trial0942 () {
        PerlTrials.trials [942].Execute ();
    }
    [Test] public void Trial0943 () {
        PerlTrials.trials [943].Execute ();
    }
    [Test] public void Trial0944 () {
        PerlTrials.trials [944].Execute ();
    }
    [Test] public void Trial0945 () {
        PerlTrials.trials [945].Execute ();
    }
    [Test] public void Trial0946 () {
        PerlTrials.trials [946].Execute ();
    }
    [Test] public void Trial0947 () {
        PerlTrials.trials [947].Execute ();
    }
    [Test] public void Trial0948 () {
        PerlTrials.trials [948].Execute ();
    }
    [Test] public void Trial0949 () {
        PerlTrials.trials [949].Execute ();
    }
    [Test] public void Trial0950 () {
        PerlTrials.trials [950].Execute ();
    }
    [Test] public void Trial0951 () {
        PerlTrials.trials [951].Execute ();
    }
    [Test] public void Trial0952 () {
        PerlTrials.trials [952].Execute ();
    }
    [Test] public void Trial0953 () {
        PerlTrials.trials [953].Execute ();
    }
    [Test] public void Trial0954 () {
        PerlTrials.trials [954].Execute ();
    }
    [Test] public void Trial0955 () {
        PerlTrials.trials [955].Execute ();
    }
    [Test] public void Trial0956 () {
        PerlTrials.trials [956].Execute ();
    }
    [Test] public void Trial0957 () {
        PerlTrials.trials [957].Execute ();
    }
    [Test] public void Trial0958 () {
        PerlTrials.trials [958].Execute ();
    }
    [Test] public void Trial0959 () {
        PerlTrials.trials [959].Execute ();
    }
    [Test] public void Trial0960 () {
        PerlTrials.trials [960].Execute ();
    }
    [Test] public void Trial0961 () {
        PerlTrials.trials [961].Execute ();
    }
    [Test] public void Trial0962 () {
        PerlTrials.trials [962].Execute ();
    }
    [Test] public void Trial0963 () {
        PerlTrials.trials [963].Execute ();
    }
    [Test] public void Trial0964 () {
        PerlTrials.trials [964].Execute ();
    }
    [Test] public void Trial0965 () {
        PerlTrials.trials [965].Execute ();
    }
    [Test] public void Trial0966 () {
        PerlTrials.trials [966].Execute ();
    }
    [Test] public void Trial0967 () {
        PerlTrials.trials [967].Execute ();
    }
    [Test] public void Trial0968 () {
        PerlTrials.trials [968].Execute ();
    }
    [Test] public void Trial0969 () {
        PerlTrials.trials [969].Execute ();
    }
    [Test] public void Trial0970 () {
        PerlTrials.trials [970].Execute ();
    }
    [Test] public void Trial0971 () {
        PerlTrials.trials [971].Execute ();
    }
    [Test] public void Trial0972 () {
        PerlTrials.trials [972].Execute ();
    }
    [Test] public void Trial0973 () {
        PerlTrials.trials [973].Execute ();
    }
    [Test] public void Trial0974 () {
        PerlTrials.trials [974].Execute ();
    }
    [Test] public void Trial0975 () {
        PerlTrials.trials [975].Execute ();
    }
    [Test] public void Trial0976 () {
        PerlTrials.trials [976].Execute ();
    }
    [Test] public void Trial0977 () {
        PerlTrials.trials [977].Execute ();
    }
    [Test] public void Trial0978 () {
        PerlTrials.trials [978].Execute ();
    }
    [Test] public void Trial0979 () {
        PerlTrials.trials [979].Execute ();
    }
    [Test] public void Trial0980 () {
        PerlTrials.trials [980].Execute ();
    }
    [Test] public void Trial0981 () {
        PerlTrials.trials [981].Execute ();
    }
    [Test] public void Trial0982 () {
        PerlTrials.trials [982].Execute ();
    }
    [Test] public void Trial0983 () {
        PerlTrials.trials [983].Execute ();
    }
    [Test] public void Trial0984 () {
        PerlTrials.trials [984].Execute ();
    }
    [Test] public void Trial0985 () {
        PerlTrials.trials [985].Execute ();
    }
    [Test] public void Trial0986 () {
        PerlTrials.trials [986].Execute ();
    }
    [Test] public void Trial0987 () {
        PerlTrials.trials [987].Execute ();
    }
    [Test] public void Trial0988 () {
        PerlTrials.trials [988].Execute ();
    }
    [Test] public void Trial0989 () {
        PerlTrials.trials [989].Execute ();
    }
    [Test] public void Trial0990 () {
        PerlTrials.trials [990].Execute ();
    }
    [Test] public void Trial0991 () {
        PerlTrials.trials [991].Execute ();
    }
}
}
