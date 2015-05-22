(*
   Copyright 2008-2014 Nikhil Swamy and Microsoft Research

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*)
#light "off"

// (c) Microsoft Corporation. All rights reserved
module Microsoft.FStar.Options
open Microsoft.FStar
open Microsoft.FStar.Util
open Microsoft.FStar.Getopt

type debug_level_t = 
    | Low
    | Medium
    | High
    | Extreme
    | Other of string

let show_signatures = Util.mk_ref []
let norm_then_print = Util.mk_ref true
let z3_exe = Util.mk_ref (Platform.exe "z3")
let silent=Util.mk_ref false
let debug=Util.mk_ref []
let debug_level = Util.mk_ref []
let dlevel = function 
    | "Low" -> Low
    | "Medium" -> Medium
    | "High" -> High
    | "Extreme" -> Extreme
    | s -> Other s
let one_debug_level_geq l1 l2 = match l1 with 
    | Other _ 
    | Low -> l1 = l2
    | Medium -> (l2 = Low || l2 = Medium)
    | High -> (l2 = Low || l2 = Medium || l2 = High)
    | Extreme -> (l2 = Low || l2 = Medium || l2 = High || l2 = Extreme)
let debug_level_geq l2 = !debug_level |> Util.for_some (fun l1 -> one_debug_level_geq l1 l2)
let log_types = Util.mk_ref false
let print_effect_args=Util.mk_ref false
let print_real_names = Util.mk_ref false
let dump_module : ref<option<string>> = Util.mk_ref None
let should_dump l = match !dump_module with 
    | None -> false
    | Some m -> m=l
let logQueries = Util.mk_ref false
let z3exe = Util.mk_ref true
let outputDir = Util.mk_ref (Some ".")
let fstar_home_opt = Util.mk_ref None
let _fstar_home = Util.mk_ref ""
let prims_ref = Util.mk_ref None
let z3timeout = Util.mk_ref 5
let admit_smt_queries = Util.mk_ref false
let pretype = Util.mk_ref true
let codegen = Util.mk_ref None
let admit_fsi = Util.mk_ref []
let trace_error = Util.mk_ref false
let verify = Util.mk_ref true
let full_context_dependency = Util.mk_ref true
let print_implicits = Util.mk_ref false
let hide_uvar_nums = Util.mk_ref false
let hide_genident_nums = Util.mk_ref false
let serialize_mods = Util.mk_ref false
let initial_fuel = Util.mk_ref 2
let initial_ifuel = Util.mk_ref 1
let max_fuel = Util.mk_ref 8
let min_fuel = Util.mk_ref 1
let max_ifuel = Util.mk_ref 2
let warn_top_level_effects = Util.mk_ref false
let no_slack = Util.mk_ref false
let eager_inference = Util.mk_ref false
let unthrottle_inductives = Util.mk_ref false
let use_eq_at_higher_order = Util.mk_ref false
let fs_typ_app = Util.mk_ref false
let n_cores = Util.mk_ref 1
let verify_module = Util.mk_ref []
let use_build_config = Util.mk_ref false
let interactive = Util.mk_ref false
let init_options () = 
    show_signatures := [];
    norm_then_print := true;
    z3_exe := Platform.exe "z3";
    silent := false;
    debug := [];
    debug_level  := [];
    log_types  := false;
    print_effect_args := false;
    print_real_names  := false;
    dump_module  := None;
    logQueries  := false;
    z3exe  := true;
    outputDir  := Some ".";
    fstar_home_opt  := None;
    _fstar_home  := "";
    prims_ref  := None;
    z3timeout  := 5;
    admit_smt_queries := false;
    pretype  := true;
    codegen  := None;
    admit_fsi  := [];
    trace_error  := false;
    verify  := true;
    full_context_dependency  := true;
    print_implicits  := false;
    hide_uvar_nums  := false;
    hide_genident_nums  := false;
    serialize_mods  := false;
    initial_fuel  := 2;
    initial_ifuel  := 1;
    max_fuel  := 8;
    min_fuel  := 1;
    max_ifuel  := 2;
    warn_top_level_effects  := false;
    no_slack  := false;
    eager_inference  := false;
    unthrottle_inductives  := false;
    use_eq_at_higher_order  := false;
    fs_typ_app  := false;
    n_cores  := 1;
    verify_module := []

