using System;
class T {
        int stuff () {
		try {
			throw new Exception ();
		} finally {
			stuff_finally ();
		}
        }
        int stuff2 () {
		try {
			throw new Exception ();
		} catch {
			try {
				throw new Exception ();
			} finally {
				stuff_finally ();
			}
		} finally {
			stuff_finally ();
		}
        }
        int stuff3 () {
		try {
			throw new Exception ();
		} catch {
			try {
				throw new Exception ();
			} finally {
				stuff_finally ();
			}
		} finally {
			stuff_finally ();
		}
        }
        void stuff_finally () {
        }
        static void Main() {
        }
}
