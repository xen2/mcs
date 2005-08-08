/* -*- Mode: C++; tab-width: 2; indent-tabs-mode: nil; c-basic-offset: 2 -*- */
/* ***** BEGIN LICENSE BLOCK *****
 * Version: MPL 1.1/GPL 2.0/LGPL 2.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is Mozilla Communicator client code, released
 * March 31, 1998.
 *
 * The Initial Developer of the Original Code is
 * Netscape Communications Corporation.
 * Portions created by the Initial Developer are Copyright (C) 1998
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *
 * Alternatively, the contents of this file may be used under the terms of
 * either the GNU General Public License Version 2 or later (the "GPL"), or
 * the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
 * in which case the provisions of the GPL or the LGPL are applicable instead
 * of those above. If you wish to allow use of your version of this file only
 * under the terms of either the GPL or the LGPL, and not to allow others to
 * use your version of this file under the terms of the MPL, indicate your
 * decision by deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL or the LGPL. If you do not delete
 * the provisions above, a recipient may use your version of this file under
 * the terms of any one of the MPL, the GPL or the LGPL.
 *
 * ***** END LICENSE BLOCK ***** */

/**
   File Name:          15.8.2.7.js
   ECMA Section:       15.8.2.7 cos( x )
   Description:        return an approximation to the cosine of the
   argument.  argument is expressed in radians
   Author:             christine@netscape.com
   Date:               7 july 1997

*/

var SECTION = "15.8.2.7";
var VERSION = "ECMA_1";
startTest();
var TITLE   = "Math.cos(x)";

writeHeaderToLog( SECTION + " "+ TITLE);

new TestCase( SECTION,
	      "Math.cos.length",
	      1,
	      Math.cos.length );

new TestCase( SECTION,
	      "Math.cos()",
	      Number.NaN,
	      Math.cos() );

new TestCase( SECTION,
	      "Math.cos(void 0)",
	      Number.NaN,
	      Math.cos(void 0) );

new TestCase( SECTION,
	      "Math.cos(false)",
	      1,
	      Math.cos(false) );

new TestCase( SECTION,
	      "Math.cos(null)",
	      1,
	      Math.cos(null) );

new TestCase( SECTION,
	      "Math.cos('0')",
	      1,
	      Math.cos('0') );

new TestCase( SECTION,
	      "Math.cos('Infinity')",
	      Number.NaN,
	      Math.cos("Infinity") );

new TestCase( SECTION,
	      "Math.cos('3.14159265359')",
	      -1,
	      Math.cos('3.14159265359') );

new TestCase( SECTION,
	      "Math.cos(NaN)",
	      Number.NaN,
	      Math.cos(Number.NaN)        );

new TestCase( SECTION,
	      "Math.cos(0)",
	      1,
	      Math.cos(0)                 );

new TestCase( SECTION,
	      "Math.cos(-0)",
	      1,
	      Math.cos(-0)                );

new TestCase( SECTION,
	      "Math.cos(Infinity)",
	      Number.NaN,
	      Math.cos(Number.POSITIVE_INFINITY) );

new TestCase( SECTION,
	      "Math.cos(-Infinity)",
	      Number.NaN,
	      Math.cos(Number.NEGATIVE_INFINITY) );

new TestCase( SECTION,
	      "Math.cos(0.7853981633974)",
	      0.7071067811865,
	      Math.cos(0.7853981633974)   );

new TestCase( SECTION,
	      "Math.cos(1.570796326795)",
	      0,
	      Math.cos(1.570796326795)    );

new TestCase( SECTION,
	      "Math.cos(2.356194490192)",
	      -0.7071067811865,
	      Math.cos(2.356194490192)    );

new TestCase( SECTION,
	      "Math.cos(3.14159265359)",
	      -1,
	      Math.cos(3.14159265359)     );

new TestCase( SECTION,
	      "Math.cos(3.926990816987)",
	      -0.7071067811865,
	      Math.cos(3.926990816987)    );

new TestCase( SECTION,
	      "Math.cos(4.712388980385)",
	      0,
	      Math.cos(4.712388980385)    );