let set_fstar_home () = 
  let fh = match !fstar_home_opt with 
    | None ->
      let x = Util.get_exec_dir () in 
      let x = x ^ "/.." in
      _fstar_home := x;
      fstar_home_opt := Some x;
      x
//      let x = Util.expand_environment_variable "FSTAR_HOME" in
//      _fstar_home := x;
      
    | Some x -> _fstar_home := x; x in
  fh
let get_fstar_home () = match !fstar_home_opt with 
    | None -> ignore <| set_fstar_home(); !_fstar_home
    | Some x -> x

let prims () = match !prims_ref with 
  | None -> (get_fstar_home()) ^ "/lib/prims.fst" 
  | Some x -> x

let prependOutputDir fname = match !outputDir with
  | None -> fname
  | Some x -> x ^ "/" ^ fname

let cache_dir = "cache" 

let display_usage specs =
  Util.print_string "fstar [option] infile...";
  List.iter
    (fun (_, flag, p, doc) ->
       match p with
         | ZeroArgs ig ->
             if doc = "" then Util.print_string (Util.format1 "  --%s\n" flag)
             else Util.print_string (Util.format2 "  --%s  %s\n" flag doc)
         | OneArg (_, argname) ->
             if doc = "" then Util.print_string (Util.format2 "  --%s %s\n" flag argname)
             else Util.print_string (Util.format3 "  --%s %s  %s\n" flag argname doc))
    specs

