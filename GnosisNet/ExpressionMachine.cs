using System.Linq.Expressions;
using static Gnosis.Inform.ExpressionMachine;

namespace Gnosis
{
    public partial class Inform
    {
        public class ExpressionMachine
        {
            public string s = "";
            int negations = 0;
            bool negated => negations % 2 == 1;

            public Expression Visit(Expression node)
            {
                switch (node)
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

            public static string GetExpressionString(Expression node)
            {
                var exprMachine = new ExpressionMachine();
                exprMachine.Visit(node);
                return exprMachine.s;
            }


            string GetOperatorString(Expression node)
            {
                if (node.NodeType == ExpressionType.Equal)
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

                if (node.NodeType == ExpressionType.NotEqual)
                {
                    return negated ? "is" : "is not";
                }

                if (node.NodeType == ExpressionType.Assign)
                {
                    return "is";
                }

                throw new System.Exception("Unknown operator");
            }


            Expression VisitConstant(ConstantExpression node)
            {
                
                var constant = node.Value.ToString();

                if (node.Type == typeof(string))
                {
                    constant = "\"" + constant + "\"";
                }

                s += constant;

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
                if (node.Expression.NodeType == ExpressionType.Constant)
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


        public class TreeDumper : System.Linq.Expressions.ExpressionVisitor
        {
            public string s = "";
            public int depth = 0;

            public override Expression Visit(Expression node)
            {
                if(node != null)
                {
                    node = node.ReduceExtensions();
                    node = node.Reduce();

                    var tabs = "";
                    for (int i = 0; i < depth; i++)
                    {
                        tabs += "\t";
                    }

                    s += tabs + node.NodeType + " : " + node.ToString() + "\n";
                }

                depth++;
                var res = base.Visit(node);
                depth--;
                return res;
            }

        }

        
        public class LocationVisitor : ExpressionVisitor
        {
            public string thingName;
            public bool isLocation;
            public override Expression Visit(Expression node)
            {
                if(node.NodeType == ExpressionType.MemberAccess)
                {
                    var member = (MemberExpression)node;
                    if(member.Type == typeof(Room) && member.Member.Name == "location")
                    {
                        
                        isLocation = true;
                        thingName = GetExpressionString(member.Expression);
                        return null;
                    }
                }

                return base.Visit(node);
            }
        }

        public class BodyVisitor : ExpressionVisitor
        {
            public string s = "";
            public int depth = 0;

            string Indent()
            {
                var tabs = "";
                for (int i = 0; i < depth; i++)
                {
                    tabs += "\t";
                }

                return tabs;
            }


            protected override Expression VisitBlock(BlockExpression node)
            {
                depth++;
                var res = base.VisitBlock(node);
                depth--;
                return res;
            }


            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var functionName = node.Method.Name;

                var machine = new ExpressionMachine();
                machine.Visit(node.Arguments[0]);

                var argString = machine.s;
                s += Indent() + functionName + " " + argString + ";\n";


                return base.VisitMethodCall(node);
            }

            public bool IsAssigningLocation(BinaryExpression node, out string thingName)
            {
                thingName = "";

                if(node.NodeType != ExpressionType.Assign)
                {
                    return false;
                }

                // lhs needs to be called "location"
                var locationVisitor = new LocationVisitor();
                locationVisitor.Visit(node.Left);
                thingName = locationVisitor.thingName;
                return locationVisitor.isLocation;
            }

            public bool IsAssigningProperty(BinaryExpression node, out string thingName)
            {
                thingName = "";

                if (node.NodeType != ExpressionType.Assign)
                {
                    return false;
                }

                // lhs needs to be called "location"
                var locationVisitor = new LocationVisitor();
                locationVisitor.Visit(node.Left);
                thingName = locationVisitor.thingName;
                return locationVisitor.isLocation;
            }


            protected override Expression VisitBinary(BinaryExpression node)
            {
                var machine = new ExpressionMachine();
                var type = node.NodeType;
                var leftExpr = GetExpressionString(node.Left);
                var rightExpr = GetExpressionString(node.Right);

                if(IsAssigningLocation(node, out var thing))
                {
                    s += Indent() + "Now " + thing + " is in " + rightExpr + ";\n";
                }
                else if (type == ExpressionType.Assign)
                {
                    s += Indent() + "Now " + leftExpr + " is " + rightExpr + ";\n";
                }

                return base.VisitBinary(node);
            }

            public override Expression Visit(Expression node)
            {
                if (node != null)
                {

                    node = node.ReduceExtensions();
                    node = node.Reduce();

                   // s += Indent() + node.NodeType + " : " + node.ToString() + "\n";

                }

               // depth++;
                var res = base.Visit(node);
               // depth--;
                return res;
            }

        }



    }
}