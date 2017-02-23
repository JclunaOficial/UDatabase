using System;
using System.Data;
using System.Data.Common;

namespace JclunaOficial
{
    /// <summary>
    /// Clase con funciones extendidas
    /// </summary>
    public static class UDatabaseExtensions
    {
        // el nombre del parámetro debe empezar con @
        private static string fixName(string value)
        {
            // evaluar el valor
            var result = (value == null ? "" : value.Trim());
            if (result.Length == 0)
                return result; // no hay valor

            // agregar @ al inicio del valor
            if (!result.StartsWith("@"))
                result = result.Insert(0, "@");
            return result;
        }

        // el valor debe ser un dato parametrizable
        private static object fixValue(object value)
        {
            // si el valor el nulo, entonces usar nulo parametrizable
            object result = DBNull.Value;
            if (value == null) return result;

            // determinar si el tipo de objeto aplica
            if (value is IDbDataParameter)
            {
                // extraer el valor del objeto
                var data = (IDbDataParameter)value;
                if (data.Value != null)
                    result = data.Value;
            }
            else { result = value; }
            return result;
        }

        /// <summary>
        /// Agregar un parámetro a un objeto del tipo <see cref="DbCommand"/>
        /// </summary>
        /// <param name="command">Objeto del tipo <see cref="DbCommand"/></param>
        /// <param name="name">Nombre del parámetro</param>
        /// <param name="type">Tipo de dato del parámetro</param>
        /// <param name="value">Valor del parámetro</param>
        /// <param name="direction">Dirección del parámetro</param>
        public static void AddParameter(this DbCommand command, string name, DbType type, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            if (command == null) // el objeto es requerido
                throw new ArgumentNullException("command");

            // evaluar el nombre del parámetro
            var parameterName = fixName(name);
            if (name.Length == 0) // el nombre del parámetro es requerido
                throw new ArgumentNullException("name");

            // preparar el parámetro
            DbParameter objResult = null;
            if (command.Parameters.Contains(parameterName))
            {
                // reconfigurar el parámetro existente
                objResult = command.Parameters[name];
            }
            else
            {
                // crear el parámetro para el comando
                objResult = command.CreateParameter();
                objResult.ParameterName = parameterName;
                command.Parameters.Add(objResult);
            }

            // configurar los atributos del parámetro
            objResult.DbType = type;
            objResult.Direction = direction;
            objResult.Value = fixValue(value);
        }

        /// <summary>
        /// Agregar un parámetro a un objeto del tipo <see cref="DbCommand"/>
        /// </summary>
        /// <param name="command">Objeto del tipo <see cref="DbCommand"/></param>
        /// <param name="parameter">Objeto del tipo <see cref="UDbParameter"/> con la información del parámetro</param>
        public static void AddParameter(this DbCommand command, UDbParameter parameter)
        {
            // el argumento [parameter] es requerido.
            if (parameter == null)
                throw new ArgumentNullException("parameter");

            // agregar el parámetro
            command.AddParameter(parameter.Name, parameter.Type,
                parameter.Value, parameter.Direction);
        }

        /// <summary>
        /// Agregar un conjunto de parámetros a un objeto del tipo <see cref="DbCommand"/>
        /// </summary>
        /// <param name="command">Objeto del tipo <see cref="DbCommand"/></param>
        /// <param name="parameters">Lista de objetos del tipo <see cref="UDbParameter"/> con la información de los parámetros</param>
        public static void AddParameter(this DbCommand command, UDbParameter[] parameters)
        {
            if (parameters != null) // agregar los parámetros al comando
                foreach (var parameter in parameters)
                    command.AddParameter(parameter);
        }

        /// <summary>
        /// Asignar el valor a un parámetro dentro de un objeto del tipo <see cref="DbCommand"/>
        /// </summary>
        /// <param name="command">Objeto del tipo <see cref="DbCommand"/></param>
        /// <param name="name">Nombre del parámetro</param>
        /// <param name="value">Valor del parámetro</param>
        public static void SetParameter(this DbCommand command, string name, object value)
        {
            if (command == null) // el objeto es requerido
                throw new ArgumentNullException("command");

            // evaluar el nombre del parámetro
            var parameterName = fixName(name);
            if (name.Length == 0) // el nombre del parámetro es requerido
                throw new ArgumentNullException("name");

            // localizar el parámetro y asignar el valor
            if (command.Parameters.Contains(parameterName))
                command.Parameters[parameterName].Value = fixValue(value);
        }

