//
// Mono.ILASM.InstrTable
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using PEAPI;
using System;
using System.Collections;
 
namespace Mono.ILASM {

	public class InstrTable {

		private static Hashtable op_table;
		private static Hashtable int_table;		
		private static Hashtable type_table;
		private static Hashtable method_table;
		private static Hashtable field_table;
		private static Hashtable branch_table;

		static InstrTable ()
		{
			CreateOpTable ();
			CreateIntTable ();
			CreateTypeTable ();
			CreateMethodTable ();
			CreateFieldTable ();
			CreateBranchTable ();
		}
		
		public static ILToken GetToken (string str)
		{
			if (IsOp (str)) {
				Op op = GetOp (str);
				return new ILToken (Token.INSTR_NONE, op);
			} else if (IsIntOp (str)) {
				IntOp op = GetIntOp (str);
				return new ILToken (Token.INSTR_I, op);
			} else if (IsTypeOp (str)) {
				TypeOp op = GetTypeOp (str);
				return new ILToken (Token.INSTR_TYPE, op);
			} else if (IsMethodOp (str)) {
				MethodOp op = GetMethodOp (str);
				return new ILToken (Token.INSTR_METHOD, op);
			} else if (IsLdstrOp (str)) {
				return new ILToken (Token.INSTR_STRING, str);
			} else if (IsFieldOp (str)) {
				FieldOp op = GetFieldOp (str);
				return new ILToken (Token.INSTR_FIELD, op);
			} else if (IsBranchOp (str)) {
				BranchOp op = GetBranchOp (str);
				return new ILToken (Token.INSTR_BRTARGET, op);
			} else if (IsLdcr4 (str)) {
				return new ILToken (Token.INSTR_R, "");
			} else if (IsLdcr8 (str)) {
				return new ILToken (Token.INSTR_R, "");
			} else if (IsSwitch (str)) {
				return new ILToken (Token.INSTR_SWITCH, str);
			}

			return null;
		}

		public static bool IsInstr (string str)
		{
			return (IsOp (str) || IsIntOp (str) || 
				IsTypeOp (str) || IsMethodOp (str) ||
				IsLdstrOp (str) || IsFieldOp (str) ||
   				IsBranchOp (str) || IsLdcr4 (str) || IsLdcr8 (str) ||
				IsSwitch (str));
		}

		public static bool IsOp (string str)
		{
			return op_table.Contains (str);
		}
		
		public static bool IsIntOp (string str)
		{
			return int_table.Contains (str);
		}

		public static bool IsTypeOp (string str) 
		{
			return type_table.Contains (str);
		}

		public static bool IsMethodOp (string str) 
		{
			return method_table.Contains (str);
		}

		public static bool IsLdstrOp (string str)
		{
			return (str == "ldstr");
		}

		public static bool IsFieldOp (string str)
		{
			return field_table.Contains (str);
		}

		public static bool IsBranchOp (string str)
		{
			return branch_table.Contains (str);
		}

		public static bool IsLdcr4 (string str)
		{
			return "ldc.r4" == str;
		}

		public static bool IsLdcr8 (string str)
		{
			return "ldc.r8" == str;
		}

		public static bool IsSwitch (string str)
		{
			return "switch" == str;
		}

		public static Op GetOp (string str)
		{
			return (Op) op_table[str];
		}

		public static IntOp GetIntOp (string str)
		{
			return (IntOp) int_table[str];
		}

		public static TypeOp GetTypeOp (string str)
		{
			return (TypeOp) type_table[str];
		}

		public static MethodOp GetMethodOp (string str)
		{
			return (MethodOp) method_table[str];
		}

		public static FieldOp GetFieldOp (string str)
		{
			return (FieldOp) field_table[str];
		}

		public static BranchOp GetBranchOp (string str)
		{
			return (BranchOp) branch_table[str];
		}

