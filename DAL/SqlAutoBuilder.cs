using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DAL.Helpers
{
    public static class SqlAutoBuilder
    {
        // ==== MÉTODOS PÚBLICOS PRINCIPALES =====================

        public static string BuildSelect<T>(Expression<Func<T, bool>> where = null, bool top1 = false)
        {
            string table = GetTableName<T>();
            string top = top1 ? "TOP 1 " : string.Empty;
            string sql = "SELECT " + top + "* FROM " + table;

            if (where != null)
            {
                string whereSql = BuildWhere(where);
                sql += " WHERE " + whereSql;
            }

            return sql;
        }

        public static string BuildDelete<T>(Expression<Func<T, bool>> where)
        {
            if (where == null) throw new ArgumentNullException("where");

            string table = GetTableName<T>();
            string whereSql = BuildWhere(where);
            return "DELETE FROM " + table + " WHERE " + whereSql;
        }

        public static string BuildInsert<T>(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            string table = GetTableName<T>();
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<string> columns = new List<string>();
            List<string> values = new List<string>();

            foreach (PropertyInfo prop in props)
            {
                object value = prop.GetValue(entity, null);

                // Regla: si es "id" int y está en 0, asumimos IDENTITY y no lo incluimos
                if (string.Equals(prop.Name, "id", StringComparison.OrdinalIgnoreCase) &&
                    prop.PropertyType == typeof(int) &&
                    (value == null || (int)value == 0))
                {
                    continue;
                }

                // Ignorar nulos (puedes cambiar esta regla si quieres)
                if (value == null)
                    continue;

                columns.Add(prop.Name);
                values.Add(ValueToSql(value));
            }

            if (columns.Count == 0)
                throw new InvalidOperationException("No hay columnas para insertar.");

            string cols = string.Join(", ", columns.ToArray());
            string vals = string.Join(", ", values.ToArray());

            return "INSERT INTO " + table + " (" + cols + ") VALUES (" + vals + ");";
        }

        public static string BuildUpdate<T>(T entity, Expression<Func<T, bool>> where)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (where == null) throw new ArgumentNullException("where");

            string table = GetTableName<T>();
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<string> sets = new List<string>();

            foreach (PropertyInfo prop in props)
            {
                if (string.Equals(prop.Name, "id", StringComparison.OrdinalIgnoreCase))
                    continue; // no actualizamos el id por defecto

                object value = prop.GetValue(entity, null);
                if (value == null)
                    continue;

                sets.Add(prop.Name + " = " + ValueToSql(value));
            }

            if (sets.Count == 0)
                throw new InvalidOperationException("No hay columnas para actualizar.");

            string setSql = string.Join(", ", sets.ToArray());
            string whereSql = BuildWhere(where);

            return "UPDATE " + table + " SET " + setSql + " WHERE " + whereSql + ";";
        }

        // ==== WHERE / EXPRESIONES ==============================

        public static string BuildWhere<T>(Expression<Func<T, bool>> expression)
        {
            return VisitExpression(expression.Body);
        }

        private static string VisitExpression(Expression exp)
        {
            if (exp is BinaryExpression)
            {
                return VisitBinary((BinaryExpression)exp);
            }

            if (exp is MethodCallExpression)
            {
                return VisitMethodCall((MethodCallExpression)exp);
            }

            if (exp is UnaryExpression)
            {
                UnaryExpression unary = (UnaryExpression)exp;
                if (unary.NodeType == ExpressionType.Convert)
                {
                    // eliminar casts como (object)x.id
                    return VisitExpression(unary.Operand);
                }
            }

            throw new NotSupportedException("Expresión no soportada: " + exp.NodeType);
        }

        private static string VisitBinary(BinaryExpression binary)
        {
            // AND / OR
            if (binary.NodeType == ExpressionType.AndAlso || binary.NodeType == ExpressionType.OrElse)
            {
                string leftExpr = VisitExpression(binary.Left);
                string rightExpr = VisitExpression(binary.Right);
                string op = binary.NodeType == ExpressionType.AndAlso ? "AND" : "OR";

                return "(" + leftExpr + " " + op + " " + rightExpr + ")";
            }

            // Comparaciones
            string operador;

            switch (binary.NodeType)
            {
                case ExpressionType.Equal:
                    operador = "=";
                    break;

                case ExpressionType.NotEqual:
                    operador = "<>";
                    break;

                case ExpressionType.GreaterThan:
                    operador = ">";
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    operador = ">=";
                    break;

                case ExpressionType.LessThan:
                    operador = "<";
                    break;

                case ExpressionType.LessThanOrEqual:
                    operador = "<=";
                    break;

                default:
                    throw new NotSupportedException("Operador no soportado: " + binary.NodeType);
            }

            // lado izquierdo: columna
            string leftColumn = GetMemberName(binary.Left);

            // lado derecho: valor
            string rightValue = GetValueSql(binary.Right);

            return leftColumn + " " + operador + " " + rightValue;
        }

        // 🔥 NUEVO: Soporte para Contains / StartsWith / EndsWith (LIKE)
        private static string VisitMethodCall(MethodCallExpression call)
        {
            // Solo manejamos métodos de string: Contains, StartsWith, EndsWith
            if (call.Method.DeclaringType == typeof(string))
            {
                string methodName = call.Method.Name;

                // objeto: x.mesa
                string column = GetMemberName(call.Object);
                // argumento: el valor a buscar
                string valueSql = GetValueSql(call.Arguments[0]); // esto ya viene como N'valor'

                switch (methodName)
                {
                    case "Contains":
                        // columna LIKE '%' + valor + '%'
                        return column + " LIKE '%' + " + valueSql + " + '%'";
                    case "StartsWith":
                        // columna LIKE valor + '%'
                        return column + " LIKE " + valueSql + " + '%'";
                    case "EndsWith":
                        // columna LIKE '%' + valor
                        return column + " LIKE '%' + " + valueSql;
                }
            }

            throw new NotSupportedException("Método no soportado en WHERE: " + call.Method.Name);
        }

        private static string GetMemberName(Expression exp)
        {
            if (exp is MemberExpression)
            {
                MemberExpression member = (MemberExpression)exp;
                return member.Member.Name;
            }

            if (exp is UnaryExpression)
            {
                UnaryExpression unary = (UnaryExpression)exp;
                if (unary.NodeType == ExpressionType.Convert)
                    return GetMemberName(unary.Operand);
            }

            throw new NotSupportedException("No se pudo obtener el nombre de la columna desde la expresión.");
        }

        private static string GetValueSql(Expression exp)
        {
            // Compilar la expresión para obtener el valor real
            LambdaExpression lambda = Expression.Lambda(exp);
            object value = lambda.Compile().DynamicInvoke();
            return ValueToSql(value);
        }

        // ==== UTILIDADES =======================================

        private static string GetTableName<T>()
        {
            // Simple: nombre de la clase = nombre de la tabla
            return typeof(T).Name;
        }

        private static string ValueToSql(object value)
        {
            if (value == null)
                return "NULL";

            if (value is string)
            {
                string s = (string)value;
                return "N'" + s.Replace("'", "''") + "'";
            }

            if (value is Guid)
            {
                Guid g = (Guid)value;
                return "N'" + g.ToString() + "'";
            }

            if (value is DateTime)
            {
                DateTime dt = (DateTime)value;
                return "'" + dt.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            }

            if (value is bool)
            {
                bool b = (bool)value;
                return b ? "1" : "0";
            }

            if (value is decimal)
            {
                decimal dec = (decimal)value;
                return dec.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (value is float)
            {
                float f = (float)value;
                return f.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (value is double)
            {
                double d = (double)value;
                return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            // ints, longs, etc.
            return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