        /// <summary>
        /// Extraer el valor <see cref="string"/> del objeto
        /// </summary>
        /// <param name="value">Objeto a evaluar</param>
        /// <param name="defaultValue">Valor por defecto en caso de que el objeto sea nulo</param>
        /// <returns></returns>
        public static string GetDbString(this object value, string defaultValue = "")
        {
            return (value == null || value.Equals(DBNull.Value) ?
                defaultValue : value.ToString());
        }

        /// <summary>
        /// Extraer el valor <see cref="bool"/> del objeto
        /// </summary>
        /// <param name="value">Objeto a evaluar</param>
        /// <param name="defaultValue">Valor por defecto en caso de que el objeto sea nulo</param>
        /// <returns></returns>
        public static bool GetDbBoolean(this object value, bool defaultValue = false)
        {
            return (value == null || value.Equals(DBNull.Value) ?
                defaultValue : (bool)value);
        }

        /// <summary>
        /// Extraer el valor <see cref="DateTime"/> del objeto
        /// </summary>
        /// <param name="value">Objeto a evaluar</param>
        /// <returns>Regresa <see cref="DateTime.MinValue"/> cuando el objeto es nulo</returns>
        public static DateTime GetDbDateTime(this object value)
        {
            return (value == null || value.Equals(DBNull.Value) ?
                DateTime.MinValue : (DateTime)value);
        }

        /// <summary>
        /// Extraer el valor <see cref="byte[]"/> del objeto
        /// </summary>
        /// <param name="value">Objeto a evaluar</param>
        /// <returns>Regresa <see cref="byte[]"/> cuando el objeto es nulo</returns>
        public static byte[] GetDbArray(this object value)
        {
            return (value == null || value.Equals(DBNull.Value) ?
                new byte[] { } : (byte[])value);
        }

        /// <summary>
        /// Extraer el valor <see cref="byte"/> del objeto
        /// </summary>
        /// <param name="value">Objeto a evaluar</param>
        /// <param name="defaultValue">Valor por defecto en caso de que el objeto sea nulo</param>
        /// <returns></returns>
        public static byte GetDbByte(this object value, byte defaultValue = 0)
        {
            return (value == null || value.Equals(DBNull.Value) ?
                defaultValue : (byte)value);
        }

        /// <summary>
        /// Extraer el valor <see cref="short"/> del objeto
        /// </summary>
        /// <param name="value">Objeto a evaluar</param>
        /// <param name="defaultValue">Valor por defecto en caso de que el objeto sea nulo</param>
        /// <returns></returns>
        public static short GetDbShort(this object value, short defaultValue = 0)
        {
            return (value == null || value.Equals(DBNull.Value) ?
                defaultValue : (short)value);
        }

        /// <summary>
        /// Extraer el valor <see cref="int"/> del objeto
        /// </summary>
        /// <param name="value">Objeto a evaluar</param>
        /// <param name="defaultValue">Valor por defecto en caso de que el objeto sea nulo</param>
        /// <returns></returns>
        public static int GetDbInteger(this object value, int defaultValue = 0)
        {
            return (value == null || value.Equals(DBNull.Value) ?
                defaultValue : (int)value);
        }

        /// <summary>
        /// Extraer el valor <see cref="long"/> del objeto
        /// </summary>
        /// <param name="value">Objeto a evaluar</param>
        /// <param name="defaultValue">Valor por defecto en caso de que el objeto sea nulo</param>
        /// <returns></returns>
        public static long GetDbLong(this object value, long defaultValue = 0)
        {
            return (value == null || value.Equals(DBNull.Value) ?
                defaultValue : (long)value);
        }

        /// <summary>
        /// Extraer el valor <see cref="decimal"/> del objeto
        /// </summary>
        /// <param name="value">Objeto a evaluar</param>
        /// <param name="defaultValue">Valor por defecto en caso de que el objeto sea nulo</param>
        /// <returns></returns>
        public static decimal GetDbDecimal(this object value, decimal defaultValue = 0)
        {
            return (value == null || value.Equals(DBNull.Value) ?
                defaultValue : (decimal)value);
        }

        /// <summary>
        /// Extraer el valor <see cref="double"/> del objeto
        /// </summary>
        /// <param name="value">Objeto a evaluar</param>
        /// <param name="defaultValue">Valor por defecto en caso de que el objeto sea nulo</param>
        /// <returns></returns>
        public static double GetDbDouble(this object value, double defaultValue = 0)
        {
            return (value == null || value.Equals(DBNull.Value) ?
                defaultValue : (double)value);
        }
    }
}