		private static void CreateOpTable ()
		{
			op_table = new Hashtable ();

			op_table["nop"] = Op.nop;
			op_table["break"] = Op.breakOp;
			op_table["ldarg.0"] = Op.ldarg_0;
			op_table["ldarg.1"] = Op.ldarg_1;
			op_table["ldarg.2"] = Op.ldarg_2;
			op_table["ldarg.3"] = Op.ldarg_3;
			op_table["ldloc.0"] = Op.ldloc_0;
			op_table["ldloc.1"] = Op.ldloc_1;
			op_table["ldloc.2"] = Op.ldloc_2;
			op_table["ldloc.3"] = Op.ldloc_3;
			op_table["stloc.0"] = Op.stloc_0;
			op_table["stloc.1"] = Op.stloc_1;
			op_table["stloc.2"] = Op.stloc_2;
			op_table["stloc.3"] = Op.stloc_3;
			op_table["ldnull"] = Op.ldnull;
			op_table["ldc.i4.m1"] = Op.ldc_i4_m1;
			op_table["ldc.i4.0"] = Op.ldc_i4_0;
			op_table["ldc.i4.1"] = Op.ldc_i4_1;
			op_table["ldc.i4.2"] = Op.ldc_i4_2;
			op_table["ldc.i4.3"] = Op.ldc_i4_3;
			op_table["ldc.i4.4"] = Op.ldc_i4_4;
			op_table["ldc.i4.5"] = Op.ldc_i4_5;
			op_table["ldc.i4.6"] = Op.ldc_i4_6;
			op_table["ldc.i4.7"] = Op.ldc_i4_7;
			op_table["ldc.i4.8"] = Op.ldc_i4_8;
			op_table["dup"] = Op.dup;
			op_table["pop"] = Op.pop;
			op_table["ret"] = Op.ret;
			op_table["ldind.i1"] = Op.ldind_i1;
			op_table["ldind.u1"] = Op.ldind_u1;
			op_table["ldind.i2"] = Op.ldind_i2;
			op_table["ldind.u2"] = Op.ldind_u2;
			op_table["ldind.i4"] = Op.ldind_i4;
			op_table["ldind.u4"] = Op.ldind_u4;
			op_table["ldind.i8"] = Op.ldind_i8;
			op_table["ldind.i"] = Op.ldind_i;
			op_table["ldind.r4"] = Op.ldind_r4;
			op_table["ldind.r8"] = Op.ldind_r8;
			op_table["ldind.ref"] = Op.ldind_ref;
			op_table["stind.ref"] = Op.stind_ref;
			op_table["stind.i1"] = Op.stind_i1;
			op_table["stind.i2"] = Op.stind_i2;
			op_table["stind.i4"] = Op.stind_i4;
			op_table["stind.i8"] = Op.stind_i8;
			op_table["stind.r4"] = Op.stind_r4;
			op_table["stind.r8"] = Op.stind_r8;
			op_table["add"] = Op.add;
			op_table["sub"] = Op.sub;
			op_table["mul"] = Op.mul;
			op_table["div"] = Op.div;
			op_table["div.un"] = Op.div_un;
			op_table["rem"] = Op.rem;
			op_table["rem.un"] = Op.rem_un;
			op_table["and"] = Op.and;
			op_table["or"] = Op.or;
			op_table["xor"] = Op.xor;
			op_table["shl"] = Op.shl;
			op_table["shr"] = Op.shr;
			op_table["shr.un"] = Op.shr_un;
			op_table["neg"] = Op.neg;
			op_table["not"] = Op.not;
			op_table["conv.i1"] = Op.conv_i1;
			op_table["conv.i2"] = Op.conv_i2;
			op_table["conv.i4"] = Op.conv_i4;
			op_table["conv.i8"] = Op.conv_i8;
			op_table["conv.r4"] = Op.conv_r4;
			op_table["conv.r8"] = Op.conv_r8;
			op_table["conv.u4"] = Op.conv_u4;
			op_table["conv.u8"] = Op.conv_u8;
			op_table["conv.r.un"] = Op.conv_r_un;
			op_table["throw"] = Op.throwOp;
			op_table["conv.ovf.i1.un"] = Op.conv_ovf_i1_un;
			op_table["conv.ovf.i2.un"] = Op.conv_ovf_i2_un;
			op_table["conv.ovf.i4.un"] = Op.conv_ovf_i4_un;
			op_table["conv.ovf.i8.un"] = Op.conv_ovf_i8_un;
			op_table["conf.ovf.u1.un"] = Op.conf_ovf_u1_un;
			op_table["conv.ovf.u2.un"] = Op.conv_ovf_u2_un;
			op_table["conv.ovf.u4.un"] = Op.conv_ovf_u4_un;
			op_table["conv.ovf.u8.un"] = Op.conv_ovf_u8_un;
			op_table["conv.ovf.i.un"] = Op.conv_ovf_i_un;
			op_table["conv.ovf.u.un"] = Op.conv_ovf_u_un;
			op_table["ldlen"] = Op.ldlen;
			op_table["ldelem.i1"] = Op.ldelem_i1;
			op_table["ldelem.u1"] = Op.ldelem_u1;
			op_table["ldelem.i2"] = Op.ldelem_i2;
			op_table["ldelem.u2"] = Op.ldelem_u2;
			op_table["ldelem.i4"] = Op.ldelem_i4;
			op_table["ldelem.u4"] = Op.ldelem_u4;
			op_table["ldelem.i8"] = Op.ldelem_i8;
			op_table["ldelem.i"] = Op.ldelem_i;
			op_table["ldelem.r4"] = Op.ldelem_r4;
			op_table["ldelem.r8"] = Op.ldelem_r8;
			op_table["ldelem.ref"] = Op.ldelem_ref;
			op_table["stelem.i"] = Op.stelem_i;
			op_table["stelem.i1"] = Op.stelem_i1;
			op_table["stelem.i2"] = Op.stelem_i2;
			op_table["stelem.i4"] = Op.stelem_i4;
			op_table["stelem.i8"] = Op.stelem_i8;
			op_table["stelem.ref"] = Op.stelem_ref;
			op_table["conv.ovf.i1"] = Op.conv_ovf_i1;
			op_table["conv.ovf.u1"] = Op.conv_ovf_u1;
			op_table["conv.ovf.i2"] = Op.conv_ovf_i2;
			op_table["conv.ovf.u2"] = Op.conv_ovf_u2;
			op_table["conv.ovf.i4"] = Op.conv_ovf_i4;
			op_table["conv.ovf.u4"] = Op.conv_ovf_u4;
			op_table["conv.ovf.i8"] = Op.conv_ovf_i8;
			op_table["conv.ovf.u8"] = Op.conv_ovf_u8;
			op_table["ckfinite"] = Op.ckfinite;
			op_table["conv.u2"] = Op.conv_u2;
			op_table["conv.u1"] = Op.conv_u1;
			op_table["conv.i"] = Op.conv_i;
			op_table["conv.ovf.i"] = Op.conv_ovf_i;
			op_table["conv.ovf.u"] = Op.conv_ovf_u;
			op_table["add.ovf"] = Op.add_ovf;
			op_table["add.ovf.un"] = Op.add_ovf_un;
			op_table["mul.ovf"] = Op.mul_ovf;
			op_table["mul.ovf.un"] = Op.mul_ovf_un;
			op_table["sub.ovf"] = Op.sub_ovf;
			op_table["sub.ovf.un"] = Op.sub_ovf_un;
			op_table["endfinally"] = Op.endfinally;
			op_table["stind.i"] = Op.stind_i;
			op_table["conv.u"] = Op.conv_u;
			op_table["arglist"] = Op.arglist;
			op_table["ceq"] = Op.ceq;
			op_table["cgt"] = Op.cgt;
			op_table["cgt.un"] = Op.cgt_un;
			op_table["clt"] = Op.clt;
			op_table["clt.un"] = Op.clt_un;
			op_table["localloc"] = Op.localloc;
			op_table["endfilter"] = Op.endfilter;
			op_table["volatile."] = Op.volatile_;
			op_table["tail."] = Op.tail_;
			op_table["cpblk"] = Op.cpblk;
			op_table["initblk"] = Op.initblk;
			op_table["rethrow"] = Op.rethrow;
			op_table["refanytype"] = Op.refanytype;

		}

