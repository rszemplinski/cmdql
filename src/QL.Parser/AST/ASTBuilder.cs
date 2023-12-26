using QL.Parser.AST.Nodes;

namespace QL.Parser.AST;

public class ASTBuilder : QLBaseVisitor<QLNode>
{
    public override QLNode VisitDocument(QLParser.DocumentContext context)
    {
        return Visit(context.action_block());
    }

    public override QLNode VisitAction_block(QLParser.Action_blockContext context)
    {
        var actionType = Visit(context.action_type());
        if (actionType is not ActionTypeNode actionTypeNode)
            throw new Exception("ActionTypeNode expected");

        var contextBlocks = Visit(context.context_blocks());
        if (contextBlocks is not ContextBlocksNode contextBlocksNode)
            throw new Exception("ContextBlocksNode expected");

        return new ActionBlockNode
        {
            ActionType = actionTypeNode,
            ContextBlocks = contextBlocksNode.ContextBlocks
        };
    }

    public override QLNode VisitAction_type(QLParser.Action_typeContext context)
    {
        return new ActionTypeNode
        {
            ActionType = context.GetText()
        };
    }

    public override QLNode VisitContext_blocks(QLParser.Context_blocksContext context)
    {
        var contextBlocks = new List<ContextBlockNode>();

        foreach (var contextBlock in context.context_block())
        {
            var qlNode = Visit(contextBlock);
            if (qlNode is not ContextBlockNode contextBlockNode)
                throw new Exception("ContextBlockNode expected");

            contextBlocks.Add(contextBlockNode);
        }

        return new ContextBlocksNode
        {
            ContextBlocks = contextBlocks
        };
    }

    public override QLNode VisitContext_block(QLParser.Context_blockContext context)
    {
        return context.local_context_block() is not null
            ? Visit(context.local_context_block())
            : Visit(context.remote_context_block());
    }

    public override QLNode VisitLocal_context_block(QLParser.Local_context_blockContext context)
    {
        var selections = Visit(context.selection_set());
        if (selections is not SelectionSetNode selectionSetNode)
            throw new Exception("SelectionSetNode expected");

        return new LocalContextBlockNode
        {
            SelectionSet = selectionSetNode.Selections
        };
    }

    public override QLNode VisitRemote_context_block(QLParser.Remote_context_blockContext context)
    {
        var args = Visit(context.args());
        if (args is not ArgumentsNode argumentsNode)
            throw new Exception("ArgumentsNode expected");

        var selections = Visit(context.selection_set());
        if (selections is not SelectionSetNode selectionSetNode)
            throw new Exception("SelectionSetNode expected");

        return new RemoteContextBlockNode
        {
            Arguments = argumentsNode.Arguments,
            SelectionSet = selectionSetNode.Selections
        };
    }

    public override QLNode VisitSelection_set(QLParser.Selection_setContext context)
    {
        var selections = new List<SelectionNode>();

        foreach (var selection in context.selection())
        {
            var qlNode = Visit(selection);
            if (qlNode is not SelectionNode fieldNode)
                throw new Exception("SelectionNode expected");

            selections.Add(fieldNode);
        }

        return new SelectionSetNode
        {
            Selections = selections
        };
    }

    public override QLNode VisitSelection(QLParser.SelectionContext context)
    {
        var field = Visit(context.field());
        if (field is not FieldNode fieldNode)
            throw new Exception("FieldNode expected");

        return new SelectionNode
        {
            Field = fieldNode
        };
    }

    public override QLNode VisitArgs(QLParser.ArgsContext context)
    {
        var arguments = new List<ArgumentNode>();

        foreach (var argument in context.arg())
        {
            var qlNode = Visit(argument);
            if (qlNode is not ArgumentNode argumentNode)
                throw new Exception("ArgumentNode expected");

            arguments.Add(argumentNode);
        }

        return new ArgumentsNode
        {
            Arguments = arguments
        };
    }

    public override QLNode VisitArg(QLParser.ArgContext context)
    {
        var name = context.NAME().GetText();
        var value = Visit(context.value());
        if (value is not ValueNode valueNode)
            throw new Exception("ValueNode expected");

        return new ArgumentNode
        {
            Name = name,
            Value = valueNode
        };
    }

    public override QLNode VisitField(QLParser.FieldContext context)
    {
        var name = context.NAME().GetText();

        var arguments = context.args() is not null ? Visit(context.args()) as ArgumentsNode : null;
        var transformations = context.transformations() is not null
            ? Visit(context.transformations()) as TransformationsNode
            : null;
        var selectionSet = context.selection_set() is not null
            ? Visit(context.selection_set()) as SelectionSetNode
            : null;

        return new FieldNode
        {
            Name = name,
            Arguments = arguments?.Arguments ?? [],
            Transformations = transformations?.Transformations ?? [],
            SelectionSet = selectionSet?.Selections ?? []
        };
    }

    public override QLNode VisitValue(QLParser.ValueContext context)
    {
        if (context.NUMBER() is not null)
        {
            var number = context.GetText();
            if (number.Contains('.'))
            {
                return new DecimalValueNode
                {
                    Value = decimal.Parse(number)
                };
            }

            return new IntValueNode
            {
                Value = int.Parse(number)
            };
        }

        if (context.STRING() is not null)
        {
            return new StringValueNode
            {
                Value = context.GetText().Trim('"')
            };
        }

        if (context.BOOLEAN() is not null)
        {
            return new BooleanValueNode
            {
                Value = context.GetText() == "true"
            };
        }

        if (context.NULL() is not null)
        {
            return new NullValueNode();
        }

        if (context.list() is not null)
        {
            return Visit(context.list());
        }

        if (context.@object() is not null)
        {
            return Visit(context.@object());
        }

        if (context.VARIABLE() is not null)
        {
            return new VariableValueNode
            {
                Name = context.GetText()
            };
        }

        throw new Exception("Invalid value was found");
    }

    public override QLNode VisitList(QLParser.ListContext context)
    {
        var values = new List<ValueNode>();

        foreach (var value in context.value())
        {
            var qlNode = Visit(value);
            if (qlNode is not ValueNode valueNode)
                throw new Exception("ValueNode expected");

            values.Add(valueNode);
        }

        return new ListValueNode
        {
            Values = values
        };
    }

    public override QLNode VisitObject(QLParser.ObjectContext context)
    {
        var fields = new List<ObjectFieldNode>();

        foreach (var field in context.object_field())
        {
            var qlNode = Visit(field);
            if (qlNode is not ObjectFieldNode objectFieldNode)
                throw new Exception("ObjectFieldNode expected");

            fields.Add(objectFieldNode);
        }

        return new ObjectValueNode
        {
            Fields = fields
        };
    }

    public override QLNode VisitObject_field(QLParser.Object_fieldContext context)
    {
        var name = context.NAME().GetText();
        var value = Visit(context.value());
        if (value is not ValueNode valueNode)
            throw new Exception("ValueNode expected");

        return new ObjectFieldNode
        {
            Name = name,
            Value = valueNode
        };
    }
}