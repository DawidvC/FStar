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
module Ex0

(* let simple_zero_ref x = ref 0 *)

(* let simple_zero x = (fun y -> 1) 0 *)

(* let simple_zero_2 x =  *)
(*   let f_one y = 1 in *)
(*     f_one 0 *)

(* let cond_zero x = *)
(*   if true then 0 else 1 *)

let zero x = (fun y -> y = 0) 0
