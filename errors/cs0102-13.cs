// cs0102.cs: The class 'SampleClass' already contains a definition for 'op_Implicit'
// Line: 10

public class SampleClass {
    
        static public implicit operator SampleClass (byte value) {
               return new SampleClass();
        }
        
        public bool op_Implicit;
}