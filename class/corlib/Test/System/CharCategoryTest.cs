//
// CharCategoryTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
// Brute force tests for Char.IsXXX() static methods.
// The result string is generated by the short program, which ran under MS.NET.
// (See the bottom of this file.)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

namespace MonoTests.System
{
	[TestFixture]
	public class CharCategoryTest : Assertion
	{
		static char DSC = Path.DirectorySeparatorChar;

		delegate bool ComparisonMethod (char c);

		private void CompareWithDump (ComparisonMethod cm, string dump, bool testTrue)
		{
			StringWriter sw = new StringWriter ();
			int total = 0;

			for (int i = 0; i <= Char.MaxValue; i++) {
				if (cm ((char) i) == testTrue) {
					sw.Write (i.ToString ("x"));
					sw.Write (' ');
					total++;
				}
			}
			sw.Write ("found " + total + " chars.");

//			string filename = Assembly.GetAssembly (typeof (Char)).Location + "/resources/" + dumpfile;
//			StreamReader sr = new StreamReader (filename);
//			dump = sr.ReadToEnd ();
//			sr.Close ();
			AssertEquals (dump, sw.ToString ());
		}

[Test]
public void TryPath ()
{
	AssertEquals ("", Environment.CurrentDirectory);
}

		[Test]
		public void IsControl ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsControl), controls, true);
		}

		[Test]
		public void IsDigit ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsDigit), digits, true);
		}

		[Test]
		[Ignore ("The dump file is huge, so avoided this test way.")]
		public void IsLetter ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsLetter), letters, false);
		}

		[Test]
		[Ignore ("The dump file is huge, so avoided this test way.")]
		public void IsLetterOrDigit ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsLetterOrDigit), letterOrDigits, false);
		}

		[Test]
		public void IsLower ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsLower), lowerChars, true);
		}

		[Test]
		public void IsNumber ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsNumber), numbers, true);
		}

		[Test]
		public void IsPunctuation ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsPunctuation), puncts, true);
		}

		[Test]
		public void IsSeparator ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsSeparator), separators, true);
		}

		[Test]
		public void IsSurrogate ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsSurrogate), surrogateChars, true);
		}

		[Test]
		public void IsSymbol ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsSymbol), symbolChars, true);
		}

		[Test]
		public void IsUpper ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsUpper), upperChars, true);
		}

		[Test]
		public void IsWhiteSpace ()
		{
			CompareWithDump (new ComparisonMethod (Char.IsWhiteSpace), whitespaceChars, true);
		}


		string controls = "0 1 2 3 4 5 6 7 8 9 a b c d e f 10 11 12 13 14 15 16 17 18 19 1a 1b 1c 1d 1e 1f 7f 80 81 82 83 84 85 86 87 88 89 8a 8b 8c 8d 8e 8f 90 91 92 93 94 95 96 97 98 99 9a 9b 9c 9d 9e 9f found 65 chars.";

		string digits = "30 31 32 33 34 35 36 37 38 39 660 661 662 663 664 665 666 667 668 669 6f0 6f1 6f2 6f3 6f4 6f5 6f6 6f7 6f8 6f9 966 967 968 969 96a 96b 96c 96d 96e 96f 9e6 9e7 9e8 9e9 9ea 9eb 9ec 9ed 9ee 9ef a66 a67 a68 a69 a6a a6b a6c a6d a6e a6f ae6 ae7 ae8 ae9 aea aeb aec aed aee aef b66 b67 b68 b69 b6a b6b b6c b6d b6e b6f be7 be8 be9 bea beb bec bed bee bef c66 c67 c68 c69 c6a c6b c6c c6d c6e c6f ce6 ce7 ce8 ce9 cea ceb cec ced cee cef d66 d67 d68 d69 d6a d6b d6c d6d d6e d6f e50 e51 e52 e53 e54 e55 e56 e57 e58 e59 ed0 ed1 ed2 ed3 ed4 ed5 ed6 ed7 ed8 ed9 f20 f21 f22 f23 f24 f25 f26 f27 f28 f29 1040 1041 1042 1043 1044 1045 1046 1047 1048 1049 1369 136a 136b 136c 136d 136e 136f 1370 1371 17e0 17e1 17e2 17e3 17e4 17e5 17e6 17e7 17e8 17e9 1810 1811 1812 1813 1814 1815 1816 1817 1818 1819 ff10 ff11 ff12 ff13 ff14 ff15 ff16 ff17 ff18 ff19 found 198 chars.";

		string lowerChars = "61 62 63 64 65 66 67 68 69 6a 6b 6c 6d 6e 6f 70 71 72 73 74 75 76 77 78 79 7a aa b5 ba df e0 e1 e2 e3 e4 e5 e6 e7 e8 e9 ea eb ec ed ee ef f0 f1 f2 f3 f4 f5 f6 f8 f9 fa fb fc fd fe ff 101 103 105 107 109 10b 10d 10f 111 113 115 117 119 11b 11d 11f 121 123 125 127 129 12b 12d 12f 131 133 135 137 138 13a 13c 13e 140 142 144 146 148 149 14b 14d 14f 151 153 155 157 159 15b 15d 15f 161 163 165 167 169 16b 16d 16f 171 173 175 177 17a 17c 17e 17f 180 183 185 188 18c 18d 192 195 199 19a 19b 19e 1a1 1a3 1a5 1a8 1aa 1ab 1ad 1b0 1b4 1b6 1b9 1ba 1bd 1be 1bf 1c6 1c9 1cc 1ce 1d0 1d2 1d4 1d6 1d8 1da 1dc 1dd 1df 1e1 1e3 1e5 1e7 1e9 1eb 1ed 1ef 1f0 1f3 1f5 1f9 1fb 1fd 1ff 201 203 205 207 209 20b 20d 20f 211 213 215 217 219 21b 21d 21f 223 225 227 229 22b 22d 22f 231 233 250 251 252 253 254 255 256 257 258 259 25a 25b 25c 25d 25e 25f 260 261 262 263 264 265 266 267 268 269 26a 26b 26c 26d 26e 26f 270 271 272 273 274 275 276 277 278 279 27a 27b 27c 27d 27e 27f 280 281 282 283 284 285 286 287 288 289 28a 28b 28c 28d 28e 28f 290 291 292 293 294 295 296 297 298 299 29a 29b 29c 29d 29e 29f 2a0 2a1 2a2 2a3 2a4 2a5 2a6 2a7 2a8 2a9 2aa 2ab 2ac 2ad 390 3ac 3ad 3ae 3af 3b0 3b1 3b2 3b3 3b4 3b5 3b6 3b7 3b8 3b9 3ba 3bb 3bc 3bd 3be 3bf 3c0 3c1 3c2 3c3 3c4 3c5 3c6 3c7 3c8 3c9 3ca 3cb 3cc 3cd 3ce 3d0 3d1 3d5 3d6 3d7 3db 3dd 3df 3e1 3e3 3e5 3e7 3e9 3eb 3ed 3ef 3f0 3f1 3f2 3f3 430 431 432 433 434 435 436 437 438 439 43a 43b 43c 43d 43e 43f 440 441 442 443 444 445 446 447 448 449 44a 44b 44c 44d 44e 44f 450 451 452 453 454 455 456 457 458 459 45a 45b 45c 45d 45e 45f 461 463 465 467 469 46b 46d 46f 471 473 475 477 479 47b 47d 47f 481 48d 48f 491 493 495 497 499 49b 49d 49f 4a1 4a3 4a5 4a7 4a9 4ab 4ad 4af 4b1 4b3 4b5 4b7 4b9 4bb 4bd 4bf 4c2 4c4 4c8 4cc 4d1 4d3 4d5 4d7 4d9 4db 4dd 4df 4e1 4e3 4e5 4e7 4e9 4eb 4ed 4ef 4f1 4f3 4f5 4f9 561 562 563 564 565 566 567 568 569 56a 56b 56c 56d 56e 56f 570 571 572 573 574 575 576 577 578 579 57a 57b 57c 57d 57e 57f 580 581 582 583 584 585 586 587 1e01 1e03 1e05 1e07 1e09 1e0b 1e0d 1e0f 1e11 1e13 1e15 1e17 1e19 1e1b 1e1d 1e1f 1e21 1e23 1e25 1e27 1e29 1e2b 1e2d 1e2f 1e31 1e33 1e35 1e37 1e39 1e3b 1e3d 1e3f 1e41 1e43 1e45 1e47 1e49 1e4b 1e4d 1e4f 1e51 1e53 1e55 1e57 1e59 1e5b 1e5d 1e5f 1e61 1e63 1e65 1e67 1e69 1e6b 1e6d 1e6f 1e71 1e73 1e75 1e77 1e79 1e7b 1e7d 1e7f 1e81 1e83 1e85 1e87 1e89 1e8b 1e8d 1e8f 1e91 1e93 1e95 1e96 1e97 1e98 1e99 1e9a 1e9b 1ea1 1ea3 1ea5 1ea7 1ea9 1eab 1ead 1eaf 1eb1 1eb3 1eb5 1eb7 1eb9 1ebb 1ebd 1ebf 1ec1 1ec3 1ec5 1ec7 1ec9 1ecb 1ecd 1ecf 1ed1 1ed3 1ed5 1ed7 1ed9 1edb 1edd 1edf 1ee1 1ee3 1ee5 1ee7 1ee9 1eeb 1eed 1eef 1ef1 1ef3 1ef5 1ef7 1ef9 1f00 1f01 1f02 1f03 1f04 1f05 1f06 1f07 1f10 1f11 1f12 1f13 1f14 1f15 1f20 1f21 1f22 1f23 1f24 1f25 1f26 1f27 1f30 1f31 1f32 1f33 1f34 1f35 1f36 1f37 1f40 1f41 1f42 1f43 1f44 1f45 1f50 1f51 1f52 1f53 1f54 1f55 1f56 1f57 1f60 1f61 1f62 1f63 1f64 1f65 1f66 1f67 1f70 1f71 1f72 1f73 1f74 1f75 1f76 1f77 1f78 1f79 1f7a 1f7b 1f7c 1f7d 1f80 1f81 1f82 1f83 1f84 1f85 1f86 1f87 1f90 1f91 1f92 1f93 1f94 1f95 1f96 1f97 1fa0 1fa1 1fa2 1fa3 1fa4 1fa5 1fa6 1fa7 1fb0 1fb1 1fb2 1fb3 1fb4 1fb6 1fb7 1fbe 1fc2 1fc3 1fc4 1fc6 1fc7 1fd0 1fd1 1fd2 1fd3 1fd6 1fd7 1fe0 1fe1 1fe2 1fe3 1fe4 1fe5 1fe6 1fe7 1ff2 1ff3 1ff4 1ff6 1ff7 207f 210a 210e 210f 2113 212f 2134 2139 fb00 fb01 fb02 fb03 fb04 fb05 fb06 fb13 fb14 fb15 fb16 fb17 ff41 ff42 ff43 ff44 ff45 ff46 ff47 ff48 ff49 ff4a ff4b ff4c ff4d ff4e ff4f ff50 ff51 ff52 ff53 ff54 ff55 ff56 ff57 ff58 ff59 ff5a found 804 chars.";

		string numbers = "30 31 32 33 34 35 36 37 38 39 b2 b3 b9 bc bd be 660 661 662 663 664 665 666 667 668 669 6f0 6f1 6f2 6f3 6f4 6f5 6f6 6f7 6f8 6f9 966 967 968 969 96a 96b 96c 96d 96e 96f 9e6 9e7 9e8 9e9 9ea 9eb 9ec 9ed 9ee 9ef 9f4 9f5 9f6 9f7 9f8 9f9 a66 a67 a68 a69 a6a a6b a6c a6d a6e a6f ae6 ae7 ae8 ae9 aea aeb aec aed aee aef b66 b67 b68 b69 b6a b6b b6c b6d b6e b6f be7 be8 be9 bea beb bec bed bee bef bf0 bf1 bf2 c66 c67 c68 c69 c6a c6b c6c c6d c6e c6f ce6 ce7 ce8 ce9 cea ceb cec ced cee cef d66 d67 d68 d69 d6a d6b d6c d6d d6e d6f e50 e51 e52 e53 e54 e55 e56 e57 e58 e59 ed0 ed1 ed2 ed3 ed4 ed5 ed6 ed7 ed8 ed9 f20 f21 f22 f23 f24 f25 f26 f27 f28 f29 f2a f2b f2c f2d f2e f2f f30 f31 f32 f33 1040 1041 1042 1043 1044 1045 1046 1047 1048 1049 1369 136a 136b 136c 136d 136e 136f 1370 1371 1372 1373 1374 1375 1376 1377 1378 1379 137a 137b 137c 16ee 16ef 16f0 17e0 17e1 17e2 17e3 17e4 17e5 17e6 17e7 17e8 17e9 1810 1811 1812 1813 1814 1815 1816 1817 1818 1819 2070 2074 2075 2076 2077 2078 2079 2080 2081 2082 2083 2084 2085 2086 2087 2088 2089 2153 2154 2155 2156 2157 2158 2159 215a 215b 215c 215d 215e 215f 2160 2161 2162 2163 2164 2165 2166 2167 2168 2169 216a 216b 216c 216d 216e 216f 2170 2171 2172 2173 2174 2175 2176 2177 2178 2179 217a 217b 217c 217d 217e 217f 2180 2181 2182 2183 2460 2461 2462 2463 2464 2465 2466 2467 2468 2469 246a 246b 246c 246d 246e 246f 2470 2471 2472 2473 2474 2475 2476 2477 2478 2479 247a 247b 247c 247d 247e 247f 2480 2481 2482 2483 2484 2485 2486 2487 2488 2489 248a 248b 248c 248d 248e 248f 2490 2491 2492 2493 2494 2495 2496 2497 2498 2499 249a 249b 24ea 2776 2777 2778 2779 277a 277b 277c 277d 277e 277f 2780 2781 2782 2783 2784 2785 2786 2787 2788 2789 278a 278b 278c 278d 278e 278f 2790 2791 2792 2793 3007 3021 3022 3023 3024 3025 3026 3027 3028 3029 3038 3039 303a 3192 3193 3194 3195 3220 3221 3222 3223 3224 3225 3226 3227 3228 3229 3280 3281 3282 3283 3284 3285 3286 3287 3288 3289 ff10 ff11 ff12 ff13 ff14 ff15 ff16 ff17 ff18 ff19 found 431 chars.";

		string puncts = "21 22 23 25 26 27 28 29 2a 2c 2d 2e 2f 3a 3b 3f 40 5b 5c 5d 5f 7b 7d a1 ab ad b7 bb bf 37e 387 55a 55b 55c 55d 55e 55f 589 58a 5be 5c0 5c3 5f3 5f4 60c 61b 61f 66a 66b 66c 66d 6d4 700 701 702 703 704 705 706 707 708 709 70a 70b 70c 70d 964 965 970 df4 e4f e5a e5b f04 f05 f06 f07 f08 f09 f0a f0b f0c f0d f0e f0f f10 f11 f12 f3a f3b f3c f3d f85 104a 104b 104c 104d 104e 104f 10fb 1361 1362 1363 1364 1365 1366 1367 1368 166d 166e 169b 169c 16eb 16ec 16ed 17d4 17d5 17d6 17d7 17d8 17d9 17da 17dc 1800 1801 1802 1803 1804 1805 1806 1807 1808 1809 180a 2010 2011 2012 2013 2014 2015 2016 2017 2018 2019 201a 201b 201c 201d 201e 201f 2020 2021 2022 2023 2024 2025 2026 2027 2030 2031 2032 2033 2034 2035 2036 2037 2038 2039 203a 203b 203c 203d 203e 203f 2040 2041 2042 2043 2045 2046 2048 2049 204a 204b 204c 204d 207d 207e 208d 208e 2329 232a 3001 3002 3003 3008 3009 300a 300b 300c 300d 300e 300f 3010 3011 3014 3015 3016 3017 3018 3019 301a 301b 301c 301d 301e 301f 3030 30fb fd3e fd3f fe30 fe31 fe32 fe33 fe34 fe35 fe36 fe37 fe38 fe39 fe3a fe3b fe3c fe3d fe3e fe3f fe40 fe41 fe42 fe43 fe44 fe49 fe4a fe4b fe4c fe4d fe4e fe4f fe50 fe51 fe52 fe54 fe55 fe56 fe57 fe58 fe59 fe5a fe5b fe5c fe5d fe5e fe5f fe60 fe61 fe63 fe68 fe6a fe6b ff01 ff02 ff03 ff05 ff06 ff07 ff08 ff09 ff0a ff0c ff0d ff0e ff0f ff1a ff1b ff1f ff20 ff3b ff3c ff3d ff3f ff5b ff5d ff61 ff62 ff63 ff64 ff65 found 298 chars.";

		string separators = "20 a0 1680 2000 2001 2002 2003 2004 2005 2006 2007 2008 2009 200a 200b 2028 2029 202f 3000 found 19 chars.";

		string surrogateChars = "d800 d801 d802 d803 d804 d805 d806 d807 d808 d809 d80a d80b d80c d80d d80e d80f d810 d811 d812 d813 d814 d815 d816 d817 d818 d819 d81a d81b d81c d81d d81e d81f d820 d821 d822 d823 d824 d825 d826 d827 d828 d829 d82a d82b d82c d82d d82e d82f d830 d831 d832 d833 d834 d835 d836 d837 d838 d839 d83a d83b d83c d83d d83e d83f d840 d841 d842 d843 d844 d845 d846 d847 d848 d849 d84a d84b d84c d84d d84e d84f d850 d851 d852 d853 d854 d855 d856 d857 d858 d859 d85a d85b d85c d85d d85e d85f d860 d861 d862 d863 d864 d865 d866 d867 d868 d869 d86a d86b d86c d86d d86e d86f d870 d871 d872 d873 d874 d875 d876 d877 d878 d879 d87a d87b d87c d87d d87e d87f d880 d881 d882 d883 d884 d885 d886 d887 d888 d889 d88a d88b d88c d88d d88e d88f d890 d891 d892 d893 d894 d895 d896 d897 d898 d899 d89a d89b d89c d89d d89e d89f d8a0 d8a1 d8a2 d8a3 d8a4 d8a5 d8a6 d8a7 d8a8 d8a9 d8aa d8ab d8ac d8ad d8ae d8af d8b0 d8b1 d8b2 d8b3 d8b4 d8b5 d8b6 d8b7 d8b8 d8b9 d8ba d8bb d8bc d8bd d8be d8bf d8c0 d8c1 d8c2 d8c3 d8c4 d8c5 d8c6 d8c7 d8c8 d8c9 d8ca d8cb d8cc d8cd d8ce d8cf d8d0 d8d1 d8d2 d8d3 d8d4 d8d5 d8d6 d8d7 d8d8 d8d9 d8da d8db d8dc d8dd d8de d8df d8e0 d8e1 d8e2 d8e3 d8e4 d8e5 d8e6 d8e7 d8e8 d8e9 d8ea d8eb d8ec d8ed d8ee d8ef d8f0 d8f1 d8f2 d8f3 d8f4 d8f5 d8f6 d8f7 d8f8 d8f9 d8fa d8fb d8fc d8fd d8fe d8ff d900 d901 d902 d903 d904 d905 d906 d907 d908 d909 d90a d90b d90c d90d d90e d90f d910 d911 d912 d913 d914 d915 d916 d917 d918 d919 d91a d91b d91c d91d d91e d91f d920 d921 d922 d923 d924 d925 d926 d927 d928 d929 d92a d92b d92c d92d d92e d92f d930 d931 d932 d933 d934 d935 d936 d937 d938 d939 d93a d93b d93c d93d d93e d93f d940 d941 d942 d943 d944 d945 d946 d947 d948 d949 d94a d94b d94c d94d d94e d94f d950 d951 d952 d953 d954 d955 d956 d957 d958 d959 d95a d95b d95c d95d d95e d95f d960 d961 d962 d963 d964 d965 d966 d967 d968 d969 d96a d96b d96c d96d d96e d96f d970 d971 d972 d973 d974 d975 d976 d977 d978 d979 d97a d97b d97c d97d d97e d97f d980 d981 d982 d983 d984 d985 d986 d987 d988 d989 d98a d98b d98c d98d d98e d98f d990 d991 d992 d993 d994 d995 d996 d997 d998 d999 d99a d99b d99c d99d d99e d99f d9a0 d9a1 d9a2 d9a3 d9a4 d9a5 d9a6 d9a7 d9a8 d9a9 d9aa d9ab d9ac d9ad d9ae d9af d9b0 d9b1 d9b2 d9b3 d9b4 d9b5 d9b6 d9b7 d9b8 d9b9 d9ba d9bb d9bc d9bd d9be d9bf d9c0 d9c1 d9c2 d9c3 d9c4 d9c5 d9c6 d9c7 d9c8 d9c9 d9ca d9cb d9cc d9cd d9ce d9cf d9d0 d9d1 d9d2 d9d3 d9d4 d9d5 d9d6 d9d7 d9d8 d9d9 d9da d9db d9dc d9dd d9de d9df d9e0 d9e1 d9e2 d9e3 d9e4 d9e5 d9e6 d9e7 d9e8 d9e9 d9ea d9eb d9ec d9ed d9ee d9ef d9f0 d9f1 d9f2 d9f3 d9f4 d9f5 d9f6 d9f7 d9f8 d9f9 d9fa d9fb d9fc d9fd d9fe d9ff da00 da01 da02 da03 da04 da05 da06 da07 da08 da09 da0a da0b da0c da0d da0e da0f da10 da11 da12 da13 da14 da15 da16 da17 da18 da19 da1a da1b da1c da1d da1e da1f da20 da21 da22 da23 da24 da25 da26 da27 da28 da29 da2a da2b da2c da2d da2e da2f da30 da31 da32 da33 da34 da35 da36 da37 da38 da39 da3a da3b da3c da3d da3e da3f da40 da41 da42 da43 da44 da45 da46 da47 da48 da49 da4a da4b da4c da4d da4e da4f da50 da51 da52 da53 da54 da55 da56 da57 da58 da59 da5a da5b da5c da5d da5e da5f da60 da61 da62 da63 da64 da65 da66 da67 da68 da69 da6a da6b da6c da6d da6e da6f da70 da71 da72 da73 da74 da75 da76 da77 da78 da79 da7a da7b da7c da7d da7e da7f da80 da81 da82 da83 da84 da85 da86 da87 da88 da89 da8a da8b da8c da8d da8e da8f da90 da91 da92 da93 da94 da95 da96 da97 da98 da99 da9a da9b da9c da9d da9e da9f daa0 daa1 daa2 daa3 daa4 daa5 daa6 daa7 daa8 daa9 daaa daab daac daad daae daaf dab0 dab1 dab2 dab3 dab4 dab5 dab6 dab7 dab8 dab9 daba dabb dabc dabd dabe dabf dac0 dac1 dac2 dac3 dac4 dac5 dac6 dac7 dac8 dac9 daca dacb dacc dacd dace dacf dad0 dad1 dad2 dad3 dad4 dad5 dad6 dad7 dad8 dad9 dada dadb dadc dadd dade dadf dae0 dae1 dae2 dae3 dae4 dae5 dae6 dae7 dae8 dae9 daea daeb daec daed daee daef daf0 daf1 daf2 daf3 daf4 daf5 daf6 daf7 daf8 daf9 dafa dafb dafc dafd dafe daff db00 db01 db02 db03 db04 db05 db06 db07 db08 db09 db0a db0b db0c db0d db0e db0f db10 db11 db12 db13 db14 db15 db16 db17 db18 db19 db1a db1b db1c db1d db1e db1f db20 db21 db22 db23 db24 db25 db26 db27 db28 db29 db2a db2b db2c db2d db2e db2f db30 db31 db32 db33 db34 db35 db36 db37 db38 db39 db3a db3b db3c db3d db3e db3f db40 db41 db42 db43 db44 db45 db46 db47 db48 db49 db4a db4b db4c db4d db4e db4f db50 db51 db52 db53 db54 db55 db56 db57 db58 db59 db5a db5b db5c db5d db5e db5f db60 db61 db62 db63 db64 db65 db66 db67 db68 db69 db6a db6b db6c db6d db6e db6f db70 db71 db72 db73 db74 db75 db76 db77 db78 db79 db7a db7b db7c db7d db7e db7f db80 db81 db82 db83 db84 db85 db86 db87 db88 db89 db8a db8b db8c db8d db8e db8f db90 db91 db92 db93 db94 db95 db96 db97 db98 db99 db9a db9b db9c db9d db9e db9f dba0 dba1 dba2 dba3 dba4 dba5 dba6 dba7 dba8 dba9 dbaa dbab dbac dbad dbae dbaf dbb0 dbb1 dbb2 dbb3 dbb4 dbb5 dbb6 dbb7 dbb8 dbb9 dbba dbbb dbbc dbbd dbbe dbbf dbc0 dbc1 dbc2 dbc3 dbc4 dbc5 dbc6 dbc7 dbc8 dbc9 dbca dbcb dbcc dbcd dbce dbcf dbd0 dbd1 dbd2 dbd3 dbd4 dbd5 dbd6 dbd7 dbd8 dbd9 dbda dbdb dbdc dbdd dbde dbdf dbe0 dbe1 dbe2 dbe3 dbe4 dbe5 dbe6 dbe7 dbe8 dbe9 dbea dbeb dbec dbed dbee dbef dbf0 dbf1 dbf2 dbf3 dbf4 dbf5 dbf6 dbf7 dbf8 dbf9 dbfa dbfb dbfc dbfd dbfe dbff dc00 dc01 dc02 dc03 dc04 dc05 dc06 dc07 dc08 dc09 dc0a dc0b dc0c dc0d dc0e dc0f dc10 dc11 dc12 dc13 dc14 dc15 dc16 dc17 dc18 dc19 dc1a dc1b dc1c dc1d dc1e dc1f dc20 dc21 dc22 dc23 dc24 dc25 dc26 dc27 dc28 dc29 dc2a dc2b dc2c dc2d dc2e dc2f dc30 dc31 dc32 dc33 dc34 dc35 dc36 dc37 dc38 dc39 dc3a dc3b dc3c dc3d dc3e dc3f dc40 dc41 dc42 dc43 dc44 dc45 dc46 dc47 dc48 dc49 dc4a dc4b dc4c dc4d dc4e dc4f dc50 dc51 dc52 dc53 dc54 dc55 dc56 dc57 dc58 dc59 dc5a dc5b dc5c dc5d dc5e dc5f dc60 dc61 dc62 dc63 dc64 dc65 dc66 dc67 dc68 dc69 dc6a dc6b dc6c dc6d dc6e dc6f dc70 dc71 dc72 dc73 dc74 dc75 dc76 dc77 dc78 dc79 dc7a dc7b dc7c dc7d dc7e dc7f dc80 dc81 dc82 dc83 dc84 dc85 dc86 dc87 dc88 dc89 dc8a dc8b dc8c dc8d dc8e dc8f dc90 dc91 dc92 dc93 dc94 dc95 dc96 dc97 dc98 dc99 dc9a dc9b dc9c dc9d dc9e dc9f dca0 dca1 dca2 dca3 dca4 dca5 dca6 dca7 dca8 dca9 dcaa dcab dcac dcad dcae dcaf dcb0 dcb1 dcb2 dcb3 dcb4 dcb5 dcb6 dcb7 dcb8 dcb9 dcba dcbb dcbc dcbd dcbe dcbf dcc0 dcc1 dcc2 dcc3 dcc4 dcc5 dcc6 dcc7 dcc8 dcc9 dcca dccb dccc dccd dcce dccf dcd0 dcd1 dcd2 dcd3 dcd4 dcd5 dcd6 dcd7 dcd8 dcd9 dcda dcdb dcdc dcdd dcde dcdf dce0 dce1 dce2 dce3 dce4 dce5 dce6 dce7 dce8 dce9 dcea dceb dcec dced dcee dcef dcf0 dcf1 dcf2 dcf3 dcf4 dcf5 dcf6 dcf7 dcf8 dcf9 dcfa dcfb dcfc dcfd dcfe dcff dd00 dd01 dd02 dd03 dd04 dd05 dd06 dd07 dd08 dd09 dd0a dd0b dd0c dd0d dd0e dd0f dd10 dd11 dd12 dd13 dd14 dd15 dd16 dd17 dd18 dd19 dd1a dd1b dd1c dd1d dd1e dd1f dd20 dd21 dd22 dd23 dd24 dd25 dd26 dd27 dd28 dd29 dd2a dd2b dd2c dd2d dd2e dd2f dd30 dd31 dd32 dd33 dd34 dd35 dd36 dd37 dd38 dd39 dd3a dd3b dd3c dd3d dd3e dd3f dd40 dd41 dd42 dd43 dd44 dd45 dd46 dd47 dd48 dd49 dd4a dd4b dd4c dd4d dd4e dd4f dd50 dd51 dd52 dd53 dd54 dd55 dd56 dd57 dd58 dd59 dd5a dd5b dd5c dd5d dd5e dd5f dd60 dd61 dd62 dd63 dd64 dd65 dd66 dd67 dd68 dd69 dd6a dd6b dd6c dd6d dd6e dd6f dd70 dd71 dd72 dd73 dd74 dd75 dd76 dd77 dd78 dd79 dd7a dd7b dd7c dd7d dd7e dd7f dd80 dd81 dd82 dd83 dd84 dd85 dd86 dd87 dd88 dd89 dd8a dd8b dd8c dd8d dd8e dd8f dd90 dd91 dd92 dd93 dd94 dd95 dd96 dd97 dd98 dd99 dd9a dd9b dd9c dd9d dd9e dd9f dda0 dda1 dda2 dda3 dda4 dda5 dda6 dda7 dda8 dda9 ddaa ddab ddac ddad ddae ddaf ddb0 ddb1 ddb2 ddb3 ddb4 ddb5 ddb6 ddb7 ddb8 ddb9 ddba ddbb ddbc ddbd ddbe ddbf ddc0 ddc1 ddc2 ddc3 ddc4 ddc5 ddc6 ddc7 ddc8 ddc9 ddca ddcb ddcc ddcd ddce ddcf ddd0 ddd1 ddd2 ddd3 ddd4 ddd5 ddd6 ddd7 ddd8 ddd9 ddda dddb dddc dddd ddde dddf dde0 dde1 dde2 dde3 dde4 dde5 dde6 dde7 dde8 dde9 ddea ddeb ddec dded ddee ddef ddf0 ddf1 ddf2 ddf3 ddf4 ddf5 ddf6 ddf7 ddf8 ddf9 ddfa ddfb ddfc ddfd ddfe ddff de00 de01 de02 de03 de04 de05 de06 de07 de08 de09 de0a de0b de0c de0d de0e de0f de10 de11 de12 de13 de14 de15 de16 de17 de18 de19 de1a de1b de1c de1d de1e de1f de20 de21 de22 de23 de24 de25 de26 de27 de28 de29 de2a de2b de2c de2d de2e de2f de30 de31 de32 de33 de34 de35 de36 de37 de38 de39 de3a de3b de3c de3d de3e de3f de40 de41 de42 de43 de44 de45 de46 de47 de48 de49 de4a de4b de4c de4d de4e de4f de50 de51 de52 de53 de54 de55 de56 de57 de58 de59 de5a de5b de5c de5d de5e de5f de60 de61 de62 de63 de64 de65 de66 de67 de68 de69 de6a de6b de6c de6d de6e de6f de70 de71 de72 de73 de74 de75 de76 de77 de78 de79 de7a de7b de7c de7d de7e de7f de80 de81 de82 de83 de84 de85 de86 de87 de88 de89 de8a de8b de8c de8d de8e de8f de90 de91 de92 de93 de94 de95 de96 de97 de98 de99 de9a de9b de9c de9d de9e de9f dea0 dea1 dea2 dea3 dea4 dea5 dea6 dea7 dea8 dea9 deaa deab deac dead deae deaf deb0 deb1 deb2 deb3 deb4 deb5 deb6 deb7 deb8 deb9 deba debb debc debd debe debf dec0 dec1 dec2 dec3 dec4 dec5 dec6 dec7 dec8 dec9 deca decb decc decd dece decf ded0 ded1 ded2 ded3 ded4 ded5 ded6 ded7 ded8 ded9 deda dedb dedc dedd dede dedf dee0 dee1 dee2 dee3 dee4 dee5 dee6 dee7 dee8 dee9 deea deeb deec deed deee deef def0 def1 def2 def3 def4 def5 def6 def7 def8 def9 defa defb defc defd defe deff df00 df01 df02 df03 df04 df05 df06 df07 df08 df09 df0a df0b df0c df0d df0e df0f df10 df11 df12 df13 df14 df15 df16 df17 df18 df19 df1a df1b df1c df1d df1e df1f df20 df21 df22 df23 df24 df25 df26 df27 df28 df29 df2a df2b df2c df2d df2e df2f df30 df31 df32 df33 df34 df35 df36 df37 df38 df39 df3a df3b df3c df3d df3e df3f df40 df41 df42 df43 df44 df45 df46 df47 df48 df49 df4a df4b df4c df4d df4e df4f df50 df51 df52 df53 df54 df55 df56 df57 df58 df59 df5a df5b df5c df5d df5e df5f df60 df61 df62 df63 df64 df65 df66 df67 df68 df69 df6a df6b df6c df6d df6e df6f df70 df71 df72 df73 df74 df75 df76 df77 df78 df79 df7a df7b df7c df7d df7e df7f df80 df81 df82 df83 df84 df85 df86 df87 df88 df89 df8a df8b df8c df8d df8e df8f df90 df91 df92 df93 df94 df95 df96 df97 df98 df99 df9a df9b df9c df9d df9e df9f dfa0 dfa1 dfa2 dfa3 dfa4 dfa5 dfa6 dfa7 dfa8 dfa9 dfaa dfab dfac dfad dfae dfaf dfb0 dfb1 dfb2 dfb3 dfb4 dfb5 dfb6 dfb7 dfb8 dfb9 dfba dfbb dfbc dfbd dfbe dfbf dfc0 dfc1 dfc2 dfc3 dfc4 dfc5 dfc6 dfc7 dfc8 dfc9 dfca dfcb dfcc dfcd dfce dfcf dfd0 dfd1 dfd2 dfd3 dfd4 dfd5 dfd6 dfd7 dfd8 dfd9 dfda dfdb dfdc dfdd dfde dfdf dfe0 dfe1 dfe2 dfe3 dfe4 dfe5 dfe6 dfe7 dfe8 dfe9 dfea dfeb dfec dfed dfee dfef dff0 dff1 dff2 dff3 dff4 dff5 dff6 dff7 dff8 dff9 dffa dffb dffc dffd dffe dfff found 2048 chars.";

		string symbolChars = "24 2b 3c 3d 3e 5e 60 7c 7e a2 a3 a4 a5 a6 a7 a8 a9 ac ae af b0 b1 b4 b6 b8 d7 f7 2b9 2ba 2c2 2c3 2c4 2c5 2c6 2c7 2c8 2c9 2ca 2cb 2cc 2cd 2ce 2cf 2d2 2d3 2d4 2d5 2d6 2d7 2d8 2d9 2da 2db 2dc 2dd 2de 2df 2e5 2e6 2e7 2e8 2e9 2ea 2eb 2ec 2ed 374 375 384 385 482 6e9 6fd 6fe 9f2 9f3 9fa b70 e3f f01 f02 f03 f13 f14 f15 f16 f17 f1a f1b f1c f1d f1e f1f f34 f36 f38 fbe fbf fc0 fc1 fc2 fc3 fc4 fc5 fc7 fc8 fc9 fca fcb fcc fcf 17db 1fbd 1fbf 1fc0 1fc1 1fcd 1fce 1fcf 1fdd 1fde 1fdf 1fed 1fee 1fef 1ffd 1ffe 2044 207a 207b 207c 208a 208b 208c 20a0 20a1 20a2 20a3 20a4 20a5 20a6 20a7 20a8 20a9 20aa 20ab 20ac 20ad 20ae 20af 2100 2101 2103 2104 2105 2106 2108 2109 2114 2116 2117 2118 211e 211f 2120 2121 2122 2123 2125 2127 2129 212e 2132 213a 2190 2191 2192 2193 2194 2195 2196 2197 2198 2199 219a 219b 219c 219d 219e 219f 21a0 21a1 21a2 21a3 21a4 21a5 21a6 21a7 21a8 21a9 21aa 21ab 21ac 21ad 21ae 21af 21b0 21b1 21b2 21b3 21b4 21b5 21b6 21b7 21b8 21b9 21ba 21bb 21bc 21bd 21be 21bf 21c0 21c1 21c2 21c3 21c4 21c5 21c6 21c7 21c8 21c9 21ca 21cb 21cc 21cd 21ce 21cf 21d0 21d1 21d2 21d3 21d4 21d5 21d6 21d7 21d8 21d9 21da 21db 21dc 21dd 21de 21df 21e0 21e1 21e2 21e3 21e4 21e5 21e6 21e7 21e8 21e9 21ea 21eb 21ec 21ed 21ee 21ef 21f0 21f1 21f2 21f3 2200 2201 2202 2203 2204 2205 2206 2207 2208 2209 220a 220b 220c 220d 220e 220f 2210 2211 2212 2213 2214 2215 2216 2217 2218 2219 221a 221b 221c 221d 221e 221f 2220 2221 2222 2223 2224 2225 2226 2227 2228 2229 222a 222b 222c 222d 222e 222f 2230 2231 2232 2233 2234 2235 2236 2237 2238 2239 223a 223b 223c 223d 223e 223f 2240 2241 2242 2243 2244 2245 2246 2247 2248 2249 224a 224b 224c 224d 224e 224f 2250 2251 2252 2253 2254 2255 2256 2257 2258 2259 225a 225b 225c 225d 225e 225f 2260 2261 2262 2263 2264 2265 2266 2267 2268 2269 226a 226b 226c 226d 226e 226f 2270 2271 2272 2273 2274 2275 2276 2277 2278 2279 227a 227b 227c 227d 227e 227f 2280 2281 2282 2283 2284 2285 2286 2287 2288 2289 228a 228b 228c 228d 228e 228f 2290 2291 2292 2293 2294 2295 2296 2297 2298 2299 229a 229b 229c 229d 229e 229f 22a0 22a1 22a2 22a3 22a4 22a5 22a6 22a7 22a8 22a9 22aa 22ab 22ac 22ad 22ae 22af 22b0 22b1 22b2 22b3 22b4 22b5 22b6 22b7 22b8 22b9 22ba 22bb 22bc 22bd 22be 22bf 22c0 22c1 22c2 22c3 22c4 22c5 22c6 22c7 22c8 22c9 22ca 22cb 22cc 22cd 22ce 22cf 22d0 22d1 22d2 22d3 22d4 22d5 22d6 22d7 22d8 22d9 22da 22db 22dc 22dd 22de 22df 22e0 22e1 22e2 22e3 22e4 22e5 22e6 22e7 22e8 22e9 22ea 22eb 22ec 22ed 22ee 22ef 22f0 22f1 2300 2301 2302 2303 2304 2305 2306 2307 2308 2309 230a 230b 230c 230d 230e 230f 2310 2311 2312 2313 2314 2315 2316 2317 2318 2319 231a 231b 231c 231d 231e 231f 2320 2321 2322 2323 2324 2325 2326 2327 2328 232b 232c 232d 232e 232f 2330 2331 2332 2333 2334 2335 2336 2337 2338 2339 233a 233b 233c 233d 233e 233f 2340 2341 2342 2343 2344 2345 2346 2347 2348 2349 234a 234b 234c 234d 234e 234f 2350 2351 2352 2353 2354 2355 2356 2357 2358 2359 235a 235b 235c 235d 235e 235f 2360 2361 2362 2363 2364 2365 2366 2367 2368 2369 236a 236b 236c 236d 236e 236f 2370 2371 2372 2373 2374 2375 2376 2377 2378 2379 237a 237b 237d 237e 237f 2380 2381 2382 2383 2384 2385 2386 2387 2388 2389 238a 238b 238c 238d 238e 238f 2390 2391 2392 2393 2394 2395 2396 2397 2398 2399 239a 2400 2401 2402 2403 2404 2405 2406 2407 2408 2409 240a 240b 240c 240d 240e 240f 2410 2411 2412 2413 2414 2415 2416 2417 2418 2419 241a 241b 241c 241d 241e 241f 2420 2421 2422 2423 2424 2425 2426 2440 2441 2442 2443 2444 2445 2446 2447 2448 2449 244a 249c 249d 249e 249f 24a0 24a1 24a2 24a3 24a4 24a5 24a6 24a7 24a8 24a9 24aa 24ab 24ac 24ad 24ae 24af 24b0 24b1 24b2 24b3 24b4 24b5 24b6 24b7 24b8 24b9 24ba 24bb 24bc 24bd 24be 24bf 24c0 24c1 24c2 24c3 24c4 24c5 24c6 24c7 24c8 24c9 24ca 24cb 24cc 24cd 24ce 24cf 24d0 24d1 24d2 24d3 24d4 24d5 24d6 24d7 24d8 24d9 24da 24db 24dc 24dd 24de 24df 24e0 24e1 24e2 24e3 24e4 24e5 24e6 24e7 24e8 24e9 2500 2501 2502 2503 2504 2505 2506 2507 2508 2509 250a 250b 250c 250d 250e 250f 2510 2511 2512 2513 2514 2515 2516 2517 2518 2519 251a 251b 251c 251d 251e 251f 2520 2521 2522 2523 2524 2525 2526 2527 2528 2529 252a 252b 252c 252d 252e 252f 2530 2531 2532 2533 2534 2535 2536 2537 2538 2539 253a 253b 253c 253d 253e 253f 2540 2541 2542 2543 2544 2545 2546 2547 2548 2549 254a 254b 254c 254d 254e 254f 2550 2551 2552 2553 2554 2555 2556 2557 2558 2559 255a 255b 255c 255d 255e 255f 2560 2561 2562 2563 2564 2565 2566 2567 2568 2569 256a 256b 256c 256d 256e 256f 2570 2571 2572 2573 2574 2575 2576 2577 2578 2579 257a 257b 257c 257d 257e 257f 2580 2581 2582 2583 2584 2585 2586 2587 2588 2589 258a 258b 258c 258d 258e 258f 2590 2591 2592 2593 2594 2595 25a0 25a1 25a2 25a3 25a4 25a5 25a6 25a7 25a8 25a9 25aa 25ab 25ac 25ad 25ae 25af 25b0 25b1 25b2 25b3 25b4 25b5 25b6 25b7 25b8 25b9 25ba 25bb 25bc 25bd 25be 25bf 25c0 25c1 25c2 25c3 25c4 25c5 25c6 25c7 25c8 25c9 25ca 25cb 25cc 25cd 25ce 25cf 25d0 25d1 25d2 25d3 25d4 25d5 25d6 25d7 25d8 25d9 25da 25db 25dc 25dd 25de 25df 25e0 25e1 25e2 25e3 25e4 25e5 25e6 25e7 25e8 25e9 25ea 25eb 25ec 25ed 25ee 25ef 25f0 25f1 25f2 25f3 25f4 25f5 25f6 25f7 2600 2601 2602 2603 2604 2605 2606 2607 2608 2609 260a 260b 260c 260d 260e 260f 2610 2611 2612 2613 2619 261a 261b 261c 261d 261e 261f 2620 2621 2622 2623 2624 2625 2626 2627 2628 2629 262a 262b 262c 262d 262e 262f 2630 2631 2632 2633 2634 2635 2636 2637 2638 2639 263a 263b 263c 263d 263e 263f 2640 2641 2642 2643 2644 2645 2646 2647 2648 2649 264a 264b 264c 264d 264e 264f 2650 2651 2652 2653 2654 2655 2656 2657 2658 2659 265a 265b 265c 265d 265e 265f 2660 2661 2662 2663 2664 2665 2666 2667 2668 2669 266a 266b 266c 266d 266e 266f 2670 2671 2701 2702 2703 2704 2706 2707 2708 2709 270c 270d 270e 270f 2710 2711 2712 2713 2714 2715 2716 2717 2718 2719 271a 271b 271c 271d 271e 271f 2720 2721 2722 2723 2724 2725 2726 2727 2729 272a 272b 272c 272d 272e 272f 2730 2731 2732 2733 2734 2735 2736 2737 2738 2739 273a 273b 273c 273d 273e 273f 2740 2741 2742 2743 2744 2745 2746 2747 2748 2749 274a 274b 274d 274f 2750 2751 2752 2756 2758 2759 275a 275b 275c 275d 275e 2761 2762 2763 2764 2765 2766 2767 2794 2798 2799 279a 279b 279c 279d 279e 279f 27a0 27a1 27a2 27a3 27a4 27a5 27a6 27a7 27a8 27a9 27aa 27ab 27ac 27ad 27ae 27af 27b1 27b2 27b3 27b4 27b5 27b6 27b7 27b8 27b9 27ba 27bb 27bc 27bd 27be 2800 2801 2802 2803 2804 2805 2806 2807 2808 2809 280a 280b 280c 280d 280e 280f 2810 2811 2812 2813 2814 2815 2816 2817 2818 2819 281a 281b 281c 281d 281e 281f 2820 2821 2822 2823 2824 2825 2826 2827 2828 2829 282a 282b 282c 282d 282e 282f 2830 2831 2832 2833 2834 2835 2836 2837 2838 2839 283a 283b 283c 283d 283e 283f 2840 2841 2842 2843 2844 2845 2846 2847 2848 2849 284a 284b 284c 284d 284e 284f 2850 2851 2852 2853 2854 2855 2856 2857 2858 2859 285a 285b 285c 285d 285e 285f 2860 2861 2862 2863 2864 2865 2866 2867 2868 2869 286a 286b 286c 286d 286e 286f 2870 2871 2872 2873 2874 2875 2876 2877 2878 2879 287a 287b 287c 287d 287e 287f 2880 2881 2882 2883 2884 2885 2886 2887 2888 2889 288a 288b 288c 288d 288e 288f 2890 2891 2892 2893 2894 2895 2896 2897 2898 2899 289a 289b 289c 289d 289e 289f 28a0 28a1 28a2 28a3 28a4 28a5 28a6 28a7 28a8 28a9 28aa 28ab 28ac 28ad 28ae 28af 28b0 28b1 28b2 28b3 28b4 28b5 28b6 28b7 28b8 28b9 28ba 28bb 28bc 28bd 28be 28bf 28c0 28c1 28c2 28c3 28c4 28c5 28c6 28c7 28c8 28c9 28ca 28cb 28cc 28cd 28ce 28cf 28d0 28d1 28d2 28d3 28d4 28d5 28d6 28d7 28d8 28d9 28da 28db 28dc 28dd 28de 28df 28e0 28e1 28e2 28e3 28e4 28e5 28e6 28e7 28e8 28e9 28ea 28eb 28ec 28ed 28ee 28ef 28f0 28f1 28f2 28f3 28f4 28f5 28f6 28f7 28f8 28f9 28fa 28fb 28fc 28fd 28fe 28ff 2e80 2e81 2e82 2e83 2e84 2e85 2e86 2e87 2e88 2e89 2e8a 2e8b 2e8c 2e8d 2e8e 2e8f 2e90 2e91 2e92 2e93 2e94 2e95 2e96 2e97 2e98 2e99 2e9b 2e9c 2e9d 2e9e 2e9f 2ea0 2ea1 2ea2 2ea3 2ea4 2ea5 2ea6 2ea7 2ea8 2ea9 2eaa 2eab 2eac 2ead 2eae 2eaf 2eb0 2eb1 2eb2 2eb3 2eb4 2eb5 2eb6 2eb7 2eb8 2eb9 2eba 2ebb 2ebc 2ebd 2ebe 2ebf 2ec0 2ec1 2ec2 2ec3 2ec4 2ec5 2ec6 2ec7 2ec8 2ec9 2eca 2ecb 2ecc 2ecd 2ece 2ecf 2ed0 2ed1 2ed2 2ed3 2ed4 2ed5 2ed6 2ed7 2ed8 2ed9 2eda 2edb 2edc 2edd 2ede 2edf 2ee0 2ee1 2ee2 2ee3 2ee4 2ee5 2ee6 2ee7 2ee8 2ee9 2eea 2eeb 2eec 2eed 2eee 2eef 2ef0 2ef1 2ef2 2ef3 2f00 2f01 2f02 2f03 2f04 2f05 2f06 2f07 2f08 2f09 2f0a 2f0b 2f0c 2f0d 2f0e 2f0f 2f10 2f11 2f12 2f13 2f14 2f15 2f16 2f17 2f18 2f19 2f1a 2f1b 2f1c 2f1d 2f1e 2f1f 2f20 2f21 2f22 2f23 2f24 2f25 2f26 2f27 2f28 2f29 2f2a 2f2b 2f2c 2f2d 2f2e 2f2f 2f30 2f31 2f32 2f33 2f34 2f35 2f36 2f37 2f38 2f39 2f3a 2f3b 2f3c 2f3d 2f3e 2f3f 2f40 2f41 2f42 2f43 2f44 2f45 2f46 2f47 2f48 2f49 2f4a 2f4b 2f4c 2f4d 2f4e 2f4f 2f50 2f51 2f52 2f53 2f54 2f55 2f56 2f57 2f58 2f59 2f5a 2f5b 2f5c 2f5d 2f5e 2f5f 2f60 2f61 2f62 2f63 2f64 2f65 2f66 2f67 2f68 2f69 2f6a 2f6b 2f6c 2f6d 2f6e 2f6f 2f70 2f71 2f72 2f73 2f74 2f75 2f76 2f77 2f78 2f79 2f7a 2f7b 2f7c 2f7d 2f7e 2f7f 2f80 2f81 2f82 2f83 2f84 2f85 2f86 2f87 2f88 2f89 2f8a 2f8b 2f8c 2f8d 2f8e 2f8f 2f90 2f91 2f92 2f93 2f94 2f95 2f96 2f97 2f98 2f99 2f9a 2f9b 2f9c 2f9d 2f9e 2f9f 2fa0 2fa1 2fa2 2fa3 2fa4 2fa5 2fa6 2fa7 2fa8 2fa9 2faa 2fab 2fac 2fad 2fae 2faf 2fb0 2fb1 2fb2 2fb3 2fb4 2fb5 2fb6 2fb7 2fb8 2fb9 2fba 2fbb 2fbc 2fbd 2fbe 2fbf 2fc0 2fc1 2fc2 2fc3 2fc4 2fc5 2fc6 2fc7 2fc8 2fc9 2fca 2fcb 2fcc 2fcd 2fce 2fcf 2fd0 2fd1 2fd2 2fd3 2fd4 2fd5 2ff0 2ff1 2ff2 2ff3 2ff4 2ff5 2ff6 2ff7 2ff8 2ff9 2ffa 2ffb 3004 3012 3013 3020 3036 3037 303e 303f 309b 309c 3190 3191 3196 3197 3198 3199 319a 319b 319c 319d 319e 319f 3200 3201 3202 3203 3204 3205 3206 3207 3208 3209 320a 320b 320c 320d 320e 320f 3210 3211 3212 3213 3214 3215 3216 3217 3218 3219 321a 321b 321c 322a 322b 322c 322d 322e 322f 3230 3231 3232 3233 3234 3235 3236 3237 3238 3239 323a 323b 323c 323d 323e 323f 3240 3241 3242 3243 3260 3261 3262 3263 3264 3265 3266 3267 3268 3269 326a 326b 326c 326d 326e 326f 3270 3271 3272 3273 3274 3275 3276 3277 3278 3279 327a 327b 327f 328a 328b 328c 328d 328e 328f 3290 3291 3292 3293 3294 3295 3296 3297 3298 3299 329a 329b 329c 329d 329e 329f 32a0 32a1 32a2 32a3 32a4 32a5 32a6 32a7 32a8 32a9 32aa 32ab 32ac 32ad 32ae 32af 32b0 32c0 32c1 32c2 32c3 32c4 32c5 32c6 32c7 32c8 32c9 32ca 32cb 32d0 32d1 32d2 32d3 32d4 32d5 32d6 32d7 32d8 32d9 32da 32db 32dc 32dd 32de 32df 32e0 32e1 32e2 32e3 32e4 32e5 32e6 32e7 32e8 32e9 32ea 32eb 32ec 32ed 32ee 32ef 32f0 32f1 32f2 32f3 32f4 32f5 32f6 32f7 32f8 32f9 32fa 32fb 32fc 32fd 32fe 3300 3301 3302 3303 3304 3305 3306 3307 3308 3309 330a 330b 330c 330d 330e 330f 3310 3311 3312 3313 3314 3315 3316 3317 3318 3319 331a 331b 331c 331d 331e 331f 3320 3321 3322 3323 3324 3325 3326 3327 3328 3329 332a 332b 332c 332d 332e 332f 3330 3331 3332 3333 3334 3335 3336 3337 3338 3339 333a 333b 333c 333d 333e 333f 3340 3341 3342 3343 3344 3345 3346 3347 3348 3349 334a 334b 334c 334d 334e 334f 3350 3351 3352 3353 3354 3355 3356 3357 3358 3359 335a 335b 335c 335d 335e 335f 3360 3361 3362 3363 3364 3365 3366 3367 3368 3369 336a 336b 336c 336d 336e 336f 3370 3371 3372 3373 3374 3375 3376 337b 337c 337d 337e 337f 3380 3381 3382 3383 3384 3385 3386 3387 3388 3389 338a 338b 338c 338d 338e 338f 3390 3391 3392 3393 3394 3395 3396 3397 3398 3399 339a 339b 339c 339d 339e 339f 33a0 33a1 33a2 33a3 33a4 33a5 33a6 33a7 33a8 33a9 33aa 33ab 33ac 33ad 33ae 33af 33b0 33b1 33b2 33b3 33b4 33b5 33b6 33b7 33b8 33b9 33ba 33bb 33bc 33bd 33be 33bf 33c0 33c1 33c2 33c3 33c4 33c5 33c6 33c7 33c8 33c9 33ca 33cb 33cc 33cd 33ce 33cf 33d0 33d1 33d2 33d3 33d4 33d5 33d6 33d7 33d8 33d9 33da 33db 33dc 33dd 33e0 33e1 33e2 33e3 33e4 33e5 33e6 33e7 33e8 33e9 33ea 33eb 33ec 33ed 33ee 33ef 33f0 33f1 33f2 33f3 33f4 33f5 33f6 33f7 33f8 33f9 33fa 33fb 33fc 33fd 33fe a490 a491 a492 a493 a494 a495 a496 a497 a498 a499 a49a a49b a49c a49d a49e a49f a4a0 a4a1 a4a4 a4a5 a4a6 a4a7 a4a8 a4a9 a4aa a4ab a4ac a4ad a4ae a4af a4b0 a4b1 a4b2 a4b3 a4b5 a4b6 a4b7 a4b8 a4b9 a4ba a4bb a4bc a4bd a4be a4bf a4c0 a4c2 a4c3 a4c4 a4c6 fb29 fe62 fe64 fe65 fe66 fe69 ff04 ff0b ff1c ff1d ff1e ff3e ff40 ff5c ff5e ffe0 ffe1 ffe2 ffe3 ffe4 ffe5 ffe6 ffe8 ffe9 ffea ffeb ffec ffed ffee fffc fffd found 2404 chars.";

		string upperChars = "41 42 43 44 45 46 47 48 49 4a 4b 4c 4d 4e 4f 50 51 52 53 54 55 56 57 58 59 5a c0 c1 c2 c3 c4 c5 c6 c7 c8 c9 ca cb cc cd ce cf d0 d1 d2 d3 d4 d5 d6 d8 d9 da db dc dd de 100 102 104 106 108 10a 10c 10e 110 112 114 116 118 11a 11c 11e 120 122 124 126 128 12a 12c 12e 130 132 134 136 139 13b 13d 13f 141 143 145 147 14a 14c 14e 150 152 154 156 158 15a 15c 15e 160 162 164 166 168 16a 16c 16e 170 172 174 176 178 179 17b 17d 181 182 184 186 187 189 18a 18b 18e 18f 190 191 193 194 196 197 198 19c 19d 19f 1a0 1a2 1a4 1a6 1a7 1a9 1ac 1ae 1af 1b1 1b2 1b3 1b5 1b7 1b8 1bc 1c4 1c7 1ca 1cd 1cf 1d1 1d3 1d5 1d7 1d9 1db 1de 1e0 1e2 1e4 1e6 1e8 1ea 1ec 1ee 1f1 1f4 1f6 1f7 1f8 1fa 1fc 1fe 200 202 204 206 208 20a 20c 20e 210 212 214 216 218 21a 21c 21e 222 224 226 228 22a 22c 22e 230 232 386 388 389 38a 38c 38e 38f 391 392 393 394 395 396 397 398 399 39a 39b 39c 39d 39e 39f 3a0 3a1 3a3 3a4 3a5 3a6 3a7 3a8 3a9 3aa 3ab 3d2 3d3 3d4 3da 3dc 3de 3e0 3e2 3e4 3e6 3e8 3ea 3ec 3ee 400 401 402 403 404 405 406 407 408 409 40a 40b 40c 40d 40e 40f 410 411 412 413 414 415 416 417 418 419 41a 41b 41c 41d 41e 41f 420 421 422 423 424 425 426 427 428 429 42a 42b 42c 42d 42e 42f 460 462 464 466 468 46a 46c 46e 470 472 474 476 478 47a 47c 47e 480 48c 48e 490 492 494 496 498 49a 49c 49e 4a0 4a2 4a4 4a6 4a8 4aa 4ac 4ae 4b0 4b2 4b4 4b6 4b8 4ba 4bc 4be 4c0 4c1 4c3 4c7 4cb 4d0 4d2 4d4 4d6 4d8 4da 4dc 4de 4e0 4e2 4e4 4e6 4e8 4ea 4ec 4ee 4f0 4f2 4f4 4f8 531 532 533 534 535 536 537 538 539 53a 53b 53c 53d 53e 53f 540 541 542 543 544 545 546 547 548 549 54a 54b 54c 54d 54e 54f 550 551 552 553 554 555 556 10a0 10a1 10a2 10a3 10a4 10a5 10a6 10a7 10a8 10a9 10aa 10ab 10ac 10ad 10ae 10af 10b0 10b1 10b2 10b3 10b4 10b5 10b6 10b7 10b8 10b9 10ba 10bb 10bc 10bd 10be 10bf 10c0 10c1 10c2 10c3 10c4 10c5 1e00 1e02 1e04 1e06 1e08 1e0a 1e0c 1e0e 1e10 1e12 1e14 1e16 1e18 1e1a 1e1c 1e1e 1e20 1e22 1e24 1e26 1e28 1e2a 1e2c 1e2e 1e30 1e32 1e34 1e36 1e38 1e3a 1e3c 1e3e 1e40 1e42 1e44 1e46 1e48 1e4a 1e4c 1e4e 1e50 1e52 1e54 1e56 1e58 1e5a 1e5c 1e5e 1e60 1e62 1e64 1e66 1e68 1e6a 1e6c 1e6e 1e70 1e72 1e74 1e76 1e78 1e7a 1e7c 1e7e 1e80 1e82 1e84 1e86 1e88 1e8a 1e8c 1e8e 1e90 1e92 1e94 1ea0 1ea2 1ea4 1ea6 1ea8 1eaa 1eac 1eae 1eb0 1eb2 1eb4 1eb6 1eb8 1eba 1ebc 1ebe 1ec0 1ec2 1ec4 1ec6 1ec8 1eca 1ecc 1ece 1ed0 1ed2 1ed4 1ed6 1ed8 1eda 1edc 1ede 1ee0 1ee2 1ee4 1ee6 1ee8 1eea 1eec 1eee 1ef0 1ef2 1ef4 1ef6 1ef8 1f08 1f09 1f0a 1f0b 1f0c 1f0d 1f0e 1f0f 1f18 1f19 1f1a 1f1b 1f1c 1f1d 1f28 1f29 1f2a 1f2b 1f2c 1f2d 1f2e 1f2f 1f38 1f39 1f3a 1f3b 1f3c 1f3d 1f3e 1f3f 1f48 1f49 1f4a 1f4b 1f4c 1f4d 1f59 1f5b 1f5d 1f5f 1f68 1f69 1f6a 1f6b 1f6c 1f6d 1f6e 1f6f 1fb8 1fb9 1fba 1fbb 1fc8 1fc9 1fca 1fcb 1fd8 1fd9 1fda 1fdb 1fe8 1fe9 1fea 1feb 1fec 1ff8 1ff9 1ffa 1ffb 2102 2107 210b 210c 210d 2110 2111 2112 2115 2119 211a 211b 211c 211d 2124 2126 2128 212a 212b 212c 212d 2130 2131 2133 ff21 ff22 ff23 ff24 ff25 ff26 ff27 ff28 ff29 ff2a ff2b ff2c ff2d ff2e ff2f ff30 ff31 ff32 ff33 ff34 ff35 ff36 ff37 ff38 ff39 ff3a found 686 chars.";

		string whitespaceChars = "9 a b c d 20 85 a0 1680 2000 2001 2002 2003 2004 2005 2006 2007 2008 2009 200a 200b 2028 2029 202f 3000 found 25 chars.";

		string letters = "";
		string letterOrDigits = "";
	}

	/*

// Usage:
//	If you want to re-generate dump.XXX.txt in your box, run this program
//	*under MS.NET*. The resulting text files will be used in corlib nunit
//	test.
//
//	BE CAREFUL: if you ran compile and ran this program *under Mono* and
//	overwrote them, it might break the purpose of the test!!
//

	public static void Main()
	{
		int total = 0;

		for (int i = 0; i < 65536; i++) {
//			if (Char.IsWhiteSpace ((char) i))
//			if (Char.IsSeparator ((char) i))
//			if (Char.IsDigit ((char) i))
//			if (Char.IsNumber ((char) i))
//			if (Char.IsControl ((char) i))
//			if (Char.IsLetter ((char) i))
			if (Char.IsLetterOrDigit ((char) i))
//			if (Char.IsLower ((char) i))
//			if (Char.IsPunctuation ((char) i))
//			if (Char.IsSurrogate ((char) i))
//			if (Char.IsSymbol ((char) i))
//			if (Char.IsUpper ((char) i))
			{
				Console.Write (i.ToString ("x"));
				Console.Write (' ');
				total++;
			}
		}

		Console.Write ("found " + total + " chars.");
	}
	*/

}