		private static void CreateIntTable ()
		{
			int_table = new Hashtable ();
			
			int_table["ldarg.s"] = IntOp.ldarg_s;
			int_table["ldarga.s"] = IntOp.ldarga_s;
			int_table["starg.s"] = IntOp.starg_s;
			int_table["ldloc.s"] = IntOp.ldloc_s;
			int_table["ldloca.s"] = IntOp.ldloca_s;
			int_table["stloc.s"] = IntOp.stloc_s;
			int_table["ldc.i4.s"] = IntOp.ldc_i4_s;
			int_table["ldc.i4"] = IntOp.ldc_i4;
			int_table["ldc.i8"] = IntOp.ldc_i4;
			int_table["ldarg"] = IntOp.ldarg;
			int_table["ldarga"] = IntOp.ldarga;
			int_table["starf"] = IntOp.starg;
			int_table["ldloc"] = IntOp.ldloc;
			int_table["ldloca"] = IntOp.ldloca;
			int_table["stloc"] = IntOp.stloc;
			int_table["unaligned"] =  IntOp.unaligned;
		}

		public static void CreateTypeTable ()
		{
			type_table = new Hashtable ();
			
			type_table["cpobj"] = TypeOp.cpobj;
			type_table["ldobj"] = TypeOp.ldobj;
			type_table["castclass"] = TypeOp.castclass;
			type_table["isinst"] = TypeOp.isinst;
			type_table["unbox"] = TypeOp.unbox;
			type_table["stobj"] = TypeOp.stobj;
			type_table["box"] = TypeOp.box;
			type_table["newarr"] = TypeOp.newarr;
			type_table["ldelema"] = TypeOp.ldelema;
			type_table["refanyval"] = TypeOp.refanyval;
			type_table["mkrefany"] = TypeOp.mkrefany;
			type_table["ldtoken"] = TypeOp.ldtoken;
			type_table["initobj"] = TypeOp.initobj;
			type_table["sizeof"] = TypeOp.sizeOf;
		}

