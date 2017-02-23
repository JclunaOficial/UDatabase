using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace JclunaOficial
{
    /// <summary>
    /// Contexto de acceso a los datos
    /// </summary>
    public class UDbContext : IDisposable
    {
        private DbProviderFactory factory = null;
        private DbConnection connection = null;
        private DbTransaction transaction = null;

        private string providerName = string.Empty;
        private string connectionString = string.Empty;
        private bool disposed = false;

        // liberar el objeto de conexión
        private void ReleaseConnection()
        {
            if (connection != null)
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
                connection.Dispose();
                connection = null;
            }
        }

        // liberar el objeto de transacción
        private void ReleaseTransaction()
        {
            if (transaction != null)
            {
                transaction.Dispose();
                transaction = null;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposed == false)
            {
                disposed = true;
                if (disposing == true)
                {
                    // liberar los objetos
                    ReleaseTransaction();
                    ReleaseConnection();

                    // destruir variables
                    factory = null;
                    providerName = null;
                    connectionString = null;
                }
            }
        }

        // preparar contexto usando el proveedor y cadena de conexión recibidos
        private void SetupWithProvider(string providerName, string connectionString, bool open, bool begin, IsolationLevel level)
        {
            // el proveedor de datos es requerido
            this.providerName = (providerName == null ? "" :
                providerName.Trim());
            if (this.providerName.Length == 0)
                throw new ArgumentNullException("providerName");

            // la cadena de conexión es requerida
            this.connectionString = (connectionString == null ? "" :
                connectionString.Trim());
            if (this.connectionString.Length == 0)
                throw new ArgumentNullException("connectionString");

            // recuperar el objeto para fabricar objetos de datos
            factory = DbProviderFactories.GetFactory(this.providerName);
            if (factory == null) throw new ArgumentException("Database provider name [" + this.providerName + "] is not valid.", "providerName");

            // fabricar un constructor para probar el proveedor de datos
            var dummy = factory.CreateConnectionStringBuilder();
            if (dummy == null) throw new ArgumentException("Database provider name [" + this.providerName + "] is not supported.", "providerName");

            if (open == true)
            {
                if (begin == true)
                    Begin(level); // conectar a DB con transacción iniciada
                else
                    Open(); // conectar a DB sin transacción.
            }
        }

        // preparar contexto usando la sección connectionString con el nombre especificado
        private void SetupWithSettings(string name, bool open, bool begin, IsolationLevel level)
        {
            // evaluar el nombre de la conexión a usar
            var settingName = (name == null ? "" : name.Trim());
            ConnectionStringSettings settings = null;

            // obtener las configuraciones de conexión
            if (settingName.Length > 0)
            {
                // recuperar la configuración con el nombre especificado
                settings = ConfigurationManager.ConnectionStrings[settingName];
                if (settings == null) throw new ArgumentException("Connection String [" + settingName + "] does not exists in configuration file section [ConnectionStrings]", "name");
            }
            else
            {
                // recuperar la primera configuración registrada
                if (ConfigurationManager.ConnectionStrings == null ||
                    ConfigurationManager.ConnectionStrings.Count == 0)
                    throw new ConfigurationErrorsException("No [ConnectionStrings] section in configuration file.");
                settings = ConfigurationManager.ConnectionStrings[0];
            }

            // aplicar la configuración usando el proveedor de la sección
            SetupWithProvider(settings.ProviderName, settings.ConnectionString, open, begin, level);
        }

        // asociar el contexto de datos al objeto de comando, incluyendo la transacción actual
        private void LinkDbContext(DbCommand command, UDbParameter[] parameters)
        {
            // agregar los parametros al comando
            command.AddParameter(parameters);

            // asociar la conexión
            command.Connection = connection;
            if (transaction != null) // asociar la transacción
                command.Transaction = transaction;
        }

        /// <summary>
        /// Crear una instancia del tipo <see cref="UDbContext"/>
        /// </summary>
        public UDbContext() : this(false)
        {
        }

        /// <summary>
        /// Crear una instancia del tipo <see cref="UDbContext"/>
        /// </summary>
        /// <param name="open">Abrir la conexión con la base de datos</param>
        /// <param name="begin">Iniciar el contexto de transacción</param>
        /// <param name="level">Nivel de aislamiento de los datos en transacción (valor por defecto: ReadCommited)</param>
        public UDbContext(bool open, bool begin = false, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            // aplicar la primer conexión registrada, sin conectar
            SetupWithSettings("", false, false, level);
        }

        /// <summary>
        /// Crear una instancia del tipo <see cref="UDbContext"/>
        /// </summary>
        /// <param name="settingName">Nombre de la configuración dentro de la sección [ConnectionStrings] del archivo de configuraciones</param>
        /// <param name="open">Abrir la conexión con la base de datos</param>
        /// <param name="begin">Iniciar el contexto de transacción</param>
        /// <param name="level">Nivel de aislamiento de los datos en transacción (valor por defecto: ReadCommited)</param>
        public UDbContext(string settingName, bool open = false, bool begin = false, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            // aplicar la conexión especificada
            SetupWithSettings("", false, false, level);
        }

        /// <summary>
        /// Crear una instancia del tipo <see cref="UDbContext"/>
        /// </summary>
        /// <param name="providerName">Proveedor de datos para fabricar los objetos (ejemplo: System.Data.SqlClient o System.Data.MySqlClient)</param>
        /// <param name="connectionString">Cadena de conexión para la base de datos</param>
        /// <param name="open">Abrir la conexión con la base de datos</param>
        /// <param name="begin">Iniciar el contexto de transacción</param>
        /// <param name="level">Nivel de aislamiento de los datos en transacción (valor por defecto: ReadCommited)</param>
        public UDbContext(string providerName, string connectionString, bool open = false, bool begin = false, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            // aplicar la configuración especificada por el proveedor de datos
            SetupWithProvider(providerName, connectionString, open, begin, level);
        }

        /// <summary>
        /// Establecer la conexión con la base de datos
        /// </summary>
        public void Open()
        {
            if (connection == null || connection.State != ConnectionState.Closed)
            {
                // liberar la conexión previa
                ReleaseConnection();

                // crear el objeto de conexión segun el proveedor de datos
                connection = factory.CreateConnection();
                if (connection != null)
                    connection.ConnectionString = connectionString;

                // abrir la conexión
                connection.Open();
            }
        }

        /// <summary>
        /// Iniciar transacción en el contexto de datos actual
        /// </summary>
        /// <param name="level">Especificar el nivel de aislamiento en la transacción</param>
        public void Begin(IsolationLevel level)
        {
            Open(); // abrir la conexión si aun no existe
            Rollback(); // desechar transacción previa

            // iniciar el contexto de transacción
            transaction = connection.BeginTransaction(level);
        }

        /// <summary>
        /// Completar transacción en el contexto de datos actual
        /// </summary>
        public void Complete()
        {
            if (transaction != null)
            {
                if (transaction.Connection != null &&
                    transaction.Connection.State != ConnectionState.Closed)
                    transaction.Commit(); // <- aplicar las operaciones de la transacción

                // liberar los recursos de la transacción
                ReleaseTransaction();
            }
        }

        /// <summary>
        /// Desechar transacción en el contexto de datos actual
        /// </summary>
        public void Rollback()
        {
            ReleaseTransaction();
        }

        /// <summary>
        /// Cerrar la conexión con la base de datos
        /// </summary>
        public void Close()
        {
            // liberar transacción y conexión
            ReleaseTransaction();
            ReleaseConnection();
        }

        /// <summary>
        /// Liberar los recursos utilizados por el objeto
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Crear un objeto del tipo <see cref="DbCommand"/> con la instrucción y los parámetros requeridos
        /// </summary>
        /// <param name="isStoredProcedure">Determina si la instrucción es un procedimiento almacenado (StoredProcedure)</param>
        /// <param name="commandText">Instrucción o procedimiento almacenado que será ejecutado</param>
        /// <param name="parameters">Lista de <see cref="UDbParameter"/> para los parámetros requeridos</param>
        /// <returns></returns>
        public DbCommand CreateCommand(bool isStoredProcedure, string commandText, params UDbParameter[] parameters)
        {
            // fabricar el objeto usando el proveedor de datos
            var objResult = factory.CreateCommand();
            if (objResult == null) return null;

            // configurar los atributos
            objResult.CommandType = (isStoredProcedure ?
                CommandType.StoredProcedure : CommandType.Text);
            objResult.CommandText = (commandText == null ? "" : commandText.Trim());

            // agregar los parámetros
            objResult.AddParameter(parameters);
            return objResult;
        }

        /// <summary>
        /// Ejecutar instrucción que no regresa datos
        /// </summary>
        /// <param name="command">Objeto <see cref="DbCommand"/> con la intrucción a ejecutar</param>
        /// <param name="parameters">Lista de <see cref="UDbParameter"/> para los parámetros requeridos.</param>
        /// <returns><see cref="int"/> con el número de registros afectados</returns>
        public int ExecuteNonQuery(DbCommand command, params UDbParameter[] parameters)
        {
            // asociar el contexto de datos
            LinkDbContext(command, parameters);

            // ejecutar la instrucción
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Ejecutar instrucción que no regresa datos
        /// </summary>
        /// <param name="isStoredProcedure">Determina si la instrucción es un procedimiento almacenado (StoredProcedure)</param>
        /// <param name="commandText">Instrucción o procedimiento almacenado que será ejecutado</param>
        /// <param name="parameters">Lista de <see cref="UDbParameter"/> para los parámetros requeridos</param>
        /// <returns><see cref="int"/> con el número de registros afectados</returns>
        public int ExecuteNonQuery(bool isStoredProcedure, string commandText, params UDbParameter[] parameters)
        {
            using (var command = CreateCommand(isStoredProcedure, commandText, parameters))
                return ExecuteNonQuery(command, null);
        }

        /// <summary>
        /// Ejecutar instrucción que regresa un valor
        /// </summary>
        /// <param name="command">Objeto <see cref="DbCommand"/> con la intrucción a ejecutar</param>
        /// <param name="parameters">Lista de <see cref="UDbParameter"/> para los parámetros requeridos.</param>
        /// <returns>Regresar un <see cref="object"/> con el valor que regresa la instrucción</returns>
        public object ExecuteScalar(DbCommand command, params UDbParameter[] parameters)
        {
            // asociar el contexto de datos
            LinkDbContext(command, parameters);

            // ejecutar la instrucción
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Ejecutar instrucción que regresa un valor
        /// </summary>
        /// <param name="isStoredProcedure">Determina si la instrucción es un procedimiento almacenado (StoredProcedure)</param>
        /// <param name="commandText">Instrucción o procedimiento almacenado que será ejecutado</param>
        /// <param name="parameters">Lista de <see cref="UDbParameter"/> para los parámetros requeridos</param>
        /// <returns>Regresar un <see cref="object"/> con el valor que regresa la instrucción</returns>
        public object ExecuteScalar(bool isStoredProcedure, string commandText, params UDbParameter[] parameters)
        {
            using (var command = CreateCommand(isStoredProcedure, commandText, parameters))
                return ExecuteScalar(command, null);
        }
    }
}
