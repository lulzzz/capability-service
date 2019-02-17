using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Settings.Tests
{
    public static class ReflectionHelper
    {
        public static PropertyInfo Property<T>(Expression<Func<T, object>> property)
        {
            if (!(property is LambdaExpression lambda))
            {
                throw new ArgumentNullException(nameof(property));
            }

            var memberExpr = GetMemberExpression(lambda);
            if (memberExpr == null)
            {
                throw new ArgumentException("Not a member access", nameof(property));
            }

            var propertyInfo = memberExpr.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("Member is not a property");
            }

            return propertyInfo;
        }

        private static MemberExpression GetMemberExpression(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression) lambda.Body;
                return body.Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                return  lambda.Body as MemberExpression;
            }
            else
            {
                return null;
            }
        }
    }
}