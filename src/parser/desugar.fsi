﻿(*
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
module Microsoft.FStar.Parser.Desugar

open Microsoft.FStar
open Microsoft.FStar.Parser
open Microsoft.FStar.Range
open Microsoft.FStar.Parser.AST
open Microsoft.FStar.Parser.DesugarEnv
open Microsoft.FStar.Absyn
open Microsoft.FStar.Absyn.Syntax
open Microsoft.FStar.Absyn.Util
open Microsoft.FStar.Util

val desugar_file: env -> file -> env * list<modul>
val desugar_decls: env -> list<AST.decl> -> env * sigelts
val desugar_partial_modul: option<(modul * 'a)> -> env -> AST.modul -> env * Syntax.modul

(* private *) val desugar_modul : env -> AST.modul -> env * Syntax.modul
(* private *) val mk_data_projectors : env -> sigelt -> list<sigelt>
(* private *) val close : env -> term -> term
(* private *) val op_as_tylid : env -> range -> string -> option<lident>
(* private *) val op_as_vlid : env -> int -> range -> string -> option<lident>

val add_modul_to_env: Syntax.modul -> env -> env
