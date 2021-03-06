﻿// https://github.com/yunbow/CSharp-WebAPI

using Hondarersoft.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hondarersoft.WebInterface
{
    public class HttpService : WebInterface, IHttpService, IWebInterfaceService
    {
        public event EventHandler<HttpRequestEventArgs> HttpRequest;

        private HttpListener _listener = null;

        public bool AllowCORS { get; set; } = false;

        public HttpService(ILogger<HttpService> logger) : base(logger)
        {
            Hostname = "+";
        }

        public new IHttpService LoadConfiguration(IConfiguration configurationRoot)
        {
            HttpServiceConfigEntry httpServiceConfig = configurationRoot.Get<HttpServiceConfigEntry>();

            if (httpServiceConfig.AllowCORS != null)
            {
                AllowCORS = (bool)httpServiceConfig.AllowCORS;
            }

            return base.LoadConfiguration(httpServiceConfig) as IHttpService;
        }

        /// <summary>
        /// HTTP サービスを起動する
        /// </summary>
        public Task StartAsync()
        {
            if ((string.IsNullOrEmpty(Hostname) == true) ||
                (PortNumber == 0))
            {
                throw new Exception("invalid endpoint parameter");
            }

            // HTTP サーバーを起動する
            _listener = new HttpListener();

            string ssl = string.Empty;
            if (UseSSL == true)
            {
                ssl = "s";
            }

            string tail = string.Empty;
            if (string.IsNullOrEmpty(BasePath) != true)
            {
                tail = "/";
            }

            _listener.Prefixes.Add($"http{ssl}://{Hostname}:{PortNumber}/{BasePath}{tail}");
            _listener.Start();

            ProcessHttpRequest(_listener).NoWaitAndWatchException();

            return Task.CompletedTask;
        }

        protected async Task ProcessHttpRequest(HttpListener httpListener)
        {
            try
            {
                while (httpListener.IsListening == true)
                {
                    // 接続待機
                    HttpListenerContext httpListenerContext = await httpListener.GetContextAsync();

                    if (httpListener.IsListening == false)
                    {
                        break;
                    }

                    try
                    {
                        if (AllowCORS == true)
                        {
                            httpListenerContext.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                        }

                        // プリフライトリクエストには、OK を返す。
                        if (httpListenerContext.Request.HttpMethod == "OPTIONS")
                        {
                            if (AllowCORS == true)
                            {
                                httpListenerContext.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                                httpListenerContext.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                                httpListenerContext.Response.AddHeader("Access-Control-Max-Age", "1728000");
                                httpListenerContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
                            }
                            else
                            {
                                httpListenerContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            }
                            continue;
                        }

                        await Invoke(httpListenerContext);
                    }
                    catch
                    {
                        // レスポンスにある程度値がセットされている場合、このタイミングで 500 にすることができない。
                        // NOP
                    }
                    finally
                    {
                        //_logger.LogInformation("Response: {0} {1} {2}", httpListenerContext.Request.RequestTraceIdentifier.ToString(), httpListenerContext.Response.StatusCode, httpListenerContext.Response.StatusDescription);

                        if (httpListenerContext.Response != null)
                        {
                            httpListenerContext.Response.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception at ProcessHttpRequest method.\r\n{0}", ex.ToString());

                Stop();
            }
        }

        protected virtual async Task Invoke(HttpListenerContext httpListenerContext)
        {
            //_logger.LogInformation("Request: {0} {1} {2}", httpListenerContext.Request.RequestTraceIdentifier.ToString(), httpListenerContext.Request.HttpMethod, httpListenerContext.Request.RawUrl);

            if (HttpRequest != null)
            {
                HttpRequest(this, new HttpRequestEventArgs(httpListenerContext));
            }
        }

        /// <summary>
        /// HTTP サービスを停止する
        /// </summary>
        public void Stop()
        {
            try
            {
                _listener.Stop();
                _listener.Close();

                //log.Info(Resources.StopServer);
                //EventLog.WriteEntry(GetSystemName(), Resources.StopServer, EventLogEntryType.Information, (int)ErrorCode.SUCCESS);
            }
            catch (Exception ex)
            {
                //log.Error(ex.ToString());

                //Assembly clsAsm = Assembly.GetExecutingAssembly();
                //string strSystemName = clsAsm.GetName().Name;

                //EventLog.WriteEntry(GetSystemName(), ex.ToString(), EventLogEntryType.Error, (int)ErrorCode.ERROR_STOP);
            }

            //log.Info("########## HTTP Server [end] ##########");
        }
    }
}
