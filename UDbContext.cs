﻿using System;
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
    }
}
