using System;

namespace YamlHandlers
{
    /// <summary>
    /// Модель для сериализации блока переменных в yaml.
    /// </summary>
    public class YamlVariablesNode
    {
        public string Path { get; set; }
        public string Content { get; set; }
        public string ContentOriginal { get; set; }
        public string FullContent { get; set; }
        public YamlVariables Variables { get; set; }
    }

    public class YamlVariables
    {
        #region instance_name

        public string Instance_name_field { get; } = "instance_name";
        public string Instance_name_old
        {
            get
            {
                return _instance_name_old;
            }
        }
        public string Instance_name
        {
            get
            {
                return _instance_name;
            }
            set
            {
                _instance_name_old = _instance_name;
                _instance_name = value;
            }
        }
        private string _instance_name;
        private string _instance_name_old;

        #endregion

        #region purpose

        public string Purpose_field { get; } = "purpose";
        public string Purpose_old
        {
            get
            {
                return _purpose_old;
            }
        }
        public string Purpose
        {
            get
            {
                return _purpose;
            }
            set
            {
                _purpose_old = _purpose;
                _purpose = value;
            }
        }
        private string _purpose;
        private string _purpose_old;

        #endregion

        #region database

        public string Database_field { get; } = "database";
        public string Database_old
        {
            get
            {
                return _database_old;
            }
        }
        public string Database
        {
            get
            {
                return _database;
            }
            set
            {
                _database_old = _database;
                _database = value;
            }
        }
        private string _database;
        private string _database_old;

        #endregion

        #region home_path

        public string Home_path_field { get; } = "home_path";
        public string Home_path_old
        {
            get
            {
                return _home_path_old;
            }
        }
        public string Home_path
        {
            get
            {
                return _home_path;
            }
            set
            {
                _home_path_old = _home_path;
                _home_path = value;
            }
        }
        private string _home_path;
        private string _home_path_old;

        #endregion

        #region home_path_src

        public string Home_path_src_field { get; } = "home_path_src";
        public string Home_path_src_old
        {
            get
            {
                return _home_path_src_old;
            }
        }
        public string Home_path_src
        {
            get
            {
                return _home_path_src;
            }
            set
            {
                _home_path_src_old = _home_path_src;
                _home_path_src = value;
            }
        }
        private string _home_path_src;
        private string _home_path_src_old;

        #endregion

        #region host_fqdn

        public string Host_fqdn_field { get; } = "host_fqdn";
        public string Host_fqdn_old
        {
            get
            {
                return _host_fqdn_old;
            }
        }
        public string Host_fqdn
        {
            get
            {
                return _host_fqdn;
            }
            set
            {
                _host_fqdn_old = _host_fqdn;
                _host_fqdn = value;
            }
        }
        private string _host_fqdn;
        private string _host_fqdn_old;

        #endregion

        #region http_port

        public string Http_port_field { get; } = "http_port";
        public string Http_port_old
        {
            get
            {
                return _http_port_old;
            }
        }
        public string Http_port
        {
            get
            {
                return _http_port;
            }
            set
            {
                _http_port_old = _http_port;
                _http_port = value;
            }
        }
        private string _http_port;
        private string _http_port_old;

        #endregion

        #region https_port

        public string Https_port_field { get; } = "https_port";
        public string Https_port_old
        {
            get
            {
                return _https_port_old;
            }
        }
        public string Https_port
        {
            get
            {
                return _https_port;
            }
            set
            {
                _https_port_old = _https_port;
                _https_port = value;
            }
        }
        private string _https_port;
        private string _https_port_old;

        #endregion

        #region protocol

        public string Protocol_field { get; } = "protocol";
        public string Protocol_old
        {
            get
            {
                return _protocol_old;
            }
        }
        public string Protocol
        {
            get
            {
                return _protocol;
            }
            set
            {
                _protocol_old = _protocol;
                _protocol = value;
            }
        }
        private string _protocol;
        private string _protocol_old;

        #endregion
    }
}
