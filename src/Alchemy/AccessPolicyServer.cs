﻿using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Alchemy
{
    /// <summary>
    /// This is the Flash Access Policy Server
    /// It manages sending the XML cross domain policy to flash socket clients over port 843.
    /// See http://www.adobe.com/devnet/articles/crossdomain_policy_file_spec.html for details.
    /// </summary>
    internal class AccessPolicyServer : TcpServer, IDisposable
    {
        /// <summary>
        /// The pre-formatted XML response.
        /// </summary>
        private const string Response =
            "<cross-domain-policy>\r\n" +
            "\t<allow-access-from domain=\"{0}\" to-ports=\"{1}\" />\r\n" +
            "</cross-domain-policy>\r\n\0";

        private readonly string _allowedHost = "localhost";
        private readonly int _allowedPort = 80;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessPolicyServer"/> class.
        /// </summary>
        /// <param name="listenAddress">The listen address.</param>
        /// <param name="originDomain">The origin domain.</param>
        /// <param name="allowedPort">The allowed port.</param>
        public AccessPolicyServer(IPAddress listenAddress, string originDomain, int allowedPort)
            : base(843, listenAddress)
        {
            _allowedHost = "*";
            if (originDomain != String.Empty)
            {
                _allowedHost = originDomain;
            }

            _allowedPort = allowedPort;
        }

        /// <summary>
        /// Fires when a client connects.
        /// </summary>
        /// <param name="data">The TCP Connection.</param>
        protected override void OnRunClient(object data)
        {
            var connection = (TcpClient) data;
            try
            {
                // mjb
                //connection.Client.Receive(new byte[32]);

                NetworkStream ns = connection.GetStream();
                byte[] buffer = new byte[32];
                ns.Read(buffer, 0, 32);

                SendResponse(connection);
                connection.Client.Close();
            }
            catch (SocketException)
            {
                /* Ignore */
            }
        }

        /// <summary>
        /// Sends the response.
        /// </summary>
        /// <param name="connection">The TCP Connection.</param>
        public void SendResponse(TcpClient connection)
        {
            try
            {
                // mjb
                /* connection.Client.Send(
                    Encoding.UTF8.GetBytes(String.Format(Response, _allowedHost, _allowedPort.ToString(CultureInfo.InvariantCulture))));
                 */

                NetworkStream ns = connection.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes(String.Format(Response, _allowedHost, _allowedPort.ToString(CultureInfo.InvariantCulture)));
                ns.Write(buffer, 0, buffer.Length);
            }
            catch (SocketException)
            {
                //Ignore
            }
        }
    }
}