grammar QL;

document : action_block EOF;

action_type : NAME ;
action_block : action_type LBRACE context_blocks RBRACE ;

context_blocks : context_block+ ;
context_block : remote_context_block | local_context_block ;

local_context_block : LOCAL selection_set ;
remote_context_block : REMOTE args selection_set ;

selection_set : LBRACE selection+ RBRACE ;
selection : field ;

field : NAME args? selection_set? transformations? ;

args : LPAREN arg (COMMA arg)* RPAREN ;
arg : (NAME COLON)? value ;

transformations : transformation+ ;
transformation : PIPE NAME args? ;

list : LBRACK value* RBRACK ;
object_field : NAME COLON value ;
object : LBRACE object_field* RBRACE ;

value : VARIABLE | BOOLEAN | NUMBER | STRING | NULL | list | object ;

// Lexer Rules

LOCAL : 'local' ;
REMOTE : 'remote' ;

BOOLEAN : 'true' | 'false' ;
NULL : 'null' ;

NAME_START : [a-zA-Z_] ;
NAME : NAME_START [a-zA-Z0-9_]* ;

STRING : '"' (ESC | ~["\\])* '"';

NUMBER : FLOAT | INT ;
FLOAT : INT ((FRACTIONAL EXPONENT) | FRACTIONAL | EXPONENT) ;
FRACTIONAL : '.' [0-9]+ ;
EXPONENT : [eE][+-]?[0-9]+ ;
INT : '-'? ('0' | [1-9][0-9]*) ;

VARIABLE : '$' NAME ;

WS: [ \t\n\r\f]+ -> skip ;
COMMENT : '#' ~ [^\n]* '\n' ;
LINE_TERMINATOR : '\r\n' | '\r' | '\n' ;
UNICODE_SCALAR_VALUE_HEX : [a-fA-F0-9]{4} ;
LETTERS : [a-zA-Z] ;
EXP : [eE][+-]?[1-9]+ ;
HEX : [0-9a-fA-F] ;
UNICODE : '\\u' HEX HEX HEX HEX ;
ESC : '\\' ["\\/bfnrt] | UNICODE;

COLON : ':' ;
LPAREN : '(' ;
RPAREN : ')' ;
LBRACE : '{' ;
RBRACE : '}' ;
LBRACK : '[' ;
RBRACK : ']' ;
PIPE : '|' ;
COMMA : ',' ;