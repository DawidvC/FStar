{
  open FSLangParser

  module Option  = BatOption
  module String  = BatString
  module Hashtbl = BatHashtbl

  module L : sig
    include module type of struct include Lexing end

    val range : lexbuf -> position * position
  end = struct
    include Lexing

    let range (lexbuf : lexbuf) =
      (lexeme_start_p lexbuf, lexeme_end_p lexbuf)
  end

  let char_of_ec = function
    | '\'' -> '\''
    | '\"' -> '"'
    | '\\' -> '\\'
    | 'n'  -> '\n'
    | 't'  -> '\t'
    | 'b'  -> '\b'
    | 'r'  -> '\r'
    | _    -> assert false

  let keywords = Hashtbl.create 0

  let () =
    Hashtbl.add keywords "and"           AND         ;
    Hashtbl.add keywords "assert"        ASSERT      ;
    Hashtbl.add keywords "assume"        ASSUME      ;
    Hashtbl.add keywords "begin"         BEGIN       ;
    Hashtbl.add keywords "define"        DEFINE      ;
    Hashtbl.add keywords "effect"        EFFECT      ;
    Hashtbl.add keywords "else"          ELSE        ;
    Hashtbl.add keywords "end"           END         ;
    Hashtbl.add keywords "exception"     EXCEPTION   ;
    Hashtbl.add keywords "exists"        EXISTS      ;
    Hashtbl.add keywords "false"         FALSE       ;
    Hashtbl.add keywords "forall"        FORALL      ;
    Hashtbl.add keywords "fun"           FUN         ;
    Hashtbl.add keywords "function"      FUNCTION    ;
    Hashtbl.add keywords "if"            IF          ;
    Hashtbl.add keywords "in"            IN          ;
    Hashtbl.add keywords "kind"          KIND        ;
    Hashtbl.add keywords "let"           LET         ;
    Hashtbl.add keywords "logic"         LOGIC       ;
    Hashtbl.add keywords "match"         MATCH       ;
    Hashtbl.add keywords "module"        MODULE      ;
    Hashtbl.add keywords "monad_lattice" MONADLATTICE;
    Hashtbl.add keywords "of"            OF          ;
    Hashtbl.add keywords "open"          OPEN        ;
    Hashtbl.add keywords "print"         PRINT       ;
    Hashtbl.add keywords "query"         QUERY       ;
    Hashtbl.add keywords "rec"           REC         ;
    Hashtbl.add keywords "then"          THEN        ;
    Hashtbl.add keywords "total"         TOTAL       ;
    Hashtbl.add keywords "true"          TRUE        ;
    Hashtbl.add keywords "try"           TRY         ;
    Hashtbl.add keywords "type"          TYPE        ;
    Hashtbl.add keywords "underscore"    UNDERSCORE  ;
    Hashtbl.add keywords "val"           VAL         ;
    Hashtbl.add keywords "when"          WHEN        ;
    Hashtbl.add keywords "with"          WITH

  let is_typ_app = fun _ -> false
}

(* -------------------------------------------------------------------- *)
let lower  = ['a'-'z']
let upper  = ['A'-'Z']
let letter = upper | lower
let digit  = ['0'-'9']
let hex    = ['0'-'9'] | ['A'-'F'] | ['a'-'f']

(* -------------------------------------------------------------------- *)
let truewhite = [' ']
let offwhite  = ['\t']
let anywhite  = truewhite | offwhite
let newline   = ('\n' | '\r' '\n')

(* -------------------------------------------------------------------- *)
let op_char = '!'|'$'|'%'|'&'|'*'|'+'|'-'|'.'|'/'|'<'|'='|'?'|'^'|'|'|'~'|':'
let ignored_op_char = '.' | '$'

(* -------------------------------------------------------------------- *)
let xinteger =
  (  '0' ('x'| 'X')  hex +
   | '0' ('o'| 'O')  (['0'-'7']) +
   | '0' ('b'| 'B')  (['0'-'1']) + )

let integer    = digit+
let int8       = integer 'y'
let uint8      = (xinteger | integer) 'u' 'y'
let int16      = integer 's'
let uint16     = (xinteger | integer) 'u' 's'
let int        = integer
let int32      = integer 'l'
let uint32     = (xinteger | integer) 'u'
let uint32l    = (xinteger | integer) 'u' 'l'
let nativeint  = (xinteger | integer) 'n'
let unativeint = (xinteger | integer) 'u' 'n'
let int64      = (xinteger | integer) 'L'
let uint64     = (xinteger | integer) ('u' | 'U') 'L'
let xint8      = xinteger 'y'
let xint16     = xinteger 's'
let xint       = xinteger
let xint32     = xinteger 'l'
let floatp     = digit+ '.' digit*
let floate     = digit+ ('.' digit* )? ('e'| 'E') ['+' '-']? digit+
let float      = floatp | floate
let bigint     = integer 'I'
let bignum     = integer 'N'
let ieee64     = float
let ieee32     = float ('f' | 'F')
let decimal    = (float | integer) ('m' | 'M')
let xieee32    = xinteger 'l' 'f'
let xieee64    = xinteger 'L' 'F'

(* -------------------------------------------------------------------- *)
let escape_char = ('\\' ( '\\' | "\"" | '\'' | 'n' | 't' | 'b' | 'r'))
let char        = [^'\\''\n''\r''\t''\b'] | escape_char

(* -------------------------------------------------------------------- *)
let constructor_start_char = upper
let ident_start_char       = lower  | '_'
let ident_char             = letter | digit  | ['\'' '_']
let tvar_char              = letter | digit