new TestCase( SECTION,
	      "Math.cos(5.497787143782)",
	      0.7071067811865,
	      Math.cos(5.497787143782)    );

new TestCase( SECTION,
	      "Math.cos(Math.PI*2)",
	      1,
	      Math.cos(Math.PI*2)         );

new TestCase( SECTION,
	      "Math.cos(Math.PI/4)",
	      Math.SQRT2/2,
	      Math.cos(Math.PI/4)         );

new TestCase( SECTION,
	      "Math.cos(Math.PI/2)",
	      0,
	      Math.cos(Math.PI/2)         );

new TestCase( SECTION,
	      "Math.cos(3*Math.PI/4)",
	      -Math.SQRT2/2,
	      Math.cos(3*Math.PI/4)       );

new TestCase( SECTION,
	      "Math.cos(Math.PI)",
	      -1,
	      Math.cos(Math.PI)           );

new TestCase( SECTION,
	      "Math.cos(5*Math.PI/4)",
	      -Math.SQRT2/2,
	      Math.cos(5*Math.PI/4)       );

new TestCase( SECTION,
	      "Math.cos(3*Math.PI/2)",
	      0,
	      Math.cos(3*Math.PI/2)       );

new TestCase( SECTION,
	      "Math.cos(7*Math.PI/4)",
	      Math.SQRT2/2,
	      Math.cos(7*Math.PI/4)       );

new TestCase( SECTION,
	      "Math.cos(Math.PI*2)",
	      1,
	      Math.cos(2*Math.PI)         );

new TestCase( SECTION,
	      "Math.cos(-0.7853981633974)",
	      0.7071067811865,
	      Math.cos(-0.7853981633974)  );

new TestCase( SECTION,
	      "Math.cos(-1.570796326795)",
	      0,
	      Math.cos(-1.570796326795)   );

new TestCase( SECTION,
	      "Math.cos(-2.3561944901920)",
	      -.7071067811865,
	      Math.cos(2.3561944901920)   );

new TestCase( SECTION,
	      "Math.cos(-3.14159265359)",
	      -1,
	      Math.cos(3.14159265359)     );

new TestCase( SECTION,
	      "Math.cos(-3.926990816987)",
	      -0.7071067811865,
	      Math.cos(3.926990816987)    );

new TestCase( SECTION,
	      "Math.cos(-4.712388980385)",
	      0,
	      Math.cos(4.712388980385)    );

new TestCase( SECTION,
	      "Math.cos(-5.497787143782)",
	      0.7071067811865,
	      Math.cos(5.497787143782)    );

new TestCase( SECTION,
	      "Math.cos(-6.28318530718)",
	      1,
	      Math.cos(6.28318530718)     );

new TestCase( SECTION,
	      "Math.cos(-Math.PI/4)",
	      Math.SQRT2/2,
	      Math.cos(-Math.PI/4)        );

new TestCase( SECTION,
	      "Math.cos(-Math.PI/2)",
	      0,
	      Math.cos(-Math.PI/2)        );

new TestCase( SECTION,
	      "Math.cos(-3*Math.PI/4)",
	      -Math.SQRT2/2,
	      Math.cos(-3*Math.PI/4)      );

new TestCase( SECTION,
	      "Math.cos(-Math.PI)",
	      -1,
	      Math.cos(-Math.PI)          );

new TestCase( SECTION,
	      "Math.cos(-5*Math.PI/4)",
	      -Math.SQRT2/2,
	      Math.cos(-5*Math.PI/4)      );

new TestCase( SECTION,
	      "Math.cos(-3*Math.PI/2)",
	      0,
	      Math.cos(-3*Math.PI/2)      );

new TestCase( SECTION,
	      "Math.cos(-7*Math.PI/4)",
	      Math.SQRT2/2,
	      Math.cos(-7*Math.PI/4)      );

new TestCase( SECTION,
	      "Math.cos(-Math.PI*2)",
	      1,
	      Math.cos(-Math.PI*2)        );

test();
