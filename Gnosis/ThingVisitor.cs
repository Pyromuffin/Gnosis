using System.Linq.Expressions;

namespace Gnosis
{
    public partial class Inform
    {
        public class ThingVisitor
        {
            public string s = "";
            int negations = 0;
            bool negated => negations % 2 == 1;

            public Expression Visit(Expression node)
            {
                switch(node)
                {
                    case LambdaExpression l:
                        Visit(l.Body);
                        break;
                    case ConstantExpression c:
                        VisitConstant(c);
                        break;
                    case BinaryExpression b:
                        VisitBinary(b);
                        break;
                    case ParameterExpression p:
                        VisitParameter(p);
                        break;
                    case MemberExpression m:
                        VisitMember(m);
                        break;
                    case UnaryExpression u:
                        VisitUnary(u);
                        break;
                   
                    default:
                        return null;
                }


                return node;
            }

            string GetOperatorString(Expression node)
            {
                if(node.NodeType == ExpressionType.Equal )
                {
                    
                    return negated ? "is not" : "is";
                }

                if (node.NodeType == ExpressionType.AndAlso)
                {
                    return negated ? "or" : "and";
                }

                if (node.NodeType == ExpressionType.OrElse)
                {
                    return negated ? "and" : "or";
                }

                if(node.NodeType == ExpressionType.NotEqual)
                {
                    return negated ? "is" : "is not";
                }

                throw new System.Exception("Unknown operator");
            }


            Expression VisitConstant(ConstantExpression node)
            {

                s += node.Value.ToString();

                return node;
            }

            Expression VisitBinary(BinaryExpression node)
            {
                s += "(";
                var left = node.Left;
                Visit(left);

                var opString = GetOperatorString(node);
                s += " " + opString + " ";

                var right = node.Right;
                Visit(right);
                s += ")";

                return node;
            }

            Expression VisitParameter(ParameterExpression node)
            {
                s += "the noun";
                return node;
            }

           Expression VisitMember(MemberExpression node)
            {
                if(node.Expression.NodeType == ExpressionType.Constant)
                {
                    s += node.Member.Name;
                    return node;
                }

                Visit(node.Expression);

                s += (negated ? " is not " : " is ") + node.Member.Name;
                return node;
            }
            
            Expression VisitUnary(UnaryExpression node)
            {
                s += " (";
                negations++;
                Visit(node.Operand);
                negations--;
                s += ") ";
                return node;
            }

        }
    }
}