let rec specs () : list<Getopt.opt> = 
  let specs =   
    [( noshort, "trace_error", ZeroArgs (fun () -> trace_error := true), "Don't print an error message; show an exception trace instead");
     ( noshort, "codegen", OneArg ((fun s -> codegen := parse_codegen s; verify := false), "OCaml|F#|JavaScript"), "Generate code for execution");
     ( noshort, "lax", ZeroArgs (fun () -> pretype := true; verify := false), "Run the lax-type checker only (admit all verification conditions)");
     ( noshort, "fstar_home", OneArg ((fun x -> fstar_home_opt := Some x), "dir"), "Set the FSTAR_HOME variable to dir");
     ( noshort, "silent", ZeroArgs (fun () -> silent := true), "");
     ( noshort, "prims", OneArg ((fun x -> prims_ref := Some x), "file"), "");
     ( noshort, "prn", ZeroArgs (fun () -> print_real_names := true), "Print real names---you may want to use this in conjunction with logQueries");
     ( noshort, "debug", OneArg ((fun x -> debug := x::!debug), "module name"), "Print LOTS of debugging information while checking module [arg]");
     ( noshort, "debug_level", OneArg ((fun x -> debug_level := dlevel x::!debug_level), "Low|Medium|High|Extreme"), "Control the verbosity of debugging info");
     ( noshort, "log_types", ZeroArgs (fun () -> log_types := true), "Print types computed for data/val/let-bindings");
     ( noshort, "print_effect_args", ZeroArgs (fun () -> print_effect_args := true), "Print inferred predicate transformers for all computation types");
     ( noshort, "dump_module", OneArg ((fun x -> dump_module := Some x), "module name"), "");
     ( noshort, "z3timeout", OneArg ((fun s -> z3timeout := int_of_string s), "t"), "Set the Z3 per-query (soft) timeout to t seconds (default 5)");
     ( noshort, "admit_smt_queries", OneArg ((fun s -> admit_smt_queries := (if s="true" then true else if s="false" then false else failwith("Invalid argument to --admit_smt_queries"))), "true|false"), "Admit SMT queries (UNSAFE! But, useful during development); default: 'false'");
     ( noshort, "logQueries", ZeroArgs (fun () -> logQueries := true), "Log the Z3 queries in queries.smt2");
     ( noshort, "admit_fsi", OneArg ((fun x -> admit_fsi := x::!admit_fsi), "module name"), "Treat .fsi as a .fst");
     ( noshort, "odir", OneArg ((fun x -> outputDir := Some x), "dir"), "Place output in directory dir");
     ( noshort, "smt", OneArg ((fun x -> z3_exe := x), "path"), "Path to the SMT solver (usually Z3, but could be any SMT2-compatible solver)");
     ( noshort, "print_before_norm", ZeroArgs(fun () -> norm_then_print := false), "Do not normalize types before printing (for debugging)");
     ( noshort, "show_signatures", OneArg((fun x -> show_signatures := x::!show_signatures), "module name"), "Show the checked signatures for all top-level symbols in the module");
     ( noshort, "full_context_dependency", ZeroArgs(fun () -> full_context_dependency := true), "Introduce unification variables that are dependent on the entire context (possibly expensive, but better for type inference (on, by default)");
     ( noshort, "MLish", ZeroArgs(fun () -> full_context_dependency := false), "Introduce unification variables that are only dependent on the type variables in the context");
     ( noshort, "print_implicits", ZeroArgs(fun () -> print_implicits := true), "Print implicit arguments");
     ( noshort, "hide_uvar_nums", ZeroArgs(fun () -> hide_uvar_nums := true), "Don't print unification variable numbers");
     ( noshort, "hide_genident_nums", ZeroArgs(fun () -> hide_genident_nums := true), "Don't print generated identifier numbers");
     ( noshort, "serialize_mods", ZeroArgs (fun () -> serialize_mods := true), "Serialize compiled modules");
     ( noshort, "initial_fuel", OneArg((fun x -> initial_fuel := int_of_string x), "non-negative integer"), "Number of unrolling of recursive functions to try initially (default 2)");
     ( noshort, "max_fuel", OneArg((fun x -> max_fuel := int_of_string x), "non-negative integer"), "Number of unrolling of recursive functions to try at most (default 8)");
     ( noshort, "min_fuel", OneArg((fun x -> min_fuel := int_of_string x), "non-negative integer"), "Minimum number of unrolling of recursive functions to try (default 1)");
     ( noshort, "initial_ifuel", OneArg((fun x -> initial_ifuel := int_of_string x), "non-negative integer"), "Number of unrolling of inductive datatypes to try at first (default 1)");
     ( noshort, "max_ifuel", OneArg((fun x -> max_ifuel := int_of_string x), "non-negative integer"), "Number of unrolling of inductive datatypes to try at most (default 1)");
     ( noshort, "warn_top_level_effects", ZeroArgs (fun () -> warn_top_level_effects := true), "Top-level effects are ignored, by default; turn this flag on to be warned when this happens");
     ( noshort, "no_slack", ZeroArgs (fun () -> no_slack := true), "Use the partially flow-insensitive variant of --rel2 (experimental)");
     ( noshort, "eager_inference", ZeroArgs (fun () -> eager_inference := true), "Solve all type-inference constraints eagerly; more efficient but at the cost of generality");
     ( noshort, "unthrottle_inductives", ZeroArgs (fun () -> unthrottle_inductives := true), "Let the SMT solver unfold inductive types to arbitrary depths (may affect verifier performance)");
     ( noshort, "use_eq_at_higher_order", ZeroArgs (fun () -> use_eq_at_higher_order := true), "Use equality constraints when comparing higher-order types; temporary");
     ( noshort, "fs_typ_app", ZeroArgs (fun () -> fs_typ_app := true), "Allow the use of t<t1,...,tn> syntax for type applications; brittle since it clashes with the integer less-than operator");
     ( noshort, "no_fs_typ_app", ZeroArgs (fun () -> fs_typ_app := false), "Do not allow the use of t<t1,...,tn> syntax for type applications");
     ( noshort, "n_cores", OneArg ((fun x -> n_cores := int_of_string x), "positive integer"), "Maximum number of cores to use for the solver (default 1)");
     ( noshort, "verify_module", OneArg ((fun x -> verify_module := x::!verify_module), "string"), "Name of the module to verify");
     ( noshort, "use_build_config", ZeroArgs (fun () -> use_build_config := true), "Expect just a single file on the command line and no options; will read the 'build-config' prelude from the file");
     ( noshort, "in", ZeroArgs (fun () -> interactive := true), "Interactive mode; reads input from stdin")
    ] in 
     ( 'h', "help", ZeroArgs (fun x -> display_usage specs; exit 0), "Display this information")::specs
and parse_codegen s =
  match s with
  | "OCaml"
  | "F#"
  | "JavaScript" -> Some s
  | _ ->
     (Util.print_string "Wrong argument to codegen flag\n";
      display_usage (specs ()); exit 1)

let should_verify m = 
    match !verify_module with 
        | [] -> true //the verify_module flag was not set, so verify everything
        | l -> List.contains m l //otherwise, look in the list to see if it is explicitly mentioned

let set_options s = Getopt.parse_string (specs()) (fun _ -> ()) s

let reset_options_string : ref<option<string>> = ref None
let reset_options () = 
    init_options();
    match !reset_options_string with 
        | Some x -> set_options x
        | _ -> Getopt.parse_cmdline (specs()) (fun x -> ())

