using Antlr4.Runtime;
using QL.Parser.AST;
using QL.Parser.AST.Nodes;

namespace QL.Parser;

public static class Parser
{
    public static ActionBlockNode ParseQuery(string query)
    {
        var inputStream = new AntlrInputStream(query);
        var speakLexer = new QLLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(speakLexer);
        var qlParser = new QLParser(commonTokenStream);
        var result = qlParser.document();
        var ast = new ASTBuilder().Visit(result);
        
        if (ast is not ActionBlockNode node)
        {
            throw new Exception("Invalid query");
        }
        
        return node;
    }
    
}