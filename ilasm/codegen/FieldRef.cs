//
// Mono.ILASM.FieldRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {


        public class FieldRef : IFieldRef {

                private TypeRef owner;
                private ITypeRef ret_type;
                private string name;

                private PEAPI.Field peapi_field;

                public FieldRef (TypeRef owner, ITypeRef ret_type, string name)
                {
                        this.owner = owner;
                        this.ret_type = ret_type;
                        this.name = name;
                }

                public PEAPI.Field PeapiField {
                        get { return peapi_field; }
                }

                public void Resolve (CodeGen code_gen)
                {
                        TypeDef owner_def = code_gen.TypeManager[owner.FullName];
                        peapi_field = owner_def.ResolveField (name, code_gen);
                }
        }
}