		private static void CreateMethodTable ()
		{
			method_table = new Hashtable ();
			
			method_table["jmp"] = MethodOp.jmp;
			method_table["call"] = MethodOp.call;
			method_table["callvirt"] = MethodOp.callvirt;
			method_table["newobj"] = MethodOp.newobj;
			method_table["ldtoken"] = MethodOp.ldtoken;
			method_table["ldftn"] = MethodOp.ldftn;
			method_table["ldvirtftn"] = MethodOp.ldvirtfn;
		}

		private static void CreateFieldTable ()
		{
			field_table = new Hashtable ();
			
			field_table["ldfld"] = FieldOp.ldfld;
			field_table["ldflda"] = FieldOp.ldflda;
			field_table["stfld"] = FieldOp.stfld;
			field_table["ldsfld"] = FieldOp.ldsfld;
			field_table["ldsflda"] = FieldOp.ldsflda;
			field_table["stsfld"] = FieldOp.stsfld;
			field_table["ldtoken"] = FieldOp.ldtoken;
		}
		
		/// TODO: .s needs fixin
		private static void CreateBranchTable ()
		{
			branch_table = new Hashtable ();

			branch_table["br"] = BranchOp.br;
			branch_table["brfalse"] = BranchOp.brfalse;
			branch_table["brtrue"] = BranchOp.brtrue;
			branch_table["beq"] = BranchOp.beq;
			branch_table["bge"] = BranchOp.bge;
			branch_table["bgt"] = BranchOp.bgt;
			branch_table["ble"] = BranchOp.ble;
			branch_table["blt"] = BranchOp.blt;
			branch_table["bne.un"] = BranchOp.bne_un;
			branch_table["bge.un"] = BranchOp.bge_un;
			branch_table["bgt.un"] = BranchOp.bgt_un;
			branch_table["ble.un"] = BranchOp.ble_un;
			branch_table["blt.un"] = BranchOp.blt_un;
			branch_table["leave"] = BranchOp.leave;

			branch_table["br.s"] = BranchOp.br;
			branch_table["brfalse.s"] = BranchOp.brfalse;
			branch_table["brtrue.s"] = BranchOp.brtrue;
			branch_table["beq.s"] = BranchOp.beq;
			branch_table["bge.s"] = BranchOp.bge;
			branch_table["bgt.s"] = BranchOp.bgt;
			branch_table["ble.s"] = BranchOp.ble;
			branch_table["blt.s"] = BranchOp.blt;
			branch_table["bne.un.s"] = BranchOp.bne_un;
			branch_table["bge.un.s"] = BranchOp.bge_un;
			branch_table["bgt.un.s"] = BranchOp.bgt_un;
			branch_table["ble.un.s"] = BranchOp.blt_un;
			branch_table["leave.s"] = BranchOp.leave;
		}
	}

}

