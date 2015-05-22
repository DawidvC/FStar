(*--build-options
  options:--z3timeout 20;
  variables:LIB=../../lib;
  other-files:$LIB/classical.fst $LIB/ext.fst $LIB/set.fsi $LIB/set.fst $LIB/heap.fst
              $LIB/stperm.fst $LIB/seq.fsi $LIB/seq.fst $LIB/seqproperties.fst $LIB/arr.fst
              qs_seq.fst qsort_arr.fst
--*)
module Downgrade
#set-options "--initial_fuel 0 --initial_ifuel 0 --max_fuel 0 --max_ifuel 0"
open Array
open Seq
open SeqProperties
open ST
open Heap
type tot_ord (a:Type) = f:(a -> a -> Tot bool){total_order a f}

val qsort_seq : #a:Type -> f:tot_ord a -> x:seq a -> ST (seq a)
  (requires (fun h -> True))
  (ensures (fun h0 y h1 -> sorted f y /\ permutation a x y))
  (modifies (no_refs))
let qsort_seq f x =
  let x_ar = Array.of_seq x in
  QuickSort.Array.qsort f x_ar;
  let res = to_seq x_ar in
  free x_ar;
  res

val qsort_seq_forget: #a:Type -> f:tot_ord a -> s1:seq a -> Dv (s2:seq a{sorted f s2 /\ permutation a s1 s2})
let qsort_seq_forget f x = forget_ST (qsort_seq f)  x
