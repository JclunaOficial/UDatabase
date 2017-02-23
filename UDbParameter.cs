using System;
using System.Data;

namespace JclunaOficial
{
    /// <summary>
    /// Parámetro de datos
    /// </summary>
    public class UDbParameter
    {
        /// <summary>
        /// Nombre del parámetro
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Tipo de dato del parámetro
        /// </summary>
        public DbType Type { get; set; }

        /// <summary>
        /// Valor del parámetro
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Dirección del parámetro (input | output)
        /// </summary>
        public ParameterDirection Direction { get; set; }

        /// <summary>
        /// Crear una instancia del tipo <see cref="UDbParameter"/>
        /// </summary>
        public UDbParameter()
        {
            // inicializar atributos
            Name = string.Empty;
            Type = DbType.String;
            Value = DBNull.Value;
            Direction = ParameterDirection.Input;
        }

        /// <summary>
        /// Crear una instancia del tipo <see cref="UDbParameter"/>
        /// </summary>
        /// <param name="name">Nombre del parámetro</param>
        /// <param name="type">Tipo de datos del parámetro</param>
        /// <param name="value">Valor del parámetro</param>
        /// <param name="direction">Dirección del parámetro</param>
        public UDbParameter(string name, DbType type, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            Name = name;
            Type = type;
            Value = (value != null ? value : DBNull.Value);
            Direction = direction;
        }
    }
}
