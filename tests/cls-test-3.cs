using System;
[assembly:CLSCompliant(true)]

public interface I1 {
}

public class CLSClass {
        protected internal I1 Foo() {
                return null;
        }
       
        static void Main() {}
}