let constructor = constructor_start_char ident_char*
let ident       = ident_start_char ident_char*
let tvar        = '\'' (ident_start_char | constructor_start_char) tvar_char*
let basekind    = '*' | 'A' | 'E' | "Prop"

rule token = parse
 | "#light"
     { PRAGMALIGHT }
 | "#set-options"
     { PRAGMA_SET_OPTIONS }
 | "#reset-options"
     { PRAGMA_RESET_OPTIONS }
 | ident as id
     { id |> Hashtbl.find_option keywords |> Option.default (IDENT id) }
 | constructor as id
     { NAME id }
 | tvar as id
     { TVAR id }
 | (xint | int | xint32 | int32) as x
     { INT32 (Int32.of_string x, false) }
 | int64 as x
     { INT64 (Int64.of_string x, false) }
 | (ieee64 | xieee64) as x
     { IEEE64 (float_of_string x) }
 | (int | xint | float) ident_char+
     { failwith "This is not a valid numeric literal." }
 | '\'' (char as c) '\''
 | '\'' (char as c) '\'' 'B'
     { let c =
         match c.[0] with
         | '\\' -> char_of_ec c.[1]
         | _    -> c.[0]
       in CHAR c }

 | "~"         { TILDE (L.lexeme lexbuf) }
 | "/\\"       { CONJUNCTION }
 | "\\/"       { DISJUNCTION }
 | "<:"        { SUBTYPE }
 | "<@"        { SUBKIND }
 | "(|"        { LENS_PAREN_LEFT }
 | "|)"        { LENS_PAREN_RIGHT }
 | '#'         { HASH }
 | "&&"        { AMP_AMP }
 | "||"        { BAR_BAR }
 | "()"        { LPAREN_RPAREN }
 | '('         { LPAREN }
 | ')'         { RPAREN }
 | '*'         { STAR }
 | ','         { COMMA }
 | "~>"        { SQUIGGLY_RARROW }
 | "->"        { RARROW }
 | "=>"        { RRARROW }
 | "<==>"      { IFF }
 | "==>"       { IMPLIES }
 | "."         { DOT }
 | "{:pattern" { LBRACE_COLON_PTN }
 | ":"         { COLON }
 | "::"        { COLON_COLON }
 | "@"         { ATSIGN }
 | "^"         { HAT }
 | ":="        { COLON_EQUALS }
 | ";;"        { SEMICOLON_SEMICOLON }
 | ";"         { SEMICOLON }
 | "=!="       { EQUALS_BANG_EQUALS }
 | "=="        { EQUALS_EQUALS }
 | "="         { EQUALS }
 | "["         { LBRACK }
 | "[|"        { LBRACK_BAR }
 | "<="        { LEQ }
 | ">="        { GEQ }
 | "<>"        { LESSGREATER }
 | "<"         { if is_typ_app lexbuf then TYP_APP_LESS else LESS  }
 | ">"         { GREATER }
 | "|>"        { PIPE_RIGHT }
 | "<|"        { PIPE_LEFT }
 | "]"         { RBRACK }
 | "|]"        { BAR_RBRACK }
 | "{"         { LBRACE }
 | "|"         { BAR }
 | "}"         { RBRACE }
 | "!"         { BANG }

 | ('/' | '%') as op { DIV_MOD_OP    (String.of_char op) }
 | ('+' | '-') as op { PLUS_MINUS_OP (String.of_char op) }

 | "(*"
     { comment lexbuf; token lexbuf }

 | "//"  [^'\n''\r']*
     { token lexbuf }

 | '"'
     { string (Buffer.create 0) lexbuf }

 | truewhite+
     { token lexbuf }

 | offwhite+
     { token lexbuf }

 | newline
     { L.new_line lexbuf; token lexbuf }

 | '`' '`'
     (([^'`' '\n' '\r' '\t'] | '`' [^'`''\n' '\r' '\t'])+) as id
   '`' '`'
     { IDENT id }

 | _ { failwith "unexpected char" }

 | eof { EOF }

and string buffer = parse
 |  '\\' (newline as x) anywhite*
    { Buffer.add_string buffer x;
      L.new_line lexbuf;
      string buffer lexbuf; }

 | newline as x
    { Buffer.add_string buffer x;
      L.new_line lexbuf;
      string buffer lexbuf; }

 | escape_char as c
    { Buffer.add_char buffer (char_of_ec c.[1]);
      string buffer lexbuf }

 |  '"'
    { STRING (Buffer.contents buffer) }

 |  '"''B'
    { BYTEARRAY (Buffer.contents buffer) }

 | _ as c
    { Buffer.add_char buffer c;
      string buffer lexbuf }

 | eof
    { failwith "unterminated string" }

and comment = parse
 | char
    { comment lexbuf }

 | '"'
    { comment_string lexbuf; comment lexbuf; }

 | "(*"
    { comment lexbuf; comment lexbuf; }

 | newline
    { L.new_line lexbuf; comment lexbuf; }

 | "*)"
    { () }

 | [^ '\'' '(' '*' '\n' '\r' '"' ')' ]+
    { comment lexbuf }

 | _
    { comment lexbuf }

 | eof
     { failwith "unterminated comment" }

and comment_string = parse
 | '\\' newline anywhite*
     { L.new_line lexbuf; comment_string lexbuf }

 | newline
     { L.new_line lexbuf; comment_string lexbuf }

 | '"'
     { () }

 | escape_char
 | ident
 | xinteger
 | anywhite+
     { comment_string lexbuf }

 | _
     { comment_string lexbuf }

 | eof
     { failwith "unterminated comment" }